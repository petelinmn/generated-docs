---
title: "Квиз: наблюдаемость и SLO"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15; Microsoft.Extensions.Telemetry 11.0.1"
status: draft
updated: 2026-11-26
---

# Квиз: наблюдаемость и SLO

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой сигнал показывает путь запроса по сервисам?

- A) Метрики
- B) Трассировка
- C) Конфигурация
- D) Дамп памяти

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Инструмент для числа обработанных запросов?

- A) `Counter`
- B) `Histogram`
- C) `Gauge`
- D) `ActivitySource`

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Чем в .NET представлен спан?

- A) `Task`
- B) `Activity`
- C) `Meter`
- D) `ILogger`

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что вернёт `StartActivity` без слушателя?

- A) Пустой объект
- B) `null`
- C) Исключение
- D) Фоновый спан

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Сколько символов в trace-id?

- A) 16
- B) 32
- C) 36
- D) 64

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что можно складывать между экземплярами?

- [ ] A) Значения счётчиков
- [ ] B) Корзины гистограмм с одинаковыми границами
- [ ] C) p99
- [ ] D) Число ошибок

<details><summary>Ответ</summary>

**A, B, D.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Какой тег опаснее всего для метрики?

- A) `outcome`
- B) `region`
- C) `order_id`
- D) `method`

<details><summary>Ответ</summary>

**C.** Неограниченное число значений. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что выведет код?

```csharp
var h = new Histogram(0.1, 0.5, 2);
h.Record(1.5);
Console.WriteLine(h.QuantileUpperBound(0.5));
```

<details><summary>Ответ</summary>

```text
2
```

Известна только граница корзины. → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что произойдёт без слушателя?

```csharp
var a = source.StartActivity("op");
a!.SetTag("k", 1);
```

<details><summary>Ответ</summary>

`NullReferenceException`. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Какие сигналы и алерты вы заведёте для сервиса оплаты?

<details><summary>Ключевые пункты ответа</summary>

- SLI с точки зрения пользователя и SLO.
- Частота, ошибки, длительность, зависимости.
- Burn rate в двух окнах и инструкция.
- Без персональных данных в телеметрии.

</details>
