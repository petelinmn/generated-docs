---
title: "Ingress и Gateway API: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 09-ingress-and-gateway-api
part: 2
tags: [ingress, ingressclass, ingress-controller, gateway-api, gateway, httproute, tls, ingress-nginx, envoy-gateway]
versions: "Kubernetes 1.37.0 (kind v0.34), Gateway API CRDs v1.4.1, Envoy Gateway v1.7.4, ingress-nginx 1.15.1 (проект выведен из поддержки)"
status: draft
updated: 2026-11-22
---

# Ingress и Gateway API: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [06 · Service и сеть](../06-services/overview.ru.md) · **Время:** ~3 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-09`. Контроллеры ставятся в одноразовый кластер; их установка описана в разделах ниже.

## Зачем это нужно

Чтобы выставить приложение наружу через [Service](../06-services/advanced.ru.md#типы-service), нужен `NodePort` или `LoadBalancer`. Для десятка приложений это десять балансировщиков и десять адресов, а маршрутизировать по домену и пути Service не умеет. Вход (ingress) решает это: один адрес и одна точка терминации TLS, а правила описывают, какой запрос в какой Service идёт.

## Контроллер: без него ничего не работает

Объекты Ingress и Gateway — только описание. Исполняет его **контроллер**: компонент, который следит за объектами и настраивает прокси (nginx, Envoy и др.), через который идёт трафик. В кластере может быть несколько контроллеров; нужный выбирает поле класса (`ingressClassName`, `gatewayClassName`). В kind контроллера по умолчанию нет.

> ⚠️ **Статус ingress-nginx.** Проект ingress-nginx сообщества Kubernetes объявлен выведенным из поддержки: после марта 2026 года нет выпусков, исправлений и обновлений безопасности ([объявление](https://kubernetes.io/blog/2025/11/11/ingress-nginx-retirement/)). Уже работающие развёртывания продолжают работать, но патчей не получают. Для новых проектов выбирайте Gateway API или другой поддерживаемый контроллер. Здесь ingress-nginx используется только как наглядный пример объекта Ingress в одноразовом кластере.

## Ingress

### Установка контроллера (для опытов)

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml
kubectl -n ingress-nginx rollout status deploy/ingress-nginx-controller
```

```text
$ kubectl get ingressclass
NAME    CONTROLLER             PARAMETERS   AGE
nginx   k8s.io/ingress-nginx   <none>       49s
```

`IngressClass` связывает класс с контроллером. В манифесте использована версия контроллера 1.15.1.

### Приложение и правило

Две версии простого веб-сервера из [модуля 06](../06-services/basics.ru.md#приложение-для-опытов) (`web-v1` и `web-v2`, у каждой свой Service) и такой Ingress:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata: {name: web-ing}
spec:
  ingressClassName: nginx
  rules:
    - host: ing.example.test
      http:
        paths:
          - {path: /api, pathType: Prefix, backend: {service: {name: web-v1, port: {number: 80}}}}
          - {path: /, pathType: Prefix, backend: {service: {name: web-v2, port: {number: 80}}}}
```

- `ingressClassName` — какой контроллер обслуживает объект.
- `rules[].host` — домен (поле `Host` запроса).
- `paths[]` — путь, тип совпадения (`pathType`) и Service с портом.

```text
$ kubectl -n learn-09 get ingress web-ing
NAME      CLASS   HOSTS              ADDRESS   PORTS   AGE
web-ing   nginx   ing.example.test             80      15s
$ kubectl -n learn-09 describe ingress web-ing
Rules:
  Host              Path  Backends
  ----              ----  --------
  ing.example.test
                    /api   web-v1:80 (10.244.0.27:80,10.244.0.28:80)
                    /      web-v2:80 (10.244.0.29:80,10.244.0.30:80)
```

В выводе `describe` видны адреса подов за каждым Service (endpoints, [модуль 06](../06-services/basics.ru.md#endpoints-какие-поды-за-service)).

### Запрос

В одноразовом kind внешнего IP нет, поэтому обращаемся к Service контроллера изнутри кластера и вручную задаём заголовок `Host`:

```text
$ kubectl -n learn-09 exec curl -- sh -c "curl -s -H 'Host: ing.example.test' http://10.96.78.112/; curl -s -H 'Host: ing.example.test' http://10.96.78.112/api/; curl -s -o /dev/null -w 'wronghost=%{http_code}\n' -H 'Host: x.test' http://10.96.78.112/"
v2 from web-v2-577d96dff8-mk2zd
v1 api from web-v1-584794c796-qxvt5
wronghost=404
```

Путь `/` попал в `web-v2`, `/api/` — в `web-v1`, запрос с чужим `Host` получил `404`: правила нет. Домен определяет `Host`, поэтому DNS-имя должно указывать на адрес входа (при опытах его подставляют заголовком).

### pathType обязателен

```text
$ kubectl -n learn-09 apply -f nopt.yaml
The Ingress "nopt" is invalid: spec.rules[0].http.paths[0].pathType: Required value: pathType must be specified
```

Типы: `Exact` (точное совпадение), `Prefix` (по префиксу), `ImplementationSpecific` (на усмотрение контроллера).

### TLS

Сертификат хранится в Secret типа `kubernetes.io/tls` ([модуль 07](../07-config-and-secrets/advanced.ru.md#типы-secret)):

```yaml
spec:
  ingressClassName: nginx
  tls: [{hosts: [app.example.test], secretName: app-tls}]
  rules: [{host: app.example.test, http: {paths: [{path: /, pathType: Prefix, backend: {service: {name: web-v2, port: {number: 80}}}}]}}]
```

```text
$ kubectl -n learn-09 exec curl -- curl -sk --resolve app.example.test:443:10.96.78.112 https://app.example.test/
v2 from web-v2-577d96dff8-kfzxp
$ kubectl -n learn-09 exec curl -- curl -sk -o /dev/null -w "http->%{http_code} redirect=%{redirect_url}\n" -H 'Host: app.example.test' http://10.96.78.112/
http->308 redirect=https://app.example.test/
```

HTTPS работает (`-k` — потому что сертификат самоподписанный), а HTTP-запрос контроллер ingress-nginx по умолчанию перенаправляет на HTTPS кодом `308`. Это поведение конкретного контроллера.

## Gateway API

Gateway API — следующее поколение API входа. Ключевые объекты:

| Объект | Кто создаёт | Что описывает |
|---|---|---|
| `GatewayClass` | поставщик контроллера | какой контроллер реализует Gateway |
| `Gateway` | администратор кластера | точка входа: порты, протоколы, TLS, кому разрешено привязываться |
| `HTTPRoute` | разработчик приложения | правила маршрутизации HTTP к Service |

### Установка контроллера (Envoy Gateway)

```bash
kubectl apply --server-side -f https://github.com/envoyproxy/gateway/releases/download/v1.7.4/install.yaml
kubectl -n envoy-gateway-system rollout status deploy/envoy-gateway
```

Манифест устанавливает и CRD Gateway API (в нём версия набора `v1.4.1`), и контроллер:

```text
$ kubectl get crd | grep gateway.networking.k8s.io
backendtlspolicies, gatewayclasses, gateways, grpcroutes, httproutes, referencegrants, tcproutes, tlsroutes, udproutes
```

### GatewayClass и Gateway

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: GatewayClass
metadata: {name: eg}
spec:
  controllerName: gateway.envoyproxy.io/gatewayclass-controller
---
apiVersion: gateway.networking.k8s.io/v1
kind: Gateway
metadata: {name: main-gw}
spec:
  gatewayClassName: eg
  listeners:
    - name: http
      protocol: HTTP
      port: 80
      allowedRoutes: {namespaces: {from: Same}}
```

```text
$ kubectl get gatewayclass
NAME   CONTROLLER                                      ACCEPTED   AGE
eg     gateway.envoyproxy.io/gatewayclass-controller   True       20s
$ kubectl -n learn-09 get gateway
NAME      CLASS   ADDRESS   PROGRAMMED   AGE
main-gw   eg                False        47s
$ kubectl -n learn-09 get gateway main-gw -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=True (Accepted)
Programmed=False (AddressNotAssigned)
```

`Gateway` принят (`Accepted`), но `Programmed=False` с причиной `AddressNotAssigned`: контроллер создал для него Service типа `LoadBalancer`, а в kind нет облака, которое выдаст адрес. Сам прокси при этом работает, и до него можно достучаться по ClusterIP этого Service ([модуль 06](../06-services/advanced.ru.md#loadbalancer)). Слушатель `listeners[]` — порт и протокол; `allowedRoutes` — из каких namespace допустимы маршруты (`Same` — из того же).

### HTTPRoute

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: HTTPRoute
metadata: {name: web-route}
spec:
  parentRefs: [{name: main-gw}]
  hostnames: ["app.example.test"]
  rules:
    - matches: [{path: {type: PathPrefix, value: /api}}]
      backendRefs: [{name: web-v1, port: 80}]
    - matches: [{path: {type: PathPrefix, value: /}}]
      backendRefs: [{name: web-v2, port: 80}]
```

- `parentRefs` — к какому `Gateway` привязан маршрут.
- `hostnames` — домены (поле `Host`).
- `rules[].matches` — условия (путь, заголовки), `backendRefs` — куда направить.

```text
$ kubectl -n learn-09 get httproute web-route -o jsonpath='{range .status.parents[0].conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=True (Accepted)
ResolvedRefs=True (ResolvedRefs)
```

Контроллер для каждого `Gateway` создаёт собственный Deployment и Service с прокси: `envoy-learn-09-main-gw-2dc8f82a` (Service типа `LoadBalancer`, `EXTERNAL-IP <pending>`). Запросы к его ClusterIP:

```text
$ kubectl -n learn-09 exec client -- wget -qO- --header "Host: app.example.test" http://10.96.33.43/
v2 from web-v2-577d96dff8-mk2zd
$ kubectl -n learn-09 exec client -- wget -qO- --header "Host: app.example.test" http://10.96.33.43/api/
v1 api from web-v1-584794c796-qxvt5
$ kubectl -n learn-09 exec client -- wget -S -qO- --header "Host: other.test" http://10.96.33.43/
  HTTP/1.1 404 Not Found
```

`/` → `web-v2`, `/api/` → `web-v1`, чужой `Host` → `404`.

## Ошибки новичка

- ❌ Создать Ingress или Gateway и ждать трафика без контроллера → ✅ установите контроллер и проверьте `IngressClass`/`GatewayClass`.
- ❌ Забыть `pathType` в Ingress → ✅ он обязателен (`Required value: pathType must be specified`).
- ❌ Проверять вход без заголовка `Host` → ✅ правила привязаны к домену: `curl -H 'Host: …'` или `--resolve`.
- ❌ Считать `Programmed=False` у Gateway в kind поломкой → ✅ нет облачного балансировщика; обращайтесь к ClusterIP или пробросьте порт.
- ❌ Строить новую систему на ingress-nginx → ✅ проект не поддерживается; выбирайте Gateway API.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — веса, редиректы, перезапись, TLS-слушатель, статусы
- Предыдущие модули: [06 · Service и сеть](../06-services/overview.ru.md), [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [Ingress](https://kubernetes.io/docs/concepts/services-networking/ingress/)
- [Ingress Controllers](https://kubernetes.io/docs/concepts/services-networking/ingress-controllers/)
- [Gateway API](https://kubernetes.io/docs/concepts/services-networking/gateway/)
- [Gateway API: документация проекта](https://gateway-api.sigs.k8s.io/)
- [Ingress NGINX Retirement: What You Need to Know](https://kubernetes.io/blog/2025/11/11/ingress-nginx-retirement/)
- [Envoy Gateway: Quickstart](https://gateway.envoyproxy.io/docs/tasks/quickstart/)
