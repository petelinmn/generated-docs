---
title: "Диагностика: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, endpoints, dns, kubectl-debug, ephemeral-containers, finalizer, resourcequota, notready, readiness]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 ноды), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Диагностика: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Опыты — в kind на трёх нодах, namespace `learn-21`.

## Service без endpoints

Если Service не отвечает, сначала смотрят его endpoints: это адреса подов, которые Service реально обслуживает ([модуль 06](../06-services/overview.ru.md)). Три Service на один Deployment `web` (nginx, 2 реплики):

```yaml
apiVersion: v1
kind: Service
metadata: {name: web}                # верный селектор
spec: {selector: {app: web}, ports: [{port: 80}]}
---
metadata: {name: web-typo}           # опечатка в селекторе
spec: {selector: {app: wbe}, ports: [{port: 80}]}
---
metadata: {name: web-port}           # неверный targetPort
spec: {selector: {app: web}, ports: [{port: 80, targetPort: 8080}]}
```

```text
$ kubectl -n learn-21 get endpoints web web-typo web-port
NAME       ENDPOINTS                         AGE
web        10.244.1.6:80,10.244.2.6:80       26s
web-typo   <none>                            26s
web-port   10.244.1.6:8080,10.244.2.6:8080   26s
$ for s in web web-typo web-port; do kubectl -n learn-21 exec client -- wget -q -T 3 -O /dev/null http://$s; done
web: OK
web-typo: wget: can't connect to remote host (10.96.75.197): Connection refused
web-port: wget: can't connect to remote host (10.96.20.92): Connection refused
```

- **Опечатка в селекторе:** `ENDPOINTS <none>`, соединение отклоняется.
- **Неверный `targetPort`:** endpoints есть, но порт `8080`, на котором никто не слушает: тот же `Connection refused`. Сравните `targetPort` с портом контейнера.
- В выводе `kubectl get endpoints` есть предупреждение, что `Endpoints` устарел с 1.33 в пользу `EndpointSlice` (`kubectl get endpointslices`).

### Неготовые поды не попадают в endpoints

Если у подов есть readiness-проба и она не проходит, они `0/1` и в endpoints не входят ([модуль 11](../11-probes-and-resources/basics.ru.md)). Опыт: у Deployment `web` с рабочей пробой `GET /` изменили путь на `/ready` (в nginx такого пути нет):

```text
$ kubectl -n learn-21 get pods -l app=web
web-559df6b76c-hjf7d   1/1   Running   0   39s     ← старые поды
web-559df6b76c-jbcqh   1/1   Running   0   38s
web-58dd899764-fkq7r   0/1   Running   0   20s     ← новый под, проба не проходит
$ kubectl -n learn-21 get deploy web
NAME   READY   UP-TO-DATE   AVAILABLE
web    2/2     1            2
$ kubectl -n learn-21 rollout status deploy/web --timeout=10s
Waiting for deployment "web" rollout to finish: 1 out of 2 new replicas have been updated...
error: timed out waiting for the condition
$ kubectl -n learn-21 get events --field-selector reason=Unhealthy
Readiness probe failed: HTTP probe failed with statuscode: 404
```

Rolling update не продолжается, пока новый под не стал готов, а старые поды продолжают работать и обслуживать трафик (endpoints остались прежними). Причина в событии `Unhealthy`: код `404`. Чтобы раскатка завершилась, нужно вернуть рабочий путь пробы (с рабочей пробой `/` предыдущая раскатка завершилась успешно: `successfully rolled out`).

## DNS

Приложения находят сервисы по именам ([модуль 06](../06-services/overview.ru.md)). Проверка из пода:

```text
$ kubectl -n learn-21 exec client -- nslookup web.learn-21.svc.cluster.local
Server:		10.96.0.10
Address:	10.96.0.10:53

Name:	web.learn-21.svc.cluster.local
Address: 10.96.175.169

$ kubectl -n learn-21 exec client -- nslookup nosuch.learn-21.svc.cluster.local
** server can't find nosuch.learn-21.svc.cluster.local: NXDOMAIN

$ kubectl -n learn-21 exec client -- cat /etc/resolv.conf
search learn-21.svc.cluster.local svc.cluster.local cluster.local
nameserver 10.96.0.10
options ndots:5
```

- `Server: 10.96.0.10` — это Service CoreDNS (`kube-dns`).
- Имя несуществующего Service даёт `NXDOMAIN`.
- `search` и `ndots:5` позволяют писать короткое имя `web` внутри namespace: резолвер подставит суффиксы по очереди. Поэтому запрос к внешнему имени сначала проходит несколько неудачных поисков.

Если DNS не работает совсем: проверьте поды CoreDNS в `kube-system`, `resolv.conf` в поде и [NetworkPolicy](../17-security/advanced.ru.md#egress-и-dns) (egress без DNS).

## `kubectl debug`: заглянуть внутрь

В образе часто нет оболочки (`distroless`), и `exec` не помогает. `kubectl debug` добавляет к работающему поду **временный контейнер** (ephemeral) с нужными инструментами:

```text
$ kubectl -n learn-21 debug pod/web-… --image=busybox:1.37 --target=nginx -c dbg -- sleep 300
$ kubectl -n learn-21 exec web-… -c dbg -- sh -c 'ps | head -5; ls /proc/1/root/etc/nginx | head -3; wget -qO- -T2 localhost | head -2'
PID   USER     TIME  COMMAND
    1 root      0:00 nginx: master process nginx -g daemon off;
   33 101       0:00 nginx: worker process
conf.d  fastcgi_params  mime.types
<!DOCTYPE html>
<html>
$ kubectl -n learn-21 get pod web-… -o jsonpath='{.spec.ephemeralContainers[0].name}'
dbg
```

`--target=nginx` делит пространство процессов с целевым контейнером: видны его процессы (`nginx: master process`), а файловая система доступна через `/proc/1/root`. Сеть у контейнеров пода общая, поэтому `wget localhost` обращается к самому nginx.

Тот же приём работает для ноды: создаётся под, где файловая система ноды смонтирована в `/host`:

```text
$ kubectl -n learn-21 debug node/learn-worker --image=busybox:1.37 -- sh -c 'ls /host/etc/kubernetes; cat /host/etc/hostname'
Creating debugging pod node-debugger-learn-worker-rvvxh with container debugger on node learn-worker.
kubelet.conf
manifests
pki
learn-worker
```

Отладочный под остаётся в namespace после завершения: его нужно удалить (`kubectl delete pod node-debugger-…`). Временный контейнер из пода не убрать, он исчезает вместе с подом.

> ⚠️ Проверить: интерактивная работа (`-it`), `--copy-to`, `--share-processes` и профили (`--profile`) в опытах не запускались.

## Объект завис в Terminating

Если у объекта есть финализатор, удаление лишь ставит `deletionTimestamp`, и объект остаётся, пока финализатор не снят ([модуль 19](../19-crd-and-operators/expert.ru.md#внутреннее-устройство)):

```text
$ kubectl -n learn-21 create cm fin
$ kubectl -n learn-21 patch cm fin -p '{"metadata":{"finalizers":["example.com/block"]}}'
$ kubectl -n learn-21 delete cm fin --wait=false
configmap "fin" deleted from learn-21 namespace
$ kubectl -n learn-21 get cm fin -o jsonpath='{.metadata.deletionTimestamp}{" "}{.metadata.finalizers}'
2026-11-23T13:57:20Z ["example.com/block"]
$ kubectl -n learn-21 patch cm fin --type=merge -p '{"metadata":{"finalizers":null}}'
$ kubectl -n learn-21 get cm fin
Error from server (NotFound): configmaps "fin" not found
```

`kubectl delete` сообщил «deleted», но объект остался с `deletionTimestamp`. Сначала выясните, **какой контроллер** ставит финализатор и почему не снимает; ручное снятие (`finalizers: null`) оправдано только когда контроллера уже нет или очистка не нужна.

## Квота: ReplicaFailure

`ResourceQuota` ограничивает число объектов в namespace. Когда лимит исчерпан, Deployment не может создать поды:

```text
$ kubectl -n learn-21 create deploy qd --image=nginx:1.29 --replicas=5
$ kubectl -n learn-21 get deploy qd
NAME   READY   UP-TO-DATE   AVAILABLE
qd     0/5     0            0
$ kubectl -n learn-21 get events --field-selector reason=FailedCreate
Error creating: pods "qd-5c6ccb5cd9-6stpj" is forbidden: exceeded quota: q, requested: pods=1, used: pods=4, limited: pods=4
$ kubectl -n learn-21 get deploy qd -o jsonpath='{.status.conditions[?(@.type=="ReplicaFailure")]}'
{"message":"pods \"qd-…\" is forbidden: exceeded quota: q, requested: pods=1, used: pods=4, limited: pods=4","reason":"FailedCreate","status":"True","type":"ReplicaFailure"}
```

Подов нет, а в самом Deployment причина видна только в условии `ReplicaFailure` (`reason: FailedCreate`) и в событиях ReplicaSet. То же условие даёт отказ Pod Security Admission ([модуль 17](../17-security/advanced.ru.md#pod-security-admission-режимы-на-практике)). Квоты — в [модуле 10](../10-namespaces-and-rbac/overview.ru.md).

## Нода `NotReady`

Опыт: контейнер ноды `learn-worker2` остановлен (`docker stop`), на ней стояли два пода Deployment.

```text
t=48  learn-worker2=Ready
t=60  learn-worker2=NotReady        ← примерно через 50 секунд после остановки
$ kubectl get node learn-worker2 -o jsonpath='{.spec.taints}'
[{"effect":"NoSchedule","key":"node.kubernetes.io/unreachable","timeAdded":"…"},
 {"effect":"NoExecute","key":"node.kubernetes.io/unreachable","timeAdded":"…"}]
$ kubectl describe node learn-worker2 | grep -A5 ^Conditions
  Ready  Unknown  NodeStatusUnknown  Kubelet stopped posting node status.
  (MemoryPressure, DiskPressure, PIDPressure — тоже Unknown)
$ kubectl -n learn-21 get pods -o wide        # через ~84 с после остановки
spread-…  1/1  Running  learn-worker2
```

Контроллер нод, не получая сообщений kubelet, пометил ноду `NotReady` (условия `Unknown`) и поставил taints `unreachable`. Поды на ноде **ещё не выселены**: у них по умолчанию есть toleration на 300 секунд ([модуль 18](../18-scheduling/advanced.ru.md#noexecute-и-tolerationseconds)), и `kubectl` продолжает показывать их `Running`. После `docker start` нода вернулась в `Ready`, taints исчезли.

> ⚠️ Проверить: само выселение подов с недоступной ноды по истечении 300 секунд в этом опыте не наблюдалось, так как эксперимент остановлен раньше.

## Лучшие практики

- Сначала факты: `get`, `describe`, события, логи — и только потом изменения.
- Проверяйте связанные объекты: Service → endpoints → поды → ноды; под → PVC → StorageClass.
- Временные контейнеры вместо расширения образа инструментами.
- Не снимайте финализаторы вслепую.
- Держите под рукой образ с инструментами (`busybox`, `nicolaka/netshoot`) — не запускалось здесь, но распространено.

## Частые ошибки

- ❌ Опечатка в селекторе Service → ✅ `get endpoints` (пусто).
- ❌ `targetPort`, не совпадающий с портом контейнера → ✅ сверить.
- ❌ Ждать, что rolling update завершится при неработающей readiness-пробе → ✅ события `Unhealthy`.
- ❌ Искать причину в Deployment, когда нет подов → ✅ `ReplicaFailure`, события ReplicaSet.
- ❌ Забыть удалить отладочный под ноды → ✅ `kubectl delete pod node-debugger-…`.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Service и DNS: [06 · Service и сеть](../06-services/overview.ru.md)
- Пробы: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- Taints и ноды: [18 · Планирование подов](../18-scheduling/overview.ru.md)
- Финализаторы: [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md)

## Источники

- [Debug Services](https://kubernetes.io/docs/tasks/debug/debug-application/debug-service/)
- [Debugging DNS Resolution](https://kubernetes.io/docs/tasks/administer-cluster/dns-debugging-resolution/)
- [Debug Running Pods (ephemeral containers)](https://kubernetes.io/docs/tasks/debug/debug-application/debug-running-pod/)
- [Debugging a Kubernetes node with kubectl debug](https://kubernetes.io/docs/tasks/debug/debug-cluster/kubectl-node-debug/)
- [Finalizers](https://kubernetes.io/docs/concepts/overview/working-with-objects/finalizers/)
- [Resource Quotas](https://kubernetes.io/docs/concepts/policy/resource-quotas/)
