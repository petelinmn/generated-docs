---
title: "Упражнения: управление потоком и функции"
lang: ru
track: python
kind: module
doc: exercises
slug: 03-control-flow-and-functions
part: 1
tags: [control-flow, loops, functions, args, kwargs]
status: draft
updated: 2026-11-25
---

# Упражнения: управление потоком и функции

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно до открытия подсказки.

## 1. 🟢 Классификация числа

**Задача:** напишите `classify(n)`, которая возвращает `"positive"`, `"zero"` или `"negative"`.

**Ожидаемый результат:** `classify(4) -> positive`, `classify(0) -> zero`, `classify(-2) -> negative`.

<details><summary>Подсказка</summary>

Проверьте `n > 0`, `n == 0`, затем оставшийся вариант.

</details>

<details><summary>Решение</summary>

```python
def classify(n):
    if n > 0:
        return "positive"
    if n == 0:
        return "zero"
    return "negative"

print(classify(4), classify(0), classify(-2))
```

</details>

## 2. 🟢 Сумма чисел

**Задача:** напишите `sum_to(n)` для суммы всех целых от 1 до `n` включительно.

**Ожидаемый результат:** `sum_to(5) == 15`.

<details><summary>Подсказка</summary>

Используйте `range(1, n + 1)` и накопитель.

</details>

<details><summary>Решение</summary>

```python
def sum_to(n):
    total = 0
    for number in range(1, n + 1):
        total += number
    return total

print(sum_to(5))
```

</details>

## 3. 🟡 Безопасный default

**Задача:** напишите `append_item(value, items=None)`, которая не разделяет state между вызовами.

**Ожидаемый результат:** два вызова изначально дают `['a']` и `['b']`.

<details><summary>Подсказка</summary>

Создавайте локальный список в ветке `items is None`.

</details>

<details><summary>Решение</summary>

```python
def append_item(value, items=None):
    if items is None:
        items = []
    items.append(value)
    return items

print(append_item("a"))
print(append_item("b"))
```

</details>

## 4. 🟡 Схема match

**Задача:** напишите `describe_command(command)`, которая распознает кортежи `("move", n)`, `("stop",)` и всё прочее.

**Ожидаемый результат:** `move 3`, `stop`, `unknown`.

<details><summary>Подсказка</summary>

Используйте `match/case` и wildcard `_`.

</details>

<details><summary>Решение</summary>

```python
def describe_command(command):
    match command:
        case ("move", distance):
            return f"move {distance}"
        case ("stop",):
            return "stop"
        case _:
            return "unknown"

print(describe_command(("move", 3)))
print(describe_command(("stop",)))
print(describe_command(("dance",)))
```

</details>

## 5. 🔴 Захват значения в замыкании

**Задача:** создайте три функции с номерами 0, 1 и 2 и проверьте их вызов.

**Ожидаемый результат:** `[0, 1, 2]`.

<details><summary>Подсказка</summary>

Передайте loop-переменную через default-параметр lambda.

</details>

<details><summary>Решение</summary>

```python
functions = [lambda value=value: value for value in range(3)]
print([function() for function in functions])
```

</details>
