---
title: "Квиз: Serilog"
lang: ru
track: csharp
kind: library
doc: quiz
slug: serilog
part: 2
tags: [serilog, structured-logging, sinks, enrichers]
status: draft
updated: 2026-11-22
---

# Квиз: Serilog

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка: 🟢 [основы](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [эксперт](expert.ru.md).

## 1. 🟢 Что такое структурное логирование?

- A) логи в таблице
- B) шаблон сообщения с именованными свойствами, значения сохраняются как данные
- C) логи в XML
- D) только JSON-вывод

<details><summary>Ответ</summary>

**B.** Значения (`User`, `Count`) — отдельные свойства, по которым можно искать.
→ [Основы](basics.ru.md#что-это-и-зачем)

</details>

## 2. 🟢 Какая запись правильная?

- A) `Log.Information($"user {user}")`
- B) `Log.Information("user {User}", user)`
- C) `Log.Information("user " + user)`
- D) `Log.Information("user {0}")`

<details><summary>Ответ</summary>

**B.** Проверено: интерполяция даёт готовую строку без свойств.
→ [Основы](basics.ru.md#частые-ошибки-новичка)

</details>

## 3. 🟢 Что делает `{@Order}`?

- A) вызывает `ToString()`
- B) деструктурирует объект в структуру со свойствами
- C) игнорирует значение
- D) шифрует

<details><summary>Ответ</summary>

**B.** Проверено: `{"Id": 7, "Customer": "Ann", "Total": 9.5, "$type": "Order"}`; без `@` — `ToString()`.
→ [Основы](basics.ru.md#шаблоны-сообщений-и-свойства)

</details>

## 4. 🟢 Зачем `Log.CloseAndFlush()`?

- A) для красоты
- B) сбросить буферы sinks при завершении
- C) чтобы изменить уровень
- D) для ротации файлов

<details><summary>Ответ</summary>

**B.** Иначе последние сообщения могут не записаться.
→ [Основы](basics.ru.md#первая-конфигурация)

</details>

## 5. 🟡 Что нужно, чтобы `LogContext.PushProperty` работал?

- A) ничего
- B) `.Enrich.FromLogContext()`
- C) `.WriteTo.Console()`
- D) `MinimumLevel.Verbose()`

<details><summary>Ответ</summary>

**B.** Проверено: внутри области свойство `RequestId` есть, снаружи нет.
→ [Продвинутый](advanced.ru.md#обогащение-и-контекст)

</details>

## 6. 🟡 Что делает `MinimumLevel.Override("Noisy", Warning)`?

- A) отключает логи
- B) для источников с префиксом `Noisy` пропускаются только `Warning` и выше
- C) меняет уровень всех логов
- D) удаляет sink

<details><summary>Ответ</summary>

**B.** Проверено: info из `Noisy.Thing` отброшен, warning прошёл; `Other` не затронут.
→ [Продвинутый](advanced.ru.md#переопределение-уровней-по-источнику)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `CompactJsonFormatter` пишет одно событие на строку
- [ ] B) `AddSerilog` позволяет Serilog принимать сообщения `ILogger`
- [ ] C) числа в JSON-логе сохраняются строками
- [ ] D) свойства области `BeginScope` попадают в событие

<details><summary>Ответ</summary>

**A, B, D.** C неверно: `"Count":3` — число.
→ [Продвинутый](advanced.ru.md#интеграция-с-microsoftextensionslogging)

</details>

## 8. 🟡 Открытый вопрос: как настроить логи так, чтобы все сообщения запроса несли `RequestId`?

<details><summary>Ключевые пункты хорошего ответа</summary>

- `Enrich.FromLogContext()`;
- в middleware — `using (LogContext.PushProperty("RequestId", id))` вокруг обработки запроса;
- вывод в JSON, поиск по `RequestId` в системе логов.

</details>

## 9. 🔴 Почему `decimal` в тексте `9,5`, а в JSON `9.5`?

- A) баг
- B) текст использует текущую культуру, структурированное значение — число
- C) разные версии Serilog
- D) из-за кодировки

<details><summary>Ответ</summary>

**B.** Проверено; используйте инвариантную культуру/`formatProvider`.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Как не вычислять дорогие аргументы `Debug`-лога, когда `Debug` выключен?

- A) никак
- B) обернуть в `Log.IsEnabled(LogEventLevel.Debug)` или использовать `LoggerMessage`
- C) использовать интерполяцию
- D) увеличить уровень sink

<details><summary>Ответ</summary>

**B.** Фильтр срабатывает до формирования события, но аргументы вычисляются раньше.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>
