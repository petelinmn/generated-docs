---
title: "Prompt engineering"
lang: ru
track: claude
kind: module
doc: overview
slug: 03-prompt-engineering
part: 1
levels: [1, 2, 3, 4]
tags: [claude, prompt-engineering, system-prompt, xml-tags, few-shot, long-context, prompt-chaining, thinking]
versions: "Claude 5.5 family; рекомендации страницы Prompting best practices на 2026-11"
status: draft
updated: 2026-11-24
---

# Prompt engineering

🌐 **Русский** · [English](overview.en.md)

## Коротко

Prompt engineering — это искусство формулировать вход так, чтобы модель стабильно давала нужный результат. Для Claude основа проста: ясные прямые инструкции, достаточный контекст и мотивация, роль в `system`, структура через XML-теги, несколько разнообразных примеров, явный формат вывода и грамотная работа с длинными документами. Новые модели следуют инструкциям буквально и чувствительны к лишним «категорическим» правилам, поэтому лучший промпт — не самый длинный, а самый точный.

## Ключевые идеи

- **Ясность важнее хитрости:** хороший тест — показать промпт коллеге без контекста; если ему непонятно, непонятно и модели.
- **Объясняйте «зачем»:** правило с причиной модель обобщает лучше, чем голый запрет.
- **Роль в `system` даже одним предложением** меняет тон и экспертность.
- **XML-теги** разделяют инструкции, данные и примеры; имена тегов постоянные и описательные.
- **Примеры (multishot):** 3–5 релевантных и разнообразных, в тегах `<example>`.
- **Говорите, что делать, а не чего не делать:** «пиши связными абзацами» вместо «не используй markdown».
- **Длинные документы — в начало, вопрос — в конец;** просите сначала процитировать нужное.
- **Цепочки промптов:** сложную задачу делят на шаги с отдельными вызовами.
- **Мышление регулируется параметрами** (`effort`, adaptive thinking), а не заклинаниями в тексте.
- **На новых моделях меньше предписаний:** промпты, написанные для старых, нужно упрощать и перепроверять.

## Минимальный пример

```python
SYSTEM = "You are a senior Python reviewer. Be concise."

def build_prompt(docs, question):
    parts = ["<documents>"]
    for i, (source, text) in enumerate(docs, 1):
        parts.append(f'<document index="{i}"><source>{source}</source><document_content>{text}</document_content></document>')
    parts.append("</documents>")
    parts.append(f"First quote the passages that are relevant in <quotes> tags, then answer in <answer> tags.\n\nQuestion: {question}")
    return "\n".join(parts)

print(build_prompt([("a.txt", "Revenue grew 12%.")], "How did revenue change?"))
```

```text
<documents>
<document index="1"><source>a.txt</source><document_content>Revenue grew 12%.</document_content></document>
</documents>
First quote the passages that are relevant in <quotes> tags, then answer in <answer> tags.

Question: How did revenue change?
```

Документы идут первыми, вопрос — последним, формат ответа задан тегами.

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Единый тон, роль, правила | `system` |
| Данные, документы, примеры | XML-теги |
| Нестандартный формат или стиль | 3–5 примеров |
| Многошаговый процесс с проверкой | цепочка вызовов |
| Строго JSON | structured outputs (модуль 04 дорожной карты), а не уговоры в промпте |
| Разное поведение при одних и тех же словах | оценка (evals, модуль 14 дорожной карты), а не угадывание |

- ❌ Не наращивайте промпт «КРИТИЧЕСКИ ВАЖНО» и запретами: на новых моделях это даёт перекосы.
- ❌ Не правьте промпт «на глаз» без набора тестовых случаев.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | ясные инструкции, контекст, роль, XML-теги, примеры, формат вывода, документы |
| [2. Продвинутая теория](advanced.ru.md) | цепочки промптов, мышление и effort, особенности новых моделей, недоверенный текст, итерации |
| [3. Экспертная теория](expert.ru.md) | справочник приёмов, внутреннее устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn claude 03-prompt-engineering level 5` |
| 6. Тесты | пока не создан — `/learn claude 03-prompt-engineering level 6` |

## Связанные материалы

- Пререквизиты: [02 · Messages API](../02-messages-api/overview.ru.md)
- Дальше: [04 · Structured outputs](../04-structured-output/overview.ru.md)
- Аналоги в других треках: [LLM](../../../llm/overview.ru.md), [Python](../../../python/overview.ru.md)
- [14 · Batch, evals и надёжность](../14-evals/overview.ru.md)
