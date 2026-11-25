---
title: "Типы и структуры данных: базовая теория"
lang: ru
track: python
kind: module
doc: basics
slug: 02-types-and-data-structures
part: 1
tags: [types, numbers, strings, list, tuple, dict, set, mutability]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Типы и структуры данных: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Окружение и инструменты](../01-environment/overview.ru.md) · **Время:** ~2 ч

## Зачем это нужно

Структуры данных определяют, как хранятся элементы, можно ли их менять и как быстро искать значения. Выбор типа уменьшает ошибки и делает код понятнее.

## Типы и идентичность

Каждое значение относится к типу:

```python
values = [42, 3.14, 1 + 2j, "hello", True, None]
for value in values:
    print(repr(value), type(value).__name__)
```

```text
42 int
3.14 float
(1+2j) complex
'hello' str
True bool
None NoneType
```

`True` и `False` относятся к `bool`, а отсутствие значения представляется объектом `None`.

## Числа и строки

```python
print(7 / 2, 7 // 2, 7 % 2, 2 ** 10)
text = "Python"
print(text.lower(), text.upper(), len(text), text[0], text[-1])
```

```text
3.5 3 1 1024
python PYTHON 6 P n
```

`/` выполняет вещественное деление, `//` — деление с отбрасыванием дробной части для положительных и отрицательных чисел с различающимся округлением к минус бесконечности, `%` даёт остаток.

Строки неизменяемы:

```python
text = "abc"
text = text.upper()
print(text)
```

## Список

`list` — упорядоченная изменяемая последовательность:

```python
items = [10, 20, 30]
items.append(40)
items.insert(1, 15)
print(items)
print(items[0], items[-1], items[1:3])
```

```text
[10, 15, 20, 30, 40]
10 40 [15, 20]
```

Срез `[1:3]` содержит индексы 1 и 2, но не 3.

## Кортеж

`tuple` — упорядоченная неизменяемая последовательность:

```python
point = (10, 20)
x, y = point
print(x, y)
print(point + (30,))
```

```text
10 20
(10, 20, 30)
```

Кортеж полезен для записи или результата с фиксированным набором полей.

## Словарь

`dict` хранит значения по ключам:

```python
user = {"name": "Ada", "age": 37}
print(user["name"])
print(user.get("email", "unknown"))
user["age"] += 1
print(user)
```

```text
Ada
unknown
{'name': 'Ada', 'age': 38}
```

Ключи должны быть хешируемыми: строки и кортежи из неизменяемых частей обычно подходят, а списки — нет.

## Множество

`set` хранит уникальные неупорядоченные элементы:

```python
numbers = {3, 1, 2, 2}
numbers.add(4)
print(numbers)
print({1, 2, 3} & {3, 4})
```

```text
{1, 2, 3, 4}
{3}
```

Порядок элементов множества не гарантируется.

## Значения и ссылки

```python
a = [1, 2]
b = a
b.append(3)
print(a, b, a is b)

c = a.copy()
c.append(4)
print(a, c, a is c)
```

```text
[1, 2, 3] [1, 2, 3] True
[1, 2, 3] [1, 2, 3, 4] False
```

Переменные хранят ссылку на объект. `is` проверяет ту же ссылку, а `==` — равенство значений.

## Ошибки новичка

- ❌ Использовать `==` для проверки на `None` → ✅ `value is None`.
- ❌ Проверять одинаковое ли содержимое через `is` → ✅ `==`.
- ❌ Создать ключ словаря из списка → ✅ преобразовать в кортеж или другую хешируемую структуру.
- ❌ Ожидать неизменности кортежа с изменяемым списком внутри → ✅ различать контейнер и вложенные объекты.
- ❌ Ожидать порядок списка в `set` → ✅ использовать `list` или словарь для порядка.

## Связанные материалы

- Пререквизиты: [01 · Окружение и инструменты](../01-environment/overview.ru.md).
- Дальше: [продвинутая теория](advanced.ru.md).
- Следующий модуль: [03 · Управление потоком и функции](../03-control-flow-and-functions/overview.ru.md).

## Источники

- [Python built-in types](https://docs.python.org/3/library/stdtypes.html)
- [Python lists](https://docs.python.org/3/tutorial/datastructures.html)
