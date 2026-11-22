---
title: "Exercises: FluentValidation"
lang: en
track: csharp
kind: library
doc: exercises
slug: fluentvalidation
part: 2
tags: [fluentvalidation, validation, async-validation, rulesets, dependency-injection]
status: draft
updated: 2026-11-22
---

# Exercises: FluentValidation

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running (FluentValidation 12.2.0 and FluentValidation.DependencyInjectionExtensions 12.2.0). There are no runnable versions in `code/` (level 5 does not exist). The model: `record User(string Name, string Email, int Age, Address Address)`, `record Address(string Zip)`.

## 1. 🟢 The first validator

**Task:** `Name` is not empty, `Email` is an address, `Age` from 18 to 120, `Address.Zip` is not empty and has length 5. Check a valid and an invalid user.

**Expected result:** the valid one — `IsValid = True`; the invalid one (`"", "nope", 15, Address("")`) — 5 errors.

<details><summary>Hint</summary>

`AbstractValidator<User>`, `NotEmpty`, `EmailAddress`, `InclusiveBetween`, `SetValidator(new AddressValidator())`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: user validator with a nested address validator.
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
```

Verified: `valid: True`; `invalid: False, errors=5` (`Name`, `Email`, `Age`, `Address.Zip` ×2).

</details>

## 2. 🟡 A condition and a cascade

**Task:** (a) age ≥ 18 only for e-mails on `@corp.com`; (b) the name `root` is forbidden; (c) for an empty name a single error with `NotEmpty().MinimumLength(5)`.

**Expected result:** for `root / a@corp.com / 10` — two errors; for `a@other.com` — age is not checked; for an empty name — 1 error with `CascadeMode.Stop`.

<details><summary>Hint</summary>

`.When(...)`, `.Must(...)`, `.Cascade(CascadeMode.Stop)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: conditional rule, custom predicate, cascade stop.
cond.RuleFor(u => u.Age).GreaterThanOrEqualTo(18)
    .When(u => u.Email.EndsWith("@corp.com"))
    .WithMessage("Corp users must be adults, got {PropertyValue}");
cond.RuleFor(u => u.Name).Must(n => n != "root").WithMessage("'root' is reserved");
casc.RuleFor(u => u.Name).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(5);
```

Verified: `Corp users must be adults, got 10 | 'root' is reserved`; `condition false: valid=True`; `cascade stop errors for empty name: 1` (by default — 2).

</details>

## 3. 🟡 Async and a collection

**Task:** (a) an asynchronous check "the name is not `taken`"; (b) for the list `["a","","c",""]` errors only on the empty elements.

**Expected result:** `name already taken`; errors `Items[1]`, `Items[3]`. A synchronous `Validate` for an async validator — an exception.

<details><summary>Hint</summary>

`MustAsync` + `ValidateAsync`; `RuleForEach(o => o.Items).NotEmpty()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: async rule and per-element collection rule.
RuleFor(u => u.Name).MustAsync(async (n, ct) => { await Task.Delay(1, ct); return n != "taken"; })
                    .WithMessage("name already taken");
RuleForEach(o => o.Items).NotEmpty().WithMessage("item must not be empty");
```

Verified: `async: name already taken`; `collection: Items[1] | Items[3]`; `sync Validate with async rule: AsyncValidatorInvokedSynchronouslyException`.

</details>

## 4. 🔴 DI and rule sets

**Task:** register validators through DI (the validator is `internal`!) and split the rules into `Create` and the rest.

**Expected result:** without `includeInternalTypes: true` `IValidator<User>` is not found; `Validate(user)` — 0 errors, `IncludeRuleSets("Create")` — 1 error.

<details><summary>Hint</summary>

`AddValidatorsFromAssemblyContaining<T>(includeInternalTypes: true)`; `RuleSet("Create", () => …)`; `Validate(user, o => o.IncludeRuleSets("Create"))`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: DI registration of internal validators and rule sets.
sc.AddValidatorsFromAssemblyContaining<UserValidator>(includeInternalTypes: true);

class RuleSetValidator : AbstractValidator<User>
{
    public RuleSetValidator()
    {
        RuleSet("Create", () => RuleFor(u => u.Name).NotEmpty());
        RuleFor(u => u.Age).GreaterThan(0);
    }
}
```

Verified: `internal validator registered by default: False`; `resolved: UserValidator, valid=True`; `default ruleset errors: 0`, `Create ruleset errors: 1`.

</details>
