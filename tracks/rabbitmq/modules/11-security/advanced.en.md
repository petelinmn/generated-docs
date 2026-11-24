---
title: "Security: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 11-security
part: 3
tags: [rabbitmq, security, tls, certificates, openssl, ssl-options, change-password]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Security: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on RabbitMQ 4.4.1 (Docker) with pika 1.5.0 and Python with OpenSSL 3.5. The certificates below are **disposable teaching ones** (valid 2 days); for a production system use your own certificate authority.

## Why TLS

Without TLS the password and messages cross the network in plain text. TLS encrypts the connection and lets the client verify it is talking to the real broker.

## Teaching certificates

A certificate authority (CA) and a server certificate for `localhost`. Verifying clients (Python included) need extensions: on the CA `basicConstraints` and `keyUsage`, on the server `subjectAltName`, `keyUsage`, `extendedKeyUsage`.

```bash
openssl req -x509 -newkey rsa:2048 -nodes -keyout ca.key -out ca.pem -days 2 \
  -subj "/CN=TestCA" -addext "basicConstraints=critical,CA:TRUE" \
  -addext "keyUsage=critical,keyCertSign,cRLSign" -addext "subjectKeyIdentifier=hash"
openssl req -newkey rsa:2048 -nodes -keyout server.key -out server.csr -subj "/CN=localhost"
printf "subjectAltName=DNS:localhost,IP:127.0.0.1\nkeyUsage=critical,digitalSignature,keyEncipherment\nextendedKeyUsage=serverAuth\nauthorityKeyIdentifier=keyid\nsubjectKeyIdentifier=hash\n" > ext.cnf
openssl x509 -req -in server.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out server.pem -days 2 -extfile ext.cnf
```

> Python rejected the first CA, which lacked the `keyUsage` extension: `CA cert does not include key usage extension`. Strict certificate verification in modern Python requires these extensions; do not weaken it (`VERIFY_X509_STRICT`) — fix the certificate.

## Broker configuration

The file `/etc/rabbitmq/conf.d/20-tls.conf`:

```ini
listeners.ssl.default = 5671
ssl_options.cacertfile = /certs/ca.pem
ssl_options.certfile   = /certs/server.pem
ssl_options.keyfile    = /certs/server.key
ssl_options.verify     = verify_none
ssl_options.fail_if_no_peer_cert = false
```

The certificates were put into the container with `docker cp`. Checking the listeners:

```text
Interface: [::], port: 5672, protocol: amqp, purpose: AMQP 0-9-1 and AMQP 1.0
Interface: [::], port: 5671, protocol: amqp/ssl, purpose: AMQP 0-9-1 and AMQP 1.0 over TLS
```

`verify_none` means the broker does not demand a certificate from the client (the client verifies the broker, the broker does not verify the client).

## The client

```python
ctx = ssl.create_default_context(cafile="tls/ca.pem")
c = pika.Connection(pika.ConnectionParameters(
    "localhost", 5671, ssl_options=pika.SSLOptions(ctx, "localhost")))
```

```text
TLS with CA verification: ok, open: True
unknown CA -> AMQPConnectionWorkflowFailed
wrong hostname -> AMQPConnectionWorkflowFailed
plain AMQP to TLS port -> IncompatibleProtocolError StreamLostError: ('Transport indicated EOF',) (host='localhost', port=5671)
```

- With verification against our CA and the right host name the connection opened.
- A context without our CA (system authorities only) rejected the broker's certificate.
- A wrong host name (`wrong.example`) was rejected: the name is checked against `subjectAltName`.
- Plain AMQP to the TLS port does not work.

Never disable verification (`ssl._create_unverified_context`) in a production system: encryption without verification does not protect against server impersonation.

> ⚠️ Verify: mutual authentication (`verify_peer`, `fail_if_no_peer_cert = true`, logging in with a client certificate), TLS versions and cipher suites, TLS for the management plugin (port 15671) and for inter-node traffic were not verified here.

## Changing a password

```text
$ rabbitmqctl change_password bob newpw
bob old password -> ProbableAuthenticationError ... (403) 'ACCESS_REFUSED ...
bob new password -> ok
```

After a password change the old one stops working for new connections.

> ⚠️ Verify: what happens to a user's already open connections after a password change or deletion was not verified here.

## Best practices

- TLS on client and inter-node connections and on management.
- Verify the certificate and the host name in the client.
- A separate user and vhost per application; minimal regular expressions.
- Delete `guest`; secrets (passwords, keys) stay out of code and the repository.
- Certificates with a limited lifetime and automatic renewal.
- Restrict the broker's ports by network: management and the inter-node port do not belong on the internet.

## Common mistakes

- ❌ Disabling certificate verification in the client → ✅ plug in your own CA.
- ❌ A CA without `keyUsage` → ✅ the required extensions.
- ❌ Keeping a key in the repository → ✅ a secret store.
- ❌ Exposing port 5672 to the internet → ✅ TLS 5671 and network restrictions.
- ❌ Believing TLS replaces permissions → ✅ you need both.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [09 · Monitoring](../09-monitoring/advanced.en.md) — the management and Prometheus ports
- Next module: [12 · Federation and Shovel](../12-federation-and-shovel/overview.en.md)

## Sources

- [TLS Support](https://www.rabbitmq.com/docs/ssl)
- [Authentication, Authorisation, Access Control](https://www.rabbitmq.com/docs/access-control)
- [Troubleshooting TLS](https://www.rabbitmq.com/docs/troubleshooting-ssl)
