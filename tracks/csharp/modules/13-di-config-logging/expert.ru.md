---
title: "DI, конфигурация, логирование: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, service-provider, disposal, constructor-selection, options-factory, configuration-providers, logging-pipeline, interview-questions]
versions: ".NET 11, C# 15; Microsoft.Extensions.* 11.0.0"
status: draft
updated: 2026-11-22
---

# DI, конфигурация, логирование: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая](basics.ru.md) и [продвинутая](advanced.ru.md) теория. **Время:** ~1,5 ч.

## Как устроен контейнер

`ServiceCollection` — список дескрипторов (`ServiceDescriptor`): тип сервиса, способ создания (тип реализации, готовый экземпляр, фабрика), время жизни и необязательный ключ. `BuildServiceProvider()` создаёт **корневой провайдер**; `CreateScope()` — дочернюю область, которая хранит свои `Scoped`-экземпляры. Singleton-ы живут в корне.

- **Выбор конструктора.** Контейнер выбирает конструктор с наибольшим числом параметров, все параметры которого можно разрешить. Проверено: у типа с тремя конструкторами `()`, `(X)`, `(X, Y)` выбран `(X, Y)`. Если подходят два конструктора, ни один из которых не «покрывает» другой, — `InvalidOperationException: Unable to activate type 'Ambig'. The following constructors are ambiguous`.
- **Циклы** обнаруживаются при разрешении: `A` зависит от `B`, `B` от `A` → `InvalidOperationException: A circular dependency was detected for the service of type 'C1'` (проверено).
- **Корневой провайдер и Scoped.** Без `ValidateScopes` запрос `Scoped`-сервиса из корня **работает** и возвращает один экземпляр на всё время жизни провайдера (проверено: `ReferenceEquals` → `True`), то есть ведёт себя как singleton. С `ValidateScopes = true` — `InvalidOperationException: Cannot resolve scoped service 'S1' from root provider.`
- **Проверка наличия.** `IServiceProviderIsService.IsService(type)` отвечает, разрешим ли тип без создания экземпляра (проверено: `True` для зарегистрированного, `False` для нет).
- **Реализация.** Провайдер на первом обращении к типу строит и кэширует «план» разрешения (на основе скомпилированных выражений или `DynamicMethod`), поэтому первый вызов дороже последующих. В режимах без динамической генерации кода (Native AOT) используется другой путь.

### Освобождение

Контейнер владеет всем, что создал, и вызывает `Dispose` у `IDisposable`/`IAsyncDisposable`-сервисов при освобождении области или провайдера, **в порядке, обратном созданию** (проверено: `B1, A1` для `B1`, зависящего от `A1`). Сервисы, которые вы передали готовым экземпляром (`AddSingleton(instance)`), контейнер **не** освобождает.

Подвох с `Transient`: контейнер хранит ссылки на созданные одноразовые `Transient`-экземпляры до освобождения области. Проверено: три запроса из корневого провайдера — три созданных `D`, ни один не освобождён (`disposed=0`) до `provider.Dispose()`, после которого `disposed=3`. В долгоживущем корневом провайдере это утечка: запрашивайте такие сервисы внутри короткой области или не регистрируйте одноразовые типы как `Transient`.

## Options изнутри

`IOptions<T>` не читает конфигурацию напрямую: `OptionsFactory<T>` создаёт объект, применяя **настройщики** по порядку: `IConfigureOptions<T>` (в том числе `Configure`/`Bind`), затем `IPostConfigureOptions<T>`, затем проверки `IValidateOptions<T>`. Результат кэшируется в `IOptionsMonitorCache<T>`. У настроек есть **имена** (`IOptionsSnapshot<T>.Get("name")`, `Configure<T>("name", …)`) — для нескольких наборов одного типа. `IOptionsMonitor` подписан на токены изменений конфигурации: при изменении кэш сбрасывается, и вызывается `OnChange`. Поэтому в проверенном примере монитор показал новое значение сразу, `IOptionsSnapshot` — в новой области, а `IOptions` — старое.

`ValidateOnStart` регистрирует проверку как часть запуска хоста: ошибка попадает в `Hosting failed to start` с исключением `OptionsValidationException` и списком проблем (проверено: `Host` обязателен, `Port` вне диапазона — 2 проблемы).

## Конфигурация изнутри

`ConfigurationBuilder` складывает провайдеры в список; `Build()` создаёт корневую конфигурацию, которая при чтении ключа обходит провайдеры **с конца**, пока не найдёт значение (поэтому побеждает последний). Ключи регистронезависимы, разделитель — `:`; для массивов индексы (`Tags:0`). Провайдеры, умеющие отслеживать изменения (файл с `reloadOnChange`), публикуют `IChangeToken`; при перезагрузке значение читается заново. Переменные окружения превращают `__` в `:`; префикс отсекается (`APP__Mail__Host` → `Mail:Host`: проверено).

## Конвейер логирования

`ILogger<T>` — тонкая обёртка: логгер категории хранит набор логгеров всех **провайдеров** и при вызове обходит их; каждый сам решает через `IsEnabled`, писать ли. Фильтры по категории и уровню применяются раньше. Шаблон разбирается один раз и кэшируется; значения передаются отдельно от текста, поэтому провайдер структурного логирования (Serilog, OpenTelemetry) получает именованные свойства. `[LoggerMessage]` убирает разбор и боксинг аргументов на каждом вызове. Проверено на собственном `ILoggerProvider`: получены уровень, отформатированное сообщение, область и тип исключения; сообщение `Debug` при `Information` отброшено.

## Справочник

| API | Назначение |
|---|---|
| `AddSingleton/Scoped/Transient<TService, TImpl>()` | регистрация по типу |
| `AddSingleton<T>(instance)` | готовый экземпляр, контейнер не освобождает |
| `AddSingleton<T>(sp => …)` | фабрика |
| `AddKeyed*` / `GetRequiredKeyedService` / `[FromKeyedServices]` | ключевые сервисы (.NET 8) |
| `TryAdd*`, `Replace`, `RemoveAll` | управление регистрациями |
| `ServiceProviderOptions.ValidateScopes/ValidateOnBuild` | проверки при сборке |
| `IServiceScopeFactory` | создать область из singleton |
| `IOptions<T>` / `IOptionsSnapshot<T>` / `IOptionsMonitor<T>` | настройки |
| `ValidateDataAnnotations` / `Validate` / `ValidateOnStart` | проверка настроек |
| `BackgroundService`, `IHostedService`, `IHostApplicationLifetime` | жизненный цикл хоста |
| `ILogger<T>`, `ILoggerFactory`, `ILoggerProvider`, `[LoggerMessage]` | логирование |

## Подвохи

**1. Singleton с зависимостью `Scoped` без включённых проверок.** В `Production` проверки по умолчанию выключены: приложение собралось (проверено), а ошибка проявится как «чужие» данные одного запроса в другом.

**2. Одноразовый `Transient` в корне.** Хранится до освобождения провайдера (см. выше).

**3. `AddSingleton(instance)` и `Dispose`.** Контейнер не освобождает переданные вами экземпляры — владелец вы.

**4. Порядок регистрации.** Для одного интерфейса побеждает последняя; `TryAdd` не заменяет существующую (проверено: 2 регистрации).

**5. `ValidateDataAnnotations` — отдельный пакет.** Без `Microsoft.Extensions.Options.DataAnnotations` метод недоступен (ошибка компиляции — проверено).

**6. `IOptions<T>` не обновляется.** Для горячей перезагрузки нужен `IOptionsMonitor`/`IOptionsSnapshot`.

**7. `ClearProviders` требует `using Microsoft.Extensions.Logging;`.** Без директивы метод расширения не виден (проверено).

**8. Service Locator.** Вызов `provider.GetService<T>()` из бизнес-кода прячет зависимости: класс нельзя понять по конструктору и сложно тестировать.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Встроенный контейнер | стандарт, быстрый, достаточно для большинства | нет автоподключения сборок, декораторов, свойств |
| Autofac, Lamar | модули, декораторы, внедрение в свойства | дополнительная зависимость и сложность |
| Scrutor | сканирование сборок и декораторы поверх встроенного | внешняя библиотека |
| Ручная композиция (Pure DI) | всё видно в одном месте | много кода в корне приложения |
| Конфигурация через `IConfiguration` напрямую | просто | нет типизации и проверки |

## Вопросы с собеседований

<details><summary>1. Что такое внедрение зависимостей и зачем оно нужно?</summary>

Класс получает зависимости снаружи (через конструктор), а не создаёт сами. Это уменьшает связанность, позволяет подменять реализации (тесты) и централизует создание объектов в контейнере.

</details>

<details><summary>2. Чем отличаются Singleton, Scoped и Transient?</summary>

Singleton — один экземпляр на приложение; Scoped — один на область (в вебе — на запрос); Transient — новый при каждом запросе сервиса. Проверено: при 2 областях × 2 запросах создано `singleton=1 scoped=2 transient=4`.

</details>

<details><summary>3. Что такое «захваченная зависимость» и как её найти?</summary>

Долгоживущий сервис (singleton) внедряет короткоживущий (scoped), и тот «застревает». Находится включением `ValidateScopes`/`ValidateOnBuild`; по умолчанию они включены только в `Development`.

</details>

<details><summary>4. Как получить Scoped-сервис из Singleton?</summary>

Внедрить `IServiceScopeFactory`, создавать область на каждую операцию и разрешать сервис из неё; область освободить через `using`.

</details>

<details><summary>5. Что произойдёт при нескольких регистрациях одного интерфейса?</summary>

`GetService<T>` вернёт последнюю, `GetServices<T>` — все по порядку. `TryAdd*` не добавит, если регистрация уже есть.

</details>

<details><summary>6. Как контейнер выбирает конструктор?</summary>

Берёт конструктор с наибольшим числом разрешимых параметров; при неоднозначности бросает `InvalidOperationException` об ambiguous constructors.

</details>

<details><summary>7. Кто и когда освобождает сервисы?</summary>

Контейнер освобождает созданные им `IDisposable` при освобождении области или провайдера в порядке, обратном созданию; переданные готовые экземпляры не освобождает.

</details>

<details><summary>8. В чём разница между IOptions, IOptionsSnapshot и IOptionsMonitor?</summary>

`IOptions` — singleton без перезагрузки; `IOptionsSnapshot` — scoped, обновляется на границе области; `IOptionsMonitor` — singleton с немедленным обновлением и `OnChange`.

</details>

<details><summary>9. Как проверить настройки при запуске?</summary>

`AddOptions<T>().Bind(...).ValidateDataAnnotations().ValidateOnStart()` — хост не запустится при некорректных настройках (`OptionsValidationException`).

</details>

<details><summary>10. Как слои конфигурации перекрывают друг друга?</summary>

Побеждает провайдер, добавленный позже: обычно `appsettings.json` → окружение-специфичный файл → секреты (в Development) → переменные окружения → командная строка.

</details>

<details><summary>11. Почему шаблон сообщения лучше интерполяции в логах?</summary>

Шаблон сохраняет структуру (именованные свойства) для поиска и агрегации, форматирование выполняется только если уровень включён; интерполяция форматирует строку всегда и теряет структуру.

</details>

<details><summary>12. Вопрос с подвохом: почему приложение собралось, а в продакшене данные одного пользователя видны другому?</summary>

Singleton держит Scoped-сервис (например, `DbContext`), а проверки областей в `Production` выключены по умолчанию — ошибки при сборке нет (проверено), но экземпляр общий для всех запросов.

</details>

<details><summary>13. Вопрос с подвохом: почему растёт память при `AddTransient<MyDisposable>()` и запросах из корневого провайдера?</summary>

Контейнер хранит ссылки на одноразовые Transient-экземпляры до освобождения провайдера (проверено: 3 созданных, 0 освобождённых). Нужно запрашивать их в короткой области.

</details>

<details><summary>14. Вопрос с подвохом: освободит ли контейнер экземпляр, переданный в `AddSingleton(instance)`?</summary>

Нет: владельцем остаётесь вы. Освобождает только то, что создал сам.

</details>

## Связанные материалы

- Назад: [продвинутая теория](advanced.ru.md). Проверка: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — `Dispose`, финализаторы.
- [10 · Async/await](../10-async-await/overview.ru.md) — `IAsyncDisposable`, отмена фоновых сервисов.
- [15 · EF Core](../15-ef-core/overview.ru.md) — время жизни `DbContext`.
- [Python](../../../python/overview.ru.md) — зависимости в FastAPI, `logging`.

## Источники

- [Рекомендации по внедрению зависимостей](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-guidelines)
- [Провайдеры конфигурации](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers)
- [Провайдеры логирования](https://learn.microsoft.com/dotnet/core/extensions/logging-providers)
- [`ServiceProviderOptions`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovideroptions)
