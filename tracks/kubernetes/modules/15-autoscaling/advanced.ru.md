---
title: "Автомасштабирование: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 15-autoscaling
part: 3
tags: [hpa, behavior, stabilization-window, custom-metrics, vpa, cluster-autoscaler, autoscaling]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Автомасштабирование: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): установка metrics-server и первый HPA не повторяются.

## Скорость: `behavior`

Поведение масштабирования вверх и вниз задаёт блок `behavior`. В опыте HPA создан с одним параметром:

```yaml
behavior:
  scaleDown: {stabilizationWindowSeconds: 30}
```

`kubectl describe hpa` показывает итоговые значения, дополненные умолчаниями:

```text
Behavior:
  Scale Up:
    Stabilization Window: 0 seconds
    Select Policy: Max
    Policies:
      - Type: Pods     Value: 4    Period: 15 seconds
      - Type: Percent  Value: 100  Period: 15 seconds
  Scale Down:
    Stabilization Window: 30 seconds
    Select Policy: Max
    Policies:
      - Type: Percent  Value: 100  Period: 15 seconds
```

Вверх по умолчанию: окно стабилизации 0, за 15 секунд можно добавить до 4 подов **или** 100% (выбирается политика с большим эффектом: `Select Policy: Max`). Поэтому в опыте реплики выросли с 1 до 5 за один шаг (+4). Вниз: сжатие допускается до 100% за 15 секунд, а окно стабилизации — то, что вы задали (30 с).

### Окно стабилизации вниз

Окно удерживает число реплик от снижения, пока в нём были более высокие рекомендации. Чтобы увидеть умолчание, на Deployment `d2` с HPA (`--min=1 --max=3`) вручную поставили 3 реплики:

```text
$ kubectl -n learn-15 scale deploy d2 --replicas=3
t=+95s  d2 replicas=3
t=+205s d2 replicas=3
t=+260s d2 replicas=3
t=+315s d2 replicas=1
```

Нагрузки нет, но HPA держал 3 реплики примерно пять минут и только после этого снизил до `minReplicas`. Это окно стабилизации по умолчанию (300 секунд).

> ⚠️ Verify: точное значение умолчания (300 с) и отсутствие окна у `scaleUp` взяты из документации Kubernetes и согласуются с наблюдением (поды сжались между 260 и 315 секундами); отдельно параметр `scaleDown` не выводился командой describe без `behavior`.

## Несколько метрик

HPA может учитывать несколько метрик: он считает желаемое число реплик по каждой и берёт **наибольшее**.

```yaml
metrics:
  - type: Resource
    resource: {name: cpu, target: {type: Utilization, averageUtilization: 50}}
  - type: Resource
    resource: {name: memory, target: {type: AverageValue, averageValue: 100Mi}}
```

```text
$ kubectl -n learn-15 get hpa multi
NAME    REFERENCE               TARGETS                              MINPODS   MAXPODS   REPLICAS
multi   Deployment/php-apache   cpu: 0%/50%, memory: 12276Ki/100Mi   1         4         1
```

Типы целей: `Utilization` (процент от `requests`, только для Resource) и `AverageValue` (абсолютное значение на под).

## Метрики не из metrics-server

Типы метрик `Pods`, `Object`, `External` берутся не из Metrics API, а из custom/external metrics API, которые предоставляет адаптер (Prometheus Adapter, KEDA и др.). Без адаптера HPA не работает:

```yaml
metrics:
  - type: Pods
    pods:
      metric: {name: http_requests_per_second}
      target: {type: AverageValue, averageValue: "10"}
```

```text
$ kubectl -n learn-15 get hpa custom
NAME     REFERENCE       TARGETS         MINPODS   MAXPODS   REPLICAS
custom   Deployment/d2   <unknown>/10    1         3         1
  ScalingActive  False   FailedGetPodsMetric  the HPA was unable to compute the replica count: unable to get metric http_requests_per_second: unable to fetch metrics from custom metrics API: no custom metrics API (custom.metrics.k8s.io) registered
```

> ⚠️ Verify: установка адаптера и работа HPA по custom/external метрикам не запускались (в кластере не было адаптера).

## Ошибки и ограничения

**Два HPA на одну цель.** Второй HPA на тот же Deployment ломает оба:

```text
ScalingActive  False  AmbiguousSelector  pods by selector app=d2 are controlled by more than one HPA (e.g. [learn-15/custom learn-15/d2])
```

После удаления лишнего HPA условие исчезло.

**`minReplicas: 0`.** Для HPA только с Resource-метриками отвергается:

```text
The HorizontalPodAutoscaler "nor" is invalid: spec.metrics: Forbidden: must specify at least one Object or External metric to support scaling to zero replicas
```

Сообщение говорит, что масштабирование до нуля требует хотя бы одной метрики `Object` или `External`.

> ⚠️ Verify: условия включения scale-to-zero (feature gate и версия) не проверялись.

**Нет `requests`.** Метрика `<unknown>`, условие `ScalingActive=False (FailedGetResourceMetric)`: `missing request for cpu in container nginx of Pod …`.

**Ручное `scale`.** Вручную заданное число реплик HPA не отменяет мгновенно: оно удерживалось до конца окна стабилизации (см. выше), а затем HPA вернул число в границы `minReplicas`/`maxReplicas`.

## VPA и Cluster Autoscaler

| Компонент | Что делает | Как ставится |
|---|---|---|
| HPA | число реплик | встроен в Kubernetes |
| VPA (Vertical Pod Autoscaler) | подбирает `requests` и `limits` подов | отдельно (CRD и контроллеры) |
| Cluster Autoscaler | добавляет и убирает ноды, когда поды не помещаются | отдельно, интеграция с облаком |

> ⚠️ Verify: VPA и Cluster Autoscaler в этом материале не запускались (kind не имеет облачных нод); описания даны по документации. Совместное использование HPA и VPA по одной и той же метрике (CPU) документация не рекомендует.

## Лучшие практики

- Всегда задавайте `requests` у контейнеров, по которым масштабируете.
- Подбирайте цель так, чтобы оставался запас на всплеск: 50–70% утилизации — типичный старт.
- `maxReplicas` — страховка бюджета и нижележащих систем.
- Не задавайте `replicas` в манифесте, который вы применяете повторно: каждый деплой будет сбрасывать число реплик.
- Для резких пиков вверх — увеличить политики `scaleUp`; для защиты от «пилы» — окно стабилизации вниз.
- Проверяйте `kubectl describe hpa`: условия и события объясняют, почему HPA не масштабирует.

## Частые ошибки

- ❌ Два HPA на один Deployment → ✅ один HPA с несколькими метриками.
- ❌ HPA по `Pods`/`External` без адаптера → ✅ сначала установить адаптер и проверить API.
- ❌ Цель CPU 100% → ✅ запас под пики.
- ❌ Ручной `scale` вместо изменения границ HPA → ✅ менять `minReplicas`/`maxReplicas`.

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): CPU `limits` превращаются в `cpu.max` и троттлинг ([модуль 11](../11-probes-and-resources/advanced.ru.md#cpu-throttling)); HPA считает процент от `requests`, а не от `limits`.
- [C#](../../../csharp/recipes/background-jobs/overview.ru.md): воркеры по очереди часто масштабируют по длине очереди (external-метрика, например KEDA), а не по CPU.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Requests, limits и QoS: [11 · Probes и ресурсы](../11-probes-and-resources/advanced.ru.md)
- Rolling update при смене числа реплик: [05 · Deployment](../05-deployments/advanced.ru.md#rolling-update-по-шагам)

## Источники

- [HPA: algorithm details](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/#algorithm-details)
- [HPA: configurable scaling behavior](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/#configurable-scaling-behavior)
- [HPA: support for multiple metrics](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale-walkthrough/#autoscaling-on-multiple-metrics-and-custom-metrics)
- [Vertical Pod Autoscaling](https://kubernetes.io/docs/concepts/workloads/autoscaling/vertical-pod-autoscale/)
- [Node autoscaling](https://kubernetes.io/docs/concepts/cluster-administration/node-autoscaling/)
