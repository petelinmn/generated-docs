---
title: "Service и сеть"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 06-services
part: 1
levels: [1, 2, 3, 4]
tags: [service, clusterip, nodeport, loadbalancer, dns, coredns, endpointslice, kube-proxy, headless-service]
versions: "Kubernetes 1.37.0 (kind v0.34, CoreDNS, kube-proxy iptables), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Service и сеть

🌐 **Русский** · [English](overview.en.md)

## Коротко

Поды эфемерны, и их IP меняются при каждой замене. Service даёт набору подов стабильный адрес (виртуальный ClusterIP и DNS-имя) и распределяет трафик между готовыми подами. Поды выбираются по меткам, а адреса подов Kubernetes ведёт сам в объектах EndpointSlice. Именно через Service приложения внутри кластера находят друг друга.

## Ключевые идеи

- **Service выбирает поды по меткам** (`selector`) и направляет трафик только на готовые (`Ready`).
- **ClusterIP — виртуальный адрес,** он не принадлежит ни одному интерфейсу; его не «пингуют», а используют по портам. Адрес не меняется, пока жив Service.
- **DNS:** CoreDNS отвечает на `<сервис>.<namespace>.svc.cluster.local`; внутри namespace достаточно короткого имени `web`.
- **Типы:** `ClusterIP` (по умолчанию, внутри кластера), `NodePort` (порт на каждой ноде, 30000–32767), `LoadBalancer` (внешний балансировщик облака), `ExternalName` (CNAME на внешнее имя), headless (`clusterIP: None`, DNS возвращает адреса подов).
- **Балансировку делает kube-proxy** на каждой ноде правилами (iptables): выбор пода случайный.
- **Ошибки селектора и портов дают `Connection refused`,** а не понятное сообщение: проверяйте endpoints.
- **`port` и `targetPort`:** порт Service и порт контейнера могут различаться; `targetPort` можно задать именем порта.

## Минимальный пример

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

```text
$ kubectl -n learn-06 get svc web
NAME   TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)   AGE
web    ClusterIP   10.96.37.242   <none>        80/TCP    1s
$ kubectl -n learn-06 exec client -- wget -qO- http://web
hello from web-8678fd4948-7xm64
```

## Когда применять, а когда нет

- ✅ Любое взаимодействие между приложениями внутри кластера: используйте имя Service, а не IP пода.
- ✅ `NodePort` — для опытов и доступа в kind; `LoadBalancer` — для выставления наружу в облаке.
- ❌ Не подключайтесь к подам по их IP: он меняется.
- ❌ Для HTTP-маршрутизации по путям и доменам, TLS нужен Ingress или Gateway API (модуль 09), а не десятки `LoadBalancer`.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | манифест Service, ClusterIP и endpoints, балансировка, DNS-имена, `port-forward` к Service |
| [2. Продвинутая теория](advanced.ru.md) | NodePort, LoadBalancer, headless, ExternalName, готовность и endpoints, типичные поломки, переменные окружения, правила kube-proxy |
| [3. Экспертная теория](expert.ru.md) | EndpointSlice и DNS изнутри, справочник полей Service, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 06-services level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 06-services level 6` |

## Связанные материалы

- Пререквизиты: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md), [04 · Pod](../04-pods/overview.ru.md)
- Дальше: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Компоненты сети: [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md) — kube-proxy и CoreDNS
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — iptables и сеть
- Безопасность подов и сети: [17 · Безопасность](../17-security/overview.ru.md)
- Диагностика: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
