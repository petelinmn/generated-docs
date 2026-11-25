---
title: "OpenSearch"
lang: ru
track: opensearch
kind: track
doc: overview
slug: opensearch
tags: [opensearch, search-engine, full-text-search, inverted-index, lucene, aggregations]
versions: "OpenSearch 3.10; Python opensearch-py 3.2"
default_level: 4
status: draft
updated: 2026-11-25
---

# OpenSearch

🌐 **Русский** · [English](overview.en.md)

## Коротко

OpenSearch — распределённый поисковый и аналитический движок с открытым кодом на основе библиотеки Apache Lucene. Данные хранятся как JSON-**документы** в **индексах**; текст разбирается **анализаторами** на термы и складывается в **обратный индекс** (inverted index), поэтому полнотекстовый поиск с оценкой релевантности выполняется быстро. Поверх поиска есть **агрегации** для аналитики, векторный поиск и работа с логами. Взаимодействуют с OpenSearch через REST API.

Проект произошёл от Elasticsearch и Kibana (ответвление версии 7.10.2); интерфейс похож, но продукты развиваются независимо, и не всё из документации Elasticsearch применимо. Для аналитики и визуализации есть OpenSearch Dashboards.

Это поисковый движок, а не основная база данных: транзакций между документами нет, а данные становятся видны поиску не мгновенно.

**Пререквизиты:** HTTP и JSON, один язык программирования; для примеров достаточно [трека Python](../python/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| Узел | `opensearchproject/opensearch:3.10.1` в Docker (команда ниже) |
| REST API | `http://localhost:9200`; запросы — `curl` или клиент |
| Язык примеров | Python 3.12+ с клиентом `opensearch-py` |
| Код модулей | как в треке Python: `code/` с `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Тесты модуля | `uv run pytest` внутри `code/` |
| Тесты и сервер | тесты, которым нужен живой узел, помечены `integration` и пропускаются без `localhost:9200`; остальные не используют сеть |
| Одноразовый узел | `docker rm -f opensearch` удаляет узел и его данные |

```bash
docker run -d --name opensearch -p 9200:9200 \
  -e discovery.type=single-node \
  -e DISABLE_SECURITY_PLUGIN=true -e DISABLE_INSTALL_DEMO_CONFIG=true \
  -e "OPENSEARCH_JAVA_OPTS=-Xms512m -Xmx512m" \
  opensearchproject/opensearch:3.10.1
```

> `DISABLE_SECURITY_PLUGIN=true` отключает аутентификацию и TLS: **только** для локального одноразового опыта. Такой узел нельзя выставлять в сеть. Настройка безопасности — в соответствующем модуле.

## Дорожная карта

Прогресс изучения отмечается в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию: **4** (теория и самопроверка), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Основы OpenSearch](modules/01-opensearch-basics/overview.ru.md) — кластер, узел, индекс, документ, CRUD, REST, обновление поиска
- [02 · Отображения и анализ текста](modules/02-mappings-and-analysis/overview.ru.md) — типы полей, `text` и `keyword`, анализаторы, динамическое отображение
- [03 · Поисковые запросы](modules/03-search-queries/overview.ru.md) — `match`, `term`, `bool`, фильтры, релевантность, сортировка, постраничность
- [04 · Агрегации](modules/04-aggregations/overview.ru.md) — метрики, корзины, вложенные агрегации

### Часть 2. Продвинутые темы

- [05 · Индексация в большом объёме](modules/05-indexing-at-scale/overview.ru.md) — bulk, refresh, шаблоны индексов, псевдонимы, reindex
- [06 · Релевантность](modules/06-relevance-tuning/overview.ru.md) — BM25, бустинг, объяснение оценки, синонимы
- [07 · Кластер и шарды](modules/07-cluster-and-shards/overview.ru.md) — шарды, реплики, распределение, восстановление
- [08 · Жизненный цикл индексов](modules/08-index-lifecycle/overview.ru.md) — data streams, ISM, удаление и перенос данных
- [09 · Векторный и гибридный поиск](modules/09-vector-search/overview.ru.md) — k-NN, эмбеддинги, объединение с полнотекстовым

### Часть 3. Темы для опытных

- [10 · Производительность](modules/10-performance/overview.ru.md) — профилирование запросов, кэши, настройка
- [11 · Безопасность](modules/11-security/overview.ru.md) — аутентификация, роли, TLS, права на уровне документов и полей
- [12 · Мониторинг и резервные копии](modules/12-monitoring-and-snapshots/overview.ru.md) — метрики, медленные запросы, снимки (snapshots)
- [13 · Приём данных](modules/13-ingest-pipelines/overview.ru.md) — ingest-конвейеры, Data Prepper
- [14 · OpenSearch в Kubernetes](modules/14-kubernetes/overview.ru.md) — оператор, хранилище, обновления

## Библиотеки и инструменты

- [opensearch-py](libraries/opensearch-py/overview.ru.md) — клиент для Python
- [OpenSearch.Client для .NET](libraries/opensearch-net/overview.ru.md) — клиент для C#
- [OpenSearch Dashboards](tools/opensearch-dashboards/overview.ru.md) — визуализация и Dev Tools

## Рецепты

- [Поиск по каталогу товаров](recipes/product-search/overview.ru.md) — анализ, фильтры, фасеты, подсветка
- [Аналитика логов](recipes/log-analytics/overview.ru.md) — data streams, ISM, агрегации по времени
- [Гибридный поиск](recipes/hybrid-search/overview.ru.md) — полнотекстовый плюс векторный

## Проекты

- [Поисковый сервис](projects/search-service/overview.ru.md) — API поиска с индексацией из базы данных

## Связанные треки

- [Kafka](../kafka/overview.ru.md) — источник потока данных для индексации
- [C# / .NET](../csharp/overview.ru.md) — приложения, использующие поиск
- [Python](../python/overview.ru.md) — язык примеров
- [Kubernetes](../kubernetes/overview.ru.md) — запуск оператором
- [Linux](../linux/overview.ru.md) — файловая система, память и диагностика

## Источники

- [OpenSearch documentation: About](https://opensearch.org/docs/latest/about/)
- [OpenSearch: Intro to OpenSearch](https://opensearch.org/docs/latest/getting-started/intro/)
- [opensearch-py](https://github.com/opensearch-project/opensearch-py)
