---
title: "Упражнения: Pod"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 04-pods
part: 1
tags: [pod, restart-policy, init-container, sidecar, termination, emptydir]
status: draft
updated: 2026-11-22
---

# Упражнения: Pod

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-04`. Убрать за собой: `kubectl delete namespace learn-04`.

## 1. 🟢 Три политики перезапуска

**Задача:** запусти три пода на `alpine:3.22`: один с `restartPolicy: Never` и командой, которая завершается с кодом 0; второй с `Never` и кодом 3; третий с политикой по умолчанию и кодом 1. Через минуту объясни статус, фазу и счётчик перезапусков каждого.

**Ожидаемый результат:**

```text
NAME        READY   STATUS      RESTARTS      AGE
once        0/1     Completed   0             45s
crash       0/1     Error       2 (34s ago)   45s
failnever   0/1     Error       0             45s
```

<details><summary>Подсказка</summary>

Фаза лежит в `status.phase`, причина завершения — в `status.containerStatuses[0].state.terminated`. Код выхода задаёт `sh -c "exit N"`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: once}
spec:
  restartPolicy: Never
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "echo done; exit 0"]}
---
apiVersion: v1
kind: Pod
metadata: {name: crash}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "echo boom; exit 1"]}
---
apiVersion: v1
kind: Pod
metadata: {name: failnever}
spec:
  restartPolicy: Never
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "exit 3"]}
```

```text
$ kubectl -n learn-04 get pod failnever -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode}'
Failed reason=Error exit=3
$ kubectl -n learn-04 get pod once -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason}'
Succeeded reason=Completed
```

`once` завершился успешно и не перезапускается (`Succeeded`), `failnever` — с ошибкой, без перезапуска (`Failed`), `crash` с политикой `Always` перезапускается снова и снова, и `RESTARTS` растёт.

</details>

## 2. 🟢 Общий `localhost` и том

**Задача:** создай под из двух контейнеров: `web` (nginx, раздаёт `/usr/share/nginx/html`) и `writer` (alpine, каждые 2 секунды пишет время в `index.html`). Общий том — `emptyDir`. Из третьего контейнера `probe` проверь, что страница доступна по `localhost` и что у всех контейнеров один IP.

**Ожидаемый результат:** страница возвращает текущее время, `hostname -i` одинаков.

<details><summary>Подсказка</summary>

`emptyDir` монтируется в оба контейнера по разным путям. `wget -qO- http://localhost:80` выполни через `kubectl exec -c probe`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: multi}
spec:
  volumes:
    - {name: shared, emptyDir: {}}
  containers:
    - name: web
      image: nginx:1.29
      volumeMounts: [{name: shared, mountPath: /usr/share/nginx/html}]
    - name: writer
      image: alpine:3.22
      command: ["sh", "-c", "while true; do date +%T > /data/index.html; sleep 2; done"]
      volumeMounts: [{name: shared, mountPath: /data}]
    - {name: probe, image: alpine:3.22, command: ["sh", "-c", "sleep 3600"]}
```

```text
$ kubectl -n learn-04 exec multi -c probe -- wget -qO- http://localhost:80
16:26:43
$ kubectl -n learn-04 exec multi -c probe -- hostname -i
10.244.0.43
$ kubectl -n learn-04 exec multi -c web -- hostname -i
10.244.0.43
```

Контейнеры делят сеть пода, поэтому `localhost` у них общий, а общий `emptyDir` служит передаточным звеном между `writer` и `web`.

</details>

## 3. 🟡 Init-контейнеры

**Задача:** создай под с двумя init-контейнерами (`init-a` печатает текст и ждёт 6 секунд, `init-b` — 3 секунды) и основным контейнером. Проследи за статусами. Затем создай второй под, где init-контейнер завершается ошибкой, и объясни статус.

**Ожидаемый результат:** `Init:0/2` → `Init:1/2` → `Running`; для второго пода `Init:Error`.

<details><summary>Подсказка</summary>

`kubectl get pod -w` показывает смену статусов. Логи init-контейнера: `kubectl logs <под> -c <имя>`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: init-demo}
spec:
  initContainers:
    - {name: wait-a, image: alpine:3.22, command: ["sh", "-c", "echo init-a; sleep 6"]}
    - {name: wait-b, image: alpine:3.22, command: ["sh", "-c", "echo init-b; sleep 3"]}
  containers:
    - {name: app, image: alpine:3.22, command: ["sh", "-c", "echo app-started; sleep 3600"]}
```

```text
$ kubectl -n learn-04 get pod init-demo -w
0/1 Init:0/2
0/1 Init:1/2
1/1 Running
```

Второй под (`command: ["sh","-c","echo failing; exit 1"]` в init-контейнере) остаётся `Init:Error`, а `RESTARTS` растёт: основной контейнер не запускается, пока init-контейнер не завершится успешно.

</details>

## 4. 🟡 Скорость удаления

**Задача:** создай два пода на `alpine:3.22`: с `sleep 3600` и с `sh -c "trap 'echo got-SIGTERM; exit 0' TERM; sleep 3600 & wait"`. Измерь время `kubectl delete pod` для каждого. Затем сократи ожидание у первого до 5 секунд и измерь снова.

**Ожидаемый результат:** около 30 с, около 1 с и около 5 с.

<details><summary>Подсказка</summary>

`terminationGracePeriodSeconds` задаётся в `spec` пода. Время измеряй командой `date +%s` до и после `delete`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: term-ignore}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sleep", "3600"]}
---
apiVersion: v1
kind: Pod
metadata: {name: term-trap}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "trap 'echo got-SIGTERM; exit 0' TERM; sleep 3600 & wait"]}
---
apiVersion: v1
kind: Pod
metadata: {name: term-short}
spec:
  terminationGracePeriodSeconds: 5
  containers:
    - {name: c, image: alpine:3.22, command: ["sleep", "3600"]}
```

```text
term-ignore deleted in 32s
term-trap deleted in 1s
term-short deleted in 6s
```

`sleep` как PID 1 игнорирует `SIGTERM`, и приходит `SIGKILL` через 30 секунд; обработчик сигнала завершает процесс сразу; сокращённый `terminationGracePeriodSeconds` ограничивает ожидание.

</details>

## 5. 🔴 Нативный sidecar и OOMKilled

**Задача:** (а) создай под с нативным sidecar (`initContainers` с `restartPolicy: Always`, который пишет строку каждые 5 секунд) и основным контейнером, проверь `READY` и логи sidecar. (б) Создай под с `limits.memory: 20Mi` и командой `tail /dev/zero`, `restartPolicy: Never`, найди причину, код выхода и QoS-класс.

**Ожидаемый результат:** (а) `2/2 Running`, `sidecar-alive`; (б) `reason=OOMKilled exit=137 qos=Burstable`.

<details><summary>Подсказка</summary>

(а) Sidecar — это элемент списка `initContainers` с полем `restartPolicy: Always`. (б) `tail /dev/zero` читает бесконечный поток и копит его в памяти.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: sidecar-demo}
spec:
  initContainers:
    - name: logger
      image: alpine:3.22
      restartPolicy: Always
      command: ["sh", "-c", "while true; do echo sidecar-alive; sleep 5; done"]
  containers:
    - {name: app, image: alpine:3.22, command: ["sh", "-c", "echo app-up; sleep 3600"]}
---
apiVersion: v1
kind: Pod
metadata: {name: oom}
spec:
  restartPolicy: Never
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "tail /dev/zero"]
      resources:
        limits: {memory: 20Mi}
```

```text
$ kubectl -n learn-04 get pod sidecar-demo
NAME           READY   STATUS    RESTARTS   AGE
sidecar-demo   2/2     Running   0          1s
$ kubectl -n learn-04 logs sidecar-demo -c logger
sidecar-alive
$ kubectl -n learn-04 get pod oom -o jsonpath='phase={.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode} qos={.status.qosClass}'
phase=Failed reason=OOMKilled exit=137 qos=Burstable
```

Sidecar стартовал до основного контейнера и не завершается, поэтому под `2/2 Running`. Контейнер, превысивший лимит, убит `SIGKILL` (137 = 128 + 9) с причиной `OOMKilled`; класс `Burstable`, потому что задан только лимит памяти.

</details>
