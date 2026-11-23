---
title: "Автомасштабирование"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 15-autoscaling
part: 3
levels: [1, 2, 3, 4]
tags: [hpa, autoscaling, metrics-server, vpa, cluster-autoscaler, requests, kubectl-top]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Автомасштабирование

🌐 **Русский** · [English](overview.en.md)

## Коротко

Число реплик не нужно подбирать вручную: **HorizontalPodAutoscaler (HPA)** сам меняет `replicas` у Deployment (или другого масштабируемого объекта) по метрикам нагрузки. Для метрик CPU и памяти нужен **metrics-server**, который отдаёт Metrics API (`kubectl top` работает на нём же). Остальные виды автомасштабирования — **VPA** (подбор requests/limits) и **Cluster Autoscaler** (число нод) — устанавливаются отдельно.

## Ключевые идеи

- **HPA сравнивает текущую метрику с целевой** и считает нужное число реплик: `ceil(текущее число × текущее / целевое)`. В опыте 1 под при 250% против цели 50% → 5 реплик.
- **Утилизация CPU считается от `requests`.** Без `requests` HPA не работает: `missing request for cpu`.
- **Нужен metrics-server.** Без него `kubectl top` и HPA по CPU/памяти не получают данных.
- **`minReplicas` не может быть 0** для HPA только по Resource-метрикам; `maxReplicas` ограничивает рост (`TooManyReplicas`).
- **`behavior` управляет скоростью:** по умолчанию вверх быстро (до +4 пода или +100% за 15 с), вниз медленно: окно стабилизации 5 минут.
- **Метрики `Pods`, `Object`, `External` требуют адаптера** (custom/external metrics API); без него `ScalingActive=False`.
- **Один Deployment — один HPA:** два HPA на один объект дают `AmbiguousSelector`.

## Минимальный пример

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

```text
$ kubectl -n learn-15 get hpa php-apache
NAME         REFERENCE               TARGETS       MINPODS   MAXPODS   REPLICAS
php-apache   Deployment/php-apache   cpu: 250%/50%   1       5         1
… через 15 с
php-apache   Deployment/php-apache   cpu: 250%/50%   1       5         5
```

## Когда применять, а когда нет

| Ситуация | Решение |
|---|---|
| Stateless-сервис с переменной нагрузкой | HPA |
| Не знаем правильные requests | VPA (в режиме рекомендаций) |
| Не хватает нод | Cluster Autoscaler |
| StatefulSet с жёстким порядком | осторожно: масштабирование тоже идёт по порядку |

- ❌ Не включайте HPA без `requests`: метрика будет `<unknown>`.
- ❌ Не задавайте `replicas` в манифесте, который применяется вместе с HPA, при каждом деплое: вы будете сбрасывать число реплик.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | metrics-server, `kubectl top`, первый HPA, как считаются реплики |
| [2. Продвинутая теория](advanced.ru.md) | `behavior`, окна стабилизации, несколько метрик, custom/external метрики, ошибки, VPA и Cluster Autoscaler |
| [3. Экспертная теория](expert.ru.md) | справочник полей и условий HPA, алгоритм, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 15-autoscaling level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 15-autoscaling level 6` |

## Связанные материалы

- Пререквизиты: [05 · Deployment](../05-deployments/overview.ru.md), [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md) (requests и limits)
- Предыдущий модуль: [14 · Kustomize](../14-kustomize/overview.ru.md)
- Дальше: модуль 16 · Наблюдаемость (в дорожной карте [трека](../../overview.ru.md))
- Масштабирование вручную: [05 · Deployment](../05-deployments/advanced.ru.md)
