---
title: "Упражнения: процесс проектирования и оценки"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 01-design-process
part: 1
tags: [system-design, capacity-estimation, slo, availability, latency, percentiles]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: процесс проектирования и оценки

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Сколько нужно серверов

**Задача:** по пиковому QPS, пропускной способности сервера, целевой загрузке и числу запасных серверов вычислить размер парка.

**Ожидаемый результат:**

```text
7000 QPS, 1000 QPS/server, utilization 0.6, 2 spares -> 14
```

<details><summary>Подсказка</summary>

Полезная мощность сервера — не вся его пропускная способность. Дробный сервер округляется вверх.

</details>

<details><summary>Решение</summary>

```csharp
public static int ServersNeeded(double peakQps, double qpsPerServer, double targetUtilization, int spares) =>
    (int)Math.Ceiling(peakQps / (qpsPerServer * targetUtilization)) + spares;
```

Запас добавляется после округления, чтобы отказ или выкатка не перегрузили остальных. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Сколько нужно реплик

**Задача:** найти минимальное число независимых реплик с доступностью `single`, чтобы достичь `target`. Недостижимые значения отклонять.

**Ожидаемый результат:**

```text
(0.99, 0.999) -> 2
(0.9, 0.9995) -> 4
```

<details><summary>Подсказка</summary>

Увеличивайте `n`, пока `1 - (1 - single)^n` меньше цели. Помните о погрешности `double`.

</details>

<details><summary>Решение</summary>

```csharp
public static int ReplicasFor(double single, double target)
{
    if (single is <= 0 or >= 1) throw new ArgumentOutOfRangeException(nameof(single));
    if (target is < 0 or >= 1) throw new ArgumentOutOfRangeException(nameof(target));

    int n = 1;
    // The tolerance absorbs floating-point error when the target is hit exactly.
    while (1 - Math.Pow(1 - single, n) < target - 1e-12) n++;
    return n;
}
```

Цель 100% недостижима конечным числом реплик, поэтому это ошибка аргумента.

</details>

## 3. 🔴 Burn rate

**Задача:** вычислить burn rate по SLO, общему числу запросов и числу неудач.

**Ожидаемый результат:**

```text
(0.999, 10000, 100) -> 10
```

<details><summary>Подсказка</summary>

Разделите наблюдаемую долю ошибок на допустимую. Подумайте о нуле запросов.

</details>

<details><summary>Решение</summary>

```csharp
public static double BurnRate(double slo, long total, long failed) =>
    total == 0 ? 0 : (double)failed / total / (1 - slo);
```

Приведение к `double` обязательно: целочисленное деление дало бы ноль.

</details>
