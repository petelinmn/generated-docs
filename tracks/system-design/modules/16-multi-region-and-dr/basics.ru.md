---
title: "Несколько регионов и восстановление: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Несколько регионов и восстановление: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** репликация, доступность · **Время:** ~2 ч

## Зачем это нужно

Резервные копии есть почти у всех, а проверенный план восстановления — не у всех. В аварии выясняется, что копия неполная, а восстановление занимает сутки.

## RPO и RTO

- **RPO** (recovery point objective) — максимально допустимая потеря данных, измеренная во времени.
- **RTO** (recovery time objective) — максимально допустимое время восстановления работы.

При асинхронной репликации теряется то, что не успело доехать:

```csharp
public static IReadOnlyList<Write> LostOnFailover(IEnumerable<Write> writes, TimeSpan failureAt, TimeSpan lag) =>
    [.. writes.Where(w => w.At <= failureAt && w.At > failureAt - lag)];
```

Время простоя складывается из обнаружения, принятия решения, повышения реплики и перенаправления клиентов. В тесте 1 + 10 + 3 + 1 минута дают 15 минут, и больше всего занимает решение людей. Файл: [Ex01_Recovery.cs](code/Examples/Ex01_Recovery.cs).

## Стратегии

| Стратегия | Что работает в резервном регионе | Цена и скорость |
|---|---|---|
| Backup and restore | только копии данных | дешевле всего, восстанавливаться дольше |
| Pilot light | данные реплицируются, приложения выключены | нужно включить и расширить |
| Warm standby | уменьшенная, но рабочая копия | нужно только расширить |
| Multi-site active/active | все регионы обслуживают трафик | дороже и сложнее всего |

Конкретные RPO и RTO зависят от системы: их измеряют на учениях.

## Реплика не заменяет копию

Случайное удаление реплицируется так же исправно, как и всё остальное. Восстановление на момент времени останавливается до ошибки:

```csharp
foreach (Write w in log.Where(w => w.At <= moment).OrderBy(w => w.At))
    if (w.Value is null) state.Remove(w.Key);
    else state[w.Key] = w.Value;
```

## Ошибки новичка

- ❌ «У нас есть реплика, копии не нужны» → ✅ нужны обе.
- ❌ Копии в том же регионе и учётной записи → ✅ отдельно и с защитой от удаления.
- ❌ Не проверять восстановление → ✅ регулярно разворачивать из копии.
- ❌ Один RTO для всего → ✅ по критичности функций.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Репликация и партиционирование](../06-replication-and-partitioning/overview.ru.md)

## Источники

- [AWS: стратегии disaster recovery](https://docs.aws.amazon.com/whitepapers/latest/disaster-recovery-workloads-on-aws/disaster-recovery-options-in-the-cloud.html)
- [Azure Well-Architected: disaster recovery](https://learn.microsoft.com/azure/well-architected/reliability/disaster-recovery)
