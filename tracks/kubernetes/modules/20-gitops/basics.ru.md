---
title: "GitOps и Argo CD: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 20-gitops
part: 3
tags: [gitops, argocd, application, sync, declarative, drift]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps и Argo CD: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Deployment](../05-deployments/overview.ru.md), [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), Argo CD v3.6.2. Источник манифестов — публичный репозиторий `argoproj/argocd-example-apps` (приложение `guestbook`), поэтому собственный репозиторий для опытов не нужен.

## Зачем это нужно

Без GitOps выкладка выглядит так: кто-то запускает `kubectl apply` или CI-скрипт. Потом непонятно, что именно сейчас в кластере, кто и когда это менял, и как вернуть прежнее состояние. GitOps отвечает: «то, что в Git, и есть правда; кластер обязан ему соответствовать». Из этого следуют аудит (история коммитов), откат (revert), воспроизводимость (тот же репозиторий создаёт тот же кластер) и самовосстановление.

## Принципы

1. **Декларативность:** желаемое состояние описано манифестами, а не командами.
2. **Версионирование:** манифесты хранятся в Git, у каждого изменения есть автор и история.
3. **Автоматический pull:** контроллер в кластере сам забирает изменения.
4. **Непрерывная сверка:** контроллер постоянно сравнивает фактическое состояние с желаемым и устраняет расхождения.

## Установка Argo CD

```text
$ kubectl create namespace argocd
$ kubectl apply -n argocd --server-side \
    -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
$ kubectl -n argocd get pods
argocd-application-controller-0                    1/1   Running
argocd-applicationset-controller-76c9bbbc9f-kkk82  1/1   Running
argocd-dex-server-c5c68fc58-mtbvs                  1/1   Running
argocd-notifications-controller-67bf66d585-78dmn   1/1   Running
argocd-redis-665bf4467d-wb62t                      1/1   Running
argocd-repo-server-7fb4dfb7fb-s92mr                1/1   Running
argocd-server-7c54fc4fd9-qhrxs                     1/1   Running
$ kubectl get crd | grep argoproj
applications.argoproj.io   applicationsets.argoproj.io   appprojects.argoproj.io
```

Манифест взят из ветки `stable`, и в опыте это образ `quay.io/argoproj/argocd:v3.6.2`. Для воспроизводимости в production фиксируйте версию, а не ветку. Установились семь компонентов и три CRD; их роли — в [экспертной теории](expert.ru.md#внутреннее-устройство).

## Первый Application

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata: {name: guestbook, namespace: argocd}
spec:
  project: default
  source:
    repoURL: https://github.com/argoproj/argocd-example-apps.git
    targetRevision: HEAD
    path: guestbook
  destination: {server: https://kubernetes.default.svc, namespace: learn-20}
  syncPolicy:
    syncOptions: [CreateNamespace=true]
```

- `source` — где лежат манифесты (репозиторий, ревизия, путь).
- `destination` — куда применять: `https://kubernetes.default.svc` означает «тот кластер, где работает Argo CD».
- `project: default` — проект Argo CD (границы: какие репозитории и кластеры разрешены).
- `syncOptions: [CreateNamespace=true]` — создать namespace назначения, если его нет.

Application — объект в namespace `argocd`, а не в namespace приложения.

## Статусы: Sync и Health

Сразу после создания:

```text
$ kubectl -n argocd get app guestbook --no-headers
guestbook   OutOfSync   Missing
$ kubectl -n argocd get app guestbook -o jsonpath='{range .status.resources[*]}{.kind}/{.name} {.status}{"\n"}{end}'
Service/guestbook-ui OutOfSync
Deployment/guestbook-ui OutOfSync
```

- **Sync status:** `Synced` — кластер совпадает с Git, `OutOfSync` — нет.
- **Health status:** `Healthy`, `Progressing`, `Degraded`, `Missing` (объекта нет в кластере) и другие.

Argo CD прочитал Git, увидел два объекта, но в кластере их нет: `OutOfSync` и `Missing`. Автоматической синхронизации по умолчанию нет, поэтому пока ничего не применено.

## Синхронизация

Синхронизация применяет манифесты из Git. Её запускают командой `argocd app sync` (CLI) или в интерфейсе. Без CLI то же делает запись в поле `operation` приложения:

```text
$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"operation":{"sync":{"syncOptions":["CreateNamespace=true"]}}}'
$ kubectl -n argocd get app guestbook --no-headers
guestbook   Synced   Progressing
$ kubectl -n learn-20 get deploy,svc --no-headers
deployment.apps/guestbook-ui   0/1
service/guestbook-ui           ClusterIP
$ kubectl -n argocd get app guestbook -o jsonpath='{.status.operationState.phase}'
Succeeded
```

`Synced` — манифесты применены; `Progressing` — Deployment ещё запускает под; затем `Healthy`.

Первая попытка синхронизации в опыте закончилась ошибкой:

```text
one or more objects failed to apply, reason: namespaces "learn-20" not found
```

Причина: в ручной операции опцию `CreateNamespace=true` нужно указать явно (политика приложения в операции не наследуется), подробнее — в [продвинутой теории](advanced.ru.md#опции-синхронизации-и-операции).

## Ошибки новичка

- ❌ Ждать, что `Application` сам применит манифесты → ✅ нужна синхронизация (вручную или `automated`).
- ❌ Путать `OutOfSync` (расхождение с Git) и `Degraded` (приложение нездорово).
- ❌ Брать манифесты из `HEAD` без фиксации в production → ✅ тег или коммит в `targetRevision`.
- ❌ Создавать `Application` не в `argocd` → ✅ по умолчанию Argo CD следит за своим namespace.
- ❌ Править объекты в кластере вручную → ✅ менять Git.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — автосинхронизация, self-heal, Helm, удаление
- CRD и операторы: [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md)
- Deployment: [05 · Deployment](../05-deployments/overview.ru.md)

## Источники

- [Argo CD: Getting Started](https://argo-cd.readthedocs.io/en/stable/getting_started/)
- [Argo CD: Declarative Setup](https://argo-cd.readthedocs.io/en/stable/operator-manual/declarative-setup/)
- [Argo CD: Automated Sync Policy](https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/)
- [argocd-example-apps](https://github.com/argoproj/argocd-example-apps)
- [OpenGitOps: principles](https://opengitops.dev/)
