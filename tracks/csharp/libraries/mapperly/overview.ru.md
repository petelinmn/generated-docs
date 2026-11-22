---
title: "Mapperly"
lang: ru
track: csharp
kind: library
doc: overview
slug: mapperly
part: 2
levels: [1, 2, 3, 4]
tags: [mapperly, object-mapping, source-generator, dto]
versions: ".NET 11, C# 15; Riok.Mapperly 4.3.1"
status: draft
updated: 2026-11-22
---

# Mapperly

🌐 **Русский** · [English](overview.en.md)

## Коротко

Mapperly — маппинг объектов через source generator: вы объявляете `partial`-метод, а при компиляции генерируется обычный C#-код присваиваний. Нет рефлексии (совместимо с Native AOT), код виден и отлаживается, а забытые поля дают предупреждения (`RMG020`). Поддерживаются коллекции, вложенные пути, обновление объектов, проекции `IQueryable` и enum (по значению — по умолчанию).

## Ключевые идеи

- `[Mapper] partial class` + `partial`-методы.
- Одноимённые свойства — автоматически; `[MapProperty]` для остального; `[MapperIgnoreTarget]`/`[MapperIgnoreSource]` для игнорирования.
- `RMG020` — предупреждение о неотображённом члене.
- `Update(source, target)` — обновление существующего объекта.
- `IQueryable`-проекция → выражение `Select` для EF Core.
- enum по значению по умолчанию — осторожно.
- Генерируемые файлы выводят с `<Compile Remove=…>`.

## Минимальный пример

```csharp
[Mapper]
partial class OrderMapper
{
    [MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
    public partial OrderDto ToDto(Order o);
}
```

## Когда применять, а когда нет

- ✅ Много DTO ↔ модель с простыми правилами.
- ✅ Нужны AOT, скорость, предсказуемость.
- ✅ Хотите видеть ошибки маппинга при сборке.
- ❌ Преобразования с богатой логикой, зависящей от контекста — удобнее вручную.
- ❌ Нужны динамические конвенции, меняющиеся в рантайме.
- ❌ Один-два тривиальных DTO — хватит конструктора.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | первый маппер, коллекции, обновление, предупреждения |
| [2. Продвинутый](advanced.ru.md) | enum, проекции, `null`, конфигурация |
| [3. Эксперт](expert.ru.md) | устройство генератора, подвохи, альтернативы, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp mapperly level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp mapperly level 6` |

## Связанные материалы

- Пререквизиты: [04 · ООП: основы](../../modules/04-oop-basics/overview.ru.md), [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md)
- Рядом: [15 · EF Core](../../modules/15-ef-core/overview.ru.md), [FluentValidation](../fluentvalidation/overview.ru.md), [Dapper](../dapper/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — Pydantic
