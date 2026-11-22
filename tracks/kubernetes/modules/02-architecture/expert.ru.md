---
title: "Архитектура Kubernetes: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 02-architecture
part: 1
tags: [kubernetes, architecture, etcd, api-server, resourceversion, finalizers, generation, kubelet, scheduler, controller]
versions: "Kubernetes 1.37.0 (kind v0.34, containerd 2.3.4), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Архитектура Kubernetes: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Как API server хранит данные

API server — единственный клиент etcd. Каждый объект лежит под ключом `/registry/<ресурс>/<namespace>/<имя>`. Посмотрим ключи Deployment и подов в namespace `learn-02` (etcd доступен через `etcdctl` внутри пода `etcd-…`, нужны клиентские сертификаты):

```text
$ kubectl -n kube-system exec etcd-learn-control-plane -- etcdctl \
    --cacert=/etc/kubernetes/pki/etcd/ca.crt \
    --cert=/etc/kubernetes/pki/etcd/server.crt \
    --key=/etc/kubernetes/pki/etcd/server.key \
    get /registry/deployments/learn-02 --prefix --keys-only
/registry/deployments/learn-02/web

$ … get /registry/pods/learn-02 --prefix --keys-only
/registry/pods/learn-02/web-689b9bdbdd-7hpbd
/registry/pods/learn-02/web-689b9bdbdd-r4tkv
```

Значение — не YAML и не JSON. Первые байты значения Deployment:

```text
0000000   k   8   s  \0  \n 025  \n  \a   a   p   p   s   /   v   1 022
0000020  \n   D   e   p   l   o   y   m   e   n   t 022 …
```

Префикс `k8s\0`, затем бинарный protobuf с `apps/v1` и `Deployment`. Поэтому «заглянуть в etcd глазами» нельзя: нужен декодер, а в нормальной работе схему хранения трогать не следует, это внутренняя деталь API server.

### Конвейер запроса

На запись и чтение у API server один и тот же конвейер: аутентификация → авторизация → admission → валидация → хранилище. Для чтения этапы admission и валидации пропускаются. Режим авторизации в нашем кластере: `--authorization-mode=Node,RBAC`; ключевые флаги из манифеста `kube-apiserver`:

| Флаг | Значение в kind | Смысл |
|---|---|---|
| `--secure-port` | `6443` | порт HTTPS API |
| `--etcd-servers` | `https://127.0.0.1:2379` | где etcd |
| `--service-cluster-ip-range` | `10.96.0.0/16` | диапазон адресов Service |
| `--authorization-mode` | `Node,RBAC` | режимы авторизации |
| `--enable-admission-plugins` | `NodeRestriction` | дополнительно включённые admission-плагины |

У controller-manager в kind: `--cluster-cidr=10.244.0.0/16` (диапазон адресов подов), `--allocate-node-cidrs=true`, `--leader-elect=true`, `--controllers=*,bootstrapsigner,tokencleaner` (`*` — все контроллеры по умолчанию).

### Версии и совместимость

Минорная версия Kubernetes выходит несколько раз в год. Политика совместимости разрешает отличие kubectl от API server на одну минорную версию в любую сторону. В нашем стенде kubectl `v1.36.1` работает с сервером `v1.37.0`. Компоненты control plane и kubelet тоже имеют допустимые расхождения (kubelet не может быть новее API server).

> ⚠️ Проверить: точные допустимые расхождения для kubelet, scheduler и controller-manager в версии 1.37 не сверены с политикой совместимости.

## Справочник

### Эндпоинты API server

| Путь | Что отдаёт | Проверено |
|---|---|---|
| `/livez`, `/readyz`, `/healthz` | состояние сервера (`ok`); `/readyz?verbose` перечисляет проверки, включая `etcd` | да, `ok` |
| `/version` | версия и платформа | да |
| `/api`, `/apis` | список версий core-группы и список API-групп | да |
| `/api/v1/…`, `/apis/<группа>/<версия>/…` | ресурсы | да |
| `/openapi/v3` | описание схем (OpenAPI) | да, отдаёт JSON |
| `/metrics` | метрики в формате Prometheus | да, отдаёт текст |

### Метаданные объекта

| Поле | Смысл |
|---|---|
| `name`, `namespace` | идентификатор в пределах типа и namespace |
| `uid` | уникальный идентификатор экземпляра (новый при пересоздании объекта с тем же именем) |
| `resourceVersion` | версия записи для оптимистичной блокировки и watch; клиент считает её непрозрачной строкой |
| `generation` | растёт при изменении `spec`; в опыте после `kubectl set image` стало `gen=5`, а `status.observedGeneration` догнало значение |
| `creationTimestamp`, `deletionTimestamp` | время создания; время начала удаления (если задан, объект в состоянии `Terminating`) |
| `labels`, `annotations` | метки для выбора объектов; произвольные пометки |
| `ownerReferences` | владельцы объекта (сборка мусора) |
| `finalizers` | блокировки удаления: объект исчезнет, когда список опустеет |
| `managedFields` | кто какие поля менял (server-side apply); kubectl по умолчанию не показывает, нужен флаг `--show-managed-fields` |

### Глаголы API

`get`, `list`, `watch`, `create`, `update`, `patch`, `delete`, `deletecollection`. Именно они используются в правилах RBAC (модуль 10).

### Что запускается на ноде

| Компонент | Задача |
|---|---|
| `kubelet` | регистрирует ноду, запускает поды, сообщает статус и проверяет probes |
| контейнерный рантайм (`containerd`) | запускает контейнеры по CRI |
| `kube-proxy` | правила для Service (в kind режим `iptables`) |
| сетевой плагин (в kind — `kindnet`) | сеть подов |

Режим `kube-proxy` получен из его ConfigMap: `mode: iptables`.

## Подвохи

### Подвох: порядок событий неточен

```text
$ kubectl -n learn-02 get events --sort-by=.metadata.creationTimestamp \
    -o custom-columns=REASON:.reason,SOURCE:.source.component
REASON              SOURCE
Scheduled           default-scheduler
Pulling             kubelet
ScalingReplicaSet   deployment-controller
SuccessfulCreate    replicaset-controller
SuccessfulCreate    replicaset-controller
Scheduled           default-scheduler
```

Событие `Scheduled` оказалось выше `ScalingReplicaSet`, хотя логически scheduler мог сработать только после создания ReplicaSet и подов. Метка `creationTimestamp` хранит секунды, поэтому события одной секунды порядок не определяет. Для восстановления последовательности смотрите причинно-следственную цепочку (кто чей владелец), а не сортировку.

### Подвох: значения в etcd не YAML

См. выше: значение начинается с `k8s\0` и содержит protobuf. `etcdctl get` без `--keys-only` выдаст нечитаемые байты.

### Подвох: объект застрял в Terminating

```text
$ kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":["example.com/block"]}}'
$ kubectl -n learn-02 delete configmap fin --wait=false
configmap "fin" deleted from learn-02 namespace
$ kubectl -n learn-02 get configmap fin -o jsonpath='{.metadata.deletionTimestamp} {.metadata.finalizers}'
2026-11-22T16:02:41Z ["example.com/block"]
```

Команда `delete` лишь выставила `deletionTimestamp`: пока в `finalizers` есть запись, объект не удаляется. Обычно finalizer снимает контроллер после очистки внешних ресурсов. Если контроллера нет, снимите запись вручную:

```text
$ kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":null}}'
$ kubectl -n learn-02 get configmap fin
Error from server (NotFound): configmaps "fin" not found
```

Предварительно убедитесь, что внешние ресурсы действительно не нужны.

### Подвох: статус устарел, если контроллер недоступен

После остановки controller-manager и удаления пода `kubectl get deploy web` показывал `READY 3/3` при двух подах (опыт в [продвинутой теории](advanced.ru.md#что-бывает-когда-компонент-недоступен)). Статус — вывод контроллера, а не наблюдение «в реальном времени».

### Подвох: nodeName в обход scheduler

```text
$ kubectl -n learn-02 apply -f - <<'EOF'
apiVersion: v1
kind: Pod
metadata: {name: manual}
spec:
  nodeName: learn-control-plane
  containers: [{name: c, image: "nginx:1.29"}]
EOF
$ kubectl -n learn-02 get events --field-selector involvedObject.name=manual \
    -o custom-columns=REASON:.reason,SOURCE:.source.component
REASON    SOURCE
Pulled    kubelet
Created   kubelet
Started   kubelet
```

События `Scheduled` нет: под минул scheduler и сразу ушёл на ноду. Проверки ресурсов, taints и affinity при этом не выполняются.

### Подвох: в одноузловом kind приложения идут на control plane

```text
$ kubectl get node learn-control-plane -o jsonpath='{.spec.taints}'
                                  # пусто: taints нет
```

В нашем одноузловом kind у ноды control plane нет taints, и поды приложения запускаются на ней (в выводе `get pods -o wide` везде `learn-control-plane`). В многоузловом kind (1 control plane + 2 worker, [модуль 03](../03-local-cluster-and-kubectl/advanced.ru.md#многоузловой-kind)) картина другая: control plane изолирован taint-ом, и три пода приложения ушли на worker-ы:

```text
$ kubectl get nodes -o jsonpath='{range .items[*]}{.metadata.name}{" taints="}{.spec.taints}{"\n"}{end}'
learn-multi-control-plane taints=[{"effect":"NoSchedule","key":"node-role.kubernetes.io/control-plane"}]
learn-multi-worker taints=
learn-multi-worker2 taints=
$ kubectl -n learn-03 get pods -o custom-columns=NAME:.metadata.name,NODE:.spec.nodeName
hello-5cf47849b9-2g9qq   learn-multi-worker2
hello-5cf47849b9-smm56   learn-multi-worker
hello-5cf47849b9-zgn5l   learn-multi-worker2
```

Taint `NoSchedule` не пускает на ноду поды без соответствующего toleration (подробнее — модуль 18).

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| Управляемый Kubernetes (облако) | control plane и etcd обслуживает провайдер | зависимость от облака, меньше контроля над control plane |
| Самостоятельный кластер (kubeadm и т. п.) | полный контроль | вы отвечаете за etcd, обновления, сертификаты |
| kind, minikube, Docker Desktop | быстрый локальный кластер для обучения | не для продакшена, нет реального HA |
| Docker Swarm, Nomad | проще в освоении | меньше экосистема, другая модель API |
| Один сервер с Docker Compose | минимум сложности | нет самовосстановления между машинами и rolling update уровня кластера |

## Вопросы с собеседований

**1. Назовите компоненты control plane и за что отвечает каждый.**

<details><summary>Ответ</summary>

`kube-apiserver` — REST API и единственная точка входа; `etcd` — хранилище состояния; `kube-scheduler` — выбор ноды для пода; `kube-controller-manager` — контроллеры, которые приводят фактическое состояние к желаемому. На нодах работают `kubelet`, контейнерный рантайм и `kube-proxy`.

</details>

**2. Кто запускает контейнер: scheduler или kubelet?**

<details><summary>Ответ</summary>

Kubelet. Scheduler только записывает в под выбранную ноду (`spec.nodeName`), а kubelet этой ноды видит назначенный под и запускает контейнеры через рантайм.

</details>

**3. Что такое цикл согласования и чем он лучше «команды»?**

<details><summary>Ответ</summary>

Контроллер сравнивает желаемое состояние (`spec`) с фактическим и исправляет разницу. Он реагирует на уровень (расхождение), а не на событие, поэтому потерянное событие или перезапуск компонента не приводят к потере изменения: следующий цикл всё исправит.

</details>

**4. (С подвохом) Вы удалили под, принадлежащий Deployment. Что произойдёт и что, если controller-manager остановлен?**

<details><summary>Ответ</summary>

Обычно ReplicaSet создаст новый под. Если controller-manager остановлен, замены не будет, а `READY` у Deployment останется устаревшим (в опыте `3/3` при двух подах). После возврата controller-manager недостающий под будет создан.

</details>

**5. Что произойдёт с новым подом, если scheduler недоступен?**

<details><summary>Ответ</summary>

Под будет создан контроллерами, но останется `Pending` без ноды. Событий `FailedScheduling` не будет, потому что их порождает сам scheduler. После возвращения scheduler под получит ноду.

</details>

**6. Зачем нужен `resourceVersion` и что такое ошибка `Conflict`?**

<details><summary>Ответ</summary>

Это версия записи для оптимистичной блокировки. При обновлении клиент присылает версию, которую прочитал; если объект изменился, сервер отвечает `Conflict`, и клиент должен перечитать объект и повторить. Это предотвращает потерянные обновления. Клиент должен считать значение непрозрачной строкой.

</details>

**7. Какие этапы проходит запрос в API server?**

<details><summary>Ответ</summary>

Аутентификация, авторизация (в нашем стенде `Node,RBAC`), admission-контроллеры (изменяющие и проверяющие), валидация схемы, запись в etcd.

</details>

**8. Кто может обращаться к etcd?**

<details><summary>Ответ</summary>

В штатной схеме — только API server. Остальные компоненты работают через API. Прямой доступ к etcd означает доступ ко всему состоянию, включая Secret, и обходит авторизацию и валидацию.

</details>

**9. (С подвохом) Что делает `kubectl delete`, если у объекта есть finalizer?**

<details><summary>Ответ</summary>

Только выставляет `deletionTimestamp`. Объект остаётся в состоянии `Terminating`, пока в списке `finalizers` есть записи; их снимают контроллеры после очистки. В опыте объект исчез сразу после `patch` с `finalizers: null`.

</details>

**10. Что такое `generation` и чем она отличается от `resourceVersion`?**

<details><summary>Ответ</summary>

`generation` растёт при изменении `spec` (в опыте 4 → 5 после `set image`), а `status.observedGeneration` показывает, какую версию спецификации обработал контроллер. `resourceVersion` меняется при любой записи объекта, включая `status`, и служит для блокировки и watch.

</details>

**11. (С подвохом) Назначил `spec.nodeName` вручную. Что произойдёт?**

<details><summary>Ответ</summary>

Под пропустит scheduler: событие `Scheduled` отсутствует, kubelet нужной ноды сразу запустит контейнеры. Правила планирования (taints, affinity, проверки ресурсов) не применяются.

</details>

**12. Зачем нужен leader election и где он виден?**

<details><summary>Ответ</summary>

Чтобы несколько копий controller-manager или scheduler не работали одновременно: активна одна, остальные ждут. Лидер записан в объекте `Lease` в `kube-system` (`kube-controller-manager`, `kube-scheduler`), а в манифесте включён флаг `--leader-elect=true`.

</details>

**13. Почему nodes kind — это контейнеры Docker, а не виртуальные машины, и что это означает для kubectl?**

<details><summary>Ответ</summary>

kind запускает каждую ноду как контейнер с systemd, kubelet и containerd внутри; поды работают внутри этих контейнеров. Для kubectl это обычный кластер: он общается с API server (порт пробрасывается на `127.0.0.1`).

</details>

## Шпаргалка

```bash
kubectl get pods -n kube-system -o wide         # компоненты control plane в kind
kubectl get --raw '/readyz?verbose'             # проверки API server
kubectl get --raw /version                      # версия сервера
kubectl api-resources                           # типы ресурсов
kubectl get events --sort-by=.metadata.creationTimestamp
kubectl get <тип> <имя> -o jsonpath='{.metadata.ownerReferences[0].kind}'
kubectl -n kube-system get lease                # leader election
kubectl auth can-i <глагол> <ресурс> [--as=…]   # проверка прав
kubectl get pods -w                             # watch
```

## Связанные материалы

- Предыдущие уровни: [базовая](basics.ru.md), [продвинутая](advanced.ru.md)
- Следующий модуль: [03 · Локальный кластер и kubectl](../03-local-cluster-and-kubectl/overview.ru.md)
- Предыдущий модуль: [01 · Контейнеры и Docker](../01-containers/overview.ru.md) — рантайм и CRI
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [Kubernetes Components](https://kubernetes.io/docs/concepts/overview/components/)
- [Kubernetes API concepts: resourceVersion](https://kubernetes.io/docs/reference/using-api/api-concepts/#resource-versions)
- [Finalizers](https://kubernetes.io/docs/concepts/overview/working-with-objects/finalizers/)
- [kube-apiserver: опции](https://kubernetes.io/docs/reference/command-line-tools-reference/kube-apiserver/)
- [Version Skew Policy](https://kubernetes.io/releases/version-skew-policy/)
- [Kubernetes API health endpoints](https://kubernetes.io/docs/reference/using-api/health-checks/)
- [Operating etcd clusters](https://kubernetes.io/docs/tasks/administer-cluster/configure-upgrade-etcd/)
- [Scheduler: назначение ноды (nodeName)](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/#nodename)
