---
title: "Автомасштабирование: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, metrics-api, hpa-algorithm, scale-subresource]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Автомасштабирование: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что подтверждено опытом в kind (Kubernetes 1.37.0) — помечено «в опыте».

## Справочник

### Поля `autoscaling/v2` HorizontalPodAutoscaler

| Поле | Смысл |
|---|---|
| `spec.scaleTargetRef` | объект, который масштабируется (`apiVersion`, `kind`, `name`): нужен subresource `scale` |
| `spec.minReplicas` | минимум (по умолчанию 1); 0 — только с Object/External метриками (в опыте отвергнуто для Resource) |
| `spec.maxReplicas` | максимум (обязателен) |
| `spec.metrics[]` | список метрик; тип `Resource`, `ContainerResource`, `Pods`, `Object`, `External` |
| `spec.behavior` | `scaleUp` и `scaleDown`: `stabilizationWindowSeconds`, `selectPolicy`, `policies[]` (`type: Pods` или `Percent`, `value`, `periodSeconds`) |
| `status.currentReplicas`, `desiredReplicas` | текущее и желаемое число реплик |
| `status.currentMetrics` | последние значения метрик |
| `status.conditions` | `AbleToScale`, `ScalingActive`, `ScalingLimited` (и `ScaledToZero` в опыте) |

> ⚠️ Verify: значение по умолчанию `minReplicas`, поддержка `ContainerResource`, `Object`, `External` и `selectPolicy: Min/Disabled` взяты из документации и не запускались (в опыте использованы `Resource` и `Pods`, `Max`).

### Типы целей метрики

| `target.type` | Поле | Где применимо |
|---|---|---|
| `Utilization` | `averageUtilization` (проценты от `requests`) | Resource, ContainerResource |
| `AverageValue` | `averageValue` | Resource, Pods, Object, External |
| `Value` | `value` | Object, External |

В опыте проверены `Utilization` (cpu 50%) и `AverageValue` (memory `100Mi`, pods `10`).

### Условия HPA (в опыте)

| Условие | Пример причины |
|---|---|
| `AbleToScale` | `ReadyForNewScale`, `SucceededGetScale` |
| `ScalingActive` | `ValidMetricFound`; `FailedGetResourceMetric` (нет requests или metrics-server); `FailedGetPodsMetric` (нет custom API); `AmbiguousSelector` |
| `ScalingLimited` | `TooManyReplicas` (упёрлись в `maxReplicas`) |
| `ScaledToZero` | `NotScaledToZero` |

### Команды

| Команда | Назначение |
|---|---|
| `kubectl get hpa` | таблица: цели, границы, реплики |
| `kubectl describe hpa` | метрики, поведение, условия, события |
| `kubectl autoscale deploy X --min --max --cpu` | создать HPA |
| `kubectl top nodes/pods [--containers]` | данные Metrics API |
| `kubectl get --raw /apis/metrics.k8s.io/v1beta1/...` | сырой ответ Metrics API (`PodMetricsList`) |

## Внутреннее устройство

- **Цикл контроллера.** HPA-контроллер в kube-controller-manager периодически (раз в 15 секунд по документации) читает метрики, считает желаемое число реплик и пишет его через subresource `scale` целевого объекта. Deployment-контроллер затем создаёт поды ([модуль 05](../05-deployments/expert.ru.md)).
- **Формула.** `желаемое = ceil(текущее × текущая метрика / цель)`. В опыте: 1 под, 250% при цели 50% → `ceil(1 × 250 / 50) = 5`; 5 подов, 71% → `ceil(5 × 71 / 50) = 8`, но ограничение `maxReplicas: 5` (`TooManyReplicas`).
- **Процент от `requests`.** Для `Utilization` метрика — отношение потребления к `requests`: 143m при requests 200m — это 71%. Поэтому без requests процент не определён.
- **Источник данных.** metrics-server опрашивает kubelet и хранит только последний срез в памяти; окно в ответе `15.903s`. История и графики — задача мониторинга ([модуль 16](../../overview.ru.md) дорожной карты).
- **Несколько метрик.** HPA считает желаемое число по каждой и берёт максимум.
- **Политики `behavior`.** Рекомендации вверх и вниз ограничиваются политиками (`Pods` и `Percent` за период) и окном стабилизации; при `Select Policy: Max` выбирается политика, допускающая большее изменение (в опыте +4 пода за 15 с).
- **Владелец `replicas`.** HPA пишет в `scale`, а не в манифест: при повторном `kubectl apply` манифеста с `replicas` число сбросится до значения из манифеста до следующего цикла HPA.

> ⚠️ Verify: интервал опроса 15 секунд и допуск (tolerance) 10% при сравнении с целью взяты из документации; допуск не проверялся запуском. Поведение при повторном apply с `replicas` выведено из документации.

## Подвохи

### 1. `<unknown>` в `TARGETS`

Причин две: нет `requests` (`missing request for cpu in container nginx of Pod …`) или нет данных от metrics-server (в первые минуты `no metrics returned from resource metrics API`, `did not receive metrics for targeted pods (pods might be unready)`).

### 2. Процент считается от `requests`, а не от `limits`

Под с `limits: 500m`, `requests: 200m` при потреблении 143m — это 71% (в опыте). Занижённые `requests` дают завышенный процент и раннее масштабирование.

### 3. Масштабирование вниз медленное

Вручную поднятые три реплики держались около пяти минут без нагрузки (в опыте) — окно стабилизации по умолчанию. Для быстрого сжатия задавайте `behavior.scaleDown`.

### 4. Два HPA на один объект

`AmbiguousSelector`: оба HPA перестают работать.

### 5. Первое масштабирование идёт сразу до максимума

Политика вверх `Pods: 4` и `Percent: 100` за 15 секунд позволила пройти путь `1 → 5` за один шаг; для резких скачков метрики это может оказаться слишком агрессивно.

### 6. `minReplicas: 0`

Не работает для Resource-метрик: `must specify at least one Object or External metric to support scaling to zero replicas`.

### 7. Метрика `Pods` без адаптера

`FailedGetPodsMetric … no custom metrics API (custom.metrics.k8s.io) registered`: адаптер — отдельная установка.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| HPA по CPU | просто, встроено | CPU не всегда отражает нагрузку; нужны `requests` |
| HPA по custom/external метрикам | реагирует на бизнес-метрики (RPS, очередь) | нужен адаптер (Prometheus Adapter, KEDA) |
| VPA | подбирает ресурсы | перезапуск подов, не сочетается с HPA по тому же ресурсу |
| Cluster Autoscaler | число нод | интеграция с облаком, задержка |
| Фиксированное число реплик | предсказуемо | переплата или нехватка при пике |

## Вопросы с собеседований

<details><summary>1. Как HPA считает желаемое число реплик?</summary>

`ceil(текущее число реплик × текущая метрика / цель)`; результат ограничивается `minReplicas` и `maxReplicas`.

→ [Базовая теория](basics.ru.md#нагрузка-и-масштабирование)

</details>

<details><summary>2. Что нужно, чтобы HPA работал по CPU?</summary>

metrics-server (Metrics API) и `requests` по CPU у контейнеров целевых подов.

→ [Базовая теория](basics.ru.md#metrics-server-и-kubectl-top)

</details>

<details><summary>3. Ловушка: <code>TARGETS</code> показывает <code>&lt;unknown&gt;</code>. Почему?</summary>

Нет `requests`, нет metrics-server или метрики ещё не собраны (`FailedGetResourceMetric`).

→ [Подвох 1](#1-unknown-в-targets)

</details>

<details><summary>4. От чего считается процент утилизации CPU?</summary>

От `requests` контейнеров, не от `limits`.

→ [Подвох 2](#2-процент-считается-от-requests-а-не-от-limits)

</details>

<details><summary>5. Как быстро HPA масштабирует вверх и вниз по умолчанию?</summary>

Вверх: окно стабилизации 0, до +4 подов или +100% за 15 секунд. Вниз: окно стабилизации около 5 минут (в опыте ~300 с).

→ [Продвинутая теория](advanced.ru.md#скорость-behavior)

</details>

<details><summary>6. Как HPA ведёт себя с несколькими метриками?</summary>

Считает желаемое число реплик по каждой и берёт наибольшее.

→ [Продвинутая теория](advanced.ru.md#несколько-метрик)

</details>

<details><summary>7. Что произойдёт, если создать два HPA на один Deployment?</summary>

`AmbiguousSelector`: оба HPA отказываются работать.

→ [Подвох 4](#4-два-hpa-на-один-объект)

</details>

<details><summary>8. Можно ли поставить <code>minReplicas: 0</code>?</summary>

Для HPA только с Resource-метриками нет (ошибка валидации); для scale-to-zero нужны Object/External метрики.

→ [Подвох 6](#6-minreplicas-0)

</details>

<details><summary>9. Чем HPA отличается от VPA и Cluster Autoscaler?</summary>

HPA меняет число реплик, VPA — requests/limits, Cluster Autoscaler — число нод.

→ [Продвинутая теория](advanced.ru.md#vpa-и-cluster-autoscaler)

</details>

<details><summary>10. Для чего нужен адаптер метрик?</summary>

Для метрик `Pods`, `Object`, `External`: HPA берёт их из custom/external metrics API, а не из metrics-server.

→ [Подвох 7](#7-метрика-pods-без-адаптера)

</details>

<details><summary>11. Почему не стоит задавать <code>replicas</code> в манифесте при HPA?</summary>

Повторный apply вернёт заданное число, перебив решение HPA.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>12. Как увидеть, почему HPA не масштабирует?</summary>

`kubectl describe hpa`: условия (`ScalingActive`, `ScalingLimited`) и события; затем `kubectl top` и проверка `requests`.

→ [Условия HPA](#условия-hpa-в-опыте)

</details>

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Самопроверка: [квиз](quiz.ru.md), [упражнения](exercises.ru.md)
- Requests и QoS: [11 · Probes и ресурсы](../11-probes-and-resources/expert.ru.md)
- Deployment: [05 · Deployment](../05-deployments/expert.ru.md)

## Источники

- [HorizontalPodAutoscaler API (autoscaling/v2)](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/horizontal-pod-autoscaler-v2/)
- [HPA: algorithm details](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/#algorithm-details)
- [Resource metrics pipeline](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/)
- [Metrics APIs](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/#metrics-api)
