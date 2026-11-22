---
title: "Локальный кластер и kubectl: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, apply, diff, dry-run, explain, jsonpath, selectors, multi-node]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Локальный кластер и kubectl: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): установка, `kind create`, namespace и базовые команды не повторяются.

## create и apply: императивно и декларативно

`kubectl create` создаёт объект и падает, если он уже есть. `kubectl apply` приводит объект к описанию в файле и безопасен при повторном запуске:

```text
$ kubectl -n learn-03 create -f pod.yaml
pod/hello created
$ kubectl -n learn-03 create -f pod.yaml
Error from server (AlreadyExists): error when creating "pod.yaml": pods "hello" already exists
$ kubectl -n learn-03 apply -f pod.yaml
Warning: resource pods/hello is missing the kubectl.kubernetes.io/last-applied-configuration annotation which is required by kubectl apply. …
pod/hello configured
$ kubectl -n learn-03 apply -f pod.yaml
pod/hello unchanged
```

Три наблюдения:

- Повторный `create` даёт `AlreadyExists`, повторный `apply` — `unchanged`: `apply` идемпотентен.
- `apply` на объекте, созданном через `create`, предупреждает об отсутствии аннотации `last-applied-configuration` и сам её добавляет. Чтобы не получать предупреждение, создавайте декларативно или с `kubectl create --save-config`.
- Аннотация `kubectl.kubernetes.io/last-applied-configuration` хранит последний применённый манифест: по ней `apply` определяет, какие поля вы удалили из файла.

Для декларативной работы ведите манифесты в Git и применяйте их `apply -f` (файл, папка или `-k` для Kustomize, [модуль 14](../../overview.ru.md#часть-2-продвинутые-темы)).

## diff и dry-run

`kubectl diff -f` сравнивает файл с состоянием кластера, не меняя его. Правим порт в манифесте и смотрим разницу:

```text
$ kubectl -n learn-03 diff -f pod2.yaml
The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than `spec.containers[*].image`,`spec.initContainers[*].image`,`spec.activeDeadlineSeconds`,`spec.tolerations` (only additions to existing tolerations),`spec.terminationGracePeriodSeconds` (allow it to be set to 1 if it was previously negative)
@@ -104,7 +104,7 @@
     {
      "Name": "",
      "HostPort": 0,
-     "ContainerPort": 8080,
+     "ContainerPort": 8081,
```

`diff` показал изменение, и тут же сервер предупредил, что реальный `apply` такого поля у существующего пода не примет: большинство полей пода неизменяемо. Этот результат полезен до применения: ошибка видна, пока кластер не тронут.

`--dry-run=client -o yaml` не обращается к кластеру и печатает объект, который был бы создан. Так делают заготовки манифестов:

```text
$ kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  labels:
    app: demo
  name: demo
spec:
  replicas: 1
  selector:
    matchLabels:
      app: demo
  strategy: {}
  template:
    metadata:
      labels:
        app: demo
    spec:
      containers:
      - image: nginx:1.29
        name: nginx
        resources: {}
status: {}
```

Строки `strategy: {}`, `resources: {}` и `status: {}` — пустые значения, их можно удалить. Режим `--dry-run=server` отправляет запрос на API server, который проверяет его, но не сохраняет.

## explain: справка по полям

`kubectl explain` показывает описание любого поля прямо из схемы API вашего кластера:

```text
$ kubectl explain deployment.spec.strategy.rollingUpdate
GROUP:      apps
KIND:       Deployment
VERSION:    v1

FIELD: rollingUpdate <RollingUpdateDeployment>

DESCRIPTION:
    Rolling update config params. Present only if DeploymentStrategyType =
    RollingUpdate.
…
FIELDS:
  maxSurge	<IntOrString>
```

Это быстрее поиска в документации и всегда соответствует версии вашего кластера. Рекурсивно все поля: `kubectl explain pod.spec --recursive`.

## Форматы вывода

| Флаг | Результат |
|---|---|
| `-o wide` | дополнительные столбцы (IP, нода) |
| `-o yaml`, `-o json` | полный объект |
| `-o name` | только `тип/имя` |
| `-o jsonpath='…'` | выбранные поля по выражению JSONPath |
| `-o custom-columns=ЗАГОЛОВОК:путь,…` | собственная таблица |
| `-o go-template='…'` | шаблон Go |

```text
$ kubectl -n learn-02 get pods -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.podIP}{"\n"}{end}'
web-689b9bdbdd-7hpbd	10.244.0.5
web-689b9bdbdd-r4tkv	10.244.0.7
$ kubectl -n learn-02 get pods -o custom-columns=NAME:.metadata.name,IMAGE:.spec.containers[0].image,NODE:.spec.nodeName
NAME                   IMAGE        NODE
web-689b9bdbdd-7hpbd   nginx:1.29   learn-control-plane
web-689b9bdbdd-r4tkv   nginx:1.29   learn-control-plane
$ kubectl get ns learn-03 -o go-template='{{.metadata.name}}{{"\n"}}'
learn-03
```

## Селекторы

Метки задаются при создании или командой `label`; выбор — флагом `-l`. Поддерживаются равенство (`app=hello`), несколько условий через запятую (логическое «и») и множества (`tier in (a,b)`, `tier notin (…)`, а также просто `key` и `!key`):

```text
$ kubectl -n learn-03 get pods -l app=hello,tier=backend --show-labels
$ kubectl -n learn-03 get pods -l 'tier in (frontend)'
No resources found in learn-03 namespace.
$ kubectl -n learn-03 delete pod -l app=demo        # удалить всё, что подходит под селектор
```

Селекторы лежат в основе связей Kubernetes: Service выбирает поды, Deployment — свои ReplicaSet-ы.

## Несколько кластеров и KUBECONFIG

По умолчанию читается `~/.kube/config`. Переменная окружения `KUBECONFIG` позволяет подключить несколько файлов; kubectl объединяет их. Разделитель — двоеточие на Linux и macOS и точка с запятой на Windows. Опыт на Windows:

```text
$ kind get kubeconfig --name learn > kc-learn.yaml
$ KUBECONFIG="kc-learn.yaml;C:\Users\petel\.kube\config" kubectl config get-contexts -o name
docker-desktop
kind-learn
```

Попытка записать вместо точки с запятой двоеточие на Windows даёт `The filename, directory name, or volume label syntax is incorrect`. Команда `kind get kubeconfig --name <имя>` печатает kubeconfig кластера, не меняя ваш файл. Флаг `--kubeconfig` указывает файл для одной команды.

Всегда понимайте, где вы: `kubectl config current-context` и `kubectl --context <имя> …`.

## Многоузловой kind

Файл конфигурации описывает ноды:

```yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
  - role: worker
```

```text
$ kind create cluster --name learn-multi --config kind-multi.yaml --wait 120s
$ kubectl --context kind-learn-multi get nodes
NAME                        STATUS     ROLES           AGE   VERSION
learn-multi-control-plane   Ready      control-plane   26s   v1.37.0
learn-multi-worker          NotReady   <none>          11s   v1.37.0
learn-multi-worker2         NotReady   <none>          11s   v1.37.0
```

Сразу после создания рабочие ноды показали `NotReady`: `--wait` ждёт только control plane. Через 15 секунд обе стали `Ready`:

```text
$ kubectl --context kind-learn-multi get nodes
learn-multi-worker          Ready      <none>          32s   v1.37.0
learn-multi-worker2         Ready      <none>          32s   v1.37.0
```

Метка роли `worker` у нод пустая: `ROLES  <none>`. Такой кластер нужен для опытов с планированием и отказами нод (модуль 18). Удаляется он командой `kind delete cluster --name learn-multi`, которая убирает и свой контекст из kubeconfig.

## Образы в kind

Образ, собранный в локальном Docker, ноде kind не виден. Загрузка:

```text
$ kind load docker-image hello:multi --name learn
Image: "hello:multi" with ID "sha256:abf0c4…" not yet present on node "learn-control-plane", loading...
```

Если образ изменился и вы собрали его заново с тем же тегом, загрузите его снова: kind сравнивает ID и перезагрузит. Используйте явный тег и `imagePullPolicy: IfNotPresent`: для тега `latest` политика по умолчанию `Always` ([модуль 01](../01-containers/expert.ru.md#подвох-latest-в-kubernetes)).

## Лучшие практики

- Один namespace на модуль или эксперимент (`learn-NN`), удаление одной командой.
- Перед `apply` — `kubectl diff`, перед опасными командами — проверка контекста.
- Храните манифесты в файлах, а императивные команды используйте для заготовок (`--dry-run=client -o yaml`).
- Не используйте `latest`; задавайте `imagePullPolicy` явно для локальных образов.
- Для скриптов выбирайте стабильные форматы: `-o name`, `-o jsonpath`, а не разбор текстовых таблиц.

## Частые ошибки

- ❌ `kubectl create` вместо `apply` в повторяемых сценариях → ✅ `apply`: повторяемо и идемпотентно.
- ❌ Править манифест неизменяемого поля у существующего пода → ✅ пересоздать под; поля пода почти все неизменяемы, а менять образ и подобные поля нужно через контроллер (Deployment).
- ❌ Разбирать вывод `kubectl get` текстом → ✅ `-o json`, `jsonpath`, `custom-columns`.
- ❌ Забыть, что `kind create cluster` сменил контекст → ✅ проверить `current-context`; после опытов вернуть привычный через `kubectl config use-context`.
- ❌ Считать кластер готовым сразу после `kind create` → ✅ проверить `kubectl get nodes`: рабочие ноды могут быть `NotReady` ещё несколько секунд.

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): kubectl — тонкая оболочка над HTTP; задачи с выводом удобно решать теми же `grep`, `awk` и `jq`.
- [Python](../../../python/overview.ru.md) и [C#](../../../csharp/overview.ru.md): то, что делает kubectl, делают и клиентские библиотеки Kubernetes через тот же REST API.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Предыдущие модули: [01 · Контейнеры и Docker](../01-containers/overview.ru.md), [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)

## Источники

- [kubectl apply](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_apply/)
- [kubectl diff](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_diff/)
- [Declarative management of objects using kubectl](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/declarative-config/)
- [kubectl explain](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_explain/)
- [JSONPath support](https://kubernetes.io/docs/reference/kubectl/jsonpath/)
- [Configure access to multiple clusters](https://kubernetes.io/docs/tasks/access-application-cluster/configure-access-multiple-clusters/)
- [kind: Configuration](https://kind.sigs.k8s.io/docs/user/configuration/)
- [kind: Working with kind (load docker-image)](https://kind.sigs.k8s.io/docs/user/quick-start/#loading-an-image-into-your-cluster)
