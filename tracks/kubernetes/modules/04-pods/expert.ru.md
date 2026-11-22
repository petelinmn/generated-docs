---
title: "Pod: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 04-pods
part: 1
tags: [pod, pause-container, sandbox, namespaces, qos, oomkilled, exit-code, termination-message, activedeadlineseconds]
versions: "Kubernetes 1.37.0 (kind v0.34, containerd 2.3.4), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Pod: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Sandbox и pause-контейнер

Под — не сущность Linux, а договорённость рантайма. Для каждого пода рантайм (containerd) создаёт **sandbox** — набор общих namespaces — и держит его живым служебным контейнером `pause`. Контейнеры пода присоединяются к namespaces этого sandbox. На ноде kind (команда `crictl` внутри ноды):

```text
$ docker exec learn-control-plane crictl pods --name multi
POD ID          CREATED                  STATE   NAME    NAMESPACE   ATTEMPT   RUNTIME
d156afd06f10c   Less than a second ago   Ready   multi   learn-04    0         (default)
$ docker exec learn-control-plane sh -c 'ps -eo pid,comm,args | grep pause | head -3'
  363 pause   /pause
  422 pause   /pause
  436 pause   /pause
```

Процесс `/pause` — по одному на под. Он ничего не делает, только владеет сетевым пространством и IP-адресом. Поэтому:

- перезапуск контейнера не меняет IP пода: namespace принадлежит sandbox, а не контейнеру;
- у всех контейнеров один IP и общий `localhost` (проверено: `hostname -i` в двух контейнерах пода `multi` дал `10.244.0.43`);
- если включить `shareProcessNamespace: true`, `pause` становится PID 1 общего пространства процессов (`PID 1 /pause`), и он же подбирает «зомби».

Файловые системы у контейнеров раздельны (из образа каждого), общие данные идут через тома.

### Состояния контейнера

У каждого контейнера один из трёх `state`:

| Состояние | Поля |
|---|---|
| `waiting` | `reason` (`ContainerCreating`, `ImagePullBackOff`, `CrashLoopBackOff`, …) |
| `running` | `startedAt` |
| `terminated` | `exitCode`, `reason` (`Completed`, `Error`, `OOMKilled`), `message`, `startedAt`, `finishedAt` |

```text
$ kubectl -n learn-04 get pod web -o jsonpath='{.status.containerStatuses[0].state}'
{"running":{"startedAt":"2026-11-22T16:24:47Z"}}
```

`lastState` хранит предыдущее завершение, оттуда берут код выхода падающего контейнера.

### Класс QoS

От requests и limits зависит `status.qosClass` (используется при вытеснении подов при нехватке ресурсов):

| Класс | Условие | Наблюдение |
|---|---|---|
| `Guaranteed` | у всех контейнеров requests = limits для CPU и памяти | — |
| `Burstable` | что-то задано, но не как у `Guaranteed` | под с одним `limits.memory: 20Mi` получил `qos=Burstable` |
| `BestEffort` | ничего не задано | под `web` без ресурсов — `qos=BestEffort` |

Подробнее — в модуле 11 (Probes и ресурсы).

## Справочник

### Поля `spec` пода

Список получен из `kubectl explain pod.spec` (kubectl 1.36.1, кластер 1.37.0). Поля, на которые стоит обратить внимание:

| Поле | Смысл |
|---|---|
| `containers`, `initContainers`, `ephemeralContainers` | основные, init- и отладочные контейнеры |
| `restartPolicy` | `Always` (по умолчанию), `OnFailure`, `Never` |
| `terminationGracePeriodSeconds` | время на остановку (30 с по умолчанию) |
| `activeDeadlineSeconds` | максимальное время активности пода; после него `Failed` с причиной `DeadlineExceeded` |
| `volumes` | тома пода |
| `nodeName`, `nodeSelector`, `affinity`, `tolerations`, `topologySpreadConstraints`, `schedulerName`, `priorityClassName`, `preemptionPolicy` | планирование (модуль 18) |
| `serviceAccountName`, `automountServiceAccountToken` | учётная запись пода |
| `securityContext`, `hostUsers` | безопасность на уровне пода |
| `hostNetwork`, `hostPID`, `hostIPC`, `shareProcessNamespace` | совместное использование namespaces хоста или контейнеров |
| `dnsPolicy`, `dnsConfig`, `hostAliases`, `hostname`, `subdomain`, `setHostnameAsFQDN`, `enableServiceLinks` | DNS и имена |
| `imagePullSecrets` | секреты для приватных реестров |
| `readinessGates` | дополнительные условия готовности |
| `runtimeClassName`, `overhead`, `resources` | рантайм и ресурсы пода |
| `os` | операционная система пода |

### Поля контейнера

`name`, `image`, `imagePullPolicy`, `command` (заменяет `ENTRYPOINT`), `args` (заменяет `CMD`), `workingDir`, `env`, `envFrom`, `ports`, `resources`, `volumeMounts`, `volumeDevices`, `livenessProbe`, `readinessProbe`, `startupProbe`, `lifecycle` (`postStart`, `preStop`), `securityContext`, `stdin`, `stdinOnce`, `tty`, `terminationMessagePath`, `terminationMessagePolicy`, `restartPolicy` (политика перезапуска отдельного контейнера; у init-контейнера значение `Always` делает его нативным sidecar), `resizePolicy`, `restartPolicyRules`.

Список полей тоже получен из `kubectl explain pod.spec.containers`.

> ⚠️ Проверить: назначение полей `restartPolicyRules` и `resizePolicy`, а также допустимость `restartPolicy` у обычных (не init) контейнеров в версии 1.37 не проверялись запуском; сверяйте с `kubectl explain pod.spec.containers.restartPolicyRules`.

### Коды выхода

Если контейнер убит сигналом, код выхода равен 128 + номер сигнала. В опыте контейнер, превысивший лимит памяти (`limits.memory: 20Mi`, команда `tail /dev/zero`), завершился так:

```text
$ kubectl -n learn-04 get pod oom -o jsonpath='phase={.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode}'
phase=Failed reason=OOMKilled exit=137
```

137 = 128 + 9 (`SIGKILL`); причина `OOMKilled`. Код 143 = 128 + 15 соответствует `SIGTERM`.

### Сообщение о завершении

Контейнер может записать причину в файл `/dev/termination-log` (путь задаёт `terminationMessagePath`), и она появится в статусе:

```text
$ kubectl -n learn-04 get pod msg -o jsonpath='{.status.containerStatuses[0].state.terminated.message}'
reason-for-exit
```

## Подвохи

### Подвох: Running не значит «работает»

`Running` — фаза пода: контейнер запущен. Приложение может ещё не принимать запросы или быть сломанным; готовность показывают условие `Ready` и readiness probe. Под `crash` из [базовой теории](basics.ru.md#crashloopbackoff) тоже в фазе `Running` между перезапусками.

### Подвох: Completed и Error — не фазы

```text
$ kubectl -n learn-04 get pod failnever -o jsonpath='{.status.phase}'
Failed
```

В столбце `STATUS` стоит `Error`, а фаза — `Failed`; у `once` `STATUS=Completed`, фаза `Succeeded`. Для скриптов используйте `status.phase`.

### Подвох: activeDeadlineSeconds убивает под с зелёным статусом

```text
$ kubectl -n learn-04 get pod deadline -o jsonpath='phase={.status.phase} reason={.status.reason} msg={.status.message}'
phase=Failed reason=DeadlineExceeded msg=Pod was active on the node longer than the specified deadline
```

Под с `activeDeadlineSeconds: 8` и `sleep 3600` через 8 секунд перешёл в `Failed`: нормальный `sleep` не завершился. Через 26 секунд после создания он ещё показывался `Running`: kubelet применяет дедлайн не мгновенно (проверено при повторном запросе, под уже был `Failed`). Для задач с ограничением времени лучше Job.

### Подвох: логи предыдущего контейнера могут исчезнуть

`kubectl logs crash --previous` в нашем опыте вернул `unable to retrieve container logs for containerd://…`: рантайм уже удалил предыдущий контейнер. Для диагностики сбоев настраивайте централизованный сбор логов (модуль 16).

### Подвох: нет `sh` — нет `exec`

Образ без оболочки (`scratch`) не даёт выполнить `kubectl exec … sh` ([модуль 03](../03-local-cluster-and-kubectl/basics.ru.md#базовые-команды)). Отладка таких подов — через эфемерные контейнеры (модуль 21).

### Подвох: удаление пода занимает 30 секунд

Если приложение не обрабатывает `SIGTERM`, каждое удаление и каждая выкатка ждут `terminationGracePeriodSeconds`: в опыте `term-ignore deleted in 32s` против `1s` у обработчика сигнала ([продвинутая теория](advanced.ru.md#остановка-пода)).

## Компромиссы и альтернативы

| Что нужно | Выбор |
|---|---|
| Долгоживущий сервис с репликами | Deployment ([модуль 05](../05-deployments/overview.ru.md)) |
| Разовая или периодическая задача | Job и CronJob (модуль 12) |
| Приложение с состоянием и стабильными именами | StatefulSet (модуль 12) |
| Агент на каждой ноде | DaemonSet (модуль 12) |
| Два тесно связанных процесса | один под, два контейнера или нативный sidecar |
| Независимое масштабирование | отдельные поды |

## Вопросы с собеседований

**1. Что такое Pod и почему не просто контейнер?**

<details><summary>Ответ</summary>

Pod — единица планирования и сети: один или несколько контейнеров с общим IP, `localhost` и томами, которые всегда живут на одной ноде. Он позволяет описывать тесно связанные процессы (приложение и sidecar) как единое целое.

</details>

**2. Что общего у контейнеров одного пода и что раздельно?**

<details><summary>Ответ</summary>

Общее: сетевое пространство (один IP, `localhost`), имя хоста, тома, которые смонтированы в оба контейнера, и (по флагу) пространство процессов. Раздельное: файловая система из образа, процессы (без `shareProcessNamespace`), ресурсы и `securityContext` контейнеров.

</details>

**3. Для чего нужен контейнер `pause`?**

<details><summary>Ответ</summary>

Он держит sandbox пода: сетевое пространство и IP. Благодаря ему IP не меняется при перезапуске обычных контейнеров. При `shareProcessNamespace: true` он становится PID 1 и подбирает завершившиеся процессы.

</details>

**4. (С подвохом) Чем отличается фаза `Failed` от статуса `CrashLoopBackOff`?**

<details><summary>Ответ</summary>

`Failed` — фаза пода: все контейнеры завершены, один с ошибкой, и перезапусков не будет (например, `restartPolicy: Never`). `CrashLoopBackOff` — состояние ожидания контейнера при политике, которая перезапускает: фаза пода при этом `Running`.

</details>

**5. Какие значения у `restartPolicy` и каково умолчание?**

<details><summary>Ответ</summary>

`Always` (по умолчанию), `OnFailure`, `Never`. При `Always` контейнер перезапускается даже после успешного завершения; в опыте под с `exit 0` и `Never` остался `Completed`, а под с `Always` и `exit 1` перезапускался.

</details>

**6. Как работают init-контейнеры?**

<details><summary>Ответ</summary>

Выполняются по очереди до основных контейнеров, каждый должен завершиться успешно. Пока они работают, статус `Init:N/M`; при падении — `Init:Error` с перезапусками по политике. Нужны для подготовки: ожидание зависимости, миграции, копирование файлов в общий том.

</details>

**7. Как устроен нативный sidecar?**

<details><summary>Ответ</summary>

Это init-контейнер с `restartPolicy: Always`: он стартует до основных контейнеров, не должен завершаться и работает весь срок жизни пода, а останавливается после основных. Под становится `Running`, и `READY` включает sidecar (`2/2`).

</details>

**8. (С подвохом) Что происходит при `kubectl delete pod`, если приложение игнорирует `SIGTERM`?**

<details><summary>Ответ</summary>

Выполняется `preStop`, затем `SIGTERM`, ожидание до `terminationGracePeriodSeconds` (30 с) и `SIGKILL`. В опыте `sleep` в роли PID 1 привёл к удалению за 32 с, а обработчик `trap` — за 1 с.

</details>

**9. Когда выполняется `preStop` и входит ли его время в grace period?**

<details><summary>Ответ</summary>

До отправки `SIGTERM`; его время входит в общий `terminationGracePeriodSeconds`. В опыте хук на 3 секунды плюс обработка сигнала дали удаление за 4 секунды.

</details>

**10. Что значит код выхода 137?**

<details><summary>Ответ</summary>

128 + 9: процесс убит `SIGKILL`. Типичные причины: `OOMKilled` (превышен лимит памяти; в опыте `reason=OOMKilled exit=137`) или истёк `terminationGracePeriodSeconds`.

</details>

**11. Чем `Succeeded` отличается от `Completed`?**

<details><summary>Ответ</summary>

`Succeeded` — фаза пода (`status.phase`); `Completed` — причина завершения контейнера в `STATUS` у `kubectl get`. Аналогично `Failed` — фаза, `Error` — причина.

</details>

**12. (С подвохом) Под с `activeDeadlineSeconds: 8` запускает `sleep 3600`. Что будет?**

<details><summary>Ответ</summary>

Через ~8 секунд под переходит в `Failed` с причиной `DeadlineExceeded` (`Pod was active on the node longer than the specified deadline`), хотя процесс не падал. Применение дедлайна не мгновенно.

</details>

**13. Как под узнаёт своё имя и IP?**

<details><summary>Ответ</summary>

Через Downward API: переменные окружения с `valueFrom.fieldRef` (`metadata.name`, `status.podIP`) или тома. В опыте `POD_NAME=multi`, `POD_IP=10.244.0.11`.

</details>

## Шпаргалка

```bash
kubectl run web --image=nginx:1.29 --restart=Never          # быстрый под
kubectl get pod web -o jsonpath='{.status.phase}'
kubectl get pod web -o jsonpath='{.status.containerStatuses[0].state}'
kubectl describe pod web                                    # события
kubectl logs web [-c имя] [-f] [--previous] [--all-containers]
kubectl exec -it web -c имя -- sh
kubectl delete pod web --now                                # без ожидания grace period
kubectl get pod web -o jsonpath='{.status.qosClass}'
```

## Связанные материалы

- Предыдущие уровни: [базовая](basics.ru.md), [продвинутая](advanced.ru.md)
- Следующий модуль: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- Предыдущие модули: [03 · kubectl](../03-local-cluster-and-kubectl/overview.ru.md), [02 · Архитектура](../02-architecture/overview.ru.md), [01 · Контейнеры](../01-containers/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — namespaces и cgroups

## Источники

- [Pods](https://kubernetes.io/docs/concepts/workloads/pods/)
- [Pod Lifecycle](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/)
- [Pod Quality of Service Classes](https://kubernetes.io/docs/concepts/workloads/pods/pod-qos/)
- [Pod API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/pod-v1/)
- [Determine the Reason for Pod Failure (termination message)](https://kubernetes.io/docs/tasks/debug/debug-application/determine-reason-pod-failure/)
- [Share Process Namespace between Containers in a Pod](https://kubernetes.io/docs/tasks/configure-pod-container/share-process-namespace/)
- [Container Runtime Interface (CRI)](https://kubernetes.io/docs/concepts/architecture/cri/)
