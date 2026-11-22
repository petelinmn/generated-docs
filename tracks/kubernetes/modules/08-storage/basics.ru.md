---
title: "Хранилище: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 08-storage
part: 1
tags: [volume, persistentvolume, persistentvolumeclaim, storageclass, dynamic-provisioning, waitforfirstconsumer, pvc-protection]
versions: "Kubernetes 1.37.0 (kind v0.34, local-path-provisioner), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Хранилище: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-08`: `kubectl create namespace learn-08`. В kind по умолчанию есть StorageClass `standard` с провайдером `rancher.io/local-path`: диски создаются как каталоги на ноде.

## Зачем это нужно

Данные внутри контейнера исчезают при его пересоздании: образ неизменяем, а слой для записи удаляется вместе с контейнером ([модуль 01](../01-containers/basics.ru.md#данные-слой-для-записи-и-volume)). Приложениям с состоянием (базы данных, загрузки пользователей) нужно место, которое переживает под.

## Простые тома

Том объявляется в `spec.volumes` пода и монтируется в контейнер через `volumeMounts`. Простейшие типы:

| Тип | Что это | Срок жизни |
|---|---|---|
| `emptyDir` | пустой каталог, создаётся вместе с подом | до удаления пода |
| `hostPath` | каталог на ноде | пока существует нода и каталог |
| `configMap`, `secret` | конфигурация ([модуль 07](../07-config-and-secrets/overview.ru.md)) | пока существует объект |
| `persistentVolumeClaim` | долговременное хранилище | независимо от пода |

`emptyDir` уже встречался в [модуле 04](../04-pods/advanced.ru.md#несколько-контейнеров-в-поде) как общий каталог контейнеров пода. Он переживает перезапуск контейнера, но не замену пода.

## PersistentVolumeClaim

Запрос на место описывает объект PVC:

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata: {name: data}
spec:
  accessModes: [ReadWriteOnce]
  resources: {requests: {storage: 1Gi}}
```

- `accessModes` — как том будет использоваться (`ReadWriteOnce` — чтение и запись с одной ноды).
- `resources.requests.storage` — нужный размер.
- `storageClassName` не указан: используется StorageClass по умолчанию (`standard`).

### PVC ждёт первого пода

Сразу после создания PVC остаётся `Pending`:

```text
$ kubectl -n learn-08 get pvc
NAME   STATUS    VOLUME   CAPACITY   ACCESS MODES   STORAGECLASS   VOLUMEATTRIBUTESCLASS   AGE
data   Pending                                      standard       <unset>                 3s
$ kubectl -n learn-08 describe pvc data
Status:        Pending
Events:
  Normal  WaitForFirstConsumer  0s (x2 over 3s)  persistentvolume-controller  waiting for first consumer to be created before binding
```

Это нормально: у StorageClass `standard` режим `volumeBindingMode: WaitForFirstConsumer`. Кластер не создаёт хранилище, пока нет пода, который его использует, потому что место нужно выбрать с учётом ноды, на которую попадёт под. PV пока тоже нет (`No resources found`).

## Под, который использует PVC

```yaml
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

После запуска пода кластер создаёт PV и связывает его с PVC:

```text
$ kubectl -n learn-08 get pvc
NAME   STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS
data   Bound    pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130   1Gi        RWO            standard
$ kubectl get pv -o custom-columns=NAME:.metadata.name,CAP:.spec.capacity.storage,MODE:.spec.accessModes,RECLAIM:.spec.persistentVolumeReclaimPolicy,SC:.spec.storageClassName,STATUS:.status.phase,CLAIM:.spec.claimRef.name
NAME                                       CAP   MODE              RECLAIM   SC         STATUS   CLAIM
pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130   1Gi   [ReadWriteOnce]   Delete    standard   Bound    data
```

PVC — `Bound`, у него появился PV с автоматически сгенерированным именем `pvc-<uid>`. PV — объект уровня кластера (без namespace), а PVC принадлежит namespace.

Где хранятся данные в kind, видно из PV:

```text
path=/var/local-path-provisioner/pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130_learn-08_data
affinity=kubernetes.io/hostname
```

Это каталог на ноде; у PV есть `nodeAffinity` по имени ноды: под с этим томом будет запущен только на ней.

## Данные переживают под

Удалим под и запустим другой с тем же PVC (`reader` читает файл):

```text
$ kubectl -n learn-08 exec writer -- cat /data/file.txt
persisted-1795372305
$ kubectl -n learn-08 delete pod writer --now
$ kubectl -n learn-08 apply -f reader.yaml
$ kubectl -n learn-08 exec reader -- cat /data/file.txt
persisted-1795372305
```

Файл на месте. Файл можно увидеть и на ноде:

```text
$ docker exec learn-control-plane sh -c "ls /var/local-path-provisioner/*/; cat /var/local-path-provisioner/*/file.txt"
file.txt
persisted-1795372305
```

## Удаление PVC

Что произойдёт, если удалить PVC, пока под его использует?

```text
$ kubectl -n learn-08 delete pvc data --wait=false
$ kubectl -n learn-08 get pvc data
NAME   STATUS        VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS
data   Terminating   pvc-eac81e8c-…                             1Gi        RWO            standard
$ kubectl -n learn-08 get pvc data -o jsonpath='{.metadata.finalizers}'
["kubernetes.io/pvc-protection"]
```

PVC остаётся `Terminating`: finalizer `kubernetes.io/pvc-protection` не даёт удалить его, пока им пользуется под. После удаления пода PVC исчезает, и вместе с ним, по политике `Delete`, исчезают PV и данные:

```text
$ kubectl -n learn-08 delete pod reader --now
$ kubectl -n learn-08 get pvc
No resources found in learn-08 namespace.
$ kubectl get pv
No resources found
$ docker exec learn-control-plane sh -c "ls /var/local-path-provisioner/"
                                     # пусто: каталог с данными удалён
```

> ⚠️ Политика `Delete` (по умолчанию у динамически выделенных томов) стирает данные вместе с PVC. Для важных данных используйте `Retain` ([продвинутая теория](advanced.ru.md#политика-освобождения-reclaimpolicy)).

## Ошибки новичка

- ❌ Хранить данные в `emptyDir` и ждать, что они переживут под → ✅ PVC.
- ❌ Удивляться, что PVC `Pending`, пока нет пода → ✅ это `WaitForFirstConsumer`; создайте под.
- ❌ Удалять PVC важных данных при политике `Delete` → ✅ перед удалением проверьте `reclaimPolicy` и сделайте резервную копию.
- ❌ Искать PV в namespace → ✅ PV — объект уровня кластера, namespace у него нет.
- ❌ Запускать приложение с состоянием как Deployment на несколько реплик с одним PVC → ✅ StatefulSet (модуль 12).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — статические PV, `Retain`, StorageClass, режимы доступа
- Предыдущий модуль: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Тома пода: [04 · Pod](../04-pods/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [Volumes](https://kubernetes.io/docs/concepts/storage/volumes/)
- [Persistent Volumes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/)
- [Storage Classes](https://kubernetes.io/docs/concepts/storage/storage-classes/)
- [Dynamic Volume Provisioning](https://kubernetes.io/docs/concepts/storage/dynamic-provisioning/)
- [Configure a Pod to Use a PersistentVolume for Storage](https://kubernetes.io/docs/tasks/configure-pod-container/configure-persistent-volume-storage/)
