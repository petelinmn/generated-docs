---
title: "FluentValidation: основы"
lang: ru
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

# FluentValidation: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Интерфейсы, структуры, records](../../modules/05-interfaces-structs-records/overview.ru.md), [09 · Делегаты, события, лямбды](../../modules/09-delegates-events-lambdas/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

FluentValidation — библиотека валидации моделей с «беглым» (fluent) API. Правила описываются в отдельном классе-валидаторе, а не в атрибутах модели: их проще тестировать, комбинировать и сопровождать. Результат валидации — объект со списком ошибок, а не исключение.

```bash
dotnet add package FluentValidation
```

## Первый валидатор

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

Проверено запуском:

| Ввод | Результат |
|---|---|
| `User("Ann", "ann@x.com", 30, Address("12345"))` | `IsValid = True` |
| `User("", "nope", 15, Address(""))` | `IsValid = False`, **5 ошибок** |

Ошибки (по свойствам): `Name: Name is required`, `Email` (`EmailValidator`), `Age` (`InclusiveBetweenValidator`), `Address.Zip` дважды — `NotEmptyValidator` и `ExactLengthValidator`. У каждой ошибки есть `PropertyName` (вложенное свойство — через точку), `ErrorMessage` и `ErrorCode`.

## Результат

```csharp
if (!result.IsValid)
    foreach (var e in result.Errors) Console.WriteLine($"{e.PropertyName}: {e.ErrorMessage}");
```

Или бросить исключение: `validator.ValidateAndThrow(user)` → `ValidationException` (проверено: в нём те же 5 ошибок, в `ex.Errors`).

## Часто используемые правила

| Правило | Что проверяет |
|---|---|
| `NotNull`, `NotEmpty` | не `null`; не пусто (строка, коллекция, значение по умолчанию) |
| `Length`, `MinimumLength`, `MaximumLength` | длина строки |
| `EmailAddress`, `Matches` | e-mail; регулярное выражение |
| `GreaterThan`, `InclusiveBetween` | сравнения, диапазон |
| `Must(predicate)` | произвольное условие |
| `SetValidator` | вложенный объект |
| `RuleForEach` | каждый элемент коллекции |

## Сообщения и язык

`WithMessage("Name is required")` задаёт текст. Стандартные сообщения зависят от культуры: проверено, что при русской культуре сообщение — `'Email' неверный email адрес.`, при `en-US` — `'Age' must be greater than '18'.` ([продвинутый](advanced.ru.md#локализация-и-имена)).

## Частые ошибки новичка

- ❌ Складывать всю валидацию в контроллер/endpoint → ✅ отдельный валидатор, который легко тестировать.
- ❌ Вызывать `Validate` и не смотреть `IsValid` → ✅ проверка результата обязательна.
- ❌ Пытаться валидировать через конструктор модели → ✅ валидатор отдельно от модели.
- ❌ Ждать локализованных сообщений «по умолчанию на английском» → ✅ язык определяется культурой.
- ❌ Валидировать вложенный объект без `SetValidator` → ✅ вложенные правила задаются явно.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — условия, коллекции, async, RuleSet, DI.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — валидация входа эндпоинтов.
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md) — тесты валидаторов.
- [Python](../../../python/overview.ru.md) — Pydantic.

## Источники

- [FluentValidation: Getting started](https://docs.fluentvalidation.net/en/latest/start.html)
- [FluentValidation: Built-in validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
- [FluentValidation: Validators and rules](https://docs.fluentvalidation.net/en/latest/including-rules.html)
