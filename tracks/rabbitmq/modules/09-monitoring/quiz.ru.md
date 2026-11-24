---
title: "Квиз: мониторинг и эксплуатация"
lang: ru
track: rabbitmq
kind: module
doc: quiz
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, prometheus, alarms, management-api]
status: draft
updated: 2026-11-24
---

# Квиз: мониторинг и эксплуатация

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 В очереди `messages` = 10, `messages_ready` = 7. Что означают остальные 3?

- A) Просроченные
- B) Неподтверждённые, их держит потребитель
- C) В dead-letter-очереди
- D) Удалённые

<details><summary>Ответ</summary>

**B.** `messages = messages_ready + messages_unacknowledged`.
→ [Основы](basics.ru.md#http-api-управления)

</details>

## 2. 🟢 Как закодировать vhost `/` в URL API?

- A) `/`
- B) `%2F`
- C) `root`
- D) Пустая строка

<details><summary>Ответ</summary>

**B.**
→ [Основы](basics.ru.md#http-api-управления)

</details>

## 3. 🟢 Зачем задавать `connection_name`?

- A) Чтобы ускорить соединение
- B) Чтобы находить приложение среди соединений в management
- C) Чтобы включить TLS
- D) Это обязательное поле

<details><summary>Ответ</summary>

**B.**
→ [Основы](basics.ru.md#соединения-каналы-потребители)

</details>

## 4. 🟢 Какая команда показывает глубину очередей без HTTP?

- A) `rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers`
- B) `rabbitmq-plugins list`
- C) `rabbitmqctl cluster_status`
- D) `rabbitmq-diagnostics check_running`

<details><summary>Ответ</summary>

**A.**
→ [Основы](basics.ru.md#командная-строка)

</details>

## 5. 🟡 Какой порт у метрик Prometheus и чем различаются `/metrics` и `/metrics/per-object`?

- A) 15672; одинаковы
- B) 15692; первый — сводные метрики узла, второй — по каждой очереди
- C) 5672; первый для очередей, второй для соединений
- D) 15692; первый — для узла, второй — для кластера

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md#prometheus)

</details>

## 6. 🟡 Что вернёт `GET /api/health/checks/alarms` при тревоге по памяти?

- A) `200 ok`
- B) `503` с перечнем тревог
- C) `404`
- D) `401`

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md#оповещения-памяти-и-диска)

</details>

## 7. 🟡 Что получит публикующий клиент при тревоге по памяти?

- A) Отказ в соединении
- B) `Connection.Blocked` с причиной `low on memory`
- C) Автоматическое удаление сообщений
- D) Ничего

<details><summary>Ответ</summary>

**B.** Соединение в API получает состояние `blocking`.
→ [Продвинутая теория](advanced.ru.md#оповещения-памяти-и-диска)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) `rabbitmq-diagnostics check_local_alarms` надёжно показывает активную тревогу в 4.4.1
- [ ] B) Статистика management обновляется с задержкой
- [ ] C) `/metrics/per-object` содержит метку `queue`
- [ ] D) Prometheus получает метрики, опрашивая порт 15692

<details><summary>Ответ</summary>

**B, C, D.** A неверно: команда устарела и ничего не делает.
→ [Подвох 1](expert.ru.md#1-check_local_alarms--ничего-не-делает)

</details>

## 9. 🟡 Что показывает `prefetch_count` и `messages_unacknowledged` у канала?

- A) Лимит и число неподтверждённых сообщений потребителя на канале
- B) Размер очереди
- C) Число соединений
- D) Скорость публикации

<details><summary>Ответ</summary>

**A.** В опыте: 3 и 3.
→ [Основы](basics.ru.md#соединения-каналы-потребители)

</details>

## 10. 🔴 Найдите ошибку

> «Мы убедились, что тревог нет: `rabbitmq-diagnostics check_local_alarms` вернул "no local alarms"».

<details><summary>Ответ</summary>

В 4.4.1 команда устарела и ничего не делает (проверено: при активной тревоге по памяти она тоже говорила «no local alarms»). Проверяйте `/api/health/checks/alarms`.
→ [Подвох 1](expert.ru.md#1-check_local_alarms--ничего-не-делает)

</details>

## 11. 🔴 Очередь растёт, `consumers = 1`, `messages_unacknowledged` равен prefetch и не меняется. Что вероятно?

- A) Потребитель завис и не подтверждает
- B) Очередь переполнилась
- C) Тревога по диску
- D) Утечка соединений

<details><summary>Ответ</summary>

**A.** Потребитель держит prefetch сообщений и не подтверждает; остальные ждут ([модуль 03](../03-queues-and-delivery/basics.ru.md#prefetch-и-справедливость)).
→ [Продвинутая теория](advanced.ru.md#что-тревожить)

</details>

## 12. 🔴 Открытый вопрос

Спроектируйте мониторинг очереди платёжных задач: какие метрики, тревоги и реакции клиента вы предусмотрите?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Метрики: `messages_ready`, `messages_unacknowledged`, `consumers`, DLQ, соединения, память/диск (Prometheus).
- Тревоги по тренду роста `ready`, `consumers = 0`, непустой DLQ, тревогам памяти/диска.
- HTTP health check в пробах; в клиенте реакция на `Connection.Blocked`.
- Именованные соединения, дашборд Grafana.
- Пороги подбирать под нагрузку (в материале не проверялись).

</details>

→ [Продвинутая теория](advanced.ru.md#что-тревожить)
