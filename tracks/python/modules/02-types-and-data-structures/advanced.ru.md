---
title: "Типы и структуры данных: продвинутая теория"
lang: ru
track: python
kind: module
doc: advanced
slug: 02-types-and-data-structures
part: 1
tags: [mutability, shallow-copy, deep-copy, identity, equality, hashable]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Типы и структуры данных: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md).

## Изменяемость и алиасы

Алиас — вторая ссылка на тот же объект:

```python
original = [1, 2]
alias = original
alias.append(3)
print(original)
```

```text
[1, 2, 3]
```

Чтобы передавать изменяемые данные без побочных изменений, создавайте копию, которая подходит задаче.

## Неглубокая и глубокая копия

```python
from copy import deepcopy

source = {"items": [1, 2]}
shallow = source.copy()
deep = deepcopy(source)

shallow["items"].append(3)
print(source, deep)
```

```text
{'items': [1, 2, 3]} {'items': [1, 2]}
```

`copy()` у словаря копирует только верхний уровень. `deepcopy()` рекурсивно копирует вложенные изменяемые объекты.

Для списков срез `items[:]` создаёт неглубокую копию.

## Сравнение и хеширование

```python
print([1, 2] == [1, 2])
print([1, 2] is [1, 2])
print(hash("python"))
try:
    hash([1, 2])
except TypeError as error:
    print(type(error).__name__)
```

```text
True
False
<hash differs>
TypeError
```

`hash("python")` зависит от среды и randomization; конкретный результат не фиксируется. Не считайте hash стабильным между процессами.

Изменяемое содержимое не должно быть ключом словаря или элементом множества.

## Числа и точность

```python
print(0.1 + 0.2)
print(0.1 + 0.2 == 0.3)

from decimal import Decimal
print(Decimal("0.1") + Decimal("0.2"))
```

```text
0.30000000000000004
False
0.3
```

Для денег используйте `Decimal` или целые минимальные единицы. Дроби в `float` хранятся приближённо.

## Лучшие практики

- выбирайте неизменяемые записи для значений, которые не должны меняться;
- используйте `copy()` или `deepcopy()` осознанно;
- проверяйте `is` только для идентичности или одиночных объектов вроде `None`;
- используйте `Decimal` для точной десятичной арифметики;
- не полагайтесь на порядок элементов в `set`;
- не создавайте словари с изменяемыми ключами.

## Частые ошибки

- ❌ Считать срез глубокой копией → ✅ `copy.deepcopy` для вложенных объектов.
- ❌ Сравнивать словари через `is` → ✅ `==`.
- ❌ Использовать float для денег → ✅ `Decimal` или целые копейки.
- ❌ Использовать изменяемый объект в множестве → ✅ неизменяемые хешируемые значения.
- ❌ Считать строковое представление хеша постоянным → ✅ использовать хеш только внутри процесса механизма контейнеров.

## Связанные материалы

- Базовая теория: [типы и структуры данных](basics.ru.md).
- Экспертная теория: [expert](expert.ru.md).
- Сравнение с JavaScript: [Типы и значения](../../../javascript/modules/02-types-and-values/overview.ru.md).

## Источники

- [Python copy module](https://docs.python.org/3/library/copy.html)
- [Python numeric types](https://docs.python.org/3/library/stdtypes.html#numeric-types-int-float-complex)
- [decimal module](https://docs.python.org/3/library/decimal.html)
