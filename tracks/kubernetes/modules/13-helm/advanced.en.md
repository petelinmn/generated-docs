---
title: "Helm: advanced theory"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 13-helm
part: 2
tags: [helm, values, hooks, dependencies, atomic, reuse-values, schema, server-side-apply]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md): the chart `mini` and the release commands are not repeated.

## Values precedence

Values are layered in increasing priority: the chart's `values.yaml` → `-f` files (in order) → `--set`. In the experiment `-f v.yaml` set `replicaCount: 5` and `env.FROM: file`, and `--set replicaCount=7`:

```text
$ helm template r mini -f v.yaml --set replicaCount=7
  replicas: 7
            - name: FROM
              value: "file"
```

`--set` won for `replicaCount`; `env.FROM` came from the file. Nested maps are merged by key, not replaced as a whole. Escaping a comma in `--set`: `a\,b` gives the value `a,b`; `--set-string` sets a string.

## `--reuse-values` and the reset on upgrade

By default `helm upgrade` **does not remember** earlier `--set` values:

```text
$ helm upgrade web mini --set replicaCount=4
$ helm upgrade web mini --set image.tag=1.28
$ helm get values web
USER-SUPPLIED VALUES:
image:
  tag: "1.28"                      # replicaCount=4 is gone
$ helm upgrade web mini --reuse-values --set env.X=1
$ helm get values web
USER-SUPPLIED VALUES:
env:
  X: 1
image:
  tag: "1.28"
```

`--reuse-values` takes the previous release's values and adds the new ones. There is also `--reset-then-reuse-values`: the chart's values, then the previous release's values, then the overrides from the command line. The most reliable way is to keep all values in a file and pass `-f values.prod.yaml` every time.

## Failed update: `--wait` and `--rollback-on-failure`

An image with a non-existent tag:

```text
$ helm upgrade web mini --set image.tag=nonexistent-tag --wait --timeout 30s
Error: UPGRADE FAILED: resource Deployment/learn-13/web-mini not ready. status: InProgress, message: Updated: 1/2
context deadline exceeded
$ helm history web
6  deployed  Upgrade complete
7  failed    Upgrade "web" failed: resource Deployment/learn-13/web-mini not ready…
```

The release gets a `failed` revision, and the cluster already holds a partly new template (`Updated: 1/2`). With `--atomic` (in Helm 4's help the flag is called `--rollback-on-failure`):

```text
$ helm upgrade web mini --set image.tag=nonexistent-tag --atomic --timeout 30s
Error: UPGRADE FAILED: release web failed, and has been rolled back due to rollback-on-failure being set: …
$ helm history web
7  failed    Upgrade "web" failed…
8  failed    Upgrade "web" failed…
9  deployed  Rollback to 6
```

The release was rolled back to the last successful revision (`9`, "Rollback to 6"); the revisions of the failed attempts stay in the history. The `--wait` values: `watcher`, `hookOnly` (the default when the flag is omitted: only hooks are waited for), `legacy`; `--wait-for-jobs` waits for Jobs to complete.

> ⚠️ Verify: what exactly `--atomic` is in Helm 4 (an alias or a deprecated flag) is inferred from the error message and the help; in the experiment it worked as a rollback.

## Input validation

`values.schema.json` validates values before anything is sent to the cluster:

```json
{"$schema":"http://json-schema.org/draft-07/schema#","type":"object",
 "properties":{"replicaCount":{"type":"integer","minimum":1}}}
```

```text
$ helm install bad mini --set replicaCount=0
Error: INSTALLATION FAILED: values don't meet the specifications of the schema(s) in the following chart(s):
mini:
- at '/replicaCount': minimum: got 0, want 1
$ helm template x mini --set replicaCount=abc
- at '/replicaCount': got string, want integer
```

In a template `required` makes a value mandatory:

```yaml
image: "{{ required "image.repository is required" .Values.image.repository }}:…"
```

```text
$ helm template x mini --set image.repository=
Error: execution error at (mini/templates/deployment.yaml:19:21): image.repository is required
```

`helm lint` catches template errors (`nil pointer evaluating interface {}.deep`) before installation.

## Hooks

A hook is a resource with the annotation `helm.sh/hook`. A typical example is a database migration before an upgrade:

```yaml
metadata:
  annotations:
    "helm.sh/hook": pre-install,pre-upgrade
    "helm.sh/hook-weight": "0"
    "helm.sh/hook-delete-policy": before-hook-creation,hook-succeeded
```

Watching an install (`--wait`), once per second:

```text
t=1 jobs=hk2-mini-migrate:Running  deploy=
t=4 jobs=hk2-mini-migrate:Running  deploy=
t=5 jobs=                          deploy=hk2-mini:0/2
t=6 jobs=                          deploy=hk2-mini:2/2
```

The Job started **before** the Deployment was created; Helm waited for it to finish, then applied the rest, and the Job was deleted (`hook-succeeded`). A hook resource is visible in `helm get hooks`. The `test` hook runs only with `helm test`.

> ⚠️ Verify: the full list of hook types and the order with several weighted hooks are taken from the documentation and were not reproduced.

## Dependencies: subcharts

```yaml
# Chart.yaml
apiVersion: v2
name: umbrella
version: 0.2.0
dependencies:
  - name: mini
    version: 0.1.0
    repository: "file://charts/mini"
    condition: mini.enabled
```

```text
$ helm dependency update umbrella
Saving 1 charts
$ ls umbrella/charts
mini  mini-0.1.0.tgz
$ helm template u umbrella | grep -E "^# Source|replicas"
# Source: umbrella/charts/mini/templates/service.yaml
# Source: umbrella/charts/mini/templates/deployment.yaml
  replicas: 1
$ helm template u umbrella --set mini.enabled=false | wc -c
1
```

`Chart.lock` pins the versions and a digest of the dependencies. A subchart's values are set under its name (`mini.replicaCount: 1` in the parent's `values.yaml` gave `replicas: 1`); `condition` disables a subchart (with `mini.enabled=false` there are no manifests).

> ⚠️ Verify: dependencies from remote repositories (HTTP and OCI), the fields `alias`, `tags`, `import-values` and the `global:` block were not run: only `file://` was used.

## Packaging

```text
$ helm package mini
Successfully packaged chart and saved it to: …\mini-0.1.0.tgz
$ helm show values mini-0.1.0.tgz | head -2
replicaCount: 2
image:
```

> ⚠️ Verify: publishing and installing from registries (`helm push`, `helm repo add`, `oci://`) were not checked: the environment had no access to a registry.

## Server-side apply in Helm 4

In Helm 4's help, `--server-side` accepts `true`, `false`, `auto` (default `auto`: the method of the previous release). A manual `kubectl scale` takes over the field `.spec.replicas`, and an upgrade hits a conflict:

```text
$ kubectl scale deploy web-mini --replicas=9
$ kubectl annotate deploy web-mini manual=yes
$ helm upgrade web mini --reuse-values
Error: UPGRADE FAILED: conflict occurred while applying object learn-13/web-mini apps/v1, Kind=Deployment: Apply failed with 1 conflict: conflict with "kubectl.exe" with subresource "scale" using apps/v1: .spec.replicas
$ helm upgrade web mini --reuse-values --force-conflicts
replicas=2 manual=yes
```

The failed attempt got a `failed` revision; with `--force-conflicts` Helm restored `replicas=2`, and the foreign annotation `manual=yes` stayed: Helm does not touch fields it does not own.

## Best practices

- Environment values go in `values.<env>.yaml` files, not in `--set`.
- The standard deploy command: `helm upgrade --install --wait --rollback-on-failure --timeout …`.
- Add `values.schema.json` and `required` for mandatory parameters.
- Move resource names and labels to `_helpers.tpl`.
- Do not put secrets in `values.yaml` in Git; use external mechanisms ([module 07](../07-config-and-secrets/advanced.en.md)).
- Do not edit a release by hand with `kubectl`: the next `helm upgrade` either overwrites the changes or fails with a conflict.

## Common mistakes

- ❌ `helm upgrade --set x=1` without `--reuse-values` → ✅ keep values in a file.
- ❌ An upgrade without `--wait` → ✅ `--wait` and a rollback on error.
- ❌ A manual `kubectl scale` on a Helm release → ✅ change `replicaCount` (in Helm 4 otherwise a conflict).
- ❌ Expecting `helm template` to have checked cluster constraints → ✅ it only renders YAML.

## Comparison with other tracks

- [Kustomize](../14-kustomize/overview.en.md): overlays instead of templates; Helm gives parameters and release history.
- [C#](../../../csharp/recipes/background-jobs/overview.en.md): an application's configuration comes from ConfigMaps and Secrets ([module 07](../07-config-and-secrets/overview.en.md)) regardless of what created them.

## Related materials

- Previous level: [basic theory](basics.en.md)
- Next: [expert theory](expert.en.md)
- Deployment rollback: [05 · Deployment](../05-deployments/advanced.en.md#history-of-revisions-and-rollback)
- Hooks on Jobs: [12 · Other workloads](../12-workloads/advanced.en.md#job-retries-indexes-cleanup)

## Sources

- [Helm: Values Files](https://helm.sh/docs/chart_template_guide/values_files/)
- [Helm: Chart Hooks](https://helm.sh/docs/topics/charts_hooks/)
- [Helm: Charts — Chart Dependencies](https://helm.sh/docs/topics/charts/#chart-dependencies)
- [Helm CLI: helm upgrade](https://helm.sh/docs/helm/helm_upgrade/)
- [Helm: Schema files](https://helm.sh/docs/topics/charts/#schema-files)
