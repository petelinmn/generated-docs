---
title: "Kustomize: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 14-kustomize
part: 2
tags: [kustomize, kustomization, patches, generators, replacements, components, load-restrictor]
versions: "Kustomize 5.8.1 (в составе kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что проверено запуском в Kustomize 5.8.1 — помечено «в опыте».

## Справочник полей `kustomization.yaml`

| Поле | Смысл | Статус в этом материале |
|---|---|---|
| `resources` | файлы, папки и URL, которые собираются | проверено (файлы, папка с `kustomization.yaml`) |
| `components` | подключаемые `Component` | проверено |
| `namespace` | namespace для всех объектов | проверено |
| `namePrefix`, `nameSuffix` | префикс и суффикс имён | `namePrefix` проверен |
| `labels` | метки с `pairs` и `includeSelectors` | проверено |
| `commonLabels` | устаревшее: добавляет метки и в селекторы | предупреждение `'commonLabels' is deprecated. Please use 'labels' instead` |
| `images` | `name`, `newName`, `newTag`, `digest` | `newTag` проверен |
| `replicas` | `name` и `count` | проверено |
| `patches` | `path` или `patch` и `target` | проверено |
| `configMapGenerator`, `secretGenerator` | `literals`, `files`, `envs`, `behavior` | `literals` проверен |
| `generatorOptions` | `disableNameSuffixHash`, `labels`, `annotations` | `disableNameSuffixHash` проверен |
| `replacements` | копирование значений между полями | проверено |
| `helmCharts` | подключение Helm-чартов (нужен `--enable-helm`) | не проверялось |
| `commonAnnotations`, `crds`, `configurations`, `openapi`, `sortOptions`, `buildMetadata`, `vars` | по документации | не проверялось |

> ⚠️ Verify: назначение и синтаксис полей, помеченных «не проверялось», взяты из документации Kustomize; на них не ставились опыты. Поле `vars` и отдельные `patchesStrategicMerge`/`patchesJson6902` помечены в документации устаревшими в пользу `replacements` и `patches`.

### Команды kubectl

| Команда | Назначение |
|---|---|
| `kubectl kustomize <dir>` | напечатать результат сборки |
| `kubectl apply -k <dir>` | применить результат |
| `kubectl diff -k <dir>` | показать разницу с кластером |
| `kubectl get -k <dir>`, `kubectl delete -k <dir>` | объекты из результата |

Флаги `kubectl kustomize` (из справки): `--enable-helm` (по умолчанию `false`), `--enable-alpha-plugins` (`false`), `--load-restrictor` (по умолчанию `LoadRestrictionsRootOnly`), `--as-current-user`.

## Внутреннее устройство

- **Сборка — чистая функция.** `kubectl kustomize` читает файлы, применяет преобразования и печатает YAML; кластер для этого не нужен. Исходные файлы не меняются.
- **Накопление ресурсов.** `resources` собираются в общий набор, где каждый объект имеет идентификатор (группа, версия, kind, имя, namespace). Дубликат запрещён: `may not add resource with an already registered id: Deployment.v1.apps/web.[noNs]`.
- **Ссылки между объектами (name reference).** После `namePrefix` и хеширования генераторов Kustomize обновляет ссылки: имя ConfigMap в `envFrom` стало `prod-app-config-dfk4bdbtkk` (в опыте).
- **Хеш содержимого.** Суффикс считается от данных ConfigMap: `LOG_LEVEL=warn` → `dfk4bdbtkk`, `debug` → `47668c6k28`. Смена данных — новое имя — новый шаблон пода — rolling update.
- **Порядок вывода.** В опыте объекты шли в порядке ConfigMap, Secret, Service, Deployment (не в порядке файлов в `resources`): Kustomize сортирует по типам.
- **`apply -k` — обычный `apply`.** Он создаёт и обновляет объекты, но не удаляет те, которых больше нет в результате (старый ConfigMap остался). Удаление устаревших объектов даёт prune или GitOps ([модуль 20](../../overview.ru.md) дорожной карты).
- **Патч по `target`.** Выбор цели идёт по `kind`, `name` и другим полям `target`; имя в теле strategic-merge патча при наличии `target` не определяет цель (в опыте `ignored`).

## Подвохи

### 1. `commonLabels` меняет селекторы

В опыте `commonLabels: {team: a}` дал пять вхождений метки: в метки объектов, в `selector` Service, в `selector.matchLabels` и метки шаблона Deployment. Селектор Deployment неизменяем, поэтому добавлять `commonLabels` к живому объекту нельзя. Используйте `labels` без `includeSelectors`.

### 2. Хеш не защищает от забытых ConfigMap

После `apply -k` с новой конфигурацией остаются оба ConfigMap (`dfk4bdbtkk` и `47668c6k28`), после `delete -k` осталась старая версия: её не было в сборке.

### 3. Патч молча не применяется? Нет — падает

Патч на несуществующий объект не игнорируется: `no resource matches strategic merge patch "Deployment.v1.apps/ghost.[noNs]"`. Сообщение указывает идентификатор объекта.

### 4. Namespace не создаётся

`namespace: learn-14-missing` без существующего namespace — `namespaces "learn-14-missing" not found` на каждом объекте.

### 5. Выход за пределы папки

Файл `../../app/base/service.yaml` в `resources` отвергается (`security; file … is not in or below …`): ссылайтесь на папки, а не на файлы в чужих.

### 6. `disableNameSuffixHash` отключает перезапуск

Без хеша смена ConfigMap не меняет шаблон пода (имя то же), и поды не перезапускаются ([модуль 07](../07-config-and-secrets/advanced.ru.md#изменение-configmap-не-перезапускает-поды)).

### 7. Secret из генератора — это base64

`password: czNjcmV0` декодируется тривиально; значение лежит в `kustomization.yaml`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Kustomize | чистый YAML, встроен в kubectl, нет шаблонов | нет релизов, истории, пакетов; сложные случаи — через патчи |
| [Helm](../13-helm/overview.ru.md) | параметры, история, готовые чарты | шаблоны, сложнее читать |
| Копии манифестов | просто | расхождение окружений |
| Helm + Kustomize | правки чужих чартов без форка | два инструмента (`--enable-helm`) |

## Вопросы с собеседований

<details><summary>1. Что такое base и overlay?</summary>

Base — общий набор манифестов; overlay — папка с `kustomization.yaml`, которая подключает base и применяет различия окружения.

→ [Базовая теория](basics.ru.md#overlay)

</details>

<details><summary>2. Чем <code>kubectl kustomize</code> отличается от <code>kubectl apply -k</code>?</summary>

Первая печатает результат, не обращаясь к кластеру; вторая применяет его.

→ [Справочник](#команды-kubectl)

</details>

<details><summary>3. Зачем суффикс-хеш у ConfigMap из генератора?</summary>

Смена данных даёт новое имя, а значит новый шаблон пода и rolling update; без хеша поды не перезапускаются.

→ [Продвинутая теория](advanced.ru.md#генераторы-configmap-и-secret)

</details>

<details><summary>4. Что произойдёт со старым ConfigMap после изменения данных и <code>apply -k</code>?</summary>

Останется: `apply -k` не удаляет объекты, исчезнувшие из результата.

→ [Подвох 2](#2-хеш-не-защищает-от-забытых-configmap)

</details>

<details><summary>5. Какие два формата патчей поддерживает Kustomize?</summary>

Strategic merge (фрагмент манифеста) и JSON 6902 (список операций `op/path/value`).

→ [Продвинутая теория](advanced.ru.md#патчи)

</details>

<details><summary>6. Как патч выбирает объект?</summary>

По `target` (`kind`, `name`, …); без `target` — по `kind` и имени из тела strategic-merge патча. Нет совпадения — ошибка.

→ [Продвинутая теория](advanced.ru.md#ошибки-патчей)

</details>

<details><summary>7. Ловушка: почему нельзя добавлять <code>commonLabels</code> к существующему Deployment?</summary>

Они попадают и в селектор, а `spec.selector` Deployment неизменяем.

→ [Подвох 1](#1-commonlabels-меняет-селекторы)

</details>

<details><summary>8. Создаёт ли Kustomize namespace из поля <code>namespace</code>?</summary>

Нет, только подставляет его в объекты; namespace нужно создать (или включить манифест в `resources`).

→ [Подвох 4](#4-namespace-не-создаётся)

</details>

<details><summary>9. Что такое компонент и чем он отличается от base?</summary>

`Component` — переиспользуемое преобразование (`apiVersion …/v1alpha1`), подключаемое через `components`, а не набор ресурсов для сборки.

→ [Продвинутая теория](advanced.ru.md#компоненты)

</details>

<details><summary>10. Что делает <code>replacements</code>?</summary>

Копирует значение из поля одного объекта в поля других (в опыте порт Service → переменная Deployment).

→ [Продвинутая теория](advanced.ru.md#replacements)

</details>

<details><summary>11. Почему Kustomize не читает файл вне своей папки?</summary>

Защита `LoadRestrictionsRootOnly` (значение по умолчанию флага `--load-restrictor`).

→ [Подвох 5](#5-выход-за-пределы-папки)

</details>

<details><summary>12. Когда выбрать Kustomize, а когда Helm?</summary>

Kustomize — для своих манифестов с небольшими различиями окружений; Helm — для параметризации, релизов и сторонних чартов.

→ [Компромиссы](#компромиссы-и-альтернативы)

</details>

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Самопроверка: [квиз](quiz.ru.md), [упражнения](exercises.ru.md)
- Helm: [13 · Helm](../13-helm/expert.ru.md)
- Конфигурация: [07 · ConfigMap и Secret](../07-config-and-secrets/expert.ru.md)

## Источники

- [Kustomize: Reference](https://kubectl.docs.kubernetes.io/references/kustomize/)
- [Kustomize: kustomization.yaml](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/)
- [Kubernetes: Declarative Management using Kustomize](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/kustomization/)
- [kubectl: kustomize](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_kustomize/)
