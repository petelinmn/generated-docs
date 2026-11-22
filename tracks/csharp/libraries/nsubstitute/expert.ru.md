---
title: "NSubstitute: экспертный уровень"
lang: ru
track: csharp
kind: library
doc: expert
slug: nsubstitute
part: 2
tags: [nsubstitute, castle-dynamicproxy, test-doubles, mocking-pitfalls, interview-questions]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md) и [продвинутый](advanced.ru.md). **Время:** ~30 мин.

## Внутреннее устройство

`Substitute.For<T>()` создаёт динамический прокси на основе **Castle DynamicProxy**: во время выполнения генерируется класс, наследующий `T` (или реализующий интерфейс), и каждый вызов члена перехватывается. Перехваченный вызов попадает в маршрутизатор (`CallRouter`), который: записывает вызов в историю; ищет подходящую настроенную спецификацию (`Returns`, `Throws`, `When…Do`) и выбирает результат; для ненастроенного члена возвращает «автозначение» — значение по умолчанию, рекурсивную подмену для интерфейсов, пустую задачу для `Task`.

`.Returns(x)` — это метод расширения над **результатом** вызова, поэтому он не знает, какой вызов настраивает: библиотека берёт «последний вызов» подмены в текущем потоке (`ThreadLocalContext`). Отсюда ограничения, проверенные запуском: настройка невиртуального члена даёт `CouldNotSetReturnDueToNoLastCallException` («Could not find a call to return from»), а вызов на **другой** подмене внутри `Returns(...)` может сбить «последний вызов».

> ⚠️ Проверить: детали внутренних типов (`CallRouter`, `ThreadLocalContext`) взяты из текста исключений и документации NSubstitute; исходники не изучались.

## Грани и подвохи

**1. Типы должны быть доступны прокси.** Проверено: `ICalculator` без `public` → `ArgumentException: Can not create proxy for type ICalculator because it is not accessible. Make it public, or internal and mark your assembly with [assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`.

**2. Невиртуальные члены не перехватываются.** Проверено: `CouldNotSetReturnDueToNoLastCallException`; у `ForPartsOf` невиртуальный член выполняется по-настоящему (`2`).

**3. Настройка внутри `Returns(...)`.** Не вызывайте другие подмены в аргументе `Returns` — вычислите значение заранее (сама библиотека предупреждает об этом в тексте исключения).

**4. Счётчик вызовов.** Проверено: `Received(3)` совпал с тремя реальными вызовами; вызов, использованный для настройки `Returns`, не учитывается. Но вызовы в самих проверках (`Received().Add(...)`) — это не вызовы реальной логики.

**5. `Received` без аргументов-проверок.** `calc.Received().Add(1, 2)` проверяет *конкретные* аргументы; для «любые» используйте `Arg.Any<T>()`.

**6. Последовательности.** Проверено: `Returns(1, 2, 3)` — после трёх значений повторяется последнее (`1, 2, 3, 3`).

**7. Автозначения и `null`.** Проверено: ненастроенный `Task<string?>`-метод вернул `null` внутри выполненной задачи; для не-nullable ссылочных типов будьте осторожны с `null`.

**8. Параллельные тесты.** Подмены потокобезопасны, но «последний вызов» — на поток; не настраивайте подмену из нескольких потоков одновременно.

**9. Над-специфицированные тесты.** Подмена каждого вызова и проверка порядка (`Received.InOrder`) связывают тест с реализацией — хрупкость.

> ⚠️ Проверить: `Received.InOrder` в этом материале не запускали.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| NSubstitute | читаемый синтаксис, без лямбд для настройки | рантайм-прокси, нет подмены `static`/`sealed`/невиртуальных |
| Moq | зрелый, много примеров | другой синтаксис (лямбды), история с зависимостями |
| FakeItEasy | похожий «беглый» синтаксис | меньше сообщество |
| Ручные заглушки (fake-классы) | полный контроль, без магии | больше кода |
| Настоящие зависимости (in-memory, [Testcontainers](../testcontainers/overview.ru.md)) | проверяют реальную интеграцию | медленнее, сложнее |

## Вопросы с собеседований

<details><summary>1. Что такое подмена (substitute/mock/stub/fake)?</summary>

Тестовый дубль зависимости: стаб возвращает заданные значения, мок ещё и проверяет вызовы, fake — упрощённая рабочая реализация.

</details>

<details><summary>2. Как NSubstitute создаёт подмены?</summary>

Через Castle DynamicProxy: во время выполнения генерируется класс-наследник/реализация интерфейса, который перехватывает вызовы.

</details>

<details><summary>3. Почему нельзя настроить невиртуальный метод класса?</summary>

Прокси перехватывает только `virtual`/`abstract` члены; иначе вызывается реальный код, и `Returns` не находит «последнего вызова» (проверено: `CouldNotSetReturnDueToNoLastCallException`).

</details>

<details><summary>4. Чем `Substitute.For<T>()` отличается от `ForPartsOf<T>()`?</summary>

`For` подменяет все члены (дефолт для ненастроенных); `ForPartsOf` выполняет реальный код для ненастроенных виртуальных членов (проверено: настроенный `8`, невиртуальный — реальный `2`).

</details>

<details><summary>5. Как проверить, что метод вызвали ровно N раз?</summary>

`sub.Received(N).Method(args)` (проверено: `Received(3)`); `DidNotReceive()` — ни разу.

</details>

<details><summary>6. Что вернёт ненастроенный метод?</summary>

Значение по умолчанию: `0`, `null`, для интерфейсов — рекурсивная подмена, для `Task` — завершённая задача (проверено).

</details>

<details><summary>7. Как задать разные результаты на последовательные вызовы?</summary>

`Returns(1, 2, 3)`: последнее значение повторяется (проверено `1, 2, 3, 3`).

</details>

<details><summary>8. Как выполнить действие при вызове `void`-метода?</summary>

`sub.When(x => x.Method(...)).Do(callInfo => …)` (проверено).

</details>

<details><summary>9. Подвох: `Substitute.For<IMyInternalInterface>()` падает с `ArgumentException`. Почему?</summary>

Прокси не видит `internal`-тип; добавьте `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` или сделайте тип публичным (проверено).

</details>

<details><summary>10. Подвох: `Received(2)` падает, хотя в тесте «два вызова». Что могли пропустить?</summary>

Дополнительные вызовы с теми же аргументами (например, при выводе/логировании в самом тесте — проверено: три вызова вместо двух); вызов для настройки `Returns` не считается.

</details>

<details><summary>11. Когда подмены вредны?</summary>

Когда тест проверяет реализацию (порядок, количество вызовов) вместо результата, или когда проверяется взаимодействие с БД/внешним сервисом — нужны интеграционные тесты.

</details>

## Связанные материалы

- Назад: [продвинутый](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md)
- [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md)
- [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md) — динамические прокси.

## Источники

- [NSubstitute: документация](https://nsubstitute.github.io/)
- [Castle DynamicProxy](https://www.castleproject.org/projects/dynamicproxy/)
- [NSubstitute: Creating a substitute](https://nsubstitute.github.io/help/creating-a-substitute/)
