---
title: "Масштабирование и балансировка"
lang: ru
track: system-design
kind: module
doc: overview
slug: 02-scalability-and-load-balancing
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Масштабирование и балансировка

🌐 **Русский** · [English](overview.en.md)

## Коротко

Когда одного сервера не хватает, нагрузку либо отдают машине помощнее, либо делят между несколькими. Во втором случае нужен кто-то, кто выбирает, куда отправить запрос, и сервис, которому это всё равно.

## Ключевые идеи

- Вертикальное масштабирование проще, но имеет потолок; горизонтальное требует stateless-сервиса.
- Балансировщик бывает на уровне соединений (L4) и на уровне HTTP (L7).
- Round robin хорош для одинаковых запросов; при разной длительности помогают алгоритмы, учитывающие нагрузку.
- Power of two choices даёт почти ровное распределение за O(1).
- Consistent hashing при добавлении узла переносит только часть ключей.
- Проверки здоровья убирают сломанные узлы.
- Добавление узлов не даёт линейного роста: мешают очереди и координация.

## Минимальный пример

```csharp
uint i = unchecked((uint)Interlocked.Increment(ref _next));
return nodes[(int)(i % (uint)nodes.Count)];
```

## Когда применять, а когда нет

- ✅ Нужны отказоустойчивость или рост выше возможностей одной машины.
- ✅ Выкатка без простоя.
- ✅ Один вход для нескольких сервисов.
- ❌ Узкое место в одной базе: копии приложения только усилят давление.
- ❌ Состояние в памяти процесса, которое нельзя вынести.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | scale up и scale out, stateless, L4 и L7, round robin, YARP |
| [2. Продвинутая теория](advanced.ru.md) | взвешенные и load-aware алгоритмы, consistent hashing, health checks, affinity |
| [3. Экспертная теория](expert.ru.md) | законы Амдала и USL, справочник YARP, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | балансировщики, hash ring, шлюз на YARP |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [процесс проектирования](../01-design-process/overview.ru.md)
- Дальше: [кэширование](../03-caching/overview.ru.md)
- То же в других треках: [Kubernetes: Services](../../../kubernetes/modules/06-services/overview.ru.md), [Kubernetes: autoscaling](../../../kubernetes/modules/15-autoscaling/overview.ru.md)
- [Репликация и партиционирование](../06-replication-and-partitioning/overview.ru.md)
