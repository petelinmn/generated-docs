---
title: "Exercises: Mapperly"
lang: en
track: csharp
kind: library
doc: exercises
slug: mapperly
part: 2
tags: [mapperly, object-mapping, enum-mapping, queryable-projection]
status: draft
updated: 2026-11-22
---

# Exercises: Mapperly

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running (Riok.Mapperly 4.3.1). There are no runnable versions in `code/` (level 5 does not exist). The model: `Order { Id, Customer { Name, Email }, Status, Total, Secret, Created }` → `OrderDto { Id, CustomerName, Status, Total, Label, Extra }`.

## 1. 🟢 The first mapper

**Task:** map `Order` to `OrderDto` (`Customer.Name → CustomerName`), ignoring `Label`, `Extra` (target) and `Secret`, `Created` (source).

**Expected result:** `Id=1 CustomerName=Ann Status=Paid Total=9,5`; a build without warnings.

<details><summary>Hint</summary>

`[Mapper] partial class`, `[MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]`, `[MapperIgnoreTarget]`, `[MapperIgnoreSource]`.

</details>

<details><summary>Solution</summary>

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

Verified: `dto: Id=1 CustomerName=Ann Status=Paid Total=9,5 Label= Extra=null`.

</details>

## 2. 🟡 A list and an update

**Task:** add list mapping and a method that updates an existing `OrderDto`. Make sure an unmapped target property is not overwritten.

**Expected result:** `list count: 2`; after `Update` — `Id=1 Label=keep?`.

<details><summary>Hint</summary>

`public partial List<OrderDto> ToDtos(IEnumerable<Order> orders);` and `public partial void Update(Order o, OrderDto target);` with the same ignore attributes.

</details>

<details><summary>Solution</summary>

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

Verified: `list count: 2`; `update: Id=1 Label=keep?` (the original `Id=99` was overwritten, `Label` kept).

</details>

## 3. 🟡 A projection and `null`

**Task:** (a) a method `IQueryable<OrderDto> Project(IQueryable<Order> q)`; (b) `OrderDto? ToDtoOrNull(Order? o)`.

**Expected result:** `projection: Ann`; for `null` — `null`. There must be no `RMG065` warning.

<details><summary>Hint</summary>

Do not hang `MapProperty` on the projection method: it uses the existing object method.

</details>

<details><summary>Solution</summary>

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

Verified: `projection: Ann`, `null source -> null`; generated `Queryable.Select(q, x => new OrderDto { Id = x.Id, CustomerName = x.Customer.Name, … })`.

</details>

## 4. 🔴 An enum and the generated code

**Task:** map `Status` to `StatusText` and look at how the method is generated. Then configure the output of generated files so that the project builds.

**Expected result:** `return (StatusText)s;` is generated; with the files emitted and the folder not excluded — CS0757.

<details><summary>Hint</summary>

In `.csproj`: `EmitCompilerGeneratedFiles`, `CompilerGeneratedFilesOutputPath`, `<Compile Remove="gen/**" />`.

</details>

<details><summary>Solution</summary>

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

Verified: `enum: Shipped`; without `Compile Remove` — seven CS0757 errors (a duplicate implementation).

</details>
