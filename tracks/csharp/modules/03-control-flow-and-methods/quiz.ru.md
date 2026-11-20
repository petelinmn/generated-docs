---
title: "Квиз: Управляющие конструкции и методы"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 03-control-flow-and-methods
part: 1
tags: [control-flow, switch-expression, loops, ref-out-in, overloading, closures]
status: verified
updated: 2026-11-20
---

# Квиз: Управляющие конструкции и методы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что произойдёт, если в `switch`-операторе секция `case 1:` содержит код и не заканчивается `break`?

- A) Выполнение «провалится» в следующую секцию, как в C
- B) Ошибка компиляции
- C) Предупреждение компилятора, код выполнится
- D) Исключение во время выполнения

<details><summary>Ответ</summary>

**B.** В C# нет неявного проваливания: секция с кодом должна заканчиваться оператором, после которого выполнение не дойдёт до её конца: `break`, `return`, `throw`, `goto case`/`goto default`, `continue` внутри цикла. Пустые метки подряд (`case 1: case 2:`) разрешены.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что вернёт `Classify(20)`?

```csharp
public static string Classify(int celsius) => celsius switch
{
    < 0 => "freezing",
    >= 0 and < 20 => "cold",
    >= 20 and < 30 => "warm",
    _ => "hot",
};
```

- A) `cold`
- B) `warm`
- C) `hot`
- D) `SwitchExpressionException`

<details><summary>Ответ</summary>

**B.** `20` не подходит под `< 20`, но подходит под `>= 20 and < 30`. Ветки проверяются по порядку, срабатывает первая подходящая.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Какой цикл гарантированно выполнит тело хотя бы один раз?

- A) `for`
- B) `while`
- C) `do … while`
- D) `foreach`

<details><summary>Ответ</summary>

**C.** `do … while` проверяет условие после тела. Поэтому `CountDigits(0)` в примерах возвращает `1`.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что вернёт `Greet(greeting: "Hi", name: "Bob")` для `Greet(string name, string greeting = "Hello")`?

- A) `Hello, Bob!`
- B) `Hi, Bob!`
- C) `Bob, Hi!`
- D) Ошибка компиляции: порядок аргументов не совпадает

<details><summary>Ответ</summary>

**B.** Именованные аргументы можно передавать в любом порядке, они сопоставляются по имени параметра.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟢 Что вернёт `FindFirst(grid, 8)` (C# 15)?

```csharp
int[][] grid = [[1, 2, 3], [4, -5, 6], [7, 8, 5]];
```

`FindFirst` обходит строки и столбцы двумя вложенными циклами. При совпадении он выполняет `found = (row, column); break outer;`, где `outer:` — метка внешнего цикла.

- A) `(2, 1)`
- B) `(2, 2)`
- C) `null`
- D) Ошибка компиляции: `break` не принимает метку

<details><summary>Ответ</summary>

**A.** `8` стоит в строке 2, столбце 1. `break outer` (C# 15) сразу завершает внешний цикл, поэтому поиск останавливается на первом совпадении. До C# 15 был бы верен вариант D: `break` с меткой появился только в C# 15.
→ [Базовая теория](basics.ru.md)

</details>

## 6. 🟡 Что вернут методы?

```csharp
var getters = new List<Func<int>>();
for (int i = 1; i <= 3; i++) getters.Add(() => i);
// A: getters.Select(g => g())

var getters2 = new List<Func<int>>();
foreach (int i in new[] { 1, 2, 3 }) getters2.Add(() => i);
// B: getters2.Select(g => g())
```

- A) A = `[1, 2, 3]`, B = `[1, 2, 3]`
- B) A = `[4, 4, 4]`, B = `[1, 2, 3]`
- C) A = `[3, 3, 3]`, B = `[3, 3, 3]`
- D) A = `[4, 4, 4]`, B = `[4, 4, 4]`

<details><summary>Ответ</summary>

**B.** Лямбда захватывает переменную. В `for` переменная `i` одна на весь цикл, и после выхода она равна 4. В `foreach` с C# 5 переменная новая на каждой итерации.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Какая перегрузка будет вызвана для `Describe('a')`, если есть `Describe(int)`, `Describe(long)`, `Describe(double)` и `Describe(object)`?

- A) `object` — `char` не число
- B) `int`
- C) `long`
- D) Ошибка компиляции: неоднозначный вызов

<details><summary>Ответ</summary>

**B.** `char` неявно преобразуется в `int`, `long` и `double`. Из них выигрывает `int`: он неявно преобразуется в `long` и `double`, а обратно — нет. Проверено тестом в [Ex05_Overloading](code/Examples/Ex05_Overloading.cs).
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Выбери все верные утверждения о модификаторах параметров

- [ ] A) Метод с параметром `out` обязан присвоить ему значение до возврата
- [ ] B) Аргумент `ref` должен быть инициализирован до вызова
- [ ] C) Параметр `in` можно изменять внутри метода
- [ ] D) `params ReadOnlySpan<int>` (C# 13) позволяет вызывать метод как `M(1, 2, 3)`

<details><summary>Ответ</summary>

**A, B, D.** `in` — ссылка только для чтения, изменять параметр нельзя (C неверно). `params` с C# 13 работает с коллекциями, включая span.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Найди ошибку

```csharp
public static void RemoveNegatives(List<int> values)
{
    foreach (int value in values)
    {
        if (value < 0)
        {
            values.Remove(value);
        }
    }
}
```

<details><summary>Ответ</summary>

Изменение `List<T>` во время обхода `foreach` делает перечислитель недействительным: следующий `MoveNext` бросает `InvalidOperationException`. Исправление — `values.RemoveAll(v => v < 0)` или цикл `for` по индексу с конца.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🟡 Что произойдёт при вызове `Math.Abs(int.MinValue)`?

- A) Вернёт `int.MaxValue`
- B) Вернёт `int.MinValue`
- C) Бросит `OverflowException`
- D) Вернёт `2147483648` типа `long`

<details><summary>Ответ</summary>

**C.** Диапазон `int` — от −2147483648 до 2147483647: у минимального значения нет положительной пары. `Math.Abs` в этом случае бросает `OverflowException`, это проверено тестом `DigitSum(int.MinValue)`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 11. 🔴 Что произойдёт при вызове `Action((TrafficLight)42)`?

```csharp
public enum TrafficLight { Red, Yellow, Green }

public static string Action(TrafficLight light) => light switch
{
    TrafficLight.Red => "stop",
    TrafficLight.Yellow => "wait",
    TrafficLight.Green => "go",
};
```

- A) Ошибка компиляции: `42` — не значение `TrafficLight`
- B) Вернёт `null`
- C) Бросит `SwitchExpressionException`
- D) Вернёт `"go"` — последнюю ветку

<details><summary>Ответ</summary>

**C.** Приведение `(TrafficLight)42` допустимо: `enum` может хранить любое значение своего базового типа. Компилятор предупреждает (CS8524) и генерирует ветку, которая бросает `SwitchExpressionException`. Лучше явно написать `_ => throw new ArgumentOutOfRangeException(…)`.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Что нужно типу, чтобы его можно было обходить в `foreach`?

- A) Реализовать `IEnumerable<T>`
- B) Реализовать `IEnumerable` или `IEnumerable<T>`
- C) Иметь доступный метод `GetEnumerator()`, результат которого имеет `MoveNext()` и `Current`
- D) Быть массивом или коллекцией из `System.Collections`

<details><summary>Ответ</summary>

**C.** `foreach` работает по шаблону, интерфейс не обязателен. Так устроены `List<T>.Enumerator` (структура, без аллокаций) и пример [Ex10_DuckTypedForeach](code/Examples/Ex10_DuckTypedForeach.cs).
→ [Экспертная теория](expert.ru.md)

</details>

## 13. 🔴 Открытый вопрос

Библиотека v1 содержит `public static string Greet(string name, string greeting = "Hello")`. В v2 значение по умолчанию поменяли на `"Hi"` и обновили DLL у приложения без перекомпиляции. Что выведет `Greet("Ann")` в приложении и почему?

<details><summary>Ключевые пункты ответа</summary>

- Выведет `Hello, Ann!`: значение по умолчанию подставляется в код вызывающей сборки при её компиляции.
- Приложение передаёт `"Hello"` явно, новая DLL этот аргумент просто получает.
- После перекомпиляции приложения будет `Hi, Ann!`.
- В публичных API безопаснее добавить перегрузку, чем менять значение по умолчанию.

</details>
