---
title: "Упражнения: разбор задач"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 17-design-case-studies
part: 3
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: разбор задач

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Длина кода

**Задача:** сколько символов base62 нужно для `count` разных кодов.

**Ожидаемый результат:**

```text
62 -> 1; 63 -> 2; 3_500_000_000_000 -> 7
```

<details><summary>Подсказка</summary>

Умножайте ёмкость на 62.

</details>

<details><summary>Решение</summary>

```csharp
int length = 1;
for (long capacity = 62; capacity < count; capacity *= 62) length++;
return length;
```

Для значений у верхней границы `long` умножение переполнится; в реальных задачах до этого далеко. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Декодирование пользовательского кода

**Задача:** декодировать base62, отклоняя чужие символы, пустую строку и переполнение.

**Ожидаемый результат:**

```text
"10" -> 62; "AzL8n0Y58m7" -> long.MaxValue; "AzL8n0Y58m8", "ab-", "" -> false
```

<details><summary>Подсказка</summary>

Проверяйте переполнение до умножения.

</details>

<details><summary>Решение</summary>

```csharp
int digit = Alphabet.IndexOf(c);
if (digit < 0 || value > (long.MaxValue - digit) / 62)
{
    value = 0;
    return false;
}
value = value * 62 + digit;
```

Это тело цикла по символам. Коды с ведущими нулями дают то же число: если это важно, их отклоняют.

</details>

## 3. 🔴 Вероятность коллизии

**Задача:** приближённая вероятность хотя бы одной коллизии среди `n` случайных кодов из `space`.

**Ожидаемый результат:**

```text
n = 1_000_000, space = 62^7 -> about 0.13
```

<details><summary>Подсказка</summary>

`1 - exp(-n(n-1) / (2 space))`.

</details>

<details><summary>Решение</summary>

```csharp
public static double CollisionProbability(long n, double space) =>
    -double.ExpM1(-(double)n * (n - 1) / (2 * space));
```

`ExpM1` точнее, чем `1 - Math.Exp`, при очень малых вероятностях.

</details>
