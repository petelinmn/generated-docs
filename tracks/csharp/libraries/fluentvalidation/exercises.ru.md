---
title: "Упражнения: FluentValidation"
lang: ru
track: csharp
kind: library
doc: exercises
slug: fluentvalidation
part: 2
tags: [fluentvalidation, validation, async-validation, rulesets, dependency-injection]
status: draft
updated: 2026-11-22
---

# Упражнения: FluentValidation

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском (FluentValidation 12.2.0 и FluentValidation.DependencyInjectionExtensions 12.2.0). Запускаемых версий в `code/` нет (уровня 5 нет). Модель: `record User(string Name, string Email, int Age, Address Address)`, `record Address(string Zip)`.

## 1. 🟢 Первый валидатор

**Задача:** `Name` не пуст, `Email` — адрес, `Age` от 18 до 120, `Address.Zip` не пуст и длиной 5. Проверьте корректного и некорректного пользователя.

**Ожидаемый результат:** корректный — `IsValid = True`; некорректный (`"", "nope", 15, Address("")`) — 5 ошибок.

<details><summary>Подсказка</summary>

`AbstractValidator<User>`, `NotEmpty`, `EmailAddress`, `InclusiveBetween`, `SetValidator(new AddressValidator())`.

</details>

<details><summary>Решение</summary>

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

Проверено: `valid: True`; `invalid: False, errors=5` (`Name`, `Email`, `Age`, `Address.Zip` ×2).

</details>

## 2. 🟡 Условие и каскад

**Задача:** (а) возраст ≥ 18 только для e-mail на `@corp.com`; (б) имя `root` запрещено; (в) для пустого имени одна ошибка при `NotEmpty().MinimumLength(5)`.

**Ожидаемый результат:** для `root / a@corp.com / 10` — две ошибки; для `a@other.com` — возраст не проверяется; для пустого имени — 1 ошибка с `CascadeMode.Stop`.

<details><summary>Подсказка</summary>

`.When(...)`, `.Must(...)`, `.Cascade(CascadeMode.Stop)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: conditional rule, custom predicate, cascade stop.
cond.RuleFor(u => u.Age).GreaterThanOrEqualTo(18)
    .When(u => u.Email.EndsWith("@corp.com"))
    .WithMessage("Corp users must be adults, got {PropertyValue}");
cond.RuleFor(u => u.Name).Must(n => n != "root").WithMessage("'root' is reserved");
casc.RuleFor(u => u.Name).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(5);
```

Проверено: `Corp users must be adults, got 10 | 'root' is reserved`; `condition false: valid=True`; `cascade stop errors for empty name: 1` (по умолчанию — 2).

</details>

## 3. 🟡 Async и коллекция

**Задача:** (а) асинхронная проверка «имя не `taken`»; (б) для списка `["a","","c",""]` ошибки только у пустых элементов.

**Ожидаемый результат:** `name already taken`; ошибки `Items[1]`, `Items[3]`. Синхронный `Validate` для async-валидатора — исключение.

<details><summary>Подсказка</summary>

`MustAsync` + `ValidateAsync`; `RuleForEach(o => o.Items).NotEmpty()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: async rule and per-element collection rule.
RuleFor(u => u.Name).MustAsync(async (n, ct) => { await Task.Delay(1, ct); return n != "taken"; })
                    .WithMessage("name already taken");
RuleForEach(o => o.Items).NotEmpty().WithMessage("item must not be empty");
```

Проверено: `async: name already taken`; `collection: Items[1] | Items[3]`; `sync Validate with async rule: AsyncValidatorInvokedSynchronouslyException`.

</details>

## 4. 🔴 DI и наборы правил

**Задача:** зарегистрируйте валидаторы через DI (валидатор `internal`!) и разделите правила на `Create` и остальные.

**Ожидаемый результат:** без `includeInternalTypes: true` `IValidator<User>` не находится; `Validate(user)` — 0 ошибок, `IncludeRuleSets("Create")` — 1 ошибка.

<details><summary>Подсказка</summary>

`AddValidatorsFromAssemblyContaining<T>(includeInternalTypes: true)`; `RuleSet("Create", () => …)`; `Validate(user, o => o.IncludeRuleSets("Create"))`.

</details>

<details><summary>Решение</summary>

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

Проверено: `internal validator registered by default: False`; `resolved: UserValidator, valid=True`; `default ruleset errors: 0`, `Create ruleset errors: 1`.

</details>
