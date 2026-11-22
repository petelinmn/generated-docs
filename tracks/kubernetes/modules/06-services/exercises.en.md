---
title: "Exercises: Service and networking"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 06-services
part: 1
tags: [service, clusterip, dns, headless-service, nodeport, endpointslice]
status: draft
updated: 2026-11-22
---

# Exercises: Service and networking

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-06`. The application (the Deployment `web` and the pod `client`) is from [basics](basics.en.md#the-application-for-experiments). Clean up afterwards: `kubectl delete namespace learn-06`.

## 1. 🟢 The first Service

**Task:** create a Service `web` for the Deployment `web`, find its ClusterIP and endpoints, and make ten requests from the `client` pod. How many different pods answered?

**Expected result:** a ClusterIP from the range `10.96.0.0/16`, three endpoints, replies from three pods in a random proportion.

<details><summary>Hint</summary>

`kubectl expose` or a manifest with `selector: {app: web}` and `targetPort: http`. Requests: `kubectl exec client -- wget -qO- http://web`.

</details>

<details><summary>Solution</summary>

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

All three pods answered: the choice is random, so the shares are unequal.

</details>

## 2. 🟢 DNS names

**Task:** call the Service in four ways (`web`, `web.learn-06`, `web.learn-06.svc`, `web.learn-06.svc.cluster.local`), show the pod's `/etc/resolv.conf` and the `nslookup` result.

**Expected result:** all names answer; `search` starts with `learn-06.svc.cluster.local`, `nameserver` is `10.96.0.10`.

<details><summary>Hint</summary>

`kubectl exec client -- cat /etc/resolv.conf`, `kubectl exec client -- nslookup web.learn-06.svc.cluster.local`.

</details>

<details><summary>Solution</summary>

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

The short name works because of the `search` list; `10.96.0.10` is the ClusterIP of CoreDNS.

</details>

## 3. 🟡 Address stability and two failures

**Task:** (a) delete all `web` pods and make sure the ClusterIP did not change while the endpoints were updated. (b) Create two broken Services: with a typo in `selector` and with a wrong `targetPort: 8081`. Show how to tell them apart by endpoints.

**Expected result:** (a) the same ClusterIP; (b) the first has `<nil>`, the second an address with port `8081`, both give `Connection refused`.

<details><summary>Hint</summary>

`kubectl get endpointslices -l kubernetes.io/service-name=<name> -o jsonpath='{.items[0].endpoints[0].addresses} port={.items[0].ports[0].port}'`.

</details>

<details><summary>Solution</summary>

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
$ kubectl -n learn-06 get svc web -o jsonpath='{.spec.clusterIP}'      # before and after deleting the pods
10.96.37.242
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-typo -o custom-columns=NAME:.metadata.name,ENDPOINTS:.endpoints
web-typo-6vqrs   <nil>
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-badport -o jsonpath='{.items[0].endpoints[0].addresses} port={.items[0].ports[0].port}'
["10.244.0.33"] port=8081
$ kubectl -n learn-06 exec client2 -- wget -qO- -T 3 http://web-typo
wget: can't connect to remote host (10.96.75.1): Connection refused
```

A typo in `selector` leaves the endpoints empty, a wrong `targetPort` gives an address with a foreign port. The symptom is the same for both.

</details>

## 4. 🟡 NodePort, headless and ExternalName

**Task:** create a Service `web-np` (NodePort 30080), a headless `web-headless` and `ext` (ExternalName `example.com`). Check the NodePort from inside a kind node, the DNS answer of the headless service and the CNAME of `ext`.

**Expected result:** the application's reply on `<node IP>:30080`, three A records for the headless one, `canonical name = example.com` for `ext`.

<details><summary>Hint</summary>

The node IP: `kubectl get node -o wide`. NodePort check: `docker exec learn-control-plane curl -s http://<IP>:30080`. DNS: `nslookup` from a pod.

</details>

<details><summary>Solution</summary>

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

A NodePort opens a port on every node, a headless service returns pod addresses instead of a ClusterIP, ExternalName gives a CNAME with no proxy.

</details>

## 5. 🔴 kube-proxy rules and readiness

**Task:** (a) find the `iptables` rules for the Service `web` on a kind node and explain the probabilities `0.333…` and `0.5`. (b) Add a readiness probe that never passes to the Deployment and show that the new pod does not get into the endpoints while the old ones keep receiving traffic. Roll back.

**Expected result:** three `KUBE-SEP` rules with probabilities 1/3, 1/2 and no condition; the new pod `0/1` outside the endpoints.

<details><summary>Hint</summary>

`docker exec learn-control-plane iptables-save -t nat | grep "learn-06/web:http"`. The readiness probe: an `httpGet` on a path that does not exist (for example, `/ready`).

</details>

<details><summary>Solution</summary>

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

The first rule fires with probability 1/3; of the remaining 2/3 the second takes half (another 1/3); the third gets the rest (1/3): equal for each pod. The new pod is not ready, so its address is not in the endpoints, the rollout stopped, and the old pods keep accepting traffic.

</details>
