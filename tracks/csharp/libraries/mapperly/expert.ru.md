---
title: "Mapperly: экспертный уровень"
lang: ru
track: csharp
kind: library
doc: expert
slug: mapperly
part: 2
tags: [mapperly, automapper, source-generator, diagnostics, interview-questions]
versions: ".NET 11, C# 15; Riok.Mapperly 4.3.1"
status: draft
updated: 2026-11-22
---

# Mapperly: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md) и [продвинутый](advanced.ru.md). **Время:** ~30 мин.

## Внутреннее устройство

Mapperly — инкрементальный Roslyn source generator. На этапе компиляции он находит классы с `[Mapper]`, анализирует сигнатуры `partial`-методов и типы источника/приёмника по семантической модели, строит дерево отображений (свойства, вложенные объекты, коллекции, enum, пользовательские методы) и выдаёт обычный C#-код реализаций (`OrderMapper.g.cs`). Проверено: для `ToDto` получены прямые присваивания `target.Id = o.Id; target.CustomerName = o.Customer.Name; …`, для списка — `Enumerable.ToList(Enumerable.Select(orders, x => ToDto(x)))`, для `IQueryable` — `Queryable.Select(q, x => new OrderDto { … })`.

Следствия:

- нет рефлексии и IL-эмита во время выполнения → совместимость с trimming и Native AOT, скорость как у ручного кода;
- ошибки маппинга — **ошибки/предупреждения компиляции**, а не исключения в рантайме;
- сгенерированный код можно прочитать, поставить точку останова и сравнить в code review.

Чтобы увидеть код: `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` и исключить папку вывода из компиляции (`<Compile Remove="gen/**" />`) — иначе CS0757 (проверено).

## Грани и подвохи

**1. enum по значению.** Проверено: `(StatusText)s`; несовпадающие числа — тихая ошибка.

**2. Конфигурация на проекции.** Проверено: `RMG065` при `MapProperty` на `IQueryable`-методе.

**3. Предупреждения о неотображённых членах.** Проверено: `RMG020` для `Secret`, `Created` и обратного направления; их можно повысить до ошибок.

**4. `partial` обязателен.** Класс и метод должны быть `partial`.

**5. Дубликаты при выводе генерируемых файлов.** Проверено: CS0757.

**6. Обновление существующего объекта.** Проверено: `Update` не трогает несопоставленные свойства цели (`Label` осталось `keep?`).

**7. Глубина и циклы.** Циклические ссылки между объектами требуют отдельной стратегии (иначе бесконечная рекурсия в рантайме).

> ⚠️ Проверить: поведение при циклических ссылках и `UseReferenceHandling` в этом материале не запускали.

**8. Проекция и EF Core.** Вызовы собственных методов внутри выражения не транслируются в SQL; Mapperly для `IQueryable` генерирует инлайн-выражение, но пользовательские методы с логикой в нём использовать нельзя.

**9. Версии.** Опции и имена диагностик меняются между версиями — сверяйтесь с документацией вашей версии.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Mapperly | без рефлексии, AOT, видимый код, диагностика на сборке | нужно объявлять каждый маппинг |
| AutoMapper | гибкие конвенции, большое сообщество | рефлексия/дерево выражений, ошибки проявляются в рантайме |
| Ручные методы/`ToDto` | максимум контроля | много шаблонного кода |
| Проекция через LINQ `Select` | простой SQL | дублирование в запросах |
| Mapster | генерация кода, быстрый | другой API |

## Вопросы с собеседований

<details><summary>1. Чем Mapperly отличается от AutoMapper?</summary>

Mapperly — source generator: код маппинга создаётся при компиляции без рефлексии; ошибки видны при сборке. AutoMapper строит маппинг в рантайме.

</details>

<details><summary>2. Как Mapperly сопоставляет свойства?</summary>

По совпадающим именам; прочее — `[MapProperty]`; ненужное — `[MapperIgnoreTarget]`/`[MapperIgnoreSource]` (проверено).

</details>

<details><summary>3. Что такое `RMG020`?</summary>

Предупреждение о члене источника, не отображённом в приёмник (проверено); помогает не терять поля.

</details>

<details><summary>4. Как сопоставляются enum по умолчанию?</summary>

По значению (`(StatusText)s`, проверено); по имени — явной стратегией.

</details>

<details><summary>5. Как использовать Mapperly с EF Core?</summary>

Метод над `IQueryable<T>` даёт выражение `Select(x => new Dto { … })`, которое транслируется в SQL.

</details>

<details><summary>6. Зачем нужен `partial`?</summary>

Генератор дописывает реализацию методов в другую часть класса.

</details>

<details><summary>7. Как обновить существующий объект?</summary>

`partial void Update(Source s, Target t)`; несопоставленные свойства цели остаются (проверено).

</details>

<details><summary>8. Как посмотреть сгенерированный код?</summary>

`EmitCompilerGeneratedFiles` + исключить папку из компиляции (иначе CS0757, проверено), либо через IDE.

</details>

<details><summary>9. Подвох: после включения `EmitCompilerGeneratedFiles` проект не собирается с CS0757. Почему?</summary>

Сгенерированные файлы лежат внутри проекта и подхватываются стандартной маской компиляции — реализация объявлена дважды. Исключите папку: `<Compile Remove="gen/**" />`.

</details>

<details><summary>10. Подвох: enum-значения в двух типах пронумерованы по-разному, а маппинг «работает». Что не так?</summary>

Стратегия по значению: числа копируются как есть, имена не сверяются (проверено вид кода `(StatusText)s`). Используйте сопоставление по имени.

</details>

<details><summary>11. Когда лучше ручной маппинг, чем Mapperly?</summary>

Когда логика преобразования нетривиальна и не повторяется; для простых DTO-преобразований генератор снижает шаблонный код.

</details>

## Связанные материалы

- Назад: [продвинутый](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md)
- [19 · Производительность](../../modules/19-performance/overview.ru.md) — стоимость рефлексии.
- [BenchmarkDotNet](../benchmarkdotnet/overview.ru.md) — измерение маппинга.

## Источники

- [Mapperly docs](https://mapperly.riok.app/docs/intro/)
- [Mapperly: Analyzer diagnostics](https://mapperly.riok.app/docs/configuration/analyzer-diagnostics/)
- [Roslyn incremental generators](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
