---
title: "Репликация и партиционирование: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Репликация и партиционирование: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** базы данных, транзакции в общих чертах · **Время:** ~2 ч

## Зачем это нужно

Один узел когда-нибудь откажет, а данные когда-нибудь перестанут на него помещаться. Репликация отвечает на первое, партиционирование — на второе.

## Лидер и реплики

Запись принимает лидер, он ведёт журнал изменений; реплики применяют его по порядку. Чтения можно отдавать репликам.

| Схема | Кто принимает запись | Особенность |
|---|---|---|
| Один лидер | один узел | нет конфликтов записи, но нужен failover |
| Несколько лидеров | несколько узлов | конфликты |
| Без лидера | клиент пишет в несколько реплик | кворумы и восстановление копий |

## Отставание реплики

```csharp
public void Replicate(int maxEntries = int.MaxValue)
{
    while (AppliedPosition < leader.Log.Count && maxEntries-- > 0)
    {
        var (key, value) = leader.Log[(int)AppliedPosition];
        _data[key] = value;
        AppliedPosition++;
    }
}
```

Пока `Replicate` не вызван, реплика отдаёт старое значение. Это моделирует лаг (replication lag): пользователь сохранил профиль, обновил страницу и увидел прежнее имя. Файл: [Ex01_ReplicationLag.cs](code/Examples/Ex01_ReplicationLag.cs).

## Партиционирование

| Вид | Что делит |
|---|---|
| Горизонтальное (шардирование) | строки: схема одна, у каждого шарда своё подмножество |
| Вертикальное | поля одной сущности по характеру использования |
| Функциональное | данные по бизнес-областям |

## По диапазону или по хэшу

```csharp
public static int ByRange(long key, params long[] upperBounds)
{
    for (int i = 0; i < upperBounds.Length; i++)
        if (key < upperBounds[i]) return i;
    return upperBounds.Length;
}
```

Диапазоны удобны для запросов «с — по», но при растущих ключах (время, счётчик) все новые записи идут в последнюю партицию: ключи 3000–3999 при границах 1000, 2000, 3000 дают нагрузку `0, 0, 0, 1000`. Хэш распределяет их как `234, 246, 266, 254`, но запрос диапазона обходит все партиции. Файл: [Ex03_Partitioning.cs](code/Examples/Ex03_Partitioning.cs).

## Ошибки новичка

- ❌ Реплика вместо резервной копии → ✅ нужны обе.
- ❌ Читать с реплики сразу после своей записи → ✅ учитывать лаг.
- ❌ Ключ партиции с малым числом значений (страна, статус) → ✅ высокая кардинальность.
- ❌ Шардировать «на будущее» → ✅ сначала оценка.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Репликация в Kafka](../../../kafka/modules/07-replication-and-availability/overview.ru.md)

## Источники

- [Azure: партиционирование данных](https://learn.microsoft.com/azure/architecture/best-practices/data-partitioning)
- [PostgreSQL: партиционирование таблиц](https://www.postgresql.org/docs/current/ddl-partitioning.html)
