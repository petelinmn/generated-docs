---
title: "Автомасштабирование: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, kubectl-top, requests]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Автомасштабирование: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Deployment](../05-deployments/overview.ru.md), [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-15`. Нагрузка создаётся подом, который в цикле шлёт запросы приложению.

## Зачем это нужно

Нагрузка меняется: днём запросов больше, ночью меньше. Держать максимум реплик постоянно дорого, держать минимум опасно. HPA наблюдает за метриками и сам подбирает число реплик между `minReplicas` и `maxReplicas`.

## metrics-server и `kubectl top`

CPU и память подов собирает **metrics-server**: он опрашивает kubelet на каждой ноде и отдаёт данные через Metrics API (`metrics.k8s.io`). В kind он не установлен ([модуль 11](../11-probes-and-resources/basics.ru.md#фактическое-потребление-kubectl-top) показывает `Metrics API not available`). Установка манифестом из релиза:

```text
$ kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

В опыте использован metrics-server v0.9.1; в kind к аргументам контейнера добавлен `--kubelet-insecure-tls`, чтобы он принимал самоподписанный сертификат kubelet (в production так делать не следует).

```text
$ kubectl -n kube-system rollout status deploy/metrics-server
deployment "metrics-server" successfully rolled out
$ kubectl get apiservice v1beta1.metrics.k8s.io
NAME                     SERVICE                      AVAILABLE   AGE
v1beta1.metrics.k8s.io   kube-system/metrics-server   True        59s
$ kubectl top nodes
NAME                  CPU(cores)   CPU(%)   MEMORY(bytes)   MEMORY(%)
learn-control-plane   127m         1%       713Mi           2%
$ kubectl -n learn-15 top pod --containers
POD                           NAME         CPU(cores)   MEMORY(bytes)
php-apache-5899f79df5-f9jkx   php-apache   1m           11Mi
```

Метрики появляются не сразу: после старта нужно около минуты. Сами данные — объект `PodMetrics`:

```text
$ kubectl get --raw /apis/metrics.k8s.io/v1beta1/namespaces/learn-15/pods
PodMetricsList d2-76fb4597fc-n22fg 15.903s {'cpu': '0', 'memory': '10560Ki'}
```

(`15.903s` — окно измерения.)

> ⚠️ Verify: без `--kubelet-insecure-tls` на kind работа metrics-server не проверялась.

## Приложение для опыта

Deployment `php-apache` с обязательными `requests` (без них HPA не посчитает процент):

```yaml
spec:
  template:
    spec:
      containers:
        - name: php-apache
          image: registry.k8s.io/hpa-example
          resources:
            limits: {cpu: 500m}
            requests: {cpu: 200m}
```

И Service `php-apache` на порт 80.

## Первый HPA

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata: {name: php-apache, namespace: learn-15}
spec:
  scaleTargetRef: {apiVersion: apps/v1, kind: Deployment, name: php-apache}
  minReplicas: 1
  maxReplicas: 5
  metrics:
    - type: Resource
      resource:
        name: cpu
        target: {type: Utilization, averageUtilization: 50}
```

Быстрая форма: `kubectl autoscale deploy d2 --min=1 --max=3 --cpu=60%` создаёт такой же объект (`averageUtilization: 60`).

Без нагрузки HPA показывает:

```text
$ kubectl -n learn-15 get hpa php-apache
NAME         REFERENCE               TARGETS       MINPODS   MAXPODS   REPLICAS   AGE
php-apache   Deployment/php-apache   cpu: 0%/50%   1         5         1          40s
```

`TARGETS` — текущее значение против цели: утилизация CPU считается от `requests` (200m), поэтому 100m — это 50%.

## Нагрузка и масштабирование

Запускаем нагрузку:

```text
$ kubectl -n learn-15 run load --image=busybox:1.37 --restart=Never -- sh -c "while sleep 0.01; do wget -q -O- http://php-apache >/dev/null; done"
```

Наблюдение каждые 15 секунд:

```text
t=15  cpu 28%/50%   replicas=1
t=45  cpu 250%/50%  replicas=1
t=60  cpu 250%/50%  replicas=5
t=75  cpu 76%/50%   replicas=5
t=90  cpu 57%/50%   replicas=5
t=105 cpu 67%/50%   replicas=5
```

Когда HPA увидел 250% при цели 50%, он за один шаг поднял реплики с 1 до 5 (максимум). Формула: `желаемое = ceil(текущих реплик × текущая метрика / цель)`. Для одного пода это `ceil(1 × 250 / 50) = 5`. Когда нагрузка распределилась по пяти подам, утилизация упала до 57–76%; она всё ещё выше 50%, но число реплик уже упёрлось в `maxReplicas`:

```text
$ kubectl -n learn-15 describe hpa php-apache
Metrics:                                               ( current / target )
  resource cpu on pods  (as a percentage of request):  71% (143m) / 50%
Min replicas:                                          1
Max replicas:                                          5
Deployment pods:       5 current / 5 desired
Conditions:
  ScalingLimited  True    TooManyReplicas  the desired replica count is more than the maximum replica count
Events:
  Normal   SuccessfulRescale   New size: 5; reason: cpu resource utilization (percentage of request) above target
```

При `71%` и 5 подах формула дала бы `ceil(5 × 71 / 50) = 8`, но потолок — 5 (`TooManyReplicas`).

Убираем нагрузку (`kubectl delete pod load`):

```text
t=45 replicas=5
t=60 replicas=2
t=75 replicas=1
```

Реплики вернулись к 1 примерно через минуту (в этом опыте окно стабилизации уменьшено до 30 секунд — подробнее в [продвинутой теории](advanced.ru.md#скорость-behavior)). По умолчанию вниз масштабирование идёт заметно медленнее.

## Ошибки новичка

- ❌ HPA на Deployment без `requests`:

```text
$ kubectl -n learn-15 get hpa nor
NAME   REFERENCE        TARGETS              MINPODS   MAXPODS   REPLICAS
nor    Deployment/nor   cpu: <unknown>/50%   1         3         1
  ScalingActive  False   FailedGetResourceMetric  … missing request for cpu in container nginx of Pod nor-b475dd579-hwg4c
```

  ✅ Всегда задавать `requests` у контейнеров, по которым масштабируем.
- ❌ HPA без metrics-server → ✅ сначала установить и проверить `kubectl top`.
- ❌ Ждать мгновенной реакции: в первые минуты события `FailedGetResourceMetric … no metrics returned` — нормальны, пока metrics-server не накопил данные.
- ❌ Ставить `minReplicas: 0` → ✅ отвергается (см. [продвинутую теорию](advanced.ru.md#ошибки-и-ограничения)).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `behavior`, несколько метрик, custom-метрики, VPA и Cluster Autoscaler
- Предыдущий модуль: [14 · Kustomize](../14-kustomize/overview.ru.md)
- Requests и limits: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- Deployment и масштабирование вручную: [05 · Deployment](../05-deployments/basics.ru.md#масштабирование)

## Источники

- [Horizontal Pod Autoscaling](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/)
- [HorizontalPodAutoscaler Walkthrough](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale-walkthrough/)
- [Resource metrics pipeline](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/)
- [metrics-server](https://github.com/kubernetes-sigs/metrics-server#readme)
- [kubectl autoscale](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_autoscale/)
