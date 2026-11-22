---
title: "Упражнения: Service и сеть"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 06-services
part: 1
tags: [service, clusterip, dns, headless-service, nodeport, endpointslice]
status: draft
updated: 2026-11-22
---

# Упражнения: Service и сеть

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-06`. Приложение (Deployment `web` и под `client`) — из [базовой теории](basics.ru.md#приложение-для-опытов). Убрать за собой: `kubectl delete namespace learn-06`.

## 1. 🟢 Первый Service

**Задача:** создай Service `web` для Deployment `web`, найди его ClusterIP и endpoints, и сделай десять запросов из пода `client`. Сколько разных подов ответило?

**Ожидаемый результат:** ClusterIP из диапазона `10.96.0.0/16`, три endpoints, ответы от трёх подов в случайной пропорции.

<details><summary>Подсказка</summary>

`kubectl expose` или манифест с `selector: {app: web}` и `targetPort: http`. Запросы: `kubectl exec client -- wget -qO- http://web`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Service
metadata: {name: web}
spec:
  selector: {app: web}
  ports:
    - {name: http, port: 80, targetPort: http}
```

```bash
kubectl -n learn-06 apply -f svc.yaml
kubectl -n learn-06 get svc web -o wide
kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web
for i in $(seq 1 10); do kubectl -n learn-06 exec client -- wget -qO- -T 3 http://web; done | sort | uniq -c
```

```text
      3 hello from web-8678fd4948-5p9dp
      2 hello from web-8678fd4948-7xm64
      5 hello from web-8678fd4948-hb7n7
```

Ответили все три пода: выбор случайный, поэтому доли неравные.

</details>

## 2. 🟢 Имена DNS

**Задача:** обратись к Service четырьмя способами (`web`, `web.learn-06`, `web.learn-06.svc`, `web.learn-06.svc.cluster.local`), покажи `/etc/resolv.conf` пода и результат `nslookup`.

**Ожидаемый результат:** все имена отвечают; `search` начинается с `learn-06.svc.cluster.local`, `nameserver` — `10.96.0.10`.

<details><summary>Подсказка</summary>

`kubectl exec client -- cat /etc/resolv.conf`, `kubectl exec client -- nslookup web.learn-06.svc.cluster.local`.

</details>

<details><summary>Решение</summary>

```bash
for n in web web.learn-06 web.learn-06.svc web.learn-06.svc.cluster.local; do
  kubectl -n learn-06 exec client -- wget -qO- -T 3 http://$n
done
kubectl -n learn-06 exec client -- cat /etc/resolv.conf
kubectl -n learn-06 exec client -- nslookup web.learn-06.svc.cluster.local
```

```text
search learn-06.svc.cluster.local svc.cluster.local cluster.local
nameserver 10.96.0.10
options ndots:5
Name:	web.learn-06.svc.cluster.local
Address: 10.96.37.242
```

Короткое имя работает из-за списка `search`; `10.96.0.10` — ClusterIP CoreDNS.

</details>

## 3. 🟡 Стабильность адреса и две поломки

**Задача:** (а) удали все поды `web` и убедись, что ClusterIP не изменился, а endpoints обновились. (б) Создай два сломанных Service: с опечаткой в `selector` и с неверным `targetPort: 8081`. Покажи, как их различить по endpoints.

**Ожидаемый результат:** (а) прежний ClusterIP; (б) у первого `<nil>`, у второго адрес с портом `8081`, оба дают `Connection refused`.

<details><summary>Подсказка</summary>

`kubectl get endpointslices -l kubernetes.io/service-name=<имя> -o jsonpath='{.items[0].endpoints[0].addresses} port={.items[0].ports[0].port}'`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: v1
kind: Service
metadata: {name: web-typo}
spec:
  selector: {app: wbe}
  ports: [{port: 80, targetPort: http}]
---
apiVersion: v1
kind: Service
metadata: {name: web-badport}
spec:
  selector: {app: web}
  ports: [{port: 80, targetPort: 8081}]
```

```text
$ kubectl -n learn-06 get svc web -o jsonpath='{.spec.clusterIP}'      # до и после удаления подов
10.96.37.242
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-typo -o custom-columns=NAME:.metadata.name,ENDPOINTS:.endpoints
web-typo-6vqrs   <nil>
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-badport -o jsonpath='{.items[0].endpoints[0].addresses} port={.items[0].ports[0].port}'
["10.244.0.33"] port=8081
$ kubectl -n learn-06 exec client2 -- wget -qO- -T 3 http://web-typo
wget: can't connect to remote host (10.96.75.1): Connection refused
```

Опечатка в `selector` оставляет endpoints пустыми, неверный `targetPort` даёт адрес с чужим портом. Симптом у обоих один.

</details>

## 4. 🟡 NodePort, headless и ExternalName

**Задача:** создай Service `web-np` (NodePort 30080), headless `web-headless` и `ext` (ExternalName `example.com`). Проверь NodePort изнутри ноды kind, DNS-ответ headless-сервиса и CNAME у `ext`.

**Ожидаемый результат:** ответ приложения на `<IP ноды>:30080`, три A-записи у headless, `canonical name = example.com` у `ext`.

<details><summary>Подсказка</summary>

IP ноды: `kubectl get node -o wide`. Проверка NodePort: `docker exec learn-control-plane curl -s http://<IP>:30080`. DNS: `nslookup` из пода.

</details>

<details><summary>Решение</summary>

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
$ docker exec learn-control-plane curl -s -m 3 http://172.18.0.9:30080
hello from web-8678fd4948-5p9dp
$ kubectl -n learn-06 exec client2 -- nslookup web-headless
Address: 10.244.0.33
Address: 10.244.0.34
Address: 10.244.0.35
$ kubectl -n learn-06 exec client2 -- nslookup ext
ext.learn-06.svc.cluster.local	canonical name = example.com
```

NodePort открывает порт на каждой ноде, headless отдаёт адреса подов вместо ClusterIP, ExternalName даёт CNAME без прокси.

</details>

## 5. 🔴 Правила kube-proxy и readiness

**Задача:** (а) найди на ноде kind правила `iptables` для Service `web` и объясни вероятности `0.333…` и `0.5`. (б) Добавь Deployment readiness probe, которая никогда не проходит, и покажи, что новый под не попадает в endpoints, а старые продолжают получать трафик. Откатись.

**Ожидаемый результат:** три правила `KUBE-SEP` с вероятностями 1/3, 1/2 и без условия; новый под `0/1` вне endpoints.

<details><summary>Подсказка</summary>

`docker exec learn-control-plane iptables-save -t nat | grep "learn-06/web:http"`. Readiness probe: `httpGet` на путь, которого нет (например, `/ready`).

</details>

<details><summary>Решение</summary>

```bash
docker exec learn-control-plane sh -c 'iptables-save -t nat | grep "learn-06/web:http"'
kubectl -n learn-06 patch deploy web --type=json -p '[{"op":"add","path":"/spec/template/spec/containers/0/readinessProbe","value":{"httpGet":{"path":"/ready","port":80},"periodSeconds":2}}]'
sleep 20
kubectl -n learn-06 get pods -l app=web
kubectl -n learn-06 get endpoints web
kubectl -n learn-06 rollout undo deploy/web
```

```text
-A KUBE-SVC-… "learn-06/web:http -> 10.244.0.39:80" -m statistic --mode random --probability 0.33333333349 -j KUBE-SEP-…
-A KUBE-SVC-… "learn-06/web:http -> 10.244.0.40:80" -m statistic --mode random --probability 0.50000000000 -j KUBE-SEP-…
-A KUBE-SVC-… "learn-06/web:http -> 10.244.0.41:80" -j KUBE-SEP-…
web-77db7cb557-gqn2v   0/1   Running
web-8678fd4948-5p9dp   1/1   Running
```

Первое правило срабатывает с вероятностью 1/3; из оставшихся 2/3 второе берёт половину (ещё 1/3); третье получает остаток (1/3): поровну на каждый под. Новый под не готов, поэтому его адреса нет в endpoints, а обновление остановилось, и старые поды продолжают принимать трафик.

</details>
