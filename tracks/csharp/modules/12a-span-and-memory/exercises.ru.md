---
title: "Упражнения: Span и Memory"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 12a-span-and-memory
part: 2
tags: [span, readonly-span, stackalloc, ref-struct, enumerator, zero-allocation]
status: draft
updated: 2026-11-22
---

# Упражнения: Span и Memory

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Подсчёт слов

**Задача:** напиши `static int CountWords(ReadOnlySpan<char> text)`: считает слова (последовательности не-пробельных символов) без выделений памяти.

**Ожидаемый результат:**

```text
CountWords("  hello   big world ") → 3
CountWords("")                     → 0
CountWords("   ")                  → 0
```

<details><summary>Подсказка</summary>

Пройди по символам и запомни, находишься ли ты внутри слова. Новое слово начинается, когда встретился не-пробел, а до этого был пробел (или начало).

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: count words without allocating.
static int CountWords(ReadOnlySpan<char> text)
{
    int count = 0;
    bool inWord = false;
    foreach (char c in text)
    {
        bool isSpace = char.IsWhiteSpace(c);
        if (!isSpace && !inWord) count++;
        inWord = !isSpace;
    }
    return count;
}
```

Проверено на трёх примерах. Строка неявно приводится к `ReadOnlySpan<char>`, поэтому вызывающему не нужно ничего менять.

</details>

## 2. 🟢 Сумма чисел из CSV

**Задача:** напиши `static int SumCsv(ReadOnlySpan<char> csv)`: числа разделены запятыми, вокруг них могут быть пробелы, пустые элементы пропускаются. Без `string.Split` и `Substring`.

**Ожидаемый результат:**

```text
SumCsv("1, 2,3 ,,4") → 10
SumCsv("")           → 0
```

<details><summary>Подсказка</summary>

`csv.Split(',')` на `ReadOnlySpan<char>` (.NET 9+) возвращает диапазоны: `csv[range]` — срез. `Trim()` на span тоже без выделений.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: parse a CSV line with spans only.
static int SumCsv(ReadOnlySpan<char> csv)
{
    int sum = 0;
    foreach (var range in csv.Split(','))
    {
        var part = csv[range].Trim();
        if (!part.IsEmpty) sum += int.Parse(part);
    }
    return sum;
}
```

Проверено: `10` и `0`. `int.Parse` принимает `ReadOnlySpan<char>`.

</details>

## 3. 🟡 Палиндром без копий

**Задача:** напиши `static bool IsPalindrome(ReadOnlySpan<char> s)`: игнорирует регистр и все символы, кроме букв и цифр. Не создавай новых строк и массивов.

**Ожидаемый результат:**

```text
"A man, a plan, a canal: Panama" → True
"hello"                          → False
""                               → True
```

<details><summary>Подсказка</summary>

Два индекса, `i` с начала и `j` с конца; на каждом шаге пропускай не-алфавитно-цифровые символы и сравнивай `char.ToLowerInvariant`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: two-pointer palindrome check over a span.
static bool IsPalindrome(ReadOnlySpan<char> s)
{
    int i = 0, j = s.Length - 1;
    while (i < j)
    {
        while (i < j && !char.IsLetterOrDigit(s[i])) i++;
        while (i < j && !char.IsLetterOrDigit(s[j])) j--;
        if (char.ToLowerInvariant(s[i++]) != char.ToLowerInvariant(s[j--])) return false;
    }
    return true;
}
```

Проверено на трёх примерах. Решение O(n) по времени и O(1) по памяти.

</details>

## 4. 🟡 Шестнадцатеричная строка

**Задача:** напиши `static string ToHex(ReadOnlySpan<byte> bytes)`: строка из двух строчных hex-цифр на байт. Для небольших входов (до 64 байт) используй `stackalloc`, иначе — массив.

**Ожидаемый результат:**

```text
ToHex([0, 15, 255, 16]) → "000fff10"
```

<details><summary>Подсказка</summary>

`Span<char> buf = bytes.Length <= 64 ? stackalloc char[bytes.Length * 2] : new char[bytes.Length * 2];` и `bytes[i].TryFormat(buf.Slice(i * 2, 2), out _, "x2")`. В конце — `new string(buf)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: hex string with a stack buffer for small inputs.
static string ToHex(ReadOnlySpan<byte> bytes)
{
    Span<char> buf = bytes.Length <= 64 ? stackalloc char[bytes.Length * 2] : new char[bytes.Length * 2];
    for (int i = 0; i < bytes.Length; i++)
        bytes[i].TryFormat(buf.Slice(i * 2, 2), out _, "x2");
    return new string(buf);
}
```

Проверено: `000fff10`. В реальном коде есть готовый `Convert.ToHexStringLower` (.NET 9+); упражнение показывает приём «стек для малого, куча для большого».

</details>

## 5. 🔴 Свой перечислитель строк

**Задача:** напиши `ref struct LineReader`, который обходит строки в `ReadOnlySpan<char>` через `foreach` без выделений: разделитель `\n`, завершающий `\r` отбрасывается. Затем напиши `static bool TryParseKv(ReadOnlySpan<char> s, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)` для строк вида `key = value`.

**Ожидаемый результат:**

```text
foreach (var l in new LineReader("a\nbb\r\nccc")) → <a><bb><ccc>
TryParseKv("key = value", …)                      → True, key = "key", value = "value"
TryParseKv("novalue", …)                          → False
```

<details><summary>Подсказка</summary>

`foreach` работает с любым типом, у которого есть `GetEnumerator()` с `MoveNext()` и `Current`. Сам `LineReader` может быть своим перечислителем: `GetEnumerator()` возвращает копию. Поля — `ReadOnlySpan<char>`, поэтому тип — `ref struct`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: a ref struct enumerator over lines and a span-returning parser.
ref struct LineReader
{
    private ReadOnlySpan<char> _rest;
    private ReadOnlySpan<char> _current;
    public LineReader(ReadOnlySpan<char> text) { _rest = text; _current = default; }
    public readonly ReadOnlySpan<char> Current => _current;
    public LineReader GetEnumerator() => this;
    public bool MoveNext()
    {
        if (_rest.IsEmpty) return false;
        int i = _rest.IndexOf('\n');
        ReadOnlySpan<char> line;
        if (i < 0) { line = _rest; _rest = default; }
        else { line = _rest[..i]; _rest = _rest[(i + 1)..]; }
        _current = line.TrimEnd('\r');
        return true;
    }
}

static bool TryParseKv(ReadOnlySpan<char> s, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
{
    int i = s.IndexOf('=');
    if (i < 0) { key = value = default; return false; }
    key = s[..i].Trim(); value = s[(i + 1)..].Trim();
    return true;
}
```

Проверено: `<a><bb><ccc>`, `True key|value`, `False`. Для готового решения есть `EnumerateLines()`, но упражнение показывает, как устроен перечислитель на `ref struct`: поля `Span` допустимы только в `ref struct`.

</details>
