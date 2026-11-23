---
title: "Хранилище"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 08-storage
part: 1
levels: [1, 2, 3, 4]
tags: [volume, persistentvolume, persistentvolumeclaim, storageclass, emptydir, hostpath, dynamic-provisioning, reclaim-policy, csi]
versions: "Kubernetes 1.37.0 (kind v0.34, local-path-provisioner), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Хранилище

🌐 **Русский** · [English](overview.en.md)

## Коротко

Файловая система контейнера эфемерна: при перезапуске контейнера или замене пода данные исчезают. Для данных, которые должны жить дольше, Kubernetes даёт **тома (volumes)**. Простые тома (`emptyDir`, `hostPath`) живут вместе с подом или нодой. Для долговременных данных используют три объекта: **PersistentVolumeClaim** (запрос приложения на место), **PersistentVolume** (само хранилище) и **StorageClass** (способ автоматически создавать хранилище по запросу).

## Ключевые идеи

- **Том принадлежит поду, а не контейнеру:** переживает перезапуск контейнера, но `emptyDir` удаляется вместе с подом.
- **PVC — запрос, PV — ресурс.** Приложение просит место в PVC и не знает, где оно физически; кластер находит или создаёт подходящий PV и связывает («bind») его с PVC.
- **StorageClass включает динамическое выделение.** Без заранее созданного PV кластер сам создаёт его по запросу PVC (в kind — класс `standard` по умолчанию).
- **`WaitForFirstConsumer`:** PVC остаётся `Pending`, пока нет пода, который его использует; хранилище создаётся рядом с нодой пода.
- **`reclaimPolicy`** решает судьбу данных после удаления PVC: `Delete` (PV и данные удаляются) или `Retain` (PV переходит в `Released`, данные остаются).
- **Режимы доступа:** `ReadWriteOnce`, `ReadWriteOncePod` и другие: ограничивают, сколько нод или подов могут писать.
- **Защита PVC:** пока PVC используется подом, его нельзя удалить (finalizer `kubernetes.io/pvc-protection`).

## Минимальный пример

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
      command: ["sh", "-c", "echo persisted > /data/file.txt; sleep 3600"]
      volumeMounts: [{name: d, mountPath: /data}]
  volumes:
    - name: d
      persistentVolumeClaim: {claimName: data}
```

```text
$ kubectl -n learn-08 get pvc
NAME   STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS
data   Bound    pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130   1Gi        RWO            standard
```

## Когда применять, а когда нет

- ✅ Базы данных, очереди, загрузки пользователей — любые данные, которые должны пережить под.
- ✅ `emptyDir` — кэш и временные файлы, общий каталог между контейнерами пода.
- ❌ Не храните состояние в файловой системе контейнера и в `emptyDir`, если его нельзя потерять.
- ❌ `hostPath` на боевых кластерах — опасная привязка к ноде и доступ к её файлам; допустим для системных агентов и опытов.
- ❌ Приложение с состоянием и репликами нужно запускать как StatefulSet с `volumeClaimTemplates` (модуль 12), а не как Deployment с одним PVC.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | типы томов, PVC и динамическое выделение, `WaitForFirstConsumer`, сохранность данных, удаление PVC и защита |
| [2. Продвинутая теория](advanced.ru.md) | статические PV и правила привязки, `reclaimPolicy`, StorageClass, режимы доступа, `emptyDir` в памяти, `hostPath`, расширение тома |
| [3. Экспертная теория](expert.ru.md) | жизненный цикл PV, привязка и CSI, справочник полей, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 08-storage level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 08-storage level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Дальше: [09 · Ingress и Gateway API](../09-ingress-and-gateway-api/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — файловые системы, монтирование, права
- То же в других треках: [Testcontainers для C#](../../../csharp/libraries/testcontainers/overview.ru.md) — тома и состояние контейнеров в тестах
- StatefulSet и PVC на реплику: [12 · Другие workload-ы](../12-workloads/overview.ru.md)
- Диагностика: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
