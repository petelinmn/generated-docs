---
title: "ConfigMap и Secret: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, etcd, encryption-at-rest, rbac, secret-types, external-secrets, sealed-secrets, tmpfs]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# ConfigMap и Secret: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Как данные попадают в контейнер

- **Переменные окружения:** kubelet читает объект из API при создании контейнера и передаёт значения рантайму. Они зафиксированы в конфигурации контейнера: поэтому не обновляются.
- **Том:** kubelet периодически сверяет состояние тома с объектом и пересобирает содержимое. Файлы лежат в каталоге вида `..2026_11_22_…`, а `..data` — символическая ссылка на актуальный каталог; её переключают атомарно, и читатель не увидит наполовину обновлённых файлов. Для `ConfigMap` том смонтирован из диска ноды (`ext4`, `ro`), для `Secret` — как `tmpfs`, то есть данные секрета не пишутся на диск ноды.
- **`subPath`:** монтируется конкретный файл, минуя символическую ссылку, поэтому обновления до него не доходят.

### Хранение в etcd и шифрование

Secret хранится в etcd в том же виде, что и остальные объекты (protobuf с префиксом `k8s`), и по умолчанию **без шифрования**. Прочитаем значение напрямую (с сертификатами etcd, как в [модуле 02](../02-architecture/expert.ru.md#как-api-server-хранит-данные)):

```text
$ kubectl -n kube-system exec etcd-learn-control-plane -- etcdctl --cacert=… --cert=… --key=… \
    get /registry/secrets/learn-07/db --print-value-only | tr -c '[:print:]' '.'
k8s.....v1..Secret........db....learn-07"…password..S3cr3t!....username..admin..Opaq…
```

Пароль `S3cr3t!` и логин `admin` лежат в открытом виде (даже не в base64: base64 — форма представления в API, внутри хранилища байты сырые). Флага `--encryption-provider-config` в манифесте `kube-apiserver` нашего kind нет:

```text
$ docker exec learn-control-plane sh -c 'grep -c encryption-provider-config /etc/kubernetes/manifests/kube-apiserver.yaml'
0
```

Любой с доступом к etcd или его резервным копиям получает все секреты кластера.

### Шифрование Secret в etcd

Шифрование включается конфигурацией `EncryptionConfiguration`, которую API server получает через флаг `--encryption-provider-config`: в ней указывают ресурсы (`secrets`) и провайдеры (`aescbc`, `secretbox`, `kms` и другие). После включения существующие секреты нужно перезаписать, чтобы они зашифровались. Управляемые облачные Kubernetes обычно предлагают шифрование через KMS.

> ⚠️ Проверить: настройка `EncryptionConfiguration`, список провайдеров и процедура перезаписи секретов не запускались в этом материале; сверяйте с [документацией](https://kubernetes.io/docs/tasks/administer-cluster/encrypt-data/).

### Доступ: RBAC

Право читать Secret — отдельное право на ресурс `secrets` (глаголы `get`, `list`, `watch`). Запрос `list` отдаёт все значения. Обычный ServiceAccount его не имеет:

```text
$ kubectl -n learn-07 auth can-i get secrets --as=system:serviceaccount:learn-07:default
no
```

Но любой, кто может **создать под** в namespace, может смонтировать любой Secret этого namespace и прочитать его. Поэтому права на создание подов тоже нужно ограничивать (RBAC — модуль 10, Pod Security — модуль 17).

## Справочник

### Поля объектов

| Объект | Поля верхнего уровня (из `kubectl explain`) |
|---|---|
| ConfigMap | `data`, `binaryData`, `immutable` |
| Secret | `data` (base64), `stringData` (только запись), `type`, `immutable` |

Ограничения:

| Параметр | Значение | Проверено |
|---|---|---|
| Размер объекта | не более 1 МиБ (`1048576` байт) | да: файл 1.1 МБ отклонён `Too long: may not be more than 1048576 bytes` |
| Имя ключа | `[-._a-zA-Z0-9]+` | да: ключ `bad key` отклонён |
| Значения ConfigMap | строки (`data`); бинарные данные — `binaryData` | да |

### Способы подключения

| Способ | Поле | Обновление | Видимость |
|---|---|---|---|
| одна переменная | `env[].valueFrom.configMapKeyRef` / `secretKeyRef` | нет | `/proc/<pid>/environ` |
| все ключи как переменные | `envFrom[].configMapRef` / `secretRef` (+ `prefix`) | нет | `/proc/<pid>/environ` |
| том | `volumes[].configMap` / `secret` (+ `items`, `defaultMode`, `optional`) | да (~минута) | файловая система контейнера |
| том + `subPath` | `volumeMounts[].subPath` | нет | файл |

`optional: true` разрешает отсутствие объекта или ключа. С префиксом `envFrom` даёт переменные вида `CFG_good`; ключ `bad-name` попал как `CFG_bad-name` (имя с дефисом допустимо для контейнера, но неудобно для оболочки).

### Типы Secret

| Тип | Назначение | Обязательные ключи | Проверено |
|---|---|---|---|
| `Opaque` | произвольные данные (по умолчанию) | — | да |
| `kubernetes.io/dockerconfigjson` | доступ к реестру образов (`imagePullSecrets`) | `.dockerconfigjson` | да |
| `kubernetes.io/tls` | пара сертификат и ключ | `tls.crt`, `tls.key` | да: без ключей объект отклонён |
| `kubernetes.io/basic-auth` | логин и пароль | `username` или `password` | нет |
| `kubernetes.io/ssh-auth` | SSH-ключ | `ssh-privatekey` | нет |
| `kubernetes.io/service-account-token` | токен ServiceAccount | — | нет |
| `bootstrap.kubernetes.io/token` | токен присоединения нод | — | нет |

> ⚠️ Проверить: типы `basic-auth`, `ssh-auth`, `service-account-token`, `bootstrap.kubernetes.io/token` и их обязательные ключи даны по документации и не создавались в этом материале.

## Подвохи

### Подвох: Secret в etcd лежит открытым текстом

Опыт выше: `S3cr3t!` в значении ключа etcd. base64 в `kubectl get secret -o yaml` — только представление в API. Защита — RBAC, шифрование etcd и защита резервных копий.

### Подвох: base64 расшифровывает любой, кто читает Secret

```text
$ kubectl -n learn-07 get secret db -o jsonpath='{.data.password}' | base64 -d
S3cr3t!
```

`kubectl describe secret` значения скрывает, а `get -o yaml` — нет.

### Подвох: секрет в переменной виден в /proc

```text
$ kubectl -n learn-07 exec app -- sh -c 'cat /proc/1/environ | tr "\0" "\n" | grep DB_PASSWORD'
DB_PASSWORD=S3cr3t!
```

Переменные доступны всем процессам контейнера, наследуются дочерними процессами и часто попадают в сообщения об ошибках и дампы. Файл с правами `0400` безопаснее.

### Подвох: правка ConfigMap не обновляет работающее приложение

Переменные не обновляются вообще, файлы обновляются с задержкой (в опыте ~57 с), а `subPath` не обновляется никогда ([продвинутая теория](advanced.ru.md#обновление-значений)). Deployment после правки не перезапускается (`RS count before=2 after=2`).

### Подвох: CreateContainerConfigError вместо понятной ошибки

Отсутствующий ConfigMap или ключ даёт статус `CreateContainerConfigError`, а причина — в событиях (`kubectl describe pod`). Под сам стартует через ~20 секунд после создания недостающего объекта (в опыте).

### Подвох: тип kubernetes.io/tls без ключей отклоняется

`error: failed to create secret Secret "bad" is invalid: [data[tls.crt]: Required value, data[tls.key]: Required value]`. Проверка выполняется на стороне API по типу.

### Подвох: объект больше 1 МиБ

```text
error: failed to create configmap: ConfigMap "huge" is invalid: []: Too long: may not be more than 1048576 bytes
```

Большие файлы не помещаются: используйте том (PersistentVolume), образ или внешнее хранилище.

### Подвох: `--from-env-file` нельзя смешивать с другими источниками

```text
error: from-env-file cannot be combined with from-file or from-literal
```

## Внешние хранилища секретов и альтернативы

| Подход | Идея | Компромисс |
|---|---|---|
| Secret + RBAC + шифрование etcd | встроенный механизм | секрет в etcd и в манифесте |
| Sealed Secrets | секрет шифруется публичным ключом кластера и безопасно лежит в Git | нужен контроллер, ключ привязан к кластеру |
| SOPS | шифрование значений в файлах манифестов | нужен ключ (KMS, age) и интеграция с GitOps |
| External Secrets Operator | синхронизирует Secret из внешнего хранилища (Vault, облачные менеджеры секретов) | зависимость от внешней системы |
| Secrets Store CSI Driver | монтирует секреты из внешнего хранилища как том | драйвер на нодах |
| Vault Agent и аналоги | sidecar получает секреты и динамические креды | сложнее эксплуатация |

> ⚠️ Проверить: Sealed Secrets, SOPS, External Secrets Operator, Secrets Store CSI Driver и Vault описаны по документации и не запускались в этом материале.

## Вопросы с собеседований

**1. Чем ConfigMap отличается от Secret?**

<details><summary>Ответ</summary>

Назначением и мерами предосторожности: ConfigMap — обычные настройки, Secret — чувствительные данные. Технически Secret — тоже пары ключ-значение, но значения хранятся в base64, `describe` их скрывает, том монтируется как `tmpfs`, а доступ к ним ограничивают отдельно (RBAC на `secrets`) и могут шифровать в etcd.

</details>

**2. (С подвохом) Secret зашифрован?**

<details><summary>Ответ</summary>

Нет. base64 — кодировка, декодируется одной командой. В etcd Secret по умолчанию лежит открытым текстом (в опыте `password..S3cr3t!` в значении ключа), флага `--encryption-provider-config` в kind нет. Нужно шифрование etcd (`EncryptionConfiguration`, KMS), RBAC и защита резервных копий.

</details>

**3. Какими способами ConfigMap попадает в под и что из них обновляется?**

<details><summary>Ответ</summary>

Переменные окружения (`env`, `envFrom`) и тома. Переменные фиксируются при старте и не обновляются; файлы тома обновляются примерно через минуту (в опыте 57 с); том с `subPath` не обновляется.

</details>

**4. (С подвохом) Вы изменили ConfigMap, а Deployment продолжает работать со старыми значениями. Почему и что делать?**

<details><summary>Ответ</summary>

Изменение ConfigMap не меняет шаблон пода, rollout не запускается (в опыте число ReplicaSet не изменилось). Нужно `kubectl rollout restart`, хэш ConfigMap в аннотации шаблона (`checksum/config` в Helm) или новое имя ConfigMap при каждом изменении.

</details>

**5. Что будет, если под ссылается на несуществующий ConfigMap или ключ?**

<details><summary>Ответ</summary>

Статус `CreateContainerConfigError` (события: `configmap "nope" not found`, `couldn't find key nokey in ConfigMap …`). Под стартует сам после появления объекта (в опыте за ~20 с). Для необязательных ссылок есть `optional: true`.

</details>

**6. Для чего `immutable: true`?**

<details><summary>Ответ</summary>

Запрещает менять `data` (ошибка `field is immutable when immutable is set`), защищает от случайных правок и снижает нагрузку на API server. Снять флаг нельзя; изменения вносят новым объектом (с новым именем).

</details>

**7. Почему секреты лучше монтировать файлами, чем переменными?**

<details><summary>Ответ</summary>

Переменные видны в `/proc/<pid>/environ`, наследуются дочерними процессами, часто попадают в логи и дампы. Файл можно ограничить правами (`defaultMode: 0400`), том Secret — `tmpfs` (не на диске ноды), файлы обновляются без перезапуска.

</details>

**8. Кто может прочитать Secret?**

<details><summary>Ответ</summary>

Тот, у кого есть право `get`/`list` на `secrets`, а также любой, кто может создать под в namespace и смонтировать Secret, и тот, кто имеет доступ к etcd. В опыте `can-i get secrets` у `default` ServiceAccount — `no`.

</details>

**9. Какой предел размера ConfigMap и Secret?**

<details><summary>Ответ</summary>

1 МиБ (1048576 байт) на объект: в опыте 1.1 МБ отклонён `Too long: may not be more than 1048576 bytes`.

</details>

**10. (С подвохом) Что такое `stringData` и сохраняется ли оно в таком виде?**

<details><summary>Ответ</summary>

Поле только для записи: значения даются открытым текстом, а API сохраняет их в `data` в base64 (в опыте `cGxhaW4tdGV4dC1pbi1tYW5pZmVzdA==`). В манифесте в Git значения остаются открытым текстом, поэтому такой файл нельзя хранить в репозитории без шифрования.

</details>

**11. Как безопасно хранить секреты в Git?**

<details><summary>Ответ</summary>

Не хранить в открытом виде. Варианты: Sealed Secrets (шифрование ключом кластера), SOPS (шифрование значений), либо внешнее хранилище (External Secrets Operator, Secrets Store CSI Driver), где в Git лежит только ссылка.

</details>

**12. Как приложение на .NET читает значения из ConfigMap?**

<details><summary>Ответ</summary>

Через переменные окружения (вложенные ключи — `Section__Key`) или файлы, смонтированные томом, которые подключают как источник конфигурации; подробности — в [модуле C# про конфигурацию](../../../csharp/modules/13-di-config-logging/overview.ru.md).

</details>

## Шпаргалка

```bash
kubectl create configmap NAME --from-literal=K=V --from-file=FILE   # не смешивать с --from-env-file
kubectl create secret generic NAME --from-literal=K=V
kubectl create secret tls NAME --cert=tls.crt --key=tls.key
kubectl create secret docker-registry NAME --docker-server=… --docker-username=… --docker-password=…
kubectl get secret NAME -o jsonpath='{.data.K}' | base64 -d
kubectl describe secret NAME                  # значения скрыты
kubectl set env deploy/X --from=configmap/NAME
kubectl rollout restart deploy/X              # подхватить новые значения
kubectl auth can-i get secrets --as=system:serviceaccount:NS:SA
```

## Связанные материалы

- Предыдущие уровни: [базовая](basics.ru.md), [продвинутая](advanced.ru.md)
- Следующий модуль: [08 · Хранилище](../08-storage/overview.ru.md)
- Предыдущие модули: [05 · Deployment](../05-deployments/overview.ru.md), [02 · Архитектура](../02-architecture/overview.ru.md)
- В других треках: [DI, конфигурация и логирование в C#](../../../csharp/modules/13-di-config-logging/overview.ru.md)

## Источники

- [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [Good practices for Kubernetes Secrets](https://kubernetes.io/docs/concepts/security/secrets-good-practices/)
- [Encrypting Confidential Data at Rest](https://kubernetes.io/docs/tasks/administer-cluster/encrypt-data/)
- [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)
- [Using RBAC Authorization](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Secret types](https://kubernetes.io/docs/concepts/configuration/secret/#secret-types)
