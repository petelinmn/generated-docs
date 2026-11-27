---
title: "Упражнения: сокращатель ссылок"
lang: ru
track: system-design
kind: recipe
doc: exercises
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# Упражнения: сокращатель ссылок

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения лежат в `code/Solutions/` и покрыты тестами.

## 1. 🟢 Контрольный символ

**Задача:** добавить к коду символ, чтобы опечатка обнаруживалась без обращения к базе.

<details><summary>Подсказка</summary>

Умножайте значения на позицию.

</details>

<details><summary>Решение</summary>

```csharp
private static char Checksum(string body)
{
    int sum = 0;
    for (int i = 0; i < body.Length; i++)
    {
        int value = Alphabet.IndexOf(body[i]);
        if (value < 0) return '\0';
        // Position weights make most swaps of neighbours visible.
        sum = (sum + value * (i + 1)) % Alphabet.Length;
    }

    return Alphabet[sum];
```

Ловит большинство одиночных опечаток и перестановок, но не все: модуль 62 не простой. Файл: [Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Идемпотентное создание

**Задача:** один владелец, повторно отправивший тот же адрес, получает тот же код.

<details><summary>Подсказка</summary>

Ключ — владелец и адрес.

</details>

<details><summary>Решение</summary>

```csharp
public string Create(string owner, string url)
{
    // The separator cannot appear in an owner id, so different pairs do not merge.
    string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{owner}\n{url}")));
    lock (_gate)
    {
        if (!_codes.TryGetValue(key, out string? code))
            _codes[key] = code = nextCode();
        return code;
```

В сервисе ключ нужно хранить с уникальным индексом, а не в памяти.

</details>

## 3. 🔴 Выбор длины

**Задача:** посчитать вероятность коллизии при миллиарде ссылок и 7 символах.

<details><summary>Подсказка</summary>

Число сохранённых, делённое на размер пространства.

</details>

<details><summary>Решение</summary>

```csharp
public static class Keyspace
{
    public static double Size(int length) => Math.Pow(62, length);

    public static double CollisionChance(long stored, int length) => stored / Size(length);
}
```

Получается около 0,00028, то есть 0,028% на каждый новый код.

</details>
