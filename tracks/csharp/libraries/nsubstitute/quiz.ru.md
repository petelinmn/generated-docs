---
title: "Квиз: NSubstitute"
lang: ru
track: csharp
kind: library
doc: quiz
slug: nsubstitute
part: 2
tags: [nsubstitute, mocking, unit-testing]
status: draft
updated: 2026-11-22
---

# Квиз: NSubstitute

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка: 🟢 [основы](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [эксперт](expert.ru.md).

## 1. 🟢 Как создать подмену интерфейса?

- A) `new ICalculator()`
- B) `Substitute.For<ICalculator>()`
- C) `Mock.Of<ICalculator>()`
- D) `Activator.CreateInstance`

<details><summary>Ответ</summary>

**B.** Ненастроенные члены возвращают значения по умолчанию.
→ [Основы](basics.ru.md#создание-и-настройка)

</details>

## 2. 🟢 Что вернёт ненастроенный `calc.Add(7, 7)`?

- A) исключение
- B) `0`
- C) `null`
- D) `7`

<details><summary>Ответ</summary>

**B.** Проверено: значение по умолчанию.
→ [Основы](basics.ru.md#создание-и-настройка)

</details>

## 3. 🟢 Как задать результат `3` на `Add(1, 2)`?

- A) `calc.Setup(...)`
- B) `calc.Add(1, 2).Returns(3)`
- C) `calc.Returns(3)`
- D) `when(calc.Add(1,2)).thenReturn(3)`

<details><summary>Ответ</summary>

**B.** Метод расширения `Returns` над результатом вызова.
→ [Основы](basics.ru.md#создание-и-настройка)

</details>

## 4. 🟢 Что делает `Arg.Any<int>()`?

- A) совпадает с любым `int`
- B) только с нулём
- C) создаёт подмену
- D) бросает исключение

<details><summary>Ответ</summary>

**A.** Проверено: `Add(Arg.Any<int>(), 10)` → `100` для `Add(5, 10)`.
→ [Основы](basics.ru.md#сопоставление-аргументов)

</details>

## 5. 🟡 Как выполнить действие при вызове `void`-метода?

- A) `Returns`
- B) `sub.When(x => x.Log(Arg.Any<string>())).Do(c => …)`
- C) `Throws`
- D) нельзя

<details><summary>Ответ</summary>

**B.** Проверено: `when:a`, `when:b`.
→ [Продвинутый](advanced.ru.md#колбэки-whendo-и-argdo)

</details>

## 6. 🟡 Что произойдёт при `cls.NonVirtual().Returns(5)` для подмены класса?

- A) 5 вернётся
- B) `CouldNotSetReturnDueToNoLastCallException`
- C) тихо проигнорируется
- D) вызовется настоящий метод и вернёт 5

<details><summary>Ответ</summary>

**B.** Проверено: невиртуальные члены не перехватываются.
→ [Продвинутый](advanced.ru.md#классы-и-виртуальные-члены)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `ForPartsOf<T>` выполняет реальный код для ненастроенных виртуальных членов
- [ ] B) `Received(3)` учитывает и вызов внутри настройки `Returns`
- [ ] C) `Returns(1, 2, 3)` повторяет последнее значение
- [ ] D) метод, возвращающий интерфейс, даёт рекурсивную подмену

<details><summary>Ответ</summary>

**A, C, D.** B неверно: проверено — вызов настройки не считается.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>

## 8. 🟡 Открытый вопрос: что вы подменяете в тесте сервиса заказов, а что нет?

<details><summary>Ключевые пункты хорошего ответа</summary>

- подменяю внешние зависимости (репозиторий, платёжный шлюз, часы) через интерфейсы;
- не подменяю тестируемый класс и простые значения/DTO;
- проверяю результат и важные побочные эффекты (`Received`), а не порядок внутренних вызовов;
- интеграцию с БД — отдельными тестами.

</details>

## 9. 🔴 `Substitute.For<IInternal>()` падает с `ArgumentException`. Причина?

- A) устаревший .NET
- B) прокси не видит `internal`-тип без `InternalsVisibleTo("DynamicProxyGenAssembly2")`
- C) тип должен быть `sealed`
- D) неверный пакет

<details><summary>Ответ</summary>

**B.** Проверено текстом исключения.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Чем опасны тесты, проверяющие `Received.InOrder` на все вызовы?

- A) ничем
- B) они связывают тест с реализацией и ломаются при безвредном рефакторинге
- C) медленные
- D) не компилируются

<details><summary>Ответ</summary>

**B.** Проверяйте поведение и значимые эффекты.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>
