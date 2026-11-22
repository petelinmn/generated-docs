---
title: "Квиз: Pod"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 04-pods
part: 1
tags: [pod, restart-policy, init-container, sidecar, termination, qos]
status: draft
updated: 2026-11-22
---

# Квиз: Pod

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что делает политика `restartPolicy: Always` с контейнером, который завершился с кодом 0?

- A) Ничего, под становится `Succeeded`
- B) Перезапускает его
- C) Удаляет под
- D) Помечает под как `Failed`

<details><summary>Ответ</summary>

**B.** `Always` перезапускает контейнер при любом завершении, даже успешном. Для разовых задач нужны `Never` или `OnFailure`: под `once` с `Never` стал `Completed` (фаза `Succeeded`).
→ [Базовая теория](basics.ru.md#restartpolicy-что-происходит-когда-контейнер-завершился)

</details>

## 2. 🟢 Что из этого — фаза пода (`status.phase`)?

- A) `CrashLoopBackOff`
- B) `Completed`
- C) `Running`
- D) `ContainerCreating`

<details><summary>Ответ</summary>

**C.** Фазы: `Pending`, `Running`, `Succeeded`, `Failed`, `Unknown`. `CrashLoopBackOff`, `Completed` и `ContainerCreating` — значения столбца `STATUS`.
→ [Базовая теория](basics.ru.md#фазы-и-статусы)

</details>

## 3. 🟢 В поде нужно запустить две программы, которые общаются через `localhost`. Что верно?

- A) Их нужно запускать в двух разных подах
- B) Их можно запустить как два контейнера одного пода: у них общий IP и `localhost`
- C) Это невозможно, `localhost` разный у каждого контейнера
- D) Нужен Service

<details><summary>Ответ</summary>

**B.** Контейнеры пода делят сетевое пространство: в опыте `hostname -i` в двух контейнерах дал один и тот же `10.244.0.43`, а `wget http://localhost:80` из контейнера `probe` достучался до nginx.
→ [Продвинутая теория](advanced.ru.md#несколько-контейнеров-в-поде)

</details>

## 4. 🟢 Что означает `Init:1/2`?

- A) Один из двух основных контейнеров запущен
- B) Выполнен один из двух init-контейнеров
- C) Один init-контейнер упал
- D) Под удаляется

<details><summary>Ответ</summary>

**B.** Init-контейнеры выполняются по очереди; `Init:N/M` — N завершились из M. После последнего под становится `Running`.
→ [Продвинутая теория](advanced.ru.md#init-контейнеры)

</details>

## 5. 🟡 Сколько займёт `kubectl delete pod` для пода, где PID 1 — `sleep 3600` без обработчика сигналов?

- A) Около секунды
- B) Около 30 секунд (`terminationGracePeriodSeconds` по умолчанию)
- C) Около 5 минут
- D) Удаление зависнет навсегда

<details><summary>Ответ</summary>

**B.** `SIGTERM` игнорируется, через 30 секунд приходит `SIGKILL` (в опыте 32 с). Под с обработчиком `trap` удалился за 1 с, а с `terminationGracePeriodSeconds: 5` — за 6 с.
→ [Продвинутая теория](advanced.ru.md#остановка-пода)

</details>

## 6. 🟡 Что выведет `kubectl logs multi` для пода с контейнерами `web`, `writer`, `probe`?

- A) Логи всех трёх контейнеров
- B) Ошибку: нужно указать контейнер
- C) Логи `web` и строку `Defaulted container "web" out of: web, writer, probe`
- D) Пустой вывод

<details><summary>Ответ</summary>

**C.** Без `-c` kubectl выбирает контейнер по умолчанию (первый или заданный аннотацией) и сообщает об этом. Для всех сразу — `--all-containers`.
→ [Продвинутая теория](advanced.ru.md#логи-и-выбор-контейнера)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) Нативный sidecar — init-контейнер с `restartPolicy: Always`
- [ ] B) Том `emptyDir` переживает пересоздание пода
- [ ] C) Хук `preStop` выполняется до `SIGTERM`
- [ ] D) Контейнеры пода по умолчанию видят процессы друг друга

<details><summary>Ответ</summary>

**A, C.** B неверно: `emptyDir` живёт вместе с подом. D неверно: общий список процессов включается `shareProcessNamespace: true`, по умолчанию процессы раздельны (в опыте `ps` в `probe` показал только свои).
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Под в `CrashLoopBackOff`. Что делать в первую очередь?

- A) Удалить и пересоздать под
- B) Посмотреть `kubectl logs` и код выхода (`lastState.terminated.exitCode`), затем `describe`
- C) Увеличить `terminationGracePeriodSeconds`
- D) Добавить `restartPolicy: Never`

<details><summary>Ответ</summary>

**B.** `CrashLoopBackOff` — лишь ожидание между перезапусками; причина в самом приложении или настройке. Пересоздание не поможет (A), `Never` лишь скроет перезапуски (D). Лог предыдущего запуска (`--previous`) может быть уже недоступен, как в нашем опыте.
→ [Базовая теория](basics.ru.md#crashloopbackoff)

</details>

## 9. 🔴 Найди ошибку

> «Под показывает `Running` и `1/1`, значит приложение работает и принимает запросы».

<details><summary>Ответ</summary>

`Running` — только фаза пода: контейнер запущен. Готовность к трафику определяет условие `Ready`, которое при отсутствии readiness probe равно «контейнер запущен», а не «приложение здоровое». Приложение может быть сломано или не прогреться. Нужны readiness и liveness probes (модуль 11).
→ [Экспертная теория](expert.ru.md#подвох-running-не-значит-работает)

</details>

## 10. 🔴 Открытый вопрос

Контейнер завершился с кодом 137 и причиной `OOMKilled`. Объясни, что это значит, как это воспроизвести и что менять.

<details><summary>Ключевые пункты ответа</summary>

- 137 = 128 + 9: процесс убит `SIGKILL`; причина `OOMKilled` — контейнер превысил лимит памяти (`limits.memory`), и ядро (OOM killer) его убило.
- Воспроизведение: под с `limits.memory: 20Mi` и командой `tail /dev/zero`; в опыте `reason=OOMKilled exit=137`, QoS `Burstable`.
- Что менять: поднять лимит или уменьшить потребление памяти приложением; смотреть реальное потребление (`kubectl top`, метрики, модули 11 и 16).
- Не путать с остановкой по тайм-ауту: там тоже `SIGKILL`, но причина другая (истёк `terminationGracePeriodSeconds`).

</details>

→ [Экспертная теория](expert.ru.md#коды-выхода)
