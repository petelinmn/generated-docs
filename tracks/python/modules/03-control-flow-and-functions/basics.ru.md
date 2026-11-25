---
title: "Управление потоком и функции: базовая теория"
lang: ru
track: python
kind: module
doc: basics
slug: 03-control-flow-and-functions
part: 1
tags: [control-flow, if, match, loops, functions, args, kwargs]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Управление потоком и функции: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Типы и структуры данных](../02-types-and-data-structures/overview.ru.md) · **Время:** ~2 ч

## Зачем это нужно

Без ветвления и повторения программы только выполняют последовательность операторов. Функции позволяют переиспользовать эту последовательность и скрыть детали реализации.

## `if`, `elif`, `else`

```python
temperature = 18
if temperature < 10:
    print("cold")
elif temperature < 25:
    print("comfortable")
else:
    print("hot")
```

```text
comfortable
```

Python вычисляет ветки сверху вниз и прекращает проверку после первого истинного условия.

## Сопоставление с образцом: `match`

```python
command = ("move", 3)
match command:
    case ("move", distance):
        print("move", distance)
    case ("stop",):
        print("stop")
    case _:
        print("unknown")
```

```text
move 3
```

`case _` является wildcard-образцом и похож на последний `else`.

## Цикл `for`

```python
total = 0
for number in range(1, 5):
    total += number
print(total)
```

```text
10
```

`range(1, 5)` включает 1, 2, 3 и 4, но не 5.

Для словаря можно перебирать ключи, пары ключ–значение или значения:

```python
for key, value in {"a": 1, "b": 2}.items():
    print(key, value)
```

## Цикл `while`

```python
count = 3
while count > 0:
    print(count)
    count -= 1
```

```text
3
2
1
```

Условие проверяется до каждой итерации. Необходимо изменять состояние, чтобы цикл не стал бесконечным.

## `break` и `continue`

```python
for number in range(6):
    if number == 2:
        continue
    if number == 4:
        break
    print(number)
```

```text
0
1
3
```

`continue` пропускает остаток итерации, а `break` завершает ближайший цикл.

## Функции

```python
def add(left, right):
    return left + right

print(add(2, 3))
```

```text
5
```

Параметры определяются в объявлении, аргументы передаются при вызове. Если нет `return`, функция возвращает `None`.

## Позиционные и именованные аргументы

```python
def format_name(first, last, title=""):
    return f"{title} {first} {last}".strip()

print(format_name("Ada", "Lovelace"))
print(format_name(last="Lovelace", first="Ada", title="Dr"))
```

```text
Ada Lovelace
Dr Ada Lovelace
```

## Ошибки новичка

- ❌ Сравнивать через `&&` или `||` → ✅ использовать `and` и `or`.
- ❌ Забыть двоеточие или отступ → ✅ проверять Python-блоки синтаксически.
- ❌ Изменять список во время прямого перебора → ✅ итерация по копии или выборка новых значений.
- ❌ Бесконечный `while` без изменения состояния → ✅ явно приближать состояние к stop-условию.
- ❌ Ожидать, что функция напечатает и вернет значение одновременно → ✅ отдельно использовать `print` и `return`.

## Связанные материалы

- Пререквизиты: [02 · Типы и структуры данных](../02-types-and-data-structures/overview.ru.md).
- Дальше: [продвинутая теория](advanced.ru.md).

## Источники

- [Python control flow tutorial](https://docs.python.org/3/tutorial/controlflow.html)
- [Python compound statements](https://docs.python.org/3/reference/compound_stmts.html)
