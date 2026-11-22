---
title: "Квиз: Интеграционные тесты"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-isolation, sqlite, test-server]
status: draft
updated: 2026-11-22
---

# Квиз: Интеграционные тесты

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что делает `WebApplicationFactory<Program>`?

- A) запускает приложение на реальном порту в Docker
- B) строит хост вашего приложения внутри теста на in-memory-сервере и выдаёт `HttpClient`
- C) генерирует unit-тесты
- D) подменяет компилятор

<details><summary>Ответ</summary>

**B.** Используется `TestServer` без сети; `CreateClient()` даёт клиент с адресом `http://localhost/` (проверено).
→ [Базовая теория](basics.ru.md#webapplicationfactory)

</details>

## 2. 🟢 Зачем в API-проекте нужно `public partial class Program { }`?

- A) для ускорения запуска
- B) чтобы тестовый проект видел класс `Program` приложения с top-level statements
- C) чтобы включить Native AOT
- D) это ошибка: лишний код

<details><summary>Ответ</summary>

**B.** Сгенерированный `Program` внутренний; частичное объявление делает его доступным для `WebApplicationFactory<Program>`.
→ [Базовая теория](basics.ru.md#webapplicationfactory)

</details>

## 3. 🟢 Какие коды вернул API в проверенных сценариях?

- A) `POST /todos` с названием → `200`; пустое название → `500`
- B) `POST /todos` с названием → `201`; пустое название → `400` (`problem+json`); `GET /todos/999` → `404`
- C) всё возвращает `200`
- D) `GET /todos/999` → `204`

<details><summary>Ответ</summary>

**B.** Создание даёт `201` и `Location`, пустое название — `400` с `application/problem+json`, неизвестный `id` — `404` (проверено).
→ [Базовая теория](basics.ru.md#проверка-ответа)

</details>

## 4. 🟢 Откуда тест берёт фиксированное время?

- A) из `DateTime.Now`
- B) через подмену `IClock` в `ConfigureTestServices`
- C) из системных часов контейнера
- D) тест не может контролировать время

<details><summary>Ответ</summary>

**B.** Сервис часов удаляется и заменяется `FixedClock`; проверено: `CreatedAt` созданной задачи равен `ApiFactory.Now`.
→ [Базовая теория](basics.ru.md#подмена-зависимостей)

</details>

## 5. 🟡 Почему данные одного теста могут «протекать» в другой?

- A) xUnit запускает все тесты в одном потоке
- B) фабрика в `IClassFixture` создаётся один раз на класс, и база данных у тестов класса общая
- C) так работает SQLite
- D) это не так: данные всегда изолированы

<details><summary>Ответ</summary>

**B.** Общая фабрика — общая БД и синглтоны. Изолируют новой фабрикой на тест (`IAsyncLifetime`), очисткой, уникальными данными или откатом транзакции.
→ [Продвинутая теория](advanced.ru.md#изоляция-данных-между-тестами)

</details>

## 6. 🟡 Что произойдёт, если не держать открытым соединение с `DataSource=:memory:`?

- A) ничего: база сохранится
- B) база исчезнет вместе с соединением, каждое новое соединение увидит пустую базу
- C) приложение упадёт при старте
- D) тест пропустится

<details><summary>Ответ</summary>

**B.** In-memory-база SQLite живёт, пока открыто соединение. Поэтому фабрика открывает соединение один раз и передаёт его в `UseSqlite(_connection)`.
→ [Продвинутая теория](advanced.ru.md#какую-базу-брать)

</details>

## 7. 🟡 Выберите все верные утверждения о выборе базы для тестов

- [ ] A) EF Core InMemory не проверяет ограничения и не выполняет SQL
- [ ] B) SQLite in-memory использует настоящий SQL, но диалект отличается от боевой СУБД
- [ ] C) Testcontainers запускает настоящую СУБД в Docker
- [ ] D) InMemory-провайдер полностью повторяет поведение PostgreSQL

<details><summary>Ответ</summary>

**A, B, C.** D неверно: InMemory — не реляционный провайдер. Логику с ограничениями лучше проверять на SQLite или контейнере.
→ [Продвинутая теория](advanced.ru.md#какую-базу-брать)

</details>

## 8. 🟡 Как переопределить конфигурацию в одном тесте?

- A) изменить `appsettings.json`
- B) `factory.WithWebHostBuilder(b => b.ConfigureAppConfiguration(...))`
- C) установить переменную окружения вручную в CI
- D) нельзя

<details><summary>Ответ</summary>

**B.** Создаётся производная фабрика со своей конфигурацией; проверено: значение `Feature:Flag = on`.
→ [Продвинутая теория](advanced.ru.md#конфигурация-теста)

</details>

## 9. 🟡 Открытый вопрос: какие сценарии проверять интеграционными тестами, а какие unit-тестами?

<details><summary>Ключевые пункты хорошего ответа</summary>

- интеграционные: основные сценарии (создать-получить-удалить), коды и форматы ошибок на границе, регистрация зависимостей, конфигурация, работа с БД, авторизация;
- unit: логика, ветвления, граничные значения, мелкие вариации;
- интеграционных тестов меньше: они медленнее и дороже в поддержке (пирамида тестов);
- подменять внешнее (время, почта, платежи), а не реальную инфраструктуру приложения.

</details>

## 10. 🔴 Вызвали `new ApiFactory()` и сразу проверили счётчик вызовов `ConfigureWebHost`. Чему он равен?

- A) 1
- B) 0
- C) 2
- D) зависит от числа тестов

<details><summary>Ответ</summary>

**B.** Хост строится лениво — при первом `CreateClient()`, `Services` или `Server` (проверено: `0` до, `1` после).
→ [Экспертная теория](expert.ru.md#как-устроена-webapplicationfactory)

</details>

## 11. 🔴 Какое окружение по умолчанию у приложения в `WebApplicationFactory` и как его сменить?

- A) `Production`; изменить нельзя
- B) `Development`; `builder.UseEnvironment("Staging")` в `ConfigureWebHost`
- C) `Test`; через `appsettings.json`
- D) `Staging`; через переменную `ASPNETCORE_URLS`

<details><summary>Ответ</summary>

**B.** Проверено: по умолчанию `Development`, после `UseEnvironment("Staging")` — `Staging`. В `Development` включены проверки областей DI.
→ [Экспертная теория](expert.ru.md#как-устроена-webapplicationfactory)

</details>
