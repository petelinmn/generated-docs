---
title: "Архитектура Kubernetes"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 02-architecture
part: 1
levels: [1, 2, 3, 4]
tags: [kubernetes, architecture, control-plane, api-server, etcd, scheduler, kubelet, controller, reconciliation]
versions: "Kubernetes 1.37.0 (kind v0.34, containerd 2.3.4), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Архитектура Kubernetes

🌐 **Русский** · [English](overview.en.md)

## Коротко

Кластер Kubernetes состоит из **control plane** (управляющая часть, принимает решения) и **нод** (рабочие машины, на которых запускаются контейнеры). Вы описываете желаемое состояние объектов (Deployment, Service и др.), API server записывает его в etcd, а контроллеры и kubelet постоянно приводят фактическое состояние к желаемому. Понимание этой схемы объясняет, почему удалённый под появляется снова, откуда берутся события и что ломается, если упал тот или иной компонент.

## Ключевые идеи

- **API server (`kube-apiserver`) — единственная точка входа.** Через него общаются kubectl, контроллеры, scheduler и kubelet; друг с другом компоненты напрямую не разговаривают.
- **etcd — источник истины.** Это распределённое хранилище ключ-значение, в нём лежит всё состояние кластера; общается с ним только API server.
- **Контроллеры работают в цикле согласования (reconciliation loop):** сравнивают желаемое состояние (`spec`) с фактическим (`status`) и исправляют расхождение. Поэтому удалённый под из Deployment создаётся заново.
- **Scheduler только выбирает ноду** для нового пода и записывает выбор в объект; запускает под не он.
- **Kubelet на ноде** следит за подами, назначенными его ноде, и запускает контейнеры через рантайм (containerd) по интерфейсу CRI.
- **Объекты связаны владельцами:** Deployment → ReplicaSet → Pod (поле `ownerReferences`).
- **Конфликты решает оптимистичная блокировка:** устаревшая версия объекта отвергается ошибкой `Conflict`.

## Минимальный пример

Создаём Deployment на 2 реплики и смотрим события: видно, кто что сделал.

```text
$ kubectl -n learn-02 create deployment web --image=nginx:1.29 --replicas=2
$ kubectl -n learn-02 get events -o custom-columns=REASON:.reason,SOURCE:.source.component,MSG:.message
REASON              SOURCE                  MSG
Scheduled           default-scheduler       Successfully assigned learn-02/web-689b9bdbdd-24sgv to learn-control-plane
Pulling             kubelet                 Pulling image "nginx:1.29"
ScalingReplicaSet   deployment-controller   Scaled up replica set web-689b9bdbdd from 0 to 2
SuccessfulCreate    replicaset-controller   Created pod: web-689b9bdbdd-24sgv
```

Здесь `deployment-controller` создал ReplicaSet, `replicaset-controller` — поды, `default-scheduler` выбрал ноду, `kubelet` скачал образ и запустил контейнер. События перечислены не по порядку, подробности — в [экспертной теории](expert.ru.md#подвох-порядок-событий-неточен).

## Когда применять, а когда нет

- ✅ Знание архитектуры нужно всегда: оно помогает диагностировать, почему под не запустился («Pending» — это scheduler, «ImagePull» — kubelet).
- ✅ Помогает правильно ожидать поведения: изменения применяются асинхронно, а не мгновенно.
- ❌ Не нужно вручную запускать компоненты control plane: в kind и управляемых облачных кластерах они уже развёрнуты.
- ❌ Не стоит работать с etcd напрямую в обход API: схема хранения не является публичным контрактом.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | компоненты control plane и ноды, порты, что видно в kind, цепочка событий при создании Deployment |
| [2. Продвинутая теория](advanced.ru.md) | цикл согласования, watch, владельцы объектов, оптимистичная блокировка, путь запроса через API server, API-группы, leader election |
| [3. Экспертная теория](expert.ru.md) | хранение в etcd, справочник эндпоинтов и полей метаданных, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 02-architecture level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 02-architecture level 6` |

## Связанные материалы

- Пререквизиты: [01 · Контейнеры и Docker](../01-containers/overview.ru.md) — рантайм containerd и образы
- Дальше: [03 · Локальный кластер и kubectl](../03-local-cluster-and-kubectl/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — процессы, сеть, cgroups
- Размещение подов на нодах: [18 · Планирование подов](../18-scheduling/overview.ru.md)
