---
title: "Testcontainers"
lang: ru
track: csharp
kind: library
doc: overview
slug: testcontainers
part: 2
levels: [1, 2, 3, 4]
tags: [testcontainers, docker, integration-testing, postgresql]
versions: ".NET 11, C# 15; Testcontainers 4.17.0"
status: draft
updated: 2026-11-22
---

# Testcontainers

🌐 **Русский** · [English](overview.en.md)

> ⚠️ **Статус проверки.** Docker в среде подготовки недоступен, контейнеры не запускались. Проверено: установка пакетов, компиляция билдеров и `DockerUnavailableException` на `Build()` без Docker. Остальное — по документации; материал остаётся `draft` до проверки на машине с Docker.

## Коротко

Testcontainers поднимает настоящие зависимости (БД, Redis, брокеры) в одноразовых Docker-контейнерах из кода теста: интеграционные тесты идут против реального PostgreSQL, а не подмены. Есть типизированные модули (`Testcontainers.PostgreSql`, `.Redis`…) и универсальный `ContainerBuilder` со стратегиями ожидания. Один контейнер на набор тестов, строка подключения из контейнера, пропуск без Docker.

## Ключевые идеи

- `new PostgreSqlBuilder("postgres:16-alpine")…Build()`, `StartAsync`, `GetConnectionString()`, `DisposeAsync`.
- Порты хоста случайны — не зашивать.
- Стратегия ожидания: «запущен» ≠ «готов».
- Фикстура на класс/набор + очистка данных между тестами.
- Без Docker `Build()` бросает `DockerUnavailableException` (проверено).
- Фиксированные версии образов, кэш образов в CI.

## Минимальный пример

```csharp
var pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
await pg.StartAsync();
var cs = pg.GetConnectionString();
await pg.DisposeAsync();
```

## Когда применять, а когда нет

- ✅ Интеграционные тесты с реальной БД/Redis/брокером.
- ✅ Проверка миграций и SQL-диалекта.
- ✅ Воспроизводимое окружение «из кода».
- ❌ Быстрые юнит-тесты (достаточно подмен).
- ❌ Среды без Docker.
- ❌ Нагрузочные тесты production-масштаба.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | модуль PostgreSQL, универсальный контейнер, поведение без Docker |
| [2. Продвинутый](advanced.ru.md) | фикстуры, пропуск без Docker, `WebApplicationFactory`, сети, очистка данных |
| [3. Эксперт](expert.ru.md) | устройство, подвохи, альтернативы, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp testcontainers level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp testcontainers level 6` |

## Связанные материалы

- Пререквизиты: [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md)
- Рядом: [15 · EF Core](../../modules/15-ef-core/overview.ru.md), [Dapper](../dapper/overview.ru.md), [NSubstitute](../nsubstitute/overview.ru.md), [MassTransit](../masstransit/overview.ru.md) — настоящий RabbitMQ в тестах
- В других треках: [Kubernetes](../../../kubernetes/overview.ru.md) — контейнеры и kind; [Контейнеры и Docker](../../../kubernetes/modules/01-containers/overview.ru.md) — образы и контейнеры, которые запускает Testcontainers
