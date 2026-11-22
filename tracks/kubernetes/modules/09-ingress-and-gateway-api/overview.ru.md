---
title: "Ingress и Gateway API"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 09-ingress-and-gateway-api
part: 2
levels: [1, 2, 3, 4]
tags: [ingress, gateway-api, httproute, gateway, ingress-controller, envoy-gateway, ingress-nginx, tls, routing]
versions: "Kubernetes 1.37.0 (kind v0.34), Gateway API CRDs v1.4.1, Envoy Gateway v1.7.4, ingress-nginx 1.15.1 (проект выведен из поддержки)"
status: draft
updated: 2026-11-22
---

# Ingress и Gateway API

🌐 **Русский** · [English](overview.en.md)

## Коротко

[Service](../06-services/overview.ru.md) даёт стабильный адрес внутри кластера, но не умеет маршрутизировать HTTP по доменам и путям и терминировать TLS. Для входящего HTTP(S)-трафика есть два API. **Ingress** — старый и простой: правила «хост и путь → Service». **Gateway API** — его преемник: роли разделены (`GatewayClass`, `Gateway`, `HTTPRoute`), а возможности вроде весов, редиректов и перезаписи путей описаны типизированными полями, а не аннотациями. Сами объекты ничего не делают: нужен **контроллер**, который превращает их в настройку прокси.

## Ключевые идеи

- **Один вход вместо множества `LoadBalancer`.** Один балансировщик и один IP обслуживают много приложений, маршрутизируя по `Host` и пути.
- **Объект без контроллера бесполезен.** Ingress или Gateway создаются, принимаются API, но трафик пойдёт только если в кластере работает контроллер (ingress-nginx, Envoy Gateway, Traefik и др.).
- **Ingress замораживается, Gateway API развивается.** Проект ingress-nginx с марта 2026 года не получает обновлений; сообщество рекомендует Gateway API или другой контроллер.
- **Gateway API делит роли:** администратор кластера создаёт `GatewayClass` и `Gateway`, разработчик приложения — `HTTPRoute` в своём namespace.
- **Статус объектов рассказывает, что не так:** условия `Accepted`, `Programmed`, `ResolvedRefs` с причинами (`NotAllowedByListeners`, `BackendNotFound`).
- **Нет совпадения — 404, нет backend — 500.**
- **Аннотации Ingress зависят от контроллера;** в Gateway API те же вещи (редирект, перезапись, веса) — часть стандарта.

## Минимальный пример

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

```text
$ kubectl -n learn-09 exec client -- wget -qO- --header "Host: app.example.test" http://10.96.33.43/api/
v1 api from web-v1-584794c796-qxvt5
```

## Когда применять, а когда нет

- ✅ Несколько HTTP-приложений за одним адресом: маршрутизация по домену и пути, TLS на входе.
- ✅ Canary-выкатка по весам и по заголовкам (Gateway API).
- ❌ Не-HTTP протоколы (чистый TCP, базы данных): для них `Service` типа `LoadBalancer` или `TCPRoute`/`UDPRoute` Gateway API.
- ❌ Для новых проектов не стройте архитектуру на аннотациях ingress-nginx: проект не поддерживается, выбирайте Gateway API.
- ❌ Внутренний трафик между подами не нужно пускать через Ingress: для него Service.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | зачем нужен вход, контроллер и IngressClass, Ingress с хостами и путями, TLS, первый Gateway и HTTPRoute |
| [2. Продвинутая теория](advanced.ru.md) | сопоставление по пути и заголовкам, веса, редирект, перезапись, TLS-слушатель, разрешения между namespace, диагностика по статусам, миграция с Ingress |
| [3. Экспертная теория](expert.ru.md) | устройство контроллера и data plane, ресурсы Gateway API, справочник полей и фильтров, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 09-ingress-and-gateway-api level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 09-ingress-and-gateway-api level 6` |

## Связанные материалы

- Пререквизиты: [06 · Service и сеть](../06-services/overview.ru.md), [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md) (TLS-секреты)
- Дальше: модуль 10 · Namespace и RBAC (в дорожной карте [трека](../../overview.ru.md))
- Выкатка и canary: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — сеть, TLS, прокси
