---
title: "Квиз: Deployment и ReplicaSet"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 05-deployments
part: 1
tags: [deployment, replicaset, rolling-update, rollback, progress-deadline]
status: draft
updated: 2026-11-22
---

# Квиз: Deployment и ReplicaSet

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что создаёт Deployment при запуске на 3 реплики?

- A) Три пода напрямую
- B) ReplicaSet, который создаёт три пода
- C) Три ReplicaSet по одному поду
- D) StatefulSet

<details><summary>Ответ</summary>

**B.** Цепочка Deployment → ReplicaSet → Pod; в опыте был один ReplicaSet `web-6d8567df8` и три пода с тем же префиксом.
→ [Базовая теория](basics.ru.md#deployment-replicaset-и-поды)

</details>

## 2. 🟢 Что запускает rolling update?

- A) Любое изменение Deployment
- B) Изменение шаблона пода (`spec.template`)
- C) Изменение `replicas`
- D) Удаление пода

<details><summary>Ответ</summary>

**B.** Смена шаблона даёт новый хэш и новый ReplicaSet. Изменение `replicas` лишь масштабирует текущий ReplicaSet, удаление пода его пересоздаёт.
→ [Базовая теория](basics.ru.md#обновление-образа)

</details>

## 3. 🟢 Как откатить Deployment на предыдущую версию?

- A) `kubectl rollout undo deploy/web`
- B) Удалить все поды
- C) `kubectl scale --replicas=0`
- D) Откатить нельзя

<details><summary>Ответ</summary>

**A.** Команда возвращает шаблон предыдущей ревизии; старые ReplicaSet хранятся для этого (`revisionHistoryLimit`, по умолчанию 10).
→ [Базовая теория](basics.ru.md#откат)

</details>

## 4. 🟢 Что обязательно у Deployment должно совпадать?

- A) Имя Deployment и имя образа
- B) `selector.matchLabels` и метки `template.metadata.labels`
- C) `replicas` и число нод
- D) Namespace и имя пода

<details><summary>Ответ</summary>

**B.** Иначе API отклонит объект: ``selector` does not match template `labels` ``.
→ [Базовая теория](basics.ru.md#манифест-deployment)

</details>

## 5. 🟡 Сколько подов за раз заменяет rolling update при 3 репликах и значениях по умолчанию?

- A) Все три сразу
- B) По одному: `maxSurge` округляется вверх до 1, `maxUnavailable` вниз до 0
- C) По два
- D) Ни одного, пока не удалят старые вручную

<details><summary>Ответ</summary>

**B.** 25% от 3 = 0.75: `maxSurge` → 1, `maxUnavailable` → 0. В трассе ReplicaSet-ов видно: новый RS сначала получает 1 реплику, и старый уменьшается только после готовности нового пода.
→ [Продвинутая теория](advanced.ru.md#rolling-update-по-шагам)

</details>

## 6. 🟡 Образ в Deployment указан с опечаткой. Что будет?

- A) Все поды упадут
- B) Новый под в `ErrImagePull`, старые поды продолжают работать, Deployment `READY 3/3`
- C) Kubernetes автоматически откатится
- D) Deployment будет удалён

<details><summary>Ответ</summary>

**B.** Rolling update не удаляет старые поды, пока новые не готовы. Откат не автоматический: через `progressDeadlineSeconds` появится `ProgressDeadlineExceeded`, но поды остаются. Откат — `kubectl rollout undo`.
→ [Продвинутая теория](advanced.ru.md#застрявший-деплой)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `Recreate` останавливает все старые поды перед запуском новых
- [ ] B) `kubectl rollout restart` одномоментно убивает все поды
- [ ] C) После `rollout pause` изменения шаблона не запускают rollout до `resume`
- [ ] D) Значение `progressDeadlineSeconds` по умолчанию — 600

<details><summary>Ответ</summary>

**A, C, D.** B неверно: `restart` добавляет аннотацию в шаблон, поды заменяются по стратегии обновления.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Почему `rollout history` показал одинаковый `CHANGE-CAUSE` у ревизий 1 и 2?

- A) Это ошибка kubectl
- B) Аннотацию `kubernetes.io/change-cause` поставили до смены образа, и она скопировалась на старый ReplicaSet
- C) Обе ревизии одинаковы
- D) История не хранится

<details><summary>Ответ</summary>

**B.** Подпись нужно ставить после изменения шаблона.
→ [Продвинутая теория](advanced.ru.md#причина-изменения-change-cause)

</details>

## 9. 🔴 Найди ошибку

> «В CI после `kubectl set image` я проверяю `kubectl get deploy web` и по `READY 3/3` считаю деплой успешным».

<details><summary>Ответ</summary>

При неверном образе `READY 3/3` и `Available=True`, потому что продолжают работать старые поды; новый под в `ErrImagePull` виден лишь как `UP-TO-DATE 1`. Нужно проверять `kubectl rollout status deploy/web --timeout=…`: его код возврата отражает результат обновления.
→ [Экспертная теория](expert.ru.md#подвох-застрявшее-обновление-выглядит-здоровым)

</details>

## 10. 🔴 Открытый вопрос

Объясни, как Kubernetes отслеживает ревизии Deployment и что происходит с номерами при откате. Как это влияет на ведение истории релизов?

<details><summary>Ключевые пункты ответа</summary>

- Каждая версия шаблона — отдельный ReplicaSet с аннотацией `deployment.kubernetes.io/revision`; хэш шаблона — в имени и метке `pod-template-hash`.
- Откат переиспользует старый ReplicaSet и присваивает ему новый наибольший номер; прежний номер пропадает (в опыте ревизия 1 стала 3, `revision-history: "1"`).
- Число хранимых ревизий — `revisionHistoryLimit` (по умолчанию 10).
- Номер ревизии не является постоянным идентификатором релиза: историю ведут в Git и метках; `change-cause` ставят после изменения шаблона.

</details>

→ [Экспертная теория](expert.ru.md#подвох-номера-ревизий-меняются-после-отката)
