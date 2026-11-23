---
title: "Helm: basic theory"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 13-helm
part: 2
tags: [helm, chart, values, release, templates]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm: basic theory

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Deployment](../05-deployments/overview.en.md), [06 · Service](../06-services/overview.en.md) · **Time:** ~3 h

All examples were run in kind (Kubernetes 1.37.0) with Helm 4.3, namespace `learn-13`. Helm is a separate program, not part of kubectl; installation: [documentation](https://helm.sh/docs/intro/install/).

## Why this is needed

The same service lives in different environments: the number of replicas, the image and the resources change. Copying manifests and editing them by hand is inconvenient and risky. Helm describes the service once with templates and substitutes parameters at install time; it also remembers the install history and can roll back.

## Chart structure

The minimal chart `mini`:

```text
mini/
├── Chart.yaml            chart metadata
├── values.yaml           default values
└── templates/
    ├── _helpers.tpl      named templates (produces no manifest)
    ├── deployment.yaml
    ├── service.yaml
    ├── NOTES.txt         text after installation
    └── tests/test.yaml   release test
```

`Chart.yaml`:

```yaml
apiVersion: v2
name: mini
description: Minimal web chart
type: application
version: 0.1.0        # chart version
appVersion: "1.29"    # application version (informational)
```

`values.yaml`:

```yaml
replicaCount: 2
image:
  repository: nginx
  tag: ""
  pullPolicy: IfNotPresent
service:
  type: ClusterIP
  port: 80
env: {}
resources: {}
```

The command `helm create <name>` creates a chart skeleton with typical templates (Deployment, Service, ServiceAccount, Ingress, HPA, HTTPRoute, a test).

## Templates

Files in `templates/` are Go templates. Actions are written in `{{ … }}`:

```yaml
# templates/deployment.yaml (fragment)
spec:
  replicas: {{ .Values.replicaCount }}
  template:
    spec:
      containers:
        - name: web
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
          {{- with .Values.env }}
          env:
            {{- range $k, $v := . }}
            - name: {{ $k }}
              value: {{ $v | quote }}
            {{- end }}
          {{- end }}
```

The essentials:

| Construct | Meaning |
|---|---|
| `.Values.x` | a value from `values.yaml`, `-f` and `--set` |
| `.Release.Name`, `.Release.Namespace` | release name and namespace |
| `.Chart.Name`, `.Chart.AppVersion` | `Chart.yaml` fields |
| `\| default X` | a default if empty |
| `\| quote` | wrap in quotes |
| `with`, `range`, `if` | scope, loop, condition |
| `include "name" .` | call a named template from `_helpers.tpl` |
| `nindent N` | a newline and an indent of N spaces |
| `toYaml` | turn a structure into YAML |
| `{{-` and `-}}` | trim whitespace and newlines on the left or right |

A named template in `_helpers.tpl`:

```yaml
{{- define "mini.fullname" -}}
{{- printf "%s-%s" .Release.Name .Chart.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
```

## Viewing the result without installing

`helm template` renders manifests locally, without a cluster; `helm lint` checks the chart:

```text
$ helm lint mini
==> Linting mini
[INFO] Chart.yaml: icon is recommended
1 chart(s) linted, 0 chart(s) failed
$ helm template rel mini --set env.A=1 --set resources.limits.cpu=100m
---
# Source: mini/templates/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: rel-mini
…
          image: "nginx:1.29"
          env:
            - name: A
              value: "1"
          resources:
            limits:
              cpu: 100m
```

The image comes from `appVersion` (`1.29`) because `image.tag` is empty; the resource name is `<release>-<chart>`.

## Release lifecycle

```text
$ helm install web mini -n learn-13 --wait --timeout 90s
NAME: web
STATUS: deployed
REVISION: 1
NOTES:
Release web installed in learn-13.
Service: web-mini:80
$ helm list -n learn-13
NAME  NAMESPACE  REVISION  STATUS    CHART       APP VERSION
web   learn-13   1         deployed  mini-0.1.0  1.29
$ helm upgrade web mini -n learn-13 --set replicaCount=3 --set image.tag=1.28 --wait
Release "web" has been upgraded. Happy Helming!
$ helm history web -n learn-13
1  superseded  mini-0.1.0  1.29  Install complete
2  deployed    mini-0.1.0  1.29  Upgrade complete
$ helm rollback web 1 -n learn-13 --wait
Rollback was a success! Happy Helming!
$ helm history web -n learn-13
1  superseded  Install complete
2  superseded  Upgrade complete
3  deployed    Rollback to 1
$ kubectl -n learn-13 get deploy web-mini -o jsonpath='{.spec.replicas} {.spec.template.spec.containers[0].image}'
2 nginx:1.29
```

A rollback creates a **new** revision (3) and does not erase the second one. What was actually passed:

```text
$ helm get values web -n learn-13          # only what the user supplied
USER-SUPPLIED VALUES:
image:
  tag: "1.28"
replicaCount: 3
$ helm get values web -n learn-13 -a       # the final values
COMPUTED VALUES:
env: {}
image:
  pullPolicy: IfNotPresent
  repository: nginx
  tag: "1.28"
…
```

Removal: `helm uninstall web -n learn-13` removes the release objects and its history (with `--keep-history` the history stays with the status `uninstalled`).

## Release test

A Pod with the annotation `"helm.sh/hook": test` is started by `helm test`:

```text
$ helm test web -n learn-13
NAME: web
STATUS: deployed
REVISION: 3
TEST SUITE:     web-mini-test
Phase:          Succeeded
```

## Beginner mistakes

- ❌ Editing release objects with `kubectl edit` → ✅ change values or templates and run `helm upgrade`.
- ❌ Omitting `--wait` and assuming the application is ready → ✅ `--wait` waits for resource readiness.
- ❌ Forgetting `-n` and installing a release into the wrong namespace → ✅ always specify the namespace.
- ❌ Expecting `helm template` to have checked cluster constraints → ✅ it only renders YAML; for a server check use `--dry-run=server`.
- ❌ Mixing up `version` and `appVersion` → ✅ the first is the chart version, the second is an informational application version.

## Related materials

- Next: [advanced theory](advanced.en.md) — values precedence, rolling back failed upgrades, hooks, dependencies
- Previous module: [12 · Other workloads](../12-workloads/overview.en.md)
- Configuration: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- A template-free alternative: [14 · Kustomize](../14-kustomize/overview.en.md)

## Sources

- [Helm: Using Helm](https://helm.sh/docs/intro/using_helm/)
- [Helm: Chart Template Guide](https://helm.sh/docs/chart_template_guide/)
- [Helm: Charts](https://helm.sh/docs/topics/charts/)
- [Helm: Chart Tests](https://helm.sh/docs/topics/chart_tests/)
- [Helm CLI: helm install](https://helm.sh/docs/helm/helm_install/)
