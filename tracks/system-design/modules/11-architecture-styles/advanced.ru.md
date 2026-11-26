---
title: "Архитектурные стили: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 11-architecture-styles
part: 2
tags: [system-design, architecture, modular-monolith, microservices, fitness-function, adr]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Архитектурные стили: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Где проводить границы

Граница хороша, если типичное изменение остаётся внутри неё. Ориентиры: бизнес-возможности, bounded context, разные темпы изменений и требования к нагрузке.

## Связанность

Неустойчивость модуля `I = Ce / (Ca + Ce)`, где `Ca` — входящие зависимости, `Ce` — исходящие. Модуль, от которого зависят многие, должен меняться редко. Циклы зависимостей разрывают контрактом или событием.

## Проверка правил тестом

```csharp
public static IEnumerable<string> Violations(Assembly assembly, string module, string forbidden) =>
    from type in assembly.GetTypes()
    where type.Namespace == module
    from used in SignatureTypes(type)
    where used.Namespace == forbidden
    select $"{type.Name} -> {used.Name}";
```

Такой тест (fitness function) падает, когда `Orders` начинает зависеть от `Billing`. Файл: [Ex02_BoundaryRules.cs](code/Examples/Ex02_BoundaryRules.cs). Он смотрит только на сигнатуры, но не в тела методов.

## Постепенная миграция

Strangler fig: перед старой системой ставят фасад (например, [YARP](../02-scalability-and-load-balancing/basics.ru.md)) и по одному переводят пути на новые реализации. Переписывать всё разом рискованно.

## ADR

Фиксируйте контекст, решение, альтернативы и последствия. Пересмотр — новая запись, отменяющая старую.

## Лучшие практики

- Начинайте с модулей, выносите сервис при доказанной нужде.
- Один модуль — одни данные.
- Контракты версионируйте и тестируйте.
- До разделения обеспечьте трассировку и автоматическое развёртывание.

## Частые ошибки

- ❌ Сервис на каждую таблицу → ✅ по возможности бизнеса.
- ❌ Длинные синхронные цепочки → ✅ меньше прыжков, события.
- ❌ Общая библиотека с доменной логикой → ✅ дублирование контрактов дешевле связанности.
- ❌ Архитектура только на диаграмме → ✅ проверки в CI.

## Сравнение с другими треками

- Слои, vertical slices, CQRS внутри приложения — [C#](../../../csharp/modules/21-architecture/overview.ru.md).
- Независимое развёртывание — [Kubernetes](../../../kubernetes/modules/05-deployments/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [Azure: архитектурные стили](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/)
