---
title: "Надёжность: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, mandatory, basic-return, persistence, at-least-once]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Надёжность: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md), [04 · Шаблоны обмена сообщениями](../04-messaging-patterns/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Все фрагменты запускались на настоящем RabbitMQ 4.4.1 в Docker с pika 1.5.0; выводы скопированы из запусков. В новом `pika.Connection` `confirm_delivery` принимает **обратный вызов**, а `basic_publish` ничего не возвращает: публикация асинхронна. Поэтому ниже используется небольшой помощник, который превращает вердикт брокера в возвращаемое значение:

```python
class Confirmed:
    """Publish and wait for the broker's verdict: 'ack', 'nack' or 'timeout'."""
    def __init__(self, ch):
        self.ch, self.events, self.verdict = ch, {}, {}
        self.returned, self.lock = [], threading.Lock()
        ch.confirm_delivery(self._on_confirm)
        ch.add_on_return_callback(lambda c, m, p, b: self.returned.append((m.reply_code, m.reply_text)))

    def _on_confirm(self, frame):
        m = frame.method
        kind = "ack" if isinstance(m, pika.spec.Basic.Ack) else "nack"
        with self.lock:
            tags = [t for t in self.events if t <= m.delivery_tag] if m.multiple else [m.delivery_tag]
            for t in tags:
                self.verdict[t] = kind
                if t in self.events: self.events[t].set()

    def publish(self, ex, rk, body, props=None, mandatory=False, timeout=5):
        tagbox, ev = [], threading.Event()
        def on_pub(tag):                       # runs on the IOLoop thread
            with self.lock: self.events[tag] = ev
            tagbox.append(tag)
        self.ch.basic_publish(ex, rk, body, props, mandatory, on_publish=on_pub)
        if not ev.wait(timeout): return "timeout"
        v = self.verdict.pop(tagbox[0]); del self.events[tagbox[0]]
        return v
```

`pub = Confirmed(ch)`; `ch` — канал основного соединения; очередь `rel` — durable.

## Где сообщение может потеряться

| Участок | Что защищает |
|---|---|
| издатель → брокер | publisher confirms |
| брокер, неподходящий маршрут | `mandatory` + Basic.Return |
| брокер → диск | durable-очередь + `delivery_mode=2` ([модуль 03](../03-queues-and-delivery/basics.ru.md#durable-и-persistent)) |
| брокер → потребитель | ручной ack после работы ([модуль 03](../03-queues-and-delivery/basics.ru.md#ack-nack-reject)) |
| потребитель обработал дважды | идемпотентность |

Надёжность — цепочка: слабое звено рвёт её целиком.

## Publisher confirms

После `confirm_delivery` брокер подтверждает каждую публикацию: `ack` — «принято» (для persistent-сообщения в durable-очереди — записано на диск), `nack` — «не смог».

```python
t = time.perf_counter()
print("confirmed publish:", pub.publish("", "rel", b"a", pika.BasicProperties(delivery_mode=2), mandatory=True),
      "ms:", round((time.perf_counter() - t) * 1000, 1))
```

```text
confirmed publish: ack ms: 46.4
```

Время зависит от машины и Docker; важно, что без подтверждения издатель не знает, дошло ли сообщение. Подтверждение — это «брокер взял ответственность», а не «потребитель обработал».

> ⚠️ Проверить: утверждение, что для persistent-сообщения в durable-очереди `ack` приходит после записи на диск, взято из документации и здесь не проверялось.

## Mandatory и Basic.Return

Если у сообщения нет маршрута, брокер по умолчанию молча его отбрасывает — **и всё равно подтверждает** публикацию. С `mandatory=True` он сначала вернёт сообщение (`Basic.Return`, код 312), а затем подтвердит:

```python
ch.exchange_declare("d", "direct")
r = pub.publish("d", "nokey", b"x", mandatory=True)
print("unroutable mandatory:", r, pub.returned)
pub.returned.clear()
r = pub.publish("d", "nokey", b"x")
print("unroutable no mandatory:", r, pub.returned)
```

```text
unroutable mandatory: ack [(312, 'NO_ROUTE')]
unroutable no mandatory: ack []
```

Поэтому `ack` не доказывает, что сообщение попало в очередь. Если маршрутизация важна, используйте `mandatory` и обработчик возвратов ([модуль 02](../02-exchanges-and-routing/basics.ru.md)).

## Что гарантирует «минимум один раз»

Confirms и ack потребителя вместе дают доставку **минимум один раз**: сообщение не потеряется, но может прийти дважды (издатель повторил публикацию, не получив подтверждения; потребитель упал после работы, но до ack). Защита от дубликатов — идемпотентный потребитель ([продвинутая теория](advanced.ru.md#идемпотентный-потребитель)).

## Типичные ошибки новичков

- ❌ Считать `basic_publish` без confirms надёжным → ✅ включить confirms.
- ❌ Считать `ack` подтверждением маршрута → ✅ `mandatory`.
- ❌ Persistent-сообщение в не-durable очереди (на 4.4 такую очередь не создать, если она не exclusive) → ✅ durable + persistent.
- ❌ Подтверждать потребителю до работы → ✅ после.
- ❌ Считать доставку «ровно один раз» → ✅ «минимум один раз» и идемпотентность.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — пропускная способность, nack, транзакции, идемпотентность
- Раньше: [04 · Шаблоны обмена сообщениями](../04-messaging-patterns/basics.ru.md), [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md)
- Следующий модуль: [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/overview.ru.md)

## Источники

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
