---
title: "Prompt caching: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 05-prompt-caching
part: 1
tags: [claude, prompt-caching, cache-control, usage, cost-optimization]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Prompt caching: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../02-messages-api/overview.ru.md) · **Время:** ~1 ч

## Как проверялся код

Ключа API при подготовке не было, поэтому код запускался против **локального мок-сервера** (как в [модуле 02](../02-messages-api/basics.ru.md)): он принимает запросы SDK `anthropic` 2.13.0, сохраняет тело и возвращает заранее заданное поле `usage` (с записью или чтением кэша). Проверены форма запросов, разбор `usage` и расчёты; **настоящее кэширование на стороне сервиса, попадания и промахи на реальных моделях не проверялись**. Правила и цены взяты со страницы документации «Prompt caching».

## Зачем это нужно

Если каждый запрос начинается с одного и того же большого текста (инструкции, справочник, документ, описание инструментов), его можно обрабатывать один раз, а потом дёшево читать из кэша. Выигрыш — в стоимости и задержке повторных запросов.

## Как работает префикс

Кэшируется **префикс** запроса: всё до блока с `cache_control` включительно, в порядке `tools` → `system` → `messages`. Попадание возможно только при **полном совпадении** префикса (весь текст и изображения). Это значит:

- кэш помогает, когда **общая часть стоит в начале**, а меняющаяся — в конце;
- любое изменение внутри префикса делает недействительным всё, что после него;
- кэширование не меняет результат ответа, только стоимость и скорость.

## Автоматическое кэширование

Простейший способ — `cache_control` на верхнем уровне запроса: точка ставится на последний кэшируемый блок и сама сдвигается по мере роста диалога.

```python
r = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    cache_control={"type": "ephemeral"},
    system=BIG,  # длинный неизменный текст
    messages=[{"role": "user", "content": "Summarize"}],
)
u = r.usage
print(u.input_tokens, u.cache_creation_input_tokens, u.cache_read_input_tokens, u.cache_creation.ephemeral_5m_input_tokens, u.cache_creation.ephemeral_1h_input_tokens)
```

```text
20 2048 0 2048 0
```

В опыте SDK отправил `"cache_control": {"type": "ephemeral"}` в теле запроса, а мок вернул запись 2048 токенов (`cache_creation_input_tokens`), 5-минутную часть (`ephemeral_5m_input_tokens`) и 20 некэшированных токенов в `input_tokens`.

## Явные точки

Для точного контроля `cache_control` ставят на конкретные блоки. TTL по умолчанию — 5 минут; для часа добавляют `"ttl": "1h"`:

```python
r = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    system=[{"type": "text", "text": "You are an expert.", "cache_control": {"type": "ephemeral", "ttl": "1h"}}],
    messages=[{"role": "user", "content": "Q"}],
)
print(r.usage.cache_read_input_tokens)
```

```text
2048
```

(В опыте мок вернул чтение 2048 токенов; тело запроса содержало `{"type": "ephemeral", "ttl": "1h"}` в блоке `system`.) Для инструментов `cache_control` ставят на **последний** инструмент: все инструменты до него и он сам кэшируются как один префикс (в опыте в запросе был `cache_control` только у второго инструмента). Максимум — **4 точки** в запросе; автоматический режим занимает одну.

## Читаем usage

Эффект виден в `usage`:

| Поле | Смысл |
|---|---|
| `input_tokens` | токены **после** последней точки: не кэшируются |
| `cache_creation_input_tokens` | записано в кэш |
| `cache_read_input_tokens` | прочитано из кэша |
| `cache_creation.ephemeral_5m_input_tokens` / `ephemeral_1h_input_tokens` | запись с разбивкой по TTL (сумма равна `cache_creation_input_tokens`) |

Общий размер входа: `cache_read_input_tokens + cache_creation_input_tokens + input_tokens` (в опыте `0 + 2048 + 20 = 2068`). Если `cache_creation_input_tokens` и `cache_read_input_tokens` оба равны `0`, префикс не кэшировался: он короче минимума модели или точка не поставлена.

## Сколько это стоит

Относительно базовой цены входных токенов:

| Операция | Множитель |
|---|---|
| Запись на 5 минут | 1,25× |
| Запись на 1 час | 2× |
| Чтение из кэша | 0,1× (на ряде моделей ниже; см. [экспертную теорию](expert.ru.md#справочник)) |
| Обычный вход без кэша | 1× |

Расчёт на условной базовой цене 5 $/млн токенов (число для примера, не цена конкретной модели):

```python
def cost(usage, base=5.0, mult_write5=1.25, mult_write1h=2.0, mult_read=0.1):
    per = base / 1_000_000
    cc = usage.cache_creation
    w5 = cc.ephemeral_5m_input_tokens if cc else usage.cache_creation_input_tokens
    w1 = cc.ephemeral_1h_input_tokens if cc else 0
    return per * (usage.input_tokens + w5 * mult_write5 + w1 * mult_write1h + usage.cache_read_input_tokens * mult_read)
```

```python
w, h = usage_of_write_request, usage_of_read_request   # usage из опыта на моке
nocache = 5.0 / 1_000_000 * (20 + 2048)
print(round(cost(w), 6), round(cost(h), 6), round(nocache, 6), round(cost(h) / nocache, 3))
```

```text
0.0129 0.001124 0.01034 0.109
```

Числа — стоимость записи, стоимость чтения, стоимость без кэша и отношение «чтение / без кэша». Для 2048 кэшируемых токенов и 20 обычных первая отправка (запись) дороже запроса без кэша примерно на четверть, зато каждое чтение стоит около 11% от обычной цены. Кэш окупается со второго запроса при совпадении префикса. Расчёт показывает порядок величин: настоящие цены берите из прайса.

## Ошибки новичков

- ❌ Кэшировать префикс короче минимума модели → ✅ проверить минимальную длину и `usage`.
- ❌ Ставить меняющийся текст (время, UUID) в начало → ✅ стабильное — в начало, переменное — в конец.
- ❌ Менять инструменты или `system` между запросами → ✅ держать их неизменными.
- ❌ Ждать экономию с первого запроса → ✅ запись дороже; выигрыш — на повторах.
- ❌ Не смотреть `usage` → ✅ проверять `cache_read_input_tokens`.
- ❌ Больше 4 точек → ✅ 400.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — TTL, прогрев, параллельные запросы, архитектура префикса
- Ранее: [02 · Messages API](../02-messages-api/basics.ru.md)
- Параллели: [LLM](../../../llm/overview.ru.md)

## Источники

- [Prompt caching](https://platform.claude.com/docs/en/build-with-claude/prompt-caching)
- [Cache diagnostics](https://platform.claude.com/docs/en/build-with-claude/cache-diagnostics)
