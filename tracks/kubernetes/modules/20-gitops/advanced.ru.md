---
title: "GitOps и Argo CD: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 20-gitops
part: 3
tags: [argocd, auto-sync, self-heal, drift, sync-options, helm, finalizer, prune]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps и Argo CD: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Опыты — в kind, источник — репозиторий `argoproj/argocd-example-apps`.

## Автоматическая синхронизация

В `syncPolicy` включается автосинхронизация:

```yaml
syncPolicy:
  syncOptions: [CreateNamespace=true]
  automated:
    prune: false       # удалять ли то, что пропало из Git
    selfHeal: false    # откатывать ли ручные правки в кластере
```

С `automated` Argo CD применяет новую ревизию из Git без команды. Что делают флаги:

- `prune` — удалять из кластера объекты, которых больше нет в Git. По умолчанию выключено: удаление опасно.
- `selfHeal` — возвращать состояние из Git, если объект изменили в кластере.

> ⚠️ Проверить: `prune` и интервал опроса репозитория (по документации около трёх минут) в опытах не проверялись: у публичного репозитория нет коммитов, которые можно было бы изменить.

## Дрейф и self-heal

**Дрейф (drift)** — расхождение между кластером и Git. Опыт: после синхронизации `guestbook-ui` с 1 репликой в Git. Автосинхронизацию выключили и вручную масштабировали:

```text
$ kubectl -n learn-20 scale deploy guestbook-ui --replicas=3
… через 30 секунд …
$ kubectl -n argocd get app guestbook --no-headers
guestbook   OutOfSync   Healthy
$ kubectl -n argocd get app guestbook -o jsonpath='{range .status.resources[?(@.status=="OutOfSync")]}{.kind}/{.name}{"\n"}{end}'
Deployment/guestbook-ui
$ kubectl -n learn-20 get deploy guestbook-ui -o jsonpath='{.spec.replicas}'
3
```

Приложение здорово (`Healthy`), но не совпадает с Git (`OutOfSync`), и Argo CD показывает, какой объект отличается. Теперь включаем `selfHeal`:

```text
$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"spec":{"syncPolicy":{"automated":{"selfHeal":true}}}}'
… через 5–10 секунд …
guestbook   Synced   Healthy   replicas=1
```

Argo CD сам вернул 1 реплику. Это главное отличие GitOps от «один раз применили»: руками кластер не изменить надолго.

## Опции синхронизации и операции

`syncPolicy.syncOptions` меняют поведение синхронизации; в опыте использована `CreateNamespace=true` (создать namespace назначения).

Подвох с ручной операцией. Первая синхронизация через запись `operation: {sync: {}}` закончилась так:

```text
$ kubectl -n argocd get app guestbook -o jsonpath='{.status.operationState.phase}{" | "}{.status.operationState.message}'
Failed | one or more objects failed to apply, reason: namespaces "learn-20" not found
```

Опция `CreateNamespace=true` стояла в `syncPolicy` приложения, но операция, созданная вручную, использует **свои** `syncOptions`. Затем включили `automated`, и статус по-прежнему остался `Failed … OutOfSync Missing`: автосинхронизация не повторяет неудавшуюся операцию для той же ревизии. Помогло явное указание опции в операции:

```text
$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"operation":{"sync":{"syncOptions":["CreateNamespace=true"]}}}'
… operationState.phase = Succeeded
```

> ⚠️ Проверить: как с этим справляются `argocd app sync` и интерфейс (по документации они учитывают политику приложения), в опыте не проверялось: CLI не использовался.

> ⚠️ Проверить: перечень остальных опций (`Prune=false`, `ServerSideApply=true`, `Replace=true`, `ApplyOutOfSyncOnly=true`), `retry` и `syncWaves`/хуки взят из документации; запуском проверена только `CreateNamespace=true`.

## Helm и Kustomize как источник

Argo CD определяет тип источника по содержимому каталога. Для каталога с чартом Helm в `source.helm` задают значения:

```yaml
source:
  repoURL: https://github.com/argoproj/argocd-example-apps.git
  targetRevision: HEAD
  path: helm-guestbook
  helm:
    parameters: [{name: replicaCount, value: "2"}]
destination: {server: https://kubernetes.default.svc, namespace: learn-20}
syncPolicy:
  automated: {prune: true, selfHeal: true}
```

```text
$ kubectl -n argocd get app helm-guestbook -o jsonpath='{.status.sourceType}'
Helm
$ kubectl -n learn-20 get deploy
guestbook-ui     1/1
helm-guestbook   2/2
```

Параметр `replicaCount=2` попал в Deployment. Argo CD сам выполнил `helm template` и применил результат: релизов Helm в кластере нет ([модуль 13](../13-helm/advanced.ru.md) показывает, как выглядит релиз Helm).

> ⚠️ Проверить: источник Kustomize и несколько источников (`sources`) не запускались.

## Удаление Application

Что происходит с ресурсами, когда удаляют `Application`, зависит от финализатора:

```text
$ kubectl -n argocd delete app helm-guestbook      # без финализатора
$ kubectl -n learn-20 get deploy
guestbook-ui     1/1
helm-guestbook   2/2      ← ресурсы остались

$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"metadata":{"finalizers":["resources-finalizer.argocd.argoproj.io"]}}'
$ kubectl -n argocd delete app guestbook           # с финализатором
$ kubectl -n learn-20 get deploy
helm-guestbook   2/2      ← guestbook-ui удалён вместе с приложением
```

Без финализатора `Application` удаляется, а ресурсы остаются «осиротевшими». Финализатор `resources-finalizer.argocd.argoproj.io` включает каскадное удаление. Kubernetes при добавлении выдал предупреждение (`prefer a domain-qualified finalizer name including a path`): это предложение, не ошибка.

## Лучшие практики

- Один репозиторий (или каталог) на окружение; общие манифесты — через Kustomize или Helm.
- Фиксируйте ревизию (`targetRevision`) тегом или коммитом в production.
- `selfHeal` включайте осознанно: срочные ручные правки перестанут работать.
- `prune` включайте только когда уверены в содержимом Git.
- Секреты — через шифрование (Sealed Secrets, SOPS) или внешние хранилища; не в открытом виде.
- Argo CD не должен быть единственной точкой доступа: храните резервную копию его конфигурации.

> ⚠️ Проверить: Sealed Secrets, SOPS, External Secrets, ApplicationSet и «app of apps» в опытах не запускались (CRD `applicationsets.argoproj.io` существует и установился).

## Частые ошибки

- ❌ Ручная операция без нужных `syncOptions` → ✅ указывать явно или использовать CLI.
- ❌ Ждать повтора неудавшейся автосинхронизации → ✅ исправить причину и запустить синхронизацию заново.
- ❌ Удалить `Application` и ждать, что ресурсы исчезнут → ✅ финализатор.
- ❌ Править ресурс в кластере при включённом `selfHeal` → ✅ править Git.
- ❌ Применять `HEAD` без фиксации → ✅ тег или хеш коммита.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Helm: [13 · Helm](../13-helm/overview.ru.md)
- Kustomize: [14 · Kustomize](../14-kustomize/overview.ru.md)
- Удаление и финализаторы: [19 · CRD и операторы](../19-crd-and-operators/expert.ru.md#внутреннее-устройство)

## Источники

- [Argo CD: Automated Sync Policy](https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/)
- [Argo CD: Sync Options](https://argo-cd.readthedocs.io/en/stable/user-guide/sync-options/)
- [Argo CD: App Deletion](https://argo-cd.readthedocs.io/en/stable/user-guide/app_deletion/)
- [Argo CD: Helm](https://argo-cd.readthedocs.io/en/stable/user-guide/helm/)
- [Argo CD: Application Specification](https://argo-cd.readthedocs.io/en/stable/operator-manual/application.yaml)
