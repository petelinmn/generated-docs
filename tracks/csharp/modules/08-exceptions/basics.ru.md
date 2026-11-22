---
title: "Исключения: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 08-exceptions
part: 1
tags: [exceptions, try-catch-finally, throw, custom-exceptions, exception-hierarchy, argument-validation, using, inner-exception]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Исключения: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md), [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md).

## Зачем это нужно

Операция может не удаться: файла нет, аргумент неверен, сеть недоступна. **Исключение** (exception) — объект, который описывает такую ошибку и **прерывает обычный ход выполнения**: управление передаётся ближайшему обработчику вверх по стеку вызовов. Это избавляет от проверки кода возврата после каждого вызова и не позволяет молча проигнорировать ошибку: необработанное исключение завершает программу.

## try, catch, finally

```csharp
try
{
    Console.WriteLine("try");
    throw new InvalidOperationException("boom");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"catch: {ex.Message}");
}
finally
{
    Console.WriteLine("finally");
}
```

Вывод: `try`, затем `catch: boom`, затем `finally`.

- **`try`** — код, который может бросить исключение.
- **`catch (Тип e)`** — обработчик исключений этого типа (и его наследников). Блоков `catch` может быть несколько.
- **`finally`** — выполняется **всегда**: после `try` без ошибок, после `catch` и даже когда исключение не поймано или в `try` выполнен `return`. Нужен для освобождения ресурсов.
- `throw` создаёт и «бросает» исключение: `throw new ArgumentException("message")`.

Проверено: для вложенных `try`/`catch`/`finally` порядок — `try catch finally`, затем `finally` внешнего блока. Метод с `return 1` в `try` и `finally` вернул `1`, и `finally` всё равно выполнился.

### Несколько catch

Блоки проверяются сверху вниз, выполняется **первый подходящий**. Поэтому сначала указывают более конкретные типы:

```csharp
try { throw new ArgumentNullException("p"); }
catch (ArgumentNullException) { Console.WriteLine("specific"); }   // chosen
catch (ArgumentException)     { Console.WriteLine("general"); }
```

Если поставить общий `catch` выше конкретного, компилятор выдаст ошибку (CS0160): конкретный блок недостижим.

## Иерархия исключений

Все исключения наследуют от `System.Exception`. Проверено цепочками базовых типов:

| Исключение | Цепочка |
|---|---|
| `ArgumentNullException` | → `ArgumentException` → `SystemException` → `Exception` |
| `FileNotFoundException` | → `IOException` → `SystemException` → `Exception` |
| `KeyNotFoundException` | → `SystemException` → `Exception` |
| `DivideByZeroException` | → `ArithmeticException` → `SystemException` → `Exception` |

`catch (Exception)` ловит всё, но это почти всегда слишком грубо: лови то, что умеешь обработать.

## Типичные исключения

| Исключение | Причина |
|---|---|
| `NullReferenceException` | обращение к члену через `null` |
| `ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException` | неверный аргумент метода |
| `InvalidOperationException` | операция недопустима в текущем состоянии объекта |
| `IndexOutOfRangeException` | индекс за границей массива |
| `ArgumentOutOfRangeException` | индекс за границей `List<T>` (параметр `index`) |
| `KeyNotFoundException` | ключа нет в словаре |
| `InvalidCastException` | приведение к несовместимому типу |
| `FormatException` | строка не в нужном формате (`int.Parse("x")`) |
| `OverflowException` | переполнение в `checked`-контексте или `Convert` |
| `DivideByZeroException` | целочисленное деление на ноль (для `double` — бесконечность, не исключение) |
| `FileNotFoundException`, `IOException` | файлы и ввод-вывод |
| `ObjectDisposedException` | использование уже освобождённого объекта |
| `NotImplementedException`, `NotSupportedException` | не реализовано / не поддерживается |

Типы всех этих исключений проверены запуском (например, `list[0]` у пустого `List<int>` — `ArgumentOutOfRangeException` с параметром `index`).

## Свойства исключения

```csharp
try { Thrower(); }
catch (Exception e)
{
    Console.WriteLine(e.Message);          // boom
    Console.WriteLine(e.GetType().Name);   // InvalidOperationException
    Console.WriteLine(e.StackTrace);       // the call stack at the moment of the throw
    Console.WriteLine(e.InnerException);   // the cause, if any
}
```

- `Message` — описание для человека; `StackTrace` — стек вызовов в момент `throw`; `InnerException` — исходная причина, если исключение обёрнуто.
- `ToString()` выдаёт тип, сообщение и стек: первая строка — `System.InvalidOperationException: boom`.
- У исключения, которое создали, но **не бросили**, стека нет (`StackTrace == null`).

## Проверка аргументов

Публичные методы проверяют аргументы и бросают исключения сразу. Для этого есть короткие вспомогательные методы:

```csharp
public void Process(string name, int count)
{
    ArgumentNullException.ThrowIfNull(name);
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    ArgumentException.ThrowIfNullOrEmpty(name);
    // ...
}
```

Проверено: они бросают `ArgumentNullException`, `ArgumentOutOfRangeException` и `ArgumentException` соответственно. Имя проверяемого аргумента подставляется автоматически.

**Выражение `throw`** можно использовать внутри выражения:

```csharp
string name = input ?? throw new ArgumentNullException(nameof(input));
```

## using и исключения

`using` освобождает ресурс даже при исключении (он превращается в `try/finally`, см. [12 · Память и GC](../12-memory-and-gc/basics.ru.md#idisposable-и-using)). Проверено: при двух `using var` и исключении в теле порядок был `body, dispose b, dispose a, catch` — ресурсы освобождены до того, как обработчик `catch` получил управление.

## Собственные исключения

Когда нужного исключения нет, создают своё — наследуют от `Exception` (или от подходящего уже существующего типа) и добавляют данные:

```csharp
public class OrderException(int orderId, string message) : InvalidOperationException(message)
{
    public int OrderId { get; } = orderId;
}

try { throw new OrderException(5, "bad order"); }
catch (OrderException e) { Console.WriteLine($"{e.OrderId} {e.Message}"); }   // 5 bad order
```

Название заканчивается на `Exception`. Свои исключения нужны, когда вызывающему коду есть смысл поймать **именно этот случай** и действовать по-другому; иначе берут стандартное.

## Обёртывание причины

Если вы превращаете одну ошибку в другую, сохраняйте исходную в `InnerException`:

```csharp
try { Thrower(); }
catch (Exception e) { throw new ApplicationException("wrapped", e); }
// message: wrapped, inner: boom (InvalidOperationException)
```

## Когда бросать исключение

- ✅ Нарушен контракт метода: неверный аргумент, недопустимое состояние.
- ✅ Операция не может выполнить своё предназначение и вызывающий код не получит корректный результат.
- ❌ Обычный, ожидаемый случай: пользователь ввёл не число, ключа нет в словаре — для них есть `TryParse`, `TryGetValue`.
- ❌ Управление потоком выполнения (выход из цикла, возврат значения): исключения медленные — в проверке 20 000 `throw`/`catch` заняли ≈37 мс, а 20 000 вызовов `int.TryParse` — ≈0,3 мс (цифры зависят от машины, порядок разницы — десятки и сотни раз).

## Ошибки новичка

- ❌ Пустой `catch { }` → ✅ обработать, залогировать или не ловить: молча проглоченная ошибка скрывает причину.
- ❌ `catch (Exception)` везде → ✅ ловить конкретные типы, на которые знаешь реакцию.
- ❌ `throw ex;` в `catch` → ✅ `throw;` сохраняет исходный стек (см. [продвинутую теорию](advanced.ru.md#throw-и-throw-ex)).
- ❌ Терять исходную причину при обёртывании → ✅ передавать `InnerException`.
- ❌ Исключения для ожидаемых ситуаций → ✅ `TryParse`/`TryGetValue`.
- ❌ Выбрасывать `Exception` или `ApplicationException` напрямую → ✅ специфичный тип.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — фильтры `when`, `throw` и `throw ex`, `AggregateException`, `ExceptionDispatchInfo`, проектирование своих исключений
- Освобождение ресурсов: [12 · Память и GC](../12-memory-and-gc/basics.ru.md)
- Классы и наследование: [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `try`/`except`/`finally`, `raise`, контекстные менеджеры

## Источники

- [Обработка исключений](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/exception-handling)
- [Создание и генерация исключений](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions)
- [Оператор try-catch-finally](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/exception-handling-statements)
- [Иерархия исключений .NET](https://learn.microsoft.com/dotnet/standard/exceptions/)
- [Рекомендации по исключениям](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
