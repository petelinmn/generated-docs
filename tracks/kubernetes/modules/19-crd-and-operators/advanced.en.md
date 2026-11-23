---
title: "CRDs and operators: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 19-crd-and-operators
part: 3
tags: [cel, x-kubernetes-validations, pruning, status-subresource, owner-reference, operator, rbac, cert-manager, reconcile]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, alpine/k8s 1.35.0, cert-manager v1.21.3, Helm 4.3"
status: draft
updated: 2026-11-23
---

# CRDs and operators: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). Experiments were run in kind, namespace `learn-19`.

## CEL rules

For checks that tie several fields together, add `x-kubernetes-validations` with a CEL expression to the schema ([module 17](../17-security/advanced.en.md#validatingadmissionpolicy-your-own-rules) uses the same language):

```yaml
spec:
  type: object
  properties: {…}
  x-kubernetes-validations:
  - rule: "self.tier != 'free' || self.replicas <= 2"
    message: "free tier allows at most 2 replicas"
```

A request with `tier: free, replicas: 4` was rejected with the rule's text:

```text
The Widget "bad" is invalid: spec: Invalid value: free tier allows at most 2 replicas
```

But such a rule has a catch. The `tier` field is optional, and when it is absent the rule fails by itself:

```text
spec.replicas: Invalid value: 9: spec.replicas in body should be less than or equal to 5
spec: Invalid value: "object": no such key: tier evaluating rule: free tier allows at most 2 replicas
```

And for an existing object without `tier` even a harmless `patch` with `replicas: 3` was rejected with the same error `no such key: tier`. The rule must check for the field with `has()`:

```yaml
- rule: "!has(self.tier) || self.tier != 'free' || self.replicas <= 2"
```

After replacing the rule (`kubectl patch crd …`) the same change `replicas: 3` passed, and `tier: free, replicas: 4` was still rejected.

## Pruning: unknown fields

A field that is not in the schema must not reach storage. What happens:

```text
$ kubectl apply -f w2.yaml        # spec: {image: nginx, color: red}
Error from server (BadRequest): … strict decoding error: unknown field "spec.color"

$ kubectl apply --validate=false -f w2.yaml
widget.example.com/w2 created
$ kubectl get widget w2 -o jsonpath='{.spec}'
{"image":"nginx","replicas":1}
```

`kubectl` validates strictly by default. If you turn the validation off (`--validate=false`), the server still **drops** the unknown field `color` (pruning), and says nothing about it: the data simply disappears. Add the field you need to the schema.

## The status subresource

With `subresources.status` enabled an ordinary change to the resource does not touch `status`:

```text
$ kubectl -n learn-19 patch widget w1 --type=merge -p '{"status":{"phase":"Ready"}}'
widget.example.com/w1 patched (no change)
$ kubectl -n learn-19 patch widget w1 --subresource=status --type=merge -p '{"status":{"phase":"Ready"}}'
widget.example.com/w1 patched
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
w1     1          Ready
```

The split exists so that the user (who writes `spec`) and the controller (who writes `status`) do not get in each other's way, and so that permissions can be granted separately: in RBAC it is the resource `widgets/status`.

## ownerReferences and garbage collection

A controller marks the objects it creates with an owner. Experiment: a ConfigMap `owned` with `ownerReferences` pointing to `Widget w1`:

```yaml
metadata:
  name: owned
  ownerReferences:
  - {apiVersion: example.com/v1, kind: Widget, name: w1, uid: <the widget uid>, controller: true, blockOwnerDeletion: true}
```

```text
$ kubectl -n learn-19 delete widget w1
$ kubectl -n learn-19 get cm owned
Error from server (NotFound): configmaps "owned" not found
```

After the owner was deleted, the garbage collector deleted the dependent object. This is how operators clean up everything they created without separate deletion code.

## A toy operator

To see the reconcile loop, run the simplest operator: an `sh` script in the image `alpine/k8s:1.35.0` (it has `kubectl` and `jq`). Every 3 seconds it reads all `Widget`s and for each one creates or updates a Deployment `widget-<name>` with `ownerReferences`, and writes `Ready` into `status.phase` when the replicas are available.

```sh
while true; do
  kubectl get widgets.example.com -A -o json | jq -c '.items[]' | while read -r w; do
    # …extract ns, name, uid, image, replicas…
    kubectl apply -f - <<DEP      # Deployment widget-$name with ownerReferences to the widget
    …
DEP
    kubectl -n "$ns" patch widget "$name" --subresource=status --type=merge -p "{\"status\":{\"phase\":\"$phase\"}}"
  done
  sleep 3
done
```

The script lives in the ConfigMap `op-script`, is run by the Deployment `operator` with the ServiceAccount `widget-operator` and this ClusterRole:

```yaml
rules:
- {apiGroups: [example.com], resources: [widgets], verbs: [get, list, watch]}
- {apiGroups: [example.com], resources: [widgets/status], verbs: [get, patch, update]}
- {apiGroups: [apps], resources: [deployments], verbs: [get, list, create, patch, update]}
```

In the first version the rule for `widgets/status` had no `get` verb, and the operator got:

```text
Error from server (Forbidden): widgets.example.com "shop" is forbidden: User "system:serviceaccount:learn-19:widget-operator" cannot get resource "widgets/status" in API group "example.com" in the namespace "learn-19"
```

`kubectl patch --subresource=status` reads the object before changing it, hence `get` is needed too. An operator written with a client library usually gets by with `patch`/`update`.

How the operator works:

```text
$ kubectl -n learn-19 apply -f shop.yaml          # spec: {image: nginx:1.29, replicas: 2}
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
shop   2          Ready
$ kubectl -n learn-19 get deploy widget-shop
NAME          READY   UP-TO-DATE   AVAILABLE
widget-shop   2/2     2            2
$ kubectl -n learn-19 patch widget shop --type=merge -p '{"spec":{"replicas":3}}'
$ kubectl -n learn-19 get wg shop --no-headers
shop   3     Ready
$ kubectl -n learn-19 scale deploy widget-shop --replicas=1     # "drift"
… after 6 seconds …
widget-shop   3/3   3     3
```

- A change to `spec` led to a change in the Deployment.
- The manual `scale` was undone by the operator at the next reconcile: it brings the world to the desired state rather than "reacting to an event".
- After `kubectl delete widget shop` the Deployment vanished (garbage collection).

This is **polling**, not watching: real controllers use watches and queues (see the [expert theory](expert.en.md#internals)), but the logic is the same: an idempotent reconcile.

## A real operator: cert-manager

cert-manager automates certificate issuance. Installation with a Helm chart (`crds.enabled=true` installs the CRDs together with the chart):

```text
$ helm repo add jetstack https://charts.jetstack.io
$ helm install cm jetstack/cert-manager -n cert-manager --create-namespace --set crds.enabled=true --wait
STATUS: deployed
$ kubectl get crd | grep cert-manager
certificaterequests.cert-manager.io  certificates.cert-manager.io  challenges.acme.cert-manager.io
clusterissuers.cert-manager.io       issuers.cert-manager.io       orders.acme.cert-manager.io
$ kubectl -n cert-manager get pods
cm-cert-manager-…                1/1   Running
cm-cert-manager-cainjector-…     1/1   Running
cm-cert-manager-webhook-…        1/1   Running
```

The objects:

```yaml
apiVersion: cert-manager.io/v1
kind: Issuer
metadata: {name: selfsigned}
spec: {selfSigned: {}}
---
apiVersion: cert-manager.io/v1
kind: Certificate
metadata: {name: demo}
spec:
  secretName: demo-tls
  commonName: demo.example.test
  dnsNames: [demo.example.test]
  issuerRef: {name: selfsigned, kind: Issuer}
```

```text
$ kubectl -n learn-19 get issuer,certificate
issuer.cert-manager.io/selfsigned   True
certificate.cert-manager.io/demo    True   demo-tls
$ kubectl -n learn-19 get certificaterequest
demo-1   True (APPROVED)   True (READY)   selfsigned
$ kubectl -n learn-19 get secret demo-tls -o jsonpath='{.type}'
kubernetes.io/tls
$ … tls.crt | openssl x509 -noout -subject -issuer -dates
subject=CN=demo.example.test
issuer=CN=demo.example.test
```

The chain of events: `Issuing` (the Secret does not exist) → `Generated` (a temporary private key) → a `CertificateRequest` is created and approved → `CertificateIssued` → `Issuing: The certificate has been successfully issued`. The owner of the `CertificateRequest` is the `Certificate`.

The operator maintains the desired state: after `kubectl delete secret demo-tls` the Secret reappeared about 8 seconds later.

> ⚠️ Verify: CRD versioning (`v1alpha1` → `v1`) and webhook conversion were not run; the cert-manager `Certificate` has only version `v1` in the experiment (`storage=v1`).

## Best practices

- A strict schema: `required`, `enum`, bounds, `default`; do not leave arbitrary fields.
- CEL rules with `has()` for optional fields.
- The controller writes only `status`, the user writes `spec`.
- Reconcile is idempotent: a re-run does not break the result.
- `ownerReferences` on everything a controller creates.
- Minimal RBAC: its own resources, `status`, managed objects.
- For ready-made tasks (certificates, databases) use an existing operator rather than writing your own.

## Common mistakes

- ❌ A CEL rule without `has()` for an optional field → ✅ `!has(self.x) || …`.
- ❌ Writing `status` with an ordinary `patch` → ✅ `--subresource=status`/a client using `/status`.
- ❌ Not granting rights on `<resource>/status` → ✅ add them to the ClusterRole.
- ❌ An unknown field "disappeared" → ✅ add it to the schema.
- ❌ A controller without `ownerReferences` → ✅ garbage is left after the object is deleted.

## Related materials

- Next: [expert theory](expert.en.md)
- RBAC and ServiceAccount: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/advanced.en.md)
- CEL in admission: [17 · Security](../17-security/advanced.en.md#validatingadmissionpolicy-your-own-rules)
- Helm: [13 · Helm](../13-helm/basics.en.md)
- Certificates and TLS in Ingress: [09 · Ingress and Gateway API](../09-ingress-and-gateway-api/overview.en.md)

## Sources

- [CRD: validation rules](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definitions/#validation-rules)
- [CRD: defaulting and pruning](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definitions/#defaulting-and-nullable)
- [Garbage Collection](https://kubernetes.io/docs/concepts/architecture/garbage-collection/)
- [cert-manager: Helm installation](https://cert-manager.io/docs/installation/helm/)
- [cert-manager: SelfSigned issuer](https://cert-manager.io/docs/configuration/selfsigned/)
