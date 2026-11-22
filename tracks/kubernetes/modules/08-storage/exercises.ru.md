---
title: "Упражнения: хранилище"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 08-storage
part: 1
tags: [persistentvolume, persistentvolumeclaim, storageclass, reclaim-policy, access-modes, emptydir]
status: draft
updated: 2026-11-22
---

# Упражнения: хранилище

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-08`. Убрать за собой: `kubectl delete namespace learn-08`, затем удалить оставшиеся PV и классы, созданные в упражнениях.

## 1. 🟢 Данные переживают под

**Задача:** создай PVC на 1 ГиБ и под, который пишет файл в смонтированный том. Проверь, что PVC `Pending` до создания пода. Удали под, запусти другой с тем же PVC и прочитай файл.

**Ожидаемый результат:** `Pending` → `Bound`; второй под показывает тот же файл.

<details><summary>Подсказка</summary>

Класс по умолчанию (`standard`) ждёт первого потребителя. Файл записывай командой `echo … > /data/file.txt`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata: {name: data}
spec:
  accessModes: [ReadWriteOnce]
  resources: {requests: {storage: 1Gi}}
---
apiVersion: v1
kind: Pod
metadata: {name: writer}
spec:
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "echo persisted-$(date +%s) > /data/file.txt; sleep 3600"]
      volumeMounts: [{name: d, mountPath: /data}]
  volumes:
    - name: d
      persistentVolumeClaim: {claimName: data}
```

```bash
kubectl -n learn-08 apply -f pvc.yaml            # только PVC: Pending
kubectl -n learn-08 apply -f writer.yaml         # теперь Bound
kubectl -n learn-08 exec writer -- cat /data/file.txt
kubectl -n learn-08 delete pod writer --now
kubectl -n learn-08 apply -f reader.yaml         # тот же PVC, без записи
kubectl -n learn-08 exec reader -- cat /data/file.txt
```

```text
data   Pending   …   standard
data   Bound     pvc-eac81e8c-…   1Gi   RWO   standard
persisted-1795372305
persisted-1795372305
```

Файл лежит в PV, а не в поде: он пережил замену пода.

</details>

## 2. 🟢 Где лежат данные и что будет при удалении PVC

**Задача:** найди каталог данных PV на ноде kind (`kubectl get pv … -o jsonpath='{.spec.hostPath.path}'`), прочитай файл прямо на ноде. Затем удали под и PVC и проверь, что PV и каталог исчезли. Почему?

**Ожидаемый результат:** до удаления файл виден на ноде; после — `No resources found`, каталог пуст.

<details><summary>Подсказка</summary>

Ноду открывай через `docker exec learn-control-plane`. Политика освобождения видна в `kubectl get pv`.

</details>

<details><summary>Решение</summary>

```bash
kubectl get pv -o custom-columns=NAME:.metadata.name,RECLAIM:.spec.persistentVolumeReclaimPolicy
docker exec learn-control-plane sh -c "cat /var/local-path-provisioner/*/file.txt"
kubectl -n learn-08 delete pvc data --wait=false     # Terminating: под ещё использует PVC
kubectl -n learn-08 delete pod reader --now
kubectl -n learn-08 get pvc; kubectl get pv
docker exec learn-control-plane sh -c "ls /var/local-path-provisioner/"
```

```text
persisted-1795372305
["kubernetes.io/pvc-protection"]      # finalizer на PVC, пока под жив
No resources found in learn-08 namespace.
No resources found
                                      # каталог пуст
```

У динамически выделенного PV политика `Delete`: после удаления PVC провайдер убирает и PV, и каталог. Пока под использовал PVC, finalizer `kubernetes.io/pvc-protection` удерживал его в `Terminating`.

</details>

## 3. 🟡 Retain и возврат PV

**Задача:** создай StorageClass `retain-sc` (провайдер `rancher.io/local-path`, `reclaimPolicy: Retain`, `WaitForFirstConsumer`), запиши данные через под, удали под и PVC. Покажи статус PV и сохранность данных, затем верни PV в состояние `Available`.

**Ожидаемый результат:** PV `Retain` / `Released`, файл `keep-me` на ноде; после правки `claimRef` — `Available`.

<details><summary>Подсказка</summary>

`kubectl patch pv <имя> --type=json -p '[{"op":"remove","path":"/spec/claimRef"}]'`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata: {name: retain-sc}
provisioner: rancher.io/local-path
reclaimPolicy: Retain
volumeBindingMode: WaitForFirstConsumer
```

```text
$ kubectl delete pvc keep -n learn-08
$ kubectl get pv pvc-2184b427-… -o custom-columns=NAME:.metadata.name,RECLAIM:.spec.persistentVolumeReclaimPolicy,STATUS:.status.phase,CLAIM:.spec.claimRef.name
NAME               RECLAIM   STATUS     CLAIM
pvc-2184b427-…     Retain    Released   keep
$ docker exec learn-control-plane sh -c "cat /var/local-path-provisioner/pvc-2184b427-…_learn-08_keep/important.txt"
keep-me
$ kubectl patch pv pvc-2184b427-… --type=json -p '[{"op":"remove","path":"/spec/claimRef"}]'
$ kubectl get pv pvc-2184b427-…
pvc-2184b427-…   Available
```

`Retain` сохраняет PV и данные после удаления PVC; ссылка `claimRef` не позволяет привязать новый PVC, пока её не удалят. Данные при этом не очищаются.

</details>

## 4. 🟡 Статический PV и привязка

**Задача:** создай PV `static-pv` на 500 МиБ из каталога ноды `/mnt/static-data` (`hostPath`, `storageClassName: ""`, `Retain`) и PVC на 200 МиБ с `storageClassName: ""`. Проверь, сколько получил PVC. Затем создай PVC на 5 ГиБ и объясни его состояние.

**Ожидаемый результат:** PVC `Bound`, `500Mi`; второй PVC `Pending` с `FailedBinding`.

<details><summary>Подсказка</summary>

Каталог на ноде создай через `docker exec learn-control-plane mkdir -p /mnt/static-data`. События — `kubectl describe pvc`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: PersistentVolume
metadata: {name: static-pv}
spec:
  capacity: {storage: 500Mi}
  accessModes: [ReadWriteOnce]
  persistentVolumeReclaimPolicy: Retain
  storageClassName: ""
  hostPath: {path: /mnt/static-data}
```

```text
static-claim Bound static-pv 500Mi
too-big Pending
  Normal  FailedBinding  persistentvolume-controller  no persistent volumes available for this claim and no storage class is set
```

PVC на 200 МиБ получил весь PV (500 МиБ): привязка идёт целиком. Для 5 ГиБ подходящего PV нет, класс не указан, поэтому динамического выделения тоже нет: PVC остаётся `Pending`.

</details>

## 5. 🔴 RWOP, расширение и hostPath

**Задача:** (а) создай PVC с `ReadWriteOncePod` и два пода, использующих его; объясни состояние второго. (б) Попробуй увеличить связанный PVC класса `standard`. (в) Запусти под с `hostPath: /etc/kubernetes` (`readOnly`) и покажи, что он видит, а также `emptyDir` с `medium: Memory`.

**Ожидаемый результат:** (а) второй под `Pending` (`FailedScheduling`); (б) `Forbidden`; (в) `admin.conf` и `tmpfs` на 16 МиБ.

<details><summary>Подсказка</summary>

(а) Режим доступа — `ReadWriteOncePod`. (б) `kubectl patch pvc … requests.storage`. (в) `emptyDir: {medium: Memory, sizeLimit: 16Mi}`.

</details>

<details><summary>Решение</summary>

```text
$ kubectl -n learn-08 get pods first second
first    1/1   Running
second   0/1   Pending
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 node(s) unavailable due to PersistentVolumeClaim with ReadWriteOncePod access mode already in-use by another pod. …
$ kubectl -n learn-08 patch pvc keep -p '{"spec":{"resources":{"requests":{"storage":"200Mi"}}}}'
Error from server (Forbidden): persistentvolumeclaims "keep" is forbidden: only dynamically provisioned pvc can be resized and the storageclass that provisions the pvc must support resize
$ kubectl -n learn-08 exec vols -- sh -c 'mount | grep " /mem "; ls /host | head -3'
tmpfs on /mem type tmpfs (rw,relatime,size=16384k,noswap)
admin.conf
controller-manager.conf
kubelet.conf
```

`ReadWriteOncePod` допускает один под, поэтому второй не планируется. У `standard` `allowVolumeExpansion: false`, расширение запрещено. `hostPath` даёт доступ к файлам ноды, включая `admin.conf` кластера: используйте его только для системных агентов и опытов.

</details>
