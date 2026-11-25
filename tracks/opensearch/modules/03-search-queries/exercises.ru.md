---
title: "Упражнения: поисковые запросы"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: поисковые запросы

🌐 **Русский** · [English](exercises.en.md)

> Нужен узел из [обзора трека](../../overview.ru.md#окружение) и данные с функцией `search` из [базовой теории](basics.ru.md).

## 1. Точные и полнотекстовые

Найдите клавиатуры (по названию) дешевле 60.

<details><summary>Подсказка</summary>

`match` в `must`, `range` в `filter`.

</details>

<details><summary>Решение</summary>

```python
print(search(query={"bool": {"must": [{"match": {"name": "keyboard"}}], "filter": [{"range": {"price": {"lt": 60}}}]}}))
```

</details>

## 2. Опечатки и стемминг

Запрос `wireles mous` с `fuzziness` по полю `name` ставит первой проводную клавиатуру. Выясните почему и добейтесь, чтобы находилась беспроводная мышь.

<details><summary>Подсказка</summary>

Посмотрите через `_analyze`, какие термы даёт `english` для запроса с опечаткой.

</details>

<details><summary>Решение</summary>

```python
q = {"query": "wireles mous", "fuzziness": "AUTO"}
print(search(query={"match": {"name": q}}))          # Wired Keyboard first
# the stemmer cut the misspelled words to "wirel" and "mou"; "wirel" is close to "wire" (wired)
print([t["token"] for t in client.indices.analyze(body={"text": "wireles mous", "analyzer": "english"})["tokens"]])

# add a non-stemmed sub-field and reindex the existing documents in place
client.indices.put_mapping(index="shop", body={"properties": {"name": {
    "type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}, "std": {"type": "text"}}}}})
client.update_by_query(index="shop", params={"refresh": "true"})
print(search(query={"match": {"name.std": q | {"operator": "and"}}}))   # only document 1
```

</details>

## 3. Бустинг наличия

Выведите электронику так, чтобы товары в наличии были выше.

<details><summary>Подсказка</summary>

`filter` плюс `should`.

</details>

<details><summary>Решение</summary>

```python
print(search(query={"bool": {"filter": [{"term": {"category": "electronics"}}], "should": [{"range": {"stock": {"gt": 0}}}]}}))
```

</details>

## 4. Обход всех страниц

Выведите все `_id` страницами по 2 через `search_after`.

<details><summary>Подсказка</summary>

Повторяйте, пока страница не пуста.

</details>

<details><summary>Решение</summary>

```python
body = {"sort": [{"price": "asc"}, {"_id": "asc"}], "size": 2}
while True:
    hits = client.search(index="shop", body=body)["hits"]["hits"]
    if not hits:
        break
    print([h["_id"] for h in hits])
    body["search_after"] = hits[-1]["sort"]
```

</details>

## 5. Фасеты

Покажите товары категории `electronics`, но счётчики — по всем категориям.

<details><summary>Подсказка</summary>

`post_filter` и `terms`.

</details>

<details><summary>Решение</summary>

```python
r = client.search(index="shop", body={"post_filter": {"term": {"category": "electronics"}},
                                      "aggs": {"c": {"terms": {"field": "category"}}}})
print([h["_id"] for h in r["hits"]["hits"]])
print({b["key"]: b["doc_count"] for b in r["aggregations"]["c"]["buckets"]})
```

</details>
