---
title: "Безопасность: продвинутый уровень"
lang: ru
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

# Безопасность: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на RabbitMQ 4.4.1 (Docker) с pika 1.5.0 и Python с OpenSSL 3.5. Сертификаты ниже — **учебные одноразовые** (срок 2 дня); для рабочей системы используйте свой центр сертификации.

## Зачем TLS

Без TLS пароль и сообщения идут по сети открытым текстом. TLS шифрует соединение и позволяет клиенту проверить, что он говорит с настоящим брокером.

## Учебные сертификаты

Центр сертификации (CA) и сертификат сервера для `localhost`. Проверяющим клиентам (в том числе Python) нужны расширения: у CA — `basicConstraints` и `keyUsage`, у сервера — `subjectAltName`, `keyUsage`, `extendedKeyUsage`.

```bash
openssl req -x509 -newkey rsa:2048 -nodes -keyout ca.key -out ca.pem -days 2 \
  -subj "/CN=TestCA" -addext "basicConstraints=critical,CA:TRUE" \
  -addext "keyUsage=critical,keyCertSign,cRLSign" -addext "subjectKeyIdentifier=hash"
openssl req -newkey rsa:2048 -nodes -keyout server.key -out server.csr -subj "/CN=localhost"
printf "subjectAltName=DNS:localhost,IP:127.0.0.1\nkeyUsage=critical,digitalSignature,keyEncipherment\nextendedKeyUsage=serverAuth\nauthorityKeyIdentifier=keyid\nsubjectKeyIdentifier=hash\n" > ext.cnf
openssl x509 -req -in server.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out server.pem -days 2 -extfile ext.cnf
```

> Первый вариант CA без расширения `keyUsage` Python отверг: `CA cert does not include key usage extension`. Строгая проверка сертификатов в современном Python требует этих расширений; ослаблять её (`VERIFY_X509_STRICT`) не нужно — исправьте сертификат.

## Настройка брокера

Файл `/etc/rabbitmq/conf.d/20-tls.conf`:

```ini
listeners.ssl.default = 5671
ssl_options.cacertfile = /certs/ca.pem
ssl_options.certfile   = /certs/server.pem
ssl_options.keyfile    = /certs/server.key
ssl_options.verify     = verify_none
ssl_options.fail_if_no_peer_cert = false
```

Сертификаты положены в контейнер `docker cp`. Проверка слушателей:

```text
Interface: [::], port: 5672, protocol: amqp, purpose: AMQP 0-9-1 and AMQP 1.0
Interface: [::], port: 5671, protocol: amqp/ssl, purpose: AMQP 0-9-1 and AMQP 1.0 over TLS
```

`verify_none` означает, что брокер не требует сертификат от клиента (клиент проверяет брокера, а брокер клиента — нет).

## Клиент

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

- С проверкой по нашему CA и верным именем хоста соединение открылось.
- Контекст без нашего CA (только системные центры) отклонил сертификат брокера.
- Неверное имя хоста (`wrong.example`) отклонено: имя проверяется по `subjectAltName`.
- Обычный AMQP на TLS-порт не работает.

Никогда не отключайте проверку (`ssl._create_unverified_context`) в рабочей системе: шифрование без проверки не защищает от подмены сервера.

> ⚠️ Проверить: взаимная аутентификация (`verify_peer`, `fail_if_no_peer_cert = true`, вход по сертификату клиента), версии и наборы шифров TLS, TLS для плагина управления (порт 15671) и для межузлового обмена здесь не проверялись.

## Смена пароля

```text
$ rabbitmqctl change_password bob newpw
bob old password -> ProbableAuthenticationError ... (403) 'ACCESS_REFUSED ...
bob new password -> ok
```

После смены пароля старый перестаёт работать для новых соединений.

> ⚠️ Проверить: что происходит с уже открытыми соединениями пользователя после смены пароля или удаления, здесь не проверялось.

## Лучшие практики

- TLS на клиентских и межузловых соединениях и на управлении.
- Проверка сертификата и имени хоста в клиенте.
- Отдельный пользователь и vhost на приложение; минимальные регулярные выражения.
- Удалить `guest`; секреты (пароли, ключи) — не в коде и не в репозитории.
- Сертификаты с ограниченным сроком и автоматическим обновлением.
- Закрыть порты брокера сетью: управление и межузловой порт — не в интернет.

## Частые ошибки

- ❌ Отключать проверку сертификата в клиенте → ✅ подключить свой CA.
- ❌ CA без `keyUsage` → ✅ нужные расширения.
- ❌ Хранить ключ в репозитории → ✅ секретное хранилище.
- ❌ Открывать порт 5672 в интернет → ✅ TLS 5671 и сетевые ограничения.
- ❌ Считать, что TLS заменяет права → ✅ нужны оба.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [09 · Мониторинг](../09-monitoring/advanced.ru.md) — порт управления и Prometheus
- Следующий модуль: [12 · Federation и Shovel](../12-federation-and-shovel/overview.ru.md)

## Источники

- [TLS Support](https://www.rabbitmq.com/docs/ssl)
- [Authentication, Authorisation, Access Control](https://www.rabbitmq.com/docs/access-control)
- [Troubleshooting TLS](https://www.rabbitmq.com/docs/troubleshooting-ssl)
