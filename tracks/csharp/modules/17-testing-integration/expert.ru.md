---
title: "Интеграционные тесты: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-server, host-factory, test-pyramid, flaky-tests, interview-questions]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; Microsoft.AspNetCore.Mvc.Testing 11.0.0"
status: draft
updated: 2026-11-22
---

# Интеграционные тесты: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая](basics.ru.md) и [продвинутая](advanced.ru.md) теория. **Время:** ~1 ч.

## Как устроена WebApplicationFactory

`WebApplicationFactory<TEntryPoint>` находит точку входа приложения (для `Program` с top-level statements — через тип `Program`, поэтому он должен быть доступен) и **строит хост** тем же кодом, что и обычный запуск, но вместо Kestrel подставляет **`TestServer`** — in-memory-реализацию `IServer`. Сетевых сокетов нет: `HttpClient`, который выдаёт `CreateClient()`, использует специальный обработчик, передающий запросы прямо в конвейер приложения (проверено: адрес клиента `http://localhost/`, сервер — `TestServer`).

- **Ленивое создание.** Хост строится не в конструкторе фабрики, а при первом обращении к `CreateClient()`, `Services` или `Server`. Проверено: счётчик вызовов `ConfigureWebHost` — `0` до `CreateClient()` и `1` после.
- **Окружение.** По умолчанию `Development` (проверено); переопределяется `builder.UseEnvironment("Staging")` в `ConfigureWebHost` (проверено: `env=Staging`). В `Development` включены проверки областей DI и подробные страницы ошибок — тесты видят те же ошибки конфигурации контейнера, что и разработчик.
- **Порядок настройки.** Сначала выполняется код приложения (`Program`), затем `ConfigureWebHost` фабрики и `WithWebHostBuilder`; регистрации в `ConfigureTestServices` идут **после** регистраций приложения и перекрывают их; обычный `ConfigureServices` в `ConfigureWebHost` выполняется раньше.
- **Корень содержимого.** Фабрика определяет content root (по атрибуту `WebApplicationFactoryContentRootAttribute` или по каталогу проекта) — важно для статических файлов и `appsettings.json` приложения.
- **`Services`** — корневой провайдер приложения: из него создают область (`CreateScope()`) для доступа к `DbContext` или сервисам напрямую, как в проверенном тесте на `ProviderName`.
- **Освобождение.** `Dispose`/`DisposeAsync` фабрики останавливает хост; ресурсы, которые вы создали сами (соединение SQLite), освобождают в переопределённом `Dispose(bool)`.

## Что не проверяет WebApplicationFactory

Хост в памяти не проходит через сеть, TLS, реальные ограничения Kestrel (размер тела, таймауты), обратный прокси. Поэтому конфигурацию развёртывания и поведение под нагрузкой проверяют отдельно: smoke-тестами после деплоя и нагрузочными тестами.

## Справочник

| Что | API |
|---|---|
| Запуск приложения | `WebApplicationFactory<Program>`, `CreateClient()`, `CreateDefaultClient()` |
| Настройки хоста | `ConfigureWebHost`, `WithWebHostBuilder`, `UseEnvironment`, `ConfigureAppConfiguration` |
| Подмена сервисов | `ConfigureTestServices` (`Microsoft.AspNetCore.TestHost`), `RemoveAll<T>()` |
| Клиент | `WebApplicationFactoryClientOptions` (`AllowAutoRedirect`, `BaseAddress`, `HandleCookies`) |
| Сервисы приложения | `factory.Services`, `factory.Server` |
| Фикстуры xUnit | `IClassFixture<T>`, `ICollectionFixture<T>`, `IAsyncLifetime` |
| Базы | SQLite in-memory, EF Core InMemory, Testcontainers |

## Подвохи

**1. Общая база в `IClassFixture`.** Данные видны всем тестам класса; тесты, считающие «всё в таблице», дают разные результаты при разном порядке. Изолируйте (см. [продвинутую теорию](advanced.ru.md#изоляция-данных-между-тестами)).

**2. Соединение SQLite in-memory.** База исчезает при закрытии соединения: если контекст сам открывает и закрывает соединение, каждый запрос увидит пустую базу. Передавайте открытое соединение (`UseSqlite(_connection)`).

**3. `Program` не виден.** Ошибка компиляции `WebApplicationFactory<Program>`: нужен `public partial class Program { }`.

**4. `ConfigureTestServices` не найден** без `using Microsoft.AspNetCore.TestHost;` (проверено: `CS1061`).

**5. InMemory-провайдер EF Core «слишком добр».** Он не проверяет ограничения, внешние ключи и SQL — тест зелёный, а в проде ошибка. Для логики с БД берите реляционный провайдер.

**6. Фоновые сервисы** (`BackgroundService`) стартуют вместе с хостом и могут мешать тесту: отключайте их в `ConfigureTestServices`.

**7. Нестабильные (flaky) тесты.** Причины: общие данные, реальное время, гонки с фоновой обработкой, порядок. Лечение — изоляция и управляемые зависимости, а не повторы и `Thread.Sleep`.

**8. Время запуска.** Каждая новая фабрика — новый запуск приложения (в проверенных тестах около секунды на старт); не создавайте её на каждую мелочь.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `WebApplicationFactory` + SQLite in-memory | быстро, без внешних зависимостей | диалект отличается от боевой СУБД |
| `WebApplicationFactory` + Testcontainers | настоящая СУБД | нужен Docker, медленнее |
| Реальный сервер + внешние тесты (Playwright, k6) | проверка развёрнутой системы | сложнее, медленнее, нестабильнее |
| Только unit-тесты | быстро | не ловят интеграционные ошибки |
| Контрактные тесты (Pact) | независимые сервисы | отдельный инструмент |

## Вопросы с собеседований

<details><summary>1. Чем интеграционный тест отличается от unit-теста?</summary>

Unit-тест проверяет единицу изолированно, с дублёрами. Интеграционный запускает приложение или его часть с реальными компонентами (маршруты, DI, БД) и проверяет через внешний интерфейс. Он медленнее, но ловит ошибки «на стыках».

</details>

<details><summary>2. Что такое `WebApplicationFactory` и `TestServer`?</summary>

Фабрика строит хост приложения для теста; `TestServer` — in-memory-сервер без сети. `CreateClient()` отдаёт `HttpClient`, который шлёт запросы прямо в конвейер.

</details>

<details><summary>3. Зачем нужен `public partial class Program`?</summary>

При top-level statements класс `Program` генерируется внутренним; чтобы `WebApplicationFactory<Program>` его увидела из другого проекта, тип открывают через частичное объявление.

</details>

<details><summary>4. Как заменить сервис в тесте?</summary>

В `ConfigureWebHost` вызвать `builder.ConfigureTestServices(...)`: удалить регистрацию (`RemoveAll<T>()`) и добавить свою. Проверено: часы и БД заменены.

</details>

<details><summary>5. Как изолировать данные между тестами?</summary>

Новая фабрика/БД на тест, очистка таблиц, уникальные данные, откат транзакции. Проверено: фабрика на тест через `IAsyncLifetime` — каждый тест видит только свои данные.

</details>

<details><summary>6. Какую БД использовать в интеграционных тестах?</summary>

Для логики — SQLite in-memory (настоящий SQL, быстро); для достоверности — настоящая СУБД в контейнере; EF Core InMemory — только когда не нужны ограничения и SQL.

</details>

<details><summary>7. Что такое `IClassFixture` для фабрики и чем это опасно?</summary>

Одна фабрика на класс: быстро, но состояние (БД, синглтоны) общее для всех тестов класса, поэтому нужна изоляция.

</details>

<details><summary>8. Как переопределить конфигурацию в одном тесте?</summary>

`factory.WithWebHostBuilder(b => b.ConfigureAppConfiguration(...))` создаёт производную фабрику (проверено: `Feature:Flag = on`).

</details>

<details><summary>9. Что не проверяет `WebApplicationFactory`?</summary>

Сеть, TLS, реальный Kestrel и прокси, поведение развёрнутой среды — их проверяют smoke-тестами после деплоя.

</details>

<details><summary>10. Вопрос с подвохом: тест создал `new MyFactory()`, но `ConfigureWebHost` не вызвался. Почему?</summary>

Хост строится лениво — при первом `CreateClient()`, `Services` или `Server` (проверено: счётчик `0` → `1`).

</details>

<details><summary>11. Вопрос с подвохом: тест с SQLite in-memory видит пустую базу на каждый запрос. Почему?</summary>

Каждое новое соединение к `:memory:` — отдельная база. Нужно открыть одно соединение и передать его в `UseSqlite(connection)`, держа его открытым.

</details>

<details><summary>12. Вопрос с подвохом: тест зелёный на InMemory-провайдере, а в проде падает на уникальном индексе. Почему?</summary>

InMemory не проверяет ограничения и не выполняет SQL; расхождение даёт реляционная СУБД. Используйте SQLite/контейнер.

</details>

## Связанные материалы

- Назад: [продвинутая теория](advanced.ru.md). Проверка: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [16 · Unit-тесты](../16-testing-unit/overview.ru.md) — пирамида тестов.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — приложение под тестом.
- [15 · EF Core](../15-ef-core/overview.ru.md) — провайдеры и миграции.
- [Python](../../../python/overview.ru.md) — тестирование FastAPI через `TestClient`.

## Источники

- [Интеграционные тесты в ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests)
- [Выбор стратегии тестирования EF Core](https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy)
- [`TestServer`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.testhost.testserver)
