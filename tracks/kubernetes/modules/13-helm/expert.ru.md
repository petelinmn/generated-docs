---
title: "Helm: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 13-helm
part: 2
tags: [helm, chart, release, values, hooks, server-side-apply, helm-history, release-secret]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Значения флагов ниже взяты из `helm <команда> --help` версии 4.3 и опытов в kind.

## Справочник

### Команды

| Команда | Назначение |
|---|---|
| `helm create` | заготовка чарта |
| `helm lint` | проверка чарта |
| `helm template` | рендер манифестов локально, без кластера |
| `helm install` / `upgrade` | установка и обновление (`upgrade --install` делает и то и другое) |
| `helm rollback` | откат на ревизию (создаёт новую ревизию) |
| `helm uninstall` | удаление релиза (`--keep-history` оставляет историю) |
| `helm list` | релизы; фильтры `--deployed`, `--failed`, `--pending`, `--superseded`, `--uninstalled`, `-A` для всех namespace |
| `helm history` | ревизии релиза |
| `helm status` | состояние релиза |
| `helm get values / manifest / hooks / notes` | что было задано и применено |
| `helm test` | запуск тестовых хуков |
| `helm package`, `helm show values`, `helm dependency update` | упаковка, просмотр, зависимости |

### Флаги `install`/`upgrade` (из справки)

| Флаг | Смысл | Умолчание |
|---|---|---|
| `--wait[=watcher\|hookOnly\|legacy]` | ждать готовности ресурсов | `hookOnly` без флага |
| `--wait-for-jobs` | ждать завершения Job | выкл. |
| `--timeout` | время ожидания операции | 5m0s |
| `--rollback-on-failure` (`--atomic`) | откат при неудаче; включает `--wait=watcher` | выкл. |
| `--reuse-values` | взять значения прошлого релиза | выкл. |
| `--reset-then-reuse-values` | значения чарта + прошлого релиза + переопределения | выкл. |
| `--server-side` | `true`, `false`, `auto` | `auto` |
| `--force-conflicts` | при server-side apply перехватывать поля у других менеджеров | выкл. |
| `--dry-run` | `none`, `client`, `server` | `none` |
| `--history-max` | сколько ревизий хранить (0 — без лимита) | 10 |
| `-i`, `--install` | у `upgrade`: установить, если релиза нет | выкл. |
| `--create-namespace` | создать namespace | выкл. |
| `--dependency-update` | обновить зависимости перед установкой | выкл. |
| `--skip-crds` | не ставить CRD при установке через `upgrade --install` | выкл. |
| `--hide-secret` | с `--dry-run` не печатать Secret | выкл. |

### Встроенные объекты шаблона

| Объект | Что содержит (наблюдалось в опытах) |
|---|---|
| `.Values` | итоговые значения |
| `.Release` | `.Name`, `.Namespace`, `.Service` (`Helm`), `.Revision` и др. |
| `.Chart` | поля `Chart.yaml` (`.Name`, `.Version`, `.AppVersion`) |
| `.Capabilities`, `.Template`, `.Files` | версии API кластера, путь шаблона, файлы чарта |

> ⚠️ Verify: `.Release.Revision`, `.Release.IsInstall`/`IsUpgrade`, `.Capabilities`, `.Files` и функции `lookup`, `tpl`, `include` с контекстом берутся из документации и в этом материале не запускались (проверены `.Release.Name`, `.Release.Namespace`, `.Release.Service`, `.Chart.Name`, `.Chart.AppVersion`, `include`, `default`, `quote`, `required`, `toYaml`, `nindent`, `with`, `range`).

### Аннотации хуков

| Аннотация | Значения |
|---|---|
| `helm.sh/hook` | `pre-install`, `pre-upgrade`, `test` (проверены), а также другие типы по документации |
| `helm.sh/hook-weight` | порядок внутри типа |
| `helm.sh/hook-delete-policy` | `before-hook-creation`, `hook-succeeded` (проверены), `hook-failed` |

## Внутреннее устройство

- **Хранение релиза.** Релиз — Secret типа `helm.sh/release.v1` с именем `sh.helm.release.v1.<релиз>.v<ревизия>`. Метки: `name`, `owner=helm`, `status`, `version`, `modifiedAt`. Данные декодируются как base64 → base64 → gzip → JSON:

```text
$ kubectl -n learn-13 get secret sh.helm.release.v1.web.v3 -o jsonpath='{.data.release}' | base64 -d | base64 -d | gunzip | head -c 80
{"name":"web","info":{"first_deployed":"2026-11-23T14:56:02.8478873+04:00","last_deployed":…
$ kubectl -n learn-13 get secret sh.helm.release.v1.web.v3 -o jsonpath='{.metadata.labels}'
{"modifiedAt":"1795431407","name":"web","owner":"helm","status":"deployed","version":"3"}
```

Это означает, что у каждого, кто может читать Secret в namespace, есть доступ к значениям релиза (включая пароли, если они в `values`). Подробнее — [модуль 07](../07-config-and-secrets/expert.ru.md).
- **Порядок установки.** Во `helm template` Service идёт до Deployment: Helm сортирует манифесты по типу ресурса, а не по имени файла.
- **Хуки вне релиза.** Хук-ресурс создаётся и удаляется отдельно от остальных объектов; в опыте Job миграции исчез после успеха.
- **`appVersion` справочный.** Он не влияет на установку; образ берётся из `image.tag` или, если он пуст, из `.Chart.AppVersion` — это решение автора шаблона (`default`).
- **Откат — новая ревизия.** `helm rollback web 1` создал ревизию 3 («Rollback to 1»); число хранимых ревизий ограничено `--history-max` (10).
- **Server-side apply и менеджеры полей.** Поле принадлежит менеджеру, который его задал; `kubectl scale` забрал `.spec.replicas`, поэтому апгрейд упал с конфликтом.

## Подвохи

### 1. `upgrade` теряет `--set` прошлых вызовов

После `--set replicaCount=4` и затем `--set image.tag=1.28` в `helm get values` остался только `image.tag`.

### 2. `failed` ревизия — не откат

Апгрейд с таймаутом оставил `failed`, а в кластере уже частично новый шаблон (`Updated: 1/2`). Нужен `--rollback-on-failure` или `helm rollback`.

### 3. `helm template` не знает кластер

Он рисует YAML локально. `lookup`, версии API и серверная валидация работают только при реальной установке или `--dry-run=server`.

### 4. Тип значения при `--set`

Имя `replicaCount` в схеме с `type: integer`: `--set replicaCount=abc` отклоняется (`got string, want integer`). Без схемы строка прошла бы в шаблон и сломала бы манифест на стороне API.

### 5. Ручные правки и Helm 4

`kubectl scale` у релиза Helm 4 приводит к конфликту (`conflict with "kubectl.exe" with subresource "scale"`), а `--force-conflicts` перехватывает поле назад.

### 6. `helm list -a` не существует

Короткого флага `-a` нет; фильтры — `--uninstalled`, `--failed` и т. д. (`Error: unknown shorthand flag: 'a' in -a`).

### 7. `uninstall` и история

Без `--keep-history` история удаляется; с ним ревизия получает статус `uninstalled`, и релиз видно только через `helm list --uninstalled`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Helm | параметры, история, откаты, хуки, готовые чарты | шаблоны на Go template сложно читать, Secret с релизом |
| [Kustomize](../14-kustomize/overview.ru.md) | чистый YAML, оверлеи, встроен в kubectl | нет истории релизов и пакетов |
| Голые манифесты | предельная простота | копипаста между окружениями |
| Операторы (модуль 19 дорожной карты) | знание предметной области | сложность |

Частая связка: Helm для сторонних компонентов, Kustomize или GitOps (модуль 20 дорожной карты) для собственных.

## Вопросы с собеседований

<details><summary>1. Что такое чарт и релиз?</summary>

Чарт — пакет шаблонов и значений; релиз — установленный экземпляр чарта с историей ревизий.

→ [Базовая теория](basics.ru.md)

</details>

<details><summary>2. Чем <code>helm template</code> отличается от <code>helm install --dry-run</code>?</summary>

`template` рисует манифесты локально, без кластера. `--dry-run=server` симулирует установку на сервере (нужно подключение), `--dry-run=client` — без обращения к кластеру.

→ [Справочник](#флаги-installupgrade-из-справки)

</details>

<details><summary>3. Где Helm хранит состояние релиза?</summary>

В Secret `sh.helm.release.v1.<имя>.v<N>` типа `helm.sh/release.v1` в namespace релиза; данные — gzip и base64.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>4. В каком порядке накладываются значения?</summary>

`values.yaml` чарта → `-f` файлы по порядку → `--set`. Побеждает последнее.

→ [Продвинутая теория](advanced.ru.md#приоритет-values)

</details>

<details><summary>5. Ловушка: что останется от <code>--set replicaCount=4</code> после <code>helm upgrade --set image.tag=1.28</code>?</summary>

Ничего: без `--reuse-values` значения прошлого вызова не наследуются.

→ [Подвох 1](#1-upgrade-теряет---set-прошлых-вызовов)

</details>

<details><summary>6. Что делает откат: удаляет ревизию?</summary>

Нет: `helm rollback web 1` создаёт новую ревизию («Rollback to 1»).

→ [Базовая теория](basics.ru.md#жизненный-цикл-релиза)

</details>

<details><summary>7. Как защититься от неудачного апгрейда?</summary>

`--wait` и `--rollback-on-failure` (`--atomic`) с `--timeout`; без отката останется ревизия `failed`.

→ [Продвинутая теория](advanced.ru.md#неудачное-обновление---wait-и---rollback-on-failure)

</details>

<details><summary>8. Что такое хук и когда запускается <code>pre-upgrade</code>?</summary>

Ресурс с аннотацией `helm.sh/hook`. `pre-upgrade` запускается до применения остальных объектов: в опыте Job выполнялся раньше, чем появился Deployment.

→ [Продвинутая теория](advanced.ru.md#хуки)

</details>

<details><summary>9. Как проверить значения до установки?</summary>

`values.schema.json` (JSON Schema) и функция `required` в шаблонах; `helm lint` для ошибок шаблонов.

→ [Продвинутая теория](advanced.ru.md#проверка-входных-значений)

</details>

<details><summary>10. Зачем <code>Chart.lock</code>?</summary>

Фиксирует версии и digest зависимостей после `helm dependency update`.

→ [Продвинутая теория](advanced.ru.md#зависимости-subchart)

</details>

<details><summary>11. Ловушка: <code>helm upgrade</code> упал на конфликте с <code>kubectl</code>. Почему?</summary>

Helm 4 применяет объекты через server-side apply; поле `.spec.replicas` принадлежит менеджеру `kubectl` (subresource `scale`). Решение: `--force-conflicts` или не править объекты руками.

→ [Продвинутая теория](advanced.ru.md#server-side-apply-в-helm-4)

</details>

<details><summary>12. Чем <code>version</code> отличается от <code>appVersion</code>?</summary>

`version` — версия самого чарта (в имени пакета), `appVersion` — версия приложения, справочная; её можно использовать как тег образа по умолчанию.

→ [Базовая теория](basics.ru.md#структура-чарта)

</details>

<details><summary>13. Кто может прочитать значения релиза?</summary>

Любой, кто может читать Secret `sh.helm.release.v1.*` в namespace; значения декодируются тривиально.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>14. Когда Kustomize лучше Helm?</summary>

Когда нужны оверлеи окружений над чистым YAML без шаблонов и не нужны история релизов и пакеты ([модуль 14](../14-kustomize/overview.ru.md)).

→ [Компромиссы](#компромиссы-и-альтернативы)

</details>

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Самопроверка: [квиз](quiz.ru.md), [упражнения](exercises.ru.md)
- Хранение секретов: [07 · ConfigMap и Secret](../07-config-and-secrets/expert.ru.md)
- Альтернатива: [14 · Kustomize](../14-kustomize/overview.ru.md)

## Источники

- [Helm CLI reference](https://helm.sh/docs/helm/helm/)
- [Helm: Architecture — release storage](https://helm.sh/docs/topics/advanced/#storage-backends)
- [Helm: Chart Template Guide — built-in objects](https://helm.sh/docs/chart_template_guide/builtin_objects/)
- [Helm: Chart Hooks](https://helm.sh/docs/topics/charts_hooks/)
- [Helm: Charts](https://helm.sh/docs/topics/charts/)
