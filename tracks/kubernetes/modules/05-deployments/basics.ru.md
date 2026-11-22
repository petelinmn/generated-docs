---
title: "Deployment и ReplicaSet: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 05-deployments
part: 1
tags: [deployment, replicaset, scaling, rollout, rollback, pod-template-hash]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment и ReplicaSet: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-05`: `kubectl create namespace learn-05`.

## Зачем это нужно

Одиночный под не вернётся, если его удалят или упадёт нода. Приложение нужно запускать несколькими репликами и обновлять без простоя. Deployment решает обе задачи: вы описываете, какой образ и сколько копий нужно, а Kubernetes поддерживает это состояние и умеет менять его постепенно.

## Манифест Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web
spec:
  replicas: 3
  selector:
    matchLabels:
      app: web
  template:
    metadata:
      labels:
        app: web
    spec:
      containers:
        - name: nginx
          image: nginx:1.28
          ports:
            - containerPort: 80
```

- `replicas` — сколько подов нужно.
- `selector.matchLabels` — по каким меткам Deployment находит «свои» поды.
- `template` — шаблон пода: всё, что в `spec` обычного пода (образ, порты, переменные).
- Метки `template.metadata.labels` обязаны совпадать с `selector`.

```text
$ kubectl -n learn-05 apply -f web.yaml
deployment.apps/web created
$ kubectl -n learn-05 rollout status deploy/web
deployment "web" successfully rolled out
```

## Deployment, ReplicaSet и поды

```text
$ kubectl -n learn-05 get deploy,rs,pods -L pod-template-hash
NAME                  READY  UP-TO-DATE  AVAILABLE  AGE
deployment.apps/web   3/3    3           3          88s

NAME                           DESIRED  CURRENT  READY  AGE
replicaset.apps/web-6d8567df8  3        3        3      88s

NAME                     READY  STATUS   RESTARTS  AGE
pod/web-6d8567df8-bqbpd  1/1    Running  0         88s
pod/web-6d8567df8-fjvhm  1/1    Running  0         88s
pod/web-6d8567df8-n9kgg  1/1    Running  0         88s
```

Deployment создал ReplicaSet `web-6d8567df8`, а тот — три пода. Имя ReplicaSet состоит из имени Deployment и хэша шаблона пода (`6d8567df8`); имя пода — из имени ReplicaSet и случайного суффикса. Хэш также записан в метке `pod-template-hash`.

Столбцы Deployment:

| Столбец | Смысл |
|---|---|
| `READY` | готовых подов / желаемых |
| `UP-TO-DATE` | подов с актуальным шаблоном |
| `AVAILABLE` | подов, доступных пользователям |

Значения по умолчанию, которые Kubernetes подставил сам:

```text
$ kubectl -n learn-05 get deploy web -o jsonpath='strategy={.spec.strategy}{"\n"}revHist={.spec.revisionHistoryLimit} progressDeadline={.spec.progressDeadlineSeconds}{"\n"}'
strategy={"rollingUpdate":{"maxSurge":"25%","maxUnavailable":"25%"},"type":"RollingUpdate"}
revHist=10 progressDeadline=600
```

## Масштабирование

```text
$ kubectl -n learn-05 scale deploy/web --replicas=5
deployment.apps/web scaled
$ kubectl -n learn-05 get deploy web --no-headers | awk '{print $2}'
3/5
```

Сразу после команды `READY 3/5`: недостающие поды ещё стартуют. Тот же результат даёт правка `replicas` в манифесте и `apply`. Масштабирование не пересоздаёт существующие поды и не создаёт новый ReplicaSet.

## Обновление образа

Команда `kubectl set image` меняет шаблон пода:

```text
$ kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
$ kubectl -n learn-05 rollout status deploy/web
Waiting for deployment "web" rollout to finish: 1 out of 3 new replicas have been updated...
Waiting for deployment "web" rollout to finish: 2 out of 3 new replicas have been updated...
Waiting for deployment "web" rollout to finish: 1 old replicas are pending termination...
deployment "web" successfully rolled out
```

Формат `nginx=nginx:1.29` — `имя-контейнера=образ`. Изменение шаблона дало новый хэш и новый ReplicaSet; старый ReplicaSet уменьшился до нуля реплик, но остался для отката:

```text
$ kubectl -n learn-05 get rs
NAME            DESIRED   CURRENT   READY
web-6d8567df8   0         0         0
web-c5748c689   3         3         3
```

(столбец AGE опущен). В декларативном стиле вы правите образ в YAML и делаете `apply`: результат тот же.

## Откат

```text
$ kubectl -n learn-05 rollout undo deploy/web
deployment.apps/web rolled back
$ kubectl -n learn-05 rollout status deploy/web
deployment "web" successfully rolled out
$ kubectl -n learn-05 get deploy web -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
nginx:1.28
```

`rollout undo` возвращает шаблон предыдущей ревизии. История и откат к конкретной ревизии — в [продвинутой теории](advanced.ru.md#история-ревизий-и-откат).

> ⚠️ После отката `kubectl` может предупредить: `resource deployments/web was previously managed with 'kubectl apply'. Rolling back will not update the kubectl.kubernetes.io/last-applied-configuration annotation`. Если вы ведёте манифесты в Git, после отката приведите файл в соответствие с кластером, иначе следующий `apply` вернёт старую версию.

## Самовосстановление

ReplicaSet следит за числом подов: удалённый под появляется снова ([модуль 02](../02-architecture/basics.ru.md#самовосстановление)). Поэтому для остановки приложения уменьшают `replicas` (`--replicas=0`) или удаляют Deployment.

```bash
kubectl -n learn-05 delete deploy web       # удалит и ReplicaSet, и поды
kubectl delete namespace learn-05           # убрать всё
```

## Ошибки новичка

- ❌ Править поды вручную или удалять их, чтобы «обновить» → ✅ менять шаблон Deployment: контроллер сам заменит поды.
- ❌ Не совпадают `selector` и метки шаблона → ✅ они должны совпадать, иначе `apply` отклонён.
- ❌ Использовать тег `latest` и ждать обновления → ✅ без смены шаблона обновления нет; задавайте уникальные теги образа.
- ❌ Масштабировать и ждать нового ReplicaSet → ✅ масштабирование не меняет шаблон и ReplicaSet тот же.
- ❌ Ожидать, что `READY 3/5` сразу после `scale` — ошибка → ✅ поды ещё запускаются; смотрите `rollout status`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — rolling update по шагам, откат, застрявший деплой
- Следующий модуль: [06 · Service и сеть](../06-services/overview.ru.md)
- Предыдущий модуль: [04 · Pod](../04-pods/overview.ru.md)
- Цикл согласования: [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)

## Источники

- [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [ReplicaSet](https://kubernetes.io/docs/concepts/workloads/controllers/replicaset/)
- [Performing a Rolling Update](https://kubernetes.io/docs/tutorials/kubernetes-basics/update/update-intro/)
- [kubectl rollout](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_rollout/)
- [kubectl scale](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_scale/)
