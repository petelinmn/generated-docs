---
title: "Жизненный цикл индексов: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Жизненный цикл индексов: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Действия ISM

Список из настройки `plugins.index_state_management.allow_list` и документации:

| Группа | Действия |
|---|---|
| Смена индекса | `rollover` |
| Запись и доступ | `read_only`, `read_write`, `open`, `close`, `search_only` |
| Уплотнение | `force_merge`, `shrink`, `rollup` |
| Размещение | `allocation`, `replica_count`, `index_priority` |
| Сохранность и удаление | `snapshot`, `delete`, `convert_index_to_remote` |
| Прочее | `alias`, `notification` |

Здесь запускались `rollover`, `read_only`, `force_merge`, `delete`.

## Повторы действий

Сохранённая политика получает у каждого действия значения по умолчанию: `retry` с `count: 3`, `backoff: exponential`, `delay: 1m`. Состояние failed лечится `_plugins/_ism/retry/<index>`; для индекса не в этом состоянии вызов отвечает `This index is not in failed state`.

## Условия переходов и rollover

| Где | Условия |
|---|---|
| `transitions.conditions` | `min_index_age`, `min_rollover_age`, `min_state_age`, `min_doc_count`, `min_size`, `cron` |
| `rollover` | `min_index_age`, `min_doc_count`, `min_size`, `min_primary_shard_size` |

## Настройки

| Настройка (`plugins.index_state_management.`) | По умолчанию |
|---|---|
| `enabled` | `true` |
| `job_interval` | `5` (минут) |
| `jitter` | `0.6` |
| `coordinator.sweep_period` | `10m` |
| `history.enabled` | `true` |
| `history.rollover_retention_period` | `30d` |

## Как устроено

Политики лежат в системном индексе `.opendistro-ism-config` (это видно в ответе на удаление), задания запускает плагин job scheduler. Имена `opendistro.*` — прежние псевдонимы настроек `plugins.*`.

## Подвохи

1. Удаление по `_id` через имя потока отклоняется с тем же сообщением, что и перезапись: разрешён только `create`.
2. Поздний документ с давней датой попадает в текущий индекс, поэтому удаление backing-индекса по возрасту удалит и «свежее» по времени прихода, а не по `@timestamp`.
3. Пустой индекс после rollover по `min_doc_count` ждёт: в `explain` стоит `Pending rollover`.
4. `ism_template` с шаблоном `logs-web*` захватил и скрытый `.ds-logs-web-000001` (в прогоне были заданы оба шаблона имени).
5. Повторный `PUT` политики без `if_seq_no` и `if_primary_term` даёт 409.
6. `_reindex` в поток без `op_type: create` завершается ошибкой 400.

> ⚠️ Проверить: действия `shrink`, `rollup`, `snapshot`, `allocation`, уведомления, условия `cron`, `min_state_age`, `min_rollover_age` и `min_size`, а также захват backing-индексов шаблоном `logs-web*` без явного `.ds-` в `ism_template` не запускались.

## Вопросы с собеседований

1. Почему удалять индексами лучше, чем по запросу?
<details><summary>Ответ</summary>

Удаление индекса сразу освобождает место; удаление документов лишь помечает их до слияния сегментов.
</details>

2. Что такое data stream?
<details><summary>Ответ</summary>

Имя над backing-индексами: запись в последний, чтение из всех, обязательный `@timestamp`.
</details>

3. Data stream или псевдоним с rollover?
<details><summary>Ответ</summary>

Поток для добавляемых событий: меньше настроек. Псевдоним, если нужны изменения по `_id` в текущем индексе или нет поля времени.
</details>

4. Из чего состоит ISM-политика?
<details><summary>Ответ</summary>

Начальное состояние, состояния с действиями и переходами, опционально `ism_template` и уведомление об ошибке.
</details>

5. Почему политика не сработала сразу?
<details><summary>Ответ</summary>

Задание выполняется по расписанию (5 минут плюс jitter), шаг за запуск.
</details>

6. Как посмотреть, где индекс?
<details><summary>Ответ</summary>

`_plugins/_ism/explain/<index>`: состояние, действие, сообщение.
</details>

7. Изменили политику — что с текущими индексами?
<details><summary>Ответ</summary>

Они держат прежнюю версию; нужен `change_policy`.
</details>

8. `min_index_age` против `min_state_age` и `min_rollover_age`?
<details><summary>Ответ</summary>

Возраст с создания индекса, время в состоянии, время после rollover.
</details>

9. Что даёт `force_merge`?
<details><summary>Ответ</summary>

Меньше сегментов у индекса, в который уже не пишут: меньше накладных расходов и очистка удалённых.
</details>

10. Что делать с зависшим действием?
<details><summary>Ответ</summary>

Прочитать сообщение в `explain`, устранить причину; для состояния failed — `_plugins/_ism/retry`.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [04 · Агрегации по времени](../04-aggregations/advanced.ru.md)

## Источники

- [ISM API](https://docs.opensearch.org/latest/im-plugin/ism/api/)
- [Policies: actions and transitions](https://docs.opensearch.org/latest/im-plugin/ism/policies/)
