---
title: "Ingress и Gateway API: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 09-ingress-and-gateway-api
part: 2
tags: [gateway-api, httproute, traffic-splitting, canary, redirect, urlrewrite, tls, referencegrant, ingress-annotations, status-conditions]
versions: "Kubernetes 1.37.0 (kind v0.34), Gateway API CRDs v1.4.1, Envoy Gateway v1.7.4, ingress-nginx 1.15.1 (проект выведен из поддержки)"
status: draft
updated: 2026-11-22
---

# Ingress и Gateway API: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): установка контроллеров, `Ingress`, `Gateway` и простой `HTTPRoute` не повторяются.

## Маршруты с несколькими правилами

Расширенный `HTTPRoute` с четырьмя правилами: веса, заголовок, редирект и общий маршрут:

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: HTTPRoute
metadata: {name: web-route}
spec:
  parentRefs: [{name: main-gw}]
  hostnames: ["app.example.test"]
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

### Веса: canary-выкатка

`weight` у `backendRefs` делит трафик между версиями. 100 запросов на `/canary/`:

```text
$ kubectl -n learn-09 exec curl -- sh -c "for i in \$(seq 1 100); do curl -s -H 'Host: app.example.test' http://10.96.33.43/canary/ | cut -c1-2; done | sort | uniq -c"
     81 v1
     19 v2
```

Распределение близко к 80/20 (81 и 19). Меняя веса, вы постепенно переводите трафик на новую версию без переразвёртывания: достаточно поправить `HTTPRoute`. Если делать то же без Gateway API, нужно либо менять число реплик двух Deployment (грубо, как в модуле 06), либо использовать аннотации контроллера.

Фильтр `URLRewrite` с `ReplacePrefixMatch: /` убрал префикс `/canary` перед отправкой на backend: `/canary/` дошёл до приложения как `/`.

### Совпадение по заголовку

```text
$ kubectl -n learn-09 exec curl -- curl -s -H "Host: app.example.test" -H "x-version: v1" http://10.96.33.43/
v1 from web-v1-584794c796-qcsdd
$ kubectl -n learn-09 exec curl -- curl -s -H "Host: app.example.test" http://10.96.33.43/
v2 from web-v2-577d96dff8-mk2zd
```

С заголовком `x-version: v1` запрос ушёл на `web-v1`, без него — на `web-v2`. Так делают «тестовые» маршруты для команды, не затрагивая остальных пользователей.

### Редирект

```text
$ kubectl -n learn-09 exec curl -- curl -s -o /dev/null -D - -H "Host: app.example.test" http://10.96.33.43/old
HTTP/1.1 301 Moved Permanently
location: http://app.example.test/new
```

`RequestRedirect` вернул `301` с новым адресом; запрос до приложения не дошёл.

Условия сопоставления: путь (`Exact`, `PathPrefix`, `RegularExpression`), заголовки, параметры запроса, HTTP-метод. Когда подходит несколько правил, у Gateway API есть детерминированный порядок (точный путь, более длинный префикс и т. д.).

> ⚠️ Проверить: точный порядок приоритета правил `HTTPRoute` и значение `RegularExpression` не проверялись запуском; сверяйте со [спецификацией](https://gateway-api.sigs.k8s.io/reference/spec/).

## TLS-слушатель

Gateway завершает TLS на слушателе, сертификат берётся из Secret ([модуль 07](../07-config-and-secrets/advanced.ru.md#типы-secret)):

```yaml
listeners:
  - name: http
    protocol: HTTP
    port: 80
    allowedRoutes: {namespaces: {from: Same}}
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
$ kubectl -n learn-09 get gateway main-gw -o jsonpath='{range .status.listeners[*]}{.name} attached={.attachedRoutes} {range .conditions[*]}{.type}={.status} {end}{"\n"}{end}'
http attached=1 Programmed=True Accepted=True ResolvedRefs=True
https attached=1 Programmed=True Accepted=True ResolvedRefs=True
$ kubectl -n learn-09 exec curl -- curl -sk --resolve app.example.test:443:10.96.33.43 https://app.example.test/
v2 from web-v2-577d96dff8-kfzxp
```

У каждого слушателя свой статус и счётчик привязанных маршрутов (`attachedRoutes`). Теперь `Programmed=True` у слушателей, хотя у самого Gateway остаётся `AddressNotAssigned` (нет внешнего адреса). Режим `Terminate` расшифровывает трафик на Gateway; `Passthrough` пропускает TLS до backend (для `TLSRoute`).

## Разрешения между namespace

Маршрут привязывается к Gateway, только если слушатель его допускает. Слушатель с `allowedRoutes: {namespaces: {from: Same}}` принимает маршруты лишь из своего namespace. Маршрут из другого namespace отвергается:

```text
$ kubectl -n learn-09b get httproute other-route -o jsonpath='{range .status.parents[0].conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=False (NotAllowedByListeners)
ResolvedRefs=False (BackendNotFound)
```

`Accepted=False (NotAllowedByListeners)`. Второе условие (`BackendNotFound`) возникло, потому что Service `web-v1` ищется в namespace маршрута `learn-09b`, а он там не существует. Разрешить чужие namespace можно в `allowedRoutes` (`from: All` или `from: Selector`). Ссылка маршрута на Service в **другом** namespace требует объекта `ReferenceGrant` в namespace цели.

> ⚠️ Проверить: `from: All`, `from: Selector` и `ReferenceGrant` не запускались в этом материале.

## Диагностика по статусам

Gateway API кладёт причины прямо в `status`:

| Объект | Условие | Значение |
|---|---|---|
| GatewayClass | `Accepted` | контроллер принял класс |
| Gateway | `Accepted`, `Programmed` | принят и настроен (`AddressNotAssigned` — нет адреса) |
| Listener | `Programmed`, `ResolvedRefs` | слушатель готов, ссылки (Secret) найдены |
| HTTPRoute | `Accepted` | маршрут привязан к родителю (`NotAllowedByListeners`) |
| HTTPRoute | `ResolvedRefs` | backend-ы найдены (`BackendNotFound`) |

Маршрут на несуществующий Service принят, но ссылка не разрешена, а запрос получает `500`:

```text
$ kubectl -n learn-09 get httproute broken -o jsonpath='{range .status.parents[0].conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=True (Accepted)
ResolvedRefs=False (BackendNotFound)
$ kubectl -n learn-09 exec curl -- curl -s -o /dev/null -w "http=%{http_code}\n" -H "Host: broken.example.test" http://10.96.33.43/
http=500
```

Запомните: **нет совпадения — `404`, backend не найден — `500`**. В Ingress такой диагностики нет: приходится смотреть логи контроллера.

## Аннотации Ingress и типизированные поля

Возможности Ingress за пределами «хост и путь» задаются аннотациями контроллера. Пример ingress-nginx: путь с группой захвата и перезапись на `/api/$1`:

```yaml
metadata:
  annotations: {nginx.ingress.kubernetes.io/rewrite-target: /api/$1}
spec:
  ingressClassName: nginx
  rules:
    - host: rw.test
      http:
        paths:
          - {path: /shop/(.*), pathType: ImplementationSpecific, backend: {service: {name: web-v1, port: {number: 80}}}}
```

```text
$ kubectl -n learn-09 exec curl -- curl -s -H 'Host: rw.test' http://10.96.78.112/shop/
v1 api from web-v1-584794c796-qcsdd
```

Запрос `/shop/` ушёл на `/api/` backend. Эта аннотация и регулярное выражение работают только в ingress-nginx; другой контроллер их проигнорирует. В Gateway API то же задаётся стандартным `URLRewrite`, редирект — `RequestRedirect`, веса — `weight`.

**Ingress без `ingressClassName`.** Объект без класса нашёл обработчик: контроллер в kind запущен с флагом `--watch-ingress-without-class=true`, поэтому `noclass.test` ответил `200`. Обработка объектов без класса зависит от настроек контроллера: не рассчитывайте на неё, всегда указывайте `ingressClassName`.

## Миграция с Ingress на Gateway API

1. Установите CRD Gateway API и контроллер.
2. Создайте `GatewayClass` и `Gateway` с теми же портами и сертификатами.
3. Перенесите правила: каждая пара «хост + путь» превращается в `HTTPRoute`; аннотации заменяются фильтрами (`URLRewrite`, `RequestRedirect`, веса).
4. Проверьте оба входа параллельно (разные адреса), затем переключите DNS.

Для автоматического преобразования существует утилита `ingress2gateway`.

> ⚠️ Проверить: утилита `ingress2gateway` и перенос аннотаций ingress-nginx не запускались в этом материале.

## Лучшие практики

- Новые входы делайте на Gateway API; Ingress оставляйте для существующих систем.
- Разделяйте роли: `Gateway` и сертификаты — у администратора, `HTTPRoute` — у команд приложений.
- Ограничивайте `allowedRoutes`: не пускайте маршруты из любых namespace.
- Проверяйте `status` объектов (`Accepted`, `ResolvedRefs`) после каждого изменения.
- TLS-сертификаты автоматизируйте (cert-manager, [инструмент трека](../../overview.ru.md#инструменты)); не храните ключи в Git.
- Для canary используйте `weight`, а не изменение числа реплик.

## Частые ошибки

- ❌ Использовать аннотации одного контроллера, не зная, что другой их проигнорирует → ✅ типизированные поля Gateway API.
- ❌ Путать `404` и `500` при диагностике → ✅ 404 — нет правила, 500 — backend не найден (смотрите `ResolvedRefs`).
- ❌ Создавать маршрут в другом namespace и ждать привязки → ✅ `allowedRoutes` слушателя.
- ❌ Ссылаться в маршруте на Service из другого namespace без `ReferenceGrant` → ✅ добавить разрешение.
- ❌ Ждать внешнего IP у Gateway в kind → ✅ используйте ClusterIP или проброс порта.

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): контроллер входа — обратный прокси (nginx, Envoy) с конфигурацией, которую обновляет контроллер.
- [C#](../../../csharp/modules/14-aspnet-core-minimal-api/overview.ru.md): такой прокси стоит перед приложением ASP.NET Core и пробрасывает заголовки; приложению нужно доверять `X-Forwarded-*` только от своего прокси.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Предыдущие модули: [06 · Service и сеть](../06-services/overview.ru.md), [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)

## Источники

- [Gateway API: HTTPRoute](https://gateway-api.sigs.k8s.io/api-types/httproute/)
- [Gateway API: Gateway](https://gateway-api.sigs.k8s.io/api-types/gateway/)
- [Gateway API: ReferenceGrant](https://gateway-api.sigs.k8s.io/api-types/referencegrant/)
- [Gateway API: спецификация](https://gateway-api.sigs.k8s.io/reference/spec/)
- [Ingress: TLS и pathType](https://kubernetes.io/docs/concepts/services-networking/ingress/)
- [Migrating from Ingress](https://gateway-api.sigs.k8s.io/guides/migrating-from-ingress/)
