---
title: "Kubernetes"
lang: ru
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

🌐 **Русский** · [English](overview.en.md)

## Коротко

Kubernetes — платформа оркестрации контейнеров с открытым исходным кодом. Она запускает контейнеры на кластере машин, перезапускает упавшие, масштабирует приложения, распределяет трафик и выкатывает обновления. Работа с ней декларативная: вы описываете желаемое состояние в YAML-манифестах, а контроллеры кластера постоянно приводят к нему фактическое состояние. Проект развивается под эгидой Cloud Native Computing Foundation (CNCF).

**Пререквизиты:** модули 01–06 [трека Linux](../linux/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| Где работать | WSL2 (Ubuntu): все команды трека выполняются там |
| Контейнеры | Docker Desktop с интеграцией WSL2 |
| Локальный кластер | kind — `kind create cluster --name learn` |
| CLI | kubectl, helm, kubeconform, bats |
| Код модуля | `code/` — `examples/*.yaml`, `solutions/`, `tests/*.bats` |
| Запустить все тесты модуля | `bats tests` в папке `code/`: валидация манифестов (kubeconform) и проверка в кластере kind |
| Убрать за модулем | `kubectl delete namespace learn-NN` |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **4** (теория и самопроверка, без кода), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Контейнеры и Docker](modules/01-containers/overview.ru.md) — образы и слои, Dockerfile, multi-stage сборка, реестры
- [02 · Архитектура Kubernetes](modules/02-architecture/overview.ru.md) — control plane, ноды, etcd, API server, scheduler, kubelet
- [03 · Локальный кластер и kubectl](modules/03-local-cluster-and-kubectl/overview.ru.md) — kind, kubeconfig, контексты, базовые команды
- [04 · Pod](modules/04-pods/overview.ru.md) — жизненный цикл, несколько контейнеров, init-контейнеры, логи и exec
- [05 · Deployment и ReplicaSet](modules/05-deployments/overview.ru.md) — декларативный подход, rolling update, rollback, масштабирование
- [06 · Service и сеть](modules/06-services/overview.ru.md) — ClusterIP, NodePort, LoadBalancer, DNS, kube-proxy
- [07 · ConfigMap и Secret](modules/07-config-and-secrets/overview.ru.md) — конфигурация приложений, env и volume, безопасность секретов
- [08 · Хранилище](modules/08-storage/overview.ru.md) — Volume, PersistentVolume, PVC, StorageClass

### Часть 2. Продвинутые темы

- [09 · Ingress и Gateway API](modules/09-ingress-and-gateway-api/overview.ru.md) — HTTP-маршрутизация, контроллеры, TLS
- [10 · Namespace и RBAC](modules/10-namespaces-and-rbac/overview.ru.md) — изоляция, ServiceAccount, Role/ClusterRole, ResourceQuota
- [11 · Probes и ресурсы](modules/11-probes-and-resources/overview.ru.md) — liveness/readiness/startup, requests/limits, QoS-классы
- [12 · Другие workload-ы](modules/12-workloads/overview.ru.md) — StatefulSet, DaemonSet, Job, CronJob
- [13 · Helm](modules/13-helm/overview.ru.md) — чарты, values, шаблоны, релизы
- [14 · Kustomize](modules/14-kustomize/overview.ru.md) — base и overlays, патчи, окружения

### Часть 3. Темы для опытных

- [15 · Автомасштабирование](modules/15-autoscaling/overview.ru.md) — HPA, VPA, Cluster Autoscaler, metrics-server
- [16 · Наблюдаемость](modules/16-observability/overview.ru.md) — Prometheus, Grafana, сбор логов, `kubectl top`
- [17 · Безопасность](modules/17-security/overview.ru.md) — NetworkPolicy, Pod Security Standards, securityContext, сканирование образов
- [18 · Планирование подов](modules/18-scheduling/overview.ru.md) — nodeSelector, affinity, taints и tolerations, приоритеты
- [19 · CRD и операторы](modules/19-crd-and-operators/overview.ru.md) — расширение API, контроллеры, паттерн оператора
- [20 · GitOps](modules/20-gitops/overview.ru.md) — Argo CD или Flux, деплой из Git
- [21 · Диагностика](modules/21-troubleshooting/overview.ru.md) — CrashLoopBackOff, Pending, события, отладка сети

## Инструменты

- [k9s](tools/k9s/overview.ru.md) — терминальный UI для кластера
- [kubectx и kubens](tools/kubectx-kubens/overview.ru.md) — быстрое переключение контекстов и namespace-ов
- [stern](tools/stern/overview.ru.md) — логи нескольких подов сразу
- [cert-manager](tools/cert-manager/overview.ru.md) — автоматические TLS-сертификаты
- [Trivy](tools/trivy/overview.ru.md) — сканирование образов и манифестов на уязвимости
- [kubeconform](tools/kubeconform/overview.ru.md) — валидация манифестов по схемам

## Рецепты

- [Веб-приложение с БД](recipes/deploy-web-app-with-db/overview.ru.md) — StatefulSet для БД, Secret, Service, приложение
- [Деплой без простоя](recipes/zero-downtime-deploy/overview.ru.md) — rolling update, probes, PodDisruptionBudget
- [HTTPS через cert-manager](recipes/tls-with-cert-manager/overview.ru.md) — TLS-сертификаты для сервисов
- [Canary и blue-green](recipes/canary-deployment/overview.ru.md) — постепенный выпуск новой версии
- [Бэкапы через CronJob](recipes/cronjob-backup/overview.ru.md) — резервное копирование БД по расписанию
- [Быстрый цикл разработки](recipes/local-dev-loop/overview.ru.md) — Skaffold или Tilt для локальной разработки

## Проекты

- [Деплой своего приложения](projects/deploy-own-app/overview.ru.md) — приложение из трека C# или Python: Dockerfile → Helm-чарт → kind
- [Полноценный кластер](projects/full-stack-cluster/overview.ru.md) — Gateway API, мониторинг, GitOps, политики безопасности

## Связанные треки

- [Linux](../linux/overview.ru.md) — фундамент: процессы, сеть, namespaces и cgroups
- [C# / .NET](../csharp/overview.ru.md) и [Python](../python/overview.ru.md) — приложения для деплоя

## Источники

- [Документация Kubernetes](https://kubernetes.io/docs/)
- [kind](https://kind.sigs.k8s.io/)
- [Документация Helm](https://helm.sh/docs/)
