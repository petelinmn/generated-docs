---
title: "Упражнения: Ingress и Gateway API"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 09-ingress-and-gateway-api
part: 2
tags: [ingress, gateway-api, httproute, tls, traffic-splitting, ingress-controller]
status: draft
updated: 2026-11-22
---

# Упражнения: Ingress и Gateway API

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен одноразовый кластер kind и namespace `learn-09`. Приложение (`web-v1`, `web-v2`, их Service и поды `client`, `curl`) — из [базовой теории](basics.ru.md#ingress). Установка контроллеров описана там же. Убрать за собой: `kubectl delete namespace learn-09 learn-09b` и удалить кластер.

## 1. 🟢 Первый Ingress

**Задача:** установи контроллер ingress-nginx (одноразовый кластер) и создай Ingress для домена `ing.example.test`: путь `/api` → `web-v1`, `/` → `web-v2`. Проверь три запроса: `/`, `/api/` и чужой `Host`.

**Ожидаемый результат:** ответы `web-v2`, `web-v1` и `404`.

<details><summary>Подсказка</summary>

Обращайся к Service контроллера изнутри кластера (`kubectl -n ingress-nginx get svc`) и задай заголовок `Host`.

</details>

<details><summary>Решение</summary>

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

```text
v2 from web-v2-577d96dff8-mk2zd
v1 api from web-v1-584794c796-qxvt5
wronghost=404
```

Домен определяется заголовком `Host`: запрос с другим доменом правил не находит.

</details>

## 2. 🟢 Ingress: ошибки и TLS

**Задача:** (а) создай Ingress без `pathType` и объясни ошибку. (б) Создай TLS-секрет (`openssl`, `kubectl create secret tls app-tls`) и Ingress с `tls` для `app.example.test`; проверь HTTPS и что делает HTTP-запрос.

**Ожидаемый результат:** (а) `pathType must be specified`; (б) HTTPS отвечает, HTTP получает `308` на HTTPS.

<details><summary>Подсказка</summary>

Самоподписанный сертификат: `openssl req -x509 -nodes -newkey rsa:2048 -keyout tls.key -out tls.crt -subj "/CN=app.example.test" -addext "subjectAltName=DNS:app.example.test"`. HTTPS проверяй с `curl -k --resolve`.

</details>

<details><summary>Решение</summary>

```text
The Ingress "nopt" is invalid: spec.rules[0].http.paths[0].pathType: Required value: pathType must be specified
```

```yaml
spec:
  ingressClassName: nginx
  tls: [{hosts: [app.example.test], secretName: app-tls}]
  rules: [{host: app.example.test, http: {paths: [{path: /, pathType: Prefix, backend: {service: {name: web-v2, port: {number: 80}}}}]}}]
```

```text
$ curl -sk --resolve app.example.test:443:<IP контроллера> https://app.example.test/
v2 from web-v2-577d96dff8-kfzxp
$ curl -sk -o /dev/null -w "http->%{http_code} redirect=%{redirect_url}\n" -H 'Host: app.example.test' http://<IP>/
http->308 redirect=https://app.example.test/
```

`pathType` обязателен; контроллер ingress-nginx при наличии `tls` перенаправляет HTTP на HTTPS (поведение контроллера).

</details>

## 3. 🟡 Gateway и HTTPRoute

**Задача:** установи Envoy Gateway v1.7.4, создай `GatewayClass`, `Gateway` на порту 80 и `HTTPRoute` для `app.example.test` (`/api` → `web-v1`, `/` → `web-v2`). Покажи статусы `Gateway` и `HTTPRoute` и объясни `Programmed=False`.

**Ожидаемый результат:** `Accepted=True`, `Programmed=False (AddressNotAssigned)`; маршрут `Accepted=True`, `ResolvedRefs=True`; запросы к ClusterIP прокси отвечают.

<details><summary>Подсказка</summary>

`kubectl apply --server-side -f https://github.com/envoyproxy/gateway/releases/download/v1.7.4/install.yaml`. Service прокси ищи в `envoy-gateway-system` по метке `gateway.envoyproxy.io/owning-gateway-name=main-gw`.

</details>

<details><summary>Решение</summary>

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
Accepted=True (Accepted)
Programmed=False (AddressNotAssigned)
```

`Programmed=False`: в kind нет облака, чтобы выдать адрес Service типа `LoadBalancer`; прокси работает, а запросы с `Host: app.example.test` на ClusterIP Service `envoy-learn-09-main-gw-…` отвечают `web-v2` и `web-v1` (для `/api/`).

</details>

## 4. 🟡 Веса, заголовок, редирект

**Задача:** дополни `HTTPRoute`: `/canary` делит трафик 80/20 между `web-v1` и `web-v2` и убирает префикс `/canary`; запрос с заголовком `x-version: v1` идёт на `web-v1`; `/old` перенаправляется на `/new` кодом 301. Проверь каждое поведение.

**Ожидаемый результат:** около 80 и 20 из 100 запросов; ответ `web-v1` по заголовку; `301` с `location`.

<details><summary>Подсказка</summary>

Веса — `weight` у `backendRefs`; перезапись — `URLRewrite` с `ReplacePrefixMatch`; редирект — `RequestRedirect`.

</details>

<details><summary>Решение</summary>

```yaml
rules:
  - matches: [{path: {type: PathPrefix, value: /canary}}]
    filters:
      - type: URLRewrite
        urlRewrite: {path: {type: ReplacePrefixMatch, replacePrefixMatch: /}}
    backendRefs:
      - {name: web-v1, port: 80, weight: 80}
      - {name: web-v2, port: 80, weight: 20}
  - matches: [{headers: [{name: x-version, value: v1}]}]
    backendRefs: [{name: web-v1, port: 80}]
  - matches: [{path: {type: Exact, value: /old}}]
    filters:
      - type: RequestRedirect
        requestRedirect: {path: {type: ReplaceFullPath, replaceFullPath: /new}, statusCode: 301}
  - matches: [{path: {type: PathPrefix, value: /}}]
    backendRefs: [{name: web-v2, port: 80}]
```

```text
     81 v1
     19 v2
v1 from web-v1-584794c796-qcsdd
HTTP/1.1 301 Moved Permanently
location: http://app.example.test/new
```

Распределение веса случайно, но в среднем соответствует 80/20; заголовок и редирект работают стандартными полями без аннотаций.

</details>

## 5. 🔴 TLS, разрешения и ошибки по статусам

**Задача:** (а) добавь HTTPS-слушатель с сертификатом из Secret и проверь HTTPS. (б) Создай `HTTPRoute` в другом namespace с `parentRefs` на Gateway и объясни статус. (в) Создай маршрут на несуществующий Service и покажи, что получает клиент.

**Ожидаемый результат:** (а) оба слушателя `Programmed=True`; (б) `Accepted=False (NotAllowedByListeners)`; (в) `ResolvedRefs=False (BackendNotFound)` и `http=500`.

<details><summary>Подсказка</summary>

TLS: `tls: {mode: Terminate, certificateRefs: [{kind: Secret, name: app-tls}]}`. Для (б) укажи `namespace` в `parentRefs`. Статусы — в `status.parents[0].conditions`.

</details>

<details><summary>Решение</summary>

```yaml
listeners:
  - name: https
    protocol: HTTPS
    port: 443
    hostname: app.example.test
    tls:
      mode: Terminate
      certificateRefs: [{kind: Secret, name: app-tls}]
    allowedRoutes: {namespaces: {from: Same}}
```

```text
http attached=1 Programmed=True Accepted=True ResolvedRefs=True
https attached=1 Programmed=True Accepted=True ResolvedRefs=True
Accepted=False (NotAllowedByListeners)
ResolvedRefs=False (BackendNotFound)          # для маршрута на несуществующий Service
http=500
```

Слушатель допускает только свой namespace, поэтому чужой маршрут не привязан. Маршрут на несуществующий Service принят, но ссылка не разрешена, а клиент получает `500`.

</details>
