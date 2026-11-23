---
title: "Квиз: автомасштабирование"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, requests]
status: draft
updated: 2026-11-23
---

# Квиз: автомасштабирование

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что меняет HorizontalPodAutoscaler?

- A) `requests` контейнеров
- B) Число реплик целевого объекта
- C) Число нод
- D) Образ контейнера

<details><summary>Ответ</summary>

**B.** `requests` подбирает VPA, ноды добавляет Cluster Autoscaler.
→ [Обзор](overview.ru.md)

</details>

## 2. 🟢 Что нужно для HPA по CPU?

- A) Только манифест HPA
- B) metrics-server и `requests` по CPU у контейнеров
- C) Prometheus
- D) Ingress

<details><summary>Ответ</summary>

**B.** Без `requests` в `TARGETS` будет `<unknown>` (`missing request for cpu`).
→ [Базовая теория](basics.ru.md#ошибки-новичка)

</details>

## 3. 🟢 1 под, утилизация 250%, цель 50%, `maxReplicas: 5`. Сколько реплик захочет HPA?

- A) 2
- B) 3
- C) 5
- D) 10

<details><summary>Ответ</summary>

**C.** `ceil(1 × 250 / 50) = 5`; ограничение 5 не мешает. В опыте реплики выросли с 1 до 5.
→ [Базовая теория](basics.ru.md#нагрузка-и-масштабирование)

</details>

## 4. 🟢 От чего считается процент утилизации CPU в HPA?

- A) От `limits`
- B) От `requests`
- C) От числа ядер ноды
- D) От среднего по кластеру

<details><summary>Ответ</summary>

**B.** В опыте 143m при `requests: 200m` дали 71%.
→ [Базовая теория](basics.ru.md#первый-hpa)

</details>

## 5. 🟡 5 подов, утилизация 71%, цель 50%, `maxReplicas: 5`. Что покажет HPA?

- A) Масштабирует до 8
- B) Остаётся 5, условие `ScalingLimited=True (TooManyReplicas)`
- C) Ошибка
- D) Масштабирует вниз

<details><summary>Ответ</summary>

**B.** Формула даёт `ceil(5 × 71 / 50) = 8`, но потолок 5.
→ [Базовая теория](basics.ru.md#нагрузка-и-масштабирование)

</details>

## 6. 🟡 Вы вручную поставили `scale deploy d2 --replicas=3` при HPA `min=1 max=3` и нулевой нагрузке. Что произойдёт?

- A) Сразу вернётся 1 реплика
- B) Реплики останутся 3 около пяти минут (окно стабилизации), затем станет 1
- C) Реплики останутся 3 навсегда
- D) HPA удалится

<details><summary>Ответ</summary>

**B.** В опыте 3 реплики держались до ~260 с, на ~315 с было 1.
→ [Продвинутая теория](advanced.ru.md#окно-стабилизации-вниз)

</details>

## 7. 🟡 Как HPA поведёт себя с метриками cpu и memory одновременно?

- A) Использует только первую
- B) Считает реплики по каждой и берёт наибольшее
- C) Берёт среднее
- D) Отвергает такой HPA

<details><summary>Ответ</summary>

**B.** В опыте таблица показала обе цели: `cpu: 0%/50%, memory: 12276Ki/100Mi`.
→ [Продвинутая теория](advanced.ru.md#несколько-метрик)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) `minReplicas: 0` отвергается для HPA только с Resource-метриками
- [ ] B) Два HPA на один Deployment дают `AmbiguousSelector`
- [ ] C) Метрика `Pods` работает без адаптера
- [ ] D) `kubectl autoscale` создаёт HPA

<details><summary>Ответ</summary>

**A, B, D.** C неверно: нужен адаптер custom metrics API (`no custom metrics API (custom.metrics.k8s.io) registered`).
→ [Продвинутая теория](advanced.ru.md#ошибки-и-ограничения)

</details>

## 9. 🔴 Найдите ошибку

> «Мы убрали `requests`, чтобы поды не ограничивались, и HPA продолжит работать по CPU».

<details><summary>Ответ</summary>

Без `requests` процент утилизации не определён: `FailedGetResourceMetric … missing request for cpu`, `TARGETS` — `<unknown>`, масштабирования нет. `requests` — это знаменатель метрики, а не ограничение.
→ [Подвох 1](expert.ru.md#1-unknown-в-targets)

</details>

## 10. 🔴 Что произойдёт при повторном `kubectl apply` манифеста Deployment с `replicas: 1`, если HPA поднял 5 реплик?

- A) Ничего
- B) Число реплик вернётся к 1 до следующего цикла HPA, затем HPA снова поднимет
- C) HPA удалится
- D) Применение отвергнется

<details><summary>Ответ</summary>

**B.** Поле `replicas` в манифесте перебивает решение HPA, поэтому из манифеста его убирают (вывод из документации; в опыте не воспроизводился).
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 11. 🔴 Открытый вопрос

Объясните, как работает HPA, и опишите, как вы будете настраивать его для нагрузочного сервиса.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Контроллер раз за циклом читает метрики и считает `ceil(текущие × метрика / цель)`, затем ограничивает `minReplicas`/`maxReplicas` и политиками `behavior`.
- Нужны metrics-server и `requests`; процент считается от `requests`.
- Цель 50–70%, `maxReplicas` под бюджет, окно стабилизации вниз против «пилы», `scaleUp` под всплески.
- Для бизнес-метрик — адаптер (custom/external); VPA и Cluster Autoscaler — отдельно.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
