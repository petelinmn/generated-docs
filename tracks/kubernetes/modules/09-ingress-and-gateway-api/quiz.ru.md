---
title: "Квиз: Ingress и Gateway API"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 09-ingress-and-gateway-api
part: 2
tags: [ingress, gateway-api, httproute, ingress-controller, traffic-splitting]
status: draft
updated: 2026-11-22
---

# Квиз: Ingress и Gateway API

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Вы создали Ingress, но трафик не идёт. Что проверить в первую очередь?

- A) Размер образа
- B) Что в кластере работает контроллер входа и совпадает `ingressClassName`
- C) Число реплик Deployment
- D) Версию kubectl

<details><summary>Ответ</summary>

**B.** Ingress — только описание; исполняет его контроллер своего класса (`kubectl get ingressclass`). Затем проверьте правило под ваш `Host` и путь и endpoints Service.
→ [Базовая теория](basics.ru.md#контроллер-без-него-ничего-не-работает)

</details>

## 2. 🟢 Что вернёт запрос с `Host`, для которого нет правила?

- A) `200` с первым backend
- B) `404`
- C) `500`
- D) `503`

<details><summary>Ответ</summary>

**B.** В опыте и Ingress, и Gateway вернули `404` на `Host: x.test` и `Host: other.test`.
→ [Базовая теория](basics.ru.md#запрос)

</details>

## 3. 🟢 Что обязательно указать в каждом пути Ingress?

- A) `weight`
- B) `pathType`
- C) `hostname`
- D) `namespace`

<details><summary>Ответ</summary>

**B.** Без него: `pathType must be specified`. Значения: `Exact`, `Prefix`, `ImplementationSpecific`.
→ [Базовая теория](basics.ru.md#pathtype-обязателен)

</details>

## 4. 🟢 Кто создаёт какие объекты Gateway API?

- A) Все объекты — разработчик приложения
- B) `GatewayClass` — поставщик контроллера, `Gateway` — администратор, `HTTPRoute` — разработчик приложения
- C) Только администратор создаёт `HTTPRoute`
- D) Gateway API не разделяет роли

<details><summary>Ответ</summary>

**B.** Разделение ролей — ключевое отличие от Ingress.
→ [Базовая теория](basics.ru.md#gateway-api)

</details>

## 5. 🟡 Как отправить 20% трафика на новую версию средствами Gateway API?

- A) Аннотацией `canary` у Ingress
- B) Двумя `backendRefs` с весами в `HTTPRoute` (например, 80 и 20)
- C) Изменить число реплик
- D) Это невозможно

<details><summary>Ответ</summary>

**B.** В опыте 100 запросов дали 81 и 19. Веса — стандартная часть `HTTPRoute`.
→ [Продвинутая теория](advanced.ru.md#веса-canary-выкатка)

</details>

## 6. 🟡 `HTTPRoute` ссылается на несуществующий Service. Что произойдёт?

- A) Маршрут отвергнут с `Accepted=False`
- B) Маршрут принят, `ResolvedRefs=False (BackendNotFound)`, запросы получают `500`
- C) Запросы получают `404`
- D) Gateway перестаёт работать

<details><summary>Ответ</summary>

**B.** Отличайте `404` (нет правила) от `500` (backend не найден).
→ [Продвинутая теория](advanced.ru.md#диагностика-по-статусам)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) Слушатель с `allowedRoutes: {namespaces: {from: Same}}` принимает маршруты только из своего namespace
- [ ] B) Аннотация `nginx.ingress.kubernetes.io/rewrite-target` работает в любом контроллере
- [ ] C) `URLRewrite` — стандартный фильтр Gateway API
- [ ] D) В kind `Programmed=False (AddressNotAssigned)` у Gateway означает, что прокси не работает

<details><summary>Ответ</summary>

**A, C.** B неверно: аннотации специфичны для контроллера. D неверно: прокси работает, нет внешнего адреса; доступ по ClusterIP, слушатели `Programmed=True`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Маршрут из другого namespace не привязался. Какое условие вы увидите?

- A) `Accepted=True`
- B) `Accepted=False (NotAllowedByListeners)`
- C) `Programmed=False`
- D) `Ready=False`

<details><summary>Ответ</summary>

**B.** Слушатель не допускает чужой namespace. Разрешить можно в `allowedRoutes` (`All` или `Selector`).
→ [Продвинутая теория](advanced.ru.md#разрешения-между-namespace)

</details>

## 9. 🔴 Найди ошибку

> «Мы строим новую платформу на ingress-nginx с аннотациями: проект зрелый, и обновления безопасности будут».

<details><summary>Ответ</summary>

Проект ingress-nginx сообщества Kubernetes выведен из поддержки: после марта 2026 нет выпусков, исправлений и обновлений безопасности. Новая платформа должна использовать Gateway API или другой поддерживаемый контроллер, а аннотации конкретного контроллера привязывают к нему конфигурацию. Существующие развёртывания работают, но патчей не получают.
→ [Базовая теория](basics.ru.md#контроллер-без-него-ничего-не-работает)

</details>

## 10. 🔴 Открытый вопрос

Объясни, как запрос доходит от пользователя до пода при Gateway API, какие компоненты участвуют и чем это отличается от маршрутизации через Service.

<details><summary>Ключевые пункты ответа</summary>

- Контроллер наблюдает за `Gateway`, `HTTPRoute` и Service/EndpointSlice и собирает конфигурацию прокси (control plane).
- Прокси (в опыте Envoy; для каждого `Gateway` свой Deployment и Service) принимает трафик (data plane) и направляет его на адреса подов.
- Выбор backend происходит на уровне запроса (L7: хост, путь, заголовки, веса), а не соединения, как у kube-proxy.
- Service нужен как источник списка endpoints и как ClusterIP внутри кластера; внешний вход — Service самого прокси (`LoadBalancer`).
- Статус (`Accepted`, `Programmed`, `ResolvedRefs`) показывает, принят ли и разрешён ли каждый шаг.

</details>

→ [Экспертная теория](expert.ru.md#control-plane-и-data-plane)
