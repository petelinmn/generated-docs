---
title: "Квиз: наблюдаемость"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 16-observability
part: 3
tags: [logs, events, prometheus, promql]
status: draft
updated: 2026-11-23
---

# Квиз: наблюдаемость

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что делает `kubectl logs multi` для пода из двух контейнеров без `-c`?

- A) Показывает логи обоих контейнеров
- B) Показывает один контейнер и сообщает `Defaulted container`
- C) Возвращает ошибку
- D) Показывает события

<details><summary>Ответ</summary>

**B.** В опыте: `Defaulted container "app" out of: app, side`. Для обоих — `--all-containers`.
→ [Базовая теория](basics.ru.md#логи-kubectl-logs)

</details>

## 2. 🟢 Как увидеть логи всех контейнеров пода с указанием источника?

- A) `kubectl logs P --all-containers --prefix`
- B) `kubectl logs P -f`
- C) `kubectl describe pod P`
- D) `kubectl events`

<details><summary>Ответ</summary>

**A.** Префикс вида `[pod/multi/app]`.
→ [Базовая теория](basics.ru.md#логи-kubectl-logs)

</details>

## 3. 🟢 Где kubelet хранит логи контейнеров на ноде?

- A) В etcd
- B) В `/var/log/pods/<ns>_<под>_<uid>/<контейнер>/`
- C) В ConfigMap
- D) В памяти kubelet

<details><summary>Ответ</summary>

**B.** Ссылки на эти файлы лежат в `/var/log/containers`.
→ [Базовая теория](basics.ru.md#где-логи-на-ноде)

</details>

## 4. 🟢 Где причина `FailedScheduling` или `BackOff`?

- A) В `kubectl logs`
- B) В событиях объекта: `kubectl describe`/`kubectl events`
- C) В `kubectl top`
- D) В логах API-сервера

<details><summary>Ответ</summary>

**B.** Контейнера может не быть вообще, а событие объясняет причину.
→ [Базовая теория](basics.ru.md#события)

</details>

## 5. 🟡 Зачем на ноде нужен сборщик логов (DaemonSet), если есть `kubectl logs`?

- A) `kubectl logs` не работает без агента
- B) Чтобы сохранять историю: логи удаляются вместе с подом и ротируются
- C) Чтобы ускорить `kubectl logs`
- D) Чтобы заменить события

<details><summary>Ответ</summary>

**B.** Файлы лежат на ноде и удаляются вместе с подом; агент читает их и отправляет в хранилище.
→ [Продвинутая теория](advanced.ru.md#сбор-логов-демоном-на-ноде)

</details>

## 6. 🟡 DaemonSet агента логов `DESIRED 2` на кластере из трёх нод. Что вероятнее всего?

- A) Одна нода недоступна
- B) На control-plane стоит taint, а у DaemonSet нет toleration
- C) Лимит подов
- D) Ошибка конфигурации образа

<details><summary>Ответ</summary>

**B.** В опыте именно так: `fb 2 2 2`.
→ [Продвинутая теория](advanced.ru.md#сбор-логов-демоном-на-ноде)

</details>

## 7. 🟡 Что такое `container_cpu_usage_seconds_total` и как его читать?

- A) Gauge, значение читается как есть
- B) Counter, читают через `rate()`
- C) Событие
- D) Метка

<details><summary>Ответ</summary>

**B.** Счётчик: суммарное CPU-время; скорость роста — ядра в секунду.
→ [Продвинутая теория](advanced.ru.md#promql-примеры)

</details>

## 8. 🟡 Интервал опроса 1 минута, а `rate(...[1m])` пуст. Что исправить?

- A) Увеличить окно, например до `[5m]`
- B) Перезапустить Prometheus
- C) Удалить метрику
- D) Уменьшить интервал до 1 часа

<details><summary>Ответ</summary>

**A.** В окне должно быть минимум два сэмпла; в опыте `[5m]` вернул данные.
→ [Продвинутая теория](advanced.ru.md#окно-rate-и-интервал-опроса)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) Prometheus сам опрашивает цели (pull)
- [ ] B) kube-state-metrics измеряет CPU контейнеров
- [ ] C) cAdvisor встроен в kubelet
- [ ] D) Prometheus 3 отклоняет цель без `Content-Type`

<details><summary>Ответ</summary>

**A, C, D.** B неверно: потребление отдаёт cAdvisor, а kube-state-metrics — состояние объектов.
→ [Продвинутая теория](advanced.ru.md#prometheus-устройство-и-установка)

</details>

## 10. 🔴 Найдите ошибку

> «Контейнер упал, значит, `kubectl logs --previous` всегда покажет причину».

<details><summary>Ответ</summary>

Предыдущие экземпляры kubelet удаляет; у пода в `CrashLoopBackOff` команда в опыте вернула `unable to retrieve container logs`. Нужны события, обычный `kubectl logs` и внешнее хранилище логов.
→ [Подвох 1](expert.ru.md#1---previous-не-всегда-возвращает-логи)

</details>

## 11. 🔴 Цель в Prometheus `down`: `non-compliant scrape target sending blank Content-Type`. Что делать?

- A) Отключить Prometheus
- B) Настроить приложение отдавать `Content-Type` (например, `text/plain`)
- C) Увеличить `scrape_interval`
- D) Добавить toleration

<details><summary>Ответ</summary>

**B.** В опыте после переименования файла на `metrics.txt` сервер стал отправлять `text/plain`, и метрика появилась.
→ [Подвох 3](expert.ru.md#3-prometheus-3-и-content-type)

</details>

## 12. 🔴 Открытый вопрос

Как построить наблюдаемость для приложения в Kubernetes: что собирать и чем?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Логи: stdout/stderr → DaemonSet-агент → хранилище (поиск, история); `kubectl logs` для оперативной проверки.
- События: `describe`/`events` для первой диагностики; не архив.
- Метрики: Prometheus (pull), kube-state-metrics, cAdvisor, `/metrics` приложения; Grafana для дашбордов.
- Алерты по симптомам; `rate()` с достаточным окном; тома для хранения.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
