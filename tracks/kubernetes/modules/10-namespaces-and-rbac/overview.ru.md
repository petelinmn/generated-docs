---
title: "Namespace и RBAC"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 10-namespaces-and-rbac
part: 2
levels: [1, 2, 3, 4]
tags: [namespace, rbac, role, rolebinding, clusterrole, serviceaccount, resourcequota, limitrange, authorization]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Namespace и RBAC

🌐 **Русский** · [English](overview.en.md)

## Коротко

Один кластер обычно делят между командами и средами. **Namespace** разделяет объекты по «папкам»: имена уникальны только внутри папки, и на каждую папку можно навесить лимиты. **RBAC** (Role-Based Access Control) отвечает на вопрос «кто что может делать»: права описываются ролями, а выдаются привязками к пользователям, группам и ServiceAccount — учётным записям приложений в кластере.

## Ключевые идеи

- **Namespace — граница имён и политик,** но не жёсткая сетевая или аппаратная изоляция: поды разных namespace по умолчанию видят друг друга по сети.
- **Часть ресурсов не принадлежит namespace** (nodes, PersistentVolume, StorageClass, сами namespace): они «кластерные».
- **RBAC работает по принципу «запрещено всё, что не разрешено».** Права только добавляются; запретительных правил нет.
- **Role + RoleBinding действуют в одном namespace,** ClusterRole + ClusterRoleBinding — во всём кластере. ClusterRole можно привязать RoleBinding-ом и ограничить одним namespace.
- **ServiceAccount — идентичность пода.** Токен ServiceAccount автоматически монтируется в под, и приложение ходит с ним в API server.
- **ResourceQuota ограничивает суммарное потребление namespace,** LimitRange задаёт значения по умолчанию и границы для одного контейнера.
- **Квота без requests и limits не пускает поды:** под без явных значений отклоняется, пока LimitRange не подставит их по умолчанию.

## Минимальный пример

```bash
kubectl create serviceaccount reader -n learn-10a
kubectl create role pod-reader --verb=get,list,watch --resource=pods -n learn-10a
kubectl create rolebinding read-pods --role=pod-reader --serviceaccount=learn-10a:reader -n learn-10a
```

```text
$ kubectl -n learn-10a auth can-i list pods --as=system:serviceaccount:learn-10a:reader
yes
$ kubectl -n learn-10a auth can-i delete pods --as=system:serviceaccount:learn-10a:reader
no
```

## Когда применять, а когда нет

- ✅ Namespace на команду, окружение или приложение; права и квоты на уровне namespace.
- ✅ Отдельный ServiceAccount на приложение с минимально необходимыми правами.
- ❌ Namespace не заменяет изоляцию безопасности: для сети нужны NetworkPolicy, для запуска подов — Pod Security (модуль 17).
- ❌ Не выдавайте `cluster-admin` приложениям и людям «на всякий случай».
- ❌ Не используйте ServiceAccount `default` для приложений, которым нужен доступ к API: выдайте отдельный и узкую роль.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | namespace: создание, область действия, удаление; Role, RoleBinding, ServiceAccount, `auth can-i`, запрос к API из пода |
| [2. Продвинутая теория](advanced.ru.md) | ResourceQuota и LimitRange, ClusterRole и привязки, встроенные роли, группы, токены ServiceAccount |
| [3. Экспертная теория](expert.ru.md) | устройство авторизации, справочник объектов RBAC, защита от повышения прав, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 10-namespaces-and-rbac level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 10-namespaces-and-rbac level 6` |

## Связанные материалы

- Пререквизиты: [06 · Service и сеть](../06-services/overview.ru.md), [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md) (доступ к Secret)
- Дальше: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- Путь запроса и авторизация: [02 · Архитектура Kubernetes](../02-architecture/advanced.ru.md#путь-запроса-через-api-server)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — пользователи, группы, права
