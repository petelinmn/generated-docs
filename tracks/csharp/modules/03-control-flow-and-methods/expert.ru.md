---
title: "Управляющие конструкции и методы: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 03-control-flow-and-methods
part: 1
tags: [control-flow, switch-expression, pattern-matching, loops, methods, ref-out-in, overloading, local-functions, closures]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Управляющие конструкции и методы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Исчерпывающий switch и SwitchExpressionException

Перечисление (`enum`) — это именованные константы поверх целого типа. Переменная `enum` может содержать **любое** значение этого типа, а не только объявленные:

```csharp
// An enum variable can hold any value of its underlying type: (TrafficLight)42 is legal.
// The compiler warns with CS8524 and emits a throw of SwitchExpressionException for unmatched input.
#pragma warning disable CS8524
public static string Action(TrafficLight light) => light switch
{
    TrafficLight.Red => "stop",
    TrafficLight.Yellow => "wait",
    TrafficLight.Green => "go",
};
#pragma warning restore CS8524

// An explicit discard arm documents what happens with unexpected values.
public static string SafeAction(TrafficLight light) => light switch
{
    TrafficLight.Red => "stop",
    TrafficLight.Yellow => "wait",
    TrafficLight.Green => "go",
    _ => throw new ArgumentOutOfRangeException(nameof(light), light, "Unknown traffic light"),
};
```

- Если switch expression не покрывает все значения, компилятор предупреждает (CS8509, а для неименованных значений `enum` — CS8524) и генерирует ветку, которая бросает `System.Runtime.CompilerServices.SwitchExpressionException`.
- `Action((TrafficLight)42)` бросает `SwitchExpressionException`, `SafeAction((TrafficLight)42)` — понятное `ArgumentOutOfRangeException`.
- Вывод: явная ветка `_ => throw …` лучше, чем подавленное предупреждение. Здесь `#pragma` нужна только для демонстрации.

Код: [Ex08_SwitchExhaustiveness.cs](code/Examples/Ex08_SwitchExhaustiveness.cs).

В C# 15 появились два вида типов, для которых `switch` может быть исчерпывающим без ветки `_`:

- **union types** — значение является одним из нескольких типов-вариантов (case types), и компилятор проверяет, что switch expression покрывает все варианты;
- **closed-иерархии** — класс с модификатором `closed` может иметь наследников только в своей сборке, поэтому компилятор знает их все, и `switch` по такой иерархии не требует ветки по умолчанию.

Оба механизма разбираются в модуле [11 · Современный C#](../11-modern-csharp/overview.ru.md).

## ref-возвраты

Метод может вернуть не значение, а **ссылку на место хранения**:

```csharp
// Returns a reference to the array element itself, so the caller can change it in place.
public static ref int FindFirst(int[] values, int target)
{
    for (int i = 0; i < values.Length; i++)
    {
        if (values[i] == target)
        {
            return ref values[i];
        }
    }
    throw new InvalidOperationException($"{target} not found");
}
```

Вызывающий код: `ref int slot = ref RefReturns.FindFirst(values, 2); slot = 20;` — и массив `[1, 2, 3]` становится `[1, 20, 3]`. Ссылку можно вернуть только на то, что переживёт метод: элемент массива, поле объекта, `ref`-параметр. На локальную переменную нельзя — компилятор не даст. На этом механизме построены `Span<T>` и высокопроизводительные API ([12a · Span и Memory](../12a-span-and-memory/overview.ru.md)). Код: [Ex09_RefReturns.cs](code/Examples/Ex09_RefReturns.cs).

## Как foreach находит перечислитель

`foreach` не требует интерфейса `IEnumerable`. Компилятор ищет **по шаблону** (pattern-based): публичный метод `GetEnumerator()`, у результата которого есть `MoveNext()` и свойство `Current`:

```csharp
// No interfaces at all: foreach finds GetEnumerator, MoveNext and Current by name.
// The enumerator is a struct, so the loop allocates nothing on the heap.
public readonly struct EvenNumbers
{
    private readonly int _max;

    public EvenNumbers(int max) => _max = max;

    public Enumerator GetEnumerator() => new(_max);

    public struct Enumerator
    {
        private readonly int _max;
        private int _current;

        public Enumerator(int max)
        {
            _max = max;
            _current = -2;
        }

        public readonly int Current => _current;

        public bool MoveNext()
        {
            _current += 2;
            return _current <= _max;
        }
    }
}
```

`foreach (int n in new EvenNumbers(7))` даёт `0, 2, 4, 6`. Зачем это нужно:

- `List<T>.Enumerator` — тоже структура, поэтому `foreach` по `List<T>` не создаёт объект в куче. Если привести список к `IEnumerable<T>`, перечислитель будет упакован.
- `foreach` по массиву компилятор превращает в обычный цикл по индексу.
- Так же, по шаблону, работают `await` (`GetAwaiter`) и деконструкция (`Deconstruct`).

Код: [Ex10_DuckTypedForeach.cs](code/Examples/Ex10_DuckTypedForeach.cs). Применить шаблон самостоятельно — [упражнение 4](exercises.ru.md).

## Рекурсия и стек

- Каждый вызов метода занимает кадр стека. Слишком глубокая рекурсия приводит к `StackOverflowException`, а его **нельзя перехватить**: процесс завершается.
- C# не гарантирует оптимизацию хвостовых вызовов, поэтому глубокую рекурсию переписывают в цикл или используют явный стек (`Stack<T>`).
- Мемоизация (как в `Fibonacci`) превращает экспоненциальную рекурсию в линейную, но не уменьшает глубину стека.

## Ловушка: значения по умолчанию компилируются в место вызова

Значение необязательного параметра подставляется **в код вызывающей сборки** при её компиляции. Если библиотека v2 меняет `greeting = "Hello"` на `greeting = "Hi"`, а приложение не перекомпилировали, оно продолжит передавать `"Hello"`. Поэтому в публичных API библиотек вместо смены значения по умолчанию добавляют новую перегрузку.

Так же ведут себя константы `const`: их значение тоже подставляется в код вызывающей сборки — см. [экспертную теорию модуля 02](../02-types-and-variables/expert.ru.md).

## Справочник

### Операторы перехода

| Оператор | Что делает |
|---|---|
| `break` | выходит из ближайшего цикла или `switch` |
| `break метка;` (C# 15) | выходит из цикла или `switch` с этой меткой |
| `continue` / `continue метка;` (C# 15) | переходит к следующей итерации ближайшего или помеченного цикла |
| `return` | выходит из метода |
| `throw` | бросает исключение; с C# 7 может быть выражением: `x ?? throw …`, ветка switch expression |
| `goto метка;` | переход к метке в том же методе |
| `goto case значение;` / `goto default;` | переход к другой секции `switch` |
| `yield return` / `yield break` | в итераторах (разбираются вместе с LINQ, [07 · LINQ](../07-linq/advanced.ru.md#итераторы-и-yield)) |

Пример `goto case`: `switch (1)` с секциями `case 1: … goto case 2;`, `case 2: … goto default;` проходит путь `one>two>default`.

### Оператор switch полностью

- Метки — константы или шаблоны: `case int n when n > 0:`, `case null:`, `case string { Length: 0 }:`. Охранное условие `when` уточняет шаблон.
- Секция с кодом не может «провалиться» в следующую: ошибка CS0163. Несколько меток подряд у одной секции допустимы.
- `default` может стоять в любом месте, выполняется последним по смыслу.
- `switch` по строке сравнивает ординально и с учётом регистра.

### Switch expression полностью

- Ветки `шаблон when условие => выражение`, проверяются по порядку.
- Ветка, которую уже покрыли предыдущие, — ошибка CS8510 (например, `> 10` после `> 0`).
- Неисчерпывающий switch — предупреждение CS8509 (CS8524 — для неименованных значений `enum`) и `SwitchExpressionException` во время выполнения.
- `throw` можно использовать как результат ветки.

### Циклы полностью

| Форма | Примечание |
|---|---|
| `for (init; condition; step)` | все три части необязательны: `for (;;)` — бесконечный цикл |
| `while (condition)` / `do … while (condition);` | проверка до / после тела |
| `foreach (var x in collection)` | по шаблону `GetEnumerator`; по `null` бросает `NullReferenceException` |
| `foreach (ref var x in span)` | изменение элементов `Span<T>` на месте: `[1, 2, 3]` → `[10, 20, 30]` после `item *= 10` |
| `await foreach` | асинхронные потоки ([10 · Async/await](../10-async-await/overview.ru.md)) |
| `метка: for …` + `break метка;` | выход из вложенных циклов (C# 15) |

### Параметры полностью

| Объявление | Передача | Примечание |
|---|---|---|
| `T x` | по значению | копия значения или ссылки |
| `ref T x` | по ссылке | аргумент инициализирован, метод может менять |
| `out T x` | по ссылке | метод обязан присвоить |
| `in T x` | по ссылке, только чтение | для больших структур |
| `ref readonly T x` (C# 12) | по ссылке, только чтение | вызывающий передаёт `ref` или `in`: `ReadOnlyRef(ref number)` и `ReadOnlyRef(in number)` дают одинаковый результат |
| `params T[] x`, `params ReadOnlySpan<T> x` (C# 13) | переменное число аргументов | |
| `T x = значение` | необязательный | значение подставляется в место вызова |
| `scoped ref T x` (C# 11) | ограничивает время жизни ссылки | для `ref struct` и `Span<T>` ([12a · Span и Memory](../12a-span-and-memory/overview.ru.md)) |
| `this T x` | первый параметр метода расширения | [11 · Современный C#](../11-modern-csharp/overview.ru.md) |

Перегрузки, отличающиеся только `ref` и `out`, запрещены (CS0663). Перегрузки «по значению» и «по `ref`» допустимы: `M(1)` вызывает версию по значению, `M(ref field)` — версию с `ref`.

### Атрибуты информации о вызывающем

| Атрибут | Что подставляет компилятор |
|---|---|
| `[CallerMemberName]` | имя вызывающего члена; из top-level statements — `<Main>$` |
| `[CallerFilePath]`, `[CallerLineNumber]` | путь к файлу и номер строки вызова |
| `[CallerArgumentExpression(nameof(arg))]` (C# 10) | текст выражения аргумента: `Check(2 + 2 == 5)` → `failed: 2 + 2 == 5` |

Параметр с таким атрибутом должен быть необязательным. На этом механизме построены `ArgumentNullException.ThrowIfNull` и логирование.

### Аргументы

- Позиционные, именованные и их смесь. Именованный аргумент может стоять не в конце, если он на своей позиции (C# 7.2): `Range(start: 1, 5)` → `1..5`.
- Аргументы и операнды вычисляются **слева направо**: вызов `Three(Log("a", 1), Log("b", 2), Log("c", 3))` записывает порядок `a,b,c`.

### Правила выбора перегрузки

| Ситуация | Что выбирается |
|---|---|
| точное совпадение и преобразование | точное совпадение |
| два преобразования | «лучшая цель»: тип, который неявно преобразуется в другой (`int` лучше `long`) |
| `Pick(int)` и `Pick(params int[])`, вызов `Pick(1)` | версия без `params` |
| `Generic<T>(T)` и `Generic(object)`, вызов `Generic(5)` | обобщённая: `T = int` — точное совпадение, без упаковки |
| `Opt(int)` и `Opt(int, int b = 0)`, вызов `Opt(1)` | версия, которой не нужны значения по умолчанию |
| `M(string?)` и `M(object?)`, вызов `M(null)` | `string` — более конкретный тип |
| `M(string?)` и `M(int[]?)`, вызов `M(null)` | ошибка CS0121: неоднозначный вызов |
| метод экземпляра и метод расширения с той же сигнатурой | метод экземпляра; расширение даже не рассматривается |

### Объявления и операторы из других модулей

Объявления внутри метода: локальные переменные (`var` или явный тип), локальные константы `const`, ref-переменные (`ref int slot = ref …`, см. раздел «ref-возвраты») и `scoped ref`.

| Оператор | Где разбирается |
|---|---|
| `try` / `catch` / `finally`, `throw` | [08 · Исключения](../08-exceptions/overview.ru.md) |
| `checked` / `unchecked` | [02 · Типы и переменные](../02-types-and-variables/basics.ru.md) |
| `yield return` / `yield break` | [07 · LINQ](../07-linq/advanced.ru.md#итераторы-и-yield) |
| `await`, `await foreach`, `await using` | [10 · Async/await](../10-async-await/overview.ru.md) |
| `using`, `fixed`, `unsafe` | [12 · Память и GC](../12-memory-and-gc/expert.ru.md#закрепление-объектов) |
| `lock` | модуль 18 · Многопоточность |

## Подвохи

### foreach по null

```csharp
int[]? none = null;
foreach (var x in none!) { }   // NullReferenceException
```

`foreach` не проверяет коллекцию на `null`. Защита: `foreach (var x in none ?? [])`.

### switch по строке учитывает регистр

`"Yes" switch { "yes" => "affirmative", _ => "unknown" }` даёт `unknown`. Для сравнения без учёта регистра нормализуют вход (`ToLowerInvariant()`) или используют `when string.Equals(…, StringComparison.OrdinalIgnoreCase)`.

### in и изменяемая структура: защитная копия

```csharp
struct Counter
{
    public int Value;
    public void Increment() => Value++;
}

static void IncrementViaIn(in Counter c) => c.Increment();
static void IncrementViaRef(ref Counter c) => c.Increment();
```

После `IncrementViaIn(in counter)` значение по-прежнему `0`, после `IncrementViaRef(ref counter)` — `1`. Параметр `in` нельзя менять, поэтому компилятор вызывает изменяющий метод на **защитной копии** (defensive copy) и молча её выбрасывает. Вывод: `in` — для `readonly struct`, иначе копия ещё и тратит время. Та же ловушка — у `SpinLock` в `readonly`-поле: см. [18 · Многопоточность](../18-concurrency/expert.ru.md).

### params и явный null

`Sum(null!)` для `Sum(params int[] values)` передаёт не пустой массив, а `null`, и `foreach` внутри бросает `NullReferenceException`.

### Обобщённая перегрузка побеждает object

Для `Generic<T>(T value)` и `Generic(object value)` вызов `Generic(5)` выбирает `generic<Int32>`. Обобщённая версия с `T = int` даёт точное совпадение, а `object` потребовал бы упаковки.

### break внутри switch в цикле

```csharp
for (int i = 0; i < 3; i++)
{
    iterations++;
    switch (i)
    {
        case 0:
            break;   // leaves only the switch
    }
}
```

Цикл выполняется 3 раза: `break` относится к `switch`, а не к циклу. С меткой (C# 15) `break loop;` выходит из цикла после первой итерации.

### Счётчик с плавающей точкой

Десять прибавлений `0.1f` дают `1.0000001`, а не `1`. Цикл `for (float f = 0; f != 1; f += 0.1f)` никогда не закончится. Счётчики — целые; условие для дробных — `<`, а не `!=`.

### & и && для bool

`false && Side()` не вызывает `Side`, а `false & Side()` вызывает: после двух выражений счётчик вызовов равен 1. `&` и `|` для `bool` вычисляют оба операнда.

### k++ + ++k

При `k = 1` выражение `k++ + ++k` равно `4`, а `k` становится `3`. C# гарантирует вычисление слева направо: `k++` даёт 1 (и `k` = 2), затем `++k` даёт 3.

### Замыкание видит последующие изменения

```csharp
int captured = 1;
Func<int> read = () => captured;
captured = 2;
read();   // 2
```

Лямбда захватывает переменную, а не её значение на момент создания.

### CallerMemberName в top-level statements

`[CallerMemberName]` при вызове из top-level statements подставляет `<Main>$` — имя метода, который компилятор сгенерировал для них.

## Компромиссы: перегрузки или необязательные параметры

| | Перегрузки | Необязательные параметры |
|---|---|---|
| Количество кода | больше | меньше |
| Смена значения по умолчанию в библиотеке | безопасна | требует перекомпиляции вызывающих |
| Разная логика для разных типов | да | нет |
| Читаемость вызова | зависит от имён | именованные аргументы делают вызов самодокументируемым |

## Вопросы с собеседований

**1. Чем `ref` отличается от `out` и от `in`?**

<details><summary>Ответ</summary>

Все три передают ссылку на переменную. `ref` — аргумент должен быть инициализирован, метод может его менять. `out` — метод обязан присвоить значение, инициализация не нужна. `in` — только чтение, предназначен для больших структур.

</details>

**2. Почему `for` с лямбдами даёт `[4, 4, 4]`, а `foreach` — `[1, 2, 3]`?**

<details><summary>Ответ</summary>

Лямбда захватывает переменную. У `for` одна переменная на весь цикл, после выхода она равна 4. У `foreach` с C# 5 переменная создаётся заново на каждой итерации.

</details>

**3. Как компилятор выбирает перегрузку для `Describe('a')` при `Describe(int)`, `Describe(long)`, `Describe(double)`, `Describe(object)`?**

<details><summary>Ответ</summary>

`char` неявно преобразуется во все числовые варианты. Лучшая цель — `int`: он неявно преобразуется в `long` и `double`, а обратно — нет. `object` требует упаковки и проигрывает любому числовому преобразованию.

</details>

**4. Нужен ли `IEnumerable`, чтобы тип работал в `foreach`?**

<details><summary>Ответ</summary>

Нет. Достаточно доступного `GetEnumerator()`, результат которого имеет `MoveNext()` и `Current`. Так `List<T>` обходится без аллокаций: его перечислитель — структура.

</details>

**5. Что произойдёт при неисчерпывающем switch expression?**

<details><summary>Ответ</summary>

Компилятор выдаст предупреждение CS8509 (или CS8524 для неименованных значений `enum`) и сгенерирует ветку, которая бросает `SwitchExpressionException`. Лучше явная ветка `_ => throw …` с понятным исключением.

</details>

**6. Почему нельзя перехватить `StackOverflowException`?**

<details><summary>Ответ</summary>

При переполнении стека runtime завершает процесс, обработчики `catch` не выполняются. Защита — ограничивать глубину рекурсии, переписывать её в цикл или использовать явный `Stack<T>`.

</details>

**7. Чем опасна смена значения по умолчанию у параметра публичного метода?**

<details><summary>Ответ</summary>

Значение подставляется в код вызывающей сборки при её компиляции. Без перекомпиляции потребители продолжат передавать старое значение. Безопаснее добавить перегрузку.

</details>

**8. 🪤 Почему вызов метода у `in`-параметра не изменил структуру?**

<details><summary>Ответ</summary>

Если структура не `readonly`, компилятор не может доказать, что метод её не меняет, и вызывает его на защитной копии. Изменения теряются. Решения: `readonly struct`, `readonly`-методы или `ref` вместо `in`.

</details>

**9. 🪤 Чему равно `k++ + ++k` при `k = 1`?**

<details><summary>Ответ</summary>

4, а `k` после выражения равно 3. В C# порядок вычисления операндов определён — слева направо, в отличие от C и C++, где такое выражение — неопределённое поведение.

</details>

**10. 🪤 Есть `M<T>(T x)` и `M(object x)`. Что вызовет `M(5)`?**

<details><summary>Ответ</summary>

Обобщённую версию: вывод типов даёт `T = int`, это точное совпадение, а `object` требует упаковки. Многие ожидают `object`.

</details>

**11. Как выйти сразу из двух вложенных циклов?**

<details><summary>Ответ</summary>

С C# 15 — `break метка;` с меткой внешнего цикла. Раньше использовали `goto`, флаг-переменную или выносили циклы в отдельный метод и выходили через `return`.

</details>

**12. Чем `&&` отличается от `&` для `bool`?**

<details><summary>Ответ</summary>

`&&` вычисляет правый операнд, только если левый — `true` (short-circuit). `&` всегда вычисляет оба. Разница важна, когда правый операнд имеет побочный эффект или защищён проверкой: `x != null && x.Length > 0`.

</details>

**13. Можно ли объявить перегрузки `M(ref int x)` и `M(out int x)`?**

<details><summary>Ответ</summary>

Нет, ошибка CS0663: на уровне CLR `ref` и `out` — одно и то же. А `M(int x)` и `M(ref int x)` объявить можно.

</details>

**14. Что такое `[CallerArgumentExpression]` и где он используется?**

<details><summary>Ответ</summary>

Атрибут (C# 10), который передаёт в необязательный параметр текст выражения другого аргумента. Так `ArgumentNullException.ThrowIfNull(user)` знает имя `user`, а самописные проверки печатают условие: `failed: 2 + 2 == 5`.

</details>

## Шпаргалка

| Задача | Как |
|---|---|
| Выбор по значению с результатом | `x switch { 1 => "a", _ => "b" }` |
| Диапазоны в switch | `< 0 => …`, `>= 0 and < 10 => …` |
| Несколько значений | `case A: case B:` или `A or B => …` |
| Вернуть успех и значение | `bool TryX(…, out T result)` |
| Поменять две переменные | `(a, b) = (b, a)` |
| Не копировать большую структуру | параметр `in` |
| Переменное число аргументов | `params int[]` или `params ReadOnlySpan<int>` |
| Удалить элементы из списка | `list.RemoveAll(predicate)` |
| Вспомогательная функция внутри метода | `static int Helper(int x) => …` в конце метода |

## Связанные материалы

- Пререквизиты: [02 · Типы и переменные](../02-types-and-variables/overview.ru.md)
- Раньше: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/overview.ru.md)
- `using`, `fixed` и закрепление объектов: [12 · Память и GC](../12-memory-and-gc/expert.ru.md#закрепление-объектов)
- То же в других треках: [Python](../../../python/overview.ru.md) — `match`, циклы `for … in`, аргументы `*args` и `**kwargs`

## Источники

- [Операторы выбора: if, switch](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/selection-statements)
- [Switch expression](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression)
- [Шаблоны (patterns)](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns)
- [Операторы цикла](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/iteration-statements)
- [Параметры методов: ref, out, in, params](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/method-parameters)
- [Именованные и необязательные аргументы](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments)
- [Локальные функции](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
- [ref-возвраты и ref-переменные](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/declarations#reference-variables)
- [Операторы перехода](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/jump-statements)
- [Разрешение перегрузки (спецификация)](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions)
- [Атрибуты информации о вызывающем](https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/caller-information)
- [Новое в C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15)
