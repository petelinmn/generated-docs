---
title: "Mapperly: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: mapperly
part: 2
tags: [mapperly, enum-mapping, queryable-projection, nullable, ef-core]
versions: ".NET 11, C# 15; Riok.Mapperly 4.3.1"
status: draft
updated: 2026-11-22
---

# Mapperly: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## Enums

For enums Mapperly maps **by value** by default:

```csharp
public partial StatusText StatusToText(Status s);
// generated: return (StatusText)s;
```

Verified: it was generated as the cast `(StatusText)s`. If the order/numbers of the values in two enums differ, the result will be wrong with no error. To map by name:

```csharp
[MapEnum(EnumMappingStrategy.ByName)]
public partial StatusText StatusToText(Status s);
```

> ⚠️ Verify: the attribute `[MapEnum(EnumMappingStrategy.ByName)]` and the options (`ByValueCheckDefined`, `IgnoreCase`) were not run in this material — check the documentation.

## Projections for `IQueryable`

```csharp
public partial IQueryable<OrderDto> Project(IQueryable<Order> q);
```

The generator builds a `Select` expression with an object initializer — the provider (EF Core) translates it to SQL, selecting only the needed columns:

```csharp
q => q.Select(x => new OrderDto { Id = x.Id, CustomerName = x.Customer.Name, Status = x.Status, Total = x.Total })
```

Verified: the projection works (`Customer.Name` → `CustomerName`), and the nested-path settings were taken from the existing method `ToDto`. **A gotcha:** hanging `[MapProperty]`/`[MapperIgnore…]` directly on the projection method gives the warning `RMG065` ("Cannot configure an object mapping on a queryable projection mapping, apply the configurations to an object mapping method instead") — configure the object method, and the projection uses it.

> ⚠️ Verify: translating the projection to SQL through EF Core was not run in this material (only working over an in-memory `IQueryable` was verified).

## Reverse mapping and `null`

```csharp
public partial Order FromDto(OrderDto d);                 // reverse mapping needs its own method
public partial OrderDto? ToDtoOrNull(Order? o);           // nullable source -> null result
```

Verified: a `null` source returns `null`. The reverse direction is a separate method with its own ignores (`CustomerName` and others are marked `MapperIgnoreSource`/`MapperIgnoreTarget`).

## Custom conversions

A method in the mapper with a suitable signature (from `A` to `B`) is used by Mapperly automatically for all properties of those types. This is how non-standard rules (formats, value objects) are plugged in.

> ⚠️ Verify: an example of a custom conversion (`[UserMapping]`, `[MapProperty(Use = …)]`) was not run in this material.

## Mapper configuration

`[Mapper]` accepts settings: the enum strategy, `PropertyNameMappingStrategy` (name case), `UseDeepCloning`, `RequiredMappingStrategy`. The "severity" of diagnostics can be raised to errors in `.editorconfig` — for example turn `RMG020` into a build error.

> ⚠️ Verify: the listed `[Mapper]` attribute options and setting severity through `.editorconfig` were not run in this material.

## Dependency injection

A mapper is an ordinary stateless class: register it as a singleton (`AddSingleton<OrderMapper>()`) or create it statically.

## Best practices

| Rule | Why |
|---|---|
| A mapper without logic and side effects | predictability |
| Warnings as errors | do not forget fields |
| Projections for reading from the DB | only the needed columns are selected |
| enum — an explicit strategy | by value silently breaks |
| Separate methods there/back | different rules and ignores |
| The generated-code folder outside compilation | no CS0757 |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — translating projections.
- [Pagination and filtering](../../recipes/pagination-filtering/overview.en.md) — projections in an API.
- [Dapper](../dapper/overview.en.md) — a query result into objects.

## Sources

- [Mapperly: Enum mapping](https://mapperly.riok.app/docs/configuration/enum/)
- [Mapperly: Queryable projections](https://mapperly.riok.app/docs/configuration/queryable-projections/)
- [Mapperly: User implemented mappings](https://mapperly.riok.app/docs/configuration/user-implemented-methods/)
