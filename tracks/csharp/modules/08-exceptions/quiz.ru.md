---
title: "Квиз: Исключения"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 08-exceptions
part: 1
tags: [exceptions, try-catch-finally, exception-filters, rethrow, aggregate-exception]
status: draft
updated: 2026-11-22
---

# Квиз: Исключения

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 В каком порядке выполнятся блоки при `Divide(1, 0)`?

```csharp
static string Divide(int a, int b, List<string> log)
{
    try { log.Add("try"); return (a / b).ToString(); }
    catch (DivideByZeroException) { log.Add("catch"); return "error"; }
    finally { log.Add("finally"); }
}
```

- A) `try > finally`
- B) `try > catch > finally`
- C) `catch > try > finally`
- D) `try > finally > catch`

<details><summary>Ответ</summary>

**B.** Сначала `try` до исключения, затем подходящий `catch`, а `finally` выполняется всегда — и при нормальном завершении (`try > finally`), и при исключении (проверено).
→ [Базовая теория](basics.ru.md#try-catch-finally)

</details>

## 2. 🟢 Как правильно перебросить пойманное исключение, сохранив его стек?

- A) `throw ex;`
- B) `throw;`
- C) `throw new Exception(ex.Message);`
- D) `return ex;`

<details><summary>Ответ</summary>

**B.** Голый `throw;` сохраняет исходную трассу. `throw ex;` начинает трассу заново: кадра метода, где ошибка возникла, в ней не будет (проверено). `new Exception(ex.Message)` теряет тип и стек, если не передать `ex` как внутреннее исключение.
→ [Продвинутая теория](advanced.ru.md#throw-и-throw-ex)

</details>

## 3. 🟢 Выберите все верные утверждения

- [ ] A) `finally` выполняется и при `return` внутри `try`
- [ ] B) собственное исключение наследуют от `Exception`
- [ ] C) исключения хорошо подходят для обычного ожидаемого результата, например «пользователь не найден»
- [ ] D) более специфичный `catch` должен идти выше общего

<details><summary>Ответ</summary>

**A, B, D.** C неверно: бросок дорог — 20 000 бросков ≈ 37 мс против ≈ 0,3 мс для `TryParse` (проверено); для ожидаемых результатов используйте `TryXxx` или возвращаемое значение. D: общий `catch` перед специфичным — ошибка компиляции.
→ [Базовая теория](basics.ru.md#когда-бросать-исключение)

</details>

## 4. 🟢 Что лучше использовать вместо исключения при разборе пользовательской строки в число?

- A) `int.Parse` в `try/catch`
- B) `int.TryParse`
- C) `Convert.ToInt32` в `try/catch`
- D) всё равно

<details><summary>Ответ</summary>

**B.** Некорректный ввод — штатная ситуация, поэтому `TryParse` возвращает `false` без исключения и в десятки раз дешевле.
→ [Базовая теория](basics.ru.md#когда-бросать-исключение)

</details>

## 5. 🟡 Что вернёт метод?

```csharp
static int F()
{
    int x = 1;
    try { return x; }
    finally { x = 99; }
}
```

- A) `99`
- B) `1`
- C) исключение
- D) ошибка компиляции

<details><summary>Ответ</summary>

**B.** Возвращаемое значение вычисляется и фиксируется до выполнения `finally`; изменение локальной `x` на результат не влияет (проверено). Но если возвращается **ссылка** на изменяемый объект, `finally` его изменит.
→ [Продвинутая теория](advanced.ru.md#finally-тонкости)

</details>

## 6. 🟡 Что получит вызывающий код?

```csharp
static void M() { try { throw new Exception("original"); } finally { throw new Exception("cleanup failed"); } }
```

- A) `original`
- B) `cleanup failed`, исходная ошибка потеряна
- C) `AggregateException` с обеими
- D) `cleanup failed` с `InnerException = original`

<details><summary>Ответ</summary>

**B.** Исключение из `finally` подменяет исходное без `InnerException` (проверено). Поэтому в `finally` код очистки нужно оборачивать в собственный `try/catch` (логировать, но не маскировать оригинал).
→ [Экспертная теория](expert.ru.md#исключение-в-finally-подменяет-исходное)

</details>

## 7. 🟡 Что произойдёт при фильтре, который возвращает `false`?

```csharp
try { throw new InvalidOperationException(); }
catch (Exception e) when (Log(e)) { Console.WriteLine("caught"); }   // Log prints and returns false
```

- A) `caught`
- B) исключение не поймано, но `Log` выполнился (удобно для логирования)
- C) исключение молча проглочено
- D) ошибка компиляции

<details><summary>Ответ</summary>

**B.** Фильтр `when` выполняется до разматывания стека; при `false` исключение идёт дальше к другим обработчикам. Это удобный способ логирования, но сам фильтр не должен бросать исключений. Порядок: `filter → внутренний finally → catch` (проверено).
→ [Продвинутая теория](advanced.ru.md#фильтры-исключений-when)

</details>

## 8. 🟡 Какой тип исключения получит вызывающий?

```csharp
var t = Task.Run(() => throw new InvalidOperationException("x"));
try { t.Wait(); } catch (Exception e) { Console.WriteLine(e.GetType().Name); }
try { await t; }   catch (Exception e) { Console.WriteLine(e.GetType().Name); }
```

<details><summary>Ответ</summary>

```text
AggregateException
InvalidOperationException
```

`Wait()`/`Result` оборачивают исключения в `AggregateException`, а `await` разворачивает и бросает исходное (проверено). Подробнее — [10 · Async/await](../10-async-await/overview.ru.md).
→ [Экспертная теория](expert.ru.md#wait-оборачивает-await-разворачивает)

</details>

## 9. 🟡 Открытый вопрос: когда бросать исключение, а когда возвращать результат или `false`?

<details><summary>Ключевые пункты хорошего ответа</summary>

- исключение — для исключительных, непредвиденных ситуаций: нарушен контракт метода, сломанное состояние, недоступный ресурс;
- ожидаемые отказы (ввод пользователя, «не найдено») — `TryXxx`, `null`, `Result`-объекты;
- бросок дорог и сбивает поток управления; проверки аргументов — `ArgumentNullException.ThrowIfNull` и подобные;
- не глотать исключения молча, ловить как можно более узкий тип, добавлять контекст через `InnerException`.

</details>

## 10. 🔴 Что выведет код и почему?

```csharp
static void Thrower() => throw new InvalidOperationException("x");
static void OuterBad() { try { Thrower(); } catch (Exception ex) { throw ex; } }

try { OuterBad(); } catch (Exception e) { Console.WriteLine(e.StackTrace!.Contains("Thrower")); }
```

<details><summary>Ответ</summary>

`False`. `throw ex;` компилируется в инструкцию IL `throw`, которая сбрасывает трассу и начинает её с места перебрасывания, поэтому кадр `Thrower` теряется. `throw;` компилируется в `rethrow` и сохраняет его (в проверенном варианте с `throw;` результат — `True`).
→ [Экспертная теория](expert.ru.md#throw-ex-теряет-стек)

</details>

## 11. 🔴 Какие результаты дадут эти выражения?

```csharp
Console.WriteLine(1.0 / 0);
Console.WriteLine(1 / 0);
```

<details><summary>Ответ</summary>

Первое выведет `∞` (в зависимости от культуры — `Infinity`) без исключения: вещественное деление следует IEEE 754. Второе бросит `DivideByZeroException`: целочисленное деление на ноль — ошибка (проверено).
→ [Экспертная теория](expert.ru.md#деление-целые-и-вещественные)

</details>
