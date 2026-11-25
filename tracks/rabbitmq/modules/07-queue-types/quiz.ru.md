---
title: "Квиз: типы очередей"
lang: ru
track: rabbitmq
kind: module
doc: quiz
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, quorum-queue, stream, delivery-limit]
status: draft
updated: 2026-11-25
---

# Квиз: типы очередей

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Каким типом будет очередь, объявленная без `x-queue-type`?

- A) quorum
- B) stream
- C) classic
- D) Ошибка: тип обязателен

<details><summary>Ответ</summary>

**C.** Если тип не указан и значение по умолчанию не меняли, создаётся classic. Quorum и stream запрашивают явно, и аргумент не обязателен.
→ [Основы](basics.ru.md#три-типа-очередей)

</details>

## 2. 🟢 Какой тип подходит для событий, которые разные потребители читают с разных позиций?

- A) classic
- B) quorum
- C) stream
- D) Любой

<details><summary>Ответ</summary>

**C.** Stream — неразрушающий журнал со смещениями.
→ [Основы](basics.ru.md#stream-журнал-вместо-очереди)

</details>

## 3. 🟢 Что даёт quorum-очередь по сравнению с classic?

- A) Приоритеты сообщений
- B) Репликацию на основе консенсуса
- C) Exclusive-режим
- D) Меньше накладных расходов

<details><summary>Ответ</summary>

**B.** Приоритеты и exclusive у quorum не поддерживаются.
→ [Основы](basics.ru.md#три-типа-очередей)

</details>

## 4. 🟢 Можно ли поменять тип существующей очереди повторным объявлением?

- A) Да
- B) Нет, `406 PRECONDITION_FAILED`
- C) Да, но только classic → quorum
- D) Нет, брокер молча проигнорирует

<details><summary>Ответ</summary>

**B.** Тип неизменяем: повторное объявление с другим вызывает ошибку канала, а не преобразование и не молчаливое игнорирование. Нужна новая очередь и перенос.
→ [Основы](basics.ru.md#ограничения-quorum-очередей-и-stream)

</details>

## 5. 🟡 Что произойдёт при `queue_declare(..., durable=False, arguments={"x-queue-type": "quorum"})` на 4.4?

- A) Канал закроется с `406`
- B) Соединение закроется с `541`
- C) Очередь создастся
- D) Тип заменится на classic

<details><summary>Ответ</summary>

**B.** Не-durable не-exclusive очереди запрещены.
→ [Подвох 3](expert.ru.md#3-quorum-без-durable)

</details>

## 6. 🟡 Потребитель stream без `basic_qos`. Результат?

- A) Работает
- B) `406 … consumer prefetch count is not set for stream queue`
- C) Читается только одно сообщение
- D) Брокер сам выберет prefetch

<details><summary>Ответ</summary>

**B.** Для stream prefetch обязателен, без него брокер отклоняет подписку. Сам значение он не выбирает.
→ [Основы](basics.ru.md#stream-журнал-вместо-очереди)

</details>

## 7. 🟡 Quorum-очередь с `x-delivery-limit: 3`. Потребитель делает `basic_reject(requeue=True)`. Что будет?

- A) Бесконечный цикл
- B) 4 доставки, затем DLX с причиной `delivery_limit`
- C) Сообщение сразу в DLX
- D) Закрытие канала

<details><summary>Ответ</summary>

**B.** В тесте: `deliveries 4 … dead 1`.
→ [Продвинутая теория](advanced.ru.md#delivery-limit)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) Чтение из stream не удаляет сообщения
- [ ] B) `x-stream-offset: "next"` вернёт только новые сообщения
- [ ] C) Число (например, 3) допустимо как `x-stream-offset`
- [ ] D) Для stream допустим `auto_ack=True`

<details><summary>Ответ</summary>

**A, B, C.** D неверно: `406`.
→ [Продвинутая теория](advanced.ru.md#смещения-в-stream)

</details>

## 9. 🟡 Что вернёт `x-stream-offset: "last"` при шести сообщениях в одном чанке?

- A) Последнее сообщение
- B) Все шесть
- C) Ничего
- D) Ошибку

<details><summary>Ответ</summary>

**B.** `last` начинает с последнего чанка.
→ [Продвинутая теория](advanced.ru.md#смещения-в-stream)

</details>

## 10. 🔴 Найдите ошибку

```python
def on_msg(ch, m, p, body):
    try:
        handle(body)
        ch.basic_ack(m.delivery_tag)
    except Exception:
        ch.basic_nack(m.delivery_tag, requeue=True)   # relying on x-delivery-limit
```

<details><summary>Ответ</summary>

В тесте `nack(requeue=True)` не увеличивал счётчик доставок: сообщение крутилось по кругу (582 доставки за 2 с) и в DLX не попадало. Используйте `reject(requeue=True)` (счётчик растёт) или `requeue=False` с DLX.
→ [Подвох 1](expert.ru.md#1-nackrequeuetrue-и-delivery-limit)

</details>

## 11. 🔴 Какая очередь подходит для задач, которые нельзя потерять при отказе узла кластера?

- A) classic
- B) quorum
- C) stream с `auto_ack`
- D) exclusive classic

<details><summary>Ответ</summary>

**B.** Quorum реплицируется. (В этом модуле репликация не проверялась — один узел; см. [модуль 08](../08-clustering/overview.ru.md).)
→ [Основы](basics.ru.md#что-выбрать)

</details>

## 12. 🔴 Открытый вопрос

Сервису нужны: очередь задач, которую нельзя терять, и журнал событий для трёх аналитических потребителей. Какие типы очередей и настройки вы выберете?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Задачи: quorum-очередь, durable, `x-delivery-limit` и DLX; ручной ack, `reject` для неисправимых ошибок.
- События: stream, `x-stream-offset` у каждого потребителя, prefetch и ack, политика хранения по размеру/времени.
- Миграция существующих classic-очередей: новая очередь и перенос (тип не меняется).
- Следить за размером stream и глубиной DLX.

</details>

→ [Продвинутая теория](advanced.ru.md)
