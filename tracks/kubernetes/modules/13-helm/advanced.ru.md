---
title: "Helm: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 13-helm
part: 2
tags: [helm, values, hooks, dependencies, atomic, reuse-values, schema, server-side-apply]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): чарт `mini` и команды релиза не повторяются.

## Приоритет values

Значения накладываются по возрастанию приоритета: `values.yaml` чарта → файлы `-f` (по порядку) → `--set`. В опыте `-f v.yaml` задавал `replicaCount: 5` и `env.FROM: file`, а `--set replicaCount=7`:

```text
$ helm template r mini -f v.yaml --set replicaCount=7
  replicas: 7
            - name: FROM
              value: "file"
```

Победила `--set` для `replicaCount`; `env.FROM` пришёл из файла. Вложенные карты сливаются по ключам, а не заменяются целиком. Экранирование запятой в `--set`: `a\,b` даёт значение `a,b`; `--set-string` задаёт строку.

## `--reuse-values` и сброс при апгрейде

По умолчанию `helm upgrade` **не помнит** прошлых `--set`:

```text
$ helm upgrade web mini --set replicaCount=4
$ helm upgrade web mini --set image.tag=1.28
$ helm get values web
USER-SUPPLIED VALUES:
image:
  tag: "1.28"                      # replicaCount=4 пропал
$ helm upgrade web mini --reuse-values --set env.X=1
$ helm get values web
USER-SUPPLIED VALUES:
env:
  X: 1
image:
  tag: "1.28"
```

`--reuse-values` берёт значения прошлого релиза и добавляет новые. Есть ещё `--reset-then-reuse-values`: значения из чарта, затем значения прошлого релиза, затем переопределения из командной строки. Надёжнее всего хранить все значения в файле и каждый раз передавать `-f values.prod.yaml`.

## Неудачное обновление: `--wait` и `--rollback-on-failure`

Образ с несуществующим тегом:

```text
$ helm upgrade web mini --set image.tag=nonexistent-tag --wait --timeout 30s
Error: UPGRADE FAILED: resource Deployment/learn-13/web-mini not ready. status: InProgress, message: Updated: 1/2
context deadline exceeded
$ helm history web
6  deployed  Upgrade complete
7  failed    Upgrade "web" failed: resource Deployment/learn-13/web-mini not ready…
```

Релиз получает ревизию `failed`, а в кластере уже частично новый шаблон (`Updated: 1/2`). С `--atomic` (в справке Helm 4 флаг называется `--rollback-on-failure`):

```text
$ helm upgrade web mini --set image.tag=nonexistent-tag --atomic --timeout 30s
Error: UPGRADE FAILED: release web failed, and has been rolled back due to rollback-on-failure being set: …
$ helm history web
7  failed    Upgrade "web" failed…
8  failed    Upgrade "web" failed…
9  deployed  Rollback to 6
```

Релиз откатан на последнюю успешную ревизию (`9`, «Rollback to 6»); ревизии неудачных попыток остаются в истории. Значения `--wait`: `watcher`, `hookOnly` (по умолчанию, если флаг не указан: ожидаются только хуки), `legacy`; `--wait-for-jobs` ждёт завершения Job.

> ⚠️ Verify: что именно делает `--atomic` в Helm 4 (алиас или устаревший флаг) выведено из сообщения об ошибке и справки; в опыте он сработал как откат.

## Проверка входных значений

`values.schema.json` проверяет значения до отправки в кластер:

```json
{"$schema":"http://json-schema.org/draft-07/schema#","type":"object",
 "properties":{"replicaCount":{"type":"integer","minimum":1}}}
```

```text
$ helm install bad mini --set replicaCount=0
Error: INSTALLATION FAILED: values don't meet the specifications of the schema(s) in the following chart(s):
mini:
- at '/replicaCount': minimum: got 0, want 1
$ helm template x mini --set replicaCount=abc
- at '/replicaCount': got string, want integer
```

В шаблоне `required` обязывает задать значение:

```yaml
image: "{{ required "image.repository is required" .Values.image.repository }}:…"
```

```text
$ helm template x mini --set image.repository=
Error: execution error at (mini/templates/deployment.yaml:19:21): image.repository is required
```

`helm lint` ловит ошибки шаблонов (`nil pointer evaluating interface {}.deep`) до установки.

## Хуки

Хук — ресурс с аннотацией `helm.sh/hook`. Типичный пример — миграция БД перед обновлением:

```yaml
metadata:
  annotations:
    "helm.sh/hook": pre-install,pre-upgrade
    "helm.sh/hook-weight": "0"
    "helm.sh/hook-delete-policy": before-hook-creation,hook-succeeded
```

Наблюдение за установкой (`--wait`), раз в секунду:

```text
t=1 jobs=hk2-mini-migrate:Running  deploy=
t=4 jobs=hk2-mini-migrate:Running  deploy=
t=5 jobs=                          deploy=hk2-mini:0/2
t=6 jobs=                          deploy=hk2-mini:2/2
```

Job запустился **до** создания Deployment; Helm дождался его завершения, затем применил остальное, а Job удалился (`hook-succeeded`). Хук-ресурс виден в `helm get hooks`. Тестовый хук `test` запускается только командой `helm test`.

> ⚠️ Verify: полный список типов хуков и порядок при нескольких хуках с весами взяты из документации и не воспроизводились.

## Зависимости: subchart

```yaml
# Chart.yaml
apiVersion: v2
name: umbrella
version: 0.2.0
dependencies:
  - name: mini
    version: 0.1.0
    repository: "file://charts/mini"
    condition: mini.enabled
```

```text
$ helm dependency update umbrella
Saving 1 charts
$ ls umbrella/charts
mini  mini-0.1.0.tgz
$ helm template u umbrella | grep -E "^# Source|replicas"
# Source: umbrella/charts/mini/templates/service.yaml
# Source: umbrella/charts/mini/templates/deployment.yaml
  replicas: 1
$ helm template u umbrella --set mini.enabled=false | wc -c
1
```

`Chart.lock` фиксирует версии и digest зависимостей. Значения подчарта задаются под его именем (`mini.replicaCount: 1` в `values.yaml` родителя дал `replicas: 1`); `condition` отключает подчарт (при `mini.enabled=false` манифестов нет).

> ⚠️ Verify: зависимости из удалённых репозиториев (HTTP и OCI), поля `alias`, `tags`, `import-values` и блок `global:` не запускались: использовался только `file://`.

## Упаковка

```text
$ helm package mini
Successfully packaged chart and saved it to: …\mini-0.1.0.tgz
$ helm show values mini-0.1.0.tgz | head -2
replicaCount: 2
image:
```

> ⚠️ Verify: публикация и установка из реестров (`helm push`, `helm repo add`, `oci://`) не проверялись: в среде не было доступа к реестру.

## Server-side apply в Helm 4

В справке Helm 4: `--server-side` принимает `true`, `false`, `auto` (по умолчанию `auto`: метод прошлого релиза). Ручное `kubectl scale` забирает поле `.spec.replicas`, и апгрейд ловит конфликт:

```text
$ kubectl scale deploy web-mini --replicas=9
$ kubectl annotate deploy web-mini manual=yes
$ helm upgrade web mini --reuse-values
Error: UPGRADE FAILED: conflict occurred while applying object learn-13/web-mini apps/v1, Kind=Deployment: Apply failed with 1 conflict: conflict with "kubectl.exe" with subresource "scale" using apps/v1: .spec.replicas
$ helm upgrade web mini --reuse-values --force-conflicts
replicas=2 manual=yes
```

Неудачная попытка получила ревизию `failed`; с `--force-conflicts` Helm вернул `replicas=2`, а чужая аннотация `manual=yes` осталась: Helm не трогает поля, которыми не владеет.

## Лучшие практики

- Значения окружения — в файлах `values.<env>.yaml`, а не в `--set`.
- Стандартная команда деплоя: `helm upgrade --install --wait --rollback-on-failure --timeout …`.
- Добавляйте `values.schema.json` и `required` для обязательных параметров.
- Имена ресурсов и метки выносите в `_helpers.tpl`.
- Секреты не кладите в `values.yaml` в Git; используйте внешние механизмы ([модуль 07](../07-config-and-secrets/advanced.ru.md)).
- Не правьте релиз руками через `kubectl`: следующий `helm upgrade` либо перезапишет изменения, либо упадёт с конфликтом.

## Частые ошибки

- ❌ `helm upgrade --set x=1` без `--reuse-values` → ✅ значения хранить в файле.
- ❌ Апгрейд без `--wait` → ✅ `--wait` и откат при ошибке.
- ❌ Ручной `kubectl scale` у релиза Helm → ✅ менять `replicaCount` (в Helm 4 иначе конфликт).
- ❌ Ждать, что `helm template` проверил кластерные ограничения → ✅ он только рисует YAML.

## Сравнение с другими треками

- [Kustomize](../14-kustomize/overview.ru.md): оверлеи вместо шаблонов; в Helm — параметры и история релизов.
- [C#](../../../csharp/recipes/background-jobs/overview.ru.md): конфигурация приложения приходит из ConfigMap и Secret ([модуль 07](../07-config-and-secrets/overview.ru.md)) независимо от того, чем они созданы.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Откат Deployment: [05 · Deployment](../05-deployments/advanced.ru.md#история-ревизий-и-откат)
- Хуки на Job: [12 · Другие workload-ы](../12-workloads/advanced.ru.md#job-повторы-индексы-очистка)

## Источники

- [Helm: Values Files](https://helm.sh/docs/chart_template_guide/values_files/)
- [Helm: Chart Hooks](https://helm.sh/docs/topics/charts_hooks/)
- [Helm: Charts — Chart Dependencies](https://helm.sh/docs/topics/charts/#chart-dependencies)
- [Helm CLI: helm upgrade](https://helm.sh/docs/helm/helm_upgrade/)
- [Helm: Schema files](https://helm.sh/docs/topics/charts/#schema-files)
