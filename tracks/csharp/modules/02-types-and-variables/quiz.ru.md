---
title: "Квиз: Типы и переменные"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 02-types-and-variables
part: 1
tags: [types, value-types, reference-types, nullable-reference-types, boxing, floating-point]
status: verified
updated: 2026-11-20
---

# Квиз: Типы и переменные

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какие из типов — значимые (value types)?

- [ ] A) `int`
- [ ] B) `string`
- [ ] C) `DateTime` (структура)
- [ ] D) `int[]`

<details><summary>Ответ</summary>

**A, C.** Числовые типы и структуры — значимые. `string` и массивы (даже массив `int`) — ссылочные.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что вернёт метод?

```csharp
var original = new PointStruct { X = 1 };
var copy = original;
copy.X = 99;
return (original.X, copy.X);
```

`PointStruct` — структура.

- A) `(1, 99)`
- B) `(99, 99)`
- C) `(1, 1)`
- D) Ошибка компиляции

<details><summary>Ответ</summary>

**A.** Присваивание структуры копирует значение целиком, поэтому `copy` — независимая копия. Если бы `PointStruct` был классом, результат был бы `(99, 99)`.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Чему равно `(int)2.9`?

- A) `3`
- B) `2`
- C) `2.9`
- D) Ошибка компиляции: нужен `Convert.ToInt32`

<details><summary>Ответ</summary>

**B.** Явное приведение `double` → `int` отбрасывает дробную часть (к нулю): `(int)-2.9` → `-2`. Округляет `Math.Round`.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что произойдёт при `int.MaxValue + 1` (значения из переменных, проект с настройками по умолчанию)?

- A) `OverflowException`
- B) Результат станет `long`
- C) Результат «завернётся» в `int.MinValue`
- D) Результат будет `int.MaxValue`

<details><summary>Ответ</summary>

**C.** По умолчанию арифметика целых не проверяется (unchecked), и переполнение молча заворачивается. Исключение бросает только `checked(...)` или настройка проекта `CheckForOverflowUnderflow`.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что вернёт `0.1 + 0.2 == 0.3` для `double` и `0.1m + 0.2m == 0.3m` для `decimal`?

- A) `true` и `true`
- B) `false` и `true`
- C) `false` и `false`
- D) `true` и `false`

<details><summary>Ответ</summary>

**B.** `double` двоичный, и 0.1 в нём не представимо точно: сумма равна `0.30000000000000004`. `decimal` хранит десятичные цифры, поэтому сумма точная.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что вернёт метод?

```csharp
object a = 42;
object b = 42;
return (ReferenceEquals(a, b), a.Equals(b), a == b);
```

- A) `(true, true, true)`
- B) `(false, true, true)`
- C) `(false, true, false)`
- D) `(false, false, false)`

<details><summary>Ответ</summary>

**C.** Каждое присваивание `int` в `object` создаёт новую «коробку» (boxing). Ссылки разные, поэтому `ReferenceEquals` и `==` (для `object` он сравнивает ссылки) дают `false`. `Equals` сравнивает значения и даёт `true`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Найди ошибку

```csharp
object boxed = 42;
long value = (long)boxed;
```

<details><summary>Ответ</summary>

В коробке лежит `int`, а распаковывают его как `long`. Unboxing требует точного типа, поэтому код бросит `InvalidCastException`. Исправление: `long value = (long)(int)boxed;` — сначала распаковать как `int`, потом расширить до `long`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Выбери все верные утверждения о nullable reference types

- [ ] A) `string?` и `string` — разные типы во время выполнения
- [ ] B) Обращение к членам `string?` без проверки даёт предупреждение CS8602
- [ ] C) Оператор `!` проверяет значение на null во время выполнения
- [ ] D) После `if (text is null) return;` компилятор считает `text` ненулевым

<details><summary>Ответ</summary>

**B, D.** `string?` — аннотация для компилятора, тип в runtime один (A неверно). `!` только подавляет предупреждение, проверки нет (C неверно). Анализ потока понимает проверки на null (D верно).
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Почему `"i".ToUpper()` может вернуть не `"I"`?

- A) `ToUpper` работает только с ASCII
- B) Без аргументов используется текущая культура, а в турецкой культуре заглавная `i` — это `İ`
- C) Строки неизменяемы, поэтому `ToUpper` возвращает исходную строку
- D) Это ошибка .NET

<details><summary>Ответ</summary>

**B.** `ToUpper()` без аргументов зависит от культуры ОС. Для идентификаторов и сравнений используют `ToUpperInvariant()` или `StringComparison.OrdinalIgnoreCase`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Что вернёт метод?

```csharp
string a = "hello";
string b = "hel" + "lo";
string c = prefix + "lo";   // prefix == "hel", known only at run time
return (ReferenceEquals(a, b), ReferenceEquals(a, c), a == c);
```

- A) `(true, true, true)`
- B) `(false, false, true)`
- C) `(true, false, true)`
- D) `(true, false, false)`

<details><summary>Ответ</summary>

**C.** `"hel" + "lo"` — константа, компилятор складывает её в литерал `"hello"`, и литералы интернированы. Конкатенация во время выполнения создаёт новый объект. `==` для `string` сравнивает содержимое.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Какой тип вернёт `((object)(int?)5).GetType()`?

- A) `System.Nullable<System.Int32>`
- B) `System.Int32`
- C) `System.Object`
- D) Бросит `NullReferenceException`

<details><summary>Ответ</summary>

**B.** При упаковке `Nullable<T>` runtime упаковывает внутреннее значение, а «пустой» `int?` превращается в null-ссылку. Упакованного `Nullable<int>` не бывает.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос

Коллега утверждает: «значимые типы всегда хранятся в стеке, поэтому они быстрее». Что не так в этом утверждении?

<details><summary>Ключевые пункты ответа</summary>

- Значение хранится там, где объявлена переменная: поле структуры в классе — в куче, элементы массива — в куче, захваченная лямбдой локальная переменная — в поле скрытого класса в куче.
- Суть различия — семантика: копирование по значению против разделения по ссылке.
- «Быстрее» не всегда: большая структура дорого копируется, а упаковка (boxing) создаёт аллокации.
- Место хранения — деталь реализации, которую JIT может оптимизировать.

</details>
