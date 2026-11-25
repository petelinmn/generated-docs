---
title: "Упражнения: типы и структуры данных"
lang: ru
track: python
kind: module
doc: exercises
slug: 02-types-and-data-structures
part: 1
tags: [types, list, tuple, dict, set, copy]
status: draft
updated: 2026-11-25
---

# Упражнения: типы и структуры данных

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно до открытия подсказки.

## 1. 🟢 Определите тип

**Задача:** напишите цикл для вывода типа каждого элемента списка.

**Ожидаемый результат:** строки `42:int`, `3.14:float`, `hello:str`, `True:bool`, `None:NoneType`.

<details><summary>Подсказка</summary>

Используйте `type(value).__name__`.

</details>

<details><summary>Решение</summary>

```python
values = [42, 3.14, "hello", True, None]
for value in values:
    print(f"{value!r}:{type(value).__name__}")
```

</details>

## 2. 🟢 Скопируйте и измените список

**Задача:** создайте копию `source`, добавьте в копию элемент и проверьте исходный список.

**Ожидаемый результат:** исходник не меняется.

<details><summary>Подсказка</summary>

Для плоского списка подходит `source.copy()` или срез.

</details>

<details><summary>Решение</summary>

```python
source = [1, 2]
copy = source.copy()
copy.append(3)
print(source)
print(copy)
```

</details>

## 3. 🟡 Маппинг и множественные ключи

**Задача:** создайте словарь, который показывает количество каждой строки в списке.

**Ожидаемый результат:** `{"a": 2, "b": 1}`.

<details><summary>Подсказка</summary>

Используйте `dict.get(item, 0) + 1`.

</details>

<details><summary>Решение</summary>

```python
items = ["a", "b", "a"]
counts = {}
for item in items:
    counts[item] = counts.get(item, 0) + 1
print(counts)
```

</details>

## 4. 🟡 Неглубокая копия

**Задача:** покажите, что неглубокая копия словаря сохраняет ссылку на вложенный список.

**Ожидаемый результат:** исходный вложенный список изменяется вместе с копией.

<details><summary>Подсказка</summary>

Используйте `source.copy()` и измените вложенный объект через копию.

</details>

<details><summary>Решение</summary>

```python
source = {"items": [1]}
copy = source.copy()
copy["items"].append(2)
print(source)
print(copy)
```

</details>

## 5. 🔴 Deep copy

**Задача:** создайте независимую глубокую копию вложенного словаря.

**Ожидаемый результат:** изменение копии не меняет исходник.

<details><summary>Подсказка</summary>

Импортируйте `deepcopy`.

</details>

<details><summary>Решение</summary>

```python
from copy import deepcopy

source = {"items": [1]}
copy = deepcopy(source)
copy["items"].append(2)
print(source)
print(copy)
```

</details>
