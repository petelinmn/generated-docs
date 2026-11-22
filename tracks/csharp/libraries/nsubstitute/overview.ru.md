---
title: "NSubstitute"
lang: ru
track: csharp
kind: library
doc: overview
slug: nsubstitute
part: 2
levels: [1, 2, 3, 4]
tags: [nsubstitute, mocking, unit-testing, test-doubles]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute

🌐 **Русский** · [English](overview.en.md)

## Коротко

NSubstitute создаёт тестовые дубли интерфейсов и классов на базе Castle DynamicProxy. Синтаксис — естественный: вызвали метод подмены и добавили `.Returns(...)`; проверка — `Received()`/`DidNotReceive()`. Умеет аргументы-шаблоны, последовательности, async, события, колбэки `When…Do`, рекурсивные подмены и частичные подмены классов. Ограничения: подмена только публичных (или видимых прокси) типов и виртуальных членов.

## Ключевые идеи

- `Substitute.For<T>()`; ненастроенное → значения по умолчанию.
- `x.Method(args).Returns(value)`; `Arg.Any/Is/Do` для аргументов.
- `Received(n)`/`DidNotReceive()` для проверки вызовов (вызов настройки не считается).
- `When…Do` для `void`-методов; `Throws`/`ThrowsAsync` для ошибок.
- Рекурсивные подмены для методов, возвращающих интерфейсы.
- Классы: только `virtual`; `ForPartsOf` выполняет реальный код.
- `internal`-типы требуют `InternalsVisibleTo("DynamicProxyGenAssembly2")`.

## Минимальный пример

```csharp
var calc = Substitute.For<ICalculator>();
calc.Add(1, 2).Returns(3);
Assert.Equal(3, calc.Add(1, 2));
calc.Received().Add(1, 2);
```

## Когда применять, а когда нет

- ✅ Юнит-тесты кода с внешними зависимостями.
- ✅ Нужно проверить значимые побочные эффекты.
- ✅ Интерфейсы клиентов, репозиториев, часов.
- ❌ Подмена тестируемого класса целиком.
- ❌ Проверка порядка и числа всех внутренних вызовов.
- ❌ Проверка интеграции с БД/сетью — нужны интеграционные тесты.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | создание, `Returns`, `Received`, аргументы, последовательности, async |
| [2. Продвинутый](advanced.ru.md) | колбэки, события, рекурсивные подмены, классы, `ForPartsOf` |
| [3. Эксперт](expert.ru.md) | устройство (прокси), подвохи, альтернативы, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp nsubstitute level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp nsubstitute level 6` |

## Связанные материалы

- Пререквизиты: [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md), [05 · Интерфейсы, структуры, records](../../modules/05-interfaces-structs-records/overview.ru.md)
- Рядом: [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md), [Testcontainers](../testcontainers/overview.ru.md), [Refit](../refit/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — `unittest.mock`
