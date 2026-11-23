---
title: "CRD и операторы: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 19-crd-and-operators
part: 3
tags: [crd, custom-resource, openapi-schema, validation, defaults, status-subresource, kubectl-explain]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# CRD и операторы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Архитектура](../02-architecture/overview.ru.md), [05 · Deployment](../05-deployments/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-19`. Объект предметной области — `Widget`: «виджет», который в [продвинутой теории](advanced.ru.md#игрушечный-оператор) превратится в Deployment.

## Зачем это нужно

Встроенных типов (Pod, Service, Deployment) не хватает, когда нужно говорить на языке своей задачи: «сертификат», «база данных», «приложение в Git». CRD добавляет такой тип в API кластера, и с ним работают привычные `kubectl get/apply/delete`, RBAC, `describe`, `--watch`.

## Создаём CRD

```yaml
apiVersion: apiextensions.k8s.io/v1
kind: CustomResourceDefinition
metadata: {name: widgets.example.com}      # <plural>.<group>
spec:
  group: example.com
  scope: Namespaced
  names: {kind: Widget, listKind: WidgetList, plural: widgets, singular: widget, shortNames: [wg]}
  versions:
  - name: v1
    served: true
    storage: true
    subresources: {status: {}}
    additionalPrinterColumns:
    - {name: Replicas, type: integer, jsonPath: .spec.replicas}
    - {name: Phase, type: string, jsonPath: .status.phase}
    schema:
      openAPIV3Schema:
        type: object
        properties:
          spec:
            type: object
            required: [image]
            properties:
              image: {type: string}
              replicas: {type: integer, minimum: 1, maximum: 5, default: 1}
              tier: {type: string, enum: [free, pro]}
          status:
            type: object
            properties:
              phase: {type: string}
```

- Имя CRD обязано быть `<plural>.<group>`.
- `scope`: `Namespaced` или `Cluster`.
- `versions[]`: у каждой версии свои `served` (отдаётся ли в API) и `storage` (в какой версии хранится в etcd; ровно одна).
- `additionalPrinterColumns` задаёт колонки `kubectl get`.

```text
$ kubectl get crd widgets.example.com
NAME                  CREATED AT
widgets.example.com   2026-11-23T14:00:10Z
$ kubectl api-resources --api-group=example.com
NAME      SHORTNAMES   APIVERSION       NAMESPACED   KIND
widgets   wg           example.com/v1   true         Widget
```

## Создаём объект

```yaml
apiVersion: example.com/v1
kind: Widget
metadata: {name: w1}
spec: {image: nginx:1.29}
```

```text
$ kubectl -n learn-19 apply -f w1.yaml
widget.example.com/w1 created
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
w1     1          
$ kubectl -n learn-19 get widget w1 -o yaml     # фрагмент
spec:
  image: nginx:1.29
  replicas: 1
```

Колонка `Replicas` взята из `.spec.replicas`, а само значение 1 подставлено API-сервером из `default` схемы: в манифесте его не было. Колонка `Phase` пуста — статуса ещё никто не записал.

## Проверки схемы

API-сервер отклоняет объекты, не подходящие под схему. Все примеры — реальные ответы:

```text
spec: {image: nginx, replicas: 9}
  spec.replicas: Invalid value: 9: spec.replicas in body should be less than or equal to 5

spec: {replicas: 2}
  spec.image: Required value

spec: {image: nginx, tier: gold}
  spec.tier: Unsupported value: "gold": supported values: "free", "pro"

spec: {image: nginx, replicas: "two"}
  spec.replicas: Invalid value: "string": spec.replicas in body must be of type integer: "string"
```

Правила схемы: тип (`type`), обязательность (`required`), диапазон (`minimum`/`maximum`), перечисление (`enum`), значение по умолчанию (`default`). Более гибкие проверки — на языке CEL, см. [продвинутую теорию](advanced.ru.md#правила-cel).

## `kubectl explain`: документация из схемы

Схема CRD используется и для подсказок:

```text
$ kubectl -n learn-19 explain widget.spec
GROUP:      example.com
KIND:       Widget
VERSION:    v1

FIELD: spec <Object>

FIELDS:
  image	<string> -required-
  replicas	<integer>
  …
```

Если в схеме добавить `description`, пояснения появятся и здесь (в опыте описаний не было: `<no description>`).

## Status subresource

В CRD включён `subresources: {status: {}}`. Теперь у `status` особый статус: его меняют не обычными изменениями ресурса, а через `/status`. Об этом подробнее в [продвинутой теории](advanced.ru.md#status-subresource).

## Ошибки новичка

- ❌ Имя CRD не по форме `<plural>.<group>` → ✅ по документации оно обязано совпадать с `names.plural` и `group` (запуском не проверялось).
- ❌ Ждать, что объект CRD «сам что-то сделает» → ✅ нужен контроллер.
- ❌ Забыть `required` и значения по умолчанию в схеме → ✅ схема — это контракт.
- ❌ Положить в манифест поле, которого нет в схеме → ✅ см. pruning.
- ❌ Менять схему несовместимо у `v1` → ✅ добавлять новую версию (см. [экспертную теорию](expert.ru.md)).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — CEL, pruning, игрушечный оператор, cert-manager
- Контроллеры и API-сервер: [02 · Архитектура](../02-architecture/overview.ru.md)
- Namespace и RBAC: [10 · Namespaces и RBAC](../10-namespaces-and-rbac/overview.ru.md)

## Источники

- [Custom Resources](https://kubernetes.io/docs/concepts/extend-kubernetes/api-extension/custom-resources/)
- [Extend the Kubernetes API with CustomResourceDefinitions](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definitions/)
- [Operator pattern](https://kubernetes.io/docs/concepts/extend-kubernetes/operator/)
