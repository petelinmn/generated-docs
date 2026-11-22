---
title: "NSubstitute: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: nsubstitute
part: 2
tags: [nsubstitute, callbacks, events, recursive-mocks, partial-substitutes]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Результат, зависящий от аргументов

```csharp
echo.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(c => c.ArgAt<int>(0) + c.ArgAt<int>(1));
```

Проверено: `echo.Add(20, 22)` → `42`. `c.Arg<T>()` и `c.ArgAt<T>(i)` дают доступ к аргументам вызова.

## Колбэки: `When…Do` и `Arg.Do`

```csharp
logger.When(l => l.Log(Arg.Any<string>())).Do(c => cap.Add("when:" + c.Arg<string>()));
logger.Log(Arg.Do<string>(s => cap.Add("do:" + s)));
```

`When…Do` выполняется на **каждый** подходящий вызов — и для `void`-методов, где `Returns` неприменим. `Arg.Do` — действие при совпадении конкретной настроенной спецификации. Проверено: после двух вызовов `Log("a")`, `Log("b")` результат `when:a, when:b, do:b` — `Arg.Do` зарегистрирован вызовом, настроившим его, и сработал только на последующем `Log("b")`.

## Рекурсивные подмены

Если метод возвращает интерфейс, NSubstitute автоматически возвращает подмену этого интерфейса:

```csharp
f.Create().Name.Returns("auto");           // IFactory.Create() returns IProduct (auto-substituted)
```

Проверено: `f.Create().Name` → `"auto"`. Это избавляет от ручного создания вложенных подмен, но «заводит» цепочки вызовов — не злоупотребляйте (закон Деметры).

## События

```csharp
pub.Changed += (s, e) => got++;
pub.Changed += Raise.Event();            // raises the event
```

Проверено: обработчик вызван (`got = 1`). Для событий с аргументами — `Raise.EventWith(...)`/`Raise.Event<EventHandler<T>>(...)`.

> ⚠️ Проверить: `Raise.EventWith` и события с типизированными аргументами в этом материале не запускали.

## Классы и виртуальные члены

Подмена **класса** перехватывает только `virtual`/`abstract` члены. Проверено на `class Concrete { virtual Virtual(); NonVirtual(); }`:

| Операция | Результат |
|---|---|
| `Substitute.For<Concrete>()` → `v.Virtual().Returns(7)` | `7` |
| `cls.NonVirtual().Returns(5)` | `CouldNotSetReturnDueToNoLastCallException` |
| `Substitute.ForPartsOf<Concrete>()` → `part.Virtual().Returns(8)` | `Virtual() = 8`, а `NonVirtual()` выполняется **настоящий** (`2`) |

`ForPartsOf` — «частичная подмена»: реальный код выполняется, если член не настроен. Конструктор класса при этом вызывается.

## Асинхронные члены

`Returns("two")` для `Task<string?>` оборачивает значение; `ThrowsAsync` задаёт ошибку при `await` ([основы](basics.ru.md#последовательности-исключения-async)).

## Best practices

| Правило | Почему |
|---|---|
| Подменяйте интерфейсы | чище и без ограничений на `virtual` |
| Проверяйте результат, а не вызовы | тесты не ломаются при рефакторинге |
| `Received` — для существенных побочных эффектов | отправка письма, запись |
| Рекурсивные подмены — умеренно | не прятать связность |
| Один тест — одно поведение | ясность |
| Общие настройки — в фабричном методе/конструкторе | меньше дублирования |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md)
- [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md) — когда подмен недостаточно.
- [Refit](../refit/overview.ru.md) — клиенты-интерфейсы как идеальные кандидаты для подмен.

## Источники

- [NSubstitute: Callbacks, void calls and When..Do](https://nsubstitute.github.io/help/callbacks/)
- [NSubstitute: Auto-values and recursive mocks](https://nsubstitute.github.io/help/auto-values/)
- [NSubstitute: Partial subs](https://nsubstitute.github.io/help/partial-subs/)
- [NSubstitute: Raising events](https://nsubstitute.github.io/help/raise-event/)
