---
title: "FluentValidation: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: fluentvalidation
part: 2
tags: [fluentvalidation, validation, validators, rules, aspnet-core]
versions: ".NET 11, C# 15; FluentValidation 12.2.0"
status: draft
updated: 2026-11-22
---

# FluentValidation: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Interfaces, structs, records](../../modules/05-interfaces-structs-records/overview.en.md), [09 · Delegates, events, lambdas](../../modules/09-delegates-events-lambdas/overview.en.md). **Time:** ~30 min.

## What it is and why

FluentValidation is a model validation library with a fluent API. Rules are described in a separate validator class rather than in attributes on the model: they are easier to test, combine and maintain. The validation result is an object with a list of errors, not an exception.

```bash
dotnet add package FluentValidation
```

## The first validator

```csharp
record User(string Name, string Email, int Age, Address Address);
record Address(string Zip);

class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator() { RuleFor(a => a.Zip).NotEmpty().Length(5); }
}

class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(u => u.Email).EmailAddress();
        RuleFor(u => u.Age).InclusiveBetween(18, 120);
        RuleFor(u => u.Address).SetValidator(new AddressValidator());
    }
}

var result = new UserValidator().Validate(user);
```

Verified by running:

| Input | Result |
|---|---|
| `User("Ann", "ann@x.com", 30, Address("12345"))` | `IsValid = True` |
| `User("", "nope", 15, Address(""))` | `IsValid = False`, **5 errors** |

The errors (by property): `Name: Name is required`, `Email` (`EmailValidator`), `Age` (`InclusiveBetweenValidator`), `Address.Zip` twice — `NotEmptyValidator` and `ExactLengthValidator`. Each error has a `PropertyName` (a nested property — with a dot), an `ErrorMessage` and an `ErrorCode`.

## The result

```csharp
if (!result.IsValid)
    foreach (var e in result.Errors) Console.WriteLine($"{e.PropertyName}: {e.ErrorMessage}");
```

Or throw: `validator.ValidateAndThrow(user)` → `ValidationException` (verified: it holds the same 5 errors, in `ex.Errors`).

## Commonly used rules

| Rule | What it checks |
|---|---|
| `NotNull`, `NotEmpty` | not `null`; not empty (a string, a collection, a default value) |
| `Length`, `MinimumLength`, `MaximumLength` | string length |
| `EmailAddress`, `Matches` | an e-mail; a regular expression |
| `GreaterThan`, `InclusiveBetween` | comparisons, a range |
| `Must(predicate)` | an arbitrary condition |
| `SetValidator` | a nested object |
| `RuleForEach` | each element of a collection |

## Messages and language

`WithMessage("Name is required")` sets the text. The standard messages depend on the culture: verified that with the Russian culture the message is `'Email' неверный email адрес.`, with `en-US` — `'Age' must be greater than '18'.` ([advanced](advanced.en.md#localization-and-names)).

## Beginner mistakes

- ❌ Putting all validation in a controller/endpoint → ✅ a separate validator that is easy to test.
- ❌ Calling `Validate` and not looking at `IsValid` → ✅ checking the result is mandatory.
- ❌ Trying to validate through the model's constructor → ✅ a validator separate from the model.
- ❌ Expecting localized messages "in English by default" → ✅ the language comes from the culture.
- ❌ Validating a nested object without `SetValidator` → ✅ nested rules are specified explicitly.

## Related materials

- Next: [advanced](advanced.en.md) — conditions, collections, async, RuleSet, DI.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — validating endpoint input.
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md) — testing validators.
- [Python](../../../python/overview.en.md) — Pydantic.

## Sources

- [FluentValidation: Getting started](https://docs.fluentvalidation.net/en/latest/start.html)
- [FluentValidation: Built-in validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
- [FluentValidation: Validators and rules](https://docs.fluentvalidation.net/en/latest/including-rules.html)
