---
title: "Упражнения: Mapperly"
lang: ru
track: csharp
kind: library
doc: exercises
slug: mapperly
part: 2
tags: [mapperly, object-mapping, enum-mapping, queryable-projection]
status: draft
updated: 2026-11-22
---

# Упражнения: Mapperly

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском (Riok.Mapperly 4.3.1). Запускаемых версий в `code/` нет (уровня 5 нет). Модель: `Order { Id, Customer { Name, Email }, Status, Total, Secret, Created }` → `OrderDto { Id, CustomerName, Status, Total, Label, Extra }`.

## 1. 🟢 Первый маппер

**Задача:** сопоставьте `Order` с `OrderDto` (`Customer.Name → CustomerName`), игнорируя `Label`, `Extra` (цель) и `Secret`, `Created` (источник).

**Ожидаемый результат:** `Id=1 CustomerName=Ann Status=Paid Total=9,5`; сборка без предупреждений.

<details><summary>Подсказка</summary>

`[Mapper] partial class`, `[MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]`, `[MapperIgnoreTarget]`, `[MapperIgnoreSource]`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: a basic mapper with explicit ignores.
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
```

Проверено: `dto: Id=1 CustomerName=Ann Status=Paid Total=9,5 Label= Extra=null`.

</details>

## 2. 🟡 Список и обновление

**Задача:** добавьте маппинг списка и метод обновления существующего `OrderDto`. Убедитесь, что несопоставленное свойство цели не затирается.

**Ожидаемый результат:** `list count: 2`; после `Update` — `Id=1 Label=keep?`.

<details><summary>Подсказка</summary>

`public partial List<OrderDto> ToDtos(IEnumerable<Order> orders);` и `public partial void Update(Order o, OrderDto target);` с теми же атрибутами игнорирования.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: collection mapping and in-place update.
public partial List<OrderDto> ToDtos(IEnumerable<Order> orders);

[MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
[MapperIgnoreTarget(nameof(OrderDto.Label))]
[MapperIgnoreTarget(nameof(OrderDto.Extra))]
[MapperIgnoreSource(nameof(Order.Secret))]
[MapperIgnoreSource(nameof(Order.Created))]
public partial void Update(Order o, OrderDto target);
```

Проверено: `list count: 2`; `update: Id=1 Label=keep?` (исходное `Id=99` перезаписано, `Label` сохранён).

</details>

## 3. 🟡 Проекция и `null`

**Задача:** (а) метод `IQueryable<OrderDto> Project(IQueryable<Order> q)`; (б) `OrderDto? ToDtoOrNull(Order? o)`.

**Ожидаемый результат:** `projection: Ann`; для `null` — `null`. Предупреждения `RMG065` быть не должно.

<details><summary>Подсказка</summary>

Не навешивайте `MapProperty` на метод-проекцию: он использует существующий объектный метод.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: queryable projection (no attributes on it) and a nullable mapping.
public partial IQueryable<OrderDto> Project(IQueryable<Order> q);

[MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
[MapperIgnoreTarget(nameof(OrderDto.Label))]
[MapperIgnoreTarget(nameof(OrderDto.Extra))]
[MapperIgnoreSource(nameof(Order.Secret))]
[MapperIgnoreSource(nameof(Order.Created))]
public partial OrderDto? ToDtoOrNull(Order? o);
```

Проверено: `projection: Ann`, `null source -> null`; сгенерирована `Queryable.Select(q, x => new OrderDto { Id = x.Id, CustomerName = x.Customer.Name, … })`.

</details>

## 4. 🔴 Enum и сгенерированный код

**Задача:** сопоставьте `Status` с `StatusText` и посмотрите, как сгенерирован метод. Затем настройте вывод сгенерированных файлов так, чтобы проект собирался.

**Ожидаемый результат:** сгенерировано `return (StatusText)s;`; при выводе файлов без исключения папки — CS0757.

<details><summary>Подсказка</summary>

В `.csproj`: `EmitCompilerGeneratedFiles`, `CompilerGeneratedFilesOutputPath`, `<Compile Remove="gen/**" />`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: enum mapping (by value by default).
public partial StatusText StatusToText(Status s);   // generated: return (StatusText)s;
```

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>gen</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
<ItemGroup><Compile Remove="gen/**" /></ItemGroup>
```

Проверено: `enum: Shipped`; без `Compile Remove` — семь ошибок CS0757 (дубликат реализации).

</details>
