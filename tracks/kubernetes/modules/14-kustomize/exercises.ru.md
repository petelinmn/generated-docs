---
title: "Упражнения: Kustomize"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 14-kustomize
part: 2
tags: [kustomize, overlay, patch, configmap-generator]
status: draft
updated: 2026-11-23
---

# Упражнения: Kustomize

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind; namespace создайте заранее: `kubectl create namespace learn-14-prod`. После работы: `kubectl delete namespace learn-14-prod`. Структура base и overlay — из [базовой теории](basics.ru.md#структура).

## 1. 🟢 Base и два overlay

**Задача:** создайте base (Deployment `web` с `nginx:1.28` и Service) и два overlay: dev (префикс `dev-`, метка `env: dev`) и prod (префикс `prod-`, 3 реплики, `nginx:1.29`). Посмотрите результат `kubectl kustomize`.

**Ожидаемый результат:** в dev имена `dev-web`, селектор содержит `env: dev`; в prod `replicas: 3`, `image: nginx:1.29`.

<details><summary>Подсказка</summary>

`labels: [{pairs: {env: dev}, includeSelectors: true}]`, `replicas: [{name: web, count: 3}]`, `images: [{name: nginx, newTag: "1.29"}]`.

</details>

<details><summary>Решение</summary>

Манифесты — из [базовой теории](basics.ru.md#overlay).

```text
$ kubectl kustomize overlays/prod | grep -E "replicas|image:"
  replicas: 3
      - image: nginx:1.29
```

</details>

## 2. 🟢 Применение и удаление

**Задача:** примените prod-overlay и затем удалите его, используя `-k`.

**Ожидаемый результат:** `configmap/…`, `service/prod-web`, `deployment.apps/prod-web` созданы и затем удалены.

<details><summary>Подсказка</summary>

`kubectl apply -k overlays/prod`, `kubectl delete -k overlays/prod`.

</details>

<details><summary>Решение</summary>

```text
$ kubectl apply -k overlays/prod
service/prod-web created
deployment.apps/prod-web created
$ kubectl delete -k overlays/prod
service "prod-web" deleted from learn-14-prod namespace
deployment.apps "prod-web" deleted from learn-14-prod namespace
```

</details>

## 3. 🟡 Патч с лимитами и JSON 6902

**Задача:** добавьте в prod лимиты `cpu: 200m`, `memory: 128Mi` патчем strategic merge, а в отдельном overlay поменяйте число реплик на 5 патчем JSON 6902.

**Ожидаемый результат:** `resources.limits` в Deployment; `replicas: 5`.

<details><summary>Подсказка</summary>

Контейнер ищется по `name: web`. Для JSON 6902: `target: {kind: Deployment, name: web}`, `op: replace`, `path: /spec/replicas`.

</details>

<details><summary>Решение</summary>

Патчи — из [продвинутой теории](advanced.ru.md#патчи).

```text
        resources:
          limits:
            cpu: 200m
            memory: 128Mi
  replicas: 5
```

</details>

## 4. 🟡 ConfigMap с хешем и перезапуск

**Задача:** добавьте `configMapGenerator` с `LOG_LEVEL=warn` и подключите его в Deployment через `envFrom`. Примените, затем поменяйте на `debug` и примените ещё раз. Посмотрите имена ConfigMap и ReplicaSet.

**Ожидаемый результат:** два ConfigMap с разными хешами, два ReplicaSet (старый 0, новый 3), `printenv LOG_LEVEL` даёт `debug`.

<details><summary>Подсказка</summary>

`kubectl diff -k overlays/prod` покажет смену имени; `kubectl get cm,rs`.

</details>

<details><summary>Решение</summary>

```text
configmap/prod-app-config-47668c6k28 created
deployment.apps/prod-web configured
prod-web-5944d4c54    3 3 3
prod-web-6fc9fb97d8   0 0 0
debug
```

Старый ConfigMap остался: `apply -k` не удаляет объекты.

</details>

## 5. 🔴 Replacements, компонент и ограничения

**Задача:** (а) с `replacements` передайте порт Service в переменную `SVC_PORT` Deployment. (б) Вынесите добавление аннотации `prometheus.io/scrape: "true"` в `Component` и подключите его. (в) Укажите в `resources` файл из соседней папки и посмотрите на ошибку.

**Ожидаемый результат:** (а) `SVC_PORT: "80"`; (б) аннотация в Deployment; (в) ошибка `security; file … is not in or below …`.

<details><summary>Подсказка</summary>

(а) `source: {kind: Service, name: web, fieldPath: spec.ports.0.port}`; (б) `kind: Component`, `components: [../comp-metrics]`; (в) `resources: [../../app/base/service.yaml]`.

</details>

<details><summary>Решение</summary>

```text
        - name: SVC_PORT
          value: "80"
  annotations:
    prometheus.io/scrape: "true"
error: accumulating resources: accumulation err='accumulating resources from '../../app/base/service.yaml': security; file '…' is not in or below '…'
```

</details>
