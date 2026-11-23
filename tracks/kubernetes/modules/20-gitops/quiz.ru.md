---
title: "Квиз: GitOps и Argo CD"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 20-gitops
part: 3
tags: [gitops, argocd, sync, self-heal]
status: draft
updated: 2026-11-23
---

# Квиз: GitOps и Argo CD

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что в GitOps считается источником истины?

- A) Состояние кластера
- B) Репозиторий Git
- C) Образ контейнера
- D) Интерфейс Argo CD

<details><summary>Ответ</summary>

**B.** Кластер обязан соответствовать Git.
→ [Базовая теория](basics.ru.md#принципы)

</details>

## 2. 🟢 Что описывает `Application`?

- A) Только образ контейнера
- B) Источник (репозиторий, ревизия, путь) и назначение (кластер, namespace)
- C) Только права доступа
- D) Только Helm-значения

<details><summary>Ответ</summary>

**B.** Плюс политика синхронизации.
→ [Базовая теория](basics.ru.md#первый-application)

</details>

## 3. 🟢 Приложение только что создано, автосинхронизации нет. Какие статусы?

- A) `Synced` / `Healthy`
- B) `OutOfSync` / `Missing`
- C) `Degraded` / `Unknown`
- D) `Failed`

<details><summary>Ответ</summary>

**B.** Манифесты прочитаны, но в кластере их нет.
→ [Базовая теория](basics.ru.md#статусы-sync-и-health)

</details>

## 4. 🟢 Что значит `Progressing` в Health status?

- A) Расхождение с Git
- B) Ресурсы применены, но ещё не готовы (например, под запускается)
- C) Ошибка применения
- D) Приложение удаляется

<details><summary>Ответ</summary>

**B.** Это здоровье, а не соответствие Git.
→ [Базовая теория](basics.ru.md#синхронизация)

</details>

## 5. 🟡 Вы вручную сделали `kubectl scale` у приложения без автосинхронизации. Что покажет Argo CD?

- A) `Synced` / `Healthy`
- B) `OutOfSync` / `Healthy`, объект `Deployment` отличается от Git
- C) `Degraded`
- D) Ничего

<details><summary>Ответ</summary>

**B.** В опыте именно так.
→ [Продвинутая теория](advanced.ru.md#дрейф-и-self-heal)

</details>

## 6. 🟡 После включения `selfHeal` реплики вернулись к значению из Git. Это…

- A) Ошибка Argo CD
- B) Ожидаемое поведение: ручная правка считается дрейфом и откатывается
- C) Работа HPA
- D) Работа kubelet

<details><summary>Ответ</summary>

**B.** Для срочных изменений меняют Git.
→ [Продвинутая теория](advanced.ru.md#дрейф-и-self-heal)

</details>

## 7. 🟡 Ручная операция `sync` упала: `namespaces "learn-20" not found`, хотя в `syncPolicy` есть `CreateNamespace=true`. Почему?

- A) Опция не поддерживается
- B) Операция использует свои `syncOptions`, их нужно указать явно
- C) Namespace закрыт RBAC
- D) Репозиторий недоступен

<details><summary>Ответ</summary>

**B.** В опыте с `syncOptions` в операции синхронизация прошла.
→ [Продвинутая теория](advanced.ru.md#опции-синхронизации-и-операции)

</details>

## 8. 🟡 Вы удалили `Application` без финализатора. Что с ресурсами?

- A) Удалены
- B) Остались в кластере
- C) Отправлены в архив
- D) Заморожены

<details><summary>Ответ</summary>

**B.** Каскад включает `resources-finalizer.argocd.argoproj.io`.
→ [Продвинутая теория](advanced.ru.md#удаление-application)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) Argo CD может рендерить Helm-чарт и применять результат
- [ ] B) При Helm-источнике в кластере создаётся релиз Helm
- [ ] C) `OutOfSync` и `Healthy` могут быть одновременно
- [ ] D) Автосинхронизация повторяет неудавшуюся операцию для той же ревизии

<details><summary>Ответ</summary>

**A, C.** B неверно: Argo CD применяет результат `helm template`, объектов релиза нет. D неверно: в опыте статус остался `Failed`.
→ [Продвинутая теория](advanced.ru.md#helm-и-kustomize-как-источник)

</details>

## 10. 🔴 Найдите ошибку

> «Мы используем GitOps, поэтому секреты спокойно лежат в Git в виде манифестов Secret».

<details><summary>Ответ</summary>

Secret хранит значения в base64 — это кодирование, не шифрование; Git хранит историю навсегда. Нужны шифрование (Sealed Secrets, SOPS) или внешнее хранилище.
→ [Подвох 7](expert.ru.md#7-секреты-в-git)

</details>

## 11. 🔴 Какой компонент Argo CD рендерит Helm и Kustomize?

- A) argocd-server
- B) argocd-repo-server
- C) argocd-redis
- D) argocd-dex-server

<details><summary>Ответ</summary>

**B.** Он клонирует репозитории и рендерит манифесты; контроллер сравнивает результат с кластером.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 12. 🔴 Открытый вопрос

Сравните push-деплой из CI и GitOps с Argo CD.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Push: CI применяет изменения один раз, ему нужен доступ к кластеру, дрейфа он не видит.
- GitOps: контроллер в кластере тянет Git, постоянно сверяет, показывает дрейф, откатывает ручные правки; история и аудит в Git.
- Цена GitOps: дополнительный компонент, права, работа с секретами.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
