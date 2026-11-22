---
title: "ConfigMap и Secret: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, environment-variables, volume, base64]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# ConfigMap и Secret: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-07`: `kubectl create namespace learn-07`.

## Зачем это нужно

Образ собирают один раз, а запускают в разных местах: в разработке, тестах, продакшене. Адреса баз данных, пароли и флаги в образ «зашивать» нельзя. ConfigMap и Secret позволяют менять настройки, не пересобирая образ, и хранить их отдельно от кода.

## ConfigMap

### Создание

Три способа: из значений, из файла, из файла переменных.

```text
$ kubectl -n learn-07 create configmap app-config --from-literal=MODE=prod --from-literal=TIMEOUT=30 --from-file=nginx.conf
configmap/app-config created
$ kubectl -n learn-07 get cm app-config -o jsonpath='{.data}'
{"MODE":"prod","TIMEOUT":"30","nginx.conf":"server { listen 80; }\n"}
```

`--from-file` кладёт содержимое файла под ключ, равный имени файла (`nginx.conf`). Для файла `app.env` с парами `KEY=value`:

```text
$ kubectl -n learn-07 create configmap from-env --from-env-file=app.env
$ kubectl -n learn-07 get cm from-env -o jsonpath='{.data}'
{"GREETING":"hello","LOG_LEVEL":"debug"}
```

`--from-env-file` нельзя смешивать с `--from-file` и `--from-literal`:

```text
error: from-env-file cannot be combined with from-file or from-literal
```

В виде манифеста ConfigMap выглядит так:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
data:
  MODE: prod
  TIMEOUT: "30"
  nginx.conf: |
    server { listen 80; }
```

Значения всегда строки: число `30` нужно писать в кавычках. Имена ключей — латинские буквы, цифры, `-`, `_`, `.`:

```text
error: "bad key" is not a valid key name for a ConfigMap: a valid config key must consist of alphanumeric characters, '-', '_' or '.' …
```

## Secret

Secret создаётся так же, но значения хранятся в base64:

```text
$ kubectl -n learn-07 create secret generic db --from-literal=username=admin --from-literal=password='S3cr3t!'
secret/db created
$ kubectl -n learn-07 get secret db -o yaml
data:
  password: UzNjcjN0IQ==
  username: YWRtaW4=
$ kubectl -n learn-07 get secret db -o jsonpath='{.data.password}' | base64 -d
S3cr3t!
```

> ⚠️ base64 — это кодировка, а не шифрование. Любой, кто может прочитать Secret, декодирует значение одной командой.

В манифесте удобнее поле `stringData`: значения пишутся открытым текстом, а API сохраняет их в `data` в base64.

```yaml
apiVersion: v1
kind: Secret
metadata: {name: db2}
stringData:
  token: plain-text-in-manifest
```

```text
$ kubectl -n learn-07 get secret db2 -o jsonpath='{.data.token}'
cGxhaW4tdGV4dC1pbi1tYW5pZmVzdA==
```

`kubectl describe secret` не показывает значения, только размеры:

```text
$ kubectl -n learn-07 describe secret db
Data
====
password:  7 bytes
username:  5 bytes
```

Тип по умолчанию — `Opaque` (произвольные данные). Манифест с `stringData` нельзя безопасно хранить в Git: значения лежат открытым текстом (см. [экспертную теорию](expert.ru.md#внешние-хранилища-секретов-и-альтернативы)).

## Как подключить к поду

Под, который использует оба объекта и тремя способами:

```yaml
apiVersion: v1
kind: Pod
metadata: {name: app}
spec:
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "sleep 3600"]
      env:
        - name: APP_MODE
          valueFrom: {configMapKeyRef: {name: app-config, key: MODE}}
        - name: DB_PASSWORD
          valueFrom: {secretKeyRef: {name: db, key: password}}
      envFrom:
        - configMapRef: {name: from-env}
      volumeMounts:
        - {name: cfg, mountPath: /etc/cfg}
        - {name: sec, mountPath: /etc/sec, readOnly: true}
  volumes:
    - name: cfg
      configMap: {name: app-config}
    - name: sec
      secret: {secretName: db}
```

- `env` + `valueFrom` — одно значение под нужным именем переменной.
- `envFrom` — все ключи объекта сразу как переменные; можно задать `prefix`.
- `volumes` + `volumeMounts` — каждый ключ становится файлом.

### Переменные окружения

```text
$ kubectl -n learn-07 exec app -- env | grep -E "^(APP_MODE|DB_PASSWORD|LOG_LEVEL|GREETING)="
GREETING=hello
LOG_LEVEL=debug
APP_MODE=prod
DB_PASSWORD=S3cr3t!
```

Переменные из `envFrom` (`GREETING`, `LOG_LEVEL`) пришли из ConfigMap `from-env`. Секрет в переменной виден и в `/proc/1/environ`: `cat /proc/1/environ | tr '\0' '\n'` внутри контейнера показывает `DB_PASSWORD=S3cr3t!`.

### Файлы

```text
$ kubectl -n learn-07 exec app -- ls -la /etc/cfg
drwxrwxrwx .
drwxr-xr-x ..
drwxr-xr-x ..2026_11_22_18_24_28.326056211
lrwxrwxrwx ..data -> ..2026_11_22_18_24_28.326056211
lrwxrwxrwx MODE -> ..data/MODE
lrwxrwxrwx TIMEOUT -> ..data/TIMEOUT
lrwxrwxrwx nginx.conf -> ..data/nginx.conf
$ kubectl -n learn-07 exec app -- cat /etc/cfg/MODE
prod
```

Каждый ключ — файл (на самом деле символическая ссылка на `..data/<ключ>`; приём нужен для атомарного обновления). В нашем kind том ConfigMap смонтирован только для чтения (`ext4`), а том Secret — как `tmpfs` (в памяти), чтобы секрет не попадал на диск ноды:

```text
/dev/sdd on /etc/cfg type ext4 (ro,relatime)
tmpfs on /etc/sec type tmpfs (ro,relatime,size=32736456k,noswap)
```

## Ошибки новичка

- ❌ Считать Secret шифрованным → ✅ это base64; ограничивайте доступ RBAC и шифруйте etcd ([экспертная теория](expert.ru.md#шифрование-secret-в-etcd)).
- ❌ Писать число в ConfigMap без кавычек (`TIMEOUT: 30`) → ✅ значения строки: `"30"`.
- ❌ Хранить пароли в ConfigMap → ✅ пароли и токены — в Secret.
- ❌ Ждать, что переменные окружения обновятся после правки ConfigMap → ✅ переменные фиксируются при старте контейнера ([продвинутая теория](advanced.ru.md#обновление-значений)).
- ❌ Коммитить манифест Secret с `stringData` в Git → ✅ использовать внешнее хранилище секретов или шифрование (Sealed Secrets, SOPS).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — обновление, `subPath`, `immutable`, отсутствующие ссылки
- Следующий модуль: [08 · Хранилище](../08-storage/overview.ru.md)
- Предыдущий модуль: [06 · Service и сеть](../06-services/overview.ru.md)
- В других треках: [DI, конфигурация и логирование в C#](../../../csharp/modules/13-di-config-logging/overview.ru.md)

## Источники

- [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)
- [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [Configure a Pod to Use a ConfigMap](https://kubernetes.io/docs/tasks/configure-pod-container/configure-pod-configmap/)
- [Distribute Credentials Securely Using Secrets](https://kubernetes.io/docs/tasks/inject-data-application/distribute-credentials-secure/)
- [Good practices for Kubernetes Secrets](https://kubernetes.io/docs/concepts/security/secrets-good-practices/)
