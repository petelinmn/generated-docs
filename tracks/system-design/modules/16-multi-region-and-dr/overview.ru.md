---
title: "Несколько регионов и восстановление"
lang: ru
track: system-design
kind: module
doc: overview
slug: 16-multi-region-and-dr
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Несколько регионов и восстановление

🌐 **Русский** · [English](overview.en.md)

## Коротко

Регион целиком может стать недоступным, а данные — оказаться испорченными. Восстановление после катастрофы (disaster recovery, DR) отвечает, сколько данных и времени вы готовы потерять и сколько за это платить.

## Ключевые идеи

- RPO — допустимая потеря данных, RTO — допустимое время восстановления.
- Реплика повторяет и ошибки: от порчи данных спасает резервная копия.
- Стратегии идут от дешёвых и медленных к дорогим и быстрым.
- Переключение — это процесс из нескольких шагов, и каждый занимает время.
- Автоматический failover должен не дёргаться от одной ошибки.
- Active-active требует разрешать конфликты.
- Непроверенное восстановление не считается существующим.

## Минимальный пример

```csharp
public static TimeSpan Downtime(TimeSpan detect, TimeSpan decide, TimeSpan promote, TimeSpan redirect) =>
    detect + decide + promote + redirect;
```

## Когда применять, а когда нет

- ✅ Бизнесу дорог простой или потеря данных.
- ✅ Есть требования регулятора о восстановлении или месте хранения.
- ❌ Несколько регионов до того, как сервис надёжен в одном.
- ❌ Active-active без плана разрешения конфликтов.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | RPO и RTO, стратегии, реплики и копии |
| [2. Продвинутая теория](advanced.ru.md) | failover, active-active, конфликты, учения |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | потери при переключении, контроллер failover, OR-Set |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [процесс проектирования](../01-design-process/overview.ru.md), [репликация](../06-replication-and-partitioning/overview.ru.md), [согласованность](../07-consistency-and-cap/overview.ru.md)
- Дальше: [разбор задач](../17-design-case-studies/overview.ru.md)
- В других треках: [федерация и shovel в RabbitMQ](../../../rabbitmq/modules/12-federation-and-shovel/overview.ru.md), [GitOps](../../../kubernetes/modules/20-gitops/overview.ru.md)
