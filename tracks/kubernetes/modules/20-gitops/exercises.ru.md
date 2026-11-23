---
title: "Упражнения: GitOps и Argo CD"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 20-gitops
part: 3
tags: [gitops, argocd, sync, self-heal, helm]
status: draft
updated: 2026-11-23
---

# Упражнения: GitOps и Argo CD

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind и доступ в интернет (установка Argo CD и клонирование публичного репозитория `argocd-example-apps`). После работы: `kubectl delete namespace argocd learn-20` и удалите CRD `*.argoproj.io`.

## 1. 🟢 Установка Argo CD

**Задача:** установите Argo CD в namespace `argocd` манифестом `install.yaml` (с `--server-side`) и дождитесь готовности. Перечислите поды и CRD.

**Ожидаемый результат:** семь компонентов `Running` (application-controller, applicationset-controller, dex, notifications, redis, repo-server, server) и CRD `applications`, `applicationsets`, `appprojects`.

<details><summary>Подсказка</summary>

`kubectl -n argocd wait --for=condition=Available deploy --all`.

</details>

<details><summary>Решение</summary>

Команды — в [базовой теории](basics.ru.md#установка-argo-cd).

</details>

## 2. 🟢 Первый Application

**Задача:** создайте `Application` для `guestbook` и посмотрите статусы до синхронизации.

**Ожидаемый результат:** `OutOfSync` / `Missing`; ресурсы `Service/guestbook-ui` и `Deployment/guestbook-ui` — `OutOfSync`.

<details><summary>Подсказка</summary>

`kubectl -n argocd get app guestbook -o jsonpath='{range .status.resources[*]}{.kind}/{.name} {.status}{"\n"}{end}'`.

</details>

<details><summary>Решение</summary>

```text
guestbook   OutOfSync   Missing
Service/guestbook-ui OutOfSync
Deployment/guestbook-ui OutOfSync
```

</details>

## 3. 🟡 Синхронизация

**Задача:** выполните синхронизацию записью в `operation`. Сначала без `syncOptions`, затем с `CreateNamespace=true`. Объясните разницу.

**Ожидаемый результат:** сначала `Failed … namespaces "learn-20" not found`; затем `Succeeded`, `Synced` / `Progressing` → `Healthy`.

<details><summary>Подсказка</summary>

`kubectl -n argocd patch app guestbook --type=merge -p '{"operation":{"sync":{}}}'`.

</details>

<details><summary>Решение</summary>

```text
Failed | one or more objects failed to apply, reason: namespaces "learn-20" not found
… с syncOptions: Succeeded
```

</details>

## 4. 🟡 Дрейф и self-heal

**Задача:** без автосинхронизации сделайте `kubectl scale` у `guestbook-ui` до 3 реплик и посмотрите статус. Включите `selfHeal` и дождитесь возврата.

**Ожидаемый результат:** `OutOfSync` / `Healthy` и `replicas=3`; после `selfHeal` — `Synced` / `Healthy` и `replicas=1`.

<details><summary>Подсказка</summary>

`kubectl -n argocd patch app guestbook --type=merge -p '{"spec":{"syncPolicy":{"automated":{"selfHeal":true}}}}'`.

</details>

<details><summary>Решение</summary>

```text
guestbook   OutOfSync   Healthy     (replicas 3)
guestbook   Synced      Healthy     (replicas=1)
```

</details>

## 5. 🔴 Helm-источник и удаление

**Задача:** создайте `Application` `helm-guestbook` (путь `helm-guestbook`, параметр `replicaCount=2`, `automated` с `prune` и `selfHeal`). Затем удалите его без финализатора и добавьте финализатор к `guestbook` перед удалением. Что останется в `learn-20`?

**Ожидаемый результат:** `sourceType = Helm`, `helm-guestbook 2/2`; после удаления остаётся `helm-guestbook` (без финализатора), `guestbook-ui` удалён (с финализатором).

<details><summary>Подсказка</summary>

Финализатор: `resources-finalizer.argocd.argoproj.io`.

</details>

<details><summary>Решение</summary>

Манифест и результат — в [продвинутой теории](advanced.ru.md#helm-и-kustomize-как-источник) и [здесь](advanced.ru.md#удаление-application).

</details>
