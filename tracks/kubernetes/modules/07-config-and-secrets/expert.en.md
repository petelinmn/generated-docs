---
title: "ConfigMap and Secret: expert"
lang: en
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

# ConfigMap and Secret: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### How data gets into a container

- **Environment variables:** the kubelet reads the object from the API when it creates the container and passes the values to the runtime. They are fixed in the container configuration: that is why they are not updated.
- **A volume:** the kubelet periodically compares the volume state with the object and rebuilds the content. The files lie in a directory like `..2026_11_22_…`, and `..data` is a symbolic link to the current directory; it is switched atomically, and a reader never sees half-updated files. For a `ConfigMap` the volume is mounted from the node disk (`ext4`, `ro`), for a `Secret` as `tmpfs`, i.e. secret data is not written to the node disk.
- **`subPath`:** a specific file is mounted, bypassing the symbolic link, so updates never reach it.

### Storage in etcd and encryption

A Secret is stored in etcd in the same form as other objects (protobuf with the `k8s` prefix) and by default **without encryption**. Let us read the value directly (with etcd certificates, as in [module 02](../02-architecture/expert.en.md#how-the-api-server-stores-data)):

```text
$ kubectl -n kube-system exec etcd-learn-control-plane -- etcdctl --cacert=… --cert=… --key=… \
    get /registry/secrets/learn-07/db --print-value-only | tr -c '[:print:]' '.'
k8s.....v1..Secret........db....learn-07"…password..S3cr3t!....username..admin..Opaq…
```

The password `S3cr3t!` and the login `admin` lie in plain form (not even base64: base64 is a representation in the API, inside the storage the bytes are raw). The flag `--encryption-provider-config` is absent from the `kube-apiserver` manifest of our kind:

```text
$ docker exec learn-control-plane sh -c 'grep -c encryption-provider-config /etc/kubernetes/manifests/kube-apiserver.yaml'
0
```

Anyone with access to etcd or its backups gets all the cluster's secrets.

### Encrypting Secrets in etcd

Encryption is enabled by an `EncryptionConfiguration` that the API server receives through the flag `--encryption-provider-config`: it lists resources (`secrets`) and providers (`aescbc`, `secretbox`, `kms` and others). After enabling it, existing secrets must be rewritten so that they get encrypted. Managed cloud Kubernetes usually offers encryption through KMS.

> ⚠️ Verify: configuring `EncryptionConfiguration`, the provider list and the secret rewrite procedure were not run in this material; check against the [documentation](https://kubernetes.io/docs/tasks/administer-cluster/encrypt-data/).

### Access: RBAC

The right to read a Secret is a separate right on the `secrets` resource (verbs `get`, `list`, `watch`). A `list` request returns all values. An ordinary ServiceAccount does not have it:

```text
$ kubectl -n learn-07 auth can-i get secrets --as=system:serviceaccount:learn-07:default
no
```

But anyone who can **create a pod** in a namespace can mount any Secret of that namespace and read it. So the right to create pods must be restricted too (RBAC — module 10, Pod Security — module 17).

## Reference

### Object fields

| Object | Top-level fields (from `kubectl explain`) |
|---|---|
| ConfigMap | `data`, `binaryData`, `immutable` |
| Secret | `data` (base64), `stringData` (write-only), `type`, `immutable` |

Limits:

| Parameter | Value | Verified |
|---|---|---|
| Object size | at most 1 MiB (`1048576` bytes) | yes: a 1.1 MB file was rejected with `Too long: may not be more than 1048576 bytes` |
| Key name | `[-._a-zA-Z0-9]+` | yes: the key `bad key` was rejected |
| ConfigMap values | strings (`data`); binary data — `binaryData` | yes |

### Ways of attaching

| Way | Field | Update | Visibility |
|---|---|---|---|
| one variable | `env[].valueFrom.configMapKeyRef` / `secretKeyRef` | no | `/proc/<pid>/environ` |
| all keys as variables | `envFrom[].configMapRef` / `secretRef` (+ `prefix`) | no | `/proc/<pid>/environ` |
| a volume | `volumes[].configMap` / `secret` (+ `items`, `defaultMode`, `optional`) | yes (~a minute) | the container file system |
| a volume + `subPath` | `volumeMounts[].subPath` | no | a file |

`optional: true` allows an object or key to be absent. With a `prefix`, `envFrom` gives variables like `CFG_good`; the key `bad-name` came as `CFG_bad-name` (a name with a hyphen is allowed for the container but inconvenient for a shell).

### Secret types

| Type | Purpose | Required keys | Verified |
|---|---|---|---|
| `Opaque` | arbitrary data (default) | — | yes |
| `kubernetes.io/dockerconfigjson` | access to an image registry (`imagePullSecrets`) | `.dockerconfigjson` | yes |
| `kubernetes.io/tls` | a certificate and key pair | `tls.crt`, `tls.key` | yes: without the keys the object was rejected |
| `kubernetes.io/basic-auth` | login and password | `username` or `password` | no |
| `kubernetes.io/ssh-auth` | an SSH key | `ssh-privatekey` | no |
| `kubernetes.io/service-account-token` | a ServiceAccount token | — | no |
| `bootstrap.kubernetes.io/token` | a node join token | — | no |

> ⚠️ Verify: the types `basic-auth`, `ssh-auth`, `service-account-token`, `bootstrap.kubernetes.io/token` and their required keys are given from documentation and were not created in this material.

## Gotchas

### Gotcha: a Secret lies in etcd as plain text

The experiment above: `S3cr3t!` in the etcd key value. The base64 in `kubectl get secret -o yaml` is only the representation in the API. Protection is RBAC, etcd encryption and protected backups.

### Gotcha: anyone who reads a Secret decodes base64

```text
$ kubectl -n learn-07 get secret db -o jsonpath='{.data.password}' | base64 -d
S3cr3t!
```

`kubectl describe secret` hides the values, `get -o yaml` does not.

### Gotcha: a secret in a variable is visible in /proc

```text
$ kubectl -n learn-07 exec app -- sh -c 'cat /proc/1/environ | tr "\0" "\n" | grep DB_PASSWORD'
DB_PASSWORD=S3cr3t!
```

Variables are available to all processes of the container, inherited by child processes and often end up in error messages and dumps. A file with `0400` permissions is safer.

### Gotcha: editing a ConfigMap does not update a running application

Variables are not updated at all, files are updated with a delay (~57 s in the experiment), and `subPath` is never updated ([advanced](advanced.en.md#updating-values)). The Deployment is not restarted after the edit (`RS count before=2 after=2`).

### Gotcha: CreateContainerConfigError instead of a clear error

A missing ConfigMap or key gives the status `CreateContainerConfigError`, and the cause is in the events (`kubectl describe pod`). The pod starts by itself ~20 seconds after the missing object is created (in the experiment).

### Gotcha: the type kubernetes.io/tls without keys is rejected

`error: failed to create secret Secret "bad" is invalid: [data[tls.crt]: Required value, data[tls.key]: Required value]`. The check is performed on the API side by type.

### Gotcha: an object larger than 1 MiB

```text
error: failed to create configmap: ConfigMap "huge" is invalid: []: Too long: may not be more than 1048576 bytes
```

Large files do not fit: use a volume (PersistentVolume), an image or an external store.

### Gotcha: `--from-env-file` cannot be mixed with other sources

```text
error: from-env-file cannot be combined with from-file or from-literal
```

## External secret stores and alternatives

| Approach | Idea | Trade-off |
|---|---|---|
| Secret + RBAC + etcd encryption | the built-in mechanism | the secret is in etcd and in the manifest |
| Sealed Secrets | a secret is encrypted with the cluster's public key and safely lies in Git | needs a controller, the key is bound to the cluster |
| SOPS | encrypting values in manifest files | needs a key (KMS, age) and GitOps integration |
| External Secrets Operator | syncs a Secret from an external store (Vault, cloud secret managers) | dependence on an external system |
| Secrets Store CSI Driver | mounts secrets from an external store as a volume | a driver on the nodes |
| Vault Agent and the like | a sidecar fetches secrets and dynamic credentials | harder to operate |

> ⚠️ Verify: Sealed Secrets, SOPS, External Secrets Operator, Secrets Store CSI Driver and Vault are described from documentation and were not run in this material.

## Interview questions

**1. How does a ConfigMap differ from a Secret?**

<details><summary>Answer</summary>

By purpose and precautions: a ConfigMap is ordinary settings, a Secret is sensitive data. Technically a Secret is also key-value pairs, but the values are stored in base64, `describe` hides them, the volume is mounted as `tmpfs`, and access to them is restricted separately (RBAC on `secrets`) and they can be encrypted in etcd.

</details>

**2. (Trick) Is a Secret encrypted?**

<details><summary>Answer</summary>

No. base64 is an encoding, decoded with one command. In etcd a Secret lies as plain text by default (in the experiment `password..S3cr3t!` in the key value), and the flag `--encryption-provider-config` is absent in kind. You need etcd encryption (`EncryptionConfiguration`, KMS), RBAC and protected backups.

</details>

**3. In which ways does a ConfigMap get into a pod and which of them update?**

<details><summary>Answer</summary>

Environment variables (`env`, `envFrom`) and volumes. Variables are fixed at start and not updated; volume files are updated in about a minute (57 s in the experiment); a volume with `subPath` is not updated.

</details>

**4. (Trick) You changed a ConfigMap, but the Deployment keeps working with the old values. Why and what to do?**

<details><summary>Answer</summary>

Changing a ConfigMap does not change the pod template, so no rollout starts (in the experiment the number of ReplicaSets did not change). You need `kubectl rollout restart`, a ConfigMap hash in the template annotation (`checksum/config` in Helm) or a new ConfigMap name on every change.

</details>

**5. What happens if a pod refers to a non-existent ConfigMap or key?**

<details><summary>Answer</summary>

The status is `CreateContainerConfigError` (events: `configmap "nope" not found`, `couldn't find key nokey in ConfigMap …`). The pod starts by itself after the object appears (in ~20 s in the experiment). For optional references there is `optional: true`.

</details>

**6. What is `immutable: true` for?**

<details><summary>Answer</summary>

It forbids changing `data` (the error `field is immutable when immutable is set`), protects from accidental edits and reduces the load on the API server. The flag cannot be removed; changes are made with a new object (with a new name).

</details>

**7. Why is it better to mount secrets as files than as variables?**

<details><summary>Answer</summary>

Variables are visible in `/proc/<pid>/environ`, inherited by child processes and often end up in logs and dumps. A file can be restricted by permissions (`defaultMode: 0400`), a Secret volume is `tmpfs` (not on the node disk), and files are updated without a restart.

</details>

**8. Who can read a Secret?**

<details><summary>Answer</summary>

Whoever has `get`/`list` on `secrets`, plus anyone who can create a pod in the namespace and mount the Secret, and anyone with access to etcd. In the experiment `can-i get secrets` for the `default` ServiceAccount is `no`.

</details>

**9. What is the size limit of a ConfigMap and Secret?**

<details><summary>Answer</summary>

1 MiB (1048576 bytes) per object: in the experiment 1.1 MB was rejected with `Too long: may not be more than 1048576 bytes`.

</details>

**10. (Trick) What is `stringData` and is it stored that way?**

<details><summary>Answer</summary>

A write-only field: values are given as plain text, and the API stores them in `data` in base64 (in the experiment `cGxhaW4tdGV4dC1pbi1tYW5pZmVzdA==`). In a manifest in Git the values stay as plain text, so such a file must not be kept in a repository without encryption.

</details>

**11. How do you store secrets in Git safely?**

<details><summary>Answer</summary>

Do not store them in plain form. Options: Sealed Secrets (encryption with the cluster key), SOPS (encrypting values), or an external store (External Secrets Operator, Secrets Store CSI Driver) where Git holds only a reference.

</details>

**12. How does a .NET application read values from a ConfigMap?**

<details><summary>Answer</summary>

Through environment variables (nested keys use `Section__Key`) or files mounted by a volume and attached as a configuration source; details are in the [C# configuration module](../../../csharp/modules/13-di-config-logging/overview.en.md).

</details>

## Cheat sheet

```bash
kubectl create configmap NAME --from-literal=K=V --from-file=FILE   # do not mix with --from-env-file
kubectl create secret generic NAME --from-literal=K=V
kubectl create secret tls NAME --cert=tls.crt --key=tls.key
kubectl create secret docker-registry NAME --docker-server=… --docker-username=… --docker-password=…
kubectl get secret NAME -o jsonpath='{.data.K}' | base64 -d
kubectl describe secret NAME                  # values are hidden
kubectl set env deploy/X --from=configmap/NAME
kubectl rollout restart deploy/X              # pick up new values
kubectl auth can-i get secrets --as=system:serviceaccount:NS:SA
```

## Related materials

- Previous levels: [basics](basics.en.md), [advanced](advanced.en.md)
- Next module: [08 · Storage](../08-storage/overview.en.md)
- Previous modules: [05 · Deployment](../05-deployments/overview.en.md), [02 · Architecture](../02-architecture/overview.en.md)
- In other tracks: [DI, configuration and logging in C#](../../../csharp/modules/13-di-config-logging/overview.en.md)

## Sources

- [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [Good practices for Kubernetes Secrets](https://kubernetes.io/docs/concepts/security/secrets-good-practices/)
- [Encrypting Confidential Data at Rest](https://kubernetes.io/docs/tasks/administer-cluster/encrypt-data/)
- [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)
- [Using RBAC Authorization](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Secret types](https://kubernetes.io/docs/concepts/configuration/secret/#secret-types)
