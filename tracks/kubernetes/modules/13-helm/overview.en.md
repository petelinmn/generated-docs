---
title: "Helm"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 13-helm
part: 2
levels: [1, 2, 3, 4]
tags: [helm, chart, values, release, templates, hooks, package-manager]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm

🌐 [Русский](overview.ru.md) · **English**

## In short

Helm is a package manager for Kubernetes. A **chart** is a set of manifest templates and default values; a **release** is an installed instance of a chart in the cluster with a revision history. Instead of a dozen YAML files copy-pasted between environments you keep one chart and pass different `values`. Helm can upgrade a release, roll it back to a previous revision, and run hooks and tests.

## Key ideas

- **Chart + values = manifests.** Go templates receive the `.Values`, `.Release`, `.Chart` objects and produce ordinary manifests (`helm template`).
- **A release is an object with history.** Every install and upgrade creates a revision; the state is stored in the Secret `sh.helm.release.v1.<name>.v<N>` in the release namespace.
- **`helm upgrade` without `--reuse-values` drops earlier `--set` values:** values come from the chart and the current command.
- **`--wait` and `--rollback-on-failure` (`--atomic`) protect upgrades:** without them a failed upgrade stays as a `failed` revision.
- **Hooks (`pre-install`, `pre-upgrade`, `test`, …)** run Jobs and Pods at the right moment of the release life.
- **`values.schema.json` and `required` validate input** before the cluster is touched.
- **Helm 4 applies objects with server-side apply:** manual changes of the same fields cause a conflict (`--force-conflicts`).

## Minimal example

```text
$ helm install web ./mini -n learn-13 --wait
NAME: web
STATUS: deployed
REVISION: 1
$ helm upgrade web ./mini -n learn-13 --set replicaCount=3
$ helm history web -n learn-13
1  superseded  Install complete
2  deployed    Upgrade complete
$ helm rollback web 1 -n learn-13
Rollback was a success! Happy Helming!
```

## When to use and when not

| Situation | Solution |
|---|---|
| Install ready-made software (database, ingress controller) | a Helm chart from a registry |
| Your own service in several environments with different parameters | your own chart or [Kustomize](../14-kustomize/overview.en.md) |
| You need rollbacks, hooks, release tests | Helm |
| Simple overlays without templating | Kustomize |

- ❌ Do not turn templates into a program with dozens of conditions: the chart becomes unreadable.
- ❌ Do not keep secrets in plain text in `values.yaml` in Git.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | chart structure, templates, values, install/upgrade/rollback/uninstall |
| [2. Advanced theory](advanced.en.md) | values precedence, `--reuse-values`, `--wait` and `--atomic`, schema and `required`, hooks, dependencies, packaging |
| [3. Expert theory](expert.en.md) | command and template-object reference, release storage, server-side apply, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 13-helm level 5` |
| 6. Tests | not created yet — `/learn kubernetes 13-helm level 6` |

## Related materials

- Prerequisites: [05 · Deployment](../05-deployments/overview.en.md), [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Next: [14 · Kustomize](../14-kustomize/overview.en.md)
- Hooks and Jobs: [12 · Other workloads](../12-workloads/overview.en.md)
- Rollout and rollback: [05 · Deployment](../05-deployments/advanced.en.md)
- Logs, events and metrics: [16 · Observability](../16-observability/overview.en.md)
