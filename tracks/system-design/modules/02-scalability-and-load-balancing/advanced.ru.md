---
title: "Масштабирование и балансировка: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: verified
updated: 2026-11-26
---

# Масштабирование и балансировка: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Взвешенный round robin

Если узлы разной мощности, им дают веса. Наивный вариант отдаёт тяжёлому узлу серию запросов подряд; «сглаженный» распределяет их равномернее.

```csharp
int best = 0;
for (int i = 0; i < nodes.Length; i++)
{
    _current[i] += nodes[i].Weight;
    if (_current[i] > _current[best]) best = i;
}
_current[best] -= _total;
return nodes[best].Node;
```

При весах a=5, b=1, c=1 первые семь выборов: `aabacaa`.

## Алгоритмы с учётом нагрузки

Least connections выбирает узел с минимумом активных запросов, но просматривает все узлы. Power of two choices берёт два случайных узла и выбирает менее загруженный.

```csharp
int a = random.Next(nodes.Count);
int b = random.Next(nodes.Count - 1);
if (b >= a) b++;
return nodes[a].Active <= nodes[b].Active ? nodes[a] : nodes[b];
```

Эксперимент: 100 000 незавершающихся запросов на 100 узлов с `Random(42)`. Случайный выбор дал от 923 до 1074 запросов на узел, два выбора — от 997 до 1002. В YARP `PowerOfTwoChoices` — политика по умолчанию.

## Consistent hashing

Когда запрос с одним ключом должен попадать на один узел (кэш, шард), `hash % N` плох: при изменении `N` переезжает почти всё. Consistent hashing располагает узлы и ключи на кольце; ключ принадлежит первому узлу по часовой стрелке.

```csharp
int i = Array.BinarySearch(_points, StableHash.Of(key));
if (i < 0) i = ~i;                         // first point clockwise from the key
return _ring[_points[i % _points.Length]]; // wrap around the end of the ring
```

Переход с 4 узлов на 5 на 100 000 ключей: `% N` переносит 80,1% ключей, кольцо — 15,8%. Каждый узел представлен множеством виртуальных точек, иначе доли получаются неравными:

| Виртуальных точек на узел | Доли четырёх узлов |
|---|---|
| 1 | 7,2% · 23,6% · 32,4% · 36,8% |
| 10 | 16,1% · 21,0% · 25,6% · 37,3% |
| 100 | 23,9% · 24,4% · 25,5% · 26,1% |
| 1000 | 24,2% · 25,0% · 25,3% · 25,4% |

Файл: [Ex02_ConsistentHashing.cs](code/Examples/Ex02_ConsistentHashing.cs). Хэш должен быть стабильным между процессами: `string.GetHashCode()` для этого не подходит.

## Проверки здоровья

- **Активные:** балансировщик сам опрашивает endpoint.
- **Пассивные:** он наблюдает за ошибками настоящих запросов.

В YARP оба вида по умолчанию выключены. Активная встроенная политика — `ConsecutiveFailures` (интервал по умолчанию 15 с, таймаут 10 с, порог 2), пассивная — `TransportFailureRate`.

## Session affinity

Привязка клиента к узлу (sticky sessions) позволяет временно жить с состоянием в памяти, но ухудшает равномерность, а при отказе узла состояние теряется. В YARP она выключена по умолчанию.

## Лучшие практики

- Readiness-проверка отвечает «могу принимать запросы», а не «все зависимости идеальны».
- При выключении сначала перестать получать новые запросы, затем дождаться активных (connection draining).
- Держите запас мощности на отказ узла: N+1 или N+2.
- Не используйте affinity вместо переноса состояния.

## Частые ошибки

- ❌ Health check, падающий из-за общей базы → все узлы разом выбывают. ✅ Разделяйте проверки.
- ❌ Повторы на каждом уровне → лавина. ✅ Повторы в одном месте, с ограничением.
- ❌ Autoscaling только по CPU для сервиса, ждущего I/O → ✅ метрика, отражающая очередь работы.
- ❌ Долгие соединения и L4 → перекос после добавления узлов. ✅ L7 или ограничение времени жизни соединения.

## Сравнение с другими треками

- Балансировка внутри кластера — [Kubernetes: Services](../../../kubernetes/modules/06-services/overview.ru.md).
- Партиции как другой способ делить нагрузку — [Kafka: основы](../../../kafka/modules/01-kafka-basics/overview.ru.md).
- Устойчивость исходящих вызовов — [устойчивый HttpClient](../../../csharp/recipes/http-client-resilience/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [YARP: load balancing](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/load-balancing)
- [YARP: health checks](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/dests-health-checks)
- [YARP: session affinity](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/session-affinity)
