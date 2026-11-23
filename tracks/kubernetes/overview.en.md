---
title: "Kubernetes"
lang: en
track: kubernetes
kind: track
doc: overview
slug: kubernetes
tags: [kubernetes, containers]
versions: "Kubernetes 1.37, kind 0.34, kubectl 1.36, Docker Engine 29.6"
default_level: 4
status: draft
updated: 2026-11-22
---

# Kubernetes

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Kubernetes is an open-source container orchestration platform. It runs containers on a cluster of machines, restarts failed ones, scales applications, distributes traffic and rolls out updates. You work with it declaratively: you describe the desired state in YAML manifests, and the cluster's controllers continuously bring the actual state in line with it. The project is developed under the Cloud Native Computing Foundation (CNCF).

**Prerequisites:** modules 01–06 of the [Linux track](../linux/overview.en.md).

## Environment

| What | Tool |
|---|---|
| Where to work | WSL2 (Ubuntu): all commands of this track run there |
| Containers | Docker Desktop with WSL2 integration |
| Local cluster | kind — `kind create cluster --name learn` |
| CLI | kubectl, helm, kubeconform, bats |
| Module code | `code/` — `examples/*.yaml`, `solutions/`, `tests/*.bats` |
| Run all module tests | `bats tests` inside `code/`: manifest validation (kubeconform) and checks in the kind cluster |
| Clean up after a module | `kubectl delete namespace learn-NN` |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check, no code), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Containers and Docker](modules/01-containers/overview.en.md) — images and layers, Dockerfile, multi-stage builds, registries
- [02 · Kubernetes architecture](modules/02-architecture/overview.en.md) — control plane, nodes, etcd, API server, scheduler, kubelet
- [03 · Local cluster and kubectl](modules/03-local-cluster-and-kubectl/overview.en.md) — kind, kubeconfig, contexts, basic commands
- [04 · Pod](modules/04-pods/overview.en.md) — lifecycle, multiple containers, init containers, logs and exec
- [05 · Deployment and ReplicaSet](modules/05-deployments/overview.en.md) — declarative approach, rolling update, rollback, scaling
- [06 · Service and networking](modules/06-services/overview.en.md) — ClusterIP, NodePort, LoadBalancer, DNS, kube-proxy
- [07 · ConfigMap and Secret](modules/07-config-and-secrets/overview.en.md) — application configuration, env vs volume, secret security
- [08 · Storage](modules/08-storage/overview.en.md) — Volume, PersistentVolume, PVC, StorageClass

### Part 2. Advanced topics

- [09 · Ingress and Gateway API](modules/09-ingress-and-gateway-api/overview.en.md) — HTTP routing, controllers, TLS
- [10 · Namespaces and RBAC](modules/10-namespaces-and-rbac/overview.en.md) — isolation, ServiceAccount, Role/ClusterRole, ResourceQuota
- [11 · Probes and resources](modules/11-probes-and-resources/overview.en.md) — liveness/readiness/startup, requests/limits, QoS classes
- [12 · Other workloads](modules/12-workloads/overview.en.md) — StatefulSet, DaemonSet, Job, CronJob
- [13 · Helm](modules/13-helm/overview.en.md) — charts, values, templates, releases
- [14 · Kustomize](modules/14-kustomize/overview.en.md) — bases and overlays, patches, environments

### Part 3. Expert topics

- [15 · Autoscaling](modules/15-autoscaling/overview.en.md) — HPA, VPA, Cluster Autoscaler, metrics-server
- [16 · Observability](modules/16-observability/overview.en.md) — Prometheus, Grafana, log collection, `kubectl top`
- [17 · Security](modules/17-security/overview.en.md) — NetworkPolicy, Pod Security Standards, securityContext, image scanning
- [18 · Pod scheduling](modules/18-scheduling/overview.en.md) — nodeSelector, affinity, taints and tolerations, priorities
- [19 · CRDs and operators](modules/19-crd-and-operators/overview.en.md) — extending the API, controllers, the operator pattern
- [20 · GitOps](modules/20-gitops/overview.en.md) — Argo CD or Flux, deploying from Git
- [21 · Troubleshooting](modules/21-troubleshooting/overview.en.md) — CrashLoopBackOff, Pending, events, network debugging

## Tools

- [k9s](tools/k9s/overview.en.md) — terminal UI for the cluster
- [kubectx and kubens](tools/kubectx-kubens/overview.en.md) — fast switching between contexts and namespaces
- [stern](tools/stern/overview.en.md) — logs from many pods at once
- [cert-manager](tools/cert-manager/overview.en.md) — automatic TLS certificates
- [Trivy](tools/trivy/overview.en.md) — vulnerability scanning of images and manifests
- [kubeconform](tools/kubeconform/overview.en.md) — schema validation of manifests

## Recipes

- [Web app with a database](recipes/deploy-web-app-with-db/overview.en.md) — StatefulSet for the database, Secret, Service, the app
- [Zero-downtime deploy](recipes/zero-downtime-deploy/overview.en.md) — rolling update, probes, PodDisruptionBudget
- [HTTPS with cert-manager](recipes/tls-with-cert-manager/overview.en.md) — TLS certificates for services
- [Canary and blue-green](recipes/canary-deployment/overview.en.md) — gradual rollout of a new version
- [Backups with CronJob](recipes/cronjob-backup/overview.en.md) — scheduled database backups
- [Fast dev loop](recipes/local-dev-loop/overview.en.md) — Skaffold or Tilt for local development

## Projects

- [Deploy your own app](projects/deploy-own-app/overview.en.md) — an app from the C# or Python track: Dockerfile → Helm chart → kind
- [Full-featured cluster](projects/full-stack-cluster/overview.en.md) — Gateway API, monitoring, GitOps, security policies

## Related tracks

- [Linux](../linux/overview.en.md) — the foundation: processes, networking, namespaces and cgroups
- [C# / .NET](../csharp/overview.en.md), [Python](../python/overview.en.md) and [JavaScript](../javascript/overview.en.md) — applications to deploy

## Sources

- [Kubernetes documentation](https://kubernetes.io/docs/)
- [kind](https://kind.sigs.k8s.io/)
- [Helm documentation](https://helm.sh/docs/)
