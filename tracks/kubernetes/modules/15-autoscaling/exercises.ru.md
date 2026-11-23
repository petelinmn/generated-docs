---
title: "Упражнения: автомасштабирование"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, requests]
status: draft
updated: 2026-11-23
---

# Упражнения: автомасштабирование

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-15`. После работы: `kubectl delete namespace learn-15`. Для упражнений нужен metrics-server (установка — в [базовой теории](basics.ru.md#metrics-server-и-kubectl-top)).

## 1. 🟢 metrics-server и `kubectl top`

**Задача:** установите metrics-server (в kind добавьте `--kubelet-insecure-tls`) и убедитесь, что `kubectl top nodes` и `kubectl top pod` возвращают данные.

**Ожидаемый результат:** APIService `v1beta1.metrics.k8s.io` в статусе `AVAILABLE True`, `kubectl top nodes` показывает CPU и память.

<details><summary>Подсказка</summary>

`kubectl get apiservice v1beta1.metrics.k8s.io`; данные появляются примерно через минуту после запуска.

</details>

<details><summary>Решение</summary>

```text
$ kubectl get apiservice v1beta1.metrics.k8s.io
NAME                     SERVICE                      AVAILABLE   AGE
v1beta1.metrics.k8s.io   kube-system/metrics-server   True        59s
$ kubectl top nodes
NAME                  CPU(cores)   CPU(%)   MEMORY(bytes)   MEMORY(%)
learn-control-plane   127m         1%       713Mi           2%
```

</details>

## 2. 🟢 Первый HPA под нагрузкой

**Задача:** разверните `php-apache` с `requests.cpu: 200m`, создайте HPA (`cpu 50%`, 1–5 реплик) и нагрузку. Покажите рост реплик, затем уберите нагрузку.

**Ожидаемый результат:** `cpu: 250%/50%` и 5 реплик под нагрузкой; через ~минуту после её снятия 1 реплика (при окне стабилизации `scaleDown` 30 с).

<details><summary>Подсказка</summary>

Нагрузка: `kubectl run load --image=busybox:1.37 --restart=Never -- sh -c "while sleep 0.01; do wget -q -O- http://php-apache >/dev/null; done"`. Наблюдение: `kubectl get hpa php-apache` раз в 15 секунд.

</details>

<details><summary>Решение</summary>

```text
t=45  cpu 250%/50%  replicas=1
t=60  cpu 250%/50%  replicas=5
t=75  cpu 76%/50%   replicas=5
… после kubectl delete pod load:
t=60 replicas=2
t=75 replicas=1
```

</details>

## 3. 🟡 Разбор `describe hpa`

**Задача:** для HPA из упражнения 2 найдите в `kubectl describe hpa` итоговые политики `behavior`, условие `ScalingLimited` и событие `SuccessfulRescale`. Рассчитайте по формуле, сколько реплик нужно было при 71% и 5 подах.

**Ожидаемый результат:** `ScalingLimited=True (TooManyReplicas)`; расчёт `ceil(5 × 71 / 50) = 8`, ограничено 5.

<details><summary>Подсказка</summary>

Раздел `Conditions` и `Events`.

</details>

<details><summary>Решение</summary>

```text
ScalingLimited  True  TooManyReplicas  the desired replica count is more than the maximum replica count
Normal  SuccessfulRescale  New size: 5; reason: cpu resource utilization (percentage of request) above target
```

</details>

## 4. 🟡 Нет `requests` и `minReplicas: 0`

**Задача:** создайте Deployment `nor` без `requests` и HPA на него. Найдите причину `<unknown>`. Затем попробуйте `minReplicas: 0`.

**Ожидаемый результат:** `missing request for cpu in container nginx of Pod …`; ошибка `must specify at least one Object or External metric to support scaling to zero replicas`.

<details><summary>Подсказка</summary>

`kubectl autoscale deploy nor --min=1 --max=3 --cpu=50%`; `kubectl patch hpa nor --type merge -p '{"spec":{"minReplicas":0}}'`.

</details>

<details><summary>Решение</summary>

```text
nor    Deployment/nor   cpu: <unknown>/50%   1   3   1
  ScalingActive  False   FailedGetResourceMetric  … missing request for cpu in container nginx of Pod nor-b475dd579-hwg4c
The HorizontalPodAutoscaler "nor" is invalid: spec.metrics: Forbidden: must specify at least one Object or External metric to support scaling to zero replicas
```

</details>

## 5. 🔴 Две метрики, дубликат HPA и custom-метрика

**Задача:** (а) создайте HPA с метриками cpu 50% и memory `100Mi`. (б) Создайте второй HPA на тот же Deployment и найдите `AmbiguousSelector`. (в) Создайте HPA с метрикой `Pods` `http_requests_per_second` и найдите причину `FailedGetPodsMetric`.

**Ожидаемый результат:** (а) `cpu: 0%/50%, memory: 12276Ki/100Mi`; (б) `AmbiguousSelector`; (в) `no custom metrics API (custom.metrics.k8s.io) registered`.

<details><summary>Подсказка</summary>

(б) два HPA с одним `scaleTargetRef`; (в) `type: Pods`, `pods.metric.name`, `target.type: AverageValue`.

</details>

<details><summary>Решение</summary>

```text
multi   Deployment/php-apache   cpu: 0%/50%, memory: 12276Ki/100Mi   1   4   1
ScalingActive  False  AmbiguousSelector  pods by selector app=d2 are controlled by more than one HPA (e.g. [learn-15/custom learn-15/d2])
ScalingActive  False  FailedGetPodsMetric  … unable to fetch metrics from custom metrics API: no custom metrics API (custom.metrics.k8s.io) registered
```

</details>
