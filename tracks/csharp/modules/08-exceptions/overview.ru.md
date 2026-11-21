---
title: "Исключения"
lang: ru
track: csharp
kind: module
doc: overview
slug: 08-exceptions
part: 1
levels: [1, 2, 3]
tags: [exceptions, try-catch-finally, throw, custom-exceptions, exception-hierarchy, argument-validation, exception-filters, rethrow, inner-exception]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Исключения

🌐 **Русский** · [English](overview.en.md)

## Коротко

Исключение — объект, описывающий ошибку, который прерывает обычное выполнение и передаётся ближайшему обработчику вверх по стеку. Блок `try` защищает код, `catch` обрабатывает исключения нужного типа, `finally` выполняется всегда. Исключения нужны для нарушений контракта и непредвиденных сбоев, а для ожидаемых ситуаций лучше шаблон `TryXxx`: бросок дорог.

## Ключевые идеи

- **`try`/`catch`/`finally`:** выполняется первый подходящий `catch` (конкретные типы — выше общих), `finally` — всегда.
- **Иерархия:** все наследуют от `Exception`; ловите конкретные типы, а не `Exception`.
- **`throw;` сохраняет стек, `throw ex;` теряет:** разные инструкции IL (`rethrow` и `throw`).
- **Фильтры `when`** выполняются до `finally` внутренних блоков (двухпроходная обработка) и позволяют логировать без перехвата.
- **Подвохи `finally`:** значение возврата фиксируется до него, исключение в нём заменяет исходное, `return` в нём запрещён.
- **`InnerException`:** оборачивая исключение, передавайте причину.
- **Стоимость:** бросок на порядки дороже обычного возврата; для ожидаемых сбоев — `TryParse`, `TryGetValue`.

## Минимальный пример

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
// try, catch: boom, finally
```

## Когда применять, а когда нет

- ✅ Нарушен контракт метода: неверный аргумент, недопустимое состояние.
- ✅ Операция не может выполнить своё назначение (файла нет, сеть недоступна).
- ✅ `using` для освобождения ресурсов; `ArgumentNullException.ThrowIfNull` для проверки аргументов.
- ❌ Исключения для обычного потока выполнения и ожидаемых результатов.
- ❌ Пустой `catch`; `throw ex;`; исключения из `finally` и `Dispose`.
- ❌ `catch (Exception)` там, где неизвестно, что делать.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `try`/`catch`/`finally`, иерархия, типичные исключения, свойства, проверка аргументов, `using`, свои исключения, обёртывание, когда бросать |
| [2. Продвинутая теория](advanced.ru.md) | фильтры `when`, `throw` и `throw ex`, обёртывание и `InnerException`, подвохи `finally`, `AggregateException`, проектирование исключений, производительность, шаблон Try |
| [3. Экспертная теория](expert.ru.md) | области обработки в IL, двухпроходная обработка, содержимое исключения, неперехватываемые сбои, справочник, подвохи, 17 вопросов с собеседований |
| 4. Самопроверка | ещё не создан — `/learn csharp 08-exceptions level 4` |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 08-exceptions level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 08-exceptions level 6` |

## Связанные материалы

- Пререквизиты: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md), [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md)
- Освобождение ресурсов: [12 · Память и GC](../12-memory-and-gc/overview.ru.md)
- Исключения в многопоточном коде: [18 · Многопоточность](../18-concurrency/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `try`/`except`, `raise from`
