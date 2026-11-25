---
title: "MCP SDK: экспертный уровень"
lang: ru
track: claude
kind: library
doc: expert
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## `MCPServer`

| Группа | Методы и параметры |
|---|---|
| Регистрация | `tool`, `resource`, `prompt`, `completion`, `add_tool`, `add_resource`, `add_prompt`, `remove_tool`, `remove_prompt` |
| Запуск | `run("stdio" / "streamable-http" / "sse")`, `run_stdio_async`, `run_streamable_http_async`, `streamable_http_app`, `custom_route` |
| Настройка | `instructions`, `version`, `lifespan`, `middleware`, `extensions`, `log_level`, `cache_hints`, `subscriptions` |
| Безопасность | `auth`, `token_verifier`, `auth_server_provider`, `resource_security`, `request_state_security` |

## `Client`

`list_tools`, `call_tool`, `list_resources`, `list_resource_templates`, `read_resource`, `list_prompts`, `get_prompt`, `complete`, `listen`, `subscribe_resource`

Параметры: `read_timeout_seconds`, `raise_exceptions`, `mode`, `logging_callback`, `elicitation_callback`, `sampling_callback`, `list_roots_callback`, `input_required_max_rounds` (10), `cache`.

Имена взяты из установленного пакета; не все запускались. Для полного контроля над протоколом есть низкоуровневый `mcp.server.lowlevel`.

## Устройство вызова

Клиент шлёт JSON-RPC `tools/call`; сервер проверяет аргументы pydantic-моделью, вызывает функцию, превращает результат в `content` и `structured_content`. Для примитивного возврата `structured_content` имеет вид `{"result": …}`, для pydantic-модели — её поля.

## Подвохи

1. `print()` в stdio-сервере портит протокол.
2. Ошибка инструмента не исключение: без проверки `is_error` её легко пропустить.
3. Обычное исключение не объясняет модели ничего — используйте `ToolError`.
4. В [Agent SDK](../agent-sdk/advanced.ru.md) поведение обратное: текст исключения доходит до модели.
5. Ресурс-шаблон не появляется в `list_resources`.
6. Пакет v2 заметно отличается от v1: примеры с `FastMCP` относятся к старой версии.

> ⚠️ Проверить: авторизация, elicitation, sampling, подписки, completions, TypeScript SDK и совместимость с конкретными хостами не запускались.

## Компромиссы

| Вариант | Когда |
|---|---|
| MCP-сервер | инструмент нужен нескольким приложениям |
| Обычный tool use | инструмент нужен одному вашему приложению |
| SDK MCP server ([Agent SDK](../agent-sdk/overview.ru.md)) | инструмент внутри процесса агента |
| MCP connector | API сам обращается к удалённому серверу |

## Вопросы с собеседований

1. Чем tool, resource и prompt отличаются?
<details><summary>Ответ</summary>

Тем, кто решает их использовать: модель, приложение, пользователь.
</details>

2. stdio или HTTP?
<details><summary>Ответ</summary>

stdio — локальный процесс одного пользователя; HTTP — удалённый сервис для многих, с авторизацией.
</details>

3. Откуда берётся схема инструмента?
<details><summary>Ответ</summary>

Из аннотаций типов и docstring.
</details>

4. Как вернуть модели понятную ошибку?
<details><summary>Ответ</summary>

`ToolError` с текстом; прочие исключения скрываются.
</details>

5. Почему клиент не получил исключение при ошибке инструмента?
<details><summary>Ответ</summary>

Это результат с `is_error`, часть нормального ответа, чтобы модель могла отреагировать.
</details>

6. Как тестировать сервер?
<details><summary>Ответ</summary>

`Client(mcp)` в процессе.
</details>

7. Где открывать соединения с БД?
<details><summary>Ответ</summary>

В lifespan.
</details>

8. Защищает ли проверка Origin от неавторизованных клиентов?
<details><summary>Ответ</summary>

Нет, только от браузерных атак; нужна аутентификация.
</details>

9. Чем опасен чужой MCP-сервер?
<details><summary>Ответ</summary>

Он выполняет свой код и возвращает текст, который может содержать инструкции для модели.
</details>

10. Что изменилось в ревизии 2026-07-28?
<details><summary>Ответ</summary>

Протокол стал stateless: без `initialize`, версия и возможности идут с каждым запросом.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [08 · MCP](../../modules/08-mcp/expert.ru.md) · [Anthropic SDK](../anthropic-sdk/overview.ru.md)

## Источники

- [What's new in v2](https://py.sdk.modelcontextprotocol.io/whats-new/)
- [Protocol versions](https://py.sdk.modelcontextprotocol.io/protocol-versions/)
- [MCP specification](https://modelcontextprotocol.io/specification/2026-07-28)
