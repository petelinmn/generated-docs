---
title: "Квиз: Probes и ресурсы"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 11-probes-and-resources
part: 2
tags: [probes, liveness-probe, readiness-probe, startup-probe, resources, qos]
status: draft
updated: 2026-11-23
---

# Квиз: Probes и ресурсы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что делает kubelet при провале liveness-пробы?

- A) Убирает под из endpoints
- B) Перезапускает контейнер
- C) Удаляет под
- D) Ничего, только пишет событие

<details><summary>Ответ</summary>

**B.** В опыте: `Killing … failed liveness probe, will be restarted`, `RESTARTS` вырос. Убрать под из endpoints — реакция на провал readiness.
→ [Базовая теория](basics.ru.md#liveness-перезапуск-зависшего)

</details>

## 2. 🟢 Под `Running`, но `0/1`, и трафик на него не идёт. Что произошло?

- A) Провалена readiness-проба
- B) Провалена liveness-проба
- C) Закончилась память
- D) Под `BestEffort`

<details><summary>Ответ</summary>

**A.** Условие `Ready=False`: под исключён из endpoints, но не перезапущен (в опыте после удаления файла `/tmp/ready`).
→ [Базовая теория](basics.ru.md#readiness-готов-ли-принимать-трафик)

</details>

## 3. 🟢 Какие значения по умолчанию у `periodSeconds` и `failureThreshold`?

- A) 1 и 1
- B) 10 и 3
- C) 30 и 5
- D) 5 и 10

<details><summary>Ответ</summary>

**B.** Ещё `timeoutSeconds: 1`, `successThreshold: 1`, `initialDelaySeconds: 0`. Видно в `kubectl describe pod`.
→ [Базовая теория](basics.ru.md#параметры-и-значения-по-умолчанию)

</details>

## 4. 🟢 Под просит `requests.cpu: "100"` на ноде с 12 ядрами. Что будет?

- A) Под запустится и будет ограничен
- B) Под останется `Pending`, `FailedScheduling: Insufficient cpu`
- C) Под упадёт с `OOMKilled`
- D) Планировщик уменьшит заявку

<details><summary>Ответ</summary>

**B.** Планировщик сравнивает requests с allocatable ноды, а не с фактической нагрузкой.
→ [Базовая теория](basics.ru.md#планирование-идёт-по-requests)

</details>

## 5. 🟡 Приложение стартует 25 секунд. Liveness: `periodSeconds: 3`, `failureThreshold: 2`. Что будет без startup-пробы?

- A) Приложение успеет стартовать
- B) Liveness убьёт контейнер до окончания старта, цикл повторится
- C) Kubernetes сам увеличит задержку
- D) Под будет `Pending`

<details><summary>Ответ</summary>

**B.** В опыте `slow-nostartup` перезапускался (`r=1`); со startup-пробой (`15 × 3 с`) запуск прошёл без перезапусков.
→ [Продвинутая теория](advanced.ru.md#медленный-старт-и-startup-проба)

</details>

## 6. 🟡 Под с CPU-лимитом `100m` жжёт CPU 20 секунд на почти свободной ноде. Сколько CPU он получит?

- A) Все 20 секунд: нода свободна
- B) Около 2 секунд (10% ядра), с притормаживанием в каждом периоде
- C) 100 секунд
- D) Под будет убит

<details><summary>Ответ</summary>

**B.** `usage_usec 2023496`, `nr_throttled 202` из `nr_periods 202`: лимит работает независимо от загрузки ноды. Под без лимита получил все 20 секунд.
→ [Продвинутая теория](advanced.ru.md#cpu-throttling)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) Без readiness-пробы контейнер `Ready` сразу после запуска
- [ ] B) `requests.cpu` ограничивает потребление CPU контейнером
- [ ] C) `limits.cpu` меньше `requests.cpu` отклоняется при создании
- [ ] D) `tcpSocket`-проба проходит, если порт принимает соединение

<details><summary>Ответ</summary>

**A, C, D.** B неверно: ограничивает `limits`, а `requests` задаёт вес и планирование.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Какой QoS-класс у пода только с `requests` (без `limits`)?

- A) `Guaranteed`
- B) `Burstable`
- C) `BestEffort`
- D) Класса нет

<details><summary>Ответ</summary>

**B.** `Guaranteed` требует requests = limits у всех контейнеров по CPU и памяти; `BestEffort` — отсутствие ресурсов. В опыте под `onlyreq` получил `Burstable`.
→ [Базовая теория](basics.ru.md#qos-классы)

</details>

## 9. 🔴 Найди ошибку

> «Liveness-проба нашего сервиса проверяет подключение к базе данных: так мы быстро увидим проблемы и перезапустим сервис».

<details><summary>Ответ</summary>

При сбое базы все поды одновременно провалят liveness и будут перезапущены, и сервис станет недоступен надолго, хотя перезапуск не поможет. Зависимости проверяет readiness (пода исключат из трафика, но не убьют); liveness должна отвечать только на вопрос «безнадёжно ли зависло приложение».
→ [Продвинутая теория](advanced.ru.md#как-проектировать-пробы)

</details>

## 10. 🔴 Открытый вопрос

Объясни, как `requests` и `limits` превращаются в настройки ядра, что происходит при превышении CPU- и memory-лимитов и как это связано с QoS.

<details><summary>Ключевые пункты ответа</summary>

- `limits.cpu` → `cpu.max` (квота: `10000 100000` для `100m`); `requests.cpu` → `cpu.weight` (вес при конкуренции: 17 для `100m`, 1 для `BestEffort`); `limits.memory` → `memory.max`.
- CPU сжимаемый: превышение лимита — throttling (в опыте `nr_throttled 202`); память несжимаемая: превышение — OOMKilled, код 137.
- QoS-класс вычисляется из requests и limits и отражается в `oom_score_adj` (`Guaranteed` −997, `Burstable` 998–999, `BestEffort` 1000): при нехватке памяти на ноде первыми убиваются процессы с большим значением.
- Планирование идёт по requests (`Insufficient cpu`), а не по фактическому потреблению.

</details>

→ [Экспертная теория](expert.ru.md#qos-и-oom_score_adj)
