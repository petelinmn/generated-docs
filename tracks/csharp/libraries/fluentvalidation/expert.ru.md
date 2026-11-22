---
title: "FluentValidation: экспертный уровень"
lang: ru
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

# FluentValidation: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md) и [продвинутый](advanced.ru.md). **Время:** ~30 мин.

## Внутреннее устройство

`RuleFor(x => x.Prop)` строит `IRuleBuilder` и добавляет в валидатор `PropertyRule`: компилируется выражение доступа к свойству, имя берётся из выражения. Каждый вызов вроде `NotEmpty()` добавляет в правило компонент (`IPropertyValidator`) — в результате у ошибки код вида `NotEmptyValidator` (проверено). Валидатор выполняет правила по порядку объявления, собирает `ValidationFailure` в `ValidationResult`. Валидаторы — потокобезопасные объекты, создаются один раз и переиспользуются (в DI — по умолчанию не singleton, но правила не должны хранить состояние запроса).

> ⚠️ Проверить: детали компиляции выражений, кэширование, значения времени жизни при регистрации и внутренние типы в этом материале не исследовались — смотрите исходники FluentValidation.

## Грани и подвохи

**1. Предупреждение делает `IsValid = False`.** Проверено: `WithSeverity(Severity.Warning)` — `IsValid=False`, ошибка с `Severity=Warning`.

**2. Синхронный `Validate` и async-правила.** Проверено: `AsyncValidatorInvokedSynchronouslyException`.

**3. `internal`-валидаторы не регистрируются.** Проверено: по умолчанию `AddValidatorsFromAssemblyContaining` пропускает их.

**4. Культура сообщений.** Проверено: русские/английские сообщения зависят от культуры; в тестах фиксируйте культуру (`ValidatorOptions.Global.LanguageManager.Culture`) или проверяйте `ErrorCode`, а не текст.

**5. Каскад по умолчанию — продолжать.** Проверено: 2 ошибки на пустое имя; для дорогих правил `CascadeMode.Stop`.

**6. `RuleSet` и правила вне набора.** Проверено: при `IncludeRuleSets("Create")` результат — только набор (1 ошибка); правила вне набора не выполняются без `IncludeRulesNotInRuleSet`.

**7. Вложенные имена.** `Address.Zip` и `Items[1]` — формат имён свойств (проверено); учитывайте при сопоставлении ошибок с полями формы.

**8. Валидатор не заменяет ограничения БД.** Проверка уникальности — гонка между валидацией и вставкой; обязателен уникальный индекс.

**9. Валидация и преобразования.** Валидатор проверяет форму входа; не меняйте данные в правилах (побочные эффекты в `Must`).

**10. Одна модель — разные сценарии.** Создание и обновление требуют разных правил: `RuleSet` или отдельные валидаторы.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| FluentValidation | выразительные правила, тесты, условия, async | отдельная зависимость и классы |
| Data Annotations | встроено, декларативно | ограниченная выразительность, смешано с моделью |
| `IValidatableObject` | логика в модели | сложнее тестировать и переиспользовать |
| Ручные проверки в коде | просто для мелочей | разрастаются, дублируются |
| Валидация на уровне домена (инварианты) | модель всегда корректна | другое место ответственности |

## Вопросы с собеседований

<details><summary>1. Чем FluentValidation отличается от Data Annotations?</summary>

Правила описываются в отдельном классе беглым API: условия, коллекции, async, наборы, вложенные валидаторы; модель остаётся чистой.

</details>

<details><summary>2. Что возвращает `Validate`?</summary>

`ValidationResult` с `IsValid` и списком `Errors` (`PropertyName`, `ErrorMessage`, `ErrorCode`); исключение — только у `ValidateAndThrow` (проверено).

</details>

<details><summary>3. Как валидировать вложенный объект и коллекцию?</summary>

`SetValidator(new ChildValidator())` и `RuleForEach`; имена ошибок `Address.Zip`, `Items[1]` (проверено).

</details>

<details><summary>4. Как сделать правило условным?</summary>

`.When(predicate)` / `.Unless(...)`; при ложном условии правило пропускается (проверено).

</details>

<details><summary>5. Что делает `CascadeMode.Stop`?</summary>

Останавливает цепочку правил свойства на первой ошибке (проверено: 1 ошибка вместо 2).

</details>

<details><summary>6. Как выполнить асинхронную проверку (например, уникальность в БД)?</summary>

`MustAsync` и `ValidateAsync`; синхронный `Validate` бросит исключение (проверено). Плюс уникальный индекс в БД.

</details>

<details><summary>7. Как зарегистрировать валидаторы в DI?</summary>

`AddValidatorsFromAssemblyContaining<T>()` (пакет DependencyInjectionExtensions); `internal` — через `includeInternalTypes: true` (проверено).

</details>

<details><summary>8. Как применить разные правила для создания и обновления?</summary>

`RuleSet("Create", …)` и `IncludeRuleSets`, либо отдельные валидаторы (проверено).

</details>

<details><summary>9. Подвох: `IsValid == false`, но вы задали только предупреждение. Почему?</summary>

`Severity.Warning` — тоже запись в `Errors`; `IsValid` ложен (проверено). Различайте по `Severity`.

</details>

<details><summary>10. Подвох: тест на текст ошибки проходит локально и падает на CI. Почему?</summary>

Сообщения зависят от культуры (проверено: русские/английские). Проверяйте `ErrorCode` или фиксируйте культуру.

</details>

<details><summary>11. Подвох: проверка уникальности в валидаторе прошла, но в БД дубликат. Почему?</summary>

Гонка между проверкой и вставкой; нужен уникальный индекс и обработка нарушения ограничения.

</details>

## Связанные материалы

- Назад: [продвинутый](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [21 · Архитектура приложения](../../modules/21-architecture/overview.ru.md) — где место валидации.
- [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md) — деревья выражений.
- [Mapperly](../mapperly/overview.ru.md) — маппинг DTO после валидации.

## Источники

- [FluentValidation docs](https://docs.fluentvalidation.net/)
- [FluentValidation: Custom validators](https://docs.fluentvalidation.net/en/latest/custom-validators.html)
- [FluentValidation: Rule sets](https://docs.fluentvalidation.net/en/latest/rulesets.html)
