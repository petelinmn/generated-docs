---
title: "CRD и операторы"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 19-crd-and-operators
part: 3
levels: [1, 2, 3, 4]
tags: [crd, custom-resource, operator, controller, reconcile, cel, status-subresource, owner-reference, cert-manager]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, cert-manager v1.21.3, Helm 4.3"
status: draft
updated: 2026-11-23
---

# CRD и операторы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Kubernetes можно расширять собственными типами объектов. **CustomResourceDefinition (CRD)** описывает новый тип (`Widget`, `Certificate`, `Application`): имя, схему полей и проверки. После создания CRD API-сервер сам хранит такие объекты и отдаёт их через `kubectl`, как встроенные. Сам по себе объект ничего не делает — это только данные. Чтобы они что-то значили, нужен **контроллер**, который сравнивает желаемое состояние (поле `spec`) с реальным и приводит мир в соответствие. Пара «CRD + контроллер» называется **оператором (operator)**.

## Ключевые идеи

- **CRD — это схема, а не код.** Она задаёт группу, имена, версии, схему OpenAPI v3, значения по умолчанию и проверки.
- **API-сервер проверяет объекты по схеме:** типы, `required`, `enum`, `minimum`/`maximum`, правила CEL (`x-kubernetes-validations`).
- **Неизвестные поля не сохраняются:** `kubectl` отклоняет их при строгой проверке, а без неё они молча отбрасываются (pruning).
- **`status` — отдельный subresource:** обычный `patch` ресурса его не меняет, контроллер пишет статус через `/status`.
- **Контроллер работает по циклу сверки (reconcile):** «прочитать желаемое → привести фактическое → записать статус», идемпотентно.
- **`ownerReferences` связывают объекты:** при удалении владельца сборщик мусора удаляет зависимые объекты.
- **Контроллеру нужны права RBAC** на свои объекты, их `status` и на всё, чем он управляет.
- **Готовые операторы устанавливаются как приложения** (чарт Helm, манифест): cert-manager, Argo CD и другие.

## Минимальный пример

```yaml
apiVersion: apiextensions.k8s.io/v1
kind: CustomResourceDefinition
metadata: {name: widgets.example.com}
spec:
  group: example.com
  scope: Namespaced
  names: {kind: Widget, plural: widgets, singular: widget, shortNames: [wg]}
  versions:
  - name: v1
    served: true
    storage: true
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
```

```text
$ kubectl -n learn-19 apply -f w1.yaml     # spec: {image: nginx:1.29}
widget.example.com/w1 created
$ kubectl -n learn-19 get widget w1 -o jsonpath='{.spec}'
{"image":"nginx:1.29","replicas":1}
```

Поле `replicas` не было указано и получило значение по умолчанию из схемы.

## Когда применять, а когда нет

| Задача | Подход |
|---|---|
| Описать собственный объект предметной области декларативно | CRD + контроллер |
| Автоматизировать эксплуатацию сложного приложения (БД, сертификаты) | оператор |
| Хранить настройки приложения | ConfigMap ([модуль 07](../07-config-and-secrets/overview.ru.md)) |
| Разовая автоматизация | Job или скрипт |
| Проверка и запрет объектов | ValidatingAdmissionPolicy ([модуль 17](../17-security/advanced.ru.md#validatingadmissionpolicy-свои-правила)) |

- ❌ Не создавайте CRD «на всякий случай»: это новая часть API, которую придётся версионировать.
- ❌ Не пишите оператор там, где хватит Helm-чарта и Deployment.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | CRD и объект, схема, значения по умолчанию, проверки, `kubectl explain`, status subresource |
| [2. Продвинутая теория](advanced.ru.md) | CEL-правила, pruning, status, ownerReferences, игрушечный оператор с RBAC, cert-manager |
| [3. Экспертная теория](expert.ru.md) | справочник полей CRD, устройство контроллеров, версии, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 19-crd-and-operators level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 19-crd-and-operators level 6` |

## Связанные материалы

- Пререквизиты: [02 · Архитектура](../02-architecture/overview.ru.md) (контроллеры и API-сервер), [05 · Deployment](../05-deployments/overview.ru.md), [10 · Namespaces и RBAC](../10-namespaces-and-rbac/overview.ru.md)
- Установка операторов: [13 · Helm](../13-helm/overview.ru.md)
- Проверка объектов: [17 · Безопасность](../17-security/overview.ru.md)
- Дальше: [20 · GitOps](../20-gitops/overview.ru.md) (Argo CD сам является оператором со своими CRD)
- Аналоги в других треках: [C# фоновые задачи](../../../csharp/recipes/background-jobs/overview.ru.md) — цикл «наблюдать и сверять» в обычном приложении
