---
title: "Компактизация и длинный контекст: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Компактизация и длинный контекст: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Бета-заголовки и параметры

| Возможность | Заголовок | Параметр |
|---|---|---|
| по требованию | `compact-2026-09-04` | верхнеуровневый `compaction: {"type": "summarize"}` |
| по порогу | `compact-2026-01-12` | `context_management.edits[compact_20260112]` |
| очистка | `context-management-2025-06-27` | `clear_tool_uses_20250919`, `clear_thinking_20251015` |

## Другие факты из документации

- Модели с окном 1M: до 128k токенов вывода; до 600 изображений или страниц PDF в запросе (100 при 200k).
- Thinking прошлых ходов по умолчанию остаётся в контексте у Opus 4.5+, Sonnet 4.6+, Haiku 5.5 и Fable/Mythos; у более старых API убирает его сам.
- Отслеживать остаток окна сами умеют только Sonnet 4.5 и Haiku 4.5; для остальных есть task budgets (бета).
- Подсчёт токенов бесплатен, имеет свои лимиты запросов, это оценка и он не учитывает кэш.
- Расход компактизации по требованию виден в `usage.iterations`.

## Подвохи

1. Если в ответ на запрос сводки `stop_reason` не `compaction`, не заменяйте историю: функция выше возвращает старую.
2. Сообщения, которые уже пересказаны, нельзя оставлять перед блоком: по документации это 400 `compaction_block_misplaced`.
3. У двух видов компактизации разные заголовки и противоположные правила: блок по требованию стоит перед новыми ходами, а при пороговой API отбрасывает всё до блока сам.
4. `instructions` не дополняют, а полностью заменяют стандартный промпт.
5. Обрезка, начинающаяся с `tool_result`, ломает запрос.

> ⚠️ Проверить: ответы сервиса (блок `compaction`, `applied_edits`, ошибки переполнения), фоновая компактизация и сохранение последних ходов не проверялись вживую.

## Вопросы с собеседований

1. Что считается в окне?
<details><summary>Ответ</summary>

Всё в запросе и генерируемый вывод, включая thinking и кэшированные токены.
</details>

2. Компактизация против очистки?
<details><summary>Ответ</summary>

Первая пересказывает, вторая удаляет выбранные блоки.
</details>

3. Компактизация против своей суммаризации?
<details><summary>Ответ</summary>

Серверная — без своего кода и с блоком, который API понимает; своя — полный контроль над текстом.
</details>

4. Почему нельзя сохранять только текст ответа?
<details><summary>Ответ</summary>

Теряется блок `compaction`, и вместе с ним сжатое состояние.
</details>

5. Как сжатие влияет на кэш?
<details><summary>Ответ</summary>

Изменённый префикс кэшируется заново.
</details>

6. Что делать с большим результатом инструмента?
<details><summary>Ответ</summary>

Фильтровать и разбивать на стороне инструмента, возвращать ссылку, очищать старые.
</details>

7. RAG или длинный контекст?
<details><summary>Ответ</summary>

Длинный проще для небольшого стабильного корпуса; поиск дешевле и масштабируется для большого и меняющегося.
</details>

8. Как не потерять важное при сжатии?
<details><summary>Ответ</summary>

Записывать во внешнюю память или файлы, задавать фокус, оставлять последние ходы дословно.
</details>

9. Что такое `model_context_window_exceeded`?
<details><summary>Ответ</summary>

`stop_reason`, когда генерация упёрлась в окно; ответ неполный.
</details>

10. Зачем считать токены под конкретную модель?
<details><summary>Ответ</summary>

Токенизаторы различаются: старые измерения не переносятся.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [05 · Prompt caching](../05-prompt-caching/expert.ru.md)

## Источники

- [Context windows](https://platform.claude.com/docs/en/build-with-claude/context-windows)
- [Task budgets](https://platform.claude.com/docs/en/build-with-claude/task-budgets)
- [Memory tool](https://platform.claude.com/docs/en/agents-and-tools/tool-use/memory-tool)
