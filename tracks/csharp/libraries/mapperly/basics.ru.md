---
title: "Mapperly: основы"
lang: ru
track: csharp
kind: library
doc: basics
slug: mapperly
part: 2
tags: [mapperly, object-mapping, source-generator, dto]
versions: ".NET 11, C# 15; Riok.Mapperly 4.3.1"
status: draft
updated: 2026-11-22
---

# Mapperly: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · ООП: основы](../../modules/04-oop-basics/overview.ru.md), [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

Mapperly — библиотека маппинга объектов (модель ↔ DTO), реализованная как **source generator**. Вы объявляете `partial`-метод, а генератор на этапе компиляции пишет обычный C#-код присваиваний. В отличие от AutoMapper, нет рефлексии и «магии» во время выполнения: код виден, отлаживается и быстр, а несопоставленные свойства дают предупреждения при сборке.

```xml
<PackageReference Include="Riok.Mapperly" Version="4.3.1" PrivateAssets="all" />
```

## Первый маппер

```csharp
[Mapper]
partial class OrderMapper
{
    [MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
    [MapperIgnoreTarget(nameof(OrderDto.Label))]
    [MapperIgnoreTarget(nameof(OrderDto.Extra))]
    [MapperIgnoreSource(nameof(Order.Secret))]
    [MapperIgnoreSource(nameof(Order.Created))]
    public partial OrderDto ToDto(Order o);
}

var dto = new OrderMapper().ToDto(order);
```

Проверено запуском (модель `Order { Id, Customer { Name, Email }, Status, Total, Secret, Created }` → `OrderDto { Id, CustomerName, Status, Total, Label, Extra }`): результат `Id=1 CustomerName=Ann Status=Paid Total=9,5 Label= Extra=null`.

Сгенерированный код (из папки генерируемых файлов) — обычные присваивания:

```csharp
public partial OrderDto ToDto(Order o)
{
    var target = new OrderDto();
    target.Id = o.Id;
    target.CustomerName = o.Customer.Name;
    target.Status = o.Status;
    target.Total = o.Total;
    return target;
}
```

Совпадающие по имени свойства сопоставляются сами; `Customer.Name → CustomerName` задан `[MapProperty]`; ненужные члены помечены `[MapperIgnoreTarget]` / `[MapperIgnoreSource]`.

## Коллекции и обновление объекта

```csharp
public partial List<OrderDto> ToDtos(IEnumerable<Order> orders);          // list count: 2
public partial void Update(Order o, OrderDto target);                      // updates an existing object
```

Проверено: `ToDtos` вернул 2 элемента (генератор вызывает `ToDto` для каждого); `Update` записал в существующий `target` значения (`Id=1`), а непомеченное в маппинге свойство `Label` осталось прежним (`keep?`).

## Предупреждения вместо сюрпризов

Если в источнике есть член, которого нет в приёмнике, при сборке будет предупреждение `RMG020` («The member Secret on the mapping source type Order is not mapped to any member on the mapping target type OrderDto»). Так вы замечаете забытые поля: явно пометьте их `MapperIgnoreSource`/`MapperIgnoreTarget`. Проверено: до добавления атрибутов предупреждения были по `Secret` и `Created`.

## Частые ошибки новичка

- ❌ Не сделать класс `partial` → ✅ генератору нужен `partial`.
- ❌ Игнорировать предупреждения `RMG020` → ✅ они показывают забытые поля.
- ❌ Включить `EmitCompilerGeneratedFiles` и не исключить папку из компиляции → ✅ ошибка CS0757 (дубликат реализации): добавьте `<Compile Remove="gen/**" />` (проверено).
- ❌ Ожидать, что enum сопоставляются по именам → ✅ по умолчанию — по значению ([продвинутый](advanced.ru.md#перечисления)).
- ❌ Класть бизнес-логику в маппер → ✅ маппер только отображает данные.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — enum, проекции, null, собственные преобразования.
- [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md) — как работают генераторы.
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — проекции `IQueryable`.
- [FluentValidation](../fluentvalidation/overview.ru.md) — валидация перед маппингом.
- [Python](../../../python/overview.ru.md) — Pydantic.

## Источники

- [Mapperly: документация](https://mapperly.riok.app/docs/intro/)
- [Mapperly: Object mappings](https://mapperly.riok.app/docs/configuration/mapper/)
- [Mapperly: Analyzer diagnostics](https://mapperly.riok.app/docs/configuration/analyzer-diagnostics/)
