---
title: "Квиз: диагностика"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, events, endpoints, kubectl-debug]
status: draft
updated: 2026-11-23
---

# Квиз: диагностика

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Под в статусе `ImagePullBackOff`. Где искать причину?

- A) В логах контейнера
- B) В `state.message` контейнера и событиях `describe pod`
- C) В `kubectl top`
- D) В логах CoreDNS

<details><summary>Ответ</summary>

**B.** Контейнера нет, логов тоже; в `message` в опыте было `… nginx:no-such-tag: not found`.
→ [Базовая теория](basics.ru.md#imagepullbackoff-и-errimagepull)

</details>

## 2. 🟢 Под в `CreateContainerConfigError`: `configmap "missing-cm" not found`. Что делать?

- A) Пересобрать образ
- B) Создать ConfigMap `missing-cm` (или поправить ссылку)
- C) Увеличить лимит памяти
- D) Добавить toleration

<details><summary>Ответ</summary>

**B.** Образ скачан, но конфигурацию контейнера собрать не удалось.
→ [Базовая теория](basics.ru.md#createcontainerconfigerror)

</details>

## 3. 🟢 Контейнер завершился с `exitCode: 137` и `reason: OOMKilled`. Причина?

- A) Неверная команда
- B) Превышен лимит памяти
- C) Нет образа
- D) Ошибка DNS

<details><summary>Ответ</summary>

**B.** `137 = 128 + 9` (`SIGKILL`), причина подтверждена `reason`.
→ [Базовая теория](basics.ru.md#oomkilled)

</details>

## 4. 🟢 Под в `Pending` с `pod has unbound immediate PersistentVolumeClaims`. Где продолжить поиск?

- A) В логах пода
- B) В событиях PVC (`describe pvc`)
- C) В `kubectl top`
- D) В образе

<details><summary>Ответ</summary>

**B.** В опыте: `storageclass … "no-such-class" not found`.
→ [Базовая теория](basics.ru.md#pending)

</details>

## 5. 🟡 `kubectl get endpoints web-typo` показывает `<none>`. Вероятная причина?

- A) У подов нет `requests`
- B) Селектор Service не совпадает с метками подов
- C) Закончился лимит подов
- D) Отключён DNS

<details><summary>Ответ</summary>

**B.** В опыте селектор `app: wbe` вместо `app: web`.
→ [Продвинутая теория](advanced.ru.md#service-без-endpoints)

</details>

## 6. 🟡 У Service есть endpoints `…:8080`, но запросы дают `Connection refused`. Причина?

- A) Неверный DNS
- B) `targetPort` не совпадает с портом, который слушает контейнер
- C) Нода `NotReady`
- D) Нет квоты

<details><summary>Ответ</summary>

**B.** Адреса подов есть, но на `8080` никто не слушает.
→ [Подвох 4](expert.ru.md#4-верный-targetport--это-порт-контейнера)

</details>

## 7. 🟡 После изменения readiness-пробы на несуществующий путь раскатка не завершается. Что происходит с трафиком?

- A) Он прекращается
- B) Продолжают обслуживать старые поды; новый под `0/1` и не входит в endpoints
- C) Новый под получает весь трафик
- D) Deployment откатывается автоматически

<details><summary>Ответ</summary>

**B.** В опыте старые поды остались `1/1`, `rollout status` завершился по таймауту, в событиях `statuscode: 404`.
→ [Продвинутая теория](advanced.ru.md#неготовые-поды-не-попадают-в-endpoints)

</details>

## 8. 🟡 Что делает `kubectl debug pod/P --image=busybox --target=nginx`?

- A) Перезапускает под
- B) Добавляет временный контейнер, разделяющий пространство процессов с `nginx`
- C) Копирует под на другую ноду
- D) Меняет образ контейнера `nginx`

<details><summary>Ответ</summary>

**B.** В опыте `ps` показал процесс `nginx: master process`, а файловая система была доступна через `/proc/1/root`.
→ [Продвинутая теория](advanced.ru.md#kubectl-debug-заглянуть-внутрь)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) Объект с финализатором после `delete` получает `deletionTimestamp` и остаётся
- [ ] B) `ReplicaFailure` может возникнуть из-за превышения квоты
- [ ] C) Нода `NotReady` сразу выселяет все поды
- [ ] D) `kubectl debug node/…` оставляет отладочный под в namespace

<details><summary>Ответ</summary>

**A, B, D.** C неверно: выселение наступает по `tolerationSeconds` (по умолчанию 300 с), в опыте поды оставались `Running`.
→ [Продвинутая теория](advanced.ru.md#нода-notready)

</details>

## 10. 🔴 Найдите ошибку

> «Код выхода 137 всегда означает OOM, увеличиваем память».

<details><summary>Ответ</summary>

`137` — это `SIGKILL`; причину подтверждает `lastState.reason`. В опыте сбойная команда дала `255` и `Error`, а не `OOMKilled`.
→ [Подвох 1](expert.ru.md#1-137--не-всегда-oom)

</details>

## 11. 🔴 Deployment `0/5`, в самом Deployment ошибок нет, событий пода нет. Где причина?

- A) В kubelet
- B) В `ReplicaFailure` и событиях ReplicaSet (например, квота `exceeded quota`)
- C) В CoreDNS
- D) В Ingress

<details><summary>Ответ</summary>

**B.** Поды не были созданы вовсе. В опыте: `FailedCreate … exceeded quota: q, requested: pods=1, used: pods=4, limited: pods=4`.
→ [Продвинутая теория](advanced.ru.md#квота-replicafailure)

</details>

## 12. 🔴 Открытый вопрос

Опишите порядок диагностики, когда «приложение недоступно».

<details><summary>Ключевые мысли хорошего ответа</summary>

- Статус подов (`get`), события (`describe`), логи (в том числе `--previous`).
- Если под `Running`: готовность (readiness), endpoints Service, `targetPort`, DNS, NetworkPolicy.
- Проверка ноды, taints, квот и admission.
- При необходимости — `kubectl debug` (под и нода); финализаторы при `Terminating`.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
