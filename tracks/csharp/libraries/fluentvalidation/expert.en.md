---
title: "FluentValidation: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: fluentvalidation
part: 2
tags: [fluentvalidation, data-annotations, validation-design, rule-builder, interview-questions]
versions: ".NET 11, C# 15; FluentValidation 12.2.0"
status: draft
updated: 2026-11-22
---

# FluentValidation: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

`RuleFor(x => x.Prop)` builds an `IRuleBuilder` and adds a `PropertyRule` to the validator: the property access expression is compiled, the name is taken from the expression. Each call such as `NotEmpty()` adds a component (`IPropertyValidator`) to the rule — as a result an error has a code like `NotEmptyValidator` (verified). The validator runs the rules in declaration order and collects `ValidationFailure`s into a `ValidationResult`. Validators are thread-safe objects, created once and reused (in DI they are not singletons by default, but rules must not hold request state).

> ⚠️ Verify: the details of expression compilation, caching, registration lifetime values and internal types were not investigated in this material — see the FluentValidation sources.

## Edges and gotchas

**1. A warning makes `IsValid = False`.** Verified: `WithSeverity(Severity.Warning)` — `IsValid=False`, an error with `Severity=Warning`.

**2. Synchronous `Validate` and async rules.** Verified: `AsyncValidatorInvokedSynchronouslyException`.

**3. `internal` validators are not registered.** Verified: by default `AddValidatorsFromAssemblyContaining` skips them.

**4. The message culture.** Verified: Russian/English messages depend on the culture; in tests fix the culture (`ValidatorOptions.Global.LanguageManager.Culture`) or check `ErrorCode`, not the text.

**5. The default cascade is to continue.** Verified: 2 errors for an empty name; for expensive rules `CascadeMode.Stop`.

**6. `RuleSet` and rules outside a set.** Verified: with `IncludeRuleSets("Create")` the result is only the set (1 error); rules outside the set do not run without `IncludeRulesNotInRuleSet`.

**7. Nested names.** `Address.Zip` and `Items[1]` are the property name format (verified); consider it when mapping errors to form fields.

**8. A validator does not replace DB constraints.** A uniqueness check is a race between validation and insert; a unique index is mandatory.

**9. Validation and transformations.** A validator checks the input's shape; do not change data in rules (side effects in `Must`).

**10. One model — different scenarios.** Create and update need different rules: a `RuleSet` or separate validators.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| FluentValidation | expressive rules, tests, conditions, async | a separate dependency and classes |
| Data Annotations | built in, declarative | limited expressiveness, mixed with the model |
| `IValidatableObject` | logic in the model | harder to test and reuse |
| Hand-written checks in code | simple for small things | grow and duplicate |
| Domain-level validation (invariants) | the model is always correct | a different place of responsibility |

## Interview questions

<details><summary>1. How does FluentValidation differ from Data Annotations?</summary>

Rules are described in a separate class with a fluent API: conditions, collections, async, sets, nested validators; the model stays clean.

</details>

<details><summary>2. What does `Validate` return?</summary>

A `ValidationResult` with `IsValid` and a list of `Errors` (`PropertyName`, `ErrorMessage`, `ErrorCode`); an exception only with `ValidateAndThrow` (verified).

</details>

<details><summary>3. How do you validate a nested object and a collection?</summary>

`SetValidator(new ChildValidator())` and `RuleForEach`; error names `Address.Zip`, `Items[1]` (verified).

</details>

<details><summary>4. How do you make a rule conditional?</summary>

`.When(predicate)` / `.Unless(...)`; when the condition is false the rule is skipped (verified).

</details>

<details><summary>5. What does `CascadeMode.Stop` do?</summary>

It stops a property's rule chain at the first error (verified: 1 error instead of 2).

</details>

<details><summary>6. How do you run an asynchronous check (for example uniqueness in the DB)?</summary>

`MustAsync` and `ValidateAsync`; a synchronous `Validate` throws (verified). Plus a unique index in the DB.

</details>

<details><summary>7. How do you register validators in DI?</summary>

`AddValidatorsFromAssemblyContaining<T>()` (the DependencyInjectionExtensions package); `internal` — via `includeInternalTypes: true` (verified).

</details>

<details><summary>8. How do you apply different rules for create and update?</summary>

`RuleSet("Create", …)` and `IncludeRuleSets`, or separate validators (verified).

</details>

<details><summary>9. Trick question: `IsValid == false` although you set only a warning. Why?</summary>

`Severity.Warning` is also an entry in `Errors`; `IsValid` is false (verified). Distinguish by `Severity`.

</details>

<details><summary>10. Trick question: a test on the error text passes locally and fails on CI. Why?</summary>

Messages depend on the culture (verified: Russian/English). Check `ErrorCode` or fix the culture.

</details>

<details><summary>11. Trick question: the uniqueness check in the validator passed, but the DB has a duplicate. Why?</summary>

A race between the check and the insert; you need a unique index and handling of the constraint violation.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — where validation belongs.
- [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md) — expression trees.
- [Mapperly](../mapperly/overview.en.md) — DTO mapping after validation.

## Sources

- [FluentValidation docs](https://docs.fluentvalidation.net/)
- [FluentValidation: Custom validators](https://docs.fluentvalidation.net/en/latest/custom-validators.html)
- [FluentValidation: Rule sets](https://docs.fluentvalidation.net/en/latest/rulesets.html)
