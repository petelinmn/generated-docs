---
title: "Безопасность и compliance: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
status: draft
updated: 2026-11-25
---

# Безопасность и compliance: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Идентификация

| Средство | Суть |
|---|---|
| API-ключ | статический секрет; срок действия, scope, ротация |
| service account | нечеловеческая учётная запись организации (`svac_…`) |
| Workload Identity Federation | обмен OIDC-токена вашего поставщика на короткоживущий токен; настраиваются service account, issuer и правило |
| Admin-ключ | управление организацией; не использовать для обычных запросов |

## Свойства sandbox

macOS использует Seatbelt, Linux и WSL2 — `bubblewrap` и `socat`. Запись — в рабочий и временный каталоги, сеть — через прокси со списком разрешённых доменов, изначально пустым. Команды из `excludedCommands` выполняются вне sandbox; `allowUnsandboxedCommands: false` запрещает выход из него при неудаче.

## Подвохи

1. Проверка пути строкой (`startswith`) пропускает `workspace-evil`; сравнивайте разрешённые пути через `is_relative_to`.
2. `deny` на `Read` не останавливает скрипт, открывающий файл сам.
3. `-p` пропускает диалог доверия к каталогу.
4. Настройки, hooks, skills и `.mcp.json` из чужого репозитория — это исполняемое содержимое.
5. Телеметрия с включёнными текстами переносит чувствительные данные в сборщик.
6. ZDR не означает, что у вас самих ничего не хранится: логи и расшифровки остаются.

> ⚠️ Проверить: условия ZDR и HIPAA, WIF, sandbox, `inference_geo` на настоящем API не проверялись. Юридические вопросы решайте по договору.

## Вопросы с собеседований

1. Что такое косвенная prompt injection?
<details><summary>Ответ</summary>

Инструкции внутри данных, которые модель читает по ходу задачи.
</details>

2. Можно ли решить это промптом?
<details><summary>Ответ</summary>

Нет, только снизить риск; нужны ограничения прав и проверки в коде.
</details>

3. Какая комбинация возможностей опасна?
<details><summary>Ответ</summary>

Приватные данные, недоверенный контент и канал наружу одновременно.
</details>

4. Как защитить инструмент чтения файлов?
<details><summary>Ответ</summary>

Разрешить корень, разрешать путь и проверять вложенность, учитывать симлинки, ограничить размер.
</details>

5. Чем WIF лучше статического ключа?
<details><summary>Ответ</summary>

Краткоживущие токены от своего поставщика удостоверений, нечего красть из CI и не надо ротировать.
</details>

6. Где вызывать API из веб-приложения?
<details><summary>Ответ</summary>

На сервере; ключ в браузер не отдают.
</details>

7. Что такое ZDR?
<details><summary>Ответ</summary>

Соглашение, при котором промпты и ответы не хранятся после ответа; не для всех функций и моделей.
</details>

8. Зачем подтверждение человеком?
<details><summary>Ответ</summary>

Для необратимых и дорогих действий: ошибку модели нельзя откатить.
</details>

9. Правила прав и sandbox?
<details><summary>Ответ</summary>

Первые решают, спрашивать ли, по тексту; второй ограничивает процесс на уровне ОС.
</details>

10. Что проверить перед подключением стороннего MCP или плагина?
<details><summary>Ответ</summary>

Код и автора, права, куда уходят данные, фиксацию версии.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [12 · Агенты](../12-agents/expert.ru.md) · [15 · Observability](../15-observability/expert.ru.md)

## Источники

- [Workload Identity Federation](https://platform.claude.com/docs/en/manage-claude/workload-identity-federation)
- [Claude Code sandboxing](https://code.claude.com/docs/en/sandboxing)
- [Claude Code data usage](https://code.claude.com/docs/en/data-usage)
