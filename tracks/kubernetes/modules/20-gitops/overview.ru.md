---
title: "GitOps и Argo CD"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 20-gitops
part: 3
levels: [1, 2, 3, 4]
tags: [gitops, argocd, application, sync, self-heal, drift, declarative, continuous-delivery]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps и Argo CD

🌐 **Русский** · [English](overview.en.md)

## Коротко

**GitOps** — способ доставки в кластер, где единственный источник истины — репозиторий Git. В нём лежат манифесты желаемого состояния, а специальный контроллер в самом кластере непрерывно сравнивает их с фактическим состоянием и приводит кластер в соответствие (модель pull). Изменение вносится коммитом, откат — откатом коммита, а ручные правки в кластере считаются отклонением (drift), которое видно и может быть исправлено автоматически. Мы разбираем **Argo CD**: он ставится в кластер и описывает развёртывания объектами `Application`.

## Ключевые идеи

- **Git — источник истины.** Желаемое состояние описано декларативно и версионируется.
- **Контроллер сам тянет изменения (pull),** а не получает их от CI. Кластеру не нужны входящие доступы из CI.
- **`Application` в Argo CD связывает источник и цель:** репозиторий, путь и ревизию → кластер и namespace.
- **Два статуса:** `Sync` (совпадает ли кластер с Git: `Synced`, `OutOfSync`) и `Health` (здоров ли результат: `Healthy`, `Progressing`, `Missing`).
- **Автоматическая синхронизация (`automated`)** применяет изменения из Git сама; **`selfHeal`** откатывает ручные правки в кластере.
- **Источник может быть обычными манифестами, Helm или Kustomize.**
- **Удаление `Application` по умолчанию не удаляет ресурсы:** для каскада нужен финализатор.
- **Argo CD сам — оператор:** он состоит из контроллеров и CRD (`Application`, `AppProject`, `ApplicationSet`).

## Минимальный пример

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
    automated: {selfHeal: true}
```

```text
$ kubectl -n argocd get app guestbook
NAME        SYNC STATUS   HEALTH STATUS
guestbook   Synced        Healthy
```

В примере используется публичный репозиторий примеров проекта Argo CD.

## Когда применять, а когда нет

| Ситуация | Решение |
|---|---|
| Нужны аудит, откат и воспроизводимость окружений | GitOps |
| Много кластеров или окружений с общими манифестами | GitOps + Helm/Kustomize |
| Разовый эксперимент на локальном кластере | `kubectl apply` |
| Секреты в открытом виде | нельзя класть в Git (нужны шифрование или внешнее хранилище) |

- ❌ Не правьте приложение `kubectl edit`'ом: при `selfHeal` правка исчезнет, без него — получите расхождение.
- ❌ Не кладите секреты в Git без шифрования.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | принципы GitOps, установка Argo CD, первый `Application`, синхронизация |
| [2. Продвинутая теория](advanced.ru.md) | автосинхронизация, self-heal и дрейф, опции синхронизации, Helm как источник, удаление приложения |
| [3. Экспертная теория](expert.ru.md) | справочник полей и статусов, устройство Argo CD, подвохи, Flux и другие подходы, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 20-gitops level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 20-gitops level 6` |

## Связанные материалы

- Пререквизиты: [05 · Deployment](../05-deployments/overview.ru.md), [13 · Helm](../13-helm/overview.ru.md), [14 · Kustomize](../14-kustomize/overview.ru.md), [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md)
- Права и namespace: [10 · Namespaces и RBAC](../10-namespaces-and-rbac/overview.ru.md)
- Предыдущий модуль: [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md)
- Дальше: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
- Секреты: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
