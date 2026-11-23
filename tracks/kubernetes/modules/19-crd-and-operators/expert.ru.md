---
title: "CRD и операторы: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 19-crd-and-operators
part: 3
tags: [crd, operator, controller, reconcile, informer, finalizer, owner-reference, cel, conversion-webhook, controller-runtime]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, cert-manager v1.21.3"
status: draft
updated: 2026-11-23
---

# CRD и операторы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что подтверждено опытом в kind (Kubernetes 1.37.0) — помечено «в опыте».

## Справочник

### Поля CustomResourceDefinition

| Поле | Смысл |
|---|---|
| `metadata.name` | `<plural>.<group>` (в опыте `widgets.example.com`) |
| `spec.group` | API-группа |
| `spec.scope` | `Namespaced` или `Cluster` |
| `spec.names` | `kind`, `listKind`, `plural`, `singular`, `shortNames`, `categories` |
| `spec.versions[]` | `name`, `served`, `storage`, `schema.openAPIV3Schema`, `subresources`, `additionalPrinterColumns`, `deprecated` |
| `subresources.status` | отдельный `/status` (в опыте) |
| `subresources.scale` | интеграция с `kubectl scale` и HPA |
| `spec.conversion` | стратегия преобразования между версиями: `None` или `Webhook` |
| `spec.preserveUnknownFields` | устаревший режим без pruning |

> ⚠️ Проверить: `categories`, `subresources.scale`, `deprecated`, `spec.conversion` (в том числе `Webhook`), `preserveUnknownFields`, `selectableFields` взяты из документации и не запускались. В опыте проверены `group`, `scope`, `names` (`shortNames`), одна версия, `status`, `additionalPrinterColumns`, схема, `default`, `x-kubernetes-validations`.

### Ключевые слова схемы (проверены в опыте)

| Слово | Действие |
|---|---|
| `type` | тип (`string`, `integer`, `object`): ошибка `must be of type integer` |
| `required` | обязательные поля: `Required value` |
| `enum` | допустимые значения: `Unsupported value … supported values` |
| `minimum`, `maximum` | границы: `should be less than or equal to 5` |
| `default` | значение по умолчанию подставляется сервером |
| `x-kubernetes-validations` | правила CEL с `rule` и `message` |

> ⚠️ Проверить: `pattern`, `format`, `x-kubernetes-preserve-unknown-fields`, `x-kubernetes-list-type`, `nullable`, `oneOf`/`anyOf` — по документации, запуском не проверялись.

### Команды

| Команда | Назначение |
|---|---|
| `kubectl get crd` | список CRD |
| `kubectl api-resources --api-group=<группа>` | типы группы, короткие имена |
| `kubectl explain <kind>.spec` | документация из схемы |
| `kubectl patch <obj> --subresource=status` | запись `status` |
| `kubectl apply --validate=false` | отключить строгую проверку клиента (поля всё равно отбрасываются) |

## Внутреннее устройство

- **CRD — часть API-сервера.** После создания CRD API-сервер начинает обслуживать новый путь (`/apis/example.com/v1/…`), хранит объекты в etcd и проверяет их по схеме. Отдельного процесса для этого нет.
- **Контроллер — отдельное приложение.** Он обычно запущен подом и работает через API: читает и наблюдает объекты, создаёт зависимые. В опыте это Deployment `operator`; у cert-manager — три Deployment (контроллер, cainjector, webhook).
- **Цикл сверки.** Контроллер получает ключ объекта, читает желаемое (`spec`) и фактическое состояние, выполняет минимальные действия и записывает `status`. Цикл «уровневый» (level-triggered): важно текущее состояние, а не событие, поэтому пропущенное событие не страшно. В опыте оператор исправил ручной `scale` без каких-либо событий об этом.
- **Informer и очередь.** Библиотеки (client-go, controller-runtime) держат локальный кэш объектов через watch и ставят изменённые ключи в очередь с повторами. Игрушечный оператор в опыте просто опрашивает API раз в 3 секунды.
- **Владение.** `ownerReferences` обеспечивают сборку мусора: при удалении владельца зависимые удаляются (в опыте). `blockOwnerDeletion` и foreground-удаление ждут зависимых.
- **Финализаторы.** Если у объекта есть `metadata.finalizers`, удаление лишь ставит `deletionTimestamp`, и объект остаётся, пока контроллер не выполнит очистку и не снимет финализатор (механизм показан в [модуле 21](../21-troubleshooting/advanced.ru.md#объект-завис-в-terminating)).
- **Admission для CRD.** Объекты проверяются схемой и правилами CEL внутри API-сервера; сложные проверки и мутации выносят в webhook или ValidatingAdmissionPolicy ([модуль 17](../17-security/overview.ru.md)).
- **Версии.** В etcd объект хранится в одной версии (`storage`), остальные отдаются через конверсию: `None` просто меняет `apiVersion`, `Webhook` вызывает внешний сервис.

> ⚠️ Проверить: выбор лидера (leader election) и повторные попытки с экспоненциальной задержкой в controller-runtime, `kubebuilder`, `operator-sdk` и конверсия версий в опытах не запускались.

## Подвохи

### 1. CEL-правило без `has()`

Для необязательного поля правило падает с `no such key: tier`, отклоняя даже безобидные изменения (в опыте). Нужно `!has(self.tier) || …`.

### 2. Обычный `patch` не меняет `status`

С включённым subresource `status` вернётся `patched (no change)` (в опыте). Нужен `--subresource=status`.

### 3. RBAC на `status` и `get`

Для `kubectl patch --subresource=status` нужны права и на чтение: оператор получил `Forbidden: cannot get resource "widgets/status"` (в опыте).

### 4. Pruning молча удаляет неизвестные поля

С `--validate=false` поле `color: red` исчезло, ошибки нет (в опыте). Данные теряются, пока поле не описано в схеме.

### 5. `default` применяется сервером

Значения по умолчанию подставляются при записи и чтении. Если убрать их из схемы, у старых объектов поля могут отсутствовать. Правила и код должны учитывать отсутствие поля.

### 6. Удаление CRD удаляет все объекты

После `kubectl delete crd` тип исчез: `the server could not find the requested resource` (в опыте). По документации вместе с типом удаляются и все объекты, поэтому CRD нельзя удалять без резервной копии.

### 7. Установка и обновление CRD вне чарта

Helm не обновляет каталог `crds/` при `helm upgrade`; cert-manager использует `crds.enabled=true` и ставит CRD как часть чарта. Это общий вопрос жизненного цикла CRD.

### 8. Опрос вместо watch

Игрушечный оператор создаёт нагрузку на API-сервер раз в 3 секунды на все объекты. Настоящие контроллеры используют watch и кэш.

> ⚠️ Проверить: подвохи 6 (удаление объектов вместе с CRD) и 7 (поведение Helm с `crds/`) взяты из документации и не воспроизводились.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| CRD + оператор | декларативно, нативно для Kubernetes, RBAC и `kubectl` из коробки | код, версионирование, эксплуатация |
| Helm-чарт | просто, без кода | нет постоянной сверки и логики «дня 2» |
| Скрипт/CronJob | быстро | нет декларативного API и статуса |
| Aggregated API server | полный контроль над API | сложно; не запускался |
| Готовый оператор | экономит труд | зависимость, нужно понимать CRD |
| Фреймворки (kubebuilder, operator-sdk, Metacontroller) | шаблон и генерация | не запускались |

## Вопросы с собеседований

<details><summary>1. Что такое CRD и чем он отличается от оператора?</summary>

CRD добавляет в API новый тип с хранением и проверкой. Оператор — CRD плюс контроллер, который делает с этими объектами реальную работу.

→ [Обзор](overview.ru.md#коротко)

</details>

<details><summary>2. Как API-сервер проверяет объекты CRD?</summary>

По схеме OpenAPI v3 (`type`, `required`, `enum`, границы), значения по умолчанию подставляются, дополнительные правила — CEL (`x-kubernetes-validations`).

→ [Базовая теория](basics.ru.md#проверки-схемы)

</details>

<details><summary>3. Ловушка: CEL-правило отклоняет объект без поля, хотя поле необязательное. Почему?</summary>

Обращение к отсутствующему полю — ошибка (`no such key`). Нужен `has()`.

→ [Подвох 1](#1-cel-правило-без-has)

</details>

<details><summary>4. Зачем status вынесен в subresource?</summary>

Чтобы пользователь и контроллер писали в разные части объекта и права выдавались раздельно (`widgets/status`).

→ [Продвинутая теория](advanced.ru.md#status-subresource)

</details>

<details><summary>5. Что значит «уровневый» цикл сверки?</summary>

Контроллер каждый раз сравнивает текущее и желаемое состояние, а не обрабатывает событие. Пропущенное событие ничего не ломает, и повтор безопасен (идемпотентность).

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>6. Ловушка: <code>kubectl patch</code> status прошёл, но ничего не изменилось. Почему?</summary>

Статус — subresource: нужен `--subresource=status`.

→ [Подвох 2](#2-обычный-patch-не-меняет-status)

</details>

<details><summary>7. Что делают ownerReferences?</summary>

Связывают зависимый объект с владельцем: при удалении владельца сборщик мусора удаляет зависимый.

→ [Продвинутая теория](advanced.ru.md#ownerreferences-и-сборка-мусора)

</details>

<details><summary>8. Зачем нужны финализаторы?</summary>

Блокируют окончательное удаление, пока контроллер не выполнит очистку (внешние ресурсы).

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>9. Что произойдёт при удалении CRD?</summary>

Тип пропадёт из API, а вместе с ним удалятся и все объекты этого типа.

→ [Подвох 6](#6-удаление-crd-удаляет-все-объекты)

</details>

<details><summary>10. Какие права нужны оператору?</summary>

На свои ресурсы (`get/list/watch`), на `<ресурс>/status` (включая `get` для `kubectl patch`) и на управляемые объекты (Deployment и т. д.).

→ [Продвинутая теория](advanced.ru.md#игрушечный-оператор)

</details>

<details><summary>11. Куда пропало поле, которого нет в схеме?</summary>

Оно отброшено при pruning: без `--validate=false` `kubectl` отклонит запрос строго, с ним сервер молча удалит поле.

→ [Подвох 4](#4-pruning-молча-удаляет-неизвестные-поля)

</details>

<details><summary>12. Как работает cert-manager на уровне объектов Kubernetes?</summary>

CRD `Issuer` и `Certificate`; контроллер создаёт `CertificateRequest`, получает сертификат и сохраняет его в Secret `kubernetes.io/tls`; удалённый Secret создаётся заново.

→ [Продвинутая теория](advanced.ru.md#настоящий-оператор-cert-manager)

</details>

## Шпаргалка

```text
kubectl get crd
kubectl api-resources --api-group=example.com
kubectl explain widget.spec
kubectl patch widget NAME --subresource=status --type=merge -p '{"status":{…}}'
```

- CRD = схема, оператор = CRD + контроллер.
- Схема: `required`, `enum`, границы, `default`; CEL с `has()`.
- `status` через subresource и RBAC `<ресурс>/status`.
- `ownerReferences` — сборка мусора; финализаторы — очистка.
- Удаление CRD удаляет объекты.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Архитектура контроллеров: [02 · Архитектура](../02-architecture/expert.ru.md)
- GitOps: [20 · GitOps](../20-gitops/overview.ru.md)
- Диагностика объектов в `Terminating`: [21 · Диагностика](../21-troubleshooting/overview.ru.md)

## Источники

- [Custom Resources](https://kubernetes.io/docs/concepts/extend-kubernetes/api-extension/custom-resources/)
- [Versions in CustomResourceDefinitions](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definition-versioning/)
- [Operator pattern](https://kubernetes.io/docs/concepts/extend-kubernetes/operator/)
- [Finalizers](https://kubernetes.io/docs/concepts/overview/working-with-objects/finalizers/)
- [Owners and Dependents](https://kubernetes.io/docs/concepts/overview/working-with-objects/owners-dependents/)
