---
title: "Наблюдаемость: логи, события, метрики"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 16-observability
part: 3
levels: [1, 2, 3, 4]
tags: [observability, logs, events, metrics, prometheus, grafana, kube-state-metrics, cadvisor, fluent-bit, promql]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, Prometheus v3.16.0 (chart 30.0.0), Grafana 13.3.1 (chart 14.5.0), fluent-bit 4.1, Helm 4.3"
status: draft
updated: 2026-11-23
---

# Наблюдаемость: логи, события, метрики

🌐 **Русский** · [English](overview.en.md)

## Коротко

Чтобы понять, что происходит в кластере, есть три источника. **Логи** — то, что контейнеры пишут в stdout и stderr (`kubectl logs`). **События (events)** — записи Kubernetes о том, что случилось с объектом: назначен на ноду, не удалось загрузить образ, перезапущен. **Метрики** — числовые ряды во времени: нагрузка, число перезапусков, задержки. Для логов и метрик «из коробки» есть только текущее состояние; история и графики требуют отдельных компонентов: сборщика логов (например, fluent-bit), **Prometheus** для метрик и **Grafana** для дашбордов.

## Ключевые идеи

- **`kubectl logs` читает файлы на ноде.** Kubelet хранит вывод контейнера на диске (`/var/log/pods/…`) с ротацией (10Mi и 5 файлов по умолчанию в опыте).
- **У пода с несколькими контейнерами нужно указывать контейнер:** `-c`, `--all-containers`, `--prefix`.
- **Логи упавшего контейнера — `--previous`,** но он работает не всегда: kubelet хранит только последний завершённый контейнер.
- **События живут недолго** и привязаны к объекту: `kubectl describe`, `kubectl events`, `--field-selector`.
- **Метрики отдают endpoint'ы** `/metrics`: kube-apiserver, kubelet и cAdvisor, kube-state-metrics, ваши приложения.
- **Prometheus сам забирает метрики (pull)** по обнаружению сервисов Kubernetes; `rate()` требует окно хотя бы в несколько интервалов опроса.
- **Prometheus 3 требует корректный `Content-Type`** у `/metrics`.
- **Логи на уровне кластера собирает DaemonSet** — агент на каждой ноде читает те же файлы.

## Минимальный пример

```text
$ kubectl -n learn-16 logs multi --all-containers --prefix --tail=2
[pod/multi/app] app line 19
[pod/multi/app] app line 20
[pod/multi/side] side-log
[pod/multi/side] side-log
$ kubectl -n learn-16 events --for pod/crash --types=Warning
LAST SEEN           TYPE      REASON    OBJECT      MESSAGE
48s (x3 over 86s)   Warning   BackOff   Pod/crash   Back-off restarting failed container c in pod crash_learn-16(…)
```

## Когда применять, а когда нет

| Вопрос | Источник |
|---|---|
| Что выводит приложение прямо сейчас | `kubectl logs` |
| Почему под не стартует, перезапускается | события (`describe`, `events`) |
| Как менялась нагрузка за последние часы | Prometheus + Grafana |
| Что было в логах вчера, после удаления пода | сборщик логов в хранилище |
| Число подов, перезапусков, статус нод | kube-state-metrics |

- ❌ Не храните историю логов в самих подах: после удаления пода `kubectl logs` не поможет.
- ❌ Не выводите секреты в логи: они копируются в агрегаторы.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `kubectl logs` и его флаги, события, `--previous`, где логи на ноде |
| [2. Продвинутая теория](advanced.ru.md) | сбор логов DaemonSet'ом, Prometheus: цели, PromQL, метрики приложения, Grafana, ошибки |
| [3. Экспертная теория](expert.ru.md) | справочник источников метрик и запросов, устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 16-observability level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 16-observability level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md) (`kubectl top`, requests)
- Метрики для автомасштабирования: [15 · Автомасштабирование](../15-autoscaling/overview.ru.md)
- Установка через Helm: [13 · Helm](../13-helm/overview.ru.md)
- DaemonSet: [12 · Другие workloads](../12-workloads/overview.ru.md)
- Дальше: [17 · Безопасность](../17-security/overview.ru.md)
- Диагностика по событиям и логам: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
