---
title: "Упражнения: CRD и операторы"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 19-crd-and-operators
part: 3
tags: [crd, operator, cel, cert-manager]
status: draft
updated: 2026-11-23
---

# Упражнения: CRD и операторы

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-19`. Упражнению 5 нужны Helm и интернет. После работы: `kubectl delete crd widgets.example.com`, `kubectl delete namespace learn-19`.

## 1. 🟢 Первый CRD

**Задача:** создайте CRD `widgets.example.com` (поля `spec.image` обязательное, `spec.replicas` 1–5 по умолчанию 1) и объект `w1` без `replicas`. Покажите, что значение подставлено.

**Ожидаемый результат:** `{"image":"nginx:1.29","replicas":1}`.

<details><summary>Подсказка</summary>

Манифест — в [базовой теории](basics.ru.md#создаём-crd); `kubectl get widget w1 -o jsonpath='{.spec}'`.

</details>

<details><summary>Решение</summary>

```text
$ kubectl -n learn-19 get widget w1 -o jsonpath='{.spec}'
{"image":"nginx:1.29","replicas":1}
```

</details>

## 2. 🟢 Отказы схемы

**Задача:** попробуйте создать объекты: без `image`; с `replicas: 9`; с `tier: gold` (если в схеме `enum: [free, pro]`); с `replicas: "two"`. Запишите сообщения.

**Ожидаемый результат:** `Required value`, `should be less than or equal to 5`, `Unsupported value: "gold"`, `must be of type integer`.

<details><summary>Подсказка</summary>

Каждое сообщение содержит путь поля (`spec.replicas`).

</details>

<details><summary>Решение</summary>

```text
spec.image: Required value
spec.replicas: Invalid value: 9: spec.replicas in body should be less than or equal to 5
spec.tier: Unsupported value: "gold": supported values: "free", "pro"
spec.replicas: Invalid value: "string": spec.replicas in body must be of type integer: "string"
```

</details>

## 3. 🟡 CEL и `has()`

**Задача:** добавьте правило `self.tier != 'free' || self.replicas <= 2` и проверьте на объекте без `tier`. Исправьте правило и убедитесь, что `tier: free, replicas: 4` отклоняется, а без `tier` всё работает.

**Ожидаемый результат:** сначала `no such key: tier`, затем `free tier allows at most 2 replicas` только при `tier: free`.

<details><summary>Подсказка</summary>

`!has(self.tier) || …`

</details>

<details><summary>Решение</summary>

```yaml
x-kubernetes-validations:
- rule: "!has(self.tier) || self.tier != 'free' || self.replicas <= 2"
  message: "free tier allows at most 2 replicas"
```

</details>

## 4. 🟡 Status и pruning

**Задача:** (а) запишите `status.phase` обычным `patch` и через `--subresource=status`. (б) Примените объект с лишним полем `color` строго и с `--validate=false`; посмотрите, что сохранилось.

**Ожидаемый результат:** (а) `(no change)` и затем `Ready`; (б) `strict decoding error: unknown field "spec.color"`, затем `{"image":"nginx","replicas":1}`.

<details><summary>Подсказка</summary>

`kubectl get wg` покажет колонку `Phase`.

</details>

<details><summary>Решение</summary>

Команды и результат — в [продвинутой теории](advanced.ru.md#status-subresource) и [здесь](advanced.ru.md#pruning-неизвестные-поля).

</details>

## 5. 🔴 Оператор: свой и готовый

**Задача:** (а) запустите игрушечный оператор из [продвинутой теории](advanced.ru.md#игрушечный-оператор) (ConfigMap со скриптом, ServiceAccount, ClusterRole, Deployment). Создайте `Widget` с `replicas: 2`, затем измените на 3, затем вручную уменьшите Deployment, затем удалите Widget. (б) Установите cert-manager (`crds.enabled=true`), создайте `Issuer` (selfSigned) и `Certificate`, найдите Secret и удалите его.

**Ожидаемый результат:** (а) Deployment следует за `spec`, ручной `scale` отменяется, при удалении Widget Deployment исчезает; (б) Secret `kubernetes.io/tls`, после удаления создаётся заново.

<details><summary>Подсказка</summary>

Не забудьте `get` для `widgets/status` в ClusterRole.

</details>

<details><summary>Решение</summary>

```text
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
shop   3          Ready
$ kubectl -n learn-19 get deploy widget-shop
widget-shop   3/3   3   3
$ kubectl -n learn-19 get certificate,issuer
certificate.cert-manager.io/demo    True   demo-tls
issuer.cert-manager.io/selfsigned   True
```

</details>
