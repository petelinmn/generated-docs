---
title: "Service и сеть: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 06-services
part: 1
tags: [service, nodeport, loadbalancer, headless-service, externalname, readiness, endpoints, kube-proxy, iptables]
versions: "Kubernetes 1.37.0 (kind v0.34, CoreDNS, kube-proxy iptables), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Service и сеть: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): манифест ClusterIP Service, endpoints и DNS не повторяются.

## Типы Service

| Тип | Что делает |
|---|---|
| `ClusterIP` | виртуальный адрес внутри кластера (по умолчанию) |
| `NodePort` | ClusterIP плюс порт на каждой ноде (30000–32767) |
| `LoadBalancer` | NodePort плюс внешний балансировщик, который создаёт облако |
| `ExternalName` | DNS-алиас (CNAME) на внешнее имя, без прокси |
| headless (`clusterIP: None`) | без виртуального адреса: DNS возвращает адреса подов |

Остальные четыре варианта (кроме обычного ClusterIP из [базовой теории](basics.ru.md#манифест-service)) создаём одним файлом:

```yaml
apiVersion: v1
kind: Service
metadata: {name: web-np}
spec:
  type: NodePort
  selector: {app: web}
  ports: [{port: 80, targetPort: http, nodePort: 30080}]
---
apiVersion: v1
kind: Service
metadata: {name: web-lb}
spec:
  type: LoadBalancer
  selector: {app: web}
  ports: [{port: 80, targetPort: http}]
---
apiVersion: v1
kind: Service
metadata: {name: web-headless}
spec:
  clusterIP: None
  selector: {app: web}
  ports: [{port: 80, targetPort: http}]
---
apiVersion: v1
kind: Service
metadata: {name: ext}
spec:
  type: ExternalName
  externalName: example.com
```

```text
$ kubectl -n learn-06 get svc
NAME           TYPE           CLUSTER-IP      EXTERNAL-IP   PORT(S)        AGE
ext            ExternalName   <none>          example.com   <none>         3s
web            ClusterIP      10.96.37.242    <none>        80/TCP         16s
web-headless   ClusterIP      None            <none>        80/TCP         4s
web-lb         LoadBalancer   10.96.24.161    <pending>     80:31708/TCP   4s
web-np         NodePort       10.96.162.222   <none>        80:30080/TCP   4s
```

### NodePort

`80:30080/TCP` означает: порт Service 80, порт на нодах 30080. Запрос на IP любой ноды и порт 30080 попадёт на поды. В kind нода — контейнер Docker, поэтому проверяем изнутри неё:

```text
$ docker exec learn-control-plane curl -s -m 3 http://172.18.0.9:30080
hello from web-8678fd4948-5p9dp
```

(`172.18.0.9` — адрес ноды из `kubectl get node -o wide`.) Диапазон NodePort — 30000–32767; другое значение API отклоняет:

```text
The Service "bad-np" is invalid: spec.ports[0].nodePort: Invalid value: 20000: provided port is not in the valid range. The range of valid ports is 30000-32767
```

С вашей машины порт ноды kind по умолчанию не виден: Docker не пробрасывает его на хост. Для доступа к NodePort с хоста в конфигурации kind задают `extraPortMappings`.

> ⚠️ Проверить: проброс NodePort на хост через `extraPortMappings` в kind не запускался в этом материале ([документация kind](https://kind.sigs.k8s.io/docs/user/configuration/#extra-port-mappings)).

### LoadBalancer

Service типа `LoadBalancer` создаёт ещё и NodePort (в выводе `31708`), а внешний адрес выдаёт облачный провайдер. В нашем kind облачного контроллера нет, поэтому `EXTERNAL-IP` остаётся `<pending>` и сервис доступен только как NodePort. Для локальных опытов с LoadBalancer существуют отдельные инструменты (например, MetalLB или cloud-provider-kind).

> ⚠️ Проверить: работа MetalLB и cloud-provider-kind с kind не проверялась в этом материале.

### Headless Service

Без `clusterIP` DNS возвращает адреса подов напрямую, по одной записи на под:

```text
$ kubectl -n learn-06 exec client2 -- nslookup web-headless
Name:	web-headless.learn-06.svc.cluster.local
Address: 10.244.0.33
Name:	web-headless.learn-06.svc.cluster.local
Address: 10.244.0.34
Name:	web-headless.learn-06.svc.cluster.local
Address: 10.244.0.35
```

Это нужно, когда клиент сам выбирает под или должен знать все (базы данных, StatefulSet, модуль 12). Балансировки kube-proxy у headless-сервиса нет.

### ExternalName

`ExternalName` не проксирует, а отдаёт CNAME:

```text
$ kubectl -n learn-06 exec client2 -- nslookup ext
ext.learn-06.svc.cluster.local	canonical name = example.com
Name:	example.com
```

Так внешний сервис получает внутреннее имя; переехав, вы смените только `externalName`.

## Готовность подов и endpoints

В EndpointSlice попадают только готовые поды. Проверим: добавим Deployment readiness probe, которая никогда не проходит (`/ready` отдаёт 404), и посмотрим, что произойдёт при выкатке:

```text
$ kubectl -n learn-06 patch deploy web --type=json -p '[{"op":"add","path":"/spec/template/spec/containers/0/readinessProbe","value":{"httpGet":{"path":"/ready","port":80},"periodSeconds":2}}]'
$ kubectl -n learn-06 get pods -l app=web
web-77db7cb557-gqn2v   0/1   Running
web-8678fd4948-5p9dp   1/1   Running
web-8678fd4948-7xm64   1/1   Running
web-8678fd4948-hb7n7   1/1   Running
$ kubectl -n learn-06 get endpoints web
NAME   ENDPOINTS                                      AGE
web    10.244.0.33:80,10.244.0.34:80,10.244.0.35:80   53s
```

Новый под запущен, но не готов (`0/1`), поэтому его адреса нет среди endpoints: трафик идёт только на три старых. Сама выкатка тоже остановилась на нём, и старые поды остались ([модуль 05](../05-deployments/advanced.ru.md#застрявший-деплой)). Readiness probe защищает пользователей и от обращений к неготовому поду, и от плохого релиза. Откат (`rollout undo`) вернул прежнюю версию.

## Типичные поломки

**Опечатка в selector.** Service с `selector: {app: wbe}` не находит подов:

```text
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-typo -o custom-columns=NAME:.metadata.name,ENDPOINTS:.endpoints
NAME             ENDPOINTS
web-typo-6vqrs   <nil>
$ kubectl -n learn-06 exec client2 -- wget -qO- -T 3 http://web-typo
wget: can't connect to remote host (10.96.75.1): Connection refused
```

**Неверный targetPort.** Service с `targetPort: 8081`, а контейнер слушает 80: endpoints есть, но адрес ведёт на порт 8081:

```text
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-badport -o jsonpath='{.items[0].endpoints[0].addresses} port={.items[0].ports[0].port}'
["10.244.0.33"] port=8081
$ kubectl -n learn-06 exec client2 -- wget -qO- -T 3 http://web-badport
wget: can't connect to remote host (10.96.133.143): Connection refused
```

В обоих случаях ошибка одна, `Connection refused`, а причина разная. Диагностика: `kubectl get endpoints <svc>`, затем сверка `selector` с метками подов (`kubectl get pods --show-labels`) и `targetPort` с портом контейнера.

## Переменные окружения Service

Для каждого Service, который существует на момент создания пода, kubelet кладёт в контейнер переменные:

```text
$ kubectl -n learn-06 exec client2 -- env | grep -E "^WEB_SERVICE|^WEB_PORT="
WEB_SERVICE_HOST=10.96.37.242
WEB_PORT=tcp://10.96.37.242:80
WEB_SERVICE_PORT=80
WEB_SERVICE_PORT_HTTP=80
```

Они появляются только если Service создан раньше пода; DNS надёжнее. Отключить переменные можно полем пода `enableServiceLinks: false`.

## Как kube-proxy балансирует трафик

На каждой ноде kube-proxy (режим `iptables`) пишет правила. Запрос к ClusterIP попадает в цепочку сервиса, которая выбирает один из подов случайно с заданными вероятностями, и затем идёт DNAT на IP пода:

```text
-A KUBE-SERVICES -d 10.96.37.242/32 -p tcp -m comment --comment "learn-06/web:http cluster IP" -m tcp --dport 80 -j KUBE-SVC-LLGEISXCDCZSME5V
-A KUBE-SVC-LLGEISXCDCZSME5V … --comment "learn-06/web:http -> 10.244.0.39:80" -m statistic --mode random --probability 0.33333333349 -j KUBE-SEP-QT7MGDW4DUHBYCOA
-A KUBE-SVC-LLGEISXCDCZSME5V … --comment "learn-06/web:http -> 10.244.0.40:80" -m statistic --mode random --probability 0.50000000000 -j KUBE-SEP-VB3DA5Y2GNNMKSMP
-A KUBE-SVC-LLGEISXCDCZSME5V … --comment "learn-06/web:http -> 10.244.0.41:80" -j KUBE-SEP-JLZCLXTREU45VXC7
-A KUBE-SEP-JLZCLXTREU45VXC7 -p tcp -m comment --comment "learn-06/web:http" -m tcp -j DNAT --to-destination 10.244.0.41:80
```

Вероятности `1/3`, затем `1/2` от оставшегося и последнее правило без условия дают каждому из трёх подов по одной трети. Правила получены командой `iptables-save -t nat` внутри ноды. ClusterIP не принадлежит интерфейсу, поэтому ping на него не отвечает:

```text
$ kubectl -n learn-06 exec client2 -- ping -c 1 -W 2 10.96.37.242
1 packets transmitted, 0 packets received, 100% packet loss
```

## Лучшие практики

- Обращайтесь по имени Service, не по IP; между namespace — `сервис.namespace`.
- Давайте портам имена и ссылайтесь на них в `targetPort`: номер порта можно менять только в одном месте.
- Всегда добавляйте readiness probe: иначе Service направит трафик на не прогревшийся под.
- Для выставления наружу — Ingress или Gateway API (модуль 09), а не `NodePort` и множество `LoadBalancer`.
- Проверяйте `get endpoints`/`endpointslices` при любой сетевой проблеме.

## Частые ошибки

- ❌ Считать, что Service «сломан» при `Connection refused` → ✅ сначала `get endpoints`: пусто или чужой порт.
- ❌ Выставлять базу данных через `LoadBalancer` → ✅ ClusterIP и сетевые политики (модуль 17).
- ❌ Полагаться на переменные окружения Service → ✅ использовать DNS.
- ❌ Ждать равномерного round-robin на малом числе запросов → ✅ выбор случайный.

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): ClusterIP — это правила iptables и DNAT; сам адрес не назначен ни одному интерфейсу.
- [C#](../../../csharp/recipes/http-client-resilience/overview.ru.md): клиент должен ретраить при кратковременных ошибках соединения во время выкатки и использовать имя Service.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Предыдущий модуль: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- Остановка подов и endpoints: [04 · Pod](../04-pods/advanced.ru.md#остановка-пода)

## Источники

- [Service: types](https://kubernetes.io/docs/concepts/services-networking/service/#publishing-services-service-types)
- [Headless Services](https://kubernetes.io/docs/concepts/services-networking/service/#headless-services)
- [ExternalName](https://kubernetes.io/docs/concepts/services-networking/service/#externalname)
- [Virtual IPs and Service Proxies](https://kubernetes.io/docs/reference/networking/virtual-ips/)
- [kind: Extra Port Mappings](https://kind.sigs.k8s.io/docs/user/configuration/#extra-port-mappings)
- [Debug Services](https://kubernetes.io/docs/tasks/debug/debug-application/debug-service/)
