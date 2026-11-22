---
title: "Упражнения: архитектура Kubernetes"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 02-architecture
part: 1
tags: [kubernetes, architecture, controller, scheduler, etcd, finalizers]
status: draft
updated: 2026-11-22
---

# Упражнения: архитектура Kubernetes

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind (`kind create cluster --name learn`) и namespace для опытов: `kubectl create namespace learn-02`. Убрать за собой: `kubectl delete namespace learn-02`.

## 1. 🟢 Прочитай цепочку событий

**Задача:** создай Deployment `web` с образом `nginx:1.29` на 2 реплики и по событиям назови, какой компонент выполнил каждый шаг: создание ReplicaSet, создание подов, выбор ноды, запуск контейнера.

**Ожидаемый результат:** в выводе есть источники `deployment-controller`, `replicaset-controller`, `default-scheduler`, `kubelet`.

<details><summary>Подсказка</summary>

`kubectl get events` умеет выводить выбранные поля через `-o custom-columns`. Поле источника — `.source.component`.

</details>

<details><summary>Решение</summary>

```bash
kubectl create namespace learn-02
kubectl -n learn-02 create deployment web --image=nginx:1.29 --replicas=2
kubectl -n learn-02 rollout status deploy/web
kubectl -n learn-02 get events -o custom-columns=REASON:.reason,SOURCE:.source.component,MSG:.message
```

```text
REASON              SOURCE                  MSG
Scheduled           default-scheduler       Successfully assigned learn-02/web-… to learn-control-plane
Pulling             kubelet                 Pulling image "nginx:1.29"
ScalingReplicaSet   deployment-controller   Scaled up replica set web-689b9bdbdd from 0 to 2
SuccessfulCreate    replicaset-controller   Created pod: web-689b9bdbdd-24sgv
```

`deployment-controller` создал ReplicaSet, `replicaset-controller` — поды, `default-scheduler` выбрал ноду, `kubelet` скачал образ. Порядок строк в выводе может быть другим: метка времени хранит только секунды.

</details>

## 2. 🟢 Найди владельцев

**Задача:** для каждого пода и для ReplicaSet выведи вид владельца. Затем удали Deployment и проверь, что ReplicaSet и поды тоже исчезли.

**Ожидаемый результат:**

```text
NAME                   OWNER
web-689b9bdbdd-24sgv   ReplicaSet
web-689b9bdbdd-7hpbd   ReplicaSet
NAME             OWNER
web-689b9bdbdd   Deployment
```

<details><summary>Подсказка</summary>

Владельцы записаны в `metadata.ownerReferences`. Используй `-o custom-columns` с путём `.metadata.ownerReferences[0].kind`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-02 get pods -o custom-columns=NAME:.metadata.name,OWNER:.metadata.ownerReferences[0].kind
kubectl -n learn-02 get rs   -o custom-columns=NAME:.metadata.name,OWNER:.metadata.ownerReferences[0].kind
kubectl -n learn-02 delete deployment web
kubectl -n learn-02 get rs,pods
```

После удаления Deployment сборщик мусора удаляет ReplicaSet и поды (вывод последней команды: `No resources found`, подробности завершения могут занять несколько секунд).

</details>

## 3. 🟡 Остановить scheduler

**Задача:** в kind временно останови scheduler, увеличь `replicas` и убедись, что новый под остаётся `Pending` без события `FailedScheduling`. Верни scheduler и проверь, что под запустился.

**Ожидаемый результат:** один под `Pending`, после возврата — `Running`.

<details><summary>Подсказка</summary>

Scheduler в kind — статический под: достаточно переместить его манифест из `/etc/kubernetes/manifests` внутри ноды. Ноду открывай через `docker exec learn-control-plane`. **Только в одноразовом кластере kind.**

</details>

<details><summary>Решение</summary>

```bash
docker exec learn-control-plane mv /etc/kubernetes/manifests/kube-scheduler.yaml /tmp/
sleep 20
kubectl -n learn-02 scale deploy/web --replicas=3
kubectl -n learn-02 get pods
kubectl -n learn-02 get events --field-selector reason=FailedScheduling
docker exec learn-control-plane mv /tmp/kube-scheduler.yaml /etc/kubernetes/manifests/
sleep 25
kubectl -n learn-02 get pods
```

```text
web-689b9bdbdd-7hpbd 1/1 Running
web-689b9bdbdd-jcc7k 0/1 Pending
web-689b9bdbdd-r4tkv 1/1 Running
No resources found in learn-02 namespace.
```

Без scheduler под не получает ноду. Событий `FailedScheduling` нет: их выдаёт сам scheduler. После возврата манифеста под стал `Running`.

</details>

## 4. 🟡 Конфликт версий

**Задача:** воспроизведи ошибку `Conflict`: прочитай Deployment как JSON, подмени `resourceVersion` на `"1"` и выполни `kubectl replace`. Объясни, почему сервер отклонил запрос, и что должен сделать клиент.

**Ожидаемый результат:** `Error from server (Conflict): … the object has been modified; please apply your changes to the latest version and try again`.

<details><summary>Подсказка</summary>

Версия записи — `metadata.resourceVersion`. Подмени её через `sed` перед `kubectl replace -f -`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-02 get deploy web -o jsonpath='{.metadata.resourceVersion}{"\n"}'
kubectl -n learn-02 get deploy web -o json \
  | sed 's/"resourceVersion": "[0-9]*"/"resourceVersion": "1"/' \
  | kubectl replace -f -
```

```text
Error from server (Conflict): error when replacing "STDIN": Operation cannot be fulfilled on deployments.apps "web": the object has been modified; please apply your changes to the latest version and try again
```

Клиент прислал устаревшую версию, сервер защитил от потерянного обновления. Правильное поведение: заново прочитать объект, применить изменение к свежей версии и повторить запрос.

</details>

## 5. 🔴 Finalizer и Terminating

**Задача:** создай ConfigMap `fin`, добавь ему finalizer `example.com/block`, удали его и покажи, что объект остался с `deletionTimestamp`. Затем сними finalizer, чтобы объект исчез.

**Ожидаемый результат:**

```text
2026-11-22T16:02:41Z ["example.com/block"]
Error from server (NotFound): configmaps "fin" not found
```

(время у тебя будет другое)

<details><summary>Подсказка</summary>

`kubectl patch -p` умеет менять `metadata.finalizers`. Для удаления без ожидания: `--wait=false`; чтобы убрать finalizer, присвой ему `null`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-02 create configmap fin --from-literal=a=b
kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":["example.com/block"]}}'
kubectl -n learn-02 delete configmap fin --wait=false
kubectl -n learn-02 get configmap fin -o jsonpath='{.metadata.deletionTimestamp} {.metadata.finalizers}{"\n"}'
kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":null}}'
kubectl -n learn-02 get configmap fin
```

`delete` лишь выставляет `deletionTimestamp`; пока finalizer на месте, объект остаётся. После снятия finalizer сервер удалил объект сразу. В реальной жизни finalizer снимает контроллер после очистки внешних ресурсов, ручное снятие допустимо только когда очистка не нужна.

</details>
