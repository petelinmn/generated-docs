---
title: "Локальный кластер и kubectl: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, namespace, manifest, pod, port-forward]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Локальный кластер и kubectl: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Контейнеры и Docker](../01-containers/overview.ru.md), [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md) · **Время:** ~2 ч

## Зачем это нужно

Чтобы пробовать Kubernetes, нужен кластер, который не жалко сломать. kind запускает его на вашей машине: каждая нода — контейнер Docker, внутри которого работают kubelet и containerd. Взаимодействие идёт через `kubectl`.

## Установка

Нужны Docker, `kind` и `kubectl` (в треке — внутри WSL2, см. [окружение](../../overview.ru.md#окружение)).

```text
$ docker version --format 'client {{.Client.Version}} server {{.Server.Version}}'
client 29.6.1 server 29.6.1
$ kubectl version --client
Client Version: v1.36.1
$ kind version
kind v0.34.0-alpha+4530a2a5af91cd go1.27.4 windows/amd64
```

Бинарные файлы и способы установки — на страницах [kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installation) и [kubectl](https://kubernetes.io/docs/tasks/tools/). Версия kubectl должна отличаться от версии кластера не более чем на одну минорную (см. [экспертную теорию](expert.ru.md#версии-и-совместимость)).

## Создать кластер

```text
$ kind create cluster --name learn
 ✓ Starting control-plane 🕹️
 ✓ Installing CNI 🔌
 ✓ Installing StorageClass 💾
 ✓ Waiting ≤ 3m0s for control-plane = Ready ⏳
 • Ready after 16s 💚
Set kubectl context to "kind-learn"
$ kubectl get nodes -o wide
NAME                  STATUS   ROLES           AGE   VERSION   INTERNAL-IP   OS-IMAGE                       CONTAINER-RUNTIME
learn-control-plane   Ready    control-plane   22s   v1.37.0   172.18.0.9    Debian GNU/Linux 13 (trixie)   containerd://2.3.4
```

Имя кластера — `learn`, имя ноды — `learn-control-plane`. Нода — контейнер Docker: `docker ps` покажет `learn-control-plane`.

Список и удаление кластеров:

```bash
kind get clusters
kind delete cluster --name learn
```

## kubeconfig и контексты

`kubectl` берёт адрес API server и учётные данные из файла kubeconfig (по умолчанию `~/.kube/config`). В нём три списка:

| Раздел | Что хранит |
|---|---|
| `clusters` | адрес API server и сертификат центра сертификации |
| `users` | учётные данные (сертификат, токен) |
| `contexts` | связка «кластер + пользователь + namespace по умолчанию» |

Контекст — это то, на какой кластер и как действует kubectl прямо сейчас:

```text
$ kubectl config get-contexts
CURRENT   NAME             CLUSTER          AUTHINFO         NAMESPACE
          docker-desktop   docker-desktop   docker-desktop
*         kind-learn       kind-learn       kind-learn
$ kubectl config use-context docker-desktop
Switched to context "docker-desktop".
```

> ⚠️ `kind create cluster` переключает текущий контекст на новый кластер. Прежде чем выполнять опасную команду (`delete`, `apply`), проверьте `kubectl config current-context`.

Чтобы разово выполнить команду в другом контексте, не переключая его, укажите флаг `--context`: `kubectl --context kind-learn get nodes`.

## Namespace

Namespace — «папка» для объектов кластера. Мы создаём по одному на модуль:

```bash
kubectl create namespace learn-03
kubectl -n learn-03 get pods            # -n выбирает namespace для одной команды
```

Если указывать `-n` каждый раз лень, namespace по умолчанию можно записать в контекст:

```text
$ kubectl config set-context kind-learn --namespace=learn-03
Context "kind-learn" modified.
$ kubectl get pods
```

Вернуть по умолчанию: `kubectl config set-context kind-learn --namespace=`. Убрать за собой: `kubectl delete namespace learn-03` — удалит всё, что внутри.

## Первый манифест

Манифест — YAML-описание объекта. Минимальный Pod:

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: hello
  labels:
    app: hello
spec:
  containers:
    - name: app
      image: hello:multi
      imagePullPolicy: IfNotPresent
      ports:
        - containerPort: 8080
```

Обязательные поля у любого объекта: `apiVersion`, `kind`, `metadata` (с `name`), а у большинства ещё `spec`. Образ `hello:multi` — из [модуля 01](../01-containers/basics.ru.md) (лежит локально в Docker). Кластер его не видит, пока мы его не загрузим:

```text
$ kubectl -n learn-03 apply -f pod.yaml
pod/hello created
$ kubectl -n learn-03 get pod hello
NAME    READY   STATUS         RESTARTS   AGE
hello   0/1     ErrImagePull   0          12s
$ kubectl -n learn-03 describe pod hello
 …
 Warning  Failed  kubelet  Failed to pull image "hello:multi": failed to resolve reference "docker.io/library/hello:multi": pull access denied, repository does not exist or may require authorization
```

kubelet пошёл за образом в Docker Hub, где такого нет. Загрузим образ в ноду и пересоздадим под:

```text
$ kind load docker-image hello:multi --name learn
$ kubectl -n learn-03 delete pod hello
$ kubectl -n learn-03 apply -f pod.yaml
$ kubectl -n learn-03 wait --for=condition=Ready pod/hello --timeout=60s
pod/hello condition met
```

## Базовые команды

| Команда | Что делает |
|---|---|
| `kubectl get <тип> [имя]` | список объектов; `-o wide` — больше столбцов |
| `kubectl describe <тип> <имя>` | подробности и события объекта |
| `kubectl logs <под>` | логи (stdout контейнера) |
| `kubectl exec <под> -- <команда>` | выполнить команду в контейнере |
| `kubectl port-forward pod/<имя> порт:порт` | пробросить порт на вашу машину |
| `kubectl apply -f файл.yaml` | создать или обновить объект из манифеста |
| `kubectl delete -f файл.yaml` / `delete <тип> <имя>` | удалить |

Примеры на нашем поде:

```text
$ kubectl -n learn-03 logs hello
listening on :8080
$ kubectl -n learn-03 port-forward pod/hello 18090:8080 &
$ curl -s localhost:18090
Hello from hello
```

В ответе имя хоста — `hello`: в Kubernetes hostname контейнера равен имени пода. Порт 8080 внутри пода мы увидели на `localhost:18090` своей машины.

```text
$ kubectl -n learn-03 get pod hello -o wide
NAME    READY   STATUS    IP            NODE
hello   1/1     Running   10.244.0.18   learn-control-plane
```

Образ `hello:multi` собран на `scratch` и не содержит `sh`, поэтому `kubectl exec` ведёт себя так же, как `docker exec` ([модуль 01](../01-containers/expert.ru.md#подвох-в-образе-на-scratch-нет-shell)):

```text
$ kubectl -n learn-03 exec hello -- sh
error: Internal error occurred: … OCI runtime exec failed: exec failed: unable to start container process: exec: "sh": executable file not found in $PATH
```

А если вызвать `exec` сразу после создания пода, когда контейнер ещё не запущен, ответ будет другим: `error: unable to upgrade connection: container not found ("app")`. Дождитесь `Running`.

## Метки

У объектов есть метки (labels) — пары ключ-значение. По ним выбирают объекты (в том числе Service и Deployment):

```text
$ kubectl -n learn-03 label pod hello tier=backend
$ kubectl -n learn-03 get pods -l app=hello,tier=backend --show-labels
NAME    READY   STATUS    RESTARTS   AGE   LABELS
hello   1/1     Running   0          1s    app=hello,tier=backend
$ kubectl -n learn-03 get pods -l 'tier in (frontend)'
No resources found in learn-03 namespace.
```

## Ошибки новичка

- ❌ Выполнить команду не в том кластере → ✅ перед опасными командами проверяйте `kubectl config current-context`, при необходимости используйте `--context`.
- ❌ Забыть namespace → ✅ без `-n` команда работает в `default` и «не находит» ваши объекты.
- ❌ Ждать, что образ из локального Docker виден кластеру → ✅ `kind load docker-image` и явный тег.
- ❌ Править объект и делать `kubectl create` повторно → ✅ повторное `create` даст `AlreadyExists`; для повторяемости используйте `apply`.
- ❌ Оставлять кластеры и namespace-ы после опытов → ✅ `kubectl delete namespace learn-NN`, `kind delete cluster --name …`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `apply` и `diff`, `explain`, несколько кластеров
- Предыдущий модуль: [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)
- Контейнеры и образы: [01 · Контейнеры и Docker](../01-containers/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [kind: Quick Start](https://kind.sigs.k8s.io/docs/user/quick-start/)
- [kind: Loading an image](https://kind.sigs.k8s.io/docs/user/quick-start/#loading-an-image-into-your-cluster)
- [Install kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Organizing cluster access using kubeconfig](https://kubernetes.io/docs/concepts/configuration/organize-cluster-access-kubeconfig/)
- [kubectl: шпаргалка](https://kubernetes.io/docs/reference/kubectl/quick-reference/)
- [Labels and Selectors](https://kubernetes.io/docs/concepts/overview/working-with-objects/labels/)
- [Namespaces](https://kubernetes.io/docs/concepts/overview/working-with-objects/namespaces/)
