---
title: "Упражнения: диагностика"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, events, endpoints, kubectl-debug]
status: draft
updated: 2026-11-23
---

# Упражнения: диагностика

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind (лучше с двумя рабочими нодами) и namespace: `kubectl create namespace learn-21`. После работы: `kubectl delete namespace learn-21` и удалите отладочные поды нод.

## 1. 🟢 Четыре сломанных пода

**Задача:** создайте поды с образом `nginx:no-such-tag`, с `envFrom` на несуществующий ConfigMap, с командой `/no/such/binary` и с `limits.memory: 50Mi`, который наращивает строку в цикле. Для каждого найдите статус, причину и источник причины.

**Ожидаемый результат:** `ImagePullBackOff`, `CreateContainerConfigError`, `RunContainerError`, `OOMKilled` (код `137`).

<details><summary>Подсказка</summary>

`kubectl describe pod`, `kubectl get pod -o jsonpath='{.status.containerStatuses[0].state}'` и `…lastState`.

</details>

<details><summary>Решение</summary>

```text
badimg   0/1   ImagePullBackOff             nginx:no-such-tag: not found
nocfg    0/1   CreateContainerConfigError   configmap "missing-cm" not found
badcmd   0/1   RunContainerError            exec: "/no/such/binary": stat /no/such/binary: no such file or directory
oom      0/1   OOMKilled                    exitCode 137
```

</details>

## 2. 🟢 Pending из-за PVC

**Задача:** создайте PVC со `storageClassName: no-such-class` и под, который его монтирует. Найдите причину в событиях пода и PVC.

**Ожидаемый результат:** `pod has unbound immediate PersistentVolumeClaims` и `storageclass … "no-such-class" not found`.

<details><summary>Подсказка</summary>

`kubectl describe pod` и `kubectl describe pvc`.

</details>

<details><summary>Решение</summary>

```text
Warning  FailedScheduling    default-scheduler  0/3 nodes are available: pod has unbound immediate PersistentVolumeClaims. not found
Warning  ProvisioningFailed  persistentvolume-controller  storageclass.storage.k8s.io "no-such-class" not found
```

</details>

## 3. 🟡 Три Service

**Задача:** разверните Deployment `web` (nginx, 2 реплики) и три Service: с верным селектором, с опечаткой в селекторе и с `targetPort: 8080`. Проверьте endpoints и доступность из пода `busybox`.

**Ожидаемый результат:** endpoints `10.244…:80`, `<none>`, `…:8080`; доступен только первый, остальные — `Connection refused`.

<details><summary>Подсказка</summary>

`kubectl get endpoints`, `kubectl exec client -- wget -q -T 3 -O /dev/null http://<svc>`.

</details>

<details><summary>Решение</summary>

Манифесты и вывод — в [продвинутой теории](advanced.ru.md#service-без-endpoints).

</details>

## 4. 🟡 Проба и временный контейнер

**Задача:** (а) измените readiness-пробу Deployment на `/ready` и найдите причину зависшей раскатки. (б) Добавьте к работающему поду `kubectl debug` контейнер `busybox` с `--target=nginx` и покажите процессы nginx и файлы `/etc/nginx`.

**Ожидаемый результат:** (а) `Readiness probe failed: HTTP probe failed with statuscode: 404`, `rollout status` завершается по таймауту, старые поды `1/1`; (б) `nginx: master process …`, `conf.d fastcgi_params mime.types`.

<details><summary>Подсказка</summary>

(б) `ls /proc/1/root/etc/nginx`.

</details>

<details><summary>Решение</summary>

Команды и вывод — в [продвинутой теории](advanced.ru.md#kubectl-debug-заглянуть-внутрь).

</details>

## 5. 🔴 Финализатор, квота и нода

**Задача:** (а) создайте ConfigMap, добавьте финализатор `example.com/block`, удалите ConfigMap и найдите причину, затем снимите финализатор. (б) Создайте `ResourceQuota` с `pods: "4"` и Deployment на 5 реплик; найдите причину `0/5`. (в) Остановите контейнер рабочей ноды (`docker stop`) и проследите статус ноды, taints и подов; затем запустите.

**Ожидаемый результат:** (а) `deletionTimestamp` и `["example.com/block"]`, после снятия `NotFound`; (б) `ReplicaFailure`, `exceeded quota`; (в) `NotReady` примерно через 50 секунд, taints `unreachable`, поды ещё `Running`.

<details><summary>Подсказка</summary>

(в) `kubectl get nodes`, `kubectl get node N -o jsonpath='{.spec.taints}'`.

</details>

<details><summary>Решение</summary>

Вывод — в [продвинутой теории](advanced.ru.md#объект-завис-в-terminating), [здесь](advanced.ru.md#квота-replicafailure) и [здесь](advanced.ru.md#нода-notready).

</details>
