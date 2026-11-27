---
title: "Согласованность и CAP: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 07-consistency-and-cap
part: 2
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# Согласованность и CAP: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Уровни изоляции в PostgreSQL

| Уровень | Dirty read | Nonrepeatable | Phantom | Аномалия сериализации |
|---|---|---|---|---|
| Read uncommitted | в PostgreSQL нет | возможно | возможно | возможно |
| Read committed (по умолчанию) | нет | возможно | возможно | возможно |
| Repeatable read | нет | нет | в PostgreSQL нет | возможно |
| Serializable | нет | нет | нет | нет |

Repeatable read в PostgreSQL реализован как snapshot isolation. Приложение должно повторять транзакции при ошибках сериализации; для Serializable это SQLSTATE `40001`. В других СУБД те же названия могут значить другое.

## Snapshot isolation и write skew

Транзакция читает снимок на момент начала; при записи одной и той же строки побеждает первый зафиксировавший.

```csharp
IEnumerable<string> mustBeUnchanged = isolation == Isolation.Serializable
    ? _writes.Keys.Union(_reads)
    : _writes.Keys;
if (_writes.Count > 0 && mustBeUnchanged.Any(k => store.ChangedSince(k, snapshot))) return false;
```

Правило: на дежурстве хотя бы один врач. Две транзакции видят двоих, каждая снимает *своего* — строки разные, конфликта записи нет, и дежурных не остаётся. Проверка прочитанного отклоняет вторую. Файл: [Ex01_Isolation.cs](code/Examples/Ex01_Isolation.cs). Это модель идеи; настоящие СУБД отслеживают зависимости иначе.

## PACELC

При разделении (P) выбирают между доступностью (A) и согласованностью (C), иначе (E) — между задержкой (L) и согласованностью (C). Синхронная репликация платит задержкой каждый день, а не только при авариях.

## Сессионные гарантии

```csharp
public string? Read(Replica replica)
{
    if (replica.Version < _seen) return null;
    _seen = replica.Version;
    return replica.Value;
}
```

Это monotonic reads: клиент не принимает ответ старее уже увиденного. Файл: [Ex03_Sessions.cs](code/Examples/Ex03_Sessions.cs).

## Лучшие практики

- Формулируйте инварианты и проверяйте, чем каждый защищён.
- Уникальность и остатки защищайте ограничениями базы.
- Повторяйте транзакцию целиком, включая чтения.
- Задавайте допустимое устаревание для каждой операции отдельно.

## Частые ошибки

- ❌ Повторить только `COMMIT` → ✅ всю транзакцию.
- ❌ Побочный эффект (письмо) внутри повторяемой транзакции → ✅ после фиксации, через outbox.
- ❌ Проверка «а потом вставка» без уникального индекса → ✅ ограничение в базе.
- ❌ Один уровень согласованности на всю систему → ✅ по операциям.

## Сравнение с другими треками

- Токены конкурентности — [EF Core](../../../csharp/modules/15-ef-core/overview.ru.md).
- Тот же класс гонок внутри процесса — [многопоточность](../../../csharp/modules/18-concurrency/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL: изоляция транзакций](https://www.postgresql.org/docs/current/transaction-iso.html)
