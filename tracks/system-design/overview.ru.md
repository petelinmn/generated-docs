---
title: "Архитектура и system design"
lang: ru
track: system-design
kind: track
doc: overview
slug: system-design
tags: [system-design, architecture, dotnet]
versions: ".NET 11, C# 15"
default_level: 6
status: draft
updated: 2026-11-26
---

# Архитектура и system design

🌐 **Русский** · [English](overview.en.md)

## Коротко

System design — это умение превратить требования («мессенджер на 10 миллионов пользователей») в устройство системы: какие компоненты нужны, как они общаются, где хранятся данные, что произойдёт при отказе и сколько это стоит. Здесь нет единственно правильного ответа: есть компромиссы, и их нужно уметь назвать и обосновать числами. Все идеи в треке показаны кодом на C#/.NET, который можно запустить и проверить тестами. Устройство кода *внутри* одного приложения разобрано отдельно: [C#: архитектура приложений](../csharp/modules/21-architecture/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| SDK | .NET 11 SDK или новее — `dotnet --version`; модули нацелены на `net11.0` |
| Редактор | VS Code + C# Dev Kit, Rider или Visual Studio |
| Код модуля | `code/` — решение `Code.slnx`: `Examples/`, `Solutions/`, `Tests/` (xUnit v3) и `global.json` с режимом Microsoft.Testing.Platform |
| Запустить все тесты модуля | `dotnet test` в папке `code/` |
| Запустить один пример | `dotnet test --filter "FullyQualifiedName~<Имя>"` |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **6** (все уровни), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Процесс проектирования и оценки](modules/01-design-process/overview.ru.md) — требования, расчёты «на салфетке», SLI/SLO, доступность, перцентили
- [02 · Масштабирование и балансировка](modules/02-scalability-and-load-balancing/overview.ru.md) — вертикальное и горизонтальное, stateless, алгоритмы балансировки, consistent hashing, YARP
- [03 · Кэширование](modules/03-caching/overview.ru.md) — уровни кэша, cache-aside, TTL и инвалидация, вытеснение, cache stampede, HybridCache
- [04 · Хранение данных](modules/04-data-storage/overview.ru.md) — реляционные и NoSQL-хранилища, индексы, B-tree и LSM, выбор по паттернам доступа
- [05 · Проектирование API](modules/05-api-design/overview.ru.md) — REST, gRPC, версионирование, идемпотентность, пагинация

### Часть 2. Продвинутые темы

- [06 · Репликация и партиционирование](modules/06-replication-and-partitioning/overview.ru.md) — leader/follower, кворумы, выбор ключа шардирования, горячие ключи
- [07 · Согласованность и CAP](modules/07-consistency-and-cap/overview.ru.md) — модели согласованности, CAP и PACELC, изоляция транзакций
- [08 · Сообщения и события](modules/08-messaging-and-events/overview.ru.md) — очереди и журналы, семантика доставки, идемпотентные потребители, backpressure
- [09 · Паттерны надёжности](modules/09-reliability-patterns/overview.ru.md) — таймауты, retry, circuit breaker, bulkhead, load shedding, rate limiting
- [10 · Распределённые транзакции](modules/10-distributed-transactions/overview.ru.md) — 2PC, saga, outbox и inbox
- [11 · Архитектурные стили](modules/11-architecture-styles/overview.ru.md) — монолит, модульный монолит, микросервисы, serverless, ADR

### Часть 3. Темы для опытных

- [12 · Консенсус и координация](modules/12-consensus-and-coordination/overview.ru.md) — Raft, выбор лидера, распределённые блокировки, fencing tokens, логические часы
- [13 · Event sourcing и CQRS](modules/13-event-sourcing-and-cqrs/overview.ru.md) — поток событий, проекции, версионирование событий
- [14 · Наблюдаемость и SLO](modules/14-observability-and-slo/overview.ru.md) — метрики, трассировка, бюджеты ошибок, алерты
- [15 · Безопасность архитектуры](modules/15-security-architecture/overview.ru.md) — модель угроз, аутентификация и авторизация между сервисами, секреты, zero trust
- [16 · Несколько регионов и восстановление](modules/16-multi-region-and-dr/overview.ru.md) — RPO и RTO, active-active, конфликты, учения
- [17 · Разбор задач](modules/17-design-case-studies/overview.ru.md) — полные разборы задач в формате интервью

## Библиотеки

- [YARP](libraries/yarp/overview.ru.md) — reverse proxy и API-шлюз на .NET
- [Orleans](libraries/orleans/overview.ru.md) — виртуальные акторы для распределённых приложений

## Рецепты

- [Сокращатель ссылок](recipes/url-shortener/overview.ru.md) — генерация коротких кодов, кэш, аналитика
- [Распределённый rate limiter](recipes/distributed-rate-limiter/overview.ru.md) — общий лимит на несколько экземпляров
- [Лента новостей](recipes/news-feed/overview.ru.md) — fan-out при записи и при чтении
- [Сервис уведомлений](recipes/notification-service/overview.ru.md) — очереди, повторы, дедупликация

## Проекты

- [Сокращатель ссылок под нагрузкой](projects/url-shortener-service/overview.ru.md) — API, кэш, несколько экземпляров, нагрузочный тест

## План расширения уровня 3

Все модули созданы на уровне 6, но экспертные файлы пока короче, чем нужно справочнику. Модули 01–10 уже расширены. Ниже то, что предстоит дописать; каждый пункт закрывается командой `/learn system-design <модуль> level 3`, после чего модуль проверяется `/verify`.

| Модуль | Что дописать в экспертную теорию |
|---|---|
| 11 · Архитектурные стили | стратегический DDD и карты контекстов, service mesh, BFF и API-шлюз, micro-frontends, branch by abstraction |
| 12 · Консенсус и координация | Paxos и Raft, смена состава кластера, снимки журнала Raft, примитивы ZooKeeper и etcd, гибридные логические часы |
| 13 · Event sourcing и CQRS | готовые event store, перестроение проекций без простоя, стратегии снимков, реализация crypto-shredding |
| 14 · Наблюдаемость и SLO | OpenTelemetry Collector, exemplars, непрерывное профилирование, сэмплирование логов, синтетический мониторинг |
| 15 · Безопасность | потоки OAuth 2.0 и OIDC, mTLS и ротация сертификатов, хранилища секретов, цепочка поставок (SBOM, подписи), изоляция арендаторов, иерархия ключей шифрования |
| 16 · Несколько регионов | глобальные базы и их уровни согласованности, время переключения DNS и anycast, game days, неизменяемые копии |
| 17 · Разбор задач | чат, распределённый rate limiter, автодополнение поиска, файловое хранилище, платёжная система, геопоиск |

## Связанные треки

- [C# / .NET](../csharp/overview.ru.md) — язык и платформа, на которых написаны примеры
- [Kubernetes](../kubernetes/overview.ru.md) — где живут и масштабируются сервисы
- [PostgreSQL](../postgresql/overview.ru.md) — реляционное хранилище
- [Kafka](../kafka/overview.ru.md) и [RabbitMQ](../rabbitmq/overview.ru.md) — брокеры сообщений
- [OpenSearch](../opensearch/overview.ru.md) — поиск

## Источники

- [Azure Architecture Center: стили архитектуры](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/)
- [Azure Architecture Center: облачные паттерны](https://learn.microsoft.com/azure/architecture/patterns/)
- [Google SRE Book: Service Level Objectives](https://sre.google/sre-book/service-level-objectives/)
