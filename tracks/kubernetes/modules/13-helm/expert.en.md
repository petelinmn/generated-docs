---
title: "Helm: expert theory"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 13-helm
part: 2
tags: [helm, chart, release, values, hooks, server-side-apply, helm-history, release-secret]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

The flag values below are taken from `helm <command> --help` of version 4.3 and from experiments in kind.

## Reference

### Commands

| Command | Purpose |
|---|---|
| `helm create` | a chart skeleton |
| `helm lint` | chart check |
| `helm template` | render manifests locally, without a cluster |
| `helm install` / `upgrade` | install and update (`upgrade --install` does both) |
| `helm rollback` | roll back to a revision (creates a new revision) |
| `helm uninstall` | remove a release (`--keep-history` keeps the history) |
| `helm list` | releases; filters `--deployed`, `--failed`, `--pending`, `--superseded`, `--uninstalled`, `-A` for all namespaces |
| `helm history` | release revisions |
| `helm status` | release state |
| `helm get values / manifest / hooks / notes` | what was set and applied |
| `helm test` | run test hooks |
| `helm package`, `helm show values`, `helm dependency update` | packaging, viewing, dependencies |

### `install`/`upgrade` flags (from help)

| Flag | Meaning | Default |
|---|---|---|
| `--wait[=watcher\|hookOnly\|legacy]` | wait for resource readiness | `hookOnly` without the flag |
| `--wait-for-jobs` | wait for Jobs to complete | off |
| `--timeout` | operation wait time | 5m0s |
| `--rollback-on-failure` (`--atomic`) | roll back on failure; enables `--wait=watcher` | off |
| `--reuse-values` | take the previous release's values | off |
| `--reset-then-reuse-values` | chart values + previous release + overrides | off |
| `--server-side` | `true`, `false`, `auto` | `auto` |
| `--force-conflicts` | with server-side apply take fields over from other managers | off |
| `--dry-run` | `none`, `client`, `server` | `none` |
| `--history-max` | how many revisions to keep (0 — unlimited) | 10 |
| `-i`, `--install` | for `upgrade`: install if the release does not exist | off |
| `--create-namespace` | create the namespace | off |
| `--dependency-update` | update dependencies before installing | off |
| `--skip-crds` | do not install CRDs when installing via `upgrade --install` | off |
| `--hide-secret` | with `--dry-run` do not print Secrets | off |

### Built-in template objects

| Object | What it holds (observed in experiments) |
|---|---|
| `.Values` | the final values |
| `.Release` | `.Name`, `.Namespace`, `.Service` (`Helm`), `.Revision`, etc. |
| `.Chart` | `Chart.yaml` fields (`.Name`, `.Version`, `.AppVersion`) |
| `.Capabilities`, `.Template`, `.Files` | cluster API versions, template path, chart files |

> ⚠️ Verify: `.Release.Revision`, `.Release.IsInstall`/`IsUpgrade`, `.Capabilities`, `.Files` and the functions `lookup`, `tpl`, `include` with a context come from the documentation and were not run in this material (verified: `.Release.Name`, `.Release.Namespace`, `.Release.Service`, `.Chart.Name`, `.Chart.AppVersion`, `include`, `default`, `quote`, `required`, `toYaml`, `nindent`, `with`, `range`).

### Hook annotations

| Annotation | Values |
|---|---|
| `helm.sh/hook` | `pre-install`, `pre-upgrade`, `test` (verified), plus other types per the documentation |
| `helm.sh/hook-weight` | order within a type |
| `helm.sh/hook-delete-policy` | `before-hook-creation`, `hook-succeeded` (verified), `hook-failed` |

## Internals

- **Release storage.** A release is a Secret of type `helm.sh/release.v1` named `sh.helm.release.v1.<release>.v<revision>`. Labels: `name`, `owner=helm`, `status`, `version`, `modifiedAt`. The data decodes as base64 → base64 → gzip → JSON:

```text
$ kubectl -n learn-13 get secret sh.helm.release.v1.web.v3 -o jsonpath='{.data.release}' | base64 -d | base64 -d | gunzip | head -c 80
{"name":"web","info":{"first_deployed":"2026-11-23T14:56:02.8478873+04:00","last_deployed":…
$ kubectl -n learn-13 get secret sh.helm.release.v1.web.v3 -o jsonpath='{.metadata.labels}'
{"modifiedAt":"1795431407","name":"web","owner":"helm","status":"deployed","version":"3"}
```

This means anyone who can read Secrets in the namespace has access to the release values (including passwords if they are in `values`). More in [module 07](../07-config-and-secrets/expert.en.md).
- **Install order.** In `helm template` the Service comes before the Deployment: Helm sorts manifests by resource kind, not by file name.
- **Hooks outside the release.** A hook resource is created and deleted separately from the other objects; in the experiment the migration Job disappeared after success.
- **`appVersion` is informational.** It does not affect installation; the image comes from `image.tag` or, if empty, from `.Chart.AppVersion` — that is the template author's decision (`default`).
- **A rollback is a new revision.** `helm rollback web 1` created revision 3 ("Rollback to 1"); the number of kept revisions is limited by `--history-max` (10).
- **Server-side apply and field managers.** A field belongs to the manager that set it; `kubectl scale` took over `.spec.replicas`, so the upgrade failed with a conflict.

## Pitfalls

### 1. `upgrade` loses `--set` from earlier calls

After `--set replicaCount=4` and then `--set image.tag=1.28` only `image.tag` remained in `helm get values`.

### 2. A `failed` revision is not a rollback

An upgrade that timed out left `failed`, while the cluster already holds a partly new template (`Updated: 1/2`). You need `--rollback-on-failure` or `helm rollback`.

### 3. `helm template` does not know the cluster

It renders YAML locally. `lookup`, API versions and server-side validation work only on a real install or with `--dry-run=server`.

### 4. The value type with `--set`

With `replicaCount` declared `type: integer` in the schema, `--set replicaCount=abc` is rejected (`got string, want integer`). Without a schema the string would go into the template and break the manifest on the API side.

### 5. Manual edits and Helm 4

`kubectl scale` on a Helm 4 release leads to a conflict (`conflict with "kubectl.exe" with subresource "scale"`), and `--force-conflicts` takes the field back.

### 6. `helm list -a` does not exist

There is no short flag `-a`; the filters are `--uninstalled`, `--failed`, etc. (`Error: unknown shorthand flag: 'a' in -a`).

### 7. `uninstall` and history

Without `--keep-history` the history is deleted; with it the revision gets the status `uninstalled`, and the release is visible only via `helm list --uninstalled`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Helm | parameters, history, rollbacks, hooks, ready-made charts | Go templates are hard to read, a Secret with the release |
| [Kustomize](../14-kustomize/overview.en.md) | plain YAML, overlays, built into kubectl | no release history or packages |
| Plain manifests | maximum simplicity | copy-paste between environments |
| Operators (module 19 of the roadmap) | domain knowledge | complexity |

A common combination: Helm for third-party components, Kustomize or GitOps (module 20 of the roadmap) for your own.

## Interview questions

<details><summary>1. What are a chart and a release?</summary>

A chart is a package of templates and values; a release is an installed instance of a chart with a revision history.

→ [Basic theory](basics.en.md)

</details>

<details><summary>2. How does <code>helm template</code> differ from <code>helm install --dry-run</code>?</summary>

`template` renders manifests locally, without a cluster. `--dry-run=server` simulates the install on the server (a connection is needed), `--dry-run=client` — without contacting the cluster.

→ [Reference](#installupgrade-flags-from-help)

</details>

<details><summary>3. Where does Helm keep a release's state?</summary>

In the Secret `sh.helm.release.v1.<name>.v<N>` of type `helm.sh/release.v1` in the release namespace; the data is gzip and base64.

→ [Internals](#internals)

</details>

<details><summary>4. In what order are values layered?</summary>

The chart's `values.yaml` → `-f` files in order → `--set`. The last one wins.

→ [Advanced theory](advanced.en.md#values-precedence)

</details>

<details><summary>5. Trap: what is left of <code>--set replicaCount=4</code> after <code>helm upgrade --set image.tag=1.28</code>?</summary>

Nothing: without `--reuse-values` the previous call's values are not inherited.

→ [Pitfall 1](#1-upgrade-loses---set-from-earlier-calls)

</details>

<details><summary>6. What does a rollback do: delete a revision?</summary>

No: `helm rollback web 1` creates a new revision ("Rollback to 1").

→ [Basic theory](basics.en.md#release-lifecycle)

</details>

<details><summary>7. How do you protect against a failed upgrade?</summary>

`--wait` and `--rollback-on-failure` (`--atomic`) with `--timeout`; without the rollback a `failed` revision remains.

→ [Advanced theory](advanced.en.md#failed-update---wait-and---rollback-on-failure)

</details>

<details><summary>8. What is a hook and when does <code>pre-upgrade</code> run?</summary>

A resource with the annotation `helm.sh/hook`. `pre-upgrade` runs before the other objects are applied: in the experiment the Job ran before the Deployment appeared.

→ [Advanced theory](advanced.en.md#hooks)

</details>

<details><summary>9. How do you check values before installing?</summary>

`values.schema.json` (JSON Schema) and the `required` function in templates; `helm lint` for template errors.

→ [Advanced theory](advanced.en.md#input-validation)

</details>

<details><summary>10. What is <code>Chart.lock</code> for?</summary>

It pins the versions and a digest of dependencies after `helm dependency update`.

→ [Advanced theory](advanced.en.md#dependencies-subcharts)

</details>

<details><summary>11. Trap: <code>helm upgrade</code> failed on a conflict with <code>kubectl</code>. Why?</summary>

Helm 4 applies objects with server-side apply; the field `.spec.replicas` belongs to the `kubectl` manager (subresource `scale`). The fix: `--force-conflicts` or do not edit objects by hand.

→ [Advanced theory](advanced.en.md#server-side-apply-in-helm-4)

</details>

<details><summary>12. How does <code>version</code> differ from <code>appVersion</code>?</summary>

`version` is the version of the chart itself (in the package name), `appVersion` is the application version, informational; it can be used as the default image tag.

→ [Basic theory](basics.en.md#chart-structure)

</details>

<details><summary>13. Who can read a release's values?</summary>

Anyone who can read the Secret `sh.helm.release.v1.*` in the namespace; the values decode trivially.

→ [Internals](#internals)

</details>

<details><summary>14. When is Kustomize better than Helm?</summary>

When you need environment overlays over plain YAML without templates and do not need release history or packages ([module 14](../14-kustomize/overview.en.md)).

→ [Trade-offs](#trade-offs-and-alternatives)

</details>

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Self-check: [quiz](quiz.en.md), [exercises](exercises.en.md)
- Secret storage: [07 · ConfigMap and Secret](../07-config-and-secrets/expert.en.md)
- Alternative: [14 · Kustomize](../14-kustomize/overview.en.md)

## Sources

- [Helm CLI reference](https://helm.sh/docs/helm/helm/)
- [Helm: Architecture — release storage](https://helm.sh/docs/topics/advanced/#storage-backends)
- [Helm: Chart Template Guide — built-in objects](https://helm.sh/docs/chart_template_guide/builtin_objects/)
- [Helm: Chart Hooks](https://helm.sh/docs/topics/charts_hooks/)
- [Helm: Charts](https://helm.sh/docs/topics/charts/)
