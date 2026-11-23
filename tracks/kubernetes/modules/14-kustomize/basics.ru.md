---
title: "Kustomize: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 14-kustomize
part: 2
tags: [kustomize, overlay, base, namespace, name-prefix, images, replicas]
versions: "Kustomize 5.8.1 (в составе kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Deployment](../05-deployments/overview.ru.md), [03 · kubectl](../03-local-cluster-and-kubectl/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0) командами `kubectl kustomize` и `kubectl apply -k` (Kustomize 5.8.1 в составе kubectl 1.36.1). Отдельно устанавливать ничего не нужно.

## Зачем это нужно

Один сервис разворачивается в dev и prod: в prod больше реплик, свежий образ, лимиты и другая конфигурация. Копировать манифесты опасно: правки приходится вносить везде. Kustomize хранит общую часть один раз (**base**) и описывает различия окружений отдельно (**overlays**), оставляя исходные файлы обычным YAML.

## Структура

```text
app/
├── base/
│   ├── deployment.yaml
│   ├── service.yaml
│   └── kustomization.yaml
└── overlays/
    ├── dev/kustomization.yaml
    └── prod/
        ├── kustomization.yaml
        └── resources-patch.yaml
```

`base/kustomization.yaml` просто перечисляет файлы:

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
resources:
  - deployment.yaml
  - service.yaml
```

`base/deployment.yaml` — обычный Deployment (`web`, `nginx:1.28`, 1 реплика), `service.yaml` — Service на тот же `app: web`.

## Overlay

Overlay подключает base через `resources` и добавляет преобразования. Для dev:

```yaml
# overlays/dev/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: learn-14-dev
namePrefix: dev-
labels:
  - pairs: {env: dev}
    includeSelectors: true
resources:
  - ../../base
```

```text
$ kubectl kustomize overlays/dev
apiVersion: v1
kind: Service
metadata:
  labels:
    env: dev
  name: dev-web
  namespace: learn-14-dev
spec:
  ports:
  - port: 80
  selector:
    app: web
    env: dev
---
apiVersion: apps/v1
kind: Deployment
metadata:
  labels:
    env: dev
  name: dev-web
  namespace: learn-14-dev
spec:
  replicas: 1
  selector:
    matchLabels:
      app: web
      env: dev
…
```

Результат: имена с префиксом `dev-`, namespace подставлен, метка `env: dev` добавлена и в `selector` (из-за `includeSelectors: true`). Исходные файлы base не изменились.

Для prod меняем реплики и образ:

```yaml
# overlays/prod/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: learn-14-prod
namePrefix: prod-
labels:
  - pairs: {env: prod}
resources:
  - ../../base
replicas:
  - name: web
    count: 3
images:
  - name: nginx
    newTag: "1.29"
```

```text
$ kubectl kustomize overlays/prod | grep -E "replicas|image:"
  replicas: 3
      - image: nginx:1.29
```

Здесь `labels` без `includeSelectors`: метка `env: prod` попала на объекты, но не в селекторы (в Service `selector` остался `app: web`).

## Применение

Команды те же, что для обычных манифестов, но с флагом `-k` и папкой:

```text
$ kubectl apply -k overlays/prod
configmap/prod-app-config-dfk4bdbtkk created
service/prod-web created
deployment.apps/prod-web created
$ kubectl get -k overlays/prod
NAME                                   DATA   AGE
configmap/prod-app-config-47668c6k28   1      35s
…
$ kubectl delete -k overlays/prod
configmap "prod-app-config-47668c6k28" deleted from learn-14-prod namespace
service "prod-web" deleted from learn-14-prod namespace
deployment.apps "prod-web" deleted from learn-14-prod namespace
```

Namespace из overlay должен существовать заранее: Kustomize его не создаёт.

```text
$ kubectl apply -k overlays/nons          # namespace: learn-14-missing
Error from server (NotFound): error when creating "overlays/nons": namespaces "learn-14-missing" not found
```

Чтобы namespace создавался вместе с приложением, добавьте манифест Namespace в `resources`.

## Основные трансформеры

| Поле | Что делает |
|---|---|
| `namespace` | выставляет namespace всем объектам |
| `namePrefix`, `nameSuffix` | добавляет префикс или суффикс к именам и обновляет ссылки |
| `labels` | добавляет метки (с `includeSelectors: true` — и в селекторы) |
| `images` | меняет образ (`newName`, `newTag`) по имени |
| `replicas` | задаёт число реплик по имени ресурса |
| `patches` | правит поля (подробнее в [продвинутой теории](advanced.ru.md#патчи)) |
| `configMapGenerator`, `secretGenerator` | создают ConfigMap и Secret |

## Ошибки новичка

- ❌ Копировать base в каждый overlay → ✅ в overlay только различия.
- ❌ Применять папку через `kubectl apply -f` → ✅ `kubectl apply -k`.
- ❌ Ждать, что namespace создастся сам → ✅ создать заранее или включить Namespace в `resources`.
- ❌ Использовать устаревшие `commonLabels` → ✅ `labels` (Kustomize предупреждает `'commonLabels' is deprecated`).
- ❌ Не смотреть результат перед применением → ✅ `kubectl kustomize` или `kubectl diff -k`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — патчи, генераторы, replacements, компоненты
- Предыдущий модуль: [13 · Helm](../13-helm/overview.ru.md)
- Deployment и образы: [05 · Deployment](../05-deployments/overview.ru.md)
- Конфигурация: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)

## Источники

- [Kubernetes: Declarative Management using Kustomize](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/kustomization/)
- [Kustomize: kustomization.yaml reference](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/)
- [Kustomize: labels](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/labels/)
- [Kustomize: images](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/images/)
- [Kustomize: replicas](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/replicas/)
