---
title: "Other workloads: basic theory"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, headless-service, volumeclaimtemplates, workload]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Other workloads: basic theory

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md), [08 · Storage](../08-storage/overview.en.md) · **Time:** ~3 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-12`. The DaemonSet is shown on a 3-node cluster (1 control plane + 2 workers).

## Why this is needed

In a Deployment all replicas are identical and replaceable. But a database must know which replica is the primary, and a log agent must have exactly one copy per node. You need controllers with different guarantees.

## StatefulSet

StatefulSet runs pods with a **stable identity**. It needs a headless Service ([module 06](../06-services/advanced.en.md#headless-service)) and a PVC template:

```yaml
apiVersion: v1
kind: Service
metadata: {name: web}
spec:
  clusterIP: None
  selector: {app: web}
  ports: [{port: 80, name: http}]
---
apiVersion: apps/v1
kind: StatefulSet
metadata: {name: web}
spec:
  serviceName: web
  replicas: 3
  selector: {matchLabels: {app: web}}
  template:
    metadata: {labels: {app: web}}
    spec:
      containers:
        - name: nginx
          image: nginx:1.29
          volumeMounts: [{name: data, mountPath: /usr/share/nginx/html}]
  volumeClaimTemplates:
    - metadata: {name: data}
      spec:
        accessModes: [ReadWriteOnce]
        resources: {requests: {storage: 100Mi}}
```

### What StatefulSet gives you

**Order and names.** Pods are created one by one and in order, and have predictable names `<set name>-<number>`:

```text
web-0 0/1 Pending → ContainerCreating → 1/1 Running
web-1 0/1 Pending → ContainerCreating → 1/1 Running       # only after web-0 is ready
web-2 0/1 Pending → ContainerCreating → 1/1 Running
```

**Its own PVC per replica.** The `volumeClaimTemplates` template creates `data-web-0`, `data-web-1`, `data-web-2`:

```text
$ kubectl -n learn-12 get pods,pvc --no-headers | awk '{print $1,$2,$3}'
web-0 1/1 Running
web-1 1/1 Running
web-2 1/1 Running
data-web-0 Bound pvc-54252831-…
data-web-1 Bound pvc-8c9baee0-…
data-web-2 Bound pvc-373d6e26-…
```

**Stable names and DNS.** A pod's hostname equals its name, and every pod has its own DNS record through the headless Service (`<pod>.<service>.<namespace>.svc`):

```text
$ for i in 0 1 2; do kubectl -n learn-12 exec web-$i -- hostname; done
web-0
web-1
web-2
$ kubectl -n learn-12 exec dns -- nslookup web-1.web.learn-12.svc.cluster.local
Name:	web-1.web.learn-12.svc.cluster.local
Address: 10.244.0.31
$ kubectl -n learn-12 exec dns -- wget -qO- http://web-2.web
hello from web-2
```

A client can address a specific replica (for example, the primary database `web-0`) rather than a random one.

## DaemonSet

DaemonSet runs exactly one pod on every suitable node. Typical tasks: log collection, node metrics, network plugins.

```yaml
apiVersion: apps/v1
kind: DaemonSet
metadata: {name: agent}
spec:
  selector: {matchLabels: {app: agent}}
  template:
    metadata: {labels: {app: agent}}
    spec:
      containers:
        - name: c
          image: alpine:3.22
          command: ["sh","-c","echo agent on $NODE; sleep 3600"]
          env: [{name: NODE, valueFrom: {fieldRef: {fieldPath: spec.nodeName}}}]
```

On a 3-node cluster (a control plane with a taint and two workers):

```text
$ kubectl -n learn-12 get ds agent
NAME    DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE
agent   2         2         2       2            2           <none>          25s
$ kubectl -n learn-12 get pods -o wide
agent-29n99 Running learn-multi-worker2
agent-56hbr Running learn-multi-worker
```

There are two pods, not three: the control plane node has a taint, and a pod without a toleration does not land there ([module 03](../03-local-cluster-and-kubectl/advanced.en.md#multi-node-kind)). The behavior with a toleration is in [advanced theory](advanced.en.md#daemonset-nodes-tolerations-and-selector). A DaemonSet has no replicas: the number of pods is determined by the number of nodes.

## Job

A Job runs pod(s) until **successful completion**: unlike a Deployment, a finished pod is not restarted without need.

```yaml
apiVersion: batch/v1
kind: Job
metadata: {name: simple}
spec:
  template:
    spec:
      restartPolicy: Never
      containers: [{name: c, image: alpine:3.22, command: ["sh","-c","echo job-done; exit 0"]}]
```

```text
$ kubectl -n learn-12 wait --for=condition=complete job/simple --timeout=90s
job.batch/simple condition met
$ kubectl -n learn-12 logs job/simple
job-done
```

A Job's `restartPolicy` can only be `Never` or `OnFailure`; `Always` is rejected:

```text
The Job "always" is invalid: spec.template.spec.restartPolicy: Required value: valid values: "OnFailure", "Never"
```

### Several runs: completions and parallelism

```yaml
spec:
  completions: 6        # how many successful pods are needed
  parallelism: 2        # how many at a time
  completionMode: Indexed
```

In `Indexed` mode each pod gets a number in the `JOB_COMPLETION_INDEX` variable:

```text
$ kubectl -n learn-12 logs -l job-name=parallel,batch.kubernetes.io/job-completion-index=3 --tail=1
index=3
$ kubectl -n learn-12 get job parallel -o jsonpath='succeeded={.status.succeeded} completedIndexes={.status.completedIndexes}'
succeeded=6 completedIndexes=0-5
```

Six pods were completed two at a time: every few seconds `running=2`, then the next pair. Pod names contain the index (`parallel-0-tx4fs`).

## CronJob

A CronJob creates Jobs on a schedule in the 5-field cron format (`minute hour day month weekday`):

```yaml
apiVersion: batch/v1
kind: CronJob
metadata: {name: tick}
spec:
  schedule: "* * * * *"          # every minute
  jobTemplate:
    spec:
      template:
        spec:
          restartPolicy: OnFailure
          containers: [{name: c, image: alpine:3.22, command: ["sh","-c","date +%T; echo tick"]}]
```

```text
$ kubectl -n learn-12 get cronjob tick
NAME   SCHEDULE    TIMEZONE   SUSPEND   ACTIVE   LAST SCHEDULE   AGE
tick   * * * * *   <none>     False     1        2s              2m34s
$ kubectl -n learn-12 get jobs
tick-29923831 Complete
tick-29923832 Complete
tick-29923833 Running
```

A Job name is the CronJob name plus a number (the launch time in minutes since the epoch). Run the task manually without waiting for the schedule:

```text
$ kubectl -n learn-12 create job manual-run --from=cronjob/tick
$ kubectl -n learn-12 get job manual-run -o jsonpath='owner={.metadata.ownerReferences[0].kind}/{.metadata.ownerReferences[0].name}'
owner=CronJob/tick
```

An invalid schedule is rejected immediately:

```text
The CronJob "badsched" is invalid: spec.schedule: Invalid value: "every minute": expected exactly 5 fields, found 2: [every minute]
```

## Beginner mistakes

- ❌ Running a database as a Deployment → ✅ StatefulSet: stable names and a separate PVC per replica.
- ❌ Assuming a DaemonSet will run on all nodes → ✅ nodes with a taint need a toleration (in the experiment 2 pods instead of 3).
- ❌ Setting `restartPolicy: Always` in a Job → ✅ only `Never` or `OnFailure`.
- ❌ Expecting an exact moment and exactly one run from a CronJob → ✅ make tasks idempotent.
- ❌ Deleting a StatefulSet and expecting the data to go → ✅ PVCs remain (by default).

## Related materials

- Next: [advanced theory](advanced.en.md) — identity, `partition`, PVC policy, Job retries, CronJob concurrency
- Previous module: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- Storage for StatefulSet: [08 · Storage](../08-storage/overview.en.md)
- Stopping pods: [04 · Pod](../04-pods/advanced.en.md#pod-termination)

## Sources

- [StatefulSets](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/)
- [DaemonSet](https://kubernetes.io/docs/concepts/workloads/controllers/daemonset/)
- [Jobs](https://kubernetes.io/docs/concepts/workloads/controllers/job/)
- [CronJob](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/)
- [Indexed Job for Parallel Processing](https://kubernetes.io/docs/tasks/job/indexed-parallel-processing-static/)
