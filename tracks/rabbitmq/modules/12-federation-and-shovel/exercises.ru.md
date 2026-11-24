---
title: "Упражнения: Federation и Shovel"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream]
status: draft
updated: 2026-11-24
---

# Упражнения: Federation и Shovel

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужны два независимых одноразовых брокера `rmqa` и `rmqb` в одной сети Docker с включёнными плагинами `rabbitmq_shovel` и `rabbitmq_federation` (учебная среда, учётная запись `guest`). В конце удалите контейнеры и сеть: `docker rm -f rmqa rmqb; docker network rm rmqnet`.

## 1. 🟢 Поднимите два брокера

**Задача:** запустите два брокера в одной сети, включите плагины и убедитесь, что они **не** образуют кластер.

**Ожидаемый результат:** `rabbitmq-plugins enable …` отвечает `started … plugins`; `cluster_status` каждого показывает только один узел.

<details><summary>Подсказка</summary>

`docker network create rmqnet`, затем два `docker run --network rmqnet`, плагины — `rabbitmq-plugins enable rabbitmq_shovel rabbitmq_shovel_management rabbitmq_federation rabbitmq_federation_management`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#как-проверялся-код).

</details>

## 2. 🟢 Shovel из очереди в очередь

**Задача:** положите пять сообщений в `q.src` на `rmqa`, заведите `q.dst` на `rmqb` и создайте shovel на `rmqb`. Выведите глубины очередей и `shovel_status`.

**Ожидаемый результат:** `q.src` — 0, `q.dst` — 5, состояние `running`.

<details><summary>Подсказка</summary>

Параметр `shovel` с URI источника и приёмника; см. JSON в основах.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#shovel-из-очереди-в-очередь).

</details>

## 3. 🟡 Порядок и непрерывность

**Задача:** опубликуйте в `q.src` ещё три сообщения и прочитайте всё из `q.dst`. Проверьте порядок.

**Ожидаемый результат:** `['m0', …, 'm7']` в исходном порядке.

<details><summary>Подсказка</summary>

Читайте через `basic_get` до `None`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#shovel-из-очереди-в-очередь).

</details>

## 4. 🟡 Сбой источника

**Задача:** остановите `rmqa`, посмотрите `shovel_status`, запустите его, опубликуйте сообщение и убедитесь, что оно дошло в `q.dst`.

**Ожидаемый результат:** `terminated` с причиной `failed to connect to source`, затем сообщение в `q.dst`.

<details><summary>Подсказка</summary>

После запуска контейнера подождите, пока брокер примет соединения, и ещё несколько секунд до восстановления shovel.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#сбой-источника).

</details>

## 5. 🔴 Федерация обменника

**Задача:** на `rmqb` задайте upstream `up-a` на `rmqa` и политику для `^fed\.`, объявите `fed.ex` на обоих брокерах, привяжите на `rmqb` очередь `fed.q` и опубликуйте на `rmqa` в `fed.ex`. Проверьте `federation_status`.

**Ожидаемый результат:** в `fed.q` на `rmqb` одно сообщение; связь `running`.

<details><summary>Подсказка</summary>

Подождите несколько секунд после создания политики: связь устанавливается асинхронно.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#federation-общий-обменник).

</details>
