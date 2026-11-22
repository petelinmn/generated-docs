---
title: "Квиз: Mapperly"
lang: ru
track: csharp
kind: library
doc: quiz
slug: mapperly
part: 2
tags: [mapperly, object-mapping, source-generator]
status: draft
updated: 2026-11-22
---

# Квиз: Mapperly

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка: 🟢 [основы](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [эксперт](expert.ru.md).

## 1. 🟢 Что такое Mapperly?

- A) рефлексивный маппер, строящий правила в рантайме
- B) source generator, который при компиляции пишет код маппинга
- C) ORM
- D) библиотека логирования

<details><summary>Ответ</summary>

**B.** Результат — обычные присваивания без рефлексии.
→ [Основы](basics.ru.md#что-это-и-зачем)

</details>

## 2. 🟢 Что обязательно для класса маппера?

- A) наследование от `Mapper`
- B) атрибут `[Mapper]` и модификатор `partial`
- C) `IDisposable`
- D) статические поля

<details><summary>Ответ</summary>

**B.** Генератор дописывает реализацию `partial`-методов.
→ [Основы](basics.ru.md#первый-маппер)

</details>

## 3. 🟢 Как сопоставить `Customer.Name` с `CustomerName`?

- A) само
- B) `[MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]`
- C) `Include`
- D) нельзя

<details><summary>Ответ</summary>

**B.** Проверено: `target.CustomerName = o.Customer.Name;`.
→ [Основы](basics.ru.md#первый-маппер)

</details>

## 4. 🟢 Что означает предупреждение `RMG020`?

- A) синтаксическая ошибка
- B) член источника не отображён ни на какой член приёмника
- C) enum не поддерживается
- D) нет `partial`

<details><summary>Ответ</summary>

**B.** Проверено: для `Secret` и `Created`; явно пометьте `MapperIgnoreSource`.
→ [Основы](basics.ru.md#предупреждения-вместо-сюрпризов)

</details>

## 5. 🟡 Как по умолчанию сопоставляются enum?

- A) по имени
- B) по значению (`(StatusText)s`)
- C) через строки
- D) нельзя

<details><summary>Ответ</summary>

**B.** Проверено по сгенерированному коду; по имени — явной стратегией.
→ [Продвинутый](advanced.ru.md#перечисления)

</details>

## 6. 🟡 Что даёт метод `IQueryable<OrderDto> Project(IQueryable<Order> q)`?

- A) загружает всё в память
- B) выражение `Select(x => new OrderDto { … })`, транслируемое провайдером
- C) копию коллекции
- D) ничего

<details><summary>Ответ</summary>

**B.** Проверено на сгенерированном коде.
→ [Продвинутый](advanced.ru.md#проекции-для-iqueryable)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `Update(source, target)` изменяет существующий объект
- [ ] B) `partial OrderDto? Map(Order? o)` вернёт `null` для `null`
- [ ] C) настройки `[MapProperty]` нужно навешивать на метод `IQueryable`-проекции
- [ ] D) обратное направление требует отдельного метода

<details><summary>Ответ</summary>

**A, B, D.** C неверно: даёт `RMG065`; настраивайте объектный метод.
→ [Продвинутый](advanced.ru.md#обратный-маппинг-и-null)

</details>

## 8. 🟡 Открытый вопрос: как организовать маппинг DTO в проекте с EF Core?

<details><summary>Ключевые пункты хорошего ответа</summary>

- `[Mapper] partial class` с методами модель↔DTO;
- проекции `IQueryable` для чтения (только нужные столбцы);
- явная стратегия enum, предупреждения как ошибки;
- маппер без бизнес-логики, регистрируется как singleton.

</details>

## 9. 🔴 После `EmitCompilerGeneratedFiles` проект не собирается (CS0757). Почему?

- A) устаревший SDK
- B) сгенерированные файлы лежат в проекте и компилируются дважды
- C) enum не поддерживается
- D) отсутствует `partial`

<details><summary>Ответ</summary>

**B.** Исключите папку: `<Compile Remove="gen/**" />` (проверено).
→ [Эксперт](expert.ru.md#внутреннее-устройство)

</details>

## 10. 🔴 Почему Mapperly совместим с Native AOT?

- A) использует кэш
- B) не использует рефлексию и IL-эмит во время выполнения — код сгенерирован заранее
- C) работает только на сервере
- D) преобразует типы в строки

<details><summary>Ответ</summary>

**B.** Итоговый код — обычный C#.
→ [Эксперт](expert.ru.md#внутреннее-устройство)

</details>
