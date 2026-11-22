---
title: "Service and networking: advanced"
lang: en
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

# Service and networking: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): the ClusterIP Service manifest, endpoints and DNS are not repeated.

## Service types

| Type | What it does |
|---|---|
| `ClusterIP` | a virtual address inside the cluster (default) |
| `NodePort` | a ClusterIP plus a port on every node (30000–32767) |
| `LoadBalancer` | a NodePort plus an external balancer that the cloud creates |
| `ExternalName` | a DNS alias (CNAME) to an external name, no proxy |
| headless (`clusterIP: None`) | no virtual address: DNS returns pod addresses |

The other four variants (besides the plain ClusterIP from [basics](basics.en.md#the-service-manifest)) are created from one file:

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

`80:30080/TCP` means: Service port 80, node port 30080. A request to the IP of any node and port 30080 reaches the pods. In kind a node is a Docker container, so we check from inside it:

```text
$ docker exec learn-control-plane curl -s -m 3 http://172.18.0.9:30080
hello from web-8678fd4948-5p9dp
```

(`172.18.0.9` is the node address from `kubectl get node -o wide`.) The NodePort range is 30000–32767; the API rejects another value:

```text
The Service "bad-np" is invalid: spec.ports[0].nodePort: Invalid value: 20000: provided port is not in the valid range. The range of valid ports is 30000-32767
```

From your machine a kind node's port is not visible by default: Docker does not forward it to the host. To reach a NodePort from the host, set `extraPortMappings` in the kind configuration.

> ⚠️ Verify: forwarding a NodePort to the host through `extraPortMappings` in kind was not run in this material ([kind documentation](https://kind.sigs.k8s.io/docs/user/configuration/#extra-port-mappings)).

### LoadBalancer

A `LoadBalancer` Service also creates a NodePort (`31708` in the output), and the external address is issued by a cloud provider. Our kind has no cloud controller, so `EXTERNAL-IP` stays `<pending>` and the service is reachable only as a NodePort. For local experiments with LoadBalancer there are separate tools (for example, MetalLB or cloud-provider-kind).

> ⚠️ Verify: MetalLB and cloud-provider-kind working with kind were not checked in this material.

### Headless Service

Without a `clusterIP`, DNS returns the pod addresses directly, one record per pod:

```text
$ kubectl -n learn-06 exec client2 -- nslookup web-headless
Name:	web-headless.learn-06.svc.cluster.local
Address: 10.244.0.33
Name:	web-headless.learn-06.svc.cluster.local
Address: 10.244.0.34
Name:	web-headless.learn-06.svc.cluster.local
Address: 10.244.0.35
```

This is needed when a client picks a pod itself or must know all of them (databases, StatefulSet, module 12). A headless service has no kube-proxy balancing.

### ExternalName

`ExternalName` does not proxy but returns a CNAME:

```text
$ kubectl -n learn-06 exec client2 -- nslookup ext
ext.learn-06.svc.cluster.local	canonical name = example.com
Name:	example.com
```

This way an external service gets an internal name; after a move you change only `externalName`.

## Pod readiness and endpoints

Only ready pods get into an EndpointSlice. Let us check: add to the Deployment a readiness probe that never passes (`/ready` returns 404) and see what happens during the rollout:

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

The new pod is running but not ready (`0/1`), so its address is not among the endpoints: traffic goes only to the three old ones. The rollout itself also stopped on it, and the old pods stayed ([module 05](../05-deployments/advanced.en.md#a-stuck-rollout)). A readiness probe protects users both from calls to a not-ready pod and from a bad release. A rollback (`rollout undo`) brought back the previous version.

## Typical failures

**A typo in the selector.** A Service with `selector: {app: wbe}` finds no pods:

```text
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-typo -o custom-columns=NAME:.metadata.name,ENDPOINTS:.endpoints
NAME             ENDPOINTS
web-typo-6vqrs   <nil>
$ kubectl -n learn-06 exec client2 -- wget -qO- -T 3 http://web-typo
wget: can't connect to remote host (10.96.75.1): Connection refused
```

**A wrong targetPort.** A Service with `targetPort: 8081` while the container listens on 80: endpoints exist, but the address leads to port 8081:

```text
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web-badport -o jsonpath='{.items[0].endpoints[0].addresses} port={.items[0].ports[0].port}'
["10.244.0.33"] port=8081
$ kubectl -n learn-06 exec client2 -- wget -qO- -T 3 http://web-badport
wget: can't connect to remote host (10.96.133.143): Connection refused
```

In both cases the error is one, `Connection refused`, and the cause differs. Diagnosis: `kubectl get endpoints <svc>`, then compare the `selector` with pod labels (`kubectl get pods --show-labels`) and `targetPort` with the container port.

## Service environment variables

For every Service that exists at the moment a pod is created, the kubelet puts variables into the container:

```text
$ kubectl -n learn-06 exec client2 -- env | grep -E "^WEB_SERVICE|^WEB_PORT="
WEB_SERVICE_HOST=10.96.37.242
WEB_PORT=tcp://10.96.37.242:80
WEB_SERVICE_PORT=80
WEB_SERVICE_PORT_HTTP=80
```

They appear only if the Service was created before the pod; DNS is more reliable. The variables can be disabled with the pod field `enableServiceLinks: false`.

## How kube-proxy balances traffic

On every node kube-proxy (the `iptables` mode) writes rules. A request to a ClusterIP lands in the service chain, which picks one of the pods randomly with set probabilities, and then DNAT goes to the pod's IP:

```text
-A KUBE-SERVICES -d 10.96.37.242/32 -p tcp -m comment --comment "learn-06/web:http cluster IP" -m tcp --dport 80 -j KUBE-SVC-LLGEISXCDCZSME5V
-A KUBE-SVC-LLGEISXCDCZSME5V … --comment "learn-06/web:http -> 10.244.0.39:80" -m statistic --mode random --probability 0.33333333349 -j KUBE-SEP-QT7MGDW4DUHBYCOA
-A KUBE-SVC-LLGEISXCDCZSME5V … --comment "learn-06/web:http -> 10.244.0.40:80" -m statistic --mode random --probability 0.50000000000 -j KUBE-SEP-VB3DA5Y2GNNMKSMP
-A KUBE-SVC-LLGEISXCDCZSME5V … --comment "learn-06/web:http -> 10.244.0.41:80" -j KUBE-SEP-JLZCLXTREU45VXC7
-A KUBE-SEP-JLZCLXTREU45VXC7 -p tcp -m comment --comment "learn-06/web:http" -m tcp -j DNAT --to-destination 10.244.0.41:80
```

The probabilities `1/3`, then `1/2` of the remainder and the last rule without a condition give each of the three pods one third. The rules were obtained with `iptables-save -t nat` inside the node. A ClusterIP belongs to no interface, so ping to it gets no reply:

```text
$ kubectl -n learn-06 exec client2 -- ping -c 1 -W 2 10.96.37.242
1 packets transmitted, 0 packets received, 100% packet loss
```

## Best practices

- Call by the Service name, not by IP; between namespaces — `service.namespace`.
- Name the ports and refer to them in `targetPort`: a port number then changes in one place only.
- Always add a readiness probe: otherwise the Service sends traffic to a not-yet-warmed pod.
- To expose outside — an Ingress or the Gateway API (module 09), not `NodePort` and many `LoadBalancer`s.
- Check `get endpoints`/`endpointslices` for any network problem.

## Common mistakes

- ❌ Thinking a Service is "broken" on `Connection refused` → ✅ first `get endpoints`: empty or a wrong port.
- ❌ Exposing a database through `LoadBalancer` → ✅ a ClusterIP and network policies (module 17).
- ❌ Relying on Service environment variables → ✅ use DNS.
- ❌ Expecting even round-robin on a small number of requests → ✅ the choice is random.

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): a ClusterIP is iptables rules and DNAT; the address itself is assigned to no interface.
- [C#](../../../csharp/recipes/http-client-resilience/overview.en.md): a client should retry short-lived connection errors during a rollout and use the Service name.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Previous module: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- Pod termination and endpoints: [04 · Pod](../04-pods/advanced.en.md#pod-termination)

## Sources

- [Service: types](https://kubernetes.io/docs/concepts/services-networking/service/#publishing-services-service-types)
- [Headless Services](https://kubernetes.io/docs/concepts/services-networking/service/#headless-services)
- [ExternalName](https://kubernetes.io/docs/concepts/services-networking/service/#externalname)
- [Virtual IPs and Service Proxies](https://kubernetes.io/docs/reference/networking/virtual-ips/)
- [kind: Extra Port Mappings](https://kind.sigs.k8s.io/docs/user/configuration/#extra-port-mappings)
- [Debug Services](https://kubernetes.io/docs/tasks/debug/debug-application/debug-service/)
