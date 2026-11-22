---
title: "Квиз: Unit-тесты"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, theory, fixtures, mocking, parallelism]
status: draft
updated: 2026-11-22
---

# Квиз: Unit-тесты

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что означает AAA в unit-тестах?

- A) Always Assert Anything
- B) Arrange – Act – Assert: подготовка, действие, проверка
- C) Async – Await – Assert
- D) Abstract – Apply – Accept

<details><summary>Ответ</summary>

**B.** Структура теста: подготовить данные, выполнить проверяемое действие, проверить результат.
→ [Базовая теория](basics.ru.md#первый-тест)

</details>

## 2. 🟢 Какой порядок аргументов в `Assert.Equal`?

- A) `(actual, expected)`
- B) `(expected, actual)`
- C) не важен
- D) только один аргумент

<details><summary>Ответ</summary>

**B.** Первым идёт ожидаемое значение; тогда сообщение «Expected: 4, Actual: 5» не вводит в заблуждение (проверено).
→ [Базовая теория](basics.ru.md#проверки-assert)

</details>

## 3. 🟢 Сколько тест-кейсов покажет отчёт?

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
public void Add_Theory(int a, int b, int expected) => Assert.Equal(expected, new Calculator().Add(a, b));
```

- A) 1
- B) 3
- C) 6
- D) 0

<details><summary>Ответ</summary>

**B.** Каждая строка данных — отдельный тест-кейс (проверено: запуск по `*Add_Theory*` дал 3 теста).
→ [Базовая теория](basics.ru.md#параметризованные-тесты-theory)

</details>

## 4. 🟢 Что покажет код?

```csharp
public class LifecycleTests
{
    private readonly Counter _counter = new();
    [Fact] public void A() { _counter.Inc(); Assert.Equal(1, _counter.Value); }
    [Fact] public void B() { _counter.Inc(); Assert.Equal(1, _counter.Value); }
}
```

- A) `A` пройдёт, `B` упадёт (значение 2)
- B) оба пройдут: на каждый тест создаётся новый экземпляр класса
- C) оба упадут
- D) порядок случайный, результат нестабилен

<details><summary>Ответ</summary>

**B.** xUnit создаёт новый экземпляр тестового класса для каждого теста, поэтому поля не делятся (проверено).
→ [Базовая теория](basics.ru.md#изоляция-тестов)

</details>

## 5. 🟡 Сколько раз создаётся `SharedFixture` в классе из двух тестов с `IClassFixture<SharedFixture>`?

- A) 2
- B) 1
- C) 0
- D) по числу ядер

<details><summary>Ответ</summary>

**B.** Фикстура класса создаётся один раз и передаётся в конструктор; оба теста увидели `Id == 1` (проверено).
→ [Продвинутая теория](advanced.ru.md#фикстуры-общее-состояние)

</details>

## 6. 🟡 Что делает `mailer.Received(1).Send("ann", "Good morning, ann")`?

- A) вызывает `Send`
- B) проверяет, что `Send` был вызван ровно один раз с такими аргументами
- C) задаёт возвращаемое значение
- D) создаёт мок

<details><summary>Ответ</summary>

**B.** `Received(n)` — проверка взаимодействия у мока NSubstitute; `Returns` задаёт возвращаемое значение у заглушки; `Substitute.For<T>()` создаёт дублёр.
→ [Продвинутая теория](advanced.ru.md#тестовые-дублёры)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `--filter-trait "Category=Slow"` запускает только тесты с такой меткой
- [ ] B) покрытие считается и для кода, лежащего в самом тестовом проекте
- [ ] C) классы тестов по умолчанию выполняются параллельно
- [ ] D) `TestContext.Current.CancellationToken` позволяет прервать асинхронный вызов при остановке запуска

<details><summary>Ответ</summary>

**A, C, D.** B неверно: отчёт покрытия оказался пустым, когда проверяемый код находился в тестовой сборке (проверено); код нужно вынести в отдельный проект. C: три класса достигли 3 одновременных исполнений.
→ [Продвинутая теория](advanced.ru.md#покрытие-кода)

</details>

## 8. 🟡 Как сделать тест, зависящий от текущего времени, детерминированным?

- A) сравнивать с `DateTime.Now` в тесте
- B) ввести время через интерфейс (`IClock`/`TimeProvider`) и подставить фиксированное значение
- C) запускать тесты ночью
- D) добавить `Thread.Sleep`

<details><summary>Ответ</summary>

**B.** Зависимость от часов заменяется дублёром: `clock.Now.Returns(new DateTime(2026, 1, 1, 9, 0, 0))`, и результат не зависит от момента запуска.
→ [Продвинутая теория](advanced.ru.md#управление-недетерминизмом)

</details>

## 9. 🟡 Открытый вопрос: что такое хороший unit-тест и чего в нём избегать?

<details><summary>Ключевые пункты хорошего ответа</summary>

- быстрый, независимый, повторяемый, самопроверяющийся; один сценарий — одна идея;
- проверяет **поведение** (результат, значимые побочные эффекты), а не детали реализации;
- имя описывает метод, сценарий и ожидание;
- избегать: зависимости от порядка, статического состояния, реального времени, сети, `Thread.Sleep`, перебора проверок вызовов моков;
- граничные значения и ошибки — в `Theory`.

</details>

## 10. 🔴 Что случится?

```csharp
static void Boom() => throw new InvalidOperationException();
[Fact] public void T() => Assert.Throws<Exception>(Boom);
```

- A) тест пройдёт: `InvalidOperationException` — наследник `Exception`
- B) тест упадёт: `Throws<T>` требует точного совпадения типа
- C) ошибка компиляции
- D) тест будет пропущен

<details><summary>Ответ</summary>

**B.** «Exception type was not an exact match» (проверено). Для наследников — `Assert.ThrowsAny<Exception>`.
→ [Экспертная теория](expert.ru.md#утверждения-и-их-семантика)

</details>

## 11. 🔴 Два теста изменяют общий статический счётчик и оба проверяют `Assert.Equal(1, Static.N)`. Что будет при запуске всего набора?

- A) оба пройдут
- B) один пройдёт, другой упадёт: состояние общее для тестов
- C) оба упадут всегда
- D) xUnit автоматически сбросит статику

<details><summary>Ответ</summary>

**B.** Статическое состояние живёт всё время процесса и общее для всех тестов; xUnit его не сбрасывает (проверено: `Actual: 2` в одном из тестов). Не используйте статику в тестах.
→ [Экспертная теория](expert.ru.md#подвохи)

</details>
