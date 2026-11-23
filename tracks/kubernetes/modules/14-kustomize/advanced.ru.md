---
title: "Kustomize: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 14-kustomize
part: 2
tags: [kustomize, patches, json6902, strategic-merge, configmap-generator, replacements, components]
versions: "Kustomize 5.8.1 (в составе kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): структура base и overlay не повторяется.

## Патчи

Поле `patches` меняет часть объекта. Есть два формата.

### Strategic merge: кусок манифеста

В prod добавим лимиты ресурсов, описав только нужную часть Deployment:

```yaml
# overlays/prod/resources-patch.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web
spec:
  template:
    spec:
      containers:
        - name: web
          resources:
            limits: {cpu: 200m, memory: 128Mi}
```

```yaml
# overlays/prod/kustomization.yaml (фрагмент)
patches:
  - path: resources-patch.yaml
```

Контейнер находится по `name: web`, остальные его поля сохраняются: в результате остались `image: nginx:1.29` и `ports`, а к ним добавились `resources`.

### JSON 6902: список операций

```yaml
patches:
  - target: {kind: Deployment, name: web}
    patch: |-
      - op: replace
        path: /spec/replicas
        value: 5
      - op: add
        path: /spec/template/spec/containers/0/env
        value: [{name: MODE, value: json6902}]
```

Результат: `replicas: 5` и `env: [{name: MODE, value: json6902}]`. Путь указывает индекс контейнера (`/containers/0`).

### Выбор цели

Патч применяется к объекту, выбранному в `target` (по `kind`, `name`, метке и т. д.), а не к тому, что написано в `metadata.name` патча:

```yaml
patches:
  - target: {kind: Service}
    patch: |-
      apiVersion: v1
      kind: Service
      metadata:
        name: ignored
      spec:
        type: NodePort
```

В опыте Service `web` получил `type: NodePort`, хотя в патче стояло имя `ignored`.

### Ошибки патчей

```text
$ kubectl kustomize .          # patches: [{path: nope.yaml}]
error: trouble configuring builtin PatchTransformer with config: `
path: nope.yaml
`: failed to get the patch file from path(nope.yaml): … The system cannot find the file specified.
$ kubectl kustomize .          # патч Deployment/ghost, которого нет
error: no resource matches strategic merge patch "Deployment.v1.apps/ghost.[noNs]": no matches for Id Deployment.v1.apps/ghost.[noNs]; failed to find unique target for patch Deployment.v1.apps/ghost.[noNs]
```

## Генераторы ConfigMap и Secret

```yaml
configMapGenerator:
  - name: app-config
    literals: [LOG_LEVEL=warn]
```

К имени добавляется хеш содержимого: `prod-app-config-dfk4bdbtkk`. Подключим ConfigMap в Deployment патчем (`envFrom` с `name: app-config`):

```yaml
spec:
  template:
    spec:
      containers:
        - name: web
          envFrom:
            - configMapRef: {name: app-config}
```

Kustomize заменяет ссылку на настоящее имя:

```text
$ kubectl -n learn-14-prod get deploy prod-web -o jsonpath='{.spec.template.spec.containers[0].envFrom}'
[{"configMapRef":{"name":"prod-app-config-dfk4bdbtkk"}}]
```

Меняем значение (`warn` → `debug`) и смотрим `kubectl diff -k`:

```text
-            name: prod-app-config-dfk4bdbtkk
+            name: prod-app-config-47668c6k28
+  LOG_LEVEL: debug
+  name: prod-app-config-47668c6k28
```

Новое содержимое — **новое имя**, а значит изменился шаблон пода и пошёл rolling update:

```text
$ kubectl apply -k overlays/prod
configmap/prod-app-config-47668c6k28 created
service/prod-web unchanged
deployment.apps/prod-web configured
$ kubectl -n learn-14-prod get rs
prod-web-5944d4c54    3 3 3
prod-web-6fc9fb97d8   0 0 0
$ kubectl -n learn-14-prod exec deploy/prod-web -- printenv LOG_LEVEL
debug
```

Так решается проблема [«изменение ConfigMap не перезапускает поды»](../07-config-and-secrets/advanced.ru.md#изменение-configmap-не-перезапускает-поды). Но старый ConfigMap **остаётся** в кластере: `kubectl apply -k` ничего не удаляет (после `delete -k` в namespace остался `prod-app-config-dfk4bdbtkk`).

Чтобы отключить хеш, используют `generatorOptions`:

```yaml
generatorOptions:
  disableNameSuffixHash: true
secretGenerator:
  - name: creds
    literals: [password=s3cret]
```

Тогда имя остаётся `plain`/`creds`, но смена данных не перезапускает поды. Secret из `secretGenerator` содержит base64 (`password: czNjcmV0`): это не шифрование ([модуль 07](../07-config-and-secrets/overview.ru.md)); не храните значения в `kustomization.yaml` в Git.

## Replacements

`replacements` копируют значение из одного поля в другое. Порт Service попадает в переменную Deployment:

```yaml
replacements:
  - source: {kind: Service, name: web, fieldPath: spec.ports.0.port}
    targets:
      - select: {kind: Deployment, name: web}
        fieldPaths: [spec.template.spec.containers.0.env.0.value]
```

```text
        - name: SVC_PORT
          value: "80"
```

Заготовка `value: placeholder` в `env[0]` заменилась на `80`. С индексом (`env.0`) поле должно существовать.

## Компоненты

Компонент — переиспользуемый кусок, не привязанный к одному base:

```yaml
# comp-metrics/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1alpha1
kind: Component
patches:
  - target: {kind: Deployment}
    patch: |-
      - op: add
        path: /metadata/annotations
        value: {prometheus.io/scrape: "true"}
```

```yaml
# app/kustomization.yaml
resources: [deployment.yaml, service.yaml]
components: [../comp-metrics]
```

```text
$ kubectl kustomize app | grep -B1 -A2 annotations
metadata:
  annotations:
    prometheus.io/scrape: "true"
  name: web
```

## Ограничения загрузки

Kustomize по умолчанию не читает файлы вне папки с `kustomization.yaml`:

```text
error: accumulating resources: accumulation err='accumulating resources from '../../app/base/service.yaml': security; file '…\app\base\service.yaml' is not in or below '…\t4\a'
```

Ссылаться нужно на папки с собственным `kustomization.yaml` (как `../../base`); флаг `--load-restrictor` у `kubectl kustomize` отключает проверку, но делать это не рекомендуется.

> ⚠️ Verify: полный текст ошибки обрезан в опыте; поведение `--load-restrictor=LoadRestrictionsNone` не запускалось.

## Helm внутри Kustomize

У `kubectl kustomize` есть флаг `--enable-helm` (по умолчанию `false`): он позволяет подключать чарты в `kustomization.yaml`.

> ⚠️ Verify: поле `helmCharts` и работа `--enable-helm` не запускались (нужен Helm в `PATH` и доступ к репозиторию).

## Лучшие практики

- base — рабочий манифест, который можно применить сам; в overlay только различия.
- Сначала `kubectl kustomize` или `kubectl diff -k`, потом `apply -k`.
- Генераторы с хешем — для конфигурации, которая должна перезапускать поды; чистите старые ConfigMap (или используйте GitOps с prune).
- Используйте `labels` вместо `commonLabels`; `includeSelectors` включайте осознанно: селекторы Deployment неизменяемы.
- Секреты не хранить в `kustomization.yaml`.
- Общие куски выносите в компоненты.

## Частые ошибки

- ❌ Патч на несуществующий объект → ✅ имя и `kind` должны совпасть (`no resource matches strategic merge patch`).
- ❌ Указать один и тот же ресурс дважды → ✅ `may not add resource with an already registered id`.
- ❌ Ссылаться на файл за пределами папки → ✅ выносить в папку со своим `kustomization.yaml`.
- ❌ Менять селектор через `labels` у живого Deployment → ✅ `includeSelectors` только на старте.

## Сравнение с другими треками

- [Helm](../13-helm/advanced.ru.md): параметры и история вместо патчей над готовым YAML.
- [C#](../../../csharp/recipes/background-jobs/overview.ru.md): конфигурация приложения приходит из ConfigMap/Secret ([07](../07-config-and-secrets/overview.ru.md)); Kustomize лишь управляет их версиями.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Rolling update: [05 · Deployment](../05-deployments/advanced.ru.md#rolling-update-по-шагам)
- Секреты: [07 · ConfigMap и Secret](../07-config-and-secrets/advanced.ru.md)

## Источники

- [Kustomize: patches](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/patches/)
- [Kustomize: configMapGenerator](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/configmapgenerator/)
- [Kustomize: replacements](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/replacements/)
- [Kustomize: components](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/components/)
- [Kustomize: generatorOptions](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/generatoroptions/)
