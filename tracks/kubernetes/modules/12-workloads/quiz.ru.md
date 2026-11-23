---
title: "Квиз: другие workload-ы"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, workload]
status: draft
updated: 2026-11-23
---

# Квиз: другие workload-ы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой контроллер подходит для базы данных с собственным диском у каждой реплики?

- A) Deployment
- B) DaemonSet
- C) StatefulSet
- D) CronJob

<details><summary>Ответ</summary>

**C.** StatefulSet даёт стабильные имена и PVC на реплику (`data-web-0`, `data-web-1`, …). Deployment делает поды взаимозаменяемыми, DaemonSet — по поду на ноду.
→ [Базовая теория](basics.ru.md#statefulset)

</details>

## 2. 🟢 Как называются поды StatefulSet `web` из трёх реплик?

- A) `web-<случайный суффикс>`
- B) `web-0`, `web-1`, `web-2`
- C) `web-a`, `web-b`, `web-c`
- D) `web-1`, `web-2`, `web-3`

<details><summary>Ответ</summary>

**B.** Номера идут с нуля и стабильны. Случайный суффикс — у подов Deployment.
→ [Базовая теория](basics.ru.md#что-даёт-statefulset)

</details>

## 3. 🟢 DaemonSet на кластере из 3 нод (1 control plane с taint + 2 worker), без toleration. Сколько подов?

- A) 3
- B) 2
- C) 1
- D) Столько, сколько `replicas`

<details><summary>Ответ</summary>

**B.** На control plane стоит taint, и без toleration под туда не попадает. Поля `replicas` у DaemonSet нет.
→ [Базовая теория](basics.ru.md#daemonset)

</details>

## 4. 🟢 Какие значения `restartPolicy` допустимы в шаблоне Job?

- A) `Always` и `Never`
- B) `OnFailure` и `Never`
- C) Только `Always`
- D) Любые

<details><summary>Ответ</summary>

**B.** Для `Always` API отвечает `Required value: valid values: "OnFailure", "Never"`.
→ [Базовая теория](basics.ru.md#job)

</details>

## 5. 🟡 Вы удалили под `web-1` StatefulSet. Что произойдёт?

- A) Под не вернётся
- B) Появится `web-1` с тем же PVC, но новым IP и UID
- C) Появится `web-3` с новым PVC
- D) Данные в томе будут потеряны

<details><summary>Ответ</summary>

**B.** В опыте файл на томе сохранился, IP сменился (`10.244.0.31` → `10.244.0.35`).
→ [Продвинутая теория](advanced.ru.md#statefulset-идентичность-переживает-пересоздание)

</details>

## 6. 🟡 `kubectl scale sts/web --replicas=1` при трёх репликах. Что с PVC?

- A) Удалены `data-web-1` и `data-web-2`
- B) Остаются все три PVC
- C) Удалены все три
- D) Остаётся только `data-web-0`

<details><summary>Ответ</summary>

**B.** По умолчанию `whenScaled: Retain`. С `Delete` ушёл бы PVC удалённых реплик.
→ [Продвинутая теория](advanced.ru.md#политика-pvc)

</details>

## 7. 🟡 Выберите все верные утверждения о `partition`

- [ ] A) При `partition: 2` обновляется под `web-2` (в наборе из трёх)
- [ ] B) При `partition: 2` обновляются `web-0` и `web-1`
- [ ] C) Снижение `partition` до 0 обновляет остальные поды
- [ ] D) `rollout status` при `partition > 0` честно сообщает, что обновлён один под

<details><summary>Ответ</summary>

**A, C.** Обновляются поды с номером не меньше `partition`. D неверно: написано `partitioned roll out complete: 3 new pods have been updated`.
→ [Продвинутая теория](advanced.ru.md#обновление-с-partition)

</details>

## 8. 🟡 Job с `backoffLimit: 2` и командой `exit 1`. Сколько подов будет создано и каков итог?

- A) 1 под, Job `Failed`
- B) 3 пода, Job `Failed` с причиной `BackoffLimitExceeded`
- C) 2 пода, Job `Complete`
- D) Бесконечно много

<details><summary>Ответ</summary>

**B.** Первая попытка и два повтора: в опыте три пода `Error`.
→ [Продвинутая теория](advanced.ru.md#backofflimit)

</details>

## 9. 🟡 CronJob с `concurrencyPolicy: Forbid`, расписание «каждую минуту», задача длится 100 секунд. Что в событиях?

- A) Каждую минуту создаётся новый Job
- B) `JobAlreadyActive … concurrency policy is Forbid`: запуск пропускается
- C) Старый Job убивается
- D) CronJob удаляется

<details><summary>Ответ</summary>

**B.** Убивает старый запуск политика `Replace`, а не `Forbid`.
→ [Продвинутая теория](advanced.ru.md#concurrencypolicy)

</details>

## 10. 🔴 Найдите ошибку

> «Мы уменьшили StatefulSet с 3 до 1, диск освободился, и в стоимости кластера это видно сразу».

<details><summary>Ответ</summary>

По умолчанию PVC остаются (`whenScaled: Retain`): диски `data-web-1` и `data-web-2` продолжают существовать и стоить денег. Освободить их можно удалением PVC или `persistentVolumeClaimRetentionPolicy.whenScaled: Delete` — но тогда данные пропадут.
→ [Подвох 2](expert.ru.md#2-scale-вниз-не-освобождает-pvc)

</details>

## 11. 🔴 «Что выведет код»: что покажет `rollout status`?

```text
$ kubectl -n learn-12 patch sts web -p '{"spec":{"updateStrategy":{"type":"RollingUpdate","rollingUpdate":{"partition":2}}}}'
$ kubectl -n learn-12 set image sts/web nginx=nginx:1.28
$ kubectl -n learn-12 rollout status sts/web
```

- A) Ждёт, пока обновятся все три пода
- B) Сообщает `partitioned roll out complete`, хотя обновлён один под
- C) Ошибка: `partition` не поддерживается
- D) Обновляет `web-0` первым

<details><summary>Ответ</summary>

**B.** Обновлён только `web-2`; чтобы увидеть истинное состояние, сравните `currentRevision` и `updateRevision`.
→ [Подвох 1](expert.ru.md#1-rollout-status-при-partition-говорит-complete)

</details>

## 12. 🔴 Открытый вопрос

Объясните, какие гарантии даёт StatefulSet по сравнению с Deployment и во что они обходятся.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Стабильное имя (`web-N`), DNS через headless Service, свой PVC на реплику, который переживает пересоздание пода.
- Упорядоченность: создание по возрастанию номера, удаление по убыванию, обновление от старшего; `Parallel` отключает ожидание.
- Цена: `OrderedReady` блокируется на первой неготовой реплике, обновления медленнее, PVC не освобождаются сами (`Retain`), репликацию и бэкапы приложения Kubernetes не делает.
- Альтернативы: оператор для конкретной БД, управляемая БД вне кластера.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
