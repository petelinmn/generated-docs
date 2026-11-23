---
title: "Упражнения: Helm"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 13-helm
part: 2
tags: [helm, chart, values, release]
status: draft
updated: 2026-11-23
---

# Упражнения: Helm

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен Helm 4 и кластер kind; namespace: `kubectl create namespace learn-13`. После работы: `kubectl delete namespace learn-13`. Чарт `mini` — из [базовой теории](basics.ru.md#структура-чарта).

## 1. 🟢 Свой чарт и рендер

**Задача:** создайте чарт `mini` (Deployment и Service из базовой теории), проверьте `helm lint` и отрендерите его с `--set env.A=1`.

**Ожидаемый результат:** `0 chart(s) failed`, в манифесте `name: A`, `value: "1"` и образ `nginx:1.29`.

<details><summary>Подсказка</summary>

Образ: `{{ .Values.image.tag | default .Chart.AppVersion }}`; env через `range $k, $v := .Values.env` и `quote`.

</details>

<details><summary>Решение</summary>

```text
$ helm lint mini
1 chart(s) linted, 0 chart(s) failed
$ helm template rel mini --set env.A=1
          image: "nginx:1.29"
          env:
            - name: A
              value: "1"
```

</details>

## 2. 🟢 Установка, обновление, откат

**Задача:** установите релиз `web`, обновите до 3 реплик и образа `1.28`, откатитесь на ревизию 1. Покажите историю.

**Ожидаемый результат:** три ревизии: `superseded`, `superseded`, `deployed (Rollback to 1)`; в кластере 2 реплики и `nginx:1.29`.

<details><summary>Подсказка</summary>

`helm install web mini -n learn-13 --wait`, `helm upgrade … --set replicaCount=3 --set image.tag=1.28`, `helm rollback web 1`.

</details>

<details><summary>Решение</summary>

```text
$ helm history web -n learn-13
1  superseded  Install complete
2  superseded  Upgrade complete
3  deployed    Rollback to 1
$ kubectl -n learn-13 get deploy web-mini -o jsonpath='{.spec.replicas} {.spec.template.spec.containers[0].image}'
2 nginx:1.29
```

</details>

## 3. 🟡 Потеря значений при апгрейде

**Задача:** сделайте два `helm upgrade` подряд с разными `--set` (`replicaCount=4`, затем `image.tag=1.28`) и посмотрите `helm get values`. Затем сделайте `--reuse-values --set env.X=1`.

**Ожидаемый результат:** после второго вызова `replicaCount` пропал; после `--reuse-values` остались `image.tag` и `env.X`.

<details><summary>Подсказка</summary>

`helm get values web -n learn-13`.

</details>

<details><summary>Решение</summary>

```text
USER-SUPPLIED VALUES:
image:
  tag: "1.28"
# после --reuse-values --set env.X=1
USER-SUPPLIED VALUES:
env:
  X: 1
image:
  tag: "1.28"
```

</details>

## 4. 🟡 Защита входа и откат при неудаче

**Задача:** добавьте `values.schema.json` с `replicaCount` как целым числом не меньше 1 и убедитесь, что `replicaCount=0` отвергается. Затем запустите апгрейд с несуществующим образом, сначала с `--wait --timeout 30s`, потом с `--atomic`, и сравните историю.

**Ожидаемый результат:** ошибка `minimum: got 0, want 1`; ревизия `failed` без отката в первом случае и `Rollback to N` во втором.

<details><summary>Подсказка</summary>

`helm history web -n learn-13`.

</details>

<details><summary>Решение</summary>

```text
Error: INSTALLATION FAILED: values don't meet the specifications of the schema(s) …
- at '/replicaCount': minimum: got 0, want 1
7  failed    Upgrade "web" failed: resource Deployment/learn-13/web-mini not ready…
8  failed    Upgrade "web" failed…
9  deployed  Rollback to 6
```

</details>

## 5. 🔴 Хук миграции и конфликт server-side apply

**Задача:** (а) добавьте хук `pre-install,pre-upgrade` с Job `sleep 3` и политикой удаления `before-hook-creation,hook-succeeded`; во время установки с `--wait` наблюдайте за Job и Deployment. (б) Выполните `kubectl scale deploy web-mini --replicas=9`, затем `helm upgrade --reuse-values` и после ошибки — с `--force-conflicts`.

**Ожидаемый результат:** (а) Job работает до появления Deployment и затем исчезает; (б) `conflict with "kubectl.exe" with subresource "scale"`, затем `replicas=2`.

<details><summary>Подсказка</summary>

(а) цикл `kubectl get jobs,deploy` раз в секунду. (б) `--force-conflicts` у `helm upgrade`.

</details>

<details><summary>Решение</summary>

```text
t=1 jobs=hk2-mini-migrate:Running  deploy=
t=5 jobs=                          deploy=hk2-mini:0/2
t=6 jobs=                          deploy=hk2-mini:2/2
Error: UPGRADE FAILED: conflict occurred while applying object … conflict with "kubectl.exe" with subresource "scale" using apps/v1: .spec.replicas
replicas=2 manual=yes
```

</details>
