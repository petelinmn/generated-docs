---
title: "Упражнения: наблюдаемость и SLO"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: наблюдаемость и SLO

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Когда будить дежурного

**Задача:** вернуть `true`, только если burn rate выше порога в обоих окнах.

**Ожидаемый результат:**

```text
(20, 30) -> true; (20, 1) -> false
```

<details><summary>Подсказка</summary>

Порог по умолчанию 14,4.

</details>

<details><summary>Решение</summary>

```csharp
public static bool ShouldPage(double longWindowBurn, double shortWindowBurn, double threshold = 14.4) =>
    longWindowBurn > threshold && shortWindowBurn > threshold;
```

Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Разбор traceparent

**Задача:** разобрать заголовок версии `00`, отклоняя неверные.

**Ожидаемый результат:**

```text
valid header -> (trace id, parent id, sampled); zeros, upper case, "ff" -> null
```

<details><summary>Подсказка</summary>

Четыре части, нижний регистр, не все нули.

</details>

<details><summary>Решение</summary>

```csharp
string[] p = header.Split('-');
if (p.Length != 4 || p[0] != "00") return null;
if (!Valid(p[1], 32) || !Valid(p[2], 16) || p[3].Length != 2 || p[3].AsSpan().ContainsAnyExcept(Hex)) return null;
if (p[1].All(c => c == '0') || p[2].All(c => c == '0')) return null;
```

Упрощение: спецификация требует разбирать и будущие версии по первым 55 символам. В приложении это делает платформа.

</details>

## 3. 🔴 Согласованное сэмплирование

**Задача:** по trace id и доле принять решение, одинаковое во всех сервисах.

**Ожидаемый результат:**

```text
the same id -> the same answer; about 10% of random ids at ratio 0.1
```

<details><summary>Подсказка</summary>

Используйте часть самого идентификатора как число.

</details>

<details><summary>Решение</summary>

```csharp
ulong value = ulong.Parse(traceId.AsSpan(traceId.Length - 16), NumberStyles.HexNumber);
return value / (double)ulong.MaxValue < ratio;
```

Работает при равномерно случайных идентификаторах.

</details>
