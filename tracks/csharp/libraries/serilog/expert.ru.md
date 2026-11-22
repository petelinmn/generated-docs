---
title: "Serilog: экспертный уровень"
lang: ru
track: csharp
kind: library
doc: expert
slug: serilog
part: 2
tags: [serilog, custom-sink, destructuring, performance, async-sink, interview-questions]
versions: ".NET 11, C# 15; Serilog 4.4.0"
status: draft
updated: 2026-11-22
---

# Serilog: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md) и [продвинутый](advanced.ru.md). **Время:** ~30 мин.

## Внутреннее устройство

Конвейер: вызов `Log.Information(template, args)` → проверка минимального уровня (дёшево, до разбора) → разбор шаблона (кэшируется) → создание `LogEvent` со свойствами (`LogEventProperty`) → обогатители → фильтры → все sinks. Свойство — это `LogEventPropertyValue`: `ScalarValue`, `StructureValue` (при `@`), `SequenceValue`, `DictionaryValue`. Проверено: значение `Count` — `ScalarValue`, а `{@Order}` — структура.

Собственный sink — реализация `ILogEventSink.Emit(LogEvent)` (так в примерах перехватывались события для проверки):

```csharp
class DelegateSink(Action<LogEvent> a) : ILogEventSink { public void Emit(LogEvent e) => a(e); }
.WriteTo.Sink(new DelegateSink(captured.Add))
```

Каждый sink получает событие синхронно в вызывающем потоке; медленные sinks (сеть, БД) оборачивают в асинхронную обёртку (`Serilog.Sinks.Async`) с ограниченной очередью.

> ⚠️ Проверить: `Serilog.Sinks.Async`, `Seq`/`Elasticsearch` sinks и их надёжность доставки в этом материале не запускались.

## Грани и подвохи

**1. Интерполяция ломает структурность.** Проверено: `Log.Information($"interpolated {user}")` дал шаблон-строку без свойств.

**2. `{Order}` без `@` — это `ToString()`.** Проверено: строка `Order { … }` вместо структуры.

**3. Культура в строковом виде.** Проверено: в `ToString()` записи `decimal` вышло `9,5` (текущая культура), в структуре — число `9.5`. Для стабильного вывода задайте `formatProvider` в sink или инвариантную культуру.

**4. `Enrich.FromLogContext()` обязателен.** Без него `LogContext.PushProperty` ничего не добавит.

**5. `Override` по префиксу.** Проверено: `Noisy` действует на `Noisy` и `Noisy.Thing`, но не на `Other`.

**6. Деструктуризация сложных графов.** `{@obj}` обходит объект; циклические ссылки и огромные коллекции ограничивайте (`Destructure.ToMaximumDepth`, `ToMaximumCollectionCount`, политики `Destructure.ByTransforming`).

> ⚠️ Проверить: имена и поведение методов ограничения деструктуризации в этом материале не запускались.

**7. Потеря хвоста.** Без `Log.CloseAndFlush()` буферизованные sinks (файл, сеть) могут не дописать последние сообщения.

**8. Конфликт имён свойств.** Свойство `SourceContext`/`Scope` зарезервировано под служебные цели; своё имя выбирайте иначе.

**9. Стоимость.** Проверка уровня дешёвая, но создание аргументов (дорогие вычисления перед вызовом) идёт всегда — защищайте `Log.IsEnabled(LogEventLevel.Debug)` или `LoggerMessage` для горячих путей.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Serilog | экосистема sinks, структурность, `@`-деструктуризация | лишняя зависимость, синхронные sinks |
| `Microsoft.Extensions.Logging` без Serilog | встроено, `LoggerMessage` source generator | меньше sinks/обогащения |
| NLog | зрелый, гибкая конфигурация | другая модель |
| OpenTelemetry Logs | единый стандарт для логов/метрик/трейсов | развивается |
| Сторонний агент (stdout → collector) | приложение просто пишет в stdout | нужна инфраструктура |

## Вопросы с собеседований

<details><summary>1. Что такое структурное логирование?</summary>

Сообщения — шаблон + именованные значения; значения хранятся как данные и доступны для поиска и фильтрации (проверено: `User`, `Count` — отдельные свойства).

</details>

<details><summary>2. Чем `{@Order}` отличается от `{Order}`?</summary>

`@` — деструктуризация в структуру со свойствами; без него вызывается `ToString()` (проверено).

</details>

<details><summary>3. Почему нельзя логировать интерполированной строкой?</summary>

Теряется шаблон и свойства: остаётся готовая строка без полей (проверено).

</details>

<details><summary>4. Что такое sink?</summary>

Получатель событий: консоль, файл, сеть. Свой sink — `ILogEventSink`.

</details>

<details><summary>5. Как добавить свойство ко всем сообщениям запроса?</summary>

`LogContext.PushProperty` при включённом `Enrich.FromLogContext()` (проверено).

</details>

<details><summary>6. Как приглушить шумный источник?</summary>

`MinimumLevel.Override("Namespace", LogEventLevel.Warning)` (проверено по префиксу).

</details>

<details><summary>7. Как интегрировать Serilog с `ILogger<T>`?</summary>

`AddSerilog` (Serilog.Extensions.Logging) или `UseSerilog` в хосте; сообщения `ILogger` приходят как события Serilog со свойствами (проверено).

</details>

<details><summary>8. Зачем `Log.CloseAndFlush()`?</summary>

Сбросить буферы sinks при завершении, иначе хвост логов может потеряться.

</details>

<details><summary>9. Подвох: в логе есть `{RequestId}` в одном сообщении, но нет в соседнем. Почему?</summary>

Свойство задано через `LogContext.PushProperty` в области `using`; вне области его нет (проверено).

</details>

<details><summary>10. Подвох: `decimal` в сообщении выводится с запятой, а в JSON — с точкой. Почему?</summary>

Текстовое представление использует текущую культуру, структурированное значение — число (проверено: `9,5` против `9.5`). Задайте инвариантную культуру/`formatProvider`.

</details>

<details><summary>11. Как не терять производительность на `Debug`-логах в горячем пути?</summary>

Фильтр по уровню срабатывает до формирования события, но дорогие аргументы вычисляются заранее — оберните в `IsEnabled` или используйте `LoggerMessage`.

</details>

## Связанные материалы

- Назад: [продвинутый](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md)
- [19 · Производительность](../../modules/19-performance/overview.ru.md) — стоимость логирования.
- [Kubernetes](../../../kubernetes/overview.ru.md) — логи контейнеров и сборщики.

## Источники

- [Serilog wiki](https://github.com/serilog/serilog/wiki)
- [Serilog: Developing a sink](https://github.com/serilog/serilog/wiki/Developing-a-sink)
- [Serilog: Structured data](https://github.com/serilog/serilog/wiki/Structured-Data)
