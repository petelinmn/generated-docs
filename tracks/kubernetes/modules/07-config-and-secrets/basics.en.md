---
title: "ConfigMap and Secret: basics"
lang: en
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

# ConfigMap and Secret: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-07`: `kubectl create namespace learn-07`.

## Why it matters

An image is built once and run in different places: development, tests, production. Database addresses, passwords and flags must not be "baked" into the image. ConfigMap and Secret let you change settings without rebuilding the image and keep them apart from code.

## ConfigMap

### Creation

Three ways: from values, from a file, from a variables file.

```text
$ kubectl -n learn-07 create configmap app-config --from-literal=MODE=prod --from-literal=TIMEOUT=30 --from-file=nginx.conf
configmap/app-config created
$ kubectl -n learn-07 get cm app-config -o jsonpath='{.data}'
{"MODE":"prod","TIMEOUT":"30","nginx.conf":"server { listen 80; }\n"}
```

`--from-file` puts the file content under a key equal to the file name (`nginx.conf`). For an `app.env` file with `KEY=value` pairs:

```text
$ kubectl -n learn-07 create configmap from-env --from-env-file=app.env
$ kubectl -n learn-07 get cm from-env -o jsonpath='{.data}'
{"GREETING":"hello","LOG_LEVEL":"debug"}
```

`--from-env-file` cannot be mixed with `--from-file` and `--from-literal`:

```text
error: from-env-file cannot be combined with from-file or from-literal
```

As a manifest a ConfigMap looks like this:

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

Values are always strings: the number `30` must be quoted. Key names are Latin letters, digits, `-`, `_`, `.`:

```text
error: "bad key" is not a valid key name for a ConfigMap: a valid config key must consist of alphanumeric characters, '-', '_' or '.' …
```

## Secret

A Secret is created the same way, but values are stored in base64:

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

> ⚠️ base64 is an encoding, not encryption. Anyone who can read a Secret decodes the value with one command.

In a manifest the `stringData` field is more convenient: values are written as plain text, and the API stores them in `data` in base64.

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

`kubectl describe secret` does not show values, only sizes:

```text
$ kubectl -n learn-07 describe secret db
Data
====
password:  7 bytes
username:  5 bytes
```

The default type is `Opaque` (arbitrary data). A manifest with `stringData` cannot be stored in Git safely: the values lie as plain text (see the [expert theory](expert.en.md#external-secret-stores-and-alternatives)).

## How to attach to a pod

A pod that uses both objects in three ways:

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

- `env` + `valueFrom` — one value under the chosen variable name.
- `envFrom` — all keys of the object at once as variables; a `prefix` can be set.
- `volumes` + `volumeMounts` — every key becomes a file.

### Environment variables

```text
$ kubectl -n learn-07 exec app -- env | grep -E "^(APP_MODE|DB_PASSWORD|LOG_LEVEL|GREETING)="
GREETING=hello
LOG_LEVEL=debug
APP_MODE=prod
DB_PASSWORD=S3cr3t!
```

The variables from `envFrom` (`GREETING`, `LOG_LEVEL`) came from the ConfigMap `from-env`. A secret in a variable is also visible in `/proc/1/environ`: `cat /proc/1/environ | tr '\0' '\n'` inside the container shows `DB_PASSWORD=S3cr3t!`.

### Files

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

Each key is a file (in fact a symbolic link to `..data/<key>`; the trick is needed for atomic updates). In our kind the ConfigMap volume is mounted read-only (`ext4`), while the Secret volume is mounted as `tmpfs` (in memory), so the secret does not reach the node's disk:

```text
/dev/sdd on /etc/cfg type ext4 (ro,relatime)
tmpfs on /etc/sec type tmpfs (ro,relatime,size=32736456k,noswap)
```

## Beginner mistakes

- ❌ Considering a Secret encrypted → ✅ it is base64; restrict access with RBAC and encrypt etcd ([expert theory](expert.en.md#encrypting-secrets-in-etcd)).
- ❌ Writing a number in a ConfigMap without quotes (`TIMEOUT: 30`) → ✅ values are strings: `"30"`.
- ❌ Storing passwords in a ConfigMap → ✅ passwords and tokens go in a Secret.
- ❌ Expecting environment variables to update after editing a ConfigMap → ✅ variables are fixed at container start ([advanced](advanced.en.md#updating-values)).
- ❌ Committing a Secret manifest with `stringData` to Git → ✅ use an external secret store or encryption (Sealed Secrets, SOPS).

## Related materials

- Next: [advanced](advanced.en.md) — updating, `subPath`, `immutable`, missing references
- Next module: [08 · Storage](../08-storage/overview.en.md)
- Previous module: [06 · Service and networking](../06-services/overview.en.md)
- In other tracks: [DI, configuration and logging in C#](../../../csharp/modules/13-di-config-logging/overview.en.md)

## Sources

- [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)
- [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [Configure a Pod to Use a ConfigMap](https://kubernetes.io/docs/tasks/configure-pod-container/configure-pod-configmap/)
- [Distribute Credentials Securely Using Secrets](https://kubernetes.io/docs/tasks/inject-data-application/distribute-credentials-secure/)
- [Good practices for Kubernetes Secrets](https://kubernetes.io/docs/concepts/security/secrets-good-practices/)
