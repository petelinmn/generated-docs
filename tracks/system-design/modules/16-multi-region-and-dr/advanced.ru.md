---
title: "Несколько регионов и восстановление: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Несколько регионов и восстановление: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Failover и failback

```csharp
if (Active == _primary && _failures >= failAfter) Active = secondary;
// Returning needs a longer streak: flapping is worse than staying on the secondary.
else if (Active == secondary && _successes >= recoverAfter) Active = _primary;
```

Переключаются после серии неудач, возвращаются после более длинной серии успехов. Файл: [Ex02_Failover.cs](code/Examples/Ex02_Failover.cs). Пример управляет только маршрутом: перед возвратом настоящей базы нужно ещё перенести данные, записанные на резерве.

## Кто решает

Автоматика быстрее, но может переключиться из-за разрыва связи и получить двух лидеров. Нужен кворум из третьей точки наблюдения и [fencing](../12-consensus-and-coordination/basics.ru.md) старого лидера.

## Active-active

| Подход | Идея |
|---|---|
| Домашний регион пользователя | все записи пользователя идут в один регион |
| Глобальный лидер на запись | чтения локальны, записи — в один регион |
| Запись везде | нужны правила слияния |

Last write wins теряет изменения: корзина `book, pen` и более поздняя `book, mug` дают результат без `pen`. Observed-remove set удаляет только те добавления, которые видел:

```csharp
public void Remove(string item)
{
    foreach (var (_, tag) in _added.Where(a => a.Item == item)) _removed.Add(tag);
}
```

Файл: [Ex03_ActiveActive.cs](code/Examples/Ex03_ActiveActive.cs).

## Учения

Проводите восстановление по расписанию, измеряйте фактические RPO и RTO, записывайте, что пошло не так.

## Лучшие практики

- Инфраструктуру описывайте кодом, чтобы разворачивать регион заново.
- Проверяйте квоты и мощности в резервном регионе заранее.
- Следите за лагом репликации и алертите по порогу RPO.
- Держите инструкцию переключения доступной без основного региона.

## Частые ошибки

- ❌ Резерв зависит от сервиса в основном регионе → ✅ проверить все зависимости.
- ❌ DNS с долгим TTL → ✅ учитывать кэширование при расчёте RTO.
- ❌ Failback без плана → ✅ он такой же отдельный процесс.
- ❌ Забытые секреты и конфигурация в резерве → ✅ реплицировать и их.

## Сравнение с другими треками

- Связь брокеров между площадками — [RabbitMQ](../../../rabbitmq/modules/12-federation-and-shovel/overview.ru.md).
- Развёртывание из репозитория — [GitOps](../../../kubernetes/modules/20-gitops/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [AWS: стратегии disaster recovery](https://docs.aws.amazon.com/whitepapers/latest/disaster-recovery-workloads-on-aws/disaster-recovery-options-in-the-cloud.html)
