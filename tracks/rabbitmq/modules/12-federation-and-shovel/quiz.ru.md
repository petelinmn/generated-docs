---
title: "Квиз: Federation и Shovel"
lang: ru
track: rabbitmq
kind: module
doc: quiz
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream, multi-datacenter]
status: draft
updated: 2026-11-24
---

# Квиз: Federation и Shovel

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Для чего подходят Shovel и Federation?

- A) Для связи независимых брокеров, например в разных дата-центрах
- B) Для объединения узлов в кластер
- C) Для шифрования трафика
- D) Для ускорения очередей

<details><summary>Ответ</summary>

**A.**
→ [Основы](basics.ru.md#зачем-соединять-брокеры)

</details>

## 2. 🟢 Что происходит с сообщением в источнике после переноса shovel?

- A) Остаётся (копия)
- B) Удаляется (перемещение)
- C) Помечается прочитанным
- D) Уходит в DLX

<details><summary>Ответ</summary>

**B.** В тесте `q.src` опустел.
→ [Основы](basics.ru.md#shovel-из-очереди-в-очередь)

</details>

## 3. 🟢 Как состояние shovel можно увидеть?

- A) `rabbitmqctl shovel_status`
- B) `rabbitmqctl list_exchanges`
- C) `rabbitmq-upgrade drain`
- D) `rabbitmqctl cluster_status`

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md#управление-shovel)

</details>

## 4. 🟢 Кластер или отдельные брокеры между дата-центрами?

- A) Кластер
- B) Независимые брокеры, связанные Shovel/Federation
- C) Всё равно
- D) Только один брокер

<details><summary>Ответ</summary>

**B.** Кластер рассчитан на быструю надёжную сеть.
→ [Основы](basics.ru.md#зачем-соединять-брокеры)

</details>

## 5. 🟡 Какое состояние и причину показал shovel при остановленном источнике?

- A) `running`
- B) `terminated`, `failed to connect to source`
- C) `paused`
- D) `deleted`

<details><summary>Ответ</summary>

**B.** После возвращения источника shovel восстановился.
→ [Продвинутая теория](advanced.ru.md#сбой-источника)

</details>

## 6. 🟡 Как включить федерацию для обменников `fed.*`?

- A) Параметр `federation-upstream` и политика с `federation-upstream-set` на шаблон `^fed\.`
- B) `rabbitmqctl join_cluster`
- C) `x-queue-type: federated`
- D) `basic_qos`

<details><summary>Ответ</summary>

**A.**
→ [Основы](basics.ru.md#federation-общий-обменник)

</details>

## 7. 🟡 Какие гарантии доставки у shovel?

- A) Ровно один раз
- B) Не более одного раза
- C) Минимум один раз (возможны дубликаты)
- D) Никаких

<details><summary>Ответ</summary>

**C.** (по документации; сбои между доставкой и подтверждением здесь не воспроизводились).
→ [Продвинутая теория](advanced.ru.md#сбой-источника)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) Shovel перемещает сообщения
- [ ] B) Federation по умолчанию однонаправленная
- [ ] C) Shovel и Federation объединяют брокеры в кластер
- [ ] D) Связь стоит мониторить (`shovel_status`, `federation_status`)

<details><summary>Ответ</summary>

**A, B, D.** C неверно: брокеры остаются независимыми.
→ [Подвохи](expert.ru.md#подвохи)

</details>

## 9. 🟡 Какую учётную запись использовать для связи?

- A) `guest` с паролем по умолчанию
- B) Отдельного пользователя с минимальными правами и TLS
- C) Администратора
- D) Анонимный доступ

<details><summary>Ответ</summary>

**B.**
→ [Подвох 7](expert.ru.md#7-guest-для-связи)

</details>

## 10. 🔴 Найдите ошибку

> «Мы растянули кластер RabbitMQ на два дата-центра, потому что между ними есть VPN».

<details><summary>Ответ</summary>

Кластер рассчитан на быструю надёжную сеть; для связи дата-центров используйте независимые брокеры и Shovel/Federation.
→ [Основы](basics.ru.md#зачем-соединять-брокеры)

</details>

## 11. 🔴 После перезапуска источника shovel снова работает, но вы не заметили простоя. Что стоило сделать?

- A) Ничего
- B) Мониторить `shovel_status` и глубину очередей, тревога на `terminated`
- C) Включить prefetch 1
- D) Отключить shovel

<details><summary>Ответ</summary>

**B.**
→ [Подвох 4](expert.ru.md#4-состояние-terminated)

</details>

## 12. 🔴 Открытый вопрос

Два дата-центра должны обмениваться событиями заказов, а потеря связи на час не должна останавливать работу каждого. Что выберете?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Независимые брокеры в каждом ЦОД; Federation обменника или Shovel очередь→очередь.
- Очереди накапливают сообщения во время обрыва; после восстановления связь продолжает.
- Идемпотентные потребители из-за дубликатов; TLS и отдельный пользователь.
- Мониторинг связей и глубины очередей.
- Что не проверялось: поведение при реальном разрыве WAN, двунаправленность.

</details>

→ [Продвинутая теория](advanced.ru.md#что-выбрать-для-дата-центров)
