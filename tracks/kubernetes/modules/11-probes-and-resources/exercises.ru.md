---
title: "Упражнения: Probes и ресурсы"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 11-probes-and-resources
part: 2
tags: [probes, liveness-probe, readiness-probe, startup-probe, resources, qos, cpu-throttling]
status: draft
updated: 2026-11-23
---

# Упражнения: Probes и ресурсы

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-11`. Убрать за собой: `kubectl delete namespace learn-11`.

## 1. 🟢 Liveness перезапускает контейнер

**Задача:** запусти nginx с liveness-пробой `httpGet` на несуществующий путь `/healthz` (период 3 с, два провала). Найди в событиях причину перезапусков и объясни код выхода.

**Ожидаемый результат:** `RESTARTS` растёт, события `Unhealthy … 404` и `Killing … will be restarted`, код выхода `0` (`Completed`).

<details><summary>Подсказка</summary>

`kubectl describe pod` показывает `Last State` и события; параметры — `periodSeconds: 3`, `failureThreshold: 2`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: live-http}
spec:
  containers:
    - name: c
      image: nginx:1.29
      livenessProbe:
        httpGet: {path: /healthz, port: 80}
        periodSeconds: 3
        failureThreshold: 2
```

```text
live-http   1/1   Running   2 (1s ago)   25s
    Last State:     Terminated
      Reason:       Completed
      Exit Code:    0
  Warning  Unhealthy  Liveness probe failed: HTTP probe failed with statuscode: 404
  Normal   Killing    Container c failed liveness probe, will be restarted
```

nginx получил `SIGTERM` и штатно завершился с кодом 0: это решение kubelet, а не падение приложения.

</details>

## 2. 🟢 Readiness снимает трафик, но не перезапускает

**Задача:** запусти под с `exec`-readiness (`cat /tmp/ready`), убери файл и покажи, что `READY` стал `0/1`, а перезапусков нет. Верни файл.

**Ожидаемый результат:** `0/1 Running` без роста `RESTARTS`; после возврата файла `1/1`.

<details><summary>Подсказка</summary>

Файл создай командой запуска: `touch /tmp/ready; exec nginx -g 'daemon off;'`. Условия пода — `-o jsonpath='{range .status.conditions[*]}{.type}={.status} {end}'`.

</details>

<details><summary>Решение</summary>

```yaml
spec:
  containers:
    - name: c
      image: nginx:1.29
      command: ["sh", "-c", "touch /tmp/ready; exec nginx -g 'daemon off;'"]
      readinessProbe:
        exec: {command: ["cat", "/tmp/ready"]}
        periodSeconds: 2
```

```text
ready-flip: 1/1
after rm: 0/1 Running
PodReadyToStartContainers=True Initialized=True Ready=False ContainersReady=False PodScheduled=True
after touch: 1/1 Running
```

Провал readiness меняет `Ready`, но не перезапускает контейнер; адрес пропадает из endpoints Service.

</details>

## 3. 🟡 Startup-проба против цикла перезапусков

**Задача:** запусти два пода, имитирующих 25-секундный старт (`sleep 25; exec nginx -g 'daemon off;'`): оба с liveness (период 3 с, два провала), но только второй со startup-пробой (период 3 с, 15 провалов). Следи за ними минуту и объясни разницу.

**Ожидаемый результат:** первый перезапускается (`r=1`), второй стартует без перезапусков.

<details><summary>Подсказка</summary>

Startup блокирует liveness и readiness до первого успеха. Запас — `failureThreshold × periodSeconds`.

</details>

<details><summary>Решение</summary>

```yaml
startupProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
  failureThreshold: 15
```

```text
t~36s: slow-nostartup[1/1 Running r=0]  slow-startup[1/1 Running r=0]
t~48s: slow-nostartup[1/1 Running r=1]  slow-startup[1/1 Running r=0]
```

Без startup liveness убивает приложение до старта (`connection refused`). Со startup liveness не запускается, пока приложение не ответит; запас 45 секунд. Заметь: без readiness первый под был `1/1` с первой секунды, хотя порт ещё был закрыт.

</details>

## 4. 🟡 QoS и значения cgroup

**Задача:** создай четыре пода: `Guaranteed` (requests = limits = 100m/64Mi), с `requests 50m/32Mi` и лимитом только по памяти `128Mi`, без ресурсов, и только с `requests 100m/64Mi`. Выведи QoS-класс каждого, а изнутри — `cpu.max`, `memory.max`, `cpu.weight` и `oom_score_adj`.

**Ожидаемый результат:** классы `Guaranteed`, `Burstable`, `BestEffort`, `Burstable`; `cpu.max=10000 100000` и `memory.max=67108864` у первого; `oom_score_adj` `-997`, `999`, `1000`, `998`.

<details><summary>Подсказка</summary>

`kubectl exec <под> -- cat /sys/fs/cgroup/cpu.max /sys/fs/cgroup/memory.max /sys/fs/cgroup/cpu.weight /proc/1/oom_score_adj`.

</details>

<details><summary>Решение</summary>

```text
guaranteed -> Guaranteed     cpu.max=10000 100000  memory.max=67108864  cpu.weight=17  oom_score_adj=-997
burstable  -> Burstable      cpu.max=max 100000    memory.max=134217728 cpu.weight=11  oom_score_adj=999
besteffort -> BestEffort     cpu.max=max 100000    memory.max=max       cpu.weight=1   oom_score_adj=1000
onlyreq    -> Burstable      cpu.weight=17                                             oom_score_adj=998
```

Лимит превращается в `cpu.max` и `memory.max`, заявка по CPU — в `cpu.weight`, а QoS-класс отражается в `oom_score_adj`: у `BestEffort` убьют первым.

</details>

## 5. 🔴 Throttling, планирование и невалидные ресурсы

**Задача:** (а) запусти два пода, жгущих CPU 20 секунд: один с лимитом `100m`, второй без лимита; сравни `usage_usec`, `nr_throttled`. (б) Создай под с `requests.cpu: "100"` и найди причину `Pending`. (в) Создай под с `requests.cpu: 500m` и `limits.cpu: 100m`.

**Ожидаемый результат:** (а) около 2 с CPU и `nr_throttled == nr_periods` у первого, 20 с у второго; (б) `Insufficient cpu`; (в) `must be less than or equal to cpu limit`.

<details><summary>Подсказка</summary>

(а) `timeout 20 sh -c 'while :; do :; done'`, затем `cat /sys/fs/cgroup/cpu.stat`. (б) `kubectl describe pod`, раздел событий.

</details>

<details><summary>Решение</summary>

```text
=== burn-limited (limit 100m)
usage_usec 2023496
nr_periods 202
nr_throttled 202
throttled_usec 8562744
=== burn-free (без лимита)
usage_usec 20017330
nr_throttled 0
0/1 Pending
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 Insufficient cpu. preemption: 0/1 nodes are available: 1 Preemption is not helpful for scheduling.
The Pod "badlimits" is invalid: spec.containers[0].resources.requests: Invalid value: "500m": must be less than or equal to cpu limit of 100m
```

Лимит притормаживает независимо от загрузки ноды; планировщик смотрит на заявки; лимит не может быть меньше заявки.

</details>
