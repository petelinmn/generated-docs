---
title: "Service и сеть: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 06-services
part: 1
tags: [service, clusterip, dns, endpointslice, load-balancing, port-forward]
versions: "Kubernetes 1.37.0 (kind v0.34, CoreDNS, kube-proxy iptables), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Service и сеть: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-06`: `kubectl create namespace learn-06`.

## Зачем это нужно

Под получает IP при создании и теряет его при удалении. Если приложение A обращается к приложению B по IP пода, всё сломается при первой же выкатке. Service решает это: у него постоянный адрес и имя, а за ним скрываются актуальные поды.

## Приложение для опытов

Deployment на 3 реплики. Каждый под отвечает своим именем хоста, поэтому видно, какой экземпляр ответил:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata: {name: web}
spec:
  replicas: 3
  selector: {matchLabels: {app: web}}
  template:
    metadata: {labels: {app: web}}
    spec:
      containers:
        - name: nginx
          image: nginx:1.29
          ports:
            - {name: http, containerPort: 80}
          command: ["sh", "-c", "echo \"hello from $(hostname)\" > /usr/share/nginx/html/index.html; exec nginx -g 'daemon off;'"]
---
apiVersion: v1
kind: Pod
metadata: {name: client}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sleep", "3600"]}
```

Здесь порт контейнера назван `http`: на имя можно ссылаться из Service. Под `client` нужен, чтобы обращаться к Service изнутри кластера.

## Манифест Service

```yaml
apiVersion: v1
kind: Service
metadata:
  name: web
spec:
  selector:
    app: web
  ports:
    - name: http
      port: 80
      targetPort: http
```

- `selector` — метки подов, на которые идёт трафик (должны совпадать с метками шаблона пода).
- `port` — порт самого Service.
- `targetPort` — порт контейнера (число или имя порта).
- Тип по умолчанию — `ClusterIP`.

```text
$ kubectl -n learn-06 get svc web -o wide
NAME   TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)   AGE   SELECTOR
web    ClusterIP   10.96.37.242   <none>        80/TCP    1s    app=web
```

ClusterIP `10.96.37.242` взят из диапазона `10.96.0.0/16` (`--service-cluster-ip-range` у API server в kind, [модуль 02](../02-architecture/expert.ru.md#конвейер-запроса)).

## Endpoints: какие поды за Service

Kubernetes ведёт для Service список адресов готовых подов:

```text
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web
NAME        ADDRESSTYPE   PORTS   ENDPOINTS                             AGE
web-574j6   IPv4          80      10.244.0.34,10.244.0.35,10.244.0.33   1s
$ kubectl -n learn-06 get endpoints web
Warning: v1 Endpoints is deprecated in v1.33+; use discovery.k8s.io/v1 EndpointSlice
NAME   ENDPOINTS                                      AGE
web    10.244.0.33:80,10.244.0.34:80,10.244.0.35:80   1s
```

Это IP трёх подов. Объект `Endpoints` устарел (предупреждение в выводе), актуальный объект — `EndpointSlice`. Если endpoints пусты, у Service нет подходящих готовых подов.

## Балансировка

Десять запросов из пода `client` по имени Service:

```text
$ for i in $(seq 1 10); do kubectl -n learn-06 exec client -- wget -qO- -T 3 http://web; done | sort | uniq -c
      3 hello from web-8678fd4948-5p9dp
      2 hello from web-8678fd4948-7xm64
      5 hello from web-8678fd4948-hb7n7
```

Запросы разошлись по трём подам неравномерно (3, 2 и 5): выбор пода случайный, а не по кругу. При большем числе запросов распределение выравнивается.

## DNS: как найти Service по имени

CoreDNS даёт каждому Service имя `<сервис>.<namespace>.svc.cluster.local`. Все четыре формы работают из пода в том же namespace:

```text
web -> hello from web-8678fd4948-7xm64
web.learn-06 -> hello from web-8678fd4948-7xm64
web.learn-06.svc -> hello from web-8678fd4948-hb7n7
web.learn-06.svc.cluster.local -> hello from web-8678fd4948-hb7n7
```

Короткое имя `web` работает благодаря настройкам DNS пода:

```text
$ kubectl -n learn-06 exec client -- cat /etc/resolv.conf
search learn-06.svc.cluster.local svc.cluster.local cluster.local
nameserver 10.96.0.10
options ndots:5
```

`nameserver 10.96.0.10` — это ClusterIP самого CoreDNS, `search` — список суффиксов, которые подставляются к короткому имени. Из **другого** namespace короткое имя не сработает: нужен `web.learn-06` (подробности — в [экспертной теории](expert.ru.md#подвох-короткое-имя-работает-только-в-своём-namespace)).

```text
$ kubectl -n learn-06 exec client -- nslookup web.learn-06.svc.cluster.local
Name:	web.learn-06.svc.cluster.local
Address: 10.96.37.242
```

## Стабильный адрес

Удалим все поды Deployment: их IP сменятся, ClusterIP — нет.

```text
$ kubectl -n learn-06 get svc web -o jsonpath='{.spec.clusterIP}{"\n"}'
10.96.37.242
$ kubectl -n learn-06 delete pod -l app=web --now
$ kubectl -n learn-06 get svc web -o jsonpath='{.spec.clusterIP}{"\n"}'
10.96.37.242
$ kubectl -n learn-06 get endpoints web
NAME   ENDPOINTS                                      AGE
web    10.244.0.39:80,10.244.0.40:80,10.244.0.41:80   63s
```

Адрес Service остался прежним, а endpoints обновились (были `.33`, `.34`, `.35`, стали `.39`, `.40`, `.41`).

## Доступ с вашей машины: port-forward

ClusterIP виден только внутри кластера. Для опытов используйте проброс порта к Service:

```text
$ kubectl -n learn-06 port-forward svc/web 18080:80 &
$ curl -s localhost:18080
hello from web-8678fd4948-5tgw5
```

`port-forward` на Service выбирает один из подов и соединяет вас с ним; это отладочный инструмент, не способ публикации приложения.

## Ошибки новичка

- ❌ Обращаться к подам по IP → ✅ использовать имя Service: IP пода меняется.
- ❌ Пинговать ClusterIP → ✅ это виртуальный адрес: `ping` теряет 100% пакетов (проверяйте порт, например `wget` или `nc`).
- ❌ Опечатка в `selector` или неверный `targetPort` → ✅ соединение отклоняется (`Connection refused`); проверьте `get endpoints`.
- ❌ Ждать, что короткое имя работает из другого namespace → ✅ использовать `сервис.namespace`.
- ❌ Выставлять приложение наружу через ClusterIP → ✅ нужен NodePort, LoadBalancer или Ingress.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — NodePort, LoadBalancer, headless, поломки
- Предыдущий модуль: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- Компоненты сети: [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [Service](https://kubernetes.io/docs/concepts/services-networking/service/)
- [EndpointSlices](https://kubernetes.io/docs/concepts/services-networking/endpoint-slices/)
- [DNS for Services and Pods](https://kubernetes.io/docs/concepts/services-networking/dns-pod-service/)
- [Connecting Applications with Services](https://kubernetes.io/docs/tutorials/services/connect-applications-service/)
- [Use Port Forwarding to Access Applications](https://kubernetes.io/docs/tasks/access-application-cluster/port-forward-access-application-cluster/)
