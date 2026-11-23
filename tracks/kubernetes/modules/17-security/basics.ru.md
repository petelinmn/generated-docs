---
title: "Безопасность подов и сети: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 17-security
part: 3
tags: [networkpolicy, default-deny, securitycontext, pod-security-admission, runasnonroot]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Безопасность подов и сети: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md), [06 · Service и сеть](../06-services/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0, сетевой плагин kindnet), namespace `learn-17`, `learn-17-other` и `learn-17-psa`.

## Зачем это нужно

Кластер — общая среда. Если один под взломан, то по умолчанию он может обратиться к любому другому поду, а процесс внутри запущен от root с широким набором привилегий. Базовая гигиена безопасности — ограничить сеть, права процесса и то, какие поды вообще допускаются в namespace.

## NetworkPolicy: ограничиваем сеть

Подготовка: `server` (nginx с меткой `app=server` и Service), два клиента в том же namespace — `client-ok` (метка `role=frontend`) и `client-bad` (`role=other`), и `outsider` в другом namespace `learn-17-other`. Проверка доступа — `wget` с таймаутом 3 секунды.

**Без политик** все три клиента достучались до сервера:

```text
client-ok -> OK
client-bad -> OK
outsider -> OK
```

### default deny

Политика выбирает поды через `podSelector`. Пустой селектор выбирает все поды namespace; без правил `ingress` она запрещает весь входящий трафик:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: default-deny-ingress}
spec:
  podSelector: {}
  policyTypes: [Ingress]
```

```text
client-ok -> wget: download timed out   (BLOCKED)
client-bad -> wget: download timed out  (BLOCKED)
outsider -> wget: download timed out    (BLOCKED)
```

Правило: **как только под выбран хотя бы одной политикой с типом `Ingress`, разрешён только тот входящий трафик, который перечислен в политиках.**

### Разрешаем нужное

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: allow-frontend}
spec:
  podSelector: {matchLabels: {app: server}}
  policyTypes: [Ingress]
  ingress:
  - from:
    - podSelector: {matchLabels: {role: frontend}}
    ports: [{protocol: TCP, port: 80}]
```

```text
client-ok -> OK
client-bad -> BLOCKED
outsider -> BLOCKED
```

Политики **складываются**: разрешено то, что разрешает хотя бы одна из них. Запретить поверх разрешения нельзя.

> ⚠️ Проверить: политики исполняет сетевой плагин (CNI); в kind это kindnet, и он их применил. Поведение в кластере с плагином без поддержки NetworkPolicy в опытах не проверялось.

## securityContext: права процесса

По умолчанию контейнер работает от root:

```text
$ kubectl -n learn-17 exec client-ok -- sh -c 'id; grep CapEff /proc/1/status; touch /x && echo rootfs-writable'
uid=0(root) gid=0(root) groups=0(root),10(wheel)
CapEff:	00000000a80425fb
rootfs-writable
```

Усиленный вариант:

```yaml
apiVersion: v1
kind: Pod
metadata: {name: secure}
spec:
  securityContext:                     # уровень пода
    runAsNonRoot: true
    runAsUser: 10001
    runAsGroup: 10001
    fsGroup: 10001
    seccompProfile: {type: RuntimeDefault}
  containers:
  - name: c
    image: busybox:1.37
    command: ["sleep","3600"]
    securityContext:                   # уровень контейнера
      allowPrivilegeEscalation: false
      readOnlyRootFilesystem: true
      capabilities: {drop: [ALL]}
    volumeMounts: [{name: tmp, mountPath: /tmp}]
  volumes: [{name: tmp, emptyDir: {}}]
```

```text
$ kubectl -n learn-17-psa exec secure -- id
uid=10001 gid=10001 groups=10001
$ kubectl -n learn-17-psa exec secure -- sh -c 'touch /x; touch /tmp/ok && echo tmp-writable'
touch: /x: Read-only file system
tmp-writable
$ kubectl -n learn-17-psa exec secure -- sh -c 'grep -E "CapEff|NoNewPrivs" /proc/1/status'
CapEff:	0000000000000000
NoNewPrivs:	1
```

- `runAsNonRoot` + `runAsUser` — процесс не root.
- `readOnlyRootFilesystem` — писать можно только в смонтированные тома (здесь `emptyDir` на `/tmp`).
- `capabilities.drop: [ALL]` — ноль capabilities (`CapEff: 0`), вместо 14 привилегий по умолчанию.
- `allowPrivilegeEscalation: false` — процесс не получит больше прав, чем имеет (`NoNewPrivs: 1`).

Если указать `runAsNonRoot: true` для образа, который запускается от root, контейнер не стартует:

```text
$ kubectl -n learn-17 get pod nonroot-fail
NAME           READY   STATUS                       RESTARTS   AGE
nonroot-fail   0/1     CreateContainerConfigError   0          6s
  Warning  Failed  kubelet  Error: container has runAsNonRoot and image will run as root (pod: "nonroot-fail_learn-17(…)", container: c)
```

## Pod Security Admission: контроль на входе

PSA — встроенный admission-контроллер. Он включается **метками namespace**:

```text
$ kubectl label ns learn-17-psa \
    pod-security.kubernetes.io/enforce=baseline \
    pod-security.kubernetes.io/warn=restricted \
    pod-security.kubernetes.io/audit=restricted
```

- **Уровни** (Pod Security Standards): `privileged` (без ограничений), `baseline` (запрещает заведомо опасное), `restricted` (жёсткая практика).
- **Режимы:** `enforce` — отказ в создании пода, `warn` — предупреждение пользователю, `audit` — запись в аудит-лог.

Что получилось в опыте:

```text
$ kubectl -n learn-17-psa run priv --image=nginx:1.29 --overrides='{…"securityContext":{"privileged":true}…}'
Error from server (Forbidden): pods "priv" is forbidden: violates PodSecurity "baseline:latest": privileged (container "priv" must not set securityContext.privileged=true)

$ kubectl -n learn-17-psa run plain --image=nginx:1.29
Warning: would violate PodSecurity "restricted:latest": allowPrivilegeEscalation != false (…), unrestricted capabilities (…), runAsNonRoot != true (…), seccompProfile (…)
pod/plain created
```

Привилегированный под отклонён (`baseline` — `enforce`), обычный создан, но с предупреждением `restricted` (`warn`). Предупреждение показывает, что именно нужно исправить.

## Ошибки новичка

- ❌ Считать, что без политик сеть «закрыта по умолчанию» → ✅ она открыта; закрывается `default-deny`.
- ❌ Создать `default-deny` и забыть разрешить нужный трафик → ✅ сначала карта связей, затем политики.
- ❌ `readOnlyRootFilesystem: true` без `emptyDir` для `/tmp` → ✅ смонтировать том для записи.
- ❌ `runAsNonRoot: true` для образа, который работает от root → ✅ задать `runAsUser` или взять образ без root.
- ❌ Включить `enforce: restricted` без проверки существующих приложений → ✅ сначала `warn`/`audit`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — namespaceSelector, egress, режимы PSA, ValidatingAdmissionPolicy
- Сеть и Service: [06 · Service и сеть](../06-services/overview.ru.md)
- Права доступа к API: [10 · Namespaces и RBAC](../10-namespaces-and-rbac/overview.ru.md)
- Pod и контейнеры: [04 · Pod](../04-pods/overview.ru.md)

## Источники

- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Configure a Security Context for a Pod or Container](https://kubernetes.io/docs/tasks/configure-pod-container/security-context/)
- [Pod Security Admission](https://kubernetes.io/docs/concepts/security/pod-security-admission/)
- [Pod Security Standards](https://kubernetes.io/docs/concepts/security/pod-security-standards/)
