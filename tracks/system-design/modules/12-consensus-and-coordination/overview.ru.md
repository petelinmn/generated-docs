---
title: "Консенсус и координация"
lang: ru
track: system-design
kind: module
doc: overview
slug: 12-consensus-and-coordination
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Консенсус и координация

🌐 **Русский** · [English](overview.en.md)

## Коротко

Нескольким узлам нужно договориться: кто лидер, какой порядок записей, кто владеет ресурсом. Узлы при этом падают, паузят и теряют связь.

## Ключевые идеи

- Консенсус требует большинства.
- Raft разделяет задачу на выбор лидера и репликацию журнала.
- Термы служат логическими часами.
- Lease истекает, а владелец может этого не заметить.
- Fencing token позволяет хранилищу отвергнуть старого владельца.
- Порядок событий не следует из показаний часов.
- Свой консенсус не пишут: берут готовую систему.

## Минимальный пример

```csharp
if (token < _highestToken) return false; // a holder from the past
```

## Когда применять, а когда нет

- ✅ Выбор единственного исполнителя, конфигурация, метаданные кластера.
- ✅ Понимание гарантий базы или брокера.
- ❌ Распределённая блокировка там, где хватает ограничения в базе.
- ❌ Собственная реализация Raft в продукте.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | большинство, выбор лидера, lease и fencing |
| [2. Продвинутая теория](advanced.ru.md) | правила голосования и фиксации в Raft |
| [3. Экспертная теория](expert.ru.md) | логические часы, справочник, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | голосование, fencing, часы Лэмпорта |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [репликация](../06-replication-and-partitioning/overview.ru.md), [согласованность и CAP](../07-consistency-and-cap/overview.ru.md)
- В других треках: [репликация Kafka](../../../kafka/modules/07-replication-and-availability/overview.ru.md), [архитектура Kubernetes](../../../kubernetes/modules/02-architecture/overview.ru.md)
