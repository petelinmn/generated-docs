---
title: "Хранилище: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 08-storage
part: 1
tags: [persistentvolume, storageclass, reclaim-policy, retain, access-modes, readwriteoncepod, emptydir, hostpath, volume-expansion, static-provisioning]
versions: "Kubernetes 1.37.0 (kind v0.34, local-path-provisioner), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Хранилище: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): PVC, динамическое выделение, `WaitForFirstConsumer` и удаление PVC не повторяются.

## Статические PV и правила привязки

Кроме динамического выделения, администратор может создать PV заранее. Пример на каталоге ноды:

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
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata: {name: static-claim}
spec:
  storageClassName: ""
  accessModes: [ReadWriteOnce]
  resources: {requests: {storage: 200Mi}}
```

```text
$ kubectl -n learn-08 get pvc static-claim
static-claim Bound static-pv 500Mi
$ kubectl get pv static-pv
static-pv 500Mi Bound learn-08/static-claim
```

Пустая строка `storageClassName: ""` означает «без динамического выделения: искать готовый PV». PVC запросил 200 МиБ, а получил весь PV на 500 МиБ: PVC связывается с PV целиком, остаток не делится.

Условия привязки: StorageClass совпадает, режимы доступа подходят, ёмкость PV не меньше запроса. Если подходящего PV нет, PVC остаётся `Pending`:

```text
$ kubectl -n learn-08 get pvc too-big          # запрос 5Gi при PV на 500Mi
too-big Pending
$ kubectl -n learn-08 describe pvc too-big
  Normal  FailedBinding  persistentvolume-controller  no persistent volumes available for this claim and no storage class is set
```

Привязать PVC к конкретному PV можно полем `volumeName`.

## Политика освобождения (reclaimPolicy)

Что делать с PV, когда PVC удалён:

| Политика | Что происходит |
|---|---|
| `Delete` | PV и данные удаляются (по умолчанию у динамически выделенных; в опыте каталог на ноде удалён) |
| `Retain` | PV остаётся, статус `Released`, данные сохраняются |

Создадим StorageClass с `Retain`, запишем данные, удалим под и PVC:

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
```

PV `Released`, данные на месте. Новый PVC автоматически к такому PV не привяжется: в нём остаётся ссылка на старый PVC (`claimRef`). Чтобы вернуть PV в работу, ссылку удаляют:

```text
$ kubectl patch pv pvc-2184b427-… --type=json -p '[{"op":"remove","path":"/spec/claimRef"}]'
$ kubectl get pv pvc-2184b427-…
NAME               STATUS
pvc-2184b427-…     Available
```

Данные при этом не очищаются: следующий PVC увидит старые файлы. Политику существующего PV меняют патчем `persistentVolumeReclaimPolicy`.

## StorageClass

StorageClass описывает «вид» хранилища и способ его создания:

| Поле | Смысл |
|---|---|
| `provisioner` | кто создаёт тома (`rancher.io/local-path`, драйвер облака, CSI-драйвер) |
| `parameters` | параметры для провайдера (тип диска, зона и др.) |
| `reclaimPolicy` | `Delete` (по умолчанию) или `Retain` |
| `volumeBindingMode` | `Immediate` или `WaitForFirstConsumer` |
| `allowVolumeExpansion` | можно ли увеличивать размер PVC |
| `mountOptions` | опции монтирования |
| `allowedTopologies` | ограничение по зонам и нодам |

Класс по умолчанию помечается аннотацией; в kind это `standard (default)`. Поля StorageClass получены из `kubectl explain storageclass`.

### Immediate и WaitForFirstConsumer

`Immediate` создаёт том сразу при создании PVC, до появления пода. Провайдер `local-path` в kind так работать не умеет: он выбирает каталог на ноде пода, а пода ещё нет.

```text
$ kubectl -n learn-08 get pvc imm
imm Pending
$ kubectl -n learn-08 get events --field-selector involvedObject.name=imm
ProvisioningFailed  failed to provision volume with StorageClass "imm-sc": configuration error, no node was specified
```

С `WaitForFirstConsumer` (режим класса `standard`) всё работает. В облачных кластерах `WaitForFirstConsumer` важен для зональных дисков: том создаётся в той же зоне, где будет под.

### Расширение тома

Размер уже созданного PVC можно увеличить, только если класс разрешает это (`allowVolumeExpansion: true`) и том выделен динамически:

```text
$ kubectl -n learn-08 patch pvc keep -p '{"spec":{"resources":{"requests":{"storage":"200Mi"}}}}'
Error from server (Forbidden): persistentvolumeclaims "keep" is forbidden: only dynamically provisioned pvc can be resized and the storageclass that provisions the pvc must support resize
```

У `standard` в kind `allowVolumeExpansion: false`, поэтому запрос отклонён. Уменьшать PVC нельзя. До привязки PVC почти неизменяем:

```text
spec is immutable after creation except resources.requests and volumeAttributesClassName for bound claims
```

## Режимы доступа

| Режим | Сокращение | Смысл |
|---|---|---|
| `ReadWriteOnce` | RWO | чтение и запись с одной ноды (несколько подов на этой ноде могут делить том) |
| `ReadOnlyMany` | ROX | чтение с многих нод |
| `ReadWriteMany` | RWX | чтение и запись с многих нод |
| `ReadWriteOncePod` | RWOP | чтение и запись только из одного пода |

Режим — требование к хранилищу, а не гарантия исполнения: реальную поддержку определяет драйвер. `ReadWriteOncePod` проверяется планировщиком. Второй под с тем же PVC не запускается:

```text
$ kubectl -n learn-08 get pods first second
first    1/1   Running
second   0/1   Pending
$ kubectl -n learn-08 describe pod second
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 node(s) unavailable due to PersistentVolumeClaim with ReadWriteOncePod access mode already in-use by another pod. …
```

> ⚠️ Проверить: поведение `ReadWriteMany` и `ReadOnlyMany` на реальных драйверах (NFS, облачные файловые системы) в этом материале не запускалось.

## Блочные тома

`volumeMode: Block` отдаёт под сырое блочное устройство (`volumeDevices` вместо `volumeMounts`). Провайдер `local-path` блочные тома не поддерживает:

```text
ProvisioningFailed  rancher.io/local-path does not support block volume provisioning
```

## emptyDir в памяти

`emptyDir` может размещаться в `tmpfs`; размер ограничивается `sizeLimit`:

```yaml
volumes:
  - name: mem
    emptyDir: {medium: Memory, sizeLimit: 16Mi}
```

```text
$ kubectl -n learn-08 exec vols -- sh -c 'mount | grep " /mem "; df -h /mem | tail -1'
tmpfs on /mem type tmpfs (rw,relatime,size=16384k,noswap)
tmpfs                    16.0M         0     16.0M   0% /mem
```

Данные в памяти быстрее, но считаются в потребление памяти пода (учитывайте при выборе `limits`, модуль 11) и теряются при удалении пода.

## hostPath

`hostPath` монтирует каталог ноды в контейнер:

```text
$ kubectl -n learn-08 exec vols -- ls /host        # hostPath: /etc/kubernetes
admin.conf
controller-manager.conf
kubelet.conf
```

Любой под с `hostPath` может прочитать файлы ноды: здесь это админский `admin.conf` кластера kind. Это же даёт привязку к конкретной ноде и неодинаковое поведение на разных нодах. Для опытов и системных агентов допустимо; для приложений используйте PVC (или `local`-тома с `nodeAffinity`). Использование `hostPath` ограничивают политиками безопасности (модуль 17).

## Лучшие практики

- Для важных данных используйте StorageClass с `Retain` и регулярные резервные копии: удаление PVC при `Delete` стирает данные.
- Выбирайте StorageClass с `allowVolumeExpansion: true`, если рост данных вероятен.
- Для зональных дисков используйте `WaitForFirstConsumer`.
- Не используйте `hostPath` в приложениях; ограничьте его политиками.
- Для приложений с состоянием и репликами — StatefulSet и `volumeClaimTemplates` (модуль 12).
- Следите за `Released` PV: при `Retain` они не освобождаются сами.

## Частые ошибки

- ❌ Рассчитывать, что PVC на 200 МиБ займёт ровно 200 МиБ статического PV → ✅ PVC связывается с PV целиком (получил 500 МиБ).
- ❌ Ждать, что `Released` PV сам станет доступным → ✅ нужно удалить `claimRef` (и очистить данные при необходимости).
- ❌ Пытаться уменьшить или увеличить PVC без `allowVolumeExpansion` → ✅ `Forbidden`; нужен класс с разрешением.
- ❌ Использовать `Immediate` с провайдером, привязанным к ноде → ✅ `WaitForFirstConsumer`.
- ❌ Монтировать `hostPath` ради «простоты» → ✅ PVC.

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): том — это монтирование (`mount`); `tmpfs` и `ext4` в выводе — обычные файловые системы ядра.
- [Docker](../01-containers/basics.ru.md#данные-слой-для-записи-и-volume): `docker volume` и bind mount соответствуют PVC и `hostPath`; в Kubernetes добавляются классы хранилищ и динамическое выделение.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Предыдущий модуль: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)

## Источники

- [Persistent Volumes: reclaiming](https://kubernetes.io/docs/concepts/storage/persistent-volumes/#reclaiming)
- [Persistent Volumes: access modes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/#access-modes)
- [Storage Classes](https://kubernetes.io/docs/concepts/storage/storage-classes/)
- [Resizing Persistent Volumes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/#expanding-persistent-volumes-claims)
- [Volumes: emptyDir](https://kubernetes.io/docs/concepts/storage/volumes/#emptydir)
- [Volumes: hostPath](https://kubernetes.io/docs/concepts/storage/volumes/#hostpath)
- [Change the Reclaim Policy of a PersistentVolume](https://kubernetes.io/docs/tasks/administer-cluster/change-pv-reclaim-policy/)
