---
title: "OpenSearch"
lang: en
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

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

OpenSearch is an open-source distributed search and analytics engine built on the Apache Lucene library. Data is stored as JSON **documents** in **indexes**; text is broken into terms by **analyzers** and put into an **inverted index**, so full-text search with relevance scoring is fast. On top of search there are **aggregations** for analytics, vector search and log handling. You talk to OpenSearch through a REST API.

The project descends from Elasticsearch and Kibana (a fork of version 7.10.2); the interface is similar, but the products evolve independently and not everything in Elasticsearch documentation applies. OpenSearch Dashboards provides analytics and visualisation.

It is a search engine, not a primary database: there are no transactions across documents and data does not become searchable instantly.

**Prerequisites:** HTTP and JSON, one programming language; the [Python track](../python/overview.en.md) is enough for the examples.

## Environment

| What | Tool |
|---|---|
| Node | `opensearchproject/opensearch:3.10.1` in Docker (command below) |
| REST API | `http://localhost:9200`; requests with `curl` or a client |
| Example language | Python 3.12+ with the `opensearch-py` client |
| Module code | as in the Python track: `code/` with `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Run module tests | `uv run pytest` inside `code/` |
| Tests and the server | tests that need a live node are marked `integration` and skipped when `localhost:9200` is unavailable; the rest use no network |
| Disposable node | `docker rm -f opensearch` removes the node and its data |

```bash
docker run -d --name opensearch -p 9200:9200 \
  -e discovery.type=single-node \
  -e DISABLE_SECURITY_PLUGIN=true -e DISABLE_INSTALL_DEMO_CONFIG=true \
  -e "OPENSEARCH_JAVA_OPTS=-Xms512m -Xmx512m" \
  opensearchproject/opensearch:3.10.1
```

> `DISABLE_SECURITY_PLUGIN=true` switches off authentication and TLS: for a local disposable experiment **only**. Such a node must not be exposed to a network. Security setup is in the corresponding module.

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · OpenSearch basics](modules/01-opensearch-basics/overview.en.md) — cluster, node, index, document, CRUD, REST, search refresh
- [02 · Mappings and text analysis](modules/02-mappings-and-analysis/overview.en.md) — field types, `text` and `keyword`, analyzers, dynamic mapping
- [03 · Search queries](modules/03-search-queries/overview.en.md) — `match`, `term`, `bool`, filters, relevance, sorting, pagination
- [04 · Aggregations](modules/04-aggregations/overview.en.md) — metrics, buckets, nested aggregations

### Part 2. Advanced topics

- [05 · Indexing at scale](modules/05-indexing-at-scale/overview.en.md) — bulk, refresh, index templates, aliases, reindex
- [06 · Relevance](modules/06-relevance-tuning/overview.en.md) — BM25, boosting, score explanation, synonyms
- [07 · Cluster and shards](modules/07-cluster-and-shards/overview.en.md) — shards, replicas, allocation, recovery
- [08 · Index lifecycle](modules/08-index-lifecycle/overview.en.md) — data streams, ISM, deleting and moving data
- [09 · Vector and hybrid search](modules/09-vector-search/overview.en.md) — k-NN, embeddings, combining with full-text

### Part 3. Expert topics

- [10 · Performance](modules/10-performance/overview.en.md) — query profiling, caches, tuning
- [11 · Security](modules/11-security/overview.en.md) — authentication, roles, TLS, document- and field-level permissions
- [12 · Monitoring and snapshots](modules/12-monitoring-and-snapshots/overview.en.md) — metrics, slow queries, snapshots
- [13 · Ingestion](modules/13-ingest-pipelines/overview.en.md) — ingest pipelines, Data Prepper
- [14 · OpenSearch on Kubernetes](modules/14-kubernetes/overview.en.md) — the operator, storage, upgrades

## Libraries and tools

- [opensearch-py](libraries/opensearch-py/overview.en.md) — the Python client
- [OpenSearch.Client for .NET](libraries/opensearch-net/overview.en.md) — the C# client
- [OpenSearch Dashboards](tools/opensearch-dashboards/overview.en.md) — visualisation and Dev Tools

## Recipes

- [Product catalog search](recipes/product-search/overview.en.md) — analysis, filters, facets, highlighting
- [Log analytics](recipes/log-analytics/overview.en.md) — data streams, ISM, time aggregations
- [Hybrid search](recipes/hybrid-search/overview.en.md) — full-text plus vector

## Projects

- [Search service](projects/search-service/overview.en.md) — a search API indexing from a database

## Related tracks

- [Kafka](../kafka/overview.en.md) — a source of a data stream to index
- [C# / .NET](../csharp/overview.en.md) — applications that use search
- [Python](../python/overview.en.md) — the language of the examples
- [Kubernetes](../kubernetes/overview.en.md) — running with an operator
- [Linux](../linux/overview.en.md) — the file system, memory and diagnostics

## Sources

- [OpenSearch documentation: About](https://opensearch.org/docs/latest/about/)
- [OpenSearch: Intro to OpenSearch](https://opensearch.org/docs/latest/getting-started/intro/)
- [opensearch-py](https://github.com/opensearch-project/opensearch-py)
