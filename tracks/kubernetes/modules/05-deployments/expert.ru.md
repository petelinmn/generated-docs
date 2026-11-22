---
title: "Deployment и ReplicaSet: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 05-deployments
part: 1
tags: [deployment, replicaset, pod-template-hash, revision, conditions, rollout, collisioncount, observedgeneration]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment и ReplicaSet: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Как контроллер ведёт ревизии

Deployment не создаёт поды сам: он управляет ReplicaSet-ами. На каждое изменение шаблона пода контроллер вычисляет хэш шаблона и ищет ReplicaSet с таким хэшем. Если находит, увеличивает его (так работает откат), если нет — создаёт новый. Хэш попадает в имя ReplicaSet и в метку `pod-template-hash`, которую получают и ReplicaSet, и его поды: так два набора подов одного Deployment не пересекаются по селектору.

Связи между объектами записаны в метаданных. ReplicaSet активной ревизии (`web-6d8567df8`):

```text
deployment.kubernetes.io/desired-replicas: "3"
deployment.kubernetes.io/max-replicas: "3"
deployment.kubernetes.io/revision: "3"
deployment.kubernetes.io/revision-history: "1"
ownerReferences: Deployment (controller: true)
```

- `revision` — номер ревизии; у Deployment такая же аннотация с номером текущей;
- `revision-history` — список прежних номеров, которые этот ReplicaSet уже носил: в опыте ReplicaSet из ревизии 1 после отката получил номер 3 и запомнил `1`;
- `desired-replicas` и `max-replicas` — служебные значения контроллера, по которым он рассчитывает шаги обновления (`max-replicas` = желаемое + `maxSurge`);
- `ownerReferences` — Deployment как владелец с `controller: true`.

### Шаги обновления

На каждом шаге контроллер вычисляет, сколько подов можно добавить в новом ReplicaSet (в пределах `replicas + maxSurge`) и сколько удалить из старых (чтобы доступных осталось не меньше `replicas − maxUnavailable`). Алгоритм работает по уровню, а не по событиям, поэтому прерванное обновление продолжится после перезапуска controller-manager ([цикл согласования](../02-architecture/advanced.ru.md#цикл-согласования)).

### Статус Deployment

Поля `status`, полученные из `kubectl explain deployment.status`:

| Поле | Смысл |
|---|---|
| `replicas` | сколько подов создано (всех ревизий) |
| `updatedReplicas` | подов с актуальным шаблоном (`UP-TO-DATE`) |
| `readyReplicas` | готовых |
| `availableReplicas` | доступных (готовых хотя бы `minReadySeconds`) |
| `unavailableReplicas` | недоступных |
| `terminatingReplicas` | завершающихся |
| `observedGeneration` | последнее `generation`, которое обработал контроллер |
| `collisionCount` | сколько раз пришлось менять хэш из-за коллизии |
| `conditions` | условия (см. ниже) |

Условия у здорового Deployment после успешного обновления:

```text
Available=True    (MinimumReplicasAvailable)  Deployment has minimum availability.
Progressing=True  (NewReplicaSetAvailable)    ReplicaSet "web-6d8567df8" has successfully progressed.
```

| Условие и причина | Когда |
|---|---|
| `Progressing=True`, `ReplicaSetUpdated` | идёт обновление |
| `Progressing=True`, `NewReplicaSetAvailable` | обновление завершено |
| `Progressing=False`, `ProgressDeadlineExceeded` | превышен `progressDeadlineSeconds` |
| `Available=True`, `MinimumReplicasAvailable` | доступно не меньше `replicas − maxUnavailable` подов |
| `ReplicaFailure` | ReplicaSet не может создать поды (например, превышена квота) |

> ⚠️ Проверить: условие `ReplicaFailure` и его причины не воспроизводились в этом материале; значения `ReplicaSetUpdated`, `NewReplicaSetAvailable`, `MinimumReplicasAvailable`, `ProgressDeadlineExceeded` получены опытом.

`kubectl rollout status` завершается кодом 0 при успехе и ненулевым при ошибке или тайм-ауте (`error: deployment "web" exceeded its progress deadline`), что удобно в CI.

## Справочник

### Поля `spec` Deployment

Список из `kubectl explain deployment.spec`:

| Поле | Смысл | Значение по умолчанию |
|---|---|---|
| `replicas` | число подов | 1 |
| `selector` | селектор подов, неизменяемый | обязателен |
| `template` | шаблон пода | обязателен |
| `strategy` | `RollingUpdate` (`maxSurge`, `maxUnavailable`) или `Recreate` | `RollingUpdate`, 25% / 25% (проверено) |
| `minReadySeconds` | сколько под должен быть готов, чтобы считаться доступным | не задано (0) |
| `revisionHistoryLimit` | сколько старых ReplicaSet хранить | 10 (проверено) |
| `progressDeadlineSeconds` | время ожидания прогресса | 600 (проверено) |
| `paused` | приостановить обновления | `false` |

Значение `replicas` по умолчанию — 1 по документации, а не по опыту.

Поля `spec` у ReplicaSet — `replicas`, `selector`, `template`, `minReadySeconds`.

### Команды `kubectl rollout`

| Команда | Что делает |
|---|---|
| `rollout status deploy/X [--timeout=…]` | ждёт завершения, код возврата отражает результат |
| `rollout history deploy/X [--revision=N]` | ревизии и шаблон ревизии |
| `rollout undo deploy/X [--to-revision=N]` | откат |
| `rollout pause` / `resume` | приостановить / продолжить |
| `rollout restart` | перезапуск через аннотацию `restartedAt` |

Команды `scale`, `set image`, `set resources`, `set env` меняют Deployment императивно; `autoscale` создаёт HPA (модуль 15).

## Подвохи

### Подвох: change-cause подписывает не ту ревизию

Аннотация, поставленная до смены шаблона, копируется на текущий ReplicaSet, и история показывает один текст у двух ревизий (опыт в [продвинутой теории](advanced.ru.md#причина-изменения-change-cause)). Подписывайте после изменения.

### Подвох: номера ревизий меняются после отката

```text
REVISION  CHANGE-CAUSE
2         to 1.29 (recreate)
3         <none>
```

Ревизия 1 (nginx:1.28) исчезла: откат переиспользовал её ReplicaSet и присвоил ему номер 3 (`revision-history: "1"`). Номер ревизии нельзя считать постоянным идентификатором релиза; для истории используйте метки и Git.

### Подвох: застрявшее обновление выглядит «здоровым»

```text
NAME   READY   UP-TO-DATE   AVAILABLE
web    3/3     1            3
Available=True (MinimumReplicasAvailable)
Progressing=True (ReplicaSetUpdated)
```

При неверном образе `READY 3/3` и `Available=True`: работают старые поды. Тревожит только `UP-TO-DATE 1` и `ErrImagePull` у нового пода; через 600 секунд появится `ProgressDeadlineExceeded`. Мониторьте `rollout status` и `UP-TO-DATE`, а не только `READY`.

### Подвох: ProgressDeadlineExceeded не откатывает

Условие `Progressing=False` — лишь пометка; Deployment продолжает пытаться, и старые поды остаются. Откат делает человек или система выкатки.

### Подвох: rollout restart — это не перезапуск подов

Команда добавляет аннотацию `restartedAt` в шаблон: хэш меняется, создаётся новый ReplicaSet, и подмена идёт по стратегии обновления (плавно, при `RollingUpdate`). Время записывается в часовом поясе клиента (`+04:00` в опыте).

### Подвох: при pause изменения не применяются

После `rollout pause` команды `set image` и `set resources` изменили шаблон, но активный ReplicaSet остался прежним, пока не выполнили `resume`; после него появился один новый ReplicaSet с обоими изменениями.

### Подвох: selector и метки шаблона

Попытка поменять `selector` отклонена: ``selector` does not match template `labels` ``. Селектор неизменяем после создания.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `RollingUpdate` | без простоя, откат по ревизиям | две версии работают одновременно |
| `Recreate` | две версии не пересекаются | неизбежный простой |
| Blue-green | мгновенное переключение, лёгкий откат | вдвое больше ресурсов, модель вне Deployment (рецепт canary и blue-green) |
| Canary | проверка на малой доле трафика | нужна поддержка маршрутизации (Ingress, Service mesh) |
| Argo Rollouts, Flagger | продвинутые стратегии, метрики | отдельный компонент (модуль 20) |
| StatefulSet | стабильные имена и диски | другое поведение обновления (модуль 12) |

## Вопросы с собеседований

**1. Как связаны Deployment, ReplicaSet и Pod?**

<details><summary>Ответ</summary>

Deployment управляет ReplicaSet-ами (по одному на ревизию шаблона), ReplicaSet управляет подами. Связь задаёт `ownerReferences`, а подбор подов — селектор и метка `pod-template-hash`.

</details>

**2. Что запускает rolling update?**

<details><summary>Ответ</summary>

Только изменение шаблона пода (`spec.template`): образ, переменные, метки шаблона, аннотации шаблона. Изменение `replicas` ничего не пересоздаёт.

</details>

**3. Как работают `maxSurge` и `maxUnavailable` и как они округляются?**

<details><summary>Ответ</summary>

`maxSurge` — на сколько подов сверх `replicas` можно временно превысить; `maxUnavailable` — сколько подов может быть недоступно. Проценты считаются от `replicas`: `maxSurge` округляется вверх, `maxUnavailable` вниз. При 3 репликах и 25% это 1 и 0, поэтому обновление идёт по одному поду.

</details>

**4. (С подвохом) Образ в Deployment неверный. Упадёт ли приложение?**

<details><summary>Ответ</summary>

Нет, пока работают старые поды: rolling update не удаляет их, пока новые не станут готовыми. Деплой застрянет (`ErrImagePull`, `UP-TO-DATE 1`), через `progressDeadlineSeconds` появится `ProgressDeadlineExceeded`. Откат не выполняется автоматически.

</details>

**5. Как откатить релиз и что происходит с номерами ревизий?**

<details><summary>Ответ</summary>

`kubectl rollout undo deploy/X [--to-revision=N]`. Контроллер переиспользует старый ReplicaSet и присваивает ему новый, самый большой номер ревизии; прежний номер пропадает из списка (в опыте ревизия 1 стала 3).

</details>

**6. (С подвохом) Что делает `kubectl rollout restart`?**

<details><summary>Ответ</summary>

Добавляет в шаблон аннотацию `kubectl.kubernetes.io/restartedAt`. Это меняет хэш, создаётся новый ReplicaSet и поды заменяются по стратегии обновления, а не все сразу.

</details>

**7. Чем `Recreate` отличается от `RollingUpdate`?**

<details><summary>Ответ</summary>

`Recreate` останавливает все старые поды, потом запускает новые: есть простой, но две версии не работают вместе. `RollingUpdate` заменяет поды постепенно без простоя.

</details>

**8. Для чего `revisionHistoryLimit`?**

<details><summary>Ответ</summary>

Сколько старых ReplicaSet хранить для отката (по умолчанию 10). При 0 откат невозможен; большее значение занимает больше объектов в etcd.

</details>

**9. Что показывает `Progressing=False (ProgressDeadlineExceeded)`?**

<details><summary>Ответ</summary>

Обновление не продвигалось дольше `progressDeadlineSeconds` (600 с по умолчанию). Это только сигнал: Kubernetes ничего не откатывает и продолжает попытки. В опыте при сроке 20 с `rollout status` вернул `error: deployment "web" exceeded its progress deadline`.

</details>

**10. (С подвохом) Почему `kubectl rollout history` показывает одинаковый CHANGE-CAUSE у двух ревизий?**

<details><summary>Ответ</summary>

Аннотация `kubernetes.io/change-cause` поставлена на Deployment до смены шаблона и скопирована на текущий (ещё старый) ReplicaSet. Подписывать нужно после изменения шаблона.

</details>

**11. Зачем `pause`/`resume`?**

<details><summary>Ответ</summary>

Чтобы внести несколько изменений одним обновлением. При паузе изменения шаблона не запускают rollout; после `resume` создаётся один ReplicaSet со всеми изменениями.

</details>

**12. Можно ли изменить `selector` существующего Deployment?**

<details><summary>Ответ</summary>

Нет, селектор неизменяем; метки шаблона обязаны с ним совпадать (ошибка валидации ``selector` does not match template `labels` ``). Для нового селектора создают новый Deployment.

</details>

## Шпаргалка

```bash
kubectl apply -f web.yaml
kubectl set image deploy/web nginx=nginx:1.29
kubectl annotate deploy/web kubernetes.io/change-cause="to 1.29" --overwrite   # после set image
kubectl rollout status deploy/web --timeout=120s        # код возврата для CI
kubectl rollout history deploy/web [--revision=N]
kubectl rollout undo deploy/web [--to-revision=N]
kubectl rollout pause|resume|restart deploy/web
kubectl scale deploy/web --replicas=5
kubectl get deploy,rs,pods -L pod-template-hash
kubectl get deploy web -o jsonpath='{.status.conditions[*].reason}'
```

## Связанные материалы

- Предыдущие уровни: [базовая](basics.ru.md), [продвинутая](advanced.ru.md)
- Следующий модуль: [06 · Service и сеть](../06-services/overview.ru.md)
- Предыдущие модули: [04 · Pod](../04-pods/overview.ru.md), [02 · Архитектура](../02-architecture/overview.ru.md)

## Источники

- [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Deployment: status and conditions](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#deployment-status)
- [Deployment API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/deployment-v1/)
- [ReplicaSet](https://kubernetes.io/docs/concepts/workloads/controllers/replicaset/)
- [Well-Known Labels, Annotations and Taints](https://kubernetes.io/docs/reference/labels-annotations-taints/)
- [kubectl rollout](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_rollout/)
