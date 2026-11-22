---
title: "FluentValidation"
lang: ru
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

🌐 **Русский** · [English](overview.en.md)

## Коротко

FluentValidation — валидация моделей с беглым API. Правила описываются в отдельных классах-валидаторах (`AbstractValidator<T>`), результат — `ValidationResult` со списком ошибок. Поддерживает условия, вложенные объекты, коллекции, async-правила, наборы правил, локализацию и интеграцию с DI. В Minimal API вызывается вручную или через endpoint filter.

## Ключевые идеи

- `RuleFor(...)` + цепочка проверок; `SetValidator` для вложенных, `RuleForEach` для коллекций.
- `Validate` → `ValidationResult`; `ValidateAndThrow` → `ValidationException`.
- Каскад по умолчанию продолжает цепочку; `CascadeMode.Stop` останавливает.
- Async-правила требуют `ValidateAsync`.
- `RuleSet` — разные правила для разных сценариев.
- `Severity.Warning` не делает результат «валидным» (`IsValid = False`).
- Сообщения зависят от культуры; `internal`-валидаторы не регистрируются по умолчанию.

## Минимальный пример

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

## Когда применять, а когда нет

- ✅ Валидация входных DTO API и форм.
- ✅ Сложные условия, async-проверки, разные сценарии.
- ✅ Нужны юнит-тесты правил.
- ❌ Достаточно пары атрибутов Data Annotations.
- ❌ Инварианты, которые должна гарантировать сама доменная модель.
- ❌ Замена ограничений БД (уникальность, внешние ключи).

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | валидатор, правила, результат, сообщения |
| [2. Продвинутый](advanced.ru.md) | каскад, условия, коллекции, async, RuleSet, DI, Minimal API |
| [3. Эксперт](expert.ru.md) | устройство, подвохи, альтернативы, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp fluentvalidation level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp fluentvalidation level 6` |

## Связанные материалы

- Пререквизиты: [05 · Интерфейсы, структуры, records](../../modules/05-interfaces-structs-records/overview.ru.md), [09 · Делегаты, события, лямбды](../../modules/09-delegates-events-lambdas/overview.ru.md)
- Рядом: [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md), [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md), [Mapperly](../mapperly/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — Pydantic
