---
title: "Квиз: несколько регионов и восстановление"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: несколько регионов и восстановление

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что измеряет RPO?

- A) Время восстановления
- B) Допустимую потерю данных по времени
- C) Число регионов
- D) Задержку сети

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Самая дешёвая стратегия?

- A) Backup and restore
- B) Pilot light
- C) Warm standby
- D) Active-active

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Разработчик случайно удалил таблицу. Что поможет?

- A) Реплика
- B) Восстановление на момент времени
- C) Балансировщик
- D) Кэш

<details><summary>Ответ</summary>

**B.** Реплика уже удалила то же самое. → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что уже работает в pilot light?

- A) Всё в полном объёме
- B) Репликация данных и основа инфраструктуры
- C) Ничего, даже копий
- D) Только балансировщик

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Зачем возвращаться по более длинной серии успехов?

- A) Чтобы не переключаться туда-обратно
- B) Чтобы сэкономить память
- C) Для шифрования
- D) Для кэша

<details><summary>Ответ</summary>

**A.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Выбери способы уменьшить конфликты в active-active

- [ ] A) Домашний регион пользователя
- [ ] B) Один регион на запись
- [ ] C) Подходящие CRDT
- [ ] D) Доверять часам серверов

<details><summary>Ответ</summary>

**A, B, C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что выведет код?

```csharp
var c = new FailoverController("eu", "us", failAfter: 3, recoverAfter: 5);
c.Probe(false); c.Probe(false); c.Probe(true); c.Probe(false);
Console.WriteLine(c.Active);
```

<details><summary>Ответ</summary>

```text
eu
```

Успех обнулил серию неудач. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Из чего складывается время простоя?

- A) Только из копирования
- B) Обнаружение, решение, повышение, перенаправление
- C) Только из DNS
- D) Из лага репликации

<details><summary>Ответ</summary>

**B.** Лаг влияет на потерю данных. → [Базовая теория](basics.ru.md)

</details>

## 9. 🔴 Что выведет код?

```csharp
var t = new DateTime(2026, 3, 29, 12, 0, 0, DateTimeKind.Unspecified);
Console.WriteLine(JsonSerializer.Serialize(t));
```

<details><summary>Ответ</summary>

```text
"2026-03-29T12:00:00"
```

Смещения нет, значение неоднозначно. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Составьте план DR для интернет-магазина.

<details><summary>Ключевые пункты ответа</summary>

- RPO и RTO по функциям: заказы и каталог различаются.
- Стратегия, копии, изоляция.
- Кто и как переключает, failback.
- Учения и измерения.

</details>
