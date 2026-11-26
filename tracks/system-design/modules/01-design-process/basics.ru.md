---
title: "Процесс проектирования и оценки: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 01-design-process
part: 1
tags: [system-design, capacity-estimation, slo, availability, latency, percentiles]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Процесс проектирования и оценки: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** базовый C# · **Время:** ~2 ч

## Зачем это нужно

Фраза «сделайте чат» не определяет систему. Чат на 100 человек — это один процесс и одна база. Чат на 10 миллионов — очереди, шардирование и несколько дата-центров. Разницу задают числа, поэтому проектирование начинается с них.

## Порядок проектирования

1. **Требования.** Функциональные: отправить сообщение, прочитать историю. Нефункциональные: задержка, доступность, объём данных, безопасность, бюджет. Явно назовите, что в объём *не* входит.
2. **Оценки.** Запросы в секунду, объём хранения, трафик.
3. **API и модель данных.** Какие операции и какие сущности.
4. **Схема верхнего уровня.** Клиент, балансировщик, сервисы, хранилища, очереди.
5. **Узкие места.** Что сломается первым и что делать.
6. **Компромиссы.** Что выбрали, от чего отказались и почему.

## Оценки на салфетке

Достаточно знать несколько чисел: в сутках 86 400 секунд (≈10⁵), 1 миллион запросов в день — это около 12 в секунду. Пиковую нагрузку получают умножением средней на коэффициент пика; это допущение, и его нужно назвать.

```csharp
public static Capacity Estimate(Workload w)
{
    double writesPerDay = w.DailyActiveUsers * w.WritesPerUserPerDay;
    double averageWriteQps = writesPerDay / SecondsPerDay;
    double peakWriteQps = averageWriteQps * w.PeakFactor;
    double peakReadQps = peakWriteQps * w.ReadsPerWrite;

    // Replicas multiply disk usage, not the incoming traffic.
    long storage = (long)(writesPerDay * w.BytesPerWrite * w.RetentionDays * w.ReplicationFactor);

    return new Capacity(averageWriteQps, peakWriteQps, peakReadQps, storage, peakReadQps * w.BytesPerWrite);
}
```

Полный файл: [Ex01_CapacityEstimate.cs](code/Examples/Ex01_CapacityEstimate.cs). Для чата с 10 млн DAU, 20 сообщениями на пользователя, 500 байтами на сообщение, пиком ×3, отношением чтения к записи 5:1, хранением 365 дней и тремя репликами получается:

| Величина | Значение |
|---|---|
| Средняя запись | ≈2 315 в секунду |
| Пиковая запись | ≈6 944 в секунду |
| Пиковое чтение | ≈34 722 в секунду |
| Хранение за год с репликами | 109,5 ТБ |
| Пиковый исходящий трафик | ≈17,4 МБ/с |

Выводы важнее цифр: 100 ТБ не поместятся на один обычный сервер базы, значит понадобится шардирование; чтений в 5 раз больше записей, значит кэш и реплики чтения имеют смысл.

## Закон Литтла

Число запросов, находящихся в системе, равно интенсивности входящего потока, умноженной на время обработки. При 2000 запросов в секунду и задержке 200 мс одновременно обрабатывается 400 запросов. Так оценивают размер пула соединений и количество экземпляров.

```csharp
public static double InFlight(double requestsPerSecond, TimeSpan latency) =>
    requestsPerSecond * latency.TotalSeconds;
```

## SLI, SLO и SLA

| Термин | Что это | Пример |
|---|---|---|
| SLI (service level indicator) | измеряемый показатель | доля успешных запросов |
| SLO (service level objective) | целевое значение SLI | 99,9% за 30 дней |
| SLA (service level agreement) | договор с последствиями за нарушение | компенсация при доступности ниже 99,5% |

SLO обычно строже SLA, чтобы заметить проблему раньше, чем она станет нарушением договора.

## Доступность

| Цель | Допустимый простой за 30 дней | За 365 дней |
|---|---|---|
| 99% | 432 мин | 5 256 мин |
| 99,9% | 43,2 мин | 525,6 мин |
| 99,99% | 4,32 мин | 52,56 мин |
| 99,999% | 0,43 мин | 5,26 мин |

Если запросу нужны все компоненты, их доступности перемножаются. Если достаточно любой из реплик, перемножаются вероятности отказа.

```csharp
// All components are required: probabilities multiply.
public static double Serial(params double[] parts) => parts.Aggregate(1.0, (acc, a) => acc * a);

// Any one replica is enough. Valid only when failures are independent.
public static double Parallel(params double[] replicas) =>
    1.0 - replicas.Aggregate(1.0, (acc, a) => acc * (1.0 - a));
```

Три зависимости по 99,9% подряд дают 99,70%. Две независимые реплики по 99% дают 99,99%. Файл: [Ex02_Availability.cs](code/Examples/Ex02_Availability.cs).

## Ошибки новичка

- ❌ Рисовать схему до выяснения требований → ✅ сначала вопросы и допущения.
- ❌ Считать только среднюю нагрузку → ✅ проектировать под пик.
- ❌ «Нужна доступность 100%» → ✅ выбрать цель и посчитать её цену.
- ❌ Добавлять компоненты «на вырост» → ✅ каждый компонент должен решать названную проблему.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Масштабирование и балансировка](../02-scalability-and-load-balancing/overview.ru.md)

## Источники

- [Google SRE Book: Service Level Objectives](https://sre.google/sre-book/service-level-objectives/)
