---
title: "Квиз: Service и сеть"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 06-services
part: 1
tags: [service, clusterip, dns, nodeport, headless-service, endpointslice]
status: draft
updated: 2026-11-22
---

# Квиз: Service и сеть

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Зачем нужен Service, если у каждого пода есть свой IP?

- A) IP пода нельзя использовать внутри кластера
- B) IP пода меняется при каждой замене, а у Service стабильный адрес и имя
- C) Service ускоряет сеть
- D) Service шифрует трафик

<details><summary>Ответ</summary>

**B.** В опыте после удаления подов их IP сменились (`.33–.35` → `.39–.41`), а ClusterIP Service остался `10.96.37.242`.
→ [Базовая теория](basics.ru.md#стабильный-адрес)

</details>

## 2. 🟢 Какие поды получают трафик Service?

- A) Все поды namespace
- B) Поды, метки которых совпадают с `selector`, и которые готовы (`Ready`)
- C) Только поды на той же ноде
- D) Поды, чьё имя совпадает с именем Service

<details><summary>Ответ</summary>

**B.** Адреса готовых подов с подходящими метками попадают в EndpointSlice.
→ [Базовая теория](basics.ru.md#endpoints-какие-поды-за-service)

</details>

## 3. 🟢 Какое DNS-имя у Service `web` в namespace `learn-06`?

- A) `web.learn-06.svc.cluster.local`
- B) `learn-06.web.cluster`
- C) `web.default.svc.cluster.local`
- D) `service.web.local`

<details><summary>Ответ</summary>

**A.** Формат `<service>.<namespace>.svc.cluster.local`; внутри namespace подойдёт и `web`.
→ [Базовая теория](basics.ru.md#dns-как-найти-service-по-имени)

</details>

## 4. 🟢 Чем отличаются `port` и `targetPort`?

- A) Ничем
- B) `port` — порт Service, `targetPort` — порт контейнера
- C) `port` — порт ноды, `targetPort` — порт Service
- D) `targetPort` нужен только для NodePort

<details><summary>Ответ</summary>

**B.** Клиент подключается к `port`, а трафик попадает на `targetPort` пода (число или имя порта). Порт ноды — `nodePort`.
→ [Базовая теория](basics.ru.md#манифест-service)

</details>

## 5. 🟡 Что покажет `kubectl get svc` для `LoadBalancer` в kind без дополнительных инструментов?

- A) Внешний IP через минуту
- B) `EXTERNAL-IP <pending>` и выделенный NodePort
- C) Ошибку при создании
- D) `EXTERNAL-IP localhost`

<details><summary>Ответ</summary>

**B.** Внешний адрес выдаёт облачный контроллер, а в kind его нет; в опыте `80:31708/TCP` и `<pending>`.
→ [Продвинутая теория](advanced.ru.md#loadbalancer)

</details>

## 6. 🟡 Что вернёт DNS для headless Service с тремя подами?

- A) Один ClusterIP
- B) Три A-записи с IP подов
- C) CNAME
- D) Ничего

<details><summary>Ответ</summary>

**B.** Без `clusterIP` DNS возвращает адреса готовых подов; kube-proxy не балансирует.
→ [Продвинутая теория](advanced.ru.md#headless-service)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) Service с опечаткой в `selector` даёт пустые endpoints и `Connection refused`
- [ ] B) Под с проваленной readiness probe попадает в endpoints
- [ ] C) ExternalName отдаёт CNAME на внешнее имя
- [ ] D) Диапазон NodePort по умолчанию — 30000–32767

<details><summary>Ответ</summary>

**A, C, D.** B неверно: в endpoints попадают только готовые поды (в опыте новый под `0/1` в списке отсутствовал).
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что выведет `kubectl exec other -- wget -qO- http://web` из пода в namespace `learn-06b`, если Service `web` в `learn-06`?

- A) Ответ приложения
- B) `wget: bad address 'web'`
- C) `Connection refused`
- D) Таймаут

<details><summary>Ответ</summary>

**B.** В `search` подставляется namespace пода (`learn-06b`). Нужно `web.learn-06`.
→ [Экспертная теория](expert.ru.md#подвох-короткое-имя-работает-только-в-своём-namespace)

</details>

## 9. 🔴 Найди ошибку

> «`ping 10.96.37.242` не отвечает, значит Service не работает и поды недоступны».

<details><summary>Ответ</summary>

ClusterIP виртуальный: правила kube-proxy (DNAT) работают для TCP/UDP-портов, ICMP не обрабатывается, поэтому ping всегда даёт 100% потерь (проверено). Работоспособность проверяют обращением к порту: `wget http://web`, `nc`. Полезно также `kubectl get endpoints web`.
→ [Экспертная теория](expert.ru.md#подвох-ping-на-clusterip-не-работает)

</details>

## 10. 🔴 Открытый вопрос

Приложение получает `Connection refused` от Service. Опиши порядок диагностики и две возможные причины, которые дают одинаковый симптом.

<details><summary>Ключевые пункты ответа</summary>

- Сначала `kubectl get endpointslices -l kubernetes.io/service-name=<svc>`: пусто или нет.
- Пусто → опечатка в `selector` или поды не готовы (сверить `kubectl get pods --show-labels`, статус `READY`, readiness probe).
- Адреса есть, но порт чужой → неверный `targetPort`: сверить с портом, который слушает контейнер.
- Оба случая дают `Connection refused` (в опыте `wget: can't connect to remote host … Connection refused`).
- Дополнительно: проверить DNS (`nslookup`), namespace в имени, NetworkPolicy.

</details>

→ [Продвинутая теория](advanced.ru.md#типичные-поломки)
