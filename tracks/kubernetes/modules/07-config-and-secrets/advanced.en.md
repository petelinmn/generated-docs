---
title: "ConfigMap and Secret: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, subpath, immutable, optional, default-mode, secret-types, rollout, binarydata]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# ConfigMap and Secret: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): creating objects and the three ways to attach are not repeated.

## Updating values

Let us change the ConfigMap from which a pod reads both the variable `APP_MODE` and the file `/etc/cfg/MODE`, plus a second pod whose file is attached through `subPath`:

```yaml
volumeMounts:
  - {name: cfg, mountPath: /etc/mode.txt, subPath: MODE}
```

```text
$ kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"dev"}}'
volume updated to 'dev' after ~57s
env still: prod
subPath file: prod
```

| Way of attaching | Updates after editing the ConfigMap |
|---|---|
| a volume (files) | yes, in about a minute (the kubelet syncs periodically; 57 s in the experiment) |
| a volume with `subPath` | no, the old value stays |
| `env`, `envFrom` | no, only after the pod is recreated |

Files are updated atomically: the kubelet creates a new directory `..2026_…`, then switches the `..data` link (this is the link chain visible in `ls -la`). The application must notice the file change itself (re-read the config, watch the file). There is no instant update.

## A ConfigMap change does not restart pods

A Deployment that takes values from a ConfigMap through `envFrom` is not updated after the ConfigMap is edited:

```text
$ kubectl -n learn-07 set env deploy/web --from=configmap/app-config
$ kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"changed"}}'
RS count before=2 after=2
$ kubectl -n learn-07 exec deploy/web -- sh -c 'echo MODE=$MODE'
MODE=dev
```

No new ReplicaSet appeared, the pod kept the former value (`dev`). For pods to pick up the change you must change the pod template. Ways:

- `kubectl rollout restart deploy/web` ([module 05](../05-deployments/advanced.en.md#restarting-without-changing-the-image));
- write a hash of the ConfigMap content into a pod template annotation (Helm charts do this with `checksum/config`);
- give the ConfigMap a new name on every change (`app-config-v2`) and change the reference in the Deployment.

`kubectl set env --from` can "unroll" a ConfigMap into variables, but renames keys that are invalid as variable names:

```text
Warning: key nginx.conf transferred to NGINX_CONF
```

## Immutable objects

The field `immutable: true` forbids changing `data`. This protects from accidental edits and reduces the load on the API server (the kubelet does not watch such objects):

```text
$ kubectl -n learn-07 patch cm frozen -p '{"immutable":true}'
$ kubectl -n learn-07 patch cm frozen -p '{"data":{"a":"2"}}'
The ConfigMap "frozen" is invalid: data: Forbidden: field is immutable when `immutable` is set
```

The flag cannot be removed: the object must be deleted and created again. The practice: roll out immutable versions with a new name (`app-config-v2`) and change the reference in the Deployment: this is both a rollback and a controlled rollout.

> ⚠️ Verify: the statement that the kubelet does not watch immutable objects is given from documentation and was not reproduced.

## Missing objects and keys

If a ConfigMap or a key is missing, the container does not start. Three pods with `configMapKeyRef`:

```text
$ kubectl -n learn-07 get pods missing badkey optional
NAME       READY   STATUS                       RESTARTS   AGE
missing    0/1     CreateContainerConfigError   0          16s
badkey     0/1     CreateContainerConfigError   0          15s
optional   1/1     Running                      0          15s
$ kubectl -n learn-07 describe pod missing
  Warning  Failed  kubelet  Error: configmap "nope" not found
$ kubectl -n learn-07 describe pod badkey
  Warning  Failed  kubelet  Error: couldn't find key nokey in ConfigMap learn-07/app-config
```

- `missing` refers to a non-existent ConfigMap `nope`;
- `badkey` to an existing ConfigMap but a non-existent key;
- `optional` has `optional: true`: the variable is simply not set, the pod starts.

A pod in `CreateContainerConfigError` waits and starts by itself once the needed object appears (in the experiment, ~20 s after creating `nope`). For a volume `optional` is set in `configMap.optional` and `secret.optional`.

## File permissions, choosing keys and paths

For volumes you can choose a subset of keys and set paths and permissions:

```yaml
volumes:
  - name: sec
    secret:
      secretName: db
      defaultMode: 0400
      items:
        - {key: password, path: nested/pw.txt}
```

```text
$ kubectl -n learn-07 exec modes -- sh -c 'ls -lL /etc/sec/nested; ls /etc/sec'
-r--------    1 root     root             7 Nov 22 18:26 pw.txt
nested
```

The volume holds only the one chosen key (`username` is not attached), at the path `nested/pw.txt`, with permissions `0400` (`-r--------`). The default permissions (`defaultMode`) are `0644`; restrict them for secrets, and when not running as root take `fsGroup` into account ([module 17](../../overview.en.md#part-3-expert-topics)).

## Binary data

In a ConfigMap binary files are stored in the `binaryData` field (the value is base64):

```text
$ kubectl -n learn-07 create configmap bin --from-file=bin.dat
$ kubectl -n learn-07 get cm bin -o jsonpath='{.binaryData}'
{"bin.dat":"Yjp5YRak1Ws/dPo/ORayHg=="}
```

The tool detected on its own that the file is binary and put it into `binaryData`, not `data`.

## Secret types

The type sets the expected keys, and the API checks them:

```text
$ kubectl -n learn-07 create secret docker-registry regcred --docker-server=registry.example.com --docker-username=u --docker-password=p
$ kubectl -n learn-07 get secret regcred -o jsonpath='{.type}'
kubernetes.io/dockerconfigjson
$ kubectl -n learn-07 get secret regcred -o jsonpath='{.data.\.dockerconfigjson}' | base64 -d
{"auths":{"registry.example.com":{"username":"u","password":"p","auth":"dTpw"}}}
$ kubectl -n learn-07 create secret tls demo-tls --cert=tls.crt --key=tls.key
$ kubectl -n learn-07 create secret generic bad --type=kubernetes.io/tls --from-literal=a=b
error: failed to create secret Secret "bad" is invalid: [data[tls.crt]: Required value, data[tls.key]: Required value]
```

`docker-registry` creates a secret for pulling images from a private registry (attached through `imagePullSecrets`), `tls` — a certificate and key pair (for Ingress, module 09). The type `kubernetes.io/tls` requires the keys `tls.crt` and `tls.key`, otherwise the object is rejected.

## Best practices

- Split ConfigMaps by purpose and by application rather than piling everything into one.
- Mount secrets as files, not variables: variables are visible in `/proc/<pid>/environ` and often end up in logs and dumps.
- Give secret files restricted permissions (`defaultMode: 0400`) and `readOnly: true` on the mount.
- Use `immutable` and versioned names for configuration that must not change on the fly.
- Restrict RBAC: not everyone has `get`/`list` on secrets (in the experiment `auth can-i get secrets` for the default ServiceAccount is `no`).
- Do not commit secrets to Git in plain text.

## Common mistakes

- ❌ Editing a ConfigMap and waiting for pods to update → ✅ `rollout restart`, a hash in the template annotation or a new ConfigMap name.
- ❌ Mounting through `subPath` and waiting for updates → ✅ `subPath` is not updated; mount the whole directory.
- ❌ Referring to an object that does not exist yet → ✅ create it earlier or set `optional: true`, otherwise `CreateContainerConfigError`.
- ❌ Using `kubernetes.io/tls` without the keys `tls.crt` and `tls.key` → ✅ create it with `kubectl create secret tls`.
- ❌ Putting a secret into an environment variable "for convenience" → ✅ a file with `0400` permissions.

## Comparison with other tracks

- [C#](../../../csharp/modules/13-di-config-logging/overview.en.md): .NET configuration reads environment variables and JSON files; in Kubernetes the same sources are filled from a ConfigMap and Secret (the variable `Section__Key` for nested settings).
- [Linux](../../../linux/overview.en.md): a volume is ordinary files and `tmpfs`; file permissions (`0400`) are the same Unix permissions.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Next module: [08 · Storage](../08-storage/overview.en.md)
- Restarting a Deployment: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)

## Sources

- [ConfigMaps: mounted ConfigMaps are updated automatically](https://kubernetes.io/docs/concepts/configuration/configmap/#mounted-configmaps-are-updated-automatically)
- [ConfigMaps: immutable](https://kubernetes.io/docs/concepts/configuration/configmap/#configmap-immutable)
- [Secrets: types](https://kubernetes.io/docs/concepts/configuration/secret/#secret-types)
- [Volumes: configMap](https://kubernetes.io/docs/concepts/storage/volumes/#configmap)
- [Volumes: secret](https://kubernetes.io/docs/concepts/storage/volumes/#secret)
- [Pull an Image from a Private Registry](https://kubernetes.io/docs/tasks/configure-pod-container/pull-image-private-registry/)
