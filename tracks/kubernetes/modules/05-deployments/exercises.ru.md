---
title: "Упражнения: Deployment и ReplicaSet"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 05-deployments
part: 1
tags: [deployment, replicaset, rolling-update, rollback, pause, progress-deadline]
status: draft
updated: 2026-11-22
---

# Упражнения: Deployment и ReplicaSet

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-05`. Убрать за собой: `kubectl delete namespace learn-05`.

## 1. 🟢 Первый Deployment

**Задача:** создай Deployment `web` на `nginx:1.28` с 3 репликами, дождись готовности, выведи Deployment, ReplicaSet и поды с меткой `pod-template-hash`. Затем увеличь число реплик до 5 и объясни, почему новый ReplicaSet не появился.

**Ожидаемый результат:** один ReplicaSet с хэшем в имени; после `scale` тот же ReplicaSet, 5 подов.

<details><summary>Подсказка</summary>

`kubectl get deploy,rs,pods -L pod-template-hash`. Масштабирование не меняет шаблон пода.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: apps/v1
kind: Deployment
metadata: {name: web}
spec:
  replicas: 3
  selector: {matchLabels: {app: web}}
  template:
    metadata: {labels: {app: web}}
    spec:
      containers:
        - {name: nginx, image: "nginx:1.28", ports: [{containerPort: 80}]}
```

```bash
kubectl -n learn-05 apply -f web.yaml
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 get deploy,rs,pods -L pod-template-hash
kubectl -n learn-05 scale deploy/web --replicas=5
kubectl -n learn-05 get rs
```

Имя ReplicaSet — `web-<хэш шаблона>`. `scale` меняет только `replicas`, шаблон прежний, поэтому тот же ReplicaSet просто получает больше подов.

</details>

## 2. 🟢 Обновление и откат

**Задача:** обнови образ на `nginx:1.29`, дождись завершения, затем откатись и проверь, какой образ в шаблоне. Сколько ReplicaSet теперь существует и сколько в них реплик?

**Ожидаемый результат:** образ `nginx:1.28` после отката; два ReplicaSet, у одного 3 реплики, у другого 0.

<details><summary>Подсказка</summary>

`kubectl set image deploy/web nginx=nginx:1.29`, затем `kubectl rollout undo deploy/web`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 rollout undo deploy/web
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 get deploy web -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
kubectl -n learn-05 get rs
```

```text
nginx:1.28
```

После обновления был новый ReplicaSet (1.29), старый уменьшился до 0 реплик, но остался для отката; `undo` вернул старый ReplicaSet с 3 репликами, новый стал 0.

</details>

## 3. 🟡 Трасса rolling update

**Задача:** запусти наблюдение за ReplicaSet (`kubectl get rs -w`) и в другом терминале смени образ. По выводу объясни, как менялись `desired`, `current` и `ready` и почему на 3 репликах обновление идёт по одному поду.

**Ожидаемый результат:** новый ReplicaSet сначала получает `desired=1`, старый уменьшается только после готовности нового пода.

<details><summary>Подсказка</summary>

`maxSurge: 25%` от 3 округляется вверх, `maxUnavailable: 25%` — вниз.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-05 get rs -w &
kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
```

```text
web-c5748c689 desired=1 current=0 ready=0
web-c5748c689 desired=1 current=1 ready=1
web-6d8567df8 desired=2 current=3 ready=3
web-c5748c689 desired=2 current=2 ready=2
web-6d8567df8 desired=1 current=2 ready=2
web-c5748c689 desired=3 current=3 ready=3
web-6d8567df8 desired=0 current=0 ready=0
```

(строки выбраны из полного вывода). `maxSurge` = ⌈0.75⌉ = 1, `maxUnavailable` = ⌊0.75⌋ = 0: сначала появляется один новый под, и только когда он готов, один старый удаляется.

</details>

## 4. 🟡 Неверный образ и застрявший деплой

**Задача:** задай образ `nginx:does-not-exist`. Проверь `READY`, `UP-TO-DATE` и состояние подов. Затем сократи `progressDeadlineSeconds` до 20, дождись сбоя и выведи условия Deployment. Откатись.

**Ожидаемый результат:** `READY 3/3`, `UP-TO-DATE 1`, у нового пода `ErrImagePull`; затем `Progressing=False (ProgressDeadlineExceeded)`.

<details><summary>Подсказка</summary>

`kubectl patch deploy web -p '{"spec":{"progressDeadlineSeconds":20}}'`. Условия: `-o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-05 set image deploy/web nginx=nginx:does-not-exist
kubectl -n learn-05 get deploy web
kubectl -n learn-05 patch deploy web -p '{"spec":{"progressDeadlineSeconds":20}}'
sleep 35
kubectl -n learn-05 rollout status deploy/web --timeout=5s
kubectl -n learn-05 get deploy web -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
kubectl -n learn-05 rollout undo deploy/web
```

```text
error: deployment "web" exceeded its progress deadline
Available=True (MinimumReplicasAvailable)
Progressing=False (ProgressDeadlineExceeded)
```

Старые поды не тронуты (`Available=True`), новый не стартует. Условие `ProgressDeadlineExceeded` — только сигнал, откат выполняет человек.

</details>

## 5. 🔴 Pause, resume и стратегия Recreate

**Задача:** (а) приостанови Deployment, измени образ и запрос CPU, убедись, что новых ReplicaSet нет, затем продолжи и проверь, что появился один новый ReplicaSet. (б) Переключи стратегию на `Recreate` и сменой образа покажи, что поды заменяются без одновременной работы старых и новых.

**Ожидаемый результат:** (а) один новый ReplicaSet после `resume`; (б) все старые поды завершаются до появления новых.

<details><summary>Подсказка</summary>

`kubectl rollout pause|resume deploy/web`, `kubectl set resources deploy/web --requests=cpu=10m`. Для `Recreate`: `kubectl patch deploy web -p '{"spec":{"strategy":{"type":"Recreate","rollingUpdate":null}}}'`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-05 rollout pause deploy/web
kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
kubectl -n learn-05 set resources deploy/web --requests=cpu=10m
kubectl -n learn-05 get rs
kubectl -n learn-05 rollout resume deploy/web
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 get rs

kubectl -n learn-05 patch deploy web -p '{"spec":{"strategy":{"type":"Recreate","rollingUpdate":null}}}'
kubectl -n learn-05 get pods -w &
kubectl -n learn-05 set image deploy/web nginx=nginx:1.28
```

Во время паузы список ReplicaSet не менялся, после `resume` появился один ReplicaSet с обоими изменениями. При `Recreate` в наблюдении сначала все старые поды `Terminating`, затем новые `Pending` → `ContainerCreating` → `Running`: между версиями есть простой.

</details>
