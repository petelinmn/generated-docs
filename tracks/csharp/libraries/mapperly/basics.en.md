---
title: "Mapperly: basics"
lang: en
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

# Mapperly: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · OOP basics](../../modules/04-oop-basics/overview.en.md), [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md). **Time:** ~30 min.

## What it is and why

Mapperly is an object mapping library (model ↔ DTO) implemented as a **source generator**. You declare a `partial` method and the generator writes plain C# assignment code at compile time. Unlike AutoMapper there is no reflection or runtime "magic": the code is visible, debuggable and fast, and unmapped properties produce warnings at build time.

```xml
<PackageReference Include="Riok.Mapperly" Version="4.3.1" PrivateAssets="all" />
```

## The first mapper

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

Verified by running (the model `Order { Id, Customer { Name, Email }, Status, Total, Secret, Created }` → `OrderDto { Id, CustomerName, Status, Total, Label, Extra }`): the result is `Id=1 CustomerName=Ann Status=Paid Total=9,5 Label= Extra=null`.

The generated code (from the generated-files folder) is plain assignments:

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

Properties with matching names are mapped by themselves; `Customer.Name → CustomerName` is set by `[MapProperty]`; unneeded members are marked `[MapperIgnoreTarget]` / `[MapperIgnoreSource]`.

## Collections and updating an object

```csharp
public partial List<OrderDto> ToDtos(IEnumerable<Order> orders);          // list count: 2
public partial void Update(Order o, OrderDto target);                      // updates an existing object
```

Verified: `ToDtos` returned 2 elements (the generator calls `ToDto` for each); `Update` wrote the values into the existing `target` (`Id=1`), while the property `Label`, not covered by the mapping, kept its value (`keep?`).

## Warnings instead of surprises

If the source has a member that the target lacks, the build emits the warning `RMG020` ("The member Secret on the mapping source type Order is not mapped to any member on the mapping target type OrderDto"). This is how you notice forgotten fields: mark them explicitly with `MapperIgnoreSource`/`MapperIgnoreTarget`. Verified: before adding the attributes there were warnings for `Secret` and `Created`.

## Beginner mistakes

- ❌ Not making the class `partial` → ✅ the generator needs `partial`.
- ❌ Ignoring `RMG020` warnings → ✅ they show forgotten fields.
- ❌ Turning on `EmitCompilerGeneratedFiles` and not excluding the folder from compilation → ✅ error CS0757 (a duplicate implementation): add `<Compile Remove="gen/**" />` (verified).
- ❌ Expecting enums to be mapped by name → ✅ by default — by value ([advanced](advanced.en.md#enums)).
- ❌ Putting business logic into a mapper → ✅ a mapper only maps data.

## Related materials

- Next: [advanced](advanced.en.md) — enums, projections, null, custom conversions.
- [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md) — how generators work.
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — `IQueryable` projections.
- [FluentValidation](../fluentvalidation/overview.en.md) — validation before mapping.
- [Python](../../../python/overview.en.md) — Pydantic.

## Sources

- [Mapperly: documentation](https://mapperly.riok.app/docs/intro/)
- [Mapperly: Object mappings](https://mapperly.riok.app/docs/configuration/mapper/)
- [Mapperly: Analyzer diagnostics](https://mapperly.riok.app/docs/configuration/analyzer-diagnostics/)
