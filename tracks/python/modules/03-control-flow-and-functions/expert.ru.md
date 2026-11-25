---
title: "Управление потоком и функции: экспертная теория"
lang: ru
track: python
kind: module
doc: expert
slug: 03-control-flow-and-functions
part: 1
tags: [execution-model, frames, LEGB, closures, recursion, call-signatures]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Управление потоком и функции: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [продвинутую теорию](advanced.ru.md).

## Справочник параметров

| Форма | Назначение |
|---|---|
| `def f(a, b)` | обязательные позиционные или именованные параметры |
| `def f(a=1)` | необязательный параметр с default |
| `def f(a, /, b)` | `a` только позиционный |
| `def f(*args)` | произвольные позиционные аргументы |
| `def f(a, *, b)` | `b` только именованный |
| `def f(**kwargs)` | произвольные именованные аргументы |

Параметры могут комбинироваться в общем порядке: positional-only, `/`, обычные, `*args`, keyword-only, `**kwargs`.

## Внутреннее устройство вызова

При вызове создается новый frame с локальным именованием и значениями параметров. Возврат удаляет обычный локальный frame, но замыкание сохраняет ячейки для нужных внешних переменных.

Python передает ссылку на объект. Внутри функции присваивание параметра локально не меняет переменную вызывающего кода.

## Оператор `return`

Функция возвращает ровно одно значение. Несколько объектов упаковываются в кортеж:

```python
def divide_pair(a, b):
    return divmod(a, b)

print(divide_pair(7, 3))
```

```text
(2, 1)
```

Без `return` результат равен `None`.

## Рекурсия

Рекурсивная функция вызывает себя со сходящимся базовым случаем:

```python
def factorial(n):
    if n <= 1:
        return 1
    return n * factorial(n - 1)

print(factorial(5))
```

```text
120
```

Глубокая рекурсия ограничена счетчиком рекурсии интерпретатора. Не используйте рекурсию для бесконечного обхода без явной базы.

## Подвохи

### 1. Shared default

```python
def append_default(value, items=[]):
    items.append(value)
    return items
print(append_default("a"))
print(append_default("b"))
```

Результат второй вызов содержит оба элемента.

### 2. `UnboundLocalError`

```python
value = 1
def change():
    print(value)
    value = 2
```

Вызов вызывает `UnboundLocalError`, потому что присваивание делает `value` локальным, а чтение идет до присваивания.

### 3. Позднее связывание замыкания

```python
functions = []
for value in range(3):
    functions.append(lambda: value)
print([function() for function in functions])
```

```text
[2, 2, 2]
```

Все lambda читают последнюю ячейку loop-переменной.

## Вопросы с собеседований

<details><summary>1. Передаются ли аргументы в Python по значению или по ссылке?</summary>

При передаче передается ссылка на объект. Это называют pass by object reference. Переменная вызывающего кода не заменяется при локальном присваивании параметра.

</details>

<details><summary>2. Где вычисляется default-параметр?</summary>

Во время определения функции, а не при каждом вызове.

</details>

<details><summary>3. Чем отличаются LEGB и локальные переменные?</summary>

LEGB описывает поиск имени local, enclosing, global, builtins.

</details>

<details><summary>4. Что делает `nonlocal`?</summary>

Он связывает присваивание с переменной ближайшей внешней функции и исключает создание новой локальной переменной.

</details>

<details><summary>5. Сколько значений возвращает функция?</summary>

Ровно один объект. Синтаксис с несколькими выражениями упаковывает их в кортеж.

</details>

<details><summary>6. Чем `for else` отличается от обычного else?</summary>

Clause выполняется после исчерпания итерации, но не при выходе через `break`.

</details>

<details><summary>7. Как устранить позднее связывание lambda?</summary>

Передать значение как default: `lambda value=value: value`.

</details>

<details><summary>8. Может ли цикл изменять список безопасно?</summary>

Безопаснее строить новый список или итерацию по копии, особенно при удалении элементов.

</details>

<details><summary>9. Когда рекурсия безопасна?</summary>

При сходящейся базе и глубине меньше ограничения интерпретатора.

</details>

<details><summary>10. Чем `match` полезнее простого `if`?</summary>

Он лучше выражает проверку структуры и распаковку полей. Для простой булевой ветки `if` яснее.

</details>

## Компромиссы и альтернативы

Используйте итеративные решения для больших глубин и рекурсии. Для callbacks применяйте именованные функции или замыкания с ясной областью.

## Связанные материалы

- Продвинутая теория: [управление потоком и функции](advanced.ru.md).
- Следующий модуль: по [дорожной карте трека Python](../../overview.ru.md).

## Источники

- [Python execution model](https://docs.python.org/3/reference/executionmodel.html)
- [Python compound statements](https://docs.python.org/3/reference/compound_stmts.html)
