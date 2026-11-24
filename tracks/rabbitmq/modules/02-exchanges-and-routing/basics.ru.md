---
title: "Обменники и маршрутизация: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, routing-key, binding, topic-exchange, fanout, direct-exchange, headers-exchange]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Обменники и маршрутизация: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Обмен сообщениями и модель AMQP](../01-messaging-basics/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Всё запускалось на настоящем RabbitMQ 4.4.1 в Docker с pika 1.5.0; выводы скопированы из запусков. В экспериментах два общих помощника:

```python
import pika, time
conn = pika.Connection(pika.ConnectionParameters("localhost"))
ch = conn.channel()

def drain(q):                       # read everything from a queue
    time.sleep(0.3)                 # publishing is asynchronous
    out = []
    while True:
        m, p, b = ch.basic_get(q, auto_ack=True)
        if m is None:
            return out
        out.append(b.decode())

def setup(ex, typ, bindings):       # one exclusive queue per binding
    ch.exchange_declare(ex, exchange_type=typ)
    names = []
    for key in bindings:
        q = ch.queue_declare("", exclusive=True).method.queue
        if isinstance(key, str):
            ch.queue_bind(q, ex, routing_key=key)
        else:
            ch.queue_bind(q, ex, arguments=key)
        names.append(q)
    return names
```

## Что делает обменник

Производитель публикует **(обменник, ключ маршрутизации, сообщение)**. Обменник смотрит на свои привязки и копирует сообщение в каждую очередь, чья привязка подходит. Правило сопоставления — **тип** обменника. Сам обменник ничего не хранит: если ни одна очередь не подошла, сообщение исчезает.

## Обменник по умолчанию

Безымянный обменник `""` — это `direct`-обменник, который брокер привязывает к каждой очереди по её имени. Поэтому `basic_publish("", "work", …)` попадает в очередь `work`:

```python
ch.queue_declare("work", durable=True)
ch.basic_publish("", "work", b"to work")
print("default:", drain("work"))
```

```text
default: ['to work']
```

Объявить, удалить или привязать к нему вручную нельзя.

## Direct

Сообщение уходит в очереди, чей ключ привязки **равен** ключу маршрутизации.

```python
qs = setup("d", "direct", ["info", "error", "error"])
for k in ("info", "error", "warn"):
    ch.basic_publish("d", k, k.encode())
print("direct:", [drain(q) for q in qs])
```

```text
direct: [['info'], ['error'], ['error']]
```

Две очереди, привязанные с `error`, получили его обе; `warn` ни с чем не совпал и исчез.

## Fanout

Каждая привязанная очередь получает копию; ключ маршрутизации игнорируется.

```python
qs = setup("f", "fanout", ["a", "b"])
ch.basic_publish("f", "whatever", b"hi")
print("fanout:", [drain(q) for q in qs])
```

```text
fanout: [['hi'], ['hi']]
```

## Topic

Ключ маршрутизации — слова через точку. В ключе привязки `*` означает ровно одно слово, `#` — ноль или больше.

```python
pats = ["*.orange.*", "*.*.rabbit", "lazy.#", "#"]
qs = setup("t", "topic", pats)
for k in ["quick.orange.rabbit", "lazy.orange.elephant", "lazy", "quick.brown.fox", "lazy.pink.rabbit"]:
    ch.basic_publish("t", k, k.encode())
for p, q in zip(pats, qs):
    print("topic", p, "->", drain(q))
```

```text
topic *.orange.* -> ['quick.orange.rabbit', 'lazy.orange.elephant']
topic *.*.rabbit -> ['quick.orange.rabbit', 'lazy.pink.rabbit']
topic lazy.# -> ['lazy.orange.elephant', 'lazy', 'lazy.pink.rabbit']
topic # -> ['quick.orange.rabbit', 'lazy.orange.elephant', 'lazy', 'quick.brown.fox', 'lazy.pink.rabbit']
```

Обратите внимание: `lazy` (одно слово) подходит под `lazy.#`, а одиночный `#` подходит под всё (как fanout). Привязка без подстановочных знаков ведёт себя как `direct`.

## Headers

Маршрутизация смотрит на **заголовки** сообщения, игнорируя ключ маршрутизации. Привязка несёт таблицу заголовков и `x-match`: `all` требует совпадения всех заголовков, `any` — хотя бы одного.

```python
args = [{"x-match": "all", "format": "pdf", "type": "report"},
        {"x-match": "any", "format": "pdf", "type": "report"}]
qs = setup("h", "headers", args)
for hd in ({"format": "pdf", "type": "report"}, {"format": "pdf"}, {"format": "zip"}):
    ch.basic_publish("h", "", str(hd).encode(), pika.BasicProperties(headers=hd))
print("headers all:", drain(qs[0]), "any:", drain(qs[1]))
```

```text
headers all: ["{'format': 'pdf', 'type': 'report'}"] any: ["{'format': 'pdf', 'type': 'report'}", "{'format': 'pdf'}"]
```

## Выбор типа

| Нужно | Тип |
|---|---|
| Прямо в очередь по имени | обменник по умолчанию |
| Точная категория | `direct` |
| Всем | `fanout` |
| Иерархический шаблон | `topic` |
| Несколько атрибутов | `headers` |

Topic-обменники покрывают большинство реальных нужд; `direct` и `fanout` — их частные случаи.

## Типичные ошибки новичков

- ❌ Публиковать в именованный обменник, не привязав очередь → ✅ сначала привязать; иначе сообщение отбрасывается.
- ❌ Ждать, что `*` совпадёт с несколькими словами → ✅ `*` — одно слово, `#` — много.
- ❌ Фильтровать ключом на fanout-обменнике → ✅ он игнорируется; используйте `direct` или `topic`.
- ❌ Забыть `x-match` → ✅ явно задайте `all` или `any`.
- ❌ Объявлять обменник заново с другим типом → ✅ `406 PRECONDITION_FAILED`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — немаршрутизируемые сообщения, alternate exchange
- Раньше: [01 · Обмен сообщениями и модель AMQP](../01-messaging-basics/basics.ru.md)
- Следующий модуль: [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md)

## Источники

- [AMQP 0-9-1 Model Explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
- [Tutorial 4: Routing](https://www.rabbitmq.com/tutorials/tutorial-four-python)
- [Tutorial 5: Topics](https://www.rabbitmq.com/tutorials/tutorial-five-python)
