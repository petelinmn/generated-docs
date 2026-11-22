---
title: "Pod: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 04-pods
part: 1
tags: [pod, container, lifecycle, restart-policy, crashloopbackoff, logs, events]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Pod: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Локальный кластер и kubectl](../03-local-cluster-and-kubectl/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в кластере kind (Kubernetes 1.37.0), namespace `learn-04`: `kubectl create namespace learn-04`.

## Зачем это нужно

Kubernetes не запускает контейнеры «голыми»: он оборачивает их в Pod. Под — единица планирования, сети и жизненного цикла. Если вы понимаете, как под переходит из `Pending` в `Running`, что означают `Completed` и `CrashLoopBackOff` и как читать события, вы можете разобраться с большинством проблем в кластере.

## Манифест пода

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: web
  labels:
    app: web
spec:
  containers:
    - name: nginx
      image: nginx:1.29
      ports:
        - containerPort: 80
```

- `metadata.name` — имя пода в namespace, `labels` — метки для выбора.
- `spec.containers` — список контейнеров; у каждого обязательны `name` и `image`.
- `ports` — информационное поле: оно документирует порт и нужно для именованных портов, но не открывает и не закрывает доступ.

```text
$ kubectl -n learn-04 apply -f web.yaml
pod/web created
```

## Фазы и статусы

В поле `status.phase` пять значений:

| Фаза | Значение |
|---|---|
| `Pending` | под принят, но контейнеры ещё не запущены (ждёт ноду, скачивание образа) |
| `Running` | под назначен на ноду, хотя бы один контейнер работает или перезапускается |
| `Succeeded` | все контейнеры завершились успешно и не будут перезапущены |
| `Failed` | все контейнеры завершились, хотя бы один с ошибкой |
| `Unknown` | состояние узнать не удалось (обычно потеря связи с нодой) |

Столбец `STATUS` в `kubectl get pods` подробнее. Пока под стартует, видно `ContainerCreating`:

```text
$ kubectl -n learn-04 get pod web
NAME   READY   STATUS              RESTARTS   AGE
web    0/1     ContainerCreating   0          2s
```

После запуска состояние пода описывают **условия** (`conditions`):

```text
$ kubectl -n learn-04 get pod web -o jsonpath='{range .status.conditions[*]}{.type}={.status}{"\n"}{end}'
PodReadyToStartContainers=True
Initialized=True
Ready=True
ContainersReady=True
PodScheduled=True
```

`PodScheduled` — нода выбрана, `Initialized` — init-контейнеры выполнены, `ContainersReady` и `Ready` — контейнеры готовы принимать трафик. Сводка по нашему поду:

```text
$ kubectl -n learn-04 get pod web -o jsonpath='phase={.status.phase} qos={.status.qosClass} ip={.status.podIP} restartPolicy={.spec.restartPolicy} dnsPolicy={.spec.dnsPolicy} grace={.spec.terminationGracePeriodSeconds}{"\n"}'
phase=Running qos=BestEffort ip=10.244.0.5 restartPolicy=Always dnsPolicy=ClusterFirst grace=30
```

Значения по умолчанию: `restartPolicy: Always`, `dnsPolicy: ClusterFirst`, `terminationGracePeriodSeconds: 30`. Класс QoS `BestEffort` потому, что у пода не заданы requests и limits ([модуль 11](../../overview.ru.md#часть-2-продвинутые-темы)).

## События и describe

Историю жизни пода показывает `kubectl describe pod`:

```text
$ kubectl -n learn-04 describe pod web
…
Events:
  Type    Reason     Age   From               Message
  ----    ------     ----  ----               -------
  Normal  Scheduled  19s   default-scheduler  Successfully assigned learn-04/web to learn-control-plane
  Normal  Pulling    19s   kubelet            spec.containers{nginx}: Pulling image "nginx:1.29"
  Normal  Pulled     0s    kubelet            spec.containers{nginx}: Successfully pulled image "nginx:1.29" in 18.092s (18.092s including waiting). Image size: 62964090 bytes.
  Normal  Created    0s    kubelet            spec.containers{nginx}: Container created
  Normal  Started    0s    kubelet            spec.containers{nginx}: Container started
```

Это та же цепочка, что в [модуле 02](../02-architecture/basics.ru.md#что-происходит-при-создании-deployment): scheduler назначил ноду, kubelet скачал образ и запустил контейнер.

## Логи и команды внутри пода

```bash
kubectl -n learn-04 logs web          # stdout и stderr контейнера
kubectl -n learn-04 logs web -f       # следить за логом
kubectl -n learn-04 exec web -- nginx -v
kubectl -n learn-04 exec -it web -- sh
```

Если контейнеров несколько, нужен флаг `-c <имя>` (подробнее в [продвинутой теории](advanced.ru.md#несколько-контейнеров-в-поде)).

## restartPolicy: что происходит, когда контейнер завершился

Три пода с разными политиками и командами (образ `alpine:3.22`):

```yaml
apiVersion: v1
kind: Pod
metadata: {name: once}
spec:
  restartPolicy: Never
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "echo done; exit 0"]
---
apiVersion: v1
kind: Pod
metadata: {name: crash}
spec:
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "echo boom; exit 1"]
---
apiVersion: v1
kind: Pod
metadata: {name: failnever}
spec:
  restartPolicy: Never
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "exit 3"]
```

Через 45 секунд:

```text
$ kubectl -n learn-04 get pods once crash failnever
NAME        READY   STATUS      RESTARTS      AGE
once        0/1     Completed   0             45s
crash       0/1     Error       2 (34s ago)   45s
failnever   0/1     Error       0             45s
$ kubectl -n learn-04 get pod failnever -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode}{"\n"}'
Failed reason=Error exit=3
$ kubectl -n learn-04 get pod once -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason}{"\n"}'
Succeeded reason=Completed
```

| Политика | Поведение |
|---|---|
| `Always` (по умолчанию) | перезапускать контейнер всегда, даже после успешного завершения |
| `OnFailure` | перезапускать только при ненулевом коде выхода |
| `Never` | не перезапускать |

`Completed` и `Error` — не фазы, а причины завершения контейнера в столбце `STATUS`: фаза пода `once` — `Succeeded`, `failnever` — `Failed`. Под `crash` с политикой `Always` постоянно перезапускается: счётчик `RESTARTS` растёт, а задержка между попытками увеличивается.

## CrashLoopBackOff

Когда контейнер падает снова и снова, kubelet увеличивает паузу между перезапусками (экспоненциально, с потолком). В это время статус пода меняется между `Error` и `CrashLoopBackOff`. Наблюдение за подом `crash` по 10 секунд:

```text
Error             4 (60s ago)
Error             4 (70s ago)
…
CrashLoopBackOff  4 (64s ago)
CrashLoopBackOff  4 (74s ago)
CrashLoopBackOff  4 (84s ago)
Error             5 (94s ago)
```

`CrashLoopBackOff` — не причина, а состояние ожидания: контейнер уже упал и kubelet ждёт перед следующей попыткой (в событиях `Back-off restarting failed container`). Причину ищите в логах и коде выхода:

```text
$ kubectl -n learn-04 logs crash
boom
$ kubectl -n learn-04 get pod crash -o jsonpath='restarts={.status.containerStatuses[0].restartCount} last={.status.containerStatuses[0].lastState.terminated.exitCode}{"\n"}'
restarts=2 last=1
```

Лог предыдущего запуска показывает `kubectl logs crash --previous`. В нашем опыте эта команда выдала `unable to retrieve container logs for containerd://…`: к моменту запроса контейнер уже был удалён. Если так — смотрите текущий лог в момент работы контейнера или `describe`.

## Удалить под

```bash
kubectl -n learn-04 delete pod web
kubectl -n learn-04 delete -f once.yaml
kubectl delete namespace learn-04     # убрать всё
```

Остановка занимает до `terminationGracePeriodSeconds` (подробности — в [продвинутой теории](advanced.ru.md#остановка-пода)).

## Ошибки новичка

- ❌ Считать, что `Running` значит «приложение работает» → ✅ `Running` значит, что контейнер запущен; готовность к трафику показывает `Ready` и readiness probe ([модуль 11](../../overview.ru.md#часть-2-продвинутые-темы)).
- ❌ Лечить `CrashLoopBackOff` перезапуском → ✅ смотреть `logs` и код выхода: причина в приложении или настройке.
- ❌ Запускать задачу «один раз» с политикой `Always` → ✅ для разовых задач `restartPolicy: Never` или `OnFailure` (а лучше Job, модуль 12).
- ❌ Создавать «голый» под для сервиса → ✅ использовать Deployment: контроллер пересоздаст под после сбоя.
- ❌ Ждать, что `ports` откроет порт → ✅ `containerPort` лишь документация; доступ задаёт Service.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — несколько контейнеров, init, остановка
- Следующий модуль: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- Предыдущие модули: [03 · kubectl](../03-local-cluster-and-kubectl/overview.ru.md), [02 · Архитектура](../02-architecture/overview.ru.md)
- Образы и `PID 1`: [01 · Контейнеры и Docker](../01-containers/overview.ru.md)

## Источники

- [Pods](https://kubernetes.io/docs/concepts/workloads/pods/)
- [Pod Lifecycle](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/)
- [Container restart policy](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#restart-policy)
- [Debug Running Pods](https://kubernetes.io/docs/tasks/debug/debug-application/debug-running-pod/)
- [Configure Quality of Service for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/quality-service-pod/)
