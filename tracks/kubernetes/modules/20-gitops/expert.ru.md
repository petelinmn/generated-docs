---
title: "GitOps и Argo CD: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 20-gitops
part: 3
tags: [gitops, argocd, application-controller, repo-server, appproject, applicationset, flux, drift]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps и Argo CD: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что подтверждено опытом в kind с Argo CD v3.6.2 — помечено «в опыте».

## Справочник

### Поля Application

| Поле | Смысл |
|---|---|
| `spec.project` | проект Argo CD (`default`) |
| `spec.source` | `repoURL`, `targetRevision`, `path`; `helm.parameters`; возможно `chart` и `kustomize` |
| `spec.destination` | `server` (или `name`) и `namespace` |
| `spec.syncPolicy.automated` | `prune`, `selfHeal` (в опыте); `allowEmpty` |
| `spec.syncPolicy.syncOptions` | список опций (в опыте `CreateNamespace=true`) |
| `operation.sync` | запрос синхронизации; своё `syncOptions` (в опыте) |
| `metadata.finalizers` | `resources-finalizer.argocd.argoproj.io` — каскадное удаление (в опыте) |

> ⚠️ Проверить: `spec.sources` (несколько источников), `ignoreDifferences`, `syncPolicy.retry`, `revisionHistoryLimit`, `info`, `source.kustomize` и `destination.name` взяты из документации и не запускались.

### Статусы

| Поле | Значения (в опыте жирным) |
|---|---|
| `status.sync.status` | **`Synced`**, **`OutOfSync`**, `Unknown` |
| `status.health.status` | **`Healthy`**, **`Progressing`**, **`Missing`**, `Degraded`, `Suspended`, `Unknown` |
| `status.operationState.phase` | **`Succeeded`**, **`Failed`**, `Running` |
| `status.resources[]` | `kind`, `name`, `status`, `health` — по каждому объекту |
| `status.sourceType` | **`Helm`**, `Kustomize`, `Directory` |
| `status.sync.revision` | коммит Git, с которым сравнивали (в опыте SHA) |

### Команды `kubectl`

| Команда | Назначение |
|---|---|
| `kubectl -n argocd get app` | список приложений: `SYNC STATUS`, `HEALTH STATUS` |
| `kubectl -n argocd get app A -o jsonpath='{.status.resources}'` | статус объектов |
| `kubectl -n argocd patch app A --type=merge -p '{"operation":{"sync":{…}}}'` | запустить синхронизацию |
| `kubectl -n argocd patch app A --type=merge -p '{"spec":{"syncPolicy":…}}'` | изменить политику |

## Внутреннее устройство

Установка состоит из семи компонентов (в опыте):

| Компонент | Роль |
|---|---|
| `argocd-application-controller` (StatefulSet) | сравнивает желаемое (Git) и фактическое (кластер), выполняет синхронизацию |
| `argocd-repo-server` | клонирует репозитории и **рендерит** манифесты (Helm, Kustomize, каталог) |
| `argocd-server` | API и интерфейс |
| `argocd-redis` | кэш |
| `argocd-dex-server` | единый вход (SSO) |
| `argocd-applicationset-controller` | генерирует `Application` по шаблонам (`ApplicationSet`) |
| `argocd-notifications-controller` | уведомления о событиях приложений |

- **Сверка.** Контроллер берёт отрендеренные манифесты у repo-server, сравнивает их с живыми объектами и обновляет `status.sync` и `status.resources`. Расхождение — `OutOfSync` (в опыте это был объект `Deployment/guestbook-ui` после `scale`).
- **Синхронизация.** `operation.sync` порождает применение манифестов (по умолчанию `kubectl apply`). Результат записывается в `status.operationState`. Автосинхронизация лишь создаёт такую операцию сама.
- **Argo CD — оператор.** Его объекты — CRD `applications`, `appprojects`, `applicationsets` ([модуль 19](../19-crd-and-operators/overview.ru.md)); состояние хранится в самом Kubernetes, а Git — единственный внешний источник.
- **Рендер без Helm-релиза.** Для Helm-источника Argo CD сам выполняет `helm template`; в кластере нет объектов релиза Helm (в опыте был только Deployment `helm-guestbook`).
- **Права.** В базовой установке контроллер получает широкие права кластера. Проекты (`AppProject`) ограничивают репозитории, кластеры и namespace, которые приложения могут использовать.

> ⚠️ Проверить: утверждения про права контроллера в базовой установке, `kubectl apply` как способ применения по умолчанию, интервал опроса Git (по документации — около 3 минут) и webhook, `AppProject` с ограничениями, `ApplicationSet`, health-проверки и ресурсные хуки (`PreSync`, `PostSync`), ротация учётных данных и RBAC Argo CD в опытах не запускались.

## Подвохи

### 1. Автосинхронизация не повторяет неудачную операцию

После `Failed` для той же ревизии автосинхронизация молчит (в опыте статус остался `Failed`, `OutOfSync`, `Missing`). Исправьте причину и запустите синхронизацию заново.

### 2. Операция не наследует `syncOptions`

Ручная операция `operation.sync` использует только свои `syncOptions` (в опыте: без `CreateNamespace=true` namespace не создан).

### 3. Удаление приложения без финализатора оставляет ресурсы

Каскад включается финализатором (в опыте: `helm-guestbook` остался, `guestbook-ui` удалён).

### 4. `selfHeal` отменяет срочные ручные правки

Ручной `scale` вернулся к значению Git за секунды (в опыте). Срочные изменения вносят коммитом или временно отключают автосинхронизацию.

### 5. `OutOfSync` не означает «сломано»

В опыте после ручного `scale` приложение было `OutOfSync` и `Healthy` одновременно. Это независимые оси.

### 6. Ветка `stable` меняется

Манифест установки из `stable` меняется со временем. Для воспроизводимости фиксируйте версию (тег релиза).

### 7. Секреты в Git

Манифест Secret только кодирует значения в base64 (см. [модуль 07](../07-config-and-secrets/overview.ru.md)); класть его в Git без шифрования нельзя.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Argo CD | интерфейс, наглядное сравнение, `ApplicationSet`, проекты | тяжёлая установка (7 компонентов), широкие права |
| Flux | компактный набор контроллеров, нативные CRD | нет встроенного интерфейса; не запускался |
| CI с `kubectl apply`/`helm upgrade` (push) | просто, без доп. компонентов | нет непрерывной сверки, CI нужен доступ к кластеру |
| Helm без GitOps | история релизов, откат ([модуль 13](../13-helm/overview.ru.md)) | нет самовосстановления |
| Kustomize + Git + CI | чистый YAML | то же ограничение: нет сверки |

## Вопросы с собеседований

<details><summary>1. Что такое GitOps и чем он отличается от CI/CD с <code>kubectl apply</code>?</summary>

Git — источник истины, контроллер в кластере тянет изменения и постоянно сверяет состояние. В push-модели CI один раз применяет изменения и не следит за дрейфом.

→ [Базовая теория](basics.ru.md#принципы)

</details>

<details><summary>2. Что описывает объект <code>Application</code>?</summary>

Источник (репозиторий, ревизия, путь), назначение (кластер, namespace) и политику синхронизации.

→ [Базовая теория](basics.ru.md#первый-application)

</details>

<details><summary>3. Чем отличаются Sync status и Health status?</summary>

Sync — совпадает ли кластер с Git; Health — здоров ли результат (поды готовы и так далее). Могут быть любые комбинации (в опыте `OutOfSync` + `Healthy`).

→ [Подвох 5](#5-outofsync-не-означает-сломано)

</details>

<details><summary>4. Что делает <code>selfHeal</code>?</summary>

Возвращает состояние из Git, если объект изменили в кластере вручную.

→ [Продвинутая теория](advanced.ru.md#дрейф-и-self-heal)

</details>

<details><summary>5. Ловушка: приложение создано, но ничего не применилось. Почему?</summary>

Автосинхронизации нет, нужна синхронизация вручную: статус `OutOfSync`/`Missing`.

→ [Базовая теория](basics.ru.md#статусы-sync-и-health)

</details>

<details><summary>6. Ловушка: синхронизация упала с <code>namespaces "…" not found</code>, хотя в приложении стоит <code>CreateNamespace=true</code>. Почему?</summary>

Ручная операция использует свои `syncOptions`: опцию нужно указать и в ней.

→ [Подвох 2](#2-операция-не-наследует-syncoptions)

</details>

<details><summary>7. Что произойдёт с ресурсами при удалении <code>Application</code>?</summary>

Без финализатора остаются; с `resources-finalizer.argocd.argoproj.io` удаляются.

→ [Продвинутая теория](advanced.ru.md#удаление-application)

</details>

<details><summary>8. Как Argo CD работает с чартами Helm?</summary>

Repo-server выполняет `helm template` с заданными параметрами, контроллер применяет результат; релиз Helm в кластере не создаётся.

→ [Продвинутая теория](advanced.ru.md#helm-и-kustomize-как-источник)

</details>

<details><summary>9. Какие компоненты входят в Argo CD и зачем нужен repo-server?</summary>

Application controller, repo-server, server, redis, dex, applicationset и notifications. Repo-server клонирует репозитории и рендерит манифесты.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>10. Почему нельзя хранить Secret в Git «как есть»?</summary>

Значения в Secret лишь кодируются base64, а Git хранит историю навсегда. Нужны шифрование (Sealed Secrets, SOPS) или внешние хранилища.

→ [Подвох 7](#7-секреты-в-git)

</details>

<details><summary>11. Что такое дрейф и как его видно?</summary>

Расхождение кластера и Git. Приложение получает `OutOfSync`, а `status.resources` показывает, какие объекты отличаются.

→ [Продвинутая теория](advanced.ru.md#дрейф-и-self-heal)

</details>

<details><summary>12. Чем Argo CD отличается от Flux?</summary>

Оба тянут Git в кластер; Argo CD даёт интерфейс и `Application`/`ApplicationSet`, Flux — набор небольших контроллеров. Подробнее в документации (Flux в этом материале не запускался).

→ [Компромиссы и альтернативы](#компромиссы-и-альтернативы)

</details>

## Шпаргалка

```text
kubectl -n argocd get app
kubectl -n argocd patch app A --type=merge -p '{"operation":{"sync":{"syncOptions":["CreateNamespace=true"]}}}'
kubectl -n argocd patch app A --type=merge -p '{"spec":{"syncPolicy":{"automated":{"selfHeal":true}}}}'
kubectl -n argocd get app A -o jsonpath='{.status.operationState.message}'
```

- Git — истина; контроллер в кластере тянет и сверяет.
- Sync и Health — независимые оси.
- `automated`: `prune` (удаление), `selfHeal` (откат ручных правок).
- Каскадное удаление — финализатор; секреты — не открытым текстом.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- CRD и операторы: [19 · CRD и операторы](../19-crd-and-operators/expert.ru.md)
- Диагностика: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
- Helm и Kustomize: [13 · Helm](../13-helm/expert.ru.md), [14 · Kustomize](../14-kustomize/expert.ru.md)

## Источники

- [Argo CD: Architectural Overview](https://argo-cd.readthedocs.io/en/stable/operator-manual/architecture/)
- [Argo CD: Resource Health](https://argo-cd.readthedocs.io/en/stable/operator-manual/health/)
- [Argo CD: Projects](https://argo-cd.readthedocs.io/en/stable/user-guide/projects/)
- [Argo CD: ApplicationSet](https://argo-cd.readthedocs.io/en/stable/operator-manual/applicationset/)
- [Flux: Core Concepts](https://fluxcd.io/flux/concepts/)
