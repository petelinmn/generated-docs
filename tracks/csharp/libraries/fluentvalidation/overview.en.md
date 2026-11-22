---
title: "FluentValidation"
lang: en
track: csharp
kind: library
doc: overview
slug: fluentvalidation
part: 2
levels: [1, 2, 3, 4]
tags: [fluentvalidation, validation, validators, aspnet-core]
versions: ".NET 11, C# 15; FluentValidation 12.2.0"
status: draft
updated: 2026-11-22
---

# FluentValidation

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

FluentValidation is model validation with a fluent API. Rules are described in separate validator classes (`AbstractValidator<T>`), the result is a `ValidationResult` with a list of errors. It supports conditions, nested objects, collections, async rules, rule sets, localization and DI integration. In Minimal API it is called by hand or through an endpoint filter.

## Key ideas

- `RuleFor(...)` + a chain of checks; `SetValidator` for nested objects, `RuleForEach` for collections.
- `Validate` → `ValidationResult`; `ValidateAndThrow` → `ValidationException`.
- The default cascade continues the chain; `CascadeMode.Stop` stops it.
- Async rules need `ValidateAsync`.
- `RuleSet` — different rules for different scenarios.
- `Severity.Warning` does not make a result "valid" (`IsValid = False`).
- Messages depend on the culture; `internal` validators are not registered by default.

## Minimal example

```csharp
class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Name).NotEmpty();
        RuleFor(u => u.Age).InclusiveBetween(18, 120);
    }
}
var result = new UserValidator().Validate(user);
```

## When to use and when not

- ✅ Validating API input DTOs and forms.
- ✅ Complex conditions, async checks, different scenarios.
- ✅ You need unit tests of the rules.
- ❌ A couple of Data Annotations attributes are enough.
- ❌ Invariants the domain model itself must guarantee.
- ❌ A replacement for DB constraints (uniqueness, foreign keys).

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | a validator, rules, the result, messages |
| [2. Advanced](advanced.en.md) | cascade, conditions, collections, async, RuleSet, DI, Minimal API |
| [3. Expert](expert.en.md) | internals, gotchas, alternatives, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp fluentvalidation level 5` |
| 6. Tests | not generated yet — `/learn csharp fluentvalidation level 6` |

## Related materials

- Prerequisites: [05 · Interfaces, structs, records](../../modules/05-interfaces-structs-records/overview.en.md), [09 · Delegates, events, lambdas](../../modules/09-delegates-events-lambdas/overview.en.md)
- Nearby: [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md), [16 · Unit testing](../../modules/16-testing-unit/overview.en.md), [Mapperly](../mapperly/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — Pydantic
