---
title: "Безопасность подов и сети: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 17-security
part: 3
tags: [networkpolicy, namespaceselector, egress, pod-security-admission, capabilities, serviceaccount, validatingadmissionpolicy]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Безопасность подов и сети: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Опыты — в kind (Kubernetes 1.37.0, kindnet).

## NetworkPolicy: from, namespaceSelector и «И» против «ИЛИ»

Список `from` состоит из элементов; элемент может содержать `podSelector`, `namespaceSelector` или оба. Здесь легко ошибиться: способ записи меняет смысл.

**Один элемент с двумя селекторами — «И»:** под из namespace с меткой `team=other` **и** с меткой `role=frontend`:

```yaml
ingress:
- from:
  - namespaceSelector: {matchLabels: {team: other}}
    podSelector: {matchLabels: {role: frontend}}
```

**Два элемента — «ИЛИ»:** любой под из namespace `team=other` **или** под `role=frontend` из namespace самой политики:

```yaml
ingress:
- from:
  - namespaceSelector: {matchLabels: {team: other}}
  - podSelector: {matchLabels: {role: frontend}}
```

Опыт (namespace `learn-17-other` с меткой `team=other`, под `outsider` с меткой `role=frontend`):

```text
== AND (один элемент)
client-ok -> BLOCKED    ← role=frontend, но не из team=other
outsider -> OK
== OR (два элемента)
client-ok -> OK
client-bad -> BLOCKED
outsider -> OK
```

Разница в одном отступе (`-` перед `podSelector`), поэтому ревьюйте такие политики внимательно.

## Egress и DNS

`policyTypes: [Egress]` ограничивает **исходящий** трафик выбранных подов. Политика без правил запрещает всё, в том числе DNS-запросы к CoreDNS — и приложение перестаёт находить сервисы по имени:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: client-egress-deny}
spec:
  podSelector: {matchLabels: {role: frontend}}
  policyTypes: [Egress]
```

```text
$ kubectl -n learn-17 exec client-ok -- wget -q -T 3 -O /dev/null http://server.learn-17.svc
BLOCKED
$ kubectl -n learn-17 exec client-ok -- nslookup server.learn-17.svc
;; connection timed out; no servers could be reached
```

Разрешаем DNS и нужный сервер:

```yaml
spec:
  podSelector: {matchLabels: {role: frontend}}
  policyTypes: [Egress]
  egress:
  - to:
    - namespaceSelector: {matchLabels: {kubernetes.io/metadata.name: kube-system}}
      podSelector: {matchLabels: {k8s-app: kube-dns}}
    ports: [{protocol: UDP, port: 53}, {protocol: TCP, port: 53}]
  - to:
    - podSelector: {matchLabels: {app: server}}
    ports: [{protocol: TCP, port: 80}]
```

```text
client-ok -> OK
```

Метку `kubernetes.io/metadata.name` Kubernetes ставит на каждый namespace сам, поэтому по имени namespace можно выбирать без ручных меток. Обратите внимание: ответы на разрешённые соединения возвращаются без отдельных правил.

> ⚠️ Проверить: `ipBlock` (в том числе `except`) и диапазон портов `endPort` взяты из документации и не запускались.

## Pod Security Admission: режимы на практике

В namespace `learn-17-psa` стояло `enforce=baseline`, `warn=restricted`, `audit=restricted`.

**Предупреждение (`warn`) при создании контроллера.** Deployment `dep` создан, а `kubectl` показал, какие поля нарушают `restricted`: PSA проверяет шаблон пода уже при создании Deployment.

**Отказ (`enforce`) действует на поды, а не на контроллеры.** После `kubectl label ns learn-17-psa pod-security.kubernetes.io/enforce=restricted --overwrite`:

```text
Warning: existing pods in namespace "learn-17-psa" violate the new PodSecurity enforce level "restricted:latest"
Warning: dep-5d9b469677-mhvf9 (and 1 other pod): allowPrivilegeEscalation != false, unrestricted capabilities, runAsNonRoot != true, seccompProfile
namespace/learn-17-psa labeled
```

Существующие поды остались, но новый rollout не прошёл:

```text
$ kubectl -n learn-17-psa rollout restart deploy/dep
$ kubectl -n learn-17-psa get rs
NAME             DESIRED   CURRENT   READY   AGE
dep-576fd7f57b   1         0         0       6s      ← новый ReplicaSet, подов нет
dep-5d9b469677   1         1         1       17s
$ kubectl -n learn-17-psa get events --field-selector reason=FailedCreate
Error creating: pods "dep-576fd7f57b-bgw6p" is forbidden: violates PodSecurity "restricted:latest": allowPrivilegeEscalation != false (…)
```

Deployment принял изменение, ReplicaSet — тоже, но поды создать не смог (`ReplicaFailure=True`, `FailedCreate`). Поэтому после включения `enforce` смотрите не только в Deployment, но и в события ReplicaSet.

**Проверка без применения.** Чтобы узнать, что произойдёт, добавьте `--dry-run=server`:

```text
$ kubectl label --dry-run=server --overwrite ns learn-17 pod-security.kubernetes.io/enforce=restricted
Warning: existing pods in namespace "learn-17" violate the new PodSecurity enforce level "restricted:latest"
Warning: client-bad (and 2 other pods): allowPrivilegeEscalation != false, unrestricted capabilities, runAsNonRoot != true, seccompProfile
namespace/learn-17 labeled (server dry run)
```

Типичный путь внедрения: `warn` и `audit` на нужном уровне → исправить манифесты → `enforce`.

Что запрещает `baseline`, в опыте видно по сообщениям: `privileged` и `hostNetwork` (`host namespaces (hostNetwork=true)`).

## Capabilities и привилегии

Linux делит привилегии root на **capabilities**. Контейнер по умолчанию получает 14 из них (`CapEff: 00000000a80425fb`). Можно убрать лишние и добавить нужные:

```yaml
securityContext:
  capabilities: {drop: [NET_RAW], add: [NET_ADMIN]}
```

```text
$ kubectl -n learn-17 exec capsdemo -- sh -c 'grep CapEff /proc/1/status; ping -c1 -W1 127.0.0.1'
CapEff:	00000000a80415fb
PING 127.0.0.1 (127.0.0.1): 56 data bytes
ping: permission denied (are you root?)
$ kubectl -n learn-17 exec client-ok -- ping -c1 -W1 127.0.0.1
64 bytes from 127.0.0.1: seq=0 ttl=64 time=0.078 ms
```

Без `NET_RAW` `ping` (сырые сокеты) не работает; с `NET_ADMIN` процесс получил лишнюю привилегию (бит добавлен). Практика: `drop: [ALL]` и добавлять только необходимое. Что делает `privileged: true`, — отдельно: все capabilities и доступ к устройствам ноды; такой под запрещает уже `baseline`.

> ⚠️ Проверить: `seccompProfile`. В опыте обычный контейнер в kind уже работал под seccomp-фильтром (`Seccomp: 2` в `/proc/1/status`), с `RuntimeDefault` значение то же. Для других сред значение по умолчанию не проверялось.

## Токен ServiceAccount

В каждый под по умолчанию монтируется токен ServiceAccount:

```text
$ kubectl -n learn-17 exec client-ok -- ls /var/run/secrets/kubernetes.io/serviceaccount
ca.crt  namespace  token
```

Если поду не нужен доступ к API, его убирают: `automountServiceAccountToken: false` в поде или в ServiceAccount. В опыте в поде `secure` каталога `/var/run/secrets/kubernetes.io/serviceaccount` не было. Права токена задаёт [RBAC](../10-namespaces-and-rbac/overview.ru.md).

## ValidatingAdmissionPolicy: свои правила

Когда стандартных проверок мало, API-сервер может сам проверять объекты по выражению на CEL (Common Expression Language). Пример: запретить образы с тегом `:latest` в одном namespace.

```yaml
apiVersion: admissionregistration.k8s.io/v1
kind: ValidatingAdmissionPolicy
metadata: {name: no-latest-17}
spec:
  failurePolicy: Fail
  matchConstraints:
    resourceRules:
    - apiGroups: [""]
      apiVersions: ["v1"]
      operations: ["CREATE","UPDATE"]
      resources: ["pods"]
  validations:
  - expression: "object.spec.containers.all(c, !c.image.endsWith(':latest'))"
    message: "image tag :latest is not allowed"
---
apiVersion: admissionregistration.k8s.io/v1
kind: ValidatingAdmissionPolicyBinding
metadata: {name: no-latest-17}
spec:
  policyName: no-latest-17
  validationActions: [Deny]
  matchResources:
    namespaceSelector: {matchLabels: {kubernetes.io/metadata.name: learn-17}}
```

```text
$ kubectl -n learn-17 run lat --image=nginx:latest
The pods "lat" is invalid: : ValidatingAdmissionPolicy 'no-latest-17' with binding 'no-latest-17' denied request: image tag :latest is not allowed
$ kubectl -n learn-17 run notlat --image=nginx:1.29
pod/notlat created
```

Политика (`Policy`) описывает правило, привязка (`Binding`) — где оно действует и что делать (`Deny`, `Warn`, `Audit`). Подробнее — в [экспертной теории](expert.ru.md).

## Лучшие практики

- В каждом namespace с приложениями начинайте с `default-deny` для входящего трафика, затем добавляйте разрешения.
- Для исходящего трафика не забывайте DNS.
- Включайте `restricted` через `warn`/`audit`, а затем `enforce`.
- В подах: `runAsNonRoot`, `allowPrivilegeEscalation: false`, `capabilities.drop: [ALL]`, `seccompProfile: RuntimeDefault`, `readOnlyRootFilesystem` с томом для записи.
- Отключайте токен ServiceAccount там, где он не нужен.
- Образы: фиксированные теги или digest, сканирование в CI (инструменты вроде Trivy).

> ⚠️ Проверить: сканирование образов (Trivy, Grype и т. п.) и подписи образов в опытах не запускались — это рекомендации из документации.

## Частые ошибки

- ❌ Перепутать «И» и «ИЛИ» в `from` → ✅ проверять политики запросами с разных подов.
- ❌ Egress `default-deny` без DNS → ✅ разрешить UDP/TCP 53 к CoreDNS.
- ❌ Ждать, что `enforce` остановит уже работающие поды → ✅ он проверяет только создание.
- ❌ Искать причину в Deployment → ✅ смотреть `kubectl get events` по ReplicaSet (`FailedCreate`).
- ❌ Выдавать capabilities «на всякий случай» → ✅ `drop: [ALL]` и точечный `add`.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- RBAC и ServiceAccount: [10 · Namespaces и RBAC](../10-namespaces-and-rbac/advanced.ru.md)
- Секреты: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Размещение подов (taints, affinity): [18 · Планирование подов](../18-scheduling/overview.ru.md)

## Источники

- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Pod Security Admission](https://kubernetes.io/docs/concepts/security/pod-security-admission/)
- [Enforce Pod Security Standards with Namespace Labels](https://kubernetes.io/docs/tasks/configure-pod-container/enforce-standards-namespace-labels/)
- [Validating Admission Policy](https://kubernetes.io/docs/reference/access-authn-authz/validating-admission-policy/)
- [Configure Service Accounts for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/)
