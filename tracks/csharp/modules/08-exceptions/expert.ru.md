---
title: "Исключения: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 08-exceptions
part: 1
tags: [two-pass-exception-handling, exception-clauses, stack-trace, rethrow, exception-filters, unhandled-exceptions, exception-cost, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Исключения: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Блоки обработки в IL

В метаданных метода обработчики описаны таблицей **областей обработки исключений** (exception handling clauses). Проверено чтением `MethodBody.ExceptionHandlingClauses`:

| Конструкция C# | Тип области в IL |
|---|---|
| `try { } catch (E) { }` | `Clause` (catch) |
| `catch (E e) when (условие)` | `Filter` — условие компилируется в **отдельный блок фильтра** |
| `try { } finally { }` | `Finally` |
| `using`, `lock` | `Finally` (компилятор разворачивает их в `try/finally`) |

В CLR существует ещё тип `Fault` (выполняется только при исключении), но C# его не генерирует. Для `throw;` компилятор выпускает инструкцию `rethrow`, а для `throw ex;` — обычный `throw` (проверено чтением IL): именно поэтому первая форма сохраняет стек, а вторая создаёт его заново.

### Двухпроходная обработка

Когда исключение брошено, runtime работает в **два прохода**:

1. **Поиск обработчика.** Идёт вверх по стеку и ищет подходящий `catch`; по пути **выполняет блоки фильтров** `when`, но не блоки `finally`. Стек пока не разматывается.
2. **Разматывание.** Идёт от места броска до найденного обработчика и по пути выполняет блоки `finally`; затем выполняется сам `catch`.

Проверено порядком вызовов: для `try { try { throw } finally { "inner finally" } } catch (Exception) when ("filter") { "catch" }` журнал — `filter`, `inner finally`, `catch`. Следствия:

- Фильтр видит состояние **на момент броска**: локальные переменные кадров ниже ещё не очищены, а `finally` ещё не выполнялись. Это удобно для диагностики и опасно, если фильтр полагается на побочные эффекты `finally`.
- Если подходящего обработчика нет, второй проход не начинается: необработанное исключение завершает процесс (а блоки `finally` могут не выполниться).
- Исключение внутри фильтра подавляется, и фильтр считается вернувшим `false` (проверено: внешний `catch` получил исходное исключение, а не «from filter»).

### Что хранит исключение

Объект исключения — обычный объект класса `Exception` (и наследников) в куче. Стек (`StackTrace`) заполняется **в момент броска**, а не создания: у `new Exception("x")`, которое не бросали, `StackTrace == null` (проверено). Поэтому создание исключения дешевле броска; бросок собирает кадры стека, а разматывание их проходит. Повторный бросок `throw;` продолжает ту же трассу, `throw ex;` начинает новую, `ExceptionDispatchInfo.Throw()` восстанавливает исходную и добавляет в трассу разделитель.

Важные члены: `Message`, `StackTrace`, `InnerException`, `Data` (словарь для контекста), `HResult` (код для взаимодействия с COM/Win32; у `InvalidOperationException` ненулевой — проверено), `Source`, `TargetSite` (метод, в котором произошёл бросок — проверено: имя `Thrower`), `HelpLink`.

### Исключения, которые нельзя перехватить

- **Переполнение стека** (`StackOverflowException`) завершает процесс: перехватить его нельзя, поэтому рекурсию ограничивают глубиной.
- **`Environment.FailFast`** завершает процесс немедленно, без `finally` и без финализаторов.
- Нехватка памяти (`OutOfMemoryException`) формально перехватывается, но состояние программы после неё ненадёжно.
- Необработанное исключение в любом потоке завершает процесс.

### Стоимость

Бросок и перехват на порядки дороже обычного возврата. В проверке 20 000 циклов `throw`/`catch` — ≈37 мс; 20 000 `int.TryParse` с неудачным разбором — ≈0,3 мс (разница более чем в 100 раз; числа зависят от машины и версии runtime). Но **путь без исключения почти бесплатен**: вход в `try` не добавляет заметных затрат, поэтому «обернуть в `try`» безопасно, а вот бросать в горячем цикле нельзя.

## Справочник

### Конструкции

| Конструкция | Назначение |
|---|---|
| `try { } catch { }` | перехват любого исключения (без объекта) |
| `catch (E e)` | перехват типа `E` и наследников |
| `catch (E) when (cond)` | перехват по условию, фильтр |
| `finally { }` | код, выполняемый всегда |
| `throw new E(…)` | бросок нового исключения |
| `throw;` | повторный бросок в `catch` с сохранением стека |
| `x ?? throw new E()` / `cond ? a : throw …` | выражение `throw` |
| `using` / `await using` | освобождение через `finally` |
| `checked { } / unchecked { }` | включает/выключает проверку переполнения целых |

### Вспомогательные методы проверок

| Метод | Бросает |
|---|---|
| `ArgumentNullException.ThrowIfNull(x)` | `ArgumentNullException` |
| `ArgumentException.ThrowIfNullOrEmpty(s)` / `ThrowIfNullOrWhiteSpace(s)` | `ArgumentNullException` для `null`, `ArgumentException` для пустой (или состоящей из пробелов) строки |
| `ArgumentOutOfRangeException.ThrowIfNegative(x)`, `ThrowIfZero`, `ThrowIfGreaterThan(x, max)`, `ThrowIfLessThan` … | `ArgumentOutOfRangeException` |
| `ObjectDisposedException.ThrowIf(cond, this)` | `ObjectDisposedException` |

Проверено: `ThrowIfNull((string?)null)`, `ThrowIfNegative(-1)`, `ThrowIfNullOrEmpty("")`, `ThrowIfGreaterThan(5, 3)` бросили `ArgumentNullException`, `ArgumentOutOfRangeException`, `ArgumentException`, `ArgumentOutOfRangeException`. Имя параметра берётся из выражения вызова (`CallerArgumentExpression`).

### Типичные причины и типы

| Ситуация | Исключение (проверено) |
|---|---|
| `s.Length` при `s == null` | `NullReferenceException` |
| `a[2]` для `int[1]` | `IndexOutOfRangeException` |
| `list[0]` у пустого `List<int>` | `ArgumentOutOfRangeException` (параметр `index`) |
| `(int)(object)"x"` | `InvalidCastException` |
| `int.Parse("x")` | `FormatException` |
| `Convert.ToInt32("99999999999")` | `OverflowException` |
| `1 / z` при `z == 0` (целые) | `DivideByZeroException`; для `double` результат — бесконечность |
| `checked(int.MaxValue + 1)` | `OverflowException`; без `checked` результат оборачивается (`-2147483648`) |
| `dict["zzz"]` | `KeyNotFoundException` |

## Подвохи

Каждый подвох проверен запуском.

### throw ex; теряет стек

После `catch (Exception ex) { throw ex; }` трасса начиналась с метода-перехватчика, а кадра `Thrower` в ней не было. `catch { throw; }` сохранил `Thrower`. Причина — разные инструкции IL (`throw` и `rethrow`).

### Исключение в finally подменяет исходное

`try { throw A } finally { throw B }` — вызывающий получил `B` без `InnerException`: исходная ошибка потеряна.

### Возвращаемое значение фиксируется до finally

`try { return x; } finally { x = 99; }` вернул `1`. Но если возвращается ссылка на изменяемый объект, `finally` его изменит (в списке стало 2 элемента).

### Фильтр выполняется до finally

Порядок `filter → inner finally → catch`. Код в фильтре видит ещё «живое» состояние.

### Wait оборачивает, await разворачивает

`Task.Run(() => throw …).Wait()` бросил `AggregateException` с внутренним `InvalidOperationException`; `await` такой же задачи бросил `InvalidOperationException` напрямую.

### Исключения во вложенных using

При исключении в теле двух `using var` порядок: `body, dispose b, dispose a, catch` — ресурсы освобождены до перехвата.

### Исключение без броска не имеет стека

`new Exception("x").StackTrace` — `null`.

### Деление: целые и вещественные

`1 / 0` для `int` — исключение, а `1.0 / 0` — `∞` без исключения.

### Фильтр, который возвращает false

`catch (Exception e) when (Log(e))`, где `Log` возвращает `false`, выводит запись и не ловит исключение: удобный способ логирования, но фильтр не должен бросать исключения сам.

### Стоимость броска

20 000 бросков ≈ 37 мс против ≈ 0,3 мс для `TryParse`: исключения не подходят для ожидаемых результатов.

## Компромиссы и альтернативы

| Задача | Вариант | Цена |
|---|---|---|
| Сообщить об ошибке | исключение | дорого при броске, но не теряется и несёт стек |
| | `TryXxx` с `bool` | вызывающий должен проверять результат |
| | возврат результата (`Result<T>`, `OneOf`) | больше шаблонного кода, зато ошибки видны в типе |
| Освободить ресурс | `using` | — |
| | `try/finally` | многословнее |
| Логирование без перехвата | фильтр `when` | побочный эффект в условии |
| Добавить контекст | обёртка с `InnerException` | дополнительный слой |
| | `ex.Data` | не типизировано |

В C# нет проверяемых исключений (как в Java): сигнатура не объявляет, что метод бросает. Документируйте исключения в XML-комментариях (`<exception cref="…">`).

## Вопросы с собеседований

**1. Как работают `try`, `catch` и `finally`? Когда выполняется `finally`?**

<details><summary>Ответ</summary>

`try` — защищаемый код, `catch` — обработчик по типу, `finally` — код, который выполняется всегда: после нормального завершения, после `catch`, при `return`/`break`/`continue` и при необработанном исключении (пока процесс жив). Не выполняется при `Environment.FailFast`, переполнении стека и других аварийных завершениях процесса.

</details>

**2. Чем `throw;` отличается от `throw ex;`?**

<details><summary>Ответ</summary>

`throw;` компилируется в `rethrow` и сохраняет исходный стек; `throw ex;` — в обычный `throw` и начинает трассу заново с текущей строки, теряя место настоящей ошибки (проверено: кадра `Thrower` в трассе нет). Правильно — `throw;`.

</details>

**3. Что такое фильтры исключений? Чем они лучше проверки внутри `catch`?**

<details><summary>Ответ</summary>

`catch (E e) when (cond)` — условие, определяющее, обрабатывает ли блок исключение. Если `false`, поиск продолжается, и стек не размотан: нет перехвата и повторного броска. Фильтр выполняется на первом проходе — до `finally` внутренних блоков — поэтому видит состояние на момент броска; удобно логировать (`when (Log(e))` с `false`).

</details>

**4. Опиши двухпроходную обработку исключений.**

<details><summary>Ответ</summary>

Первый проход ищет подходящий обработчик вверх по стеку и выполняет фильтры; второй проход разматывает стек, выполняя `finally`, и затем выполняет найденный `catch`. Проверено: порядок `filter → inner finally → catch`. Если обработчика нет, второго прохода нет, процесс завершается.

</details>

**5. Как исключение попадает в `InnerException` и зачем?**

<details><summary>Ответ</summary>

При создании нового исключения исходное передают вторым аргументом конструктора: `new ApplicationException("wrapped", e)`. Так добавляют контекст или меняют уровень абстракции, не теряя причину; `ToString()` печатает всю цепочку.

</details>

**6. Что произойдёт, если в `finally` бросить исключение?**

<details><summary>Ответ</summary>

Оно заменит исходное: вызывающий получит только новое, а `InnerException` будет `null` (проверено). Поэтому в `finally` не должно быть кода, который может бросить; для освобождения ресурсов используйте `using`.

</details>

**7. Что вернёт метод `try { return x; } finally { x = 99; }` при `x = 1`?**

<details><summary>Ответ</summary>

`1`: значение для возврата вычисляется до `finally`. Если бы возвращалась ссылка на изменяемый объект, `finally` мог бы изменить его содержимое. `return` внутри `finally` запрещён (ошибка CS0157).

</details>

**8. Когда исключения — плохой выбор? Что вместо них?**

<details><summary>Ответ</summary>

Для ожидаемых случаев (ввод пользователя, отсутствие ключа): они дороги (в проверке 20 000 бросков ≈ 37 мс против ≈ 0,3 мс для `TryParse`). Шаблон `TryXxx` с `bool` и `out`, `TryGetValue`, возврат результата. Исключения — для нарушений контракта и непредвиденных сбоев.

</details>

**9. Как создать собственное исключение? Какие правила?**

<details><summary>Ответ</summary>

Наследоваться от `Exception` или подходящего специфичного типа (не `ApplicationException`), назвать с суффиксом `Exception`, передать сообщение и `InnerException` в базовый конструктор, добавить свойства с данными. Создавать только если вызывающий код будет ловить именно этот тип.

</details>

**10. Чем `AggregateException` отличается от обычного? Что делает `await`?**

<details><summary>Ответ</summary>

Собирает несколько исключений (`InnerExceptions`) — от `Task.WaitAll`, `Parallel`, `Task.Wait()`. `await` разворачивает и бросает **первое** исходное исключение (проверено: `InvalidOperationException`, а `Wait()` — `AggregateException`). Методы `Flatten()` и `Handle()` разбирают вложенные.

</details>

**11. Что такое `ExceptionDispatchInfo`?**

<details><summary>Ответ</summary>

Класс, который захватывает исключение (`Capture`) и позже бросает его (`Throw()`) с сохранением исходных кадров стека — в том числе в другом потоке или методе. На нём основан проброс исключений через `await`.

</details>

**12. Как исключения влияют на производительность? Стоят ли `try` блоки чего-то?**

<details><summary>Ответ</summary>

Бросок и перехват дороги (стек собирается и разматывается). Вход в `try` на «счастливом пути» практически бесплатен, поэтому `try` в горячем коде допустимы, а бросок исключений в цикле — нет.

</details>

**13. Подвох: почему `1 / 0` бросает исключение, а `1.0 / 0` нет?**

<details><summary>Ответ</summary>

Целочисленное деление на ноль не имеет результата и бросает `DivideByZeroException`; для чисел с плавающей точкой стандарт IEEE 754 определяет бесконечность и `NaN` (проверено: `∞`).

</details>

**14. Что такое `OverflowException` и когда она возникает?**

<details><summary>Ответ</summary>

При арифметике целых в `checked`-контексте (`checked(int.MaxValue + 1)`), в `Convert`/`Parse` для слишком больших значений, в `Enumerable.Sum` для `int`. Без `checked` арифметика оборачивается (`-2147483648`). По умолчанию проверка переполнения для обычной арифметики выключена, её включают `checked` или параметр компилятора.

</details>

**15. Какие исключения нельзя перехватить?**

<details><summary>Ответ</summary>

Переполнение стека (`StackOverflowException`) завершает процесс; `Environment.FailFast` завершает его немедленно, без `finally`. После `OutOfMemoryException` состояние программы ненадёжно. Необработанное исключение в любом потоке тоже завершает процесс.

</details>

**16. Подвох: что выведет код?**

```csharp
try { try { throw new Exception("e"); } finally { Console.Write("inner "); } }
catch (Exception) when (Log()) { Console.Write("catch"); }
// Log() prints "filter " and returns true
```

<details><summary>Ответ</summary>

`filter inner catch`: фильтр выполняется на первом проходе, до `finally`. Если `Log()` вернёт `false`, исключение не будет поймано этим блоком.

</details>

**17. Как проверять аргументы метода и какие исключения бросать?**

<details><summary>Ответ</summary>

Сразу в начале публичного метода: `ArgumentNullException.ThrowIfNull(x)`, `ArgumentOutOfRangeException.ThrowIfNegative(n)`, `ArgumentException.ThrowIfNullOrEmpty(s)`. Для недопустимого состояния — `InvalidOperationException`, для использованного объекта — `ObjectDisposedException.ThrowIf`. Имя параметра подставляется автоматически.

</details>

## Шпаргалка

| Нужно | Решение |
|---|---|
| Освободить ресурс | `using` |
| Перебросить исключение | `throw;` |
| Обернуть с причиной | `new X("…", inner)` |
| Ловить по условию | `catch (E e) when (…)` |
| Залогировать, не ловя | фильтр, возвращающий `false` |
| Проверить аргумент | `ArgumentNullException.ThrowIfNull` и др. |
| Ожидаемая ошибка | `TryXxx` |
| Отложить бросок | `ExceptionDispatchInfo.Capture(e).Throw()` |
| Несколько ошибок задач | `AggregateException`, `await` |
| Нельзя бросать | `Dispose`, `finally`, финализаторы |

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md), [продвинутая теория](advanced.ru.md)
- `using`, `Dispose`, финализаторы: [12 · Память и GC](../12-memory-and-gc/expert.ru.md)
- Исключения в `async`: 10 · Async/await
- `checked`/`unchecked` и переполнение: [02 · Типы и переменные](../02-types-and-variables/basics.ru.md)
- Методы и итераторы: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/expert.ru.md)

## Источники

- [Исключения в C#](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/)
- [Рекомендации по исключениям](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Структура обработки исключений в CLR (ECMA-335, раздел I.12.4.2)](https://learn.microsoft.com/dotnet/standard/clr)
- [Exception](https://learn.microsoft.com/dotnet/api/system.exception)
- [ExceptionHandlingClause](https://learn.microsoft.com/dotnet/api/system.reflection.exceptionhandlingclause)
- [Environment.FailFast](https://learn.microsoft.com/dotnet/api/system.environment.failfast)
- [Операторы checked и unchecked](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/checked-and-unchecked)
