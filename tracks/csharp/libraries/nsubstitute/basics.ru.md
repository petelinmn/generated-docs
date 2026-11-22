---
title: "NSubstitute: основы"
lang: ru
track: csharp
kind: library
doc: basics
slug: nsubstitute
part: 2
tags: [nsubstitute, mocking, unit-testing, test-doubles]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md), [05 · Интерфейсы, структуры, records](../../modules/05-interfaces-structs-records/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

NSubstitute — библиотека для создания **тестовых дублей** (substitutes/mocks): подставных реализаций интерфейсов и классов, поведение которых вы задаёте в тесте. Это позволяет проверять код изолированно от БД, сети и других зависимостей. Синтаксис — без лямбд-выражений для настройки: вы просто вызываете метод подмены и затем `.Returns(...)`.

```bash
dotnet add package NSubstitute
```

## Создание и настройка

```csharp
public interface ICalculator { int Add(int a, int b); string Mode { get; set; } }

var calc = Substitute.For<ICalculator>();
calc.Add(1, 2).Returns(3);
calc.Add(Arg.Any<int>(), 10).Returns(100);
calc.Mode.Returns("sci");
```

Проверено запуском:

| Вызов | Результат |
|---|---|
| `calc.Add(1, 2)` до настройки | `0` (значение по умолчанию для `int`) |
| `calc.Add(1, 2)` после `.Returns(3)` | `3` |
| `calc.Add(5, 10)` по шаблону `Arg.Any<int>(), 10` | `100` |
| `calc.Add(7, 7)` — не настроено | `0` |
| `calc.Mode` | `"sci"` |

Ненастроенные члены возвращают **значения по умолчанию** (0, `null`, пустая строка для `string`-свойств зависит от типа), а не бросают исключение.

## Проверка вызовов

```csharp
calc.Received().Add(1, 2);          // at least one call
calc.Received(3).Add(1, 2);         // exactly three calls
calc.DidNotReceive().Add(9, 9);
```

Проверено: `Received(3)` прошёл при трёх реальных вызовах `Add(1, 2)` — вызов внутри `.Returns(...)` при настройке **не считается**. Несовпадение (`Received(5)`) и нарушение `DidNotReceive` дают `ReceivedCallsException` с перечислением фактических вызовов.

## Сопоставление аргументов

| Запись | Смысл |
|---|---|
| значение (`1`) | равенство |
| `Arg.Any<T>()` | любое значение типа |
| `Arg.Is<T>(x => …)` | условие |
| `Arg.Do<T>(x => …)` | выполнить действие с аргументом |

Проверено: `m.Received().Log(Arg.Is<string>(s => s.StartsWith("he")))` прошёл для вызова `Log("hello")`, а `DidNotReceive` с условием `StartsWith("x")` — тоже.

## Последовательности, исключения, async

```csharp
seq.Add(0, 0).Returns(1, 2, 3);                     // 1, 2, 3, 3, ...
bad.Add(Arg.Any<int>(), Arg.Any<int>()).Throws(new InvalidOperationException("boom"));
repo.GetAsync(2).Returns("two");                     // wraps into Task<string?>
repo.GetAsync(99).ThrowsAsync(new KeyNotFoundException());
```

Проверено: последовательность — четыре вызова дали `1, 2, 3, 3` (последнее значение повторяется); `Throws` бросило `InvalidOperationException`; для `Task<T>` достаточно вернуть значение (`two`), ненастроенный `GetAsync(3)` вернул `null`; `ThrowsAsync` — исключение при `await`.

## Частые ошибки новичка

- ❌ Мокать конкретный класс с не-виртуальными членами → ✅ интерфейсы или `virtual`-члены ([продвинутый](advanced.ru.md#классы-и-виртуальные-члены)).
- ❌ Проверять реализацию, а не результат → ✅ проверяйте поведение; `Received` — только для важных побочных эффектов.
- ❌ Настраивать другую подмену внутри `.Returns(...)` → ✅ создайте значение заранее.
- ❌ `internal`-интерфейс без `InternalsVisibleTo` → ✅ [подвох](expert.ru.md#грани-и-подвохи).
- ❌ Ожидать исключение от ненастроенного члена → ✅ вернётся значение по умолчанию.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — колбэки, события, рекурсивные подмены, `ForPartsOf`.
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md) — xUnit и структура тестов.
- [Refit](../refit/overview.ru.md) — подмена HTTP-клиента в тестах.
- [Testcontainers](../testcontainers/overview.ru.md) — когда нужна настоящая БД, а не подмена.
- [Python](../../../python/overview.ru.md) — `unittest.mock`.

## Источники

- [NSubstitute: Getting started](https://nsubstitute.github.io/help/getting-started/)
- [NSubstitute: Return for specific args](https://nsubstitute.github.io/help/return-for-args/)
- [NSubstitute: Checking received calls](https://nsubstitute.github.io/help/received-calls/)
