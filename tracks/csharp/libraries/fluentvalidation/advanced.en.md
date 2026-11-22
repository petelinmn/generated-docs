---
title: "FluentValidation: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: fluentvalidation
part: 2
tags: [fluentvalidation, cascade, rulesets, async-validation, dependency-injection, localization]
versions: ".NET 11, C# 15; FluentValidation 12.2.0, FluentValidation.DependencyInjectionExtensions 12.2.0"
status: draft
updated: 2026-11-22
---

# FluentValidation: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## Rule cascade

By default all rules in a chain run even if the first has already failed:

```csharp
RuleFor(u => u.Name).NotEmpty().MinimumLength(5);                                   // 2 errors for ""
RuleFor(u => u.Name).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(5);         // 1 error
```

Verified: for an empty name — 2 errors by default and 1 with `CascadeMode.Stop`.

## Conditions, `Must`, messages

```csharp
RuleFor(u => u.Age).GreaterThanOrEqualTo(18)
    .When(u => u.Email.EndsWith("@corp.com"))
    .WithMessage("Corp users must be adults, got {PropertyValue}");
RuleFor(u => u.Name).Must(n => n != "root").WithMessage("'root' is reserved");
```

Verified: for `root` / `a@corp.com` / 10 — both errors (`… got 10`, `'root' is reserved`); for `a@other.com` the condition does not apply, no age errors. Placeholders are available in the message (`{PropertyValue}`, `{PropertyName}`).

## Collections

```csharp
RuleForEach(o => o.Items).NotEmpty().WithMessage("item must not be empty");
RuleFor(o => o.Items).NotEmpty();
```

Verified: for `["a", "", "c", ""]` the errors are on `Items[1]` and `Items[3]` — the index is part of the property name.

## Asynchronous rules

```csharp
RuleFor(u => u.Name).MustAsync(async (n, ct) => { await Task.Delay(1, ct); return n != "taken"; })
                    .WithMessage("name already taken");
var r = await validator.ValidateAsync(user);
```

Verified: `ValidateAsync` returned `name already taken`. **A synchronous `Validate` on a validator with an async rule throws `AsyncValidatorInvokedSynchronouslyException`.** Use `ValidateAsync` everywhere there are async rules (for example a uniqueness check against the DB).

## Rule sets (RuleSet)

```csharp
RuleSet("Create", () => RuleFor(u => u.Name).NotEmpty());
RuleFor(u => u.Age).GreaterThan(0);
```

Verified: `Validate(user)` runs only the rules outside sets (0 errors), `Validate(user, o => o.IncludeRuleSets("Create"))` — the `Create` set (1 error; a rule outside the set does not run then unless you add `IncludeRulesNotInRuleSet()`).

## Severity

`WithSeverity(Severity.Warning)` turns an error into a warning. A gotcha: verified — a result with one warning has `IsValid = False` (a warning is also an entry in `Errors`). For "success" you must check `Severity` separately.

## Localization and names

Standard messages come from the culture: `ValidatorOptions.Global.LanguageManager.Culture`. Verified: `ru-RU` → `'Age' должно быть больше '18'.`, `en-US` → `'Age' must be greater than '18'.`. `WithName("Возраст")` changes the property name in the message (verified).

## Dependency injection

```csharp
services.AddValidatorsFromAssemblyContaining<UserValidator>();
var v = sp.GetRequiredService<IValidator<User>>();
```

The `FluentValidation.DependencyInjectionExtensions` package registers all validators of an assembly (by default with the `Scoped` lifetime). **A gotcha:** verified — an `internal` validator is **not registered** by default (`GetRequiredService` fails with `No service for type IValidator<User>`); make the class `public` or pass `includeInternalTypes: true`.

> ⚠️ Verify: the default lifetime (`Scoped`) was not verified by running in this material; check the documentation.

## Integration with Minimal API

There is no automatic validation (as in MVC) in Minimal API: call the validator by hand or through an endpoint filter.

```csharp
var res = await validator.ValidateAsync(user);
if (!res.IsValid) return Results.ValidationProblem(res.ToDictionary());
```

Verified: `ToDictionary()` gives a "property → messages" dictionary (`Name=… ; Age=…`) that `Results.ValidationProblem` accepts.

> ⚠️ Verify: the `Results.ValidationProblem` call itself and the `400` response with `ProblemDetails` were not run in this material.

## Best practices

| Rule | Why |
|---|---|
| A validator is a separate class registered in DI | testability |
| `ValidateAsync` with any async rules | otherwise an exception |
| `CascadeMode.Stop` for expensive rules | do not run the extra |
| Error codes (`WithErrorCode`) for clients | a stable contract |
| No business logic in a validator | a validator is about the input's shape |
| Test validators with unit tests | fast and precise |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/advanced.en.md#validation-and-problemdetails) — `ProblemDetails`.
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — registration.
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md)

## Sources

- [FluentValidation: Cascade mode](https://docs.fluentvalidation.net/en/latest/cascade.html)
- [FluentValidation: Asynchronous validation](https://docs.fluentvalidation.net/en/latest/async.html)
- [FluentValidation: Dependency injection](https://docs.fluentvalidation.net/en/latest/di.html)
- [FluentValidation: Localization](https://docs.fluentvalidation.net/en/latest/localization.html)
