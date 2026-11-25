---
title: "Anthropic SDK: экспертный уровень"
lang: ru
track: claude
kind: library
doc: expert
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
status: draft
updated: 2026-11-25
---

# Anthropic SDK: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Поверхность клиента

| Ресурс | Что внутри |
|---|---|
| `client.messages` | `create`, `stream`, `parse`, `count_tokens`, `batches` |
| `client.models` | список и описание моделей |
| `client.files`, `client.skills` | файлы и skills (вне беты) |
| `client.beta` | бета-функции с параметром `betas=[...]`, `tool_runner` |
| `client.get` / `client.post` | endpoints без типизированного метода |

## Что уходит в запрос

SDK добавляет `anthropic-version` (в 2.x — `2026-10-21`), `User-Agent` вида `Anthropic/Python 2.14.0`, служебные заголовки `x-stainless-*`. Версию API меняет смена мажорной версии SDK. Повторы идут тем же клиентом; число сделанных повторов — `raw.retries_taken`.

## Особенности

- `response.model_fields_set` отличает отсутствующее поле от `null`; незнакомые поля видны в `model_extra`.
- SemVer соблюдается в основном: изменения только в типах могут выйти минорной версией. Фиксируйте версию.
- Прокси и транспорт задаются через `DefaultHttpxClient`; обычный клиент из пакета `httpx` вызовет `TypeError`.
- SDK выдаёт OpenTelemetry-спаны, если приложение зарегистрировало tracer provider.
- В браузере TypeScript SDK по умолчанию отказывается создаваться; `dangerouslyAllowBrowser` раскроет ключ пользователям.
- Платформенные клиенты и их отличия — в [17 · Развёртывание](../../modules/17-production/expert.ru.md).

## Подвохи

1. `page.data` — только первая страница; для всех элементов итерируйте саму страницу.
2. Tool runner по умолчанию оставляет разговор на сервере (thread).
3. Текст исключения вашего инструмента уходит модели: не кладите в исключения секреты.
4. `ANTHROPIC_LOG=debug` пишет содержимое сообщений.
5. `extra_body` перекрывает обычные параметры: в проверке `max_tokens` из него заменил переданный аргумент.
6. Переменные `ANTHROPIC_*` из окружения меняют адрес и учётные данные незаметно для кода.

> ⚠️ Проверить: SDK для C#, Go, Java, PHP и Ruby и их версии взяты из документации; OpenTelemetry-спаны и прокси не запускались.

## Компромиссы

| Вариант | Когда |
|---|---|
| Client SDK | вы сами управляете циклом |
| tool runner | простой цикл с функциями |
| [Agent SDK](../agent-sdk/overview.ru.md) | нужен агент с готовыми инструментами |
| чистый HTTP | язык без SDK |

## Вопросы с собеседований

1. Зачем SDK вместо `requests`?
<details><summary>Ответ</summary>

Типы, повторы с учётом заголовков, стриминг с накоплением, пагинация, классы ошибок, платформенные клиенты.
</details>

2. `stream()` против `create(stream=True)`?
<details><summary>Ответ</summary>

Первый — контекстный менеджер с накоплением и помощниками, второй — сырые события.
</details>

3. Что делает `with_options`?
<details><summary>Ответ</summary>

Возвращает копию клиента с другими настройками, не меняя исходный.
</details>

4. Где взять заголовки ответа?
<details><summary>Ответ</summary>

`with_raw_response` или `with_streaming_response`; в TypeScript `.withResponse()`.
</details>

5. Как передать параметр, которого нет в типах?
<details><summary>Ответ</summary>

`extra_body`, `extra_headers`, `extra_query`; они перекрывают одноимённые параметры, поэтому не передавайте в них недоверенные данные.
</details>

6. Чем опасен большой `max_tokens` без стриминга?
<details><summary>Ответ</summary>

Долгое соединение обрывается; SDK заранее бросает `ValueError`, если ждать пришлось бы около 10 минут.
</details>

7. Что такое thread в tool runner?
<details><summary>Ответ</summary>

Хранение разговора на сервере, чтобы не слать историю; бета, не под ZDR, отключается `thread=False`.
</details>

8. Как тестировать код с SDK?
<details><summary>Ответ</summary>

Подменить транспорт `httpx2.MockTransport` или передать фиктивный клиент.
</details>

9. Когда нужен `AsyncAnthropic`?
<details><summary>Ответ</summary>

В асинхронном приложении и для параллельных запросов; ограничивайте параллелизм.
</details>

10. Сколько клиентов создавать?
<details><summary>Ответ</summary>

Обычно один на приложение: он переиспользует соединения.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [Agent SDK](../agent-sdk/expert.ru.md) · [MCP SDK](../mcp-sdk/expert.ru.md)

## Источники

- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
- [Message threads](https://platform.claude.com/docs/en/build-with-claude/message-threads)
- [anthropic-sdk-python](https://github.com/anthropics/anthropic-sdk-python)
