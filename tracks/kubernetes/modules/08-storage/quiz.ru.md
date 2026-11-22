---
title: "Квиз: хранилище"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 08-storage
part: 1
tags: [persistentvolume, persistentvolumeclaim, storageclass, reclaim-policy, access-modes]
status: draft
updated: 2026-11-22
---

# Квиз: хранилище

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что происходит с данными в `emptyDir` при замене пода?

- A) Сохраняются
- B) Удаляются вместе с подом
- C) Копируются на новый под
- D) Переносятся в PV

<details><summary>Ответ</summary>

**B.** `emptyDir` создаётся вместе с подом и удаляется с ним (переживает только перезапуск контейнера). Для долговременных данных нужен PVC.
→ [Базовая теория](basics.ru.md#простые-тома)

</details>

## 2. 🟢 PVC только что создан и находится в `Pending`, ошибок нет. Почему?

- A) Кончилось место
- B) У StorageClass режим `WaitForFirstConsumer`: PVC ждёт пода, который его использует
- C) Забыли создать PV
- D) Нет прав

<details><summary>Ответ</summary>

**B.** Событие: `waiting for first consumer to be created before binding`. После создания пода PVC стал `Bound`, а PV создан автоматически.
→ [Базовая теория](basics.ru.md#pvc-ждёт-первого-пода)

</details>

## 3. 🟢 Кто из объектов не принадлежит namespace?

- A) PVC
- B) Pod
- C) PV
- D) ConfigMap

<details><summary>Ответ</summary>

**C.** PersistentVolume — объект уровня кластера (как и StorageClass). PVC, Pod и ConfigMap живут в namespace.
→ [Базовая теория](basics.ru.md#под-который-использует-pvc)

</details>

## 4. 🟢 Что будет с данными, если удалить PVC у динамически выделенного тома со стандартной политикой?

- A) Сохранятся
- B) PV и данные удалятся (`reclaimPolicy: Delete`)
- C) Данные останутся, но PV станет `Released`
- D) Данные уйдут в архив

<details><summary>Ответ</summary>

**B.** В опыте каталог на ноде (`/var/local-path-provisioner/…`) был удалён вместе с PV. Сохранить данные позволяет `Retain`.
→ [Базовая теория](basics.ru.md#удаление-pvc)

</details>

## 5. 🟡 PVC на 200 МиБ привязан к статическому PV на 500 МиБ. Сколько он получит?

- A) 200 МиБ, остаток свободен для других
- B) 500 МиБ: привязка идёт целиком
- C) Ошибку
- D) 700 МиБ

<details><summary>Ответ</summary>

**B.** В `CAPACITY` PVC — `500Mi`. Остаток другим PVC не отдаётся.
→ [Продвинутая теория](advanced.ru.md#статические-pv-и-правила-привязки)

</details>

## 6. 🟡 Какой статус у PV с `Retain` после удаления PVC?

- A) `Available`
- B) `Bound`
- C) `Released`
- D) `Deleted`

<details><summary>Ответ</summary>

**C.** Данные остались. В PV хранится ссылка `claimRef` на удалённый PVC; чтобы PV стал `Available`, ссылку удаляют.
→ [Продвинутая теория](advanced.ru.md#политика-освобождения-reclaimpolicy)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) Пока под использует PVC, его удаление оставит PVC в `Terminating`
- [ ] B) `ReadWriteOnce` запрещает нескольким подам на одной ноде использовать том
- [ ] C) `ReadWriteOncePod` допускает только один под
- [ ] D) У `standard` в kind разрешено расширение PVC

<details><summary>Ответ</summary>

**A, C.** B неверно: RWO ограничивает ноды, а не поды. D неверно: `allowVolumeExpansion: false`, расширение даёт `Forbidden`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что покажет `describe pod second`, если два пода используют один PVC с `ReadWriteOncePod`?

- A) Оба пода работают
- B) `FailedScheduling … PersistentVolumeClaim with ReadWriteOncePod access mode already in-use by another pod`
- C) `ImagePullBackOff`
- D) `CrashLoopBackOff`

<details><summary>Ответ</summary>

**B.** Второй под остаётся `Pending`: планировщик не допускает второго потребителя.
→ [Продвинутая теория](advanced.ru.md#режимы-доступа)

</details>

## 9. 🔴 Найди ошибку

> «Мы запустили базу данных как Deployment на 3 реплики с одним PVC (`ReadWriteOnce`), данные теперь общие и надёжные».

<details><summary>Ответ</summary>

RWO разрешает запись с одной ноды, поэтому реплики на других нодах не смогут смонтировать том (а на одной ноде несколько писателей одного каталога повредят данные базы). Для приложений с состоянием нужен StatefulSet с `volumeClaimTemplates`: свой PVC на реплику и стабильные имена, а репликацию данных обеспечивает сама база. Дополнительно проверьте `reclaimPolicy`: при `Delete` удаление PVC стирает данные.
→ [Экспертная теория](expert.ru.md#подвох-readwriteonce--про-ноду-а-не-про-под)

</details>

## 10. 🔴 Открытый вопрос

Опиши, что происходит между созданием PVC и появлением файла в контейнере при динамическом выделении с `WaitForFirstConsumer`. Какие события и объекты ты увидишь?

<details><summary>Ключевые пункты ответа</summary>

- PVC создан: `Pending`, событие `waiting for first consumer to be created before binding`.
- Создан под с PVC: scheduler выбирает ноду с учётом хранилища.
- Provisioner (в kind `local-path`) создаёт каталог на ноде и PV `pvc-<uid>` с `nodeAffinity`; события `Provisioning`, `ExternalProvisioning`.
- Контроллер PV связывает PV и PVC: `Bound`, `claimRef` и `volumeName`.
- Kubelet монтирует том и запускает контейнер; файл видно и на ноде в `/var/local-path-provisioner/…`.
- При ошибке — событие `ProvisioningFailed` в PVC.

</details>

→ [Экспертная теория](expert.ru.md#путь-от-pvc-до-файла-в-контейнере)
