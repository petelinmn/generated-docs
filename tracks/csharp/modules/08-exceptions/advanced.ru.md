---
title: "Исключения: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 08-exceptions
part: 1
tags: [exception-filters, rethrow, exception-dispatch-info, aggregate-exception, finally, try-pattern, custom-exceptions, exception-cost, global-handlers]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Исключения: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Фильтры исключений when

Условие `when` уточняет, будет ли блок `catch` обрабатывать исключение:

```csharp
try
{
    try { throw new InvalidOperationException("a"); }
    catch (InvalidOperationException e) when (e.Message == "b") { /* not for us */ }
}
catch (Exception) { Console.WriteLine("filter false → outer catch"); }
```

Если фильтр вернул `false`, исключение продолжает подниматься к внешним обработчикам, а **стек не разматывается и не теряется**. Фильтр выполняется **до** блоков `finally` внутренних `try`: проверено — для `try { try { throw } finally { inner } } catch when (filter) { catch }` порядок: `filter`, `inner finally`, `catch`. Поэтому фильтр можно использовать для логирования без перехвата:

```csharp
catch (Exception e) when (LogAndReturnFalse(e)) { }   // logs, never catches
```

Проверено: такой фильтр записал сообщение и не поймал исключение. Побочный эффект в фильтре — осознанный приём, но фильтр должен быть быстрым и не бросать собственных исключений (оно будет проглочено).

## throw и throw ex

Внутри `catch` повторно бросить исключение можно двумя способами, и они **различаются**:

```csharp
try { Thrower(); } catch { throw; }              // keeps the original stack
try { Thrower(); } catch (Exception ex) { throw ex; }   // resets the stack to this line
```

Проверено: стек после `throw;` содержит кадр `Thrower` (`Thrower > лямбда > Where`), а после `throw ex;` — только лямбду, то есть **место настоящей ошибки потеряно**. Используйте `throw;`. Если нужно бросить исключение в другом потоке или позже, сохраните его в `ExceptionDispatchInfo`:

```csharp
ExceptionDispatchInfo? edi = null;
try { Thrower(); } catch (Exception e) { edi = ExceptionDispatchInfo.Capture(e); }
edi!.Throw();      // the original stack frames are preserved (Thrower is in the trace)
```

Именно так `await` пробрасывает исключения задач.

## Обёртывание и InnerException

```csharp
catch (Exception e) { throw new ApplicationException("wrapped", e); }
```

Оборачивайте, когда нужно добавить контекст или перейти на уровень абстракции слоя (например, исключение доступа к данным → исключение сервиса). Исходную причину всегда передавайте вторым аргументом. Дерево причин видно через `InnerException`; `ToString()` печатает всю цепочку. Не оборачивайте без причины — лишние слои мешают найти источник.

## finally: тонкости

- `finally` выполняется при `return`, `break`, `continue` и при исключении в `try`/`catch`.
- **Значение возврата фиксируется до `finally`**: для `return x;` с `finally { x = 99; }` метод вернул `1`, а для ссылочного типа `finally` может изменить содержимое объекта (список получил второй элемент — проверено).
- **Исключение внутри `finally` заменяет исходное**: `try { throw A } finally { throw B }` — вызывающий получит только `B`, а `InnerException` будет `null` (проверено). Поэтому в `finally` не должно быть кода, который может бросить исключение.
- `return` в `finally` запрещён (CS0157): нельзя выйти из `finally`.
- `finally` не выполнится, если процесс завершён аварийно (`Environment.FailFast`, потеря питания, `StackOverflow`).

## AggregateException

Операции с несколькими задачами могут завершиться несколькими ошибками, поэтому их собирает `AggregateException` (подробнее — 10 · Async/await):

```csharp
try { Task.WaitAll(Task.Run(() => throw new InvalidOperationException("t1")),
                   Task.Run(() => throw new ArgumentException("t2"))); }
catch (AggregateException ae)
{
    Console.WriteLine(string.Join(",", ae.InnerExceptions.Select(x => x.GetType().Name)));
}
```

Проверено: `WaitAll` собрал оба исключения; `Task.Wait()` для одной упавшей задачи тоже оборачивает исключение в `AggregateException`, а `await` — **разворачивает** и бросает исходное (`InvalidOperationException`). Методы `Flatten()` и `Handle(...)` помогают разобрать вложенные агрегаты.

## Проектирование собственных исключений

- Наследуйте от `Exception` или от подходящего наследника (`InvalidOperationException`, `ArgumentException`), а не от `ApplicationException`.
- Имя оканчивается на `Exception`.
- Передавайте сообщение и `InnerException` в базовый конструктор; поддержите нужные конструкторы (`()`, `(string)`, `(string, Exception)`), если исключение публичное и расширяемое.
- Добавляйте свойства с данными (`OrderId`), чтобы вызывающий код не разбирал сообщение строкой.
- Сообщение — для разработчика и логов; не показывайте его пользователю: оно может содержать внутренние детали.
- Создавайте свой тип, только если по нему есть смысл **ловить отдельно**.

Словарь `ex.Data["k"] = value` позволяет прикрепить контекст к существующему исключению, не создавая нового типа.

## Исключения и производительность

Исключения предназначены для исключительных случаев. Бросок и перехват — на порядки дороже обычного возврата: стек собирается, исключение разматывается через кадры. В проверке 20 000 циклов `throw`/`catch` заняли ≈37 мс, а 20 000 вызовов `int.TryParse` с неудачным разбором — ≈0,3 мс (разница более чем в 100 раз; абсолютные числа зависят от машины).

**Шаблон Try**: вместо исключения метод возвращает `bool` и значение через `out`:

```csharp
public static bool TryParseId(string text, out int id) => int.TryParse(text, out id) && id > 0;
```

Правило: если сбой — **ожидаемая** часть работы (ввод пользователя, поиск по ключу), используйте `TryXxx`; если сбой означает нарушение контракта — исключение.

## Глобальные обработчики

Необработанное исключение завершает процесс. Для логирования перед завершением есть событие `AppDomain.CurrentDomain.UnhandledException`; в веб-приложениях исключения перехватывает middleware (14 · ASP.NET Core: Minimal API). Глобальный обработчик не заменяет локальную обработку: он нужен, чтобы **записать** сбой, а не «исправить» его.

## Лучшие практики

- Лови конкретные исключения там, где знаешь, что делать; остальные пусть поднимаются выше.
- `throw;`, а не `throw ex;`.
- Не глотай исключения; если ловишь, чтобы залогировать, перебрасывай или используй фильтр `when`.
- Проверяй аргументы публичных методов сразу (`ArgumentNullException.ThrowIfNull`).
- Освобождай ресурсы через `using`, а не вручную в `finally`.
- Не используй исключения для управления потоком; для ожидаемых сбоев — `TryXxx`.
- Бросай специфичные типы; не `Exception`.
- Не бросай исключения из `Dispose`, финализаторов и `finally`.

## Частые ошибки

- ❌ `catch (Exception) { }` → ✅ конкретный тип или логирование и повторный бросок.
- ❌ `throw ex;` → ✅ `throw;`.
- ❌ Исключение в `finally` → ✅ код, который не бросает; освобождение через `using`.
- ❌ Обёртка без `InnerException` → ✅ передавать причину.
- ❌ `catch (Exception e) when (e.Message.Contains(...))` как способ различать ошибки → ✅ типы исключений и свойства.
- ❌ Показывать `ex.ToString()` пользователю → ✅ общее сообщение, подробности — в лог.
- ❌ Ловить `NullReferenceException` вместо проверки на `null` → ✅ исправить причину.

## Сравнение с другими треками

| | C# | Python |
|---|---|---|
| Синтаксис | `try`/`catch`/`finally`, `throw` | `try`/`except`/`finally`/`else`, `raise` |
| Фильтры | `catch … when (условие)` | нет, проверка внутри `except` |
| Повторный бросок | `throw;` (сохраняет стек) | `raise` без аргументов |
| Цепочка причин | `InnerException` | `raise X from e`, `__cause__` |
| Освобождение ресурсов | `using` | `with` (контекстные менеджеры) |
| Проверяемые исключения | нет | нет |

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — двухпроходная обработка, устройство стека, справочник, вопросы с собеседований
- `using` и `Dispose`: [12 · Память и GC](../12-memory-and-gc/advanced.ru.md)
- Исключения в задачах: 10 · Async/await
- Многопоточность и необработанные исключения в потоках: [18 · Многопоточность](../18-concurrency/basics.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md)

## Источники

- [Фильтры исключений](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/exception-handling-statements#exception-filters)
- [Рекомендации по исключениям](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Создание пользовательских исключений](https://learn.microsoft.com/dotnet/standard/exceptions/how-to-create-user-defined-exceptions)
- [ExceptionDispatchInfo](https://learn.microsoft.com/dotnet/api/system.runtime.exceptionservices.exceptiondispatchinfo)
- [AggregateException](https://learn.microsoft.com/dotnet/api/system.aggregateexception)
- [Ошибка компилятора CS0157](https://learn.microsoft.com/dotnet/csharp/misc/cs0157)
- [AppDomain.UnhandledException](https://learn.microsoft.com/dotnet/api/system.appdomain.unhandledexception)
