---
title: "Helm: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 13-helm
part: 2
tags: [helm, chart, values, release, templates]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Deployment](../05-deployments/overview.ru.md), [06 · Service](../06-services/overview.ru.md) · **Время:** ~3 ч

Все примеры выполнены в kind (Kubernetes 1.37.0) с Helm 4.3, namespace `learn-13`. Helm — отдельная программа, не часть kubectl; установка: [документация](https://helm.sh/docs/intro/install/).

## Зачем это нужно

Один и тот же сервис живёт в разных окружениях: меняются число реплик, образ, ресурсы. Копировать манифесты и править вручную неудобно и опасно. Helm описывает сервис один раз шаблонами и подставляет параметры при установке; кроме того, помнит историю установок и умеет откатывать.

## Структура чарта

Минимальный чарт `mini`:

```text
mini/
├── Chart.yaml            метаданные чарта
├── values.yaml           значения по умолчанию
└── templates/
    ├── _helpers.tpl      именованные шаблоны (не создаёт манифест)
    ├── deployment.yaml
    ├── service.yaml
    ├── NOTES.txt         текст после установки
    └── tests/test.yaml   тест релиза
```

`Chart.yaml`:

```yaml
apiVersion: v2
name: mini
description: Minimal web chart
type: application
version: 0.1.0        # версия чарта
appVersion: "1.29"    # версия приложения (справочно)
```

`values.yaml`:

```yaml
replicaCount: 2
image:
  repository: nginx
  tag: ""
  pullPolicy: IfNotPresent
service:
  type: ClusterIP
  port: 80
env: {}
resources: {}
```

Команда `helm create <имя>` создаёт заготовку чарта с типовыми шаблонами (Deployment, Service, ServiceAccount, Ingress, HPA, HTTPRoute, тест).

## Шаблоны

Файлы в `templates/` — шаблоны Go template. Действия записываются в `{{ … }}`:

```yaml
# templates/deployment.yaml (фрагмент)
spec:
  replicas: {{ .Values.replicaCount }}
  template:
    spec:
      containers:
        - name: web
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
          {{- with .Values.env }}
          env:
            {{- range $k, $v := . }}
            - name: {{ $k }}
              value: {{ $v | quote }}
            {{- end }}
          {{- end }}
```

Основное:

| Конструкция | Смысл |
|---|---|
| `.Values.x` | значение из `values.yaml`, `-f` и `--set` |
| `.Release.Name`, `.Release.Namespace` | имя релиза и namespace |
| `.Chart.Name`, `.Chart.AppVersion` | поля `Chart.yaml` |
| `\| default X` | значение по умолчанию, если пусто |
| `\| quote` | обернуть в кавычки |
| `with`, `range`, `if` | область, цикл, условие |
| `include "имя" .` | вызвать именованный шаблон из `_helpers.tpl` |
| `nindent N` | перенос строки и отступ в N пробелов |
| `toYaml` | превратить структуру в YAML |
| `{{-` и `-}}` | убрать пробелы и переносы слева или справа |

Именованный шаблон в `_helpers.tpl`:

```yaml
{{- define "mini.fullname" -}}
{{- printf "%s-%s" .Release.Name .Chart.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
```

## Посмотреть результат без установки

`helm template` рисует манифесты локально, без кластера; `helm lint` проверяет чарт:

```text
$ helm lint mini
==> Linting mini
[INFO] Chart.yaml: icon is recommended
1 chart(s) linted, 0 chart(s) failed
$ helm template rel mini --set env.A=1 --set resources.limits.cpu=100m
---
# Source: mini/templates/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: rel-mini
…
          image: "nginx:1.29"
          env:
            - name: A
              value: "1"
          resources:
            limits:
              cpu: 100m
```

Образ взят из `appVersion` (`1.29`), так как `image.tag` пуст; имя ресурса — `<релиз>-<чарт>`.

## Жизненный цикл релиза

```text
$ helm install web mini -n learn-13 --wait --timeout 90s
NAME: web
STATUS: deployed
REVISION: 1
NOTES:
Release web installed in learn-13.
Service: web-mini:80
$ helm list -n learn-13
NAME  NAMESPACE  REVISION  STATUS    CHART       APP VERSION
web   learn-13   1         deployed  mini-0.1.0  1.29
$ helm upgrade web mini -n learn-13 --set replicaCount=3 --set image.tag=1.28 --wait
Release "web" has been upgraded. Happy Helming!
$ helm history web -n learn-13
1  superseded  mini-0.1.0  1.29  Install complete
2  deployed    mini-0.1.0  1.29  Upgrade complete
$ helm rollback web 1 -n learn-13 --wait
Rollback was a success! Happy Helming!
$ helm history web -n learn-13
1  superseded  Install complete
2  superseded  Upgrade complete
3  deployed    Rollback to 1
$ kubectl -n learn-13 get deploy web-mini -o jsonpath='{.spec.replicas} {.spec.template.spec.containers[0].image}'
2 nginx:1.29
```

Откат создаёт **новую** ревизию (3), а не стирает вторую. Что реально передано:

```text
$ helm get values web -n learn-13          # только заданное пользователем
USER-SUPPLIED VALUES:
image:
  tag: "1.28"
replicaCount: 3
$ helm get values web -n learn-13 -a       # итоговые значения
COMPUTED VALUES:
env: {}
image:
  pullPolicy: IfNotPresent
  repository: nginx
  tag: "1.28"
…
```

Удаление: `helm uninstall web -n learn-13` убирает объекты релиза и его историю (с `--keep-history` история остаётся со статусом `uninstalled`).

## Тест релиза

Pod с аннотацией `"helm.sh/hook": test` запускается командой `helm test`:

```text
$ helm test web -n learn-13
NAME: web
STATUS: deployed
REVISION: 3
TEST SUITE:     web-mini-test
Phase:          Succeeded
```

## Ошибки новичка

- ❌ Править объекты релиза через `kubectl edit` → ✅ менять values или шаблоны и делать `helm upgrade`.
- ❌ Не указывать `--wait` и считать, что приложение готово → ✅ `--wait` ждёт готовности ресурсов.
- ❌ Забывать `-n` и ставить релиз не в тот namespace → ✅ всегда указывать namespace.
- ❌ Ждать, что `helm template` проверил кластерные ограничения → ✅ он только рисует YAML; для проверки на сервере — `--dry-run=server`.
- ❌ Путать `version` и `appVersion` → ✅ первая — версия чарта, вторая — справочная версия приложения.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — приоритет values, откат неудачных обновлений, хуки, зависимости
- Предыдущий модуль: [12 · Другие workload-ы](../12-workloads/overview.ru.md)
- Конфигурация: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Альтернатива без шаблонов: [14 · Kustomize](../14-kustomize/overview.ru.md)

## Источники

- [Helm: Using Helm](https://helm.sh/docs/intro/using_helm/)
- [Helm: Chart Template Guide](https://helm.sh/docs/chart_template_guide/)
- [Helm: Charts](https://helm.sh/docs/topics/charts/)
- [Helm: Chart Tests](https://helm.sh/docs/topics/chart_tests/)
- [Helm CLI: helm install](https://helm.sh/docs/helm/helm_install/)
