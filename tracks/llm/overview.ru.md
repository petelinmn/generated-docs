---
title: "LLM"
lang: ru
track: llm
kind: track
doc: overview
slug: llm
tags: [llm, ai]
versions: ""
default_level: 4
status: draft
updated: 2026-11-20
---

# LLM

🌐 **Русский** · [English](overview.en.md)

## Коротко

Большие языковые модели (large language models, LLM) — нейросети на архитектуре трансформер, обученные предсказывать следующий токен на огромных объёмах текста. Через API или локально они решают задачи с текстом и кодом: отвечают на вопросы, извлекают данные, вызывают внешние инструменты и работают как агенты. Трек о том, как модели устроены и как строить на них надёжные приложения: от первого запроса к API до RAG, агентов, оценки качества и production.

**Пререквизиты:** модули 01–08 [трека Python](../python/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| Язык | Python 3.12+ и uv, как в треке Python. Для .NET — Microsoft.Extensions.AI |
| Доступ к API | ключ провайдера в переменной окружения или в `.env` (файл в `.gitignore`, не коммитить) |
| Локальные модели | Ollama |
| Код модуля | как в треке Python: `code/` с `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Запустить все тесты модуля | `uv run pytest` в папке `code/` |
| Тесты и API | юнит-тесты не обращаются к API: клиент подменяется. Интеграционные тесты помечены маркером `integration` и пропускаются, если нет ключа |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **4** (теория и самопроверка, без кода), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Как работают LLM](modules/01-how-llms-work/overview.ru.md) — токены, предсказание следующего токена, обучение, контекстное окно, temperature
- [02 · Промпт-инжиниринг](modules/02-prompt-engineering/overview.ru.md) — структура промпта, системные инструкции, few-shot, рассуждения
- [03 · Работа с API](modules/03-llm-apis/overview.ru.md) — сообщения и роли, параметры, стриминг, лимиты, стоимость
- [04 · Структурированный вывод](modules/04-structured-output/overview.ru.md) — JSON, схемы, валидация через Pydantic
- [05 · Tool use](modules/05-tool-use/overview.ru.md) — описание инструментов, цикл вызовов, обработка ошибок
- [06 · Эмбеддинги и векторный поиск](modules/06-embeddings/overview.ru.md) — векторы, косинусная близость, векторные БД

### Часть 2. Продвинутые темы

- [07 · RAG](modules/07-rag/overview.ru.md) — чанкинг, индексация, retrieval, reranking, оценка качества
- [08 · Агенты](modules/08-agents/overview.ru.md) — агентный цикл, планирование, память, multi-agent паттерны
- [09 · Model Context Protocol](modules/09-mcp/overview.ru.md) — серверы и клиенты, инструменты и ресурсы, свой MCP-сервер
- [10 · Оценка качества (evals)](modules/10-evals/overview.ru.md) — датасеты, метрики, LLM-as-judge, регрессионное тестирование промптов
- [11 · Локальные модели](modules/11-local-models/overview.ru.md) — Ollama, llama.cpp, квантизация, open-weight модели
- [12 · Контекст-инжиниринг](modules/12-context-engineering/overview.ru.md) — управление контекстом, кэширование промптов, сжатие, длинные диалоги

### Часть 3. Темы для опытных

- [13 · Трансформер изнутри](modules/13-transformer-internals/overview.ru.md) — attention, позиционное кодирование, мини-GPT своими руками
- [14 · Дообучение](modules/14-fine-tuning/overview.ru.md) — когда оно нужно, LoRA и QLoRA, подготовка данных
- [15 · LLM в production](modules/15-production/overview.ru.md) — стоимость, латентность, ретраи, наблюдаемость, guardrails
- [16 · Безопасность](modules/16-security/overview.ru.md) — prompt injection, утечки данных, безопасный tool use

## Библиотеки

- [SDK провайдеров](libraries/provider-sdks/overview.ru.md) — официальные клиенты API (Anthropic, OpenAI и другие)
- [MCP SDK](libraries/mcp-sdk/overview.ru.md) — создание MCP-серверов и клиентов
- [Ollama](libraries/ollama/overview.ru.md) — запуск моделей локально
- [sentence-transformers](libraries/sentence-transformers/overview.ru.md) — эмбеддинги локально
- [Qdrant](libraries/qdrant/overview.ru.md) — векторная БД
- [pgvector](libraries/pgvector/overview.ru.md) — векторный поиск в PostgreSQL
- [Hugging Face Transformers](libraries/hf-transformers/overview.ru.md) — загрузка и запуск open-weight моделей
- [Агентные фреймворки](libraries/agent-frameworks/overview.ru.md) — обзор и сравнение: когда брать фреймворк, а когда писать свой цикл

## Рецепты

- [Чат-бот с историей](recipes/chat-with-history/overview.ru.md) — диалог, стриминг, обрезка контекста
- [RAG по документации](recipes/rag-over-docs/overview.ru.md) — поиск ответов по этой базе знаний
- [Агент с инструментами](recipes/agent-with-tools/overview.ru.md) — поиск, файлы, вычисления
- [Свой MCP-сервер](recipes/mcp-server/overview.ru.md) — подключение к Claude Code и другим клиентам
- [Пайплайн оценки промптов](recipes/llm-eval-pipeline/overview.ru.md) — датасет, прогон, сравнение версий
- [Извлечение данных из текста](recipes/data-extraction/overview.ru.md) — структурированные данные из документов и PDF

## Проекты

- [Ассистент по базе знаний](projects/docs-assistant/overview.ru.md) — RAG по этому репозиторию: ответы с цитатами и квизы по модулям
- [Мини-агент для кода](projects/coding-agent/overview.ru.md) — агент, который читает и меняет файлы проекта

## Связанные треки

- [Python](../python/overview.ru.md) — основной язык трека
- [C# / .NET](../csharp/overview.ru.md) — LLM-приложения на .NET
- [Claude](../claude/overview.ru.md) — Anthropic-модели, Messages API и Claude Code

## Источники

- Документация API выбранного провайдера
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Документация Hugging Face](https://huggingface.co/docs)
