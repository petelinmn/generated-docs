---
title: "Квиз: управление потоком и функции"
lang: ru
track: python
kind: module
doc: quiz
slug: 03-control-flow-and-functions
part: 1
tags: [control-flow, loops, functions, scopes, args, kwargs]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Квиз: управление потоком и функции

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ.

## 1. 🟢 Что выведет этот цикл?

```python
for number in range(3):
    print(number)
```

- A) `1 2 3`
- B) `0 1 2`
- C) `0 1 2 3`
- D) Ничего

<details><summary>Ответ</summary>

**B.** `range(3)` содержит 0, 1 и 2.

</details>

## 2. 🟢 Какое значение возвращает функция без `return`?

- A) `0`
- B) `False`
- C) `None`
- D) Пустая строка

<details><summary>Ответ</summary>

**C.** Функция без явного `return` возвращает `None`.

</details>

## 3. 🟢 Для чего нужен `break`?

- A) Завершить программу
- B) Завершить ближайший цикл
- C) Пропустить итерацию
- D) Завершить функцию

<details><summary>Ответ</summary>

**B.** `continue` пропускает итерацию, а `break` выходит из ближайшего цикла.

</details>

## 4. 🟢 Что собирает `*args`?

- A) Именованные аргументы
- B) Позиционные аргументы в кортеж
- C) Глобальные переменные
- D) Ключи словаря

<details><summary>Ответ</summary>

**B.** Extra positional arguments собраны в кортеж `args`.

</details>

## 5. 🟡 Что выведет этот код?

```python
def add_item(item, items=[]):
    items.append(item)
    return items

print(add_item("a"))
print(add_item("b"))
```

- A) `['a']`, `['b']`
- B) `['a']`, `['a', 'b']`
- C) `[]`, `['b']`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** Mutable default является общим объектом для всех вызовов.

</details>

## 6. 🟡 Что делает `nonlocal`?

- A) Создает глобальную переменную
- B) Связывает имя с переменной enclosing-функции
- C) Удаляет переменную
- D) Копирует локальный словарь

<details><summary>Ответ</summary>

**B.** `nonlocal` разрешает присваивание переменной внешней функции.

</details>

## 7. 🟡 Что выведет match?

```python
value = ("stop",)
match value:
    case ("move", distance):
        print(distance)
    case ("stop",):
        print("stopped")
```

- A) `1`
- B) `stopped`
- C) `stop`
- D) Ничего

<details><summary>Ответ</summary>

**B.** Кортеж имеет ровно нужную структуру для второго образца.

</details>

## 8. 🟡 Выберите безопасную альтернативу mutable default

- [ ] A) `items=[]`
- [ ] B) `items=None`
- [ ] C) Создать `[]` внутри функции при `None`
- [ ] D) Глобальный список

<details><summary>Ответ</summary>

**B, C.** `None` не является изменяемым объектом; локальный список создается при вызове.

</details>

## 9. 🔴 Найдите ошибку

```python
functions = [lambda: value for value in range(3)]
print([function() for function in functions])
```

<details><summary>Ответ</summary>

Все lambda читают одну последнюю loop-переменную, поэтому результат `[2, 2, 2]`. Передайте значение по default:

```python
functions = [lambda value=value: value for value in range(3)]
```

</details>

## 10. 🔴 Открытый вопрос

Назовите порядок поиска имен в Python и объясните роль замыкания.

<details><summary>Ключевые пункты ответа</summary>

- LEGB: local, enclosing, global, builtins.
- Замыкание хранит ссылку на ячейки внешней области после завершения фабрики функции.
- Дальнейшее использование должно учитывать позднее связывание.

</details>
