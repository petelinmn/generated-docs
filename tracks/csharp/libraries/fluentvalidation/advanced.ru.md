---
title: "FluentValidation: продвинутый уровень"
lang: ru
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

# FluentValidation: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Каскад правил

По умолчанию все правила цепочки выполняются, даже если первое уже не прошло:

```csharp
RuleFor(u => u.Name).NotEmpty().MinimumLength(5);                                   // 2 errors for ""
RuleFor(u => u.Name).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(5);         // 1 error
```

Проверено: для пустого имени — 2 ошибки по умолчанию и 1 с `CascadeMode.Stop`.

## Условия, `Must`, сообщения

```csharp
RuleFor(u => u.Age).GreaterThanOrEqualTo(18)
    .When(u => u.Email.EndsWith("@corp.com"))
    .WithMessage("Corp users must be adults, got {PropertyValue}");
RuleFor(u => u.Name).Must(n => n != "root").WithMessage("'root' is reserved");
```

Проверено: для `root` / `a@corp.com` / 10 — обе ошибки (`… got 10`, `'root' is reserved`); для `a@other.com` условие не применяется, ошибок по возрасту нет. В сообщении доступны плейсхолдеры (`{PropertyValue}`, `{PropertyName}`).

## Коллекции

```csharp
RuleForEach(o => o.Items).NotEmpty().WithMessage("item must not be empty");
RuleFor(o => o.Items).NotEmpty();
```

Проверено: для `["a", "", "c", ""]` ошибки у `Items[1]` и `Items[3]` — индекс входит в имя свойства.

## Асинхронные правила

```csharp
RuleFor(u => u.Name).MustAsync(async (n, ct) => { await Task.Delay(1, ct); return n != "taken"; })
                    .WithMessage("name already taken");
var r = await validator.ValidateAsync(user);
```

Проверено: `ValidateAsync` вернул `name already taken`. **Синхронный `Validate` для валидатора с асинхронным правилом бросает `AsyncValidatorInvokedSynchronouslyException`.** Используйте `ValidateAsync` везде, где есть async-правила (например, проверка уникальности в БД).

## Наборы правил (RuleSet)

```csharp
RuleSet("Create", () => RuleFor(u => u.Name).NotEmpty());
RuleFor(u => u.Age).GreaterThan(0);
```

Проверено: `Validate(user)` выполняет только правила вне наборов (0 ошибок), `Validate(user, o => o.IncludeRuleSets("Create"))` — набор `Create` (1 ошибка; правило вне набора при этом не выполняется, если не добавить `IncludeRulesNotInRuleSet()`).

## Серьёзность

`WithSeverity(Severity.Warning)` превращает ошибку в предупреждение. Подвох: проверено — результат с одним предупреждением имеет `IsValid = False` (предупреждение — тоже запись в `Errors`). Для «успеха» нужно отдельно проверять `Severity`.

## Локализация и имена

Стандартные сообщения берутся по культуре: `ValidatorOptions.Global.LanguageManager.Culture`. Проверено: `ru-RU` → `'Age' должно быть больше '18'.`, `en-US` → `'Age' must be greater than '18'.`. `WithName("Возраст")` меняет имя свойства в сообщении (проверено).

## Внедрение зависимостей

```csharp
services.AddValidatorsFromAssemblyContaining<UserValidator>();
var v = sp.GetRequiredService<IValidator<User>>();
```

Пакет `FluentValidation.DependencyInjectionExtensions` регистрирует все валидаторы сборки (по умолчанию — со временем жизни `Scoped`). **Подвох:** проверено — `internal`-валидатор по умолчанию **не регистрируется** (`GetRequiredService` падает с `No service for type IValidator<User>`); сделайте класс `public` или передайте `includeInternalTypes: true`.

> ⚠️ Проверить: время жизни по умолчанию (`Scoped`) в этом материале не проверялось запуском; сверьтесь с документацией.

## Интеграция с Minimal API

Автоматической валидации (как в MVC) в Minimal API нет: валидатор вызывают вручную или через endpoint filter.

```csharp
var res = await validator.ValidateAsync(user);
if (!res.IsValid) return Results.ValidationProblem(res.ToDictionary());
```

Проверено: `ToDictionary()` даёт словарь «свойство → сообщения» (`Name=… ; Age=…`), который принимает `Results.ValidationProblem`.

> ⚠️ Проверить: сам вызов `Results.ValidationProblem` и ответ `400` с `ProblemDetails` в этом материале не запускались.

## Best practices

| Правило | Почему |
|---|---|
| Валидатор — отдельный класс, регистрируется в DI | тестируемость |
| `ValidateAsync` при любых async-правилах | иначе исключение |
| `CascadeMode.Stop` для дорогих правил | не выполнять лишнее |
| Коды ошибок (`WithErrorCode`) для клиентов | стабильный контракт |
| Не класть бизнес-логику в валидатор | валидатор — про форму входа |
| Тестировать валидаторы юнит-тестами | быстрые и точные |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/advanced.ru.md#валидация-и-problemdetails) — `ProblemDetails`.
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — регистрация.
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md)

## Источники

- [FluentValidation: Cascade mode](https://docs.fluentvalidation.net/en/latest/cascade.html)
- [FluentValidation: Asynchronous validation](https://docs.fluentvalidation.net/en/latest/async.html)
- [FluentValidation: Dependency injection](https://docs.fluentvalidation.net/en/latest/di.html)
- [FluentValidation: Localization](https://docs.fluentvalidation.net/en/latest/localization.html)
