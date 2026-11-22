---
title: "Локальный кластер и kubectl: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, plugins, jsonpath, discovery, apply, diff]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Локальный кластер и kubectl: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Устройство kubeconfig

Файл kubeconfig — YAML с `kind: Config`. Контекст `kind-learn` без секретов (`kubectl config view --minify`):

```text
apiVersion: v1
clusters:
- cluster:
    certificate-authority-data: DATA+OMITTED
    server: https://127.0.0.1:PORT
  name: kind-learn
contexts:
- context:
    cluster: kind-learn
    user: kind-learn
  name: kind-learn
current-context: kind-learn
kind: Config
users:
- name: kind-learn
  user:
    client-certificate-data: DATA+OMITTED
    client-key-data: DATA+OMITTED
```

kind аутентифицирует вас клиентским сертификатом (в файле — сертификат и ключ). Адрес `127.0.0.1:PORT` — порт API server ноды, который Docker пробрасывает на вашу машину; порт случайный. Облачные кластеры обычно используют токены или exec-плагины, которые получают токен командой.

Порядок поиска файла: флаг `--kubeconfig`, затем переменная `KUBECONFIG` (список файлов, объединяемых kubectl), затем `~/.kube/config`.

### Что делает kubectl при одной команде

1. Находит kubeconfig, берёт текущий контекст (или `--context`).
2. Определяет, какому ресурсу соответствует имя в команде (`pods`, `po`, `deployments`). Для этого он запрашивает у сервера список ресурсов (discovery) и кэширует его в `~/.kube/cache/discovery`, отдельно для каждого сервера. На нашем стенде в кэше лежат папки по адресам `127.0.0.1_<порт>` для разных кластеров kind.
3. Отправляет HTTP-запрос к API server.
4. Выводит результат. Для `kubectl get` таблицу формирует сервер: kubectl просит её заголовком `Accept: application/json;as=Table;v=v1;g=meta.k8s.io,…`, поэтому столбцы одинаковы у любого клиента и у любых ресурсов, включая CRD.

Увидеть запросы можно флагом `-v`: `-v=6` печатает метод, URL и статус, `-v=8` — ещё и заголовки.

### Версии и совместимость

Допускается отличие kubectl от API server на одну минорную версию в обе стороны. В нашем стенде kubectl `1.36.1` работает с сервером `1.37.0`. Для kind версия Kubernetes определяется образом ноды `kindest/node:vX.Y.Z` (у нас `v1.37.0`); другой образ задаётся флагом `--image` или полем `image` в конфиге.

> ⚠️ Проверить: наличие образов `kindest/node` для конкретных версий и совместимость версии kind с ними сверяйте с релизами kind; в этом материале проверена только пара kind v0.34 и `kindest/node:v1.37.0`.

## Справочник

### Команды kubectl

Список получен из `kubectl --help` на kubectl 1.36.1.

| Группа | Команды |
|---|---|
| Создание | `create`, `apply`, `replace`, `run`, `expose`, `kustomize` |
| Просмотр | `get`, `describe`, `explain`, `events`, `logs`, `top`, `api-resources`, `api-versions`, `cluster-info`, `version` |
| Изменение | `edit`, `patch`, `label`, `annotate`, `set`, `scale`, `autoscale`, `rollout`, `delete`, `taint` |
| Доступ к контейнерам | `exec`, `attach`, `port-forward`, `cp`, `proxy`, `debug` |
| Обслуживание нод | `cordon`, `drain`, `taint` |
| Проверки | `diff`, `wait`, `auth` (`auth can-i`), `certificate` |
| Настройка клиента | `config`, `plugin`, `completion` |

### Глобальные и частые флаги

| Флаг | Смысл |
|---|---|
| `-n`, `--namespace` / `-A` | namespace / все namespace |
| `--context`, `--kubeconfig` | контекст / файл kubeconfig для одной команды |
| `-o` | формат вывода (см. ниже) |
| `-l`, `--field-selector` | выбор по меткам / по полям (`status.phase=Running`) |
| `-w` | следить за изменениями (watch) |
| `--show-labels`, `--sort-by` | показать метки / сортировка (например, `.metadata.creationTimestamp`) |
| `--dry-run=none\|client\|server` | не выполнять (клиентская или серверная проверка) |
| `--as` | выполнить от имени пользователя или ServiceAccount |
| `-v` | подробность лога (6 — запросы, 8 — ещё и заголовки) |

### Форматы вывода

| Формат | Для чего |
|---|---|
| (пусто) | таблица, форматируемая сервером |
| `wide` | таблица с доп. столбцами |
| `yaml`, `json` | полный объект |
| `name` | `тип/имя`, удобно в скриптах |
| `jsonpath`, `jsonpath-file` | выборка полей по выражению |
| `custom-columns`, `custom-columns-file` | своя таблица |
| `go-template`, `go-template-file` | шаблоны Go |

### Команды kind

Список из `kind --help`.

| Команда | Что делает |
|---|---|
| `kind create cluster [--name N] [--config файл] [--image образ] [--wait 120s]` | создать кластер |
| `kind delete cluster --name N` | удалить кластер (и его контекст) |
| `kind get clusters\|nodes\|kubeconfig` | список кластеров / нод / kubeconfig |
| `kind export kubeconfig\|logs` | записать kubeconfig / собрать логи |
| `kind load docker-image\|image-archive` | загрузить образ с хоста в ноды |
| `kind build node-image` | собрать образ ноды |

Файл конфигурации (`kind.x-k8s.io/v1alpha4`) в этом материале проверялся только в части `nodes` с ролями `control-plane` и `worker`. Остальные поля (`extraPortMappings`, `networking`, `containerdConfigPatches`, `kubeadmConfigPatches` и др.) приведены из документации kind и не запускались.

> ⚠️ Проверить: поля конфигурации kind `extraPortMappings`, `networking`, `containerdConfigPatches`, `kubeadmConfigPatches` не запускались в этом материале; сверяйте с [документацией kind](https://kind.sigs.k8s.io/docs/user/configuration/).

### Плагины kubectl

Любой исполняемый файл `kubectl-<имя>` в `PATH` становится командой `kubectl <имя>`. Плагин получает аргументы командной строки:

```text
$ kubectl hello a b
plugin hello, args: a b
```

Список найденных плагинов: `kubectl plugin list`. На Windows плагин должен иметь расширение из `PATHEXT` (у нас — `kubectl-hello.cmd`): файл без расширения `kubectl plugin list` показал, но запустить не смог (`error: unknown command "hello" for "kubectl"`). Менеджер плагинов — krew.

## Подвохи

### Подвох: kind create cluster меняет текущий контекст

```text
$ kubectl config current-context
docker-desktop            # ваш прежний контекст (у нас — Docker Desktop)
$ kind create cluster --name learn
Set kubectl context to "kind-learn"
$ kubectl config current-context
kind-learn
```

После создания любого кластера kind ваши команды идут в него. Если до этого вы работали с другим кластером, команда `delete` или `apply` без проверки может попасть не туда. Возврат: `kubectl config use-context <прежний контекст>`; у нас после опытов выполнено `kubectl config use-context docker-desktop`.

### Подвох: образ не найден (ErrImagePull)

Под из локального образа без `kind load` не стартует: kubelet идёт в Docker Hub (`pull access denied, repository does not exist …`). Подробности — в [базовой теории](basics.ru.md#первый-манифест).

### Подвох: diff предупреждает, что apply не пройдёт

`kubectl diff` на изменении неизменяемого поля пода выводит и разницу, и `The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than …`. Сообщение означает, что настоящий `apply` с таким изменением тоже будет отклонён ([продвинутая теория](advanced.ru.md#diff-и-dry-run)).

### Подвох: apply поверх create выдаёт предупреждение

```text
$ kubectl apply -f pod.yaml     # объект создан через create
Warning: resource pods/hello is missing the kubectl.kubernetes.io/last-applied-configuration annotation which is required by kubectl apply. …
pod/hello configured
```

Предупреждение безвредно (аннотация добавится), но показывает, что объект управлялся не декларативно с самого начала.

### Подвох: get all показывает не всё

```text
$ kubectl -n learn-03 create configmap cm1 --from-literal=a=b
$ kubectl -n learn-03 get all
NAME        READY   STATUS    RESTARTS   AGE
pod/hello   1/1     Running   0          56s
$ kubectl -n learn-03 get cm cm1 -o name
configmap/cm1
```

`all` — это не «все объекты», а небольшой набор типов (поды, сервисы, деплойменты и подобные). ConfigMap в `get all` не попал. Список всех типов в namespace — через `kubectl api-resources --verbs=list --namespaced -o name` и цикл по ним.

### Подвох: exec сразу после создания пода

```text
$ kubectl -n learn-03 apply -f pod.yaml
$ kubectl -n learn-03 exec hello -- ls
error: unable to upgrade connection: container not found ("app")
```

Контейнер ещё не запущен. Это не ошибка образа: дождитесь `Running` (`kubectl wait --for=condition=Ready pod/hello`).

### Подвох: рабочие ноды NotReady сразу после kind create

```text
$ kind create cluster --name learn-multi --config kind-multi.yaml --wait 120s
$ kubectl --context kind-learn-multi get nodes
learn-multi-control-plane   Ready      control-plane   26s   v1.37.0
learn-multi-worker          NotReady   <none>          11s   v1.37.0
learn-multi-worker2         NotReady   <none>          11s   v1.37.0
```

`--wait` ждёт готовности control plane, а не рабочих нод. Подождите или используйте `kubectl wait --for=condition=Ready node --all`.

### Подвох: разделитель в KUBECONFIG зависит от ОС

На Windows разделитель — `;`: запись `kc-learn.yaml:C:\…\config` приводит к `error: error loading config file … The filename, directory name, or volume label syntax is incorrect`. На Linux и macOS разделитель — `:`.

### Подвох: одиночный под не воскресает

```text
$ kubectl -n learn-03 delete pod hello --now
$ kubectl -n learn-03 get pods
No resources found in learn-03 namespace.
```

Под, созданный напрямую, не принадлежит контроллеру: никто не восстановит его после удаления. Для самовосстановления используйте Deployment ([модуль 02](../02-architecture/basics.ru.md#самовосстановление)).

## Компромиссы и альтернативы

| Инструмент | Плюсы | Минусы |
|---|---|---|
| kind | кластер из контейнеров, быстрый старт, несколько нод, удобен для CI | нужны Docker и ресурсы; не продакшен |
| minikube | драйверы (VM, Docker), аддоны | тяжелее kind |
| Kubernetes в Docker Desktop | встроенный, включается галочкой | один узел, версия задаётся Docker Desktop |
| k3d, k3s | лёгкий дистрибутив | отличия от стандартного Kubernetes |
| Облачный кластер | настоящая среда | стоит денег, нужен доступ |

kubectl можно заменить клиентскими библиотеками (Go, Python, C#) и терминальными интерфейсами вроде k9s: все они используют тот же API.

## Вопросы с собеседований

**1. Что такое контекст kubeconfig?**

<details><summary>Ответ</summary>

Связка «кластер + пользователь + namespace по умолчанию». kubectl выполняет команды в текущем контексте, который переключают `kubectl config use-context`, а для одной команды переопределяют флагом `--context`.

</details>

**2. (С подвохом) Вы запустили `kind create cluster` и затем `kubectl delete deployment web`. В какой кластер ушла команда?**

<details><summary>Ответ</summary>

В новый кластер kind: `kind create` переключает текущий контекст на него. Поэтому перед опасными командами проверяют `kubectl config current-context`.

</details>

**3. В чём разница между `kubectl create` и `kubectl apply`?**

<details><summary>Ответ</summary>

`create` создаёт объект и даёт `AlreadyExists`, если он есть (императивно). `apply` приводит объект к описанию в файле и идемпотентен: повторный запуск даёт `unchanged`. `apply` хранит последний применённый манифест в аннотации `last-applied-configuration`.

</details>

**4. Как увидеть, что изменит `apply`, не применяя его?**

<details><summary>Ответ</summary>

`kubectl diff -f файл.yaml` или `--dry-run=server`. Если поле неизменяемое, `diff` дополнительно выдаст ошибку валидации.

</details>

**5. Как получить заготовку манифеста без создания объекта?**

<details><summary>Ответ</summary>

`kubectl create … --dry-run=client -o yaml`, например `kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml`. Лишние пустые поля (`strategy: {}`, `status: {}`) можно удалить.

</details>

**6. Как узнать описание поля манифеста?**

<details><summary>Ответ</summary>

`kubectl explain deployment.spec.strategy.rollingUpdate` (рекурсивно: `--recursive`). Описание берётся из схемы API вашего кластера.

</details>

**7. (С подвохом) Образ собран в Docker, под ссылается на него, но статус `ErrImagePull`. Что делать?**

<details><summary>Ответ</summary>

Кластер kind не видит образы локального Docker. Загрузить командой `kind load docker-image имя:тег --name кластер`, использовать явный тег (не `latest`, иначе политика `Always` пойдёт в реестр) и `imagePullPolicy: IfNotPresent`.

</details>

**8. Как подключить несколько kubeconfig?**

<details><summary>Ответ</summary>

Через `KUBECONFIG` со списком файлов (разделитель `:` на Linux и macOS, `;` на Windows) или по одному через `--kubeconfig`. `kind get kubeconfig --name N` печатает kubeconfig кластера, не трогая основной файл.

</details>

**9. (С подвохом) Что покажет `kubectl get all`?**

<details><summary>Ответ</summary>

Только небольшой набор типов (поды, сервисы, деплойменты и подобные), не все объекты namespace. В опыте созданный ConfigMap в вывод не попал.

</details>

**10. Как kubectl формирует таблицу для `get`?**

<details><summary>Ответ</summary>

Сервер возвращает таблицу сам: kubectl запрашивает её заголовком `Accept: application/json;as=Table;v=v1;g=meta.k8s.io,…`. Поэтому столбцы одинаковы у всех клиентов и работают для CRD.

</details>

**11. Как расширить kubectl своей командой?**

<details><summary>Ответ</summary>

Положить в `PATH` исполняемый файл `kubectl-<имя>`: он станет командой `kubectl <имя>`. Список — `kubectl plugin list`. На Windows нужно расширение из `PATHEXT`, иначе файл не запустится.

</details>

**12. Что произойдёт, если удалить под, созданный напрямую, без Deployment?**

<details><summary>Ответ</summary>

Он исчезнет навсегда: контроллера, который его пересоздаст, нет. В опыте после `delete pod hello` вывод `No resources found`.

</details>

**13. Почему рабочие ноды могут быть `NotReady` сразу после `kind create cluster --wait`?**

<details><summary>Ответ</summary>

`--wait` ждёт только готовности control plane. Рабочие ноды присоединяются и инициализируются чуть позже: в опыте они стали `Ready` через ~15 секунд.

</details>

## Шпаргалка

```bash
kind create cluster --name learn            # создать (переключит контекст!)
kind get clusters; kind delete cluster --name learn
kind load docker-image app:1.0 --name learn
kubectl config get-contexts; kubectl config use-context NAME
kubectl config set-context --current --namespace=learn-03
kubectl -n NS apply -f f.yaml; kubectl diff -f f.yaml
kubectl get pods -o wide -l app=x --show-labels
kubectl get pods -o jsonpath='{.items[*].metadata.name}'
kubectl describe pod NAME; kubectl logs NAME; kubectl port-forward pod/NAME 8080:8080
kubectl explain pod.spec --recursive | less
kubectl create deployment d --image=nginx:1.29 --dry-run=client -o yaml
kubectl delete namespace learn-03
```

## Связанные материалы

- Предыдущие уровни: [базовая](basics.ru.md), [продвинутая](advanced.ru.md)
- Предыдущие модули: [01 · Контейнеры и Docker](../01-containers/overview.ru.md), [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [kubectl reference](https://kubernetes.io/docs/reference/kubectl/)
- [kubectl: шпаргалка](https://kubernetes.io/docs/reference/kubectl/quick-reference/)
- [Organizing cluster access using kubeconfig](https://kubernetes.io/docs/concepts/configuration/organize-cluster-access-kubeconfig/)
- [Extend kubectl with plugins](https://kubernetes.io/docs/tasks/extend-kubectl/kubectl-plugins/)
- [Server-side printing (Table)](https://kubernetes.io/docs/reference/using-api/api-concepts/#receiving-resources-as-tables)
- [Version Skew Policy](https://kubernetes.io/releases/version-skew-policy/)
- [kind: Configuration](https://kind.sigs.k8s.io/docs/user/configuration/)
- [kind: Quick Start](https://kind.sigs.k8s.io/docs/user/quick-start/)
