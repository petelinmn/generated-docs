---
title: "Управление потоком и функции: продвинутая теория"
lang: ru
track: python
kind: module
doc: advanced
slug: 03-control-flow-and-functions
part: 1
tags: [args, kwargs, scopes, closure, lambda, default-arguments]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Управление потоком и функции: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md).

## `*args` и `**kwargs`

```python
def summarize(label, *values, **options):
    print(label, values, options)

summarize("result", 10, 20, unit="items")
```

```text
result (10, 20) {'unit': 'items'}
```

`*args` собирает лишние позиционные аргументы в кортеж, а `**kwargs` — именованные аргументы в словарь.

## Значения по умолчанию

Значения по умолчанию вычисляются в момент определения функции:

```python
def add_item(item, items=[]):
    items.append(item)
    return items

print(add_item("a"))
print(add_item("b"))
```

```text
['a']
['a', 'b']
```

Изменяемый объект используется совместно всеми вызовами. Для типичного поведения используйте `None`:

```python
def add_item(item, items=None):
    if items is None:
        items = []
    items.append(item)
    return items
```

## Области видимости и LEGB

Python ищет имя по цепочке: local, enclosing, global, builtins:

```python
count = "global"

def outer():
    count = "enclosing"
    def inner():
        count = "local"
        return count
    return inner()

print(count, outer())
```

```text
global local
```

`global` читает и изменяет модульную переменную; `nonlocal` изменяет переменную внешней функции. Обычно лучше передавать данные через параметры.

## Замыкание

```python
def make_counter(start=0):
    value = start
    def next_value():
        nonlocal value
        value += 1
        return value
    return next_value

counter = make_counter()
print(counter(), counter())
```

```text
1 2
```

## Lambda-функции

`lambda` создает короткую анонимную функцию:

```python
names = ["Ada", "Linus", "Grace"]
print(sorted(names, key=lambda name: len(name)))
```

```text
['Ada', 'Linus', 'Grace']
```

Для сложной логики предпочтительнее именованная функция.

## Лучшие практики

- Не используйте изменяемые аргументы по умолчанию.
- Явно выбирайте порядок параметров.
- Ограничивайте lambda одним выражением.
- Предпочитайте возврат данных перед изменением глобальных переменных.
- Используйте замыкания только там, где сохраняемое состояние действительно нужно.

## Частые ошибки

- ❌ Shared-list default для накопителя → ✅ `None` и локальная коллекция.
- ❌ Изменять глобальную переменную без необходимости → ✅ параметры и `return`.
- ❌ Смешивать позиционные и именованные аргументы некорректно → ✅ именованные после `*`.
- ❌ Считать `lambda` обязательным способом → ✅ именованная функция для сложной логики.

## Связанные материалы

- Базовая теория: [control flow and functions](basics.ru.md).
- Следующий модуль: по [дорожной карте трека Python](../../overview.ru.md).

## Источники

- [Python control flow](https://docs.python.org/3/tutorial/controlflow.html)
- [Python execution model](https://docs.python.org/3/reference/executionmodel.html)
