---
title: "CRD и операторы: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 19-crd-and-operators
part: 3
tags: [cel, x-kubernetes-validations, pruning, status-subresource, owner-reference, operator, rbac, cert-manager, reconcile]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, alpine/k8s 1.35.0, cert-manager v1.21.3, Helm 4.3"
status: draft
updated: 2026-11-23
---

# CRD и операторы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Опыты — в kind, namespace `learn-19`.

## Правила CEL

Для проверок, связывающих несколько полей, в схему добавляют `x-kubernetes-validations` с выражением на языке CEL ([модуль 17](../17-security/advanced.ru.md#validatingadmissionpolicy-свои-правила) использует тот же язык):

```yaml
spec:
  type: object
  properties: {…}
  x-kubernetes-validations:
  - rule: "self.tier != 'free' || self.replicas <= 2"
    message: "free tier allows at most 2 replicas"
```

Запрос `tier: free, replicas: 4` отклонён с текстом правила:

```text
The Widget "bad" is invalid: spec: Invalid value: free tier allows at most 2 replicas
```

Но у такого правила есть подвох. Поле `tier` необязательное, и когда его нет, правило падает само:

```text
spec.replicas: Invalid value: 9: spec.replicas in body should be less than or equal to 5
spec: Invalid value: "object": no such key: tier evaluating rule: free tier allows at most 2 replicas
```

А у уже существующего объекта без `tier` даже безобидный `patch` с `replicas: 3` отклонялся с той же ошибкой `no such key: tier`. Правило нужно писать с проверкой наличия поля через `has()`:

```yaml
- rule: "!has(self.tier) || self.tier != 'free' || self.replicas <= 2"
```

После замены правила (`kubectl patch crd …`) то же изменение `replicas: 3` прошло, а `tier: free, replicas: 4` по-прежнему отклоняется.

## Pruning: неизвестные поля

Поле, которого нет в схеме, не должно попадать в хранилище. Что происходит:

```text
$ kubectl apply -f w2.yaml        # spec: {image: nginx, color: red}
Error from server (BadRequest): … strict decoding error: unknown field "spec.color"

$ kubectl apply --validate=false -f w2.yaml
widget.example.com/w2 created
$ kubectl get widget w2 -o jsonpath='{.spec}'
{"image":"nginx","replicas":1}
```

`kubectl` по умолчанию проверяет объект строго. Если проверку отключить (`--validate=false`), сервер всё равно **отбросит** неизвестное поле `color` (pruning), и об этом не будет ни слова: данные просто пропадут. Нужное поле добавляйте в схему.

## Status subresource

С включённым `subresources.status` обычное изменение ресурса не затрагивает `status`:

```text
$ kubectl -n learn-19 patch widget w1 --type=merge -p '{"status":{"phase":"Ready"}}'
widget.example.com/w1 patched (no change)
$ kubectl -n learn-19 patch widget w1 --subresource=status --type=merge -p '{"status":{"phase":"Ready"}}'
widget.example.com/w1 patched
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
w1     1          Ready
```

Разделение нужно, чтобы пользователь (пишет `spec`) и контроллер (пишет `status`) не мешали друг другу и чтобы права можно было выдавать отдельно: в RBAC это ресурс `widgets/status`.

## ownerReferences и сборка мусора

Контроллер помечает созданные объекты владельцем. Опыт: ConfigMap `owned` с `ownerReferences` на `Widget w1`:

```yaml
metadata:
  name: owned
  ownerReferences:
  - {apiVersion: example.com/v1, kind: Widget, name: w1, uid: <uid виджета>, controller: true, blockOwnerDeletion: true}
```

```text
$ kubectl -n learn-19 delete widget w1
$ kubectl -n learn-19 get cm owned
Error from server (NotFound): configmaps "owned" not found
```

После удаления владельца сборщик мусора удалил зависимый объект. Так операторы убирают всё, что создали, не вызывая отдельного кода на удаление.

## Игрушечный оператор

Чтобы увидеть цикл сверки, запустим простейший оператор: скрипт `sh` в образе `alpine/k8s:1.35.0` (там есть `kubectl` и `jq`). Он каждые 3 секунды читает все `Widget` и для каждого создаёт или обновляет Deployment `widget-<имя>` с `ownerReferences`, а в `status.phase` пишет `Ready`, когда реплики доступны.

```sh
while true; do
  kubectl get widgets.example.com -A -o json | jq -c '.items[]' | while read -r w; do
    # …достаём ns, name, uid, image, replicas…
    kubectl apply -f - <<DEP      # Deployment widget-$name с ownerReferences на виджет
    …
DEP
    kubectl -n "$ns" patch widget "$name" --subresource=status --type=merge -p "{\"status\":{\"phase\":\"$phase\"}}"
  done
  sleep 3
done
```

Скрипт лежит в ConfigMap `op-script`, запускается Deployment'ом `operator` с ServiceAccount `widget-operator` и ClusterRole:

```yaml
rules:
- {apiGroups: [example.com], resources: [widgets], verbs: [get, list, watch]}
- {apiGroups: [example.com], resources: [widgets/status], verbs: [get, patch, update]}
- {apiGroups: [apps], resources: [deployments], verbs: [get, list, create, patch, update]}
```

В первой версии правила для `widgets/status` не было глагола `get`, и оператор получил:

```text
Error from server (Forbidden): widgets.example.com "shop" is forbidden: User "system:serviceaccount:learn-19:widget-operator" cannot get resource "widgets/status" in API group "example.com" in the namespace "learn-19"
```

`kubectl patch --subresource=status` читает объект перед изменением, поэтому нужен и `get`. Оператор, написанный на клиентской библиотеке, обычно ограничивается `patch`/`update`.

Работа оператора:

```text
$ kubectl -n learn-19 apply -f shop.yaml          # spec: {image: nginx:1.29, replicas: 2}
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
shop   2          Ready
$ kubectl -n learn-19 get deploy widget-shop
NAME          READY   UP-TO-DATE   AVAILABLE
widget-shop   2/2     2            2
$ kubectl -n learn-19 patch widget shop --type=merge -p '{"spec":{"replicas":3}}'
$ kubectl -n learn-19 get wg shop --no-headers
shop   3     Ready
$ kubectl -n learn-19 scale deploy widget-shop --replicas=1     # «дрейф»
… через 6 секунд …
widget-shop   3/3   3     3
```

- Изменение `spec` привело к изменению Deployment.
- Ручное `scale` оператор отменил при следующей сверке: он приводит мир к желаемому состоянию, а не «реагирует на событие».
- После `kubectl delete widget shop` Deployment исчез (сборка мусора).

Это **опрос**, а не наблюдение (watch): настоящие контроллеры используют watch и очереди (см. [экспертную теорию](expert.ru.md#внутреннее-устройство)), но логика та же: идемпотентная сверка.

## Настоящий оператор: cert-manager

cert-manager автоматизирует выпуск сертификатов. Установка чартом Helm (`crds.enabled=true` ставит CRD вместе с чартом):

```text
$ helm repo add jetstack https://charts.jetstack.io
$ helm install cm jetstack/cert-manager -n cert-manager --create-namespace --set crds.enabled=true --wait
STATUS: deployed
$ kubectl get crd | grep cert-manager
certificaterequests.cert-manager.io  certificates.cert-manager.io  challenges.acme.cert-manager.io
clusterissuers.cert-manager.io       issuers.cert-manager.io       orders.acme.cert-manager.io
$ kubectl -n cert-manager get pods
cm-cert-manager-…                1/1   Running
cm-cert-manager-cainjector-…     1/1   Running
cm-cert-manager-webhook-…        1/1   Running
```

Объекты:

```yaml
apiVersion: cert-manager.io/v1
kind: Issuer
metadata: {name: selfsigned}
spec: {selfSigned: {}}
---
apiVersion: cert-manager.io/v1
kind: Certificate
metadata: {name: demo}
spec:
  secretName: demo-tls
  commonName: demo.example.test
  dnsNames: [demo.example.test]
  issuerRef: {name: selfsigned, kind: Issuer}
```

```text
$ kubectl -n learn-19 get issuer,certificate
issuer.cert-manager.io/selfsigned   True
certificate.cert-manager.io/demo    True   demo-tls
$ kubectl -n learn-19 get certificaterequest
demo-1   True (APPROVED)   True (READY)   selfsigned
$ kubectl -n learn-19 get secret demo-tls -o jsonpath='{.type}'
kubernetes.io/tls
$ … tls.crt | openssl x509 -noout -subject -issuer -dates
subject=CN=demo.example.test
issuer=CN=demo.example.test
```

Цепочка событий: `Issuing` (Secret не существует) → `Generated` (временный закрытый ключ) → `CertificateRequest` создан и одобрен → `CertificateIssued` → `Issuing: The certificate has been successfully issued`. Владелец `CertificateRequest` — `Certificate`.

Оператор поддерживает желаемое состояние: после `kubectl delete secret demo-tls` Secret появился снова примерно через 8 секунд.

> ⚠️ Проверить: версионирование CRD (`v1alpha1` → `v1`) и webhook-конверсия в опытах не запускались; у `Certificate` в cert-manager в опыте только версия `v1` (`storage=v1`).

## Лучшие практики

- Схема строгая: `required`, `enum`, границы, `default`; не оставляйте произвольные поля.
- CEL-правила с `has()` для необязательных полей.
- Контроллер пишет только `status`, пользователь — `spec`.
- Сверка идемпотентна: повторный запуск не ломает результат.
- `ownerReferences` на всё, что создаёт контроллер.
- Минимальные права RBAC: свои ресурсы, `status`, управляемые объекты.
- Для готовых задач (сертификаты, базы) используйте существующий оператор, а не пишите свой.

## Частые ошибки

- ❌ CEL-правило без `has()` для необязательного поля → ✅ `!has(self.x) || …`.
- ❌ Писать `status` обычным `patch` → ✅ `--subresource=status`/клиент с `/status`.
- ❌ Не выдать права на `<ресурс>/status` → ✅ добавить в ClusterRole.
- ❌ Неизвестное поле «пропало» → ✅ добавить в схему.
- ❌ Контроллер без `ownerReferences` → ✅ после удаления объекта остаётся мусор.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- RBAC и ServiceAccount: [10 · Namespaces и RBAC](../10-namespaces-and-rbac/advanced.ru.md)
- CEL в admission: [17 · Безопасность](../17-security/advanced.ru.md#validatingadmissionpolicy-свои-правила)
- Helm: [13 · Helm](../13-helm/basics.ru.md)
- Сертификаты и TLS в Ingress: [09 · Ingress и Gateway API](../09-ingress-and-gateway-api/overview.ru.md)

## Источники

- [CRD: validation rules](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definitions/#validation-rules)
- [CRD: defaulting and pruning](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definitions/#defaulting-and-nullable)
- [Garbage Collection](https://kubernetes.io/docs/concepts/architecture/garbage-collection/)
- [cert-manager: Helm installation](https://cert-manager.io/docs/installation/helm/)
- [cert-manager: SelfSigned issuer](https://cert-manager.io/docs/configuration/selfsigned/)
