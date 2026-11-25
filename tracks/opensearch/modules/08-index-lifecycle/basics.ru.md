---
title: "Жизненный цикл индексов: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Жизненный цикл индексов: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Зачем это

Логи и метрики только прибывают, а нужны недавние. Удалять документы по одному дорого, удалить целый индекс — дёшево. Поэтому данные пишут в череду индексов: новый создаётся по расписанию или размеру, старые переводятся в более дешёвое состояние и удаляются.

## Data stream

Поток данных (data stream) — одно имя над скрытыми backing-индексами. Запись идёт в последний, поиск — по всем. Ему нужен шаблон с `data_stream`, а каждому документу — поле `@timestamp`.

```python
from opensearchpy import OpenSearch
from opensearchpy.exceptions import RequestError

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])

def api(method, path, body=None, **params):
    return client.transport.perform_request(method, path, body=body, params=params)

api("PUT", "/_index_template/logs", {"index_patterns": ["logs-app*"], "data_stream": {}, "priority": 100,
    "template": {"settings": {"number_of_shards": 1, "number_of_replicas": 0}}})

client.index(index="logs-app", body={"@timestamp": "2026-11-25T10:00:00Z", "msg": "start"},
             params={"refresh": "true"})["_index"]        # '.ds-logs-app-000001'
```

```python
ds = api("GET", "/_data_stream/logs-app")["data_streams"][0]
ds["generation"], [i["index_name"] for i in ds["indices"]]    # (1, ['.ds-logs-app-000001'])

api("POST", "/logs-app/_rollover")["new_index"]               # '.ds-logs-app-000002'
client.index(index="logs-app", body={"@timestamp": "2020-01-01T00:00:00Z", "msg": "late"},
             params={"refresh": "true"})["_index"]            # '.ds-logs-app-000002'
client.count(index="logs-app")["count"]                       # 2
```

Документ с датой 2020 года попал в новый индекс: выбирается всегда текущий индекс записи, а не «подходящий по дате».

## Что запрещено

```python
def refused(call):
    try:
        call()
    except RequestError as e:
        return e.error

refused(lambda: client.index(index="logs-app", body={"msg": "no time"}))
# 'mapper_parsing_exception'
refused(lambda: client.index(index="logs-app", id="1", body={"@timestamp": "2026-11-25T10:00:00Z"}))
# 'illegal_argument_exception'    only op_type=create is allowed
client.create(index="logs-app", id="1", body={"@timestamp": "2026-11-25T10:00:00Z"}, params={"refresh": "true"})["result"]
# 'created'
refused(lambda: client.delete(index="logs-app", id="1"))
# 'illegal_argument_exception'
refused(lambda: client.indices.delete(index=".ds-logs-app-000002"))
# 'illegal_argument_exception'    the write index cannot be deleted
```

Поток рассчитан на добавление. Если данные часто меняются по `_id`, берите обычный индекс.

## Удаление старого

```python
api("DELETE", "/_data_stream/logs-app")       # the stream and all its backing indices
```

Вручную удаляют и отдельные старые backing-индексы. Автоматически это делает ISM — [следующий уровень](advanced.ru.md).

## Частые ошибки новичков

- Чистить логи через `_delete_by_query` вместо удаления индексов.
- Забыть `@timestamp` в отправителе логов.
- Заводить индекс на каждый день при малом объёме: получаются сотни крошечных шардов.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [05 · Индексация: шаблоны, псевдонимы, rollover](../05-indexing-at-scale/basics.ru.md)

## Источники

- [Data streams](https://docs.opensearch.org/latest/im-plugin/data-streams/)
