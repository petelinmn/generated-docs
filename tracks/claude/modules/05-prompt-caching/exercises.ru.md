---
title: "Упражнения: prompt caching"
lang: ru
track: claude
kind: module
doc: exercises
slug: 05-prompt-caching
part: 1
tags: [claude, prompt-caching, cache-ttl, cost-optimization, cache-prewarming]
status: draft
updated: 2026-11-24
---

# Упражнения: prompt caching

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Решения — локальные функции Python и запросы к мок-серверу (см. «Как проверялся код» в [базовой теории](basics.ru.md)); настоящий кэш сервиса не проверялся.

## 1. 🟢 Стоимость запроса по `usage`

**Задача:** функция `cost(usage, base=5.0, …)` считает стоимость входных токенов по полям `usage` (запись 5 минут — 1,25×, запись 1 час — 2×, чтение — 0,1×; цена в $ за млн токенов). Для записи 2048 токенов и 20 обычных, а затем чтения 2048 токенов и 20 обычных найдите стоимость и отношение «чтение / без кэша».

**Ожидаемый результат:** `0.0129 0.001124 0.01034 0.109`.

<details><summary>Подсказка</summary>

`per = base / 1_000_000`; запись по TTL берите из `usage.cache_creation`.

</details>

<details><summary>Решение</summary>

См. функцию `cost` и вывод в [базовой теории](basics.ru.md#сколько-это-стоит):

```python
def cost(usage, base=5.0, mult_write5=1.25, mult_write1h=2.0, mult_read=0.1):
    per = base / 1_000_000
    cc = usage.cache_creation
    w5 = cc.ephemeral_5m_input_tokens if cc else usage.cache_creation_input_tokens
    w1 = cc.ephemeral_1h_input_tokens if cc else 0
    return per * (usage.input_tokens + w5 * mult_write5 + w1 * mult_write1h + usage.cache_read_input_tokens * mult_read)
```

```text
0.0129 0.001124 0.01034 0.109
```

</details>

## 2. 🟢 Проверка порядка TTL

**Задача:** функция `valid_ttl_order(blocks)` принимает список значений `"5m"`/`"1h"` в порядке следования точек и возвращает `False`, если 1-часовая точка стоит **после** 5-минутной.

**Ожидаемый результат:** `True False True` для `["1h","5m"]`, `["5m","1h"]`, `["1h","1h","5m"]`.

<details><summary>Подсказка</summary>

Запомните, встречалась ли уже 5-минутная точка.

</details>

<details><summary>Решение</summary>

```python
def valid_ttl_order(blocks):
    seen_5m = False
    for ttl in blocks:
        if ttl == "5m": seen_5m = True
        elif ttl == "1h" and seen_5m: return False
    return True
print(valid_ttl_order(["1h", "5m"]), valid_ttl_order(["5m", "1h"]), valid_ttl_order(["1h", "1h", "5m"]))
```

```text
True False True
```

</details>

## 3. 🟡 Когда кэш окупается

**Задача:** общая стоимость `n` запросов с одним и тем же префиксом в долях стоимости одного обычного входа: `write + read × (n − 1)` против `n` без кэша. Найдите минимальное `n`, при котором кэш выгоден, для записи на 5 минут (1,25×) и на 1 час (2×), если чтение — 0,1×.

**Ожидаемый результат:** `2 3`.

<details><summary>Подсказка</summary>

Переберите `n` от 1 и сравните `total(n) < n`.

</details>

<details><summary>Решение</summary>

```python
def total(n, write=1.25, read=0.1): return write + read * (n - 1)
print(min(n for n in range(1, 50) if total(n) < n), min(n for n in range(1, 50) if total(n, 2.0) < n))
```

```text
2 3
```

Для 5-минутной записи кэш выгоден со второго запроса (`1,35 < 2`), для часовой — с третьего (`2,2 < 3`). Расчёт упрощён: он считает только префикс и не учитывает переменную часть и выход.

</details>

## 4. 🟡 Стабильный префикс

**Задача:** покажите на отпечатках (`sha256`), что `system` с меняющейся секундой даёт разные префиксы, а без неё — одинаковые; а JSON с разным порядком ключей совпадает только после `sort_keys=True`.

**Ожидаемый результат:** `False True` для отпечатков и `False True` для JSON.

<details><summary>Подсказка</summary>

`hashlib.sha256(s.encode()).hexdigest()[:8]`; `json.dumps(obj, sort_keys=True)`.

</details>

<details><summary>Решение</summary>

```python
import hashlib, json, datetime
def fp(s): return hashlib.sha256(s.encode()).hexdigest()[:8]
def system_bad(rules, sec): return f"Now: 2026-11-24T10:00:{sec:02d}\n{rules}"
def system_good(rules): return rules
print(fp(system_bad("R", 1)) == fp(system_bad("R", 2)), fp(system_good("R")) == fp(system_good("R")))
print(json.dumps({"b": 1, "a": 2}) == json.dumps({"a": 2, "b": 1}), json.dumps({"b": 1, "a": 2}, sort_keys=True) == json.dumps({"a": 2, "b": 1}, sort_keys=True))
```

```text
False True
False True
```

</details>

## 5. 🔴 Запрос прогрева

**Задача:** отправьте запрос с `max_tokens=0`, автоматическим `cache_control` и длинным `system`; убедитесь по ответу, что `content` пуст, `stop_reason` равен `max_tokens`, а записано 2048 токенов (ответ мока).

**Ожидаемый результат:** `[] max_tokens 0 2048` (содержимое, причина остановки, выходные токены, записанные токены).

<details><summary>Подсказка</summary>

`cache_control={"type": "ephemeral"}` на верхнем уровне; `r.usage.cache_creation_input_tokens`.

</details>

<details><summary>Решение</summary>

```python
r = client.messages.create(model="claude-opus-5-5", max_tokens=0, cache_control={"type": "ephemeral"},
                           system=BIG_SYSTEM, messages=[{"role": "user", "content": "x"}])
print(r.content, r.stop_reason, r.usage.output_tokens, r.usage.cache_creation_input_tokens)
```

```text
[] max_tokens 0 2048
```

На настоящем API прогрев имеет смысл только с теми же настройками мышления и `effort`, что у рабочих запросов, и без `stream: true`.

</details>
