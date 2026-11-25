---
title: "Управление потоком и функции"
lang: ru
track: python
kind: module
doc: overview
slug: 03-control-flow-and-functions
part: 1
levels: [1, 2, 3, 4]
tags: [control-flow, if, match, loops, functions, args, kwargs, scopes, closure]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Управление потоком и функции

🌐 **Русский** · [English](overview.en.md)

## Коротко

Управление потоком выбирает ветки и повторяет действия с помощью `if`, `match`, `for` и `while`. Функции упаковывают код в именованные блоки с параметрами и возвращаемым значением. Позиционные и именованные аргументы, параметры с `*args` и `**kwargs`, а также области видимости определяют передачу данных.

## Ключевые идеи

- **`if/elif/else` выполняют ровно одну подходящую ветку.**
- **`match` выбирает ветку по структуре значения; он появился в Python 3.10.**
- **`for` перебирает элементы итераторов и коллекций, а `while` — пока условие истинно.**
- **`break` завершает цикл, `continue` переходит к следующей итерации.**
- **Функция вызывается с аргументами; `return` передает один объект результата.**
- **По умолчанию аргументы передаются ссылками на объекты.**
- **Имена в функциях следуют правилу LEGB; замыкание сохраняет доступ к внешней области.**

## Минимальный пример

```python
def classify(score):
    if score >= 90:
        return "A"
    if score >= 75:
        return "B"
    return "C"

print(classify(92))
print(classify(80))
```

```text
A
B
```

## Когда применять

- ✅ `if/elif/else` для последовательной проверки условий.
- ✅ `match` для структуры и типов данных.
- ✅ `for` для известной коллекции или диапазона.
- ✅ `while` для состояния, которое изменяется до выполнения условия остановки.
- ❌ Не изменяйте коллекцию в процессе прямого перебора без необходимости.
- ❌ Не используйте изменяемые значения как аргументы по умолчанию.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `if`, `match`, циклы, break/continue, функции, параметры |
| [2. Продвинутая теория](advanced.ru.md) | `*args`, `**kwargs`, scopes, closures, lambdas, best practices |
| [3. Экспертная теория](expert.ru.md) | справочник вызовов, LEGB, frame internals, pitfalls, interview questions |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn python 03-control-flow-and-functions level 5` |
| 6. Тесты | ещё не создан — `/learn python 03-control-flow-and-functions level 6` |

## Связанные материалы

- Пререквизиты: [02 · Типы и структуры данных](../02-types-and-data-structures/overview.ru.md).
- Дальше: следующий модуль по [дорожной карте трека Python](../../overview.ru.md).
- Аналог в JavaScript: [03 · Variables and scopes](../../../javascript/modules/03-scopes-and-hoisting/overview.ru.md).
- Аналог в C#: [03 · Control flow and methods](../../../csharp/modules/03-control-flow-and-methods/overview.ru.md).

## Источники

- [Python control flow](https://docs.python.org/3/tutorial/controlflow.html)
- [Python compound statements](https://docs.python.org/3/reference/compound_stmts.html)
- [Python data model: execution model](https://docs.python.org/3/reference/executionmodel.html)
