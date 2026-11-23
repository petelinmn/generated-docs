---
title: "Безопасность подов и сети: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 17-security
part: 3
tags: [networkpolicy, pod-security-admission, pod-security-standards, securitycontext, validatingadmissionpolicy, cel, seccomp]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Безопасность подов и сети: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что подтверждено опытом в kind (Kubernetes 1.37.0) — помечено «в опыте».

## Справочник

### Поля `securityContext`

| Поле | Где | Смысл |
|---|---|---|
| `runAsUser`, `runAsGroup` | под, контейнер | числовые uid и gid процесса (в опыте `uid=10001 gid=10001`) |
| `runAsNonRoot` | под, контейнер | kubelet отказывается запускать контейнер от root (в опыте `CreateContainerConfigError`) |
| `fsGroup` | под | группа-владелец томов |
| `seccompProfile` | под, контейнер | `RuntimeDefault`, `Localhost`, `Unconfined` |
| `allowPrivilegeEscalation` | контейнер | запрет получения прав выше родительского процесса (в опыте `NoNewPrivs: 1`) |
| `readOnlyRootFilesystem` | контейнер | корневая ФС только для чтения (в опыте `Read-only file system`) |
| `capabilities.add/drop` | контейнер | добавить и убрать capabilities (в опыте `CapEff`) |
| `privileged` | контейнер | привилегированный режим (в опыте отклонён уровнем `baseline`) |

> ⚠️ Проверить: поля `supplementalGroups`, `sysctls`, `seLinuxOptions`, `procMount`, `fsGroupChangePolicy` и значения `Localhost`/`Unconfined` профиля seccomp взяты из документации и не запускались.

### Метки Pod Security Admission

| Метка namespace | Значение |
|---|---|
| `pod-security.kubernetes.io/enforce` | уровень, при нарушении которого под отклоняется |
| `pod-security.kubernetes.io/warn` | уровень, при нарушении которого выдаётся предупреждение |
| `pod-security.kubernetes.io/audit` | уровень, нарушения которого пишутся в аудит-лог |
| `…/<режим>-version` | версия стандарта (по умолчанию `latest`, в опыте `baseline:latest`) |

Уровни: `privileged` → `baseline` → `restricted` (от мягкого к жёсткому).

В опыте `baseline` отклонил `privileged: true` и `hostNetwork`. `restricted` потребовал: `allowPrivilegeEscalation=false`, `capabilities.drop=["ALL"]`, `runAsNonRoot=true`, `seccompProfile.type` равный `RuntimeDefault` или `Localhost`.

> ⚠️ Проверить: полный список контролей каждого уровня (hostPath, hostPort, допустимые capabilities, типы томов, sysctls и т. д.), метки версий и общекластерная настройка исключений (`AdmissionConfiguration`) не запускались.

### Поля NetworkPolicy

| Поле | Смысл |
|---|---|
| `spec.podSelector` | какие поды выбирает политика (пустой — все в namespace) |
| `spec.policyTypes` | `Ingress`, `Egress` или оба; если не указано, определяется наличием разделов |
| `ingress[].from[]` / `egress[].to[]` | `podSelector`, `namespaceSelector`, `ipBlock` |
| `ingress[].ports[]` / `egress[].ports[]` | `protocol`, `port`, необязательный `endPort` |

В опыте проверены `podSelector`, `namespaceSelector` (в одном элементе и в двух), `ports` (TCP, UDP). Объект `NetworkPolicy` относится к namespace.

### Команды

| Команда | Назначение |
|---|---|
| `kubectl get netpol` | политики namespace (`POD-SELECTOR`, `AGE`) |
| `kubectl label ns N pod-security.kubernetes.io/enforce=restricted` | включить PSA |
| `kubectl label --dry-run=server --overwrite ns N …` | проверить, какие существующие поды нарушат уровень |
| `kubectl exec P -- grep CapEff /proc/1/status` | проверить фактические права процесса (также `NoNewPrivs`, `Seccomp`) |
| `kubectl get validatingadmissionpolicy,validatingadmissionpolicybinding` | политики CEL и привязки |

## Внутреннее устройство

- **NetworkPolicy — только объект API.** Сам API-сервер трафик не фильтрует. Политики читает сетевой плагин (CNI) и превращает в правила на нодах. В kind это kindnet — в опыте он их исполнил. С плагином без этой функции политики создаются, но не действуют.
- **Семантика.** Под без выбирающей политики открыт. Под, выбранный хотя бы одной политикой данного направления, получает «запрещено всё, кроме разрешённого объединением правил». Политик нет «запретительных»: только разрешающие и «пустые» (default deny).
- **PSA — встроенный admission-контроллер.** Он читает метки namespace и проверяет поды при создании (и обновлении). Для Deployment, Job и других контроллеров режимы `warn` и `audit` срабатывают на шаблон пода, а `enforce` — на реальный под, который создаёт ReplicaSet. Поэтому отказ виден в событиях ReplicaSet (в опыте `FailedCreate … violates PodSecurity "restricted:latest"`).
- **ValidatingAdmissionPolicy выполняется внутри API-сервера** без внешнего webhook: выражение CEL получает объект как `object` и возвращает `true` (допустимо) или `false` (отказ с `message`). `ValidatingAdmissionPolicyBinding` задаёт область (`namespaceSelector`, `objectSelector`) и действия (`validationActions`: `Deny`, `Warn`, `Audit`). В опыте проверен `Deny`.
- **Токен ServiceAccount** — projected volume с токеном, привязанным к поду; `automountServiceAccountToken: false` убирает том (в опыте).
- **Процесс и ядро.** `securityContext` превращается в параметры контейнерного runtime: uid/gid, набор capabilities, флаг `no_new_privs`, профиль seccomp. Проверить результат можно в `/proc/1/status` контейнера (`CapEff`, `NoNewPrivs`, `Seccomp`). Подробнее о capabilities и seccomp — в [треке Linux](../../../linux/overview.ru.md).

> ⚠️ Проверить: поведение NetworkPolicy в кластере с CNI без поддержки политик не проверялось. Действия `Warn` и `Audit` у ValidatingAdmissionPolicyBinding взяты из документации.

## Подвохи

### 1. «И» и «ИЛИ» в `from`

Один элемент с `namespaceSelector` и `podSelector` — пересечение; два элемента — объединение. В опыте разница между вариантами изменила результат для `client-ok` и `outsider`.

### 2. Egress без DNS

`policyTypes: [Egress]` без правил блокирует и DNS: `nslookup` — `connection timed out; no servers could be reached`. К политике нужно добавить правило к `kube-dns` (UDP/TCP 53).

### 3. `enforce` не трогает работающие поды

Смена метки показывает `Warning: existing pods … violate`, но поды остаются. Нарушение проявится при следующем создании (рестарте Deployment, перепланировании).

### 4. Причина отказа в ReplicaSet, а не в Deployment

`kubectl rollout status` зависает, `kubectl get deploy` показывает `0/1`, а причина — в событиях ReplicaSet. Ищите `FailedCreate` и `violates PodSecurity`.

### 5. `runAsNonRoot` и образ с root

`runAsNonRoot: true` проверяется kubelet'ом: образ без `USER` стартует от root, и контейнер не запускается (`CreateContainerConfigError`: `container has runAsNonRoot and image will run as root`). Решение — `runAsUser` или другой образ.

### 6. `readOnlyRootFilesystem` и пути записи

Приложение, пишущее во временные каталоги, упадёт. В опыте `touch /x` дал `Read-only file system`, а `/tmp` на `emptyDir` — работал.

### 7. Политики только разрешают

Нельзя «разрешить всё, кроме одного». Для этого нужно перечислить разрешённое. Исключение — `ipBlock.except` внутри одного блока.

### 8. Namespace без меток PSA — как `privileged`

В `learn-17` меток не было, и поды работали от root с полным набором capabilities (в опыте). Защита включается только метками (или настройкой кластера).

### 9. Сообщение о нарушении содержит версию

`baseline:latest` и `restricted:latest` в сообщениях означают, что используется последняя версия стандарта для данной версии Kubernetes. После обновления кластера требования могут измениться, поэтому в production часто фиксируют версию (`…-version`).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| NetworkPolicy | стандартный API, переносимо | зависит от CNI; L3/L4; без явного запрета |
| CNI-расширения (Calico, Cilium и др.) | L7, глобальные политики, явный deny | привязка к плагину; не запускались |
| PSA | встроен, простые уровни | три уровня, нет произвольных правил |
| ValidatingAdmissionPolicy | свои правила без webhook | CEL нужно осваивать |
| OPA Gatekeeper, Kyverno | богатые политики, мутации, отчёты | отдельная установка; не запускались |
| Service mesh (mTLS) | шифрование и идентичность сервисов | сложность; не запускался |

## Вопросы с собеседований

<details><summary>1. Что произойдёт без единой NetworkPolicy?</summary>

Весь трафик разрешён: любой под достучится до любого. Сеть закрывается политиками.

→ [Базовая теория](basics.ru.md#networkpolicy-ограничиваем-сеть)

</details>

<details><summary>2. Как сделать default deny для входящего трафика?</summary>

Политика с пустым `podSelector` и `policyTypes: [Ingress]` без правил `ingress`.

→ [Базовая теория](basics.ru.md#default-deny)

</details>

<details><summary>3. Ловушка: две политики разрешают разное. Что разрешено?</summary>

Объединение обоих разрешений: политики только добавляют разрешения.

→ [Подвох 7](#7-политики-только-разрешают)

</details>

<details><summary>4. Чем отличаются два варианта записи <code>from</code> с <code>namespaceSelector</code> и <code>podSelector</code>?</summary>

В одном элементе — «И», в двух отдельных — «ИЛИ».

→ [Продвинутая теория](advanced.ru.md#networkpolicy-from-namespaceselector-и-и-против-или)

</details>

<details><summary>5. Почему после egress <code>default-deny</code> перестали резолвиться имена?</summary>

Заблокированы запросы к DNS (CoreDNS). Нужно разрешить UDP/TCP 53 к `kube-dns`.

→ [Подвох 2](#2-egress-без-dns)

</details>

<details><summary>6. Чем отличаются <code>enforce</code>, <code>warn</code> и <code>audit</code> в PSA?</summary>

`enforce` отклоняет под, `warn` показывает предупреждение клиенту, `audit` пишет событие в аудит.

→ [Базовая теория](basics.ru.md#pod-security-admission-контроль-на-входе)

</details>

<details><summary>7. Ловушка: <code>enforce: restricted</code> включён, а Deployment создаётся без ошибок. Почему?</summary>

Отклоняются поды, а не Deployment. ReplicaSet не может создать поды (`FailedCreate`); Deployment сам остаётся «созданным».

→ [Подвох 4](#4-причина-отказа-в-replicaset-а-не-в-deployment)

</details>

<details><summary>8. Что будет с уже работающими подами после включения <code>enforce</code>?</summary>

Ничего: проверяется создание. Предупреждение при смене метки покажет нарушителей.

→ [Подвох 3](#3-enforce-не-трогает-работающие-поды)

</details>

<details><summary>9. Какие поля делают под «restricted»?</summary>

`allowPrivilegeEscalation: false`, `capabilities.drop: [ALL]`, `runAsNonRoot: true`, `seccompProfile: RuntimeDefault` (в опыте перечислены сообщением).

→ [Справочник](#метки-pod-security-admission)

</details>

<details><summary>10. Зачем <code>readOnlyRootFilesystem</code> и что с ним делать приложению?</summary>

Снижает ущерб при взломе: нельзя записать файлы в образ. Для записи монтируют тома (`emptyDir`).

→ [Подвох 6](#6-readonlyrootfilesystem-и-пути-записи)

</details>

<details><summary>11. Как отключить токен ServiceAccount у пода?</summary>

`automountServiceAccountToken: false` в поде или в ServiceAccount.

→ [Продвинутая теория](advanced.ru.md#токен-serviceaccount)

</details>

<details><summary>12. Как запретить образы с тегом <code>latest</code> без внешних компонентов?</summary>

ValidatingAdmissionPolicy с выражением CEL и привязкой к namespace.

→ [Продвинутая теория](advanced.ru.md#validatingadmissionpolicy-свои-правила)

</details>

<details><summary>13. Ловушка: NetworkPolicy создана, но ничего не блокирует. Почему?</summary>

Возможные причины: политика не выбирает под (`podSelector`), нужное направление не указано в `policyTypes`, либо сетевой плагин не поддерживает политики.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

## Шпаргалка

```text
kubectl get netpol
kubectl label ns N pod-security.kubernetes.io/{enforce,warn,audit}=baseline|restricted
kubectl label --dry-run=server --overwrite ns N pod-security.kubernetes.io/enforce=restricted
kubectl exec P -- grep -E "CapEff|NoNewPrivs|Seccomp" /proc/1/status
```

- Сеть открыта, пока под не выбран политикой; политики только разрешают.
- В `from`: один элемент — «И», два — «ИЛИ».
- Egress → не забыть DNS.
- PSA проверяет создание подов; смотреть события ReplicaSet.
- В поде: non-root, без escalation, `drop: [ALL]`, seccomp, read-only rootfs.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- RBAC: [10 · Namespaces и RBAC](../10-namespaces-and-rbac/expert.ru.md)
- Linux: [трек Linux](../../../linux/overview.ru.md)
- Планирование: [18 · Планирование подов](../18-scheduling/overview.ru.md)

## Источники

- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Pod Security Standards](https://kubernetes.io/docs/concepts/security/pod-security-standards/)
- [Pod Security Admission](https://kubernetes.io/docs/concepts/security/pod-security-admission/)
- [Validating Admission Policy](https://kubernetes.io/docs/reference/access-authn-authz/validating-admission-policy/)
- [Security Context](https://kubernetes.io/docs/tasks/configure-pod-container/security-context/)
- [Service Accounts](https://kubernetes.io/docs/concepts/security/service-accounts/)
