---
title: "Упражнения: локальный кластер и kubectl"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, namespace, apply, diff, multi-node]
status: draft
updated: 2026-11-22
---

# Упражнения: локальный кластер и kubectl

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужны Docker, `kind` и `kubectl`; образ `hello:multi` — из [модуля 01](../01-containers/basics.ru.md). Все опыты выполняются в одноразовых кластере и namespace; убрать за собой: `kubectl delete namespace learn-03`, `kind delete cluster --name learn`.

## 1. 🟢 Первый под

**Задача:** создай кластер `learn`, namespace `learn-03`, загрузи в кластер образ `hello:multi`, запусти под `hello` из манифеста и обратись к нему через `port-forward`.

**Ожидаемый результат:**

```text
pod/hello condition met
Hello from hello
```

<details><summary>Подсказка</summary>

Образ из локального Docker нужно загрузить `kind load docker-image`, а в манифесте указать `imagePullPolicy: IfNotPresent`. Для `port-forward` нужен запущенный под.

</details>

<details><summary>Решение</summary>

```yaml
# pod.yaml
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

```bash
kind create cluster --name learn
kubectl create namespace learn-03
kind load docker-image hello:multi --name learn
kubectl -n learn-03 apply -f pod.yaml
kubectl -n learn-03 wait --for=condition=Ready pod/hello --timeout=60s
kubectl -n learn-03 port-forward pod/hello 18090:8080 &
curl -s localhost:18090
```

```text
pod/hello condition met
Hello from hello
```

В Kubernetes hostname контейнера равен имени пода, поэтому ответ `Hello from hello`.

</details>

## 2. 🟢 Контексты и namespace

**Задача:** выведи список контекстов, определи текущий, запиши `learn-03` как namespace по умолчанию для контекста `kind-learn` (чтобы не писать `-n`), проверь, что `kubectl get pods` видит под `hello`, и верни namespace по умолчанию.

**Ожидаемый результат:** `kubectl get pods` без `-n` показывает под `hello`.

<details><summary>Подсказка</summary>

`kubectl config set-context <контекст> --namespace=<ns>`. Чтобы вернуть значение по умолчанию, задай пустой namespace.

</details>

<details><summary>Решение</summary>

```bash
kubectl config get-contexts
kubectl config current-context
kubectl config set-context kind-learn --namespace=learn-03
kubectl --context kind-learn get pods
kubectl config set-context kind-learn --namespace=
```

```text
Context "kind-learn" modified.
NAME    READY   STATUS    RESTARTS   AGE
hello   1/1     Running   0          1m
```

Namespace записан в контексте, поэтому `-n` не нужен. Флаг `--context` позволяет выполнить команду в нужном контексте, не переключая текущий.

</details>

## 3. 🟡 create, apply и diff

**Задача:** удали под `hello` и создай его через `create` два раза подряд. Затем запусти `apply` для того же файла дважды. Измени в копии манифеста `containerPort` и посмотри `kubectl diff`. Объясни три разных результата.

**Ожидаемый результат:** `AlreadyExists` на втором `create`; `configured` при первом `apply` (с предупреждением), `unchanged` при втором; `diff` показывает изменение порта.

<details><summary>Подсказка</summary>

Предупреждение про `last-applied-configuration` появляется, когда `apply` применяется к объекту, созданному через `create`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-03 delete pod hello --now
kubectl -n learn-03 create -f pod.yaml
kubectl -n learn-03 create -f pod.yaml
kubectl -n learn-03 apply -f pod.yaml
kubectl -n learn-03 apply -f pod.yaml
sed 's/containerPort: 8080/containerPort: 8081/' pod.yaml > pod2.yaml
kubectl -n learn-03 diff -f pod2.yaml
```

```text
pod/hello created
Error from server (AlreadyExists): error when creating "pod.yaml": pods "hello" already exists
Warning: resource pods/hello is missing the kubectl.kubernetes.io/last-applied-configuration annotation which is required by kubectl apply. …
pod/hello configured
pod/hello unchanged
The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than …
@@ -104,7 +104,7 @@
-     "ContainerPort": 8080,
+     "ContainerPort": 8081,
```

`create` не идемпотентен, `apply` идемпотентен и при первом запуске добавил аннотацию, а `diff` показал разницу и заодно предупредил, что у существующего пода порт менять нельзя.

</details>

## 4. 🟡 Заготовка манифеста и вывод

**Задача:** сгенерируй манифест Deployment `demo` на `nginx:1.29` без создания объекта, сохрани в файл, примени его, а затем выведи имена подов и IP одной командой с `jsonpath` и таблицу с узлом через `custom-columns`.

**Ожидаемый результат:** строки вида `demo-…<TAB>10.244.0.…` и таблица `NAME  IMAGE  NODE`.

<details><summary>Подсказка</summary>

`--dry-run=client -o yaml` печатает манифест. Для `jsonpath` используй `range .items[*]`.

</details>

<details><summary>Решение</summary>

```bash
kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml > demo.yaml
kubectl -n learn-03 apply -f demo.yaml
kubectl -n learn-03 rollout status deploy/demo
kubectl -n learn-03 get pods -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.podIP}{"\n"}{end}'
kubectl -n learn-03 get pods -o custom-columns=NAME:.metadata.name,IMAGE:.spec.containers[0].image,NODE:.spec.nodeName
```

```text
demo-645c7f9dc-kjxff	10.244.0.24
NAME                   IMAGE        NODE
demo-645c7f9dc-kjxff   nginx:1.29   learn-control-plane
```

Манифест из `--dry-run=client` содержит лишние пустые поля (`strategy: {}`, `status: {}`): их можно удалить. Для скриптов `jsonpath` и `custom-columns` надёжнее, чем разбор текстовой таблицы.

</details>

## 5. 🔴 Многоузловой кластер

**Задача:** создай кластер `learn-multi` из 1 control plane и 2 рабочих нод, дождись готовности всех нод, загрузи образ `hello:multi` и запусти Deployment на 3 реплики. Покажи, на каких нодах оказались поды, и объясни, почему не на control plane. Затем удали кластер.

**Ожидаемый результат:** поды только на `learn-multi-worker` и `learn-multi-worker2`.

<details><summary>Подсказка</summary>

Роли нод задаются в файле конфигурации `kind`. `kubectl wait --for=condition=Ready node --all` ждёт готовности. У control plane проверь `.spec.taints`.

</details>

<details><summary>Решение</summary>

```yaml
# kind-multi.yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
  - role: worker
```

```bash
kind create cluster --name learn-multi --config kind-multi.yaml --wait 120s
kubectl --context kind-learn-multi wait --for=condition=Ready node --all --timeout=120s
kind load docker-image hello:multi --name learn-multi
kubectl --context kind-learn-multi create namespace learn-03
kubectl --context kind-learn-multi -n learn-03 create deployment hello --image=hello:multi --replicas=3
kubectl --context kind-learn-multi -n learn-03 rollout status deploy/hello --timeout=90s
kubectl --context kind-learn-multi -n learn-03 get pods -o custom-columns=NAME:.metadata.name,NODE:.spec.nodeName
kubectl --context kind-learn-multi get nodes -o jsonpath='{range .items[*]}{.metadata.name}{" taints="}{.spec.taints}{"\n"}{end}'
kind delete cluster --name learn-multi
```

```text
hello-5cf47849b9-2g9qq   learn-multi-worker2
hello-5cf47849b9-smm56   learn-multi-worker
hello-5cf47849b9-zgn5l   learn-multi-worker2
learn-multi-control-plane taints=[{"effect":"NoSchedule","key":"node-role.kubernetes.io/control-plane"}]
```

На ноде control plane стоит taint `node-role.kubernetes.io/control-plane:NoSchedule`, поэтому scheduler не размещает на ней обычные поды. `kind load docker-image` загрузил образ во все три ноды (в выводе по строке на каждую), поэтому поды стартуют на любой ноде. Для тега `multi` политика загрузки по умолчанию `IfNotPresent`, и kubelet не ходит в Docker Hub; для `latest` было бы `Always` ([модуль 01](../01-containers/expert.ru.md#подвох-latest-в-kubernetes)).

</details>
