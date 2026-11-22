---
title: "Deployment и ReplicaSet: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 05-deployments
part: 1
tags: [deployment, rolling-update, maxsurge, maxunavailable, recreate, rollback, revision, progress-deadline, pause, rollout-restart]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment и ReplicaSet: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): манифест, масштабирование и простое обновление не повторяются.

## Rolling update по шагам

Что именно происходит при смене образа у Deployment на 3 реплики (значения по умолчанию: `maxSurge: 25%`, `maxUnavailable: 25%`)? Наблюдение за ReplicaSet-ами во время обновления (`kubectl get rs -w`); `web-6d8567df8` — старый (nginx:1.28), `web-c5748c689` — новый (nginx:1.29). Поля: желаемое число, текущее, готовое.

```text
web-c5748c689 desired=1 current=0 ready=0     # новый RS: разрешено 1 лишний под (surge)
web-c5748c689 desired=1 current=1 ready=0
web-c5748c689 desired=1 current=1 ready=1     # новый под готов
web-6d8567df8 desired=2 current=3 ready=3     # теперь можно убрать один старый
web-c5748c689 desired=2 current=1 ready=1
web-6d8567df8 desired=2 current=2 ready=2
web-c5748c689 desired=2 current=2 ready=1
web-c5748c689 desired=2 current=2 ready=2
web-6d8567df8 desired=1 current=2 ready=2
web-c5748c689 desired=3 current=2 ready=2
web-6d8567df8 desired=1 current=1 ready=1
web-c5748c689 desired=3 current=3 ready=3
web-6d8567df8 desired=0 current=1 ready=1
web-6d8567df8 desired=0 current=0 ready=0
```

(выбраны строки с изменениями, повторы убраны.) Шаги:

1. Новый ReplicaSet получает 1 реплику: `maxSurge` позволяет временно иметь на 1 под больше желаемого.
2. Пока новый под не стал `Ready`, старые не трогают, потому что `maxUnavailable` округлён вниз до 0.
3. После готовности нового пода старый ReplicaSet уменьшается на 1, а новый увеличивается на 1.
4. Шаги повторяются, пока новый ReplicaSet не достигнет 3, а старый — 0.

**Округление:** 25% от 3 — это 0.75. `maxSurge` округляется вверх (до 1), `maxUnavailable` — вниз (до 0). Поэтому на 3 репликах обновление идёт строго по одному поду, а число доступных подов не падает ниже 3.

Значения можно задать числом или процентом:

```yaml
spec:
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 0
      maxUnavailable: 1
```

| Настройка | Эффект |
|---|---|
| `maxSurge: 1`, `maxUnavailable: 0` | нет потери мощности, нужна запасная нода под лишний под |
| `maxSurge: 0`, `maxUnavailable: 1` | не нужно лишних ресурсов, но на время обновления на один под меньше |
| `maxSurge: 100%` | сразу поднять весь новый набор (blue-green по скорости) |

Оба параметра не могут быть нулевыми одновременно: обновление не сможет начаться.

> ⚠️ Проверить: запрет одновременных нулей у `maxSurge` и `maxUnavailable` дан по документации и не воспроизводился.

## Стратегия Recreate

`Recreate` останавливает все старые поды, а потом создаёт новые:

```text
$ kubectl -n learn-05 patch deploy web -p '{"spec":{"strategy":{"type":"Recreate","rollingUpdate":null}}}'
$ kubectl -n learn-05 get pods -w   # во время set image (сводка наблюдения, не дословный вывод)
Running (3 старых) → Terminating (3) → Pending → ContainerCreating → Running (3 новых)
```

Старые и новые поды не работают одновременно: между ними есть простой. `rollingUpdate: null` нужно передать, иначе API отклонит конфликтующие настройки. Применяйте `Recreate`, когда две версии не могут работать вместе (например, несовместимая схема БД) и простой допустим.

## История ревизий и откат

Каждый ReplicaSet соответствует ревизии. История:

```text
$ kubectl -n learn-05 rollout history deploy/web
deployment.apps/web
REVISION  CHANGE-CAUSE
2         to 1.29 (recreate)
3         <none>
```

Подробности ревизии:

```text
$ kubectl -n learn-05 rollout history deploy/web --revision=2
deployment.apps/web with revision #2
Pod Template:
  Labels:	app=web
	pod-template-hash=c5748c689
  Annotations:	kubernetes.io/change-cause: to 1.29 (recreate)
  Containers:
   nginx:
    Image:	nginx:1.29
```

Откат:

```bash
kubectl -n learn-05 rollout undo deploy/web                    # на предыдущую ревизию
kubectl -n learn-05 rollout undo deploy/web --to-revision=1    # на конкретную
```

После отката номера меняются: откат к старому ReplicaSet превращает его в самую новую ревизию. В опыте ревизия 1 (nginx:1.28) после отката получила номер 3, а ревизия 1 из списка исчезла. Число хранимых ревизий ограничивает `revisionHistoryLimit` (по умолчанию 10); при значении 0 откат невозможен.

### Причина изменения (change-cause)

Столбец `CHANGE-CAUSE` берётся из аннотации `kubernetes.io/change-cause`. Задавайте её **после** смены шаблона: аннотация, поставленная на Deployment, копируется на текущий ReplicaSet. В опыте аннотацию `upgrade nginx to 1.29` поставили до `set image`, и обе ревизии получили этот текст:

```text
REVISION  CHANGE-CAUSE
1         upgrade nginx to 1.29
2         upgrade nginx to 1.29
```

Старая ревизия оказалась подписана неверно. Правильный порядок: сначала изменить шаблон, затем `kubectl annotate deploy web kubernetes.io/change-cause="…" --overwrite`. Флаг `--record` устарел.

> ⚠️ Проверить: статус флага `--record` (устарел или удалён) в kubectl 1.36 не проверялся запуском.

## Застрявший деплой

Что будет, если указать несуществующий образ?

```text
$ kubectl -n learn-05 set image deploy/web nginx=nginx:does-not-exist
$ kubectl -n learn-05 rollout status deploy/web --timeout=10s
Waiting for deployment "web" rollout to finish: 1 out of 3 new replicas have been updated...
error: timed out waiting for the condition
$ kubectl -n learn-05 get pods
web-7b466b6cf6-n9xnt   0/1   ErrImagePull
web-c5748c689-4r6cf    1/1   Running
web-c5748c689-6t6k2    1/1   Running
web-c5748c689-h4lz6    1/1   Running
$ kubectl -n learn-05 get deploy web
NAME   READY   UP-TO-DATE   AVAILABLE
web    3/3     1            3
```

Rolling update защитил приложение: новый под не стал готовым, поэтому ни один старый под не удалён (`READY 3/3`, `AVAILABLE 3`). Состояние Deployment:

```text
Available=True (MinimumReplicasAvailable)
Progressing=True (ReplicaSetUpdated)
```

С `progressDeadlineSeconds: 600` (по умолчанию) Deployment 10 минут считается «в процессе». Чтобы увидеть сбой быстро, срок можно сократить:

```text
$ kubectl -n learn-05 patch deploy web -p '{"spec":{"progressDeadlineSeconds":20}}'
$ kubectl -n learn-05 rollout status deploy/web --timeout=5s
error: deployment "web" exceeded its progress deadline
Available=True (MinimumReplicasAvailable)
Progressing=False (ProgressDeadlineExceeded)
```

Состояние `ProgressDeadlineExceeded` — только сигнал: Kubernetes ничего не откатывает сам. Откат выполняется командой `kubectl rollout undo deploy/web`; в опыте он вернул `nginx:1.29`.

## Pause и resume

Если нужно внести несколько изменений одним обновлением, деплой можно приостановить:

```text
$ kubectl -n learn-05 rollout pause deploy/web
$ kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
$ kubectl -n learn-05 set resources deploy/web --requests=cpu=10m
$ kubectl -n learn-05 get rs --no-headers | awk '{print $1,$2,$3,$4}'
web-6d8567df8 0 0 0
web-79f5c5889 3 3 3          # новых ReplicaSet нет, работает прежний
web-7b466b6cf6 0 0 0
web-c5748c689 0 0 0
$ kubectl -n learn-05 rollout resume deploy/web
$ kubectl -n learn-05 get rs --no-headers | awk '{print $1,$2,$3,$4}'
web-754dbc8465 3 3 3         # после resume один новый ReplicaSet
web-79f5c5889 0 0 0
…
```

Оба изменения (образ и ресурсы) вошли в один новый ReplicaSet `web-754dbc8465`, вместо двух последовательных обновлений.

## Перезапуск без изменения образа

```text
$ kubectl -n learn-05 rollout restart deploy/web
deployment.apps/web restarted
$ kubectl -n learn-05 get deploy web -o jsonpath='{.spec.template.metadata.annotations}'
{"kubectl.kubernetes.io/restartedAt":"2026-11-22T20:32:57+04:00"}
```

Команда меняет шаблон: добавляет аннотацию `restartedAt`. Это новый хэш шаблона, новый ReplicaSet и плавный rolling update, а не одномоментный перезапуск. Время записывается в часовом поясе клиента.

## Неизменяемый selector

Менять `spec.selector` у существующего Deployment нельзя; попытка получает ошибку валидации:

```text
$ kubectl -n learn-05 patch deploy web --type=merge -p '{"spec":{"selector":{"matchLabels":{"app":"other"}}}}'
The Deployment "web" is invalid:
* spec.template.metadata.labels: Invalid value: {"app":"web"}: `selector` does not match template `labels`
```

Ошибка показывает правило: метки шаблона обязаны совпадать с селектором. Чтобы поменять селектор, создают новый Deployment.

## Лучшие практики

- Теги образов — конкретные версии (`nginx:1.29`), а не `latest`.
- Задавайте `readinessProbe` (модуль 11): без неё rolling update считает под готовым, как только он запущен.
- Подберите `maxSurge` и `maxUnavailable` под доступные ресурсы и допустимый простой.
- Храните манифесты в Git и применяйте `apply`; после `rollout undo` приведите файл в соответствие.
- Ставьте `kubernetes.io/change-cause` после смены шаблона.
- Не оставляйте `progressDeadlineSeconds` по умолчанию в CI: сократите его, чтобы сбой выкатки обнаруживался быстро.

## Частые ошибки

- ❌ Считать, что `ProgressDeadlineExceeded` откатит релиз → ✅ он только помечает сбой; откат нужно выполнить самому (или инструментом выкатки).
- ❌ Ждать «мгновенного» обновления → ✅ оно идёт шагами; смотрите `rollout status`.
- ❌ Подписывать ревизию `change-cause` до смены шаблона → ✅ подпись попадёт на предыдущую ревизию.
- ❌ Ставить `maxSurge: 0` и `maxUnavailable: 0` → ✅ обновление не сможет сделать шаг.
- ❌ Использовать `Recreate` для сервиса, которому нужна непрерывность → ✅ `RollingUpdate` с readiness probe.

## Сравнение с другими треками

- [C#](../../../csharp/recipes/background-jobs/overview.ru.md): rolling update посылает `SIGTERM` старым подам; воркер должен завершить работу корректно.
- [Linux](../../../linux/overview.ru.md): `rollout restart` не посылает сигнал процессам напрямую, а заменяет поды по очереди.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Следующий модуль: [06 · Service и сеть](../06-services/overview.ru.md)
- Остановка подов при выкатке: [04 · Pod](../04-pods/advanced.ru.md#остановка-пода)

## Источники

- [Deployments: strategy](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#strategy)
- [Deployments: updating](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#updating-a-deployment)
- [Deployments: rolling back](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#rolling-back-a-deployment)
- [Deployments: progress deadline](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#progress-deadline-seconds)
- [Deployments: pausing and resuming](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#pausing-and-resuming-a-deployment)
- [kubectl rollout restart](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_rollout/kubectl_rollout_restart/)
