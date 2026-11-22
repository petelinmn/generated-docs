---
title: "Квиз: ConfigMap и Secret"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, environment-variables, volume, base64, immutable]
status: draft
updated: 2026-11-22
---

# Квиз: ConfigMap и Secret

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что содержит поле `data` у Secret?

- A) Зашифрованные значения
- B) Значения в base64
- C) Значения открытым текстом
- D) Хэши значений

<details><summary>Ответ</summary>

**B.** base64 — кодировка, значение декодируется командой `base64 -d` (в опыте `UzNjcjN0IQ==` → `S3cr3t!`). Шифрования нет. Открытым текстом пишут в `stringData`, а API сохраняет его в `data` в base64.
→ [Базовая теория](basics.ru.md#secret)

</details>

## 2. 🟢 Чем `envFrom` отличается от `env` + `valueFrom`?

- A) Ничем
- B) `envFrom` подключает все ключи объекта как переменные, `valueFrom` — одно значение под выбранным именем
- C) `envFrom` работает только для Secret
- D) `valueFrom` обновляет значение на лету

<details><summary>Ответ</summary>

**B.** `envFrom` берёт все ключи (можно с `prefix`), `valueFrom` — конкретный ключ. Оба фиксируют значения при старте контейнера.
→ [Базовая теория](basics.ru.md#как-подключить-к-поду)

</details>

## 3. 🟢 Какой флаг нельзя смешивать с `--from-file` и `--from-literal` в `kubectl create configmap`?

- A) `--dry-run`
- B) `--from-env-file`
- C) `--namespace`
- D) `--output`

<details><summary>Ответ</summary>

**B.** Ошибка: `from-env-file cannot be combined with from-file or from-literal`.
→ [Базовая теория](basics.ru.md#configmap)

</details>

## 4. 🟢 Какое значение у ключа `TIMEOUT` в ConfigMap допустимо в YAML?

- A) `TIMEOUT: 30`
- B) `TIMEOUT: "30"`
- C) Любое число
- D) Только значения в base64

<details><summary>Ответ</summary>

**B.** Значения `data` — строки; число нужно писать в кавычках. Бинарные данные кладут в `binaryData`.
→ [Базовая теория](basics.ru.md#создание)

</details>

## 5. 🟡 Вы изменили ConfigMap. Что обновится в работающем поде?

- A) Всё сразу
- B) Файлы тома примерно через минуту; переменные окружения и файл через `subPath` — нет
- C) Только переменные окружения
- D) Ничего, даже файлы тома

<details><summary>Ответ</summary>

**B.** В опыте том обновился через ~57 с, переменная осталась `prod`, файл через `subPath` — тоже `prod`.
→ [Продвинутая теория](advanced.ru.md#обновление-значений)

</details>

## 6. 🟡 Какой статус у пода, который ссылается на несуществующий ключ ConfigMap?

- A) `Pending`
- B) `ImagePullBackOff`
- C) `CreateContainerConfigError`
- D) `CrashLoopBackOff`

<details><summary>Ответ</summary>

**C.** Событие: `couldn't find key nokey in ConfigMap learn-07/app-config`. С `optional: true` под стартует без переменной.
→ [Продвинутая теория](advanced.ru.md#отсутствующие-объекты-и-ключи)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) У `immutable: true` объекта нельзя изменить `data`
- [ ] B) Том Secret монтируется как `tmpfs`
- [ ] C) `kubectl describe secret` показывает значения в открытом виде
- [ ] D) Изменение ConfigMap автоматически перезапускает Deployment

<details><summary>Ответ</summary>

**A, B.** C неверно: `describe` показывает только размеры (`password: 7 bytes`). D неверно: число ReplicaSet не изменилось, поды остались со старыми значениями.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Как заставить Deployment подхватить новое значение ConfigMap?

- A) Подождать минуту
- B) Изменить шаблон пода: `rollout restart`, хэш ConfigMap в аннотации или новое имя ConfigMap
- C) Удалить ConfigMap
- D) Увеличить `replicas`

<details><summary>Ответ</summary>

**B.** Rollout запускает только изменение шаблона пода. Новые поды из увеличения `replicas` тоже получат новое значение, но уже работающие останутся со старым.
→ [Продвинутая теория](advanced.ru.md#изменение-configmap-не-перезапускает-поды)

</details>

## 9. 🔴 Найди ошибку

> «Мы шифруем секреты: в манифесте они в base64, и даже `kubectl describe` их не показывает. Значит, атакующий с доступом к бэкапу etcd ничего не прочитает».

<details><summary>Ответ</summary>

Неверно: base64 — не шифрование, а в etcd Secret по умолчанию лежит открытым текстом (в опыте `password..S3cr3t!` в значении ключа, флага `--encryption-provider-config` нет). `describe` скрывает значения только в выводе kubectl. Нужны шифрование etcd (`EncryptionConfiguration`/KMS), RBAC и защита резервных копий.
→ [Экспертная теория](expert.ru.md#подвох-secret-в-etcd-лежит-открытым-текстом)

</details>

## 10. 🔴 Открытый вопрос

Как организовать хранение секретов для приложения в GitOps-процессе (манифесты в Git)? Назови минимум два подхода и их компромиссы.

<details><summary>Ключевые пункты ответа</summary>

- Нельзя хранить `Secret` в Git открытым текстом (base64 и `stringData` — не защита).
- Sealed Secrets: шифрование публичным ключом кластера, в Git лежит `SealedSecret`; плюс — простота, минус — ключ привязан к кластеру.
- SOPS: шифрование значений прямо в манифесте; нужен ключ (KMS, age) и интеграция с GitOps.
- External Secrets Operator или Secrets Store CSI Driver: в Git только ссылка на секрет во внешнем хранилище (Vault, облачный менеджер секретов); минус — зависимость от внешней системы.
- В любом случае: RBAC на `secrets`, шифрование etcd, монтирование файлами с правами `0400`.

</details>

→ [Экспертная теория](expert.ru.md#внешние-хранилища-секретов-и-альтернативы)
