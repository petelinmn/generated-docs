---
title: "Упражнения: безопасность подов и сети"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 17-security
part: 3
tags: [networkpolicy, pod-security-admission, securitycontext, validatingadmissionpolicy]
status: draft
updated: 2026-11-23
---

# Упражнения: безопасность подов и сети

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind (kindnet применяет NetworkPolicy) и namespace: `kubectl create namespace learn-17`. После работы: `kubectl delete namespace learn-17 learn-17-psa`.

## 1. 🟢 default deny и разрешение

**Задача:** запустите `server` (nginx, метка `app=server`) с Service, клиентов `client-ok` (`role=frontend`) и `client-bad` (`role=other`). Проверьте доступ, затем примените `default-deny-ingress` и разрешение для `role=frontend`.

**Ожидаемый результат:** сначала оба клиента получают ответ; после deny — оба заблокированы; после разрешения — только `client-ok`.

<details><summary>Подсказка</summary>

Проверка: `kubectl exec client-ok -- wget -q -T 3 -O /dev/null http://server.learn-17.svc`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: allow-frontend}
spec:
  podSelector: {matchLabels: {app: server}}
  policyTypes: [Ingress]
  ingress:
  - from: [{podSelector: {matchLabels: {role: frontend}}}]
    ports: [{protocol: TCP, port: 80}]
```

```text
client-ok -> OK
client-bad -> wget: download timed out   (BLOCKED)
```

</details>

## 2. 🟢 Усиленный под

**Задача:** создайте под `secure` из `busybox:1.37` с uid 10001, `runAsNonRoot`, `readOnlyRootFilesystem`, `drop: [ALL]`, `allowPrivilegeEscalation: false` и `emptyDir` на `/tmp`. Покажите, что запись в `/` невозможна, а в `/tmp` возможна.

**Ожидаемый результат:** `uid=10001`, `touch: /x: Read-only file system`, `tmp-writable`, `CapEff: 0000000000000000`.

<details><summary>Подсказка</summary>

`kubectl exec secure -- grep -E "CapEff|NoNewPrivs" /proc/1/status`.

</details>

<details><summary>Решение</summary>

Манифест — в [базовой теории](basics.ru.md#securitycontext-права-процесса).

```text
uid=10001 gid=10001 groups=10001
touch: /x: Read-only file system
tmp-writable
CapEff:	0000000000000000
NoNewPrivs:	1
```

</details>

## 3. 🟡 «И» и «ИЛИ»

**Задача:** создайте namespace `learn-17-other` с меткой `team=other` и в нём под `outsider` с меткой `role=frontend`. Напишите политику ingress с `namespaceSelector` и `podSelector` в **одном** элементе, затем в **двух**. Сравните доступ для `client-ok` и `outsider`.

**Ожидаемый результат:** в одном элементе `client-ok` заблокирован, `outsider` проходит; в двух элементах проходят оба.

<details><summary>Подсказка</summary>

Различие — наличие второго `-` в списке `from`.

</details>

<details><summary>Решение</summary>

```text
== один элемент (И)
client-ok -> BLOCKED
outsider -> OK
== два элемента (ИЛИ)
client-ok -> OK
client-bad -> BLOCKED
outsider -> OK
```

</details>

## 4. 🟡 Egress и DNS

**Задача:** запретите весь исходящий трафик для `role=frontend` и убедитесь, что имя `server.learn-17.svc` не резолвится. Затем разрешите DNS (к `kube-dns` в `kube-system`) и доступ к `app=server`.

**Ожидаемый результат:** сначала `nslookup` — `connection timed out`, после правила — `client-ok -> OK`.

<details><summary>Подсказка</summary>

`namespaceSelector` с меткой `kubernetes.io/metadata.name: kube-system` и `podSelector` с `k8s-app: kube-dns`; порты 53 UDP и TCP.

</details>

<details><summary>Решение</summary>

Манифест — в [продвинутой теории](advanced.ru.md#egress-и-dns).

```text
;; connection timed out; no servers could be reached
client-ok -> OK
```

</details>

## 5. 🔴 PSA и ValidatingAdmissionPolicy

**Задача:** (а) создайте namespace `learn-17-psa` с `enforce=baseline`, `warn=restricted`; попробуйте создать привилегированный под и обычный. (б) Переключите `enforce` на `restricted`, перезапустите Deployment и найдите причину в событиях ReplicaSet. (в) Создайте ValidatingAdmissionPolicy, запрещающую `:latest`.

**Ожидаемый результат:** (а) отказ `violates PodSecurity "baseline:latest": privileged`, предупреждение `restricted` для обычного пода; (б) `FailedCreate`; (в) `image tag :latest is not allowed`.

<details><summary>Подсказка</summary>

(б) `kubectl get rs`, `kubectl get events --field-selector reason=FailedCreate`; (в) политика и привязка с `validationActions: [Deny]`.

</details>

<details><summary>Решение</summary>

```text
Error from server (Forbidden): pods "priv" is forbidden: violates PodSecurity "baseline:latest": privileged (container "priv" must not set securityContext.privileged=true)
Warning: would violate PodSecurity "restricted:latest": allowPrivilegeEscalation != false (…), …
dep-576fd7f57b   1   0   0   6s
Error creating: pods "dep-576fd7f57b-bgw6p" is forbidden: violates PodSecurity "restricted:latest": …
The pods "lat" is invalid: : ValidatingAdmissionPolicy 'no-latest-17' with binding 'no-latest-17' denied request: image tag :latest is not allowed
```

Манифесты политики — в [продвинутой теории](advanced.ru.md#validatingadmissionpolicy-свои-правила).

</details>
