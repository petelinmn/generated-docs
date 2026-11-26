---
title: "Квиз: архитектурные стили"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 11-architecture-styles
part: 2
tags: [system-design, architecture, modular-monolith, microservices, fitness-function, adr]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: архитектурные стили

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что такое монолит?

- A) Плохой код
- B) Одна единица развёртывания
- C) Одна таблица
- D) Один разработчик

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Из чего состоит Web-Queue-Worker?

- A) Веб-часть, очередь, фоновый обработчик
- B) Три базы
- C) Только функции
- D) Два балансировщика

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что чаще всего нарушает независимость сервисов?

- A) HTTP
- B) Общая база
- C) Контейнеры
- D) Логи

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Как общаются модули модульного монолита?

- A) Через внутренние классы
- B) Через контракты
- C) Через глобальные переменные
- D) Никак

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `Ca = 4`, `Ce = 1`. Неустойчивость?

- A) 0,2
- B) 0,25
- C) 0,8
- D) 4

<details><summary>Ответ</summary>

**A.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что такое strangler fig?

- A) Переписать всё разом
- B) Постепенно переводить пути на новую реализацию
- C) Удалить старую базу
- D) Заморозить изменения

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери верные причины выделить сервис

- [ ] A) Независимые релизы
- [ ] B) Отличающаяся нагрузка
- [ ] C) Изоляция отказов
- [ ] D) «Так делают все»

<details><summary>Ответ</summary>

**A, B, C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что выведет код?

```csharp
var (latency, availability) = NetworkCost.Chain(5, TimeSpan.FromMilliseconds(20), 0.999);
Console.WriteLine(latency.TotalMilliseconds);
```

<details><summary>Ответ</summary>

```text
100
```

Последовательные задержки складываются. → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что выведет код?

```csharp
Action handlers = () => Console.WriteLine("first");
handlers += () => throw new InvalidOperationException();
handlers += () => Console.WriteLine("third");
try { handlers(); } catch (InvalidOperationException) { Console.WriteLine("caught"); }
```

<details><summary>Ответ</summary>

```text
first
caught
```

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Команда из шести человек делает новый продукт. Какой стиль выберете?

<details><summary>Ключевые пункты ответа</summary>

- Модульный монолит как отправная точка.
- Явные контракты и тесты границ.
- Условия, при которых выделять сервис.
- ADR с альтернативами.

</details>
