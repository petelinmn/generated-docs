---
title: "Квиз: семейство Claude и выбор модели"
lang: ru
track: claude
kind: module
doc: quiz
slug: 01-models
part: 1
tags: [claude, model-selection, effort, context-window]
status: verified
updated: 2026-11-23
---

# Квиз: семейство Claude и выбор модели

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень вопроса: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая теория](advanced.ru.md) · 🔴 [экспертная теория](expert.ru.md).

**Уровни:** [обзор](overview.ru.md) · [1. Базовая теория](basics.ru.md) · [2. Продвинутая теория](advanced.ru.md) · [3. Экспертная теория](expert.ru.md) · **4. Квиз** · [5. Запускаемые примеры](code/examples/) · [6. Тесты](code/tests/)

## 1. 🟢 Какая модель является стартовой для low-latency и high-volume workload?

- A) `claude-fable-5-2`
- B) `claude-opus-5-5`
- C) `claude-sonnet-5-5`
- D) `claude-haiku-5-5`

<details><summary>Ответ</summary>

**D.** Haiku 5.5 оптимизирована для низкой latency и высокой экономичности. Остальные не являются стартовым профилем именно для этого сценария. См. [базовую теорию](basics.ru.md).

</details>

## 2. 🟢 Какой параметр управляет effort в Messages API?

- A) `output_config.effort`
- B) `thinking.budget_tokens`
- C) `messages.effort`
- D) `context_effort`

<details><summary>Ответ</summary>

**A.** Effort задаётся через `output_config.effort`. Budget extended thinking имеет отдельную механику в legacy thinking mode. См. [базовую теорию](basics.ru.md).

</details>

## 3. 🟢 Что попадает в context window?

- A) Только исходный prompt пользователя
- B) Только предыдущие ответы
- C) System prompt, messages, tools и сгенерированный output
- D) Только training data

<details><summary>Ответ</summary>

**C.** Context window включает system prompt, всю историю, tools, tool results и документы/изображения в запросе, а также output turn. См. [базовую теорию](basics.ru.md).

</details>

## 4. 🟡 Какой подход лучше для высокой доли простых задач и редкого escalation?

- A) Всегда Fable
- B) Дешёвый executor с escalation к более сильной advisor-модели
- C) Один Opus на все задачи
- D) Увеличить только `max_tokens`

<details><summary>Ответ</summary>

**B.** Multimodel-схема экономит типовые токены и оставляет сложные решения сильной модели. Остальные варианты не связывают cost с распределением workload. См. [продвинутую теорию](advanced.ru.md).

</details>

## 5. 🟢 Что выведет код?

```python
models = {
    "high_volume": "claude-haiku-5-5",
    "demanding": "claude-fable-5-2",
}
print(models.get("unknown", "claude-opus-5-5"))
```

<details><summary>Ответ</summary>

```text
claude-opus-5-5
```

`dict.get` возвращает fallback-третий аргумент, когда ключ отсутствует.
См. [базовую теорию](basics.ru.md).

</details>

## 6. 🟡 Выберите все верные утверждения

- [ ] A) Effort можно тестировать внутри одной модели
- [ ] B) 1M context гарантирует perfect retrieval
- [ ] C) В eval нужно фиксировать prompts и datasets
- [ ] D) Partner-platform lifecycle всегда совпадает с Claude API

<details><summary>Ответ</summary>

**A, C.** Effort является tunable-параметром одной модели, а eval требует воспроизводимого набора. 1M не гарантирует perfect retrieval, а partner lifecycle может отличаться. См. [продвинутую теорию](advanced.ru.md).

</details>

## 7. 🟢 Какой параметр в API задаёт абсолютный максимум сгенерированного output?

- A) `effort`
- B) `max_tokens`
- C) `context_window`
- D) `cache_control`

<details><summary>Ответ</summary>

**B.** `max_tokens` ограничивает генерацию output; он не устанавливает весь размер context window. См. [базовую теорию](basics.ru.md).

</details>

## 8. 🟡 Найдите ошибку

```python
config = {
    "provider": "claude-api",
    "model": "claude-opus-5-5",
}
print(config["deployment"])
```

<details><summary>Ответ</summary>

Ошибка — ключ `deployment` отсутствует. Для Claude API нужно использовать `model`, а deployment name нужен только partner platform с deployment-конфигурацией.
См. [базовую теорию](basics.ru.md).

</details>

## 9. 🟡 Что лучше протестировать первым после смены модели?

- A) Только happy path
- B) Реальные prompts, edge cases и output contract
- C) Только price
- D) Только latency

<details><summary>Ответ</summary>

**B.** Смена модели меняет accuracy, tool strategy и stop behavior; все эти аспекты нужно проверить на production-похожих данных. См. [продвинутую теорию](advanced.ru.md).

</details>

## 10. 🔴 Как следует интерпретировать dateless model ID в поколении 4.6+?

- A) Всегда alias
- B) Всегда Bedrock profile
- C) Он может быть own pinned snapshot
- D) Всегда устаревшая модель

<details><summary>Ответ</summary>

**C.** Dateless ID модели поколения 4.6+ — это собственный pinned snapshot, и в таблице он повторяется как alias. Семантика alias в старых поколениях может отличаться. См. [экспертную теорию](expert.ru.md).

</details>

## 11. 🟡 Найдите проблему в migration-плане

```text
Меняем claude-sonnet-5-5 на claude-opus-5-5,
не меняем tools и system,
пускаем весь traffic сразу.
```

<details><summary>Ответ</summary>

План игнорирует breaking changes и feature differences. Нужно прогнать test matrix, проверить tools/system/earlier messages, thinking и partner deployment, а rollout проводить staged. См. [продвинутую теорию](advanced.ru.md).

</details>

## 12. 🔴 Открытый вопрос

Как вы выберете модель для сервиса 10 000 классификаций в час с автоматическим escalation при низкой уверенности?

<details><summary>Ключевые пункты ответа</summary>

- Начать с Haiku или другого low-cost balanced варианта.
- Использовать одинаковый eval-набор и фиксированный output contract.
- Измерить accuracy, latency, cost и долю escalation.
- Escalate сложные или низкоуверенные cases в Sonnet/Opus/Fable.
- Прогнать threshold tuning и error-cost analysis.
- Проверить prompt caching и partner deployment для production.
См. [экспертную теорию](expert.ru.md).

</details>
