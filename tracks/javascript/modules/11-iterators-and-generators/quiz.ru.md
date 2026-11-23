---
title: "Квиз: итераторы и генераторы"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 11-iterators-and-generators
part: 2
tags: [iterator, generator, yield, async-iterator, iterator-helpers]
status: draft
updated: 2026-11-23
---

# Квиз: итераторы и генераторы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что нужно объекту, чтобы по нему работал `for...of`?

- A) Свойство `length`
- B) Метод `[Symbol.iterator]()`, возвращающий итератор
- C) Метод `forEach`
- D) Быть массивом

<details><summary>Ответ</summary>

**B.** Это определение итерируемого объекта. Обычный объект `{}` без такого метода — `TypeError: {} is not iterable`.
→ [Базовая теория](basics.ru.md#протокол-итерации)

</details>

## 2. 🟢 Что вернёт `it.next()` после конца последовательности?

- A) `undefined`
- B) `{ value: undefined, done: true }`
- C) `null`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** Конец сигнализируется флагом `done: true`.
→ [Базовая теория](basics.ru.md#протокол-итерации)

</details>

## 3. 🟢 Что выведет код?

```js
function* gen() { const x = yield 1; return x; }
const g = gen();
console.log(g.next('a').value, g.next('b').value);
```

- A) `a b`
- B) `1 b`
- C) `1 a`
- D) `undefined b`

<details><summary>Ответ</summary>

**B.** Аргумент первого `next` отбрасывается; `next('b')` становится результатом `yield 1`, а затем возвращается через `return`.
→ [Базовая теория](basics.ru.md#генераторы)

</details>

## 4. 🟢 Что выведет код?

```js
function* r() { yield 1; return 99; }
console.log([...r()]);
```

- A) `[ 1, 99 ]`
- B) `[ 1 ]`
- C) `[ 99 ]`
- D) `[]`

<details><summary>Ответ</summary>

**B.** Spread игнорирует значение `return`.
→ [Базовая теория](basics.ru.md#ленивость-и-бесконечные-последовательности)

</details>

## 5. 🟢 Что даст повторный `[...it]` для `it = [1, 2, 3][Symbol.iterator]()`?

- A) `[ 1, 2, 3 ]`
- B) `[]`
- C) `[ 1 ]`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** Итератор израсходован; новый перебор требует нового итератора.
→ [Подвох 3](expert.ru.md#3-итератор-потребляется)

</details>

## 6. 🟡 Что выведет код?

```js
function* inner() { yield 'a'; return 'ret'; }
function* outer() { const r = yield* inner(); yield r; }
console.log([...outer()]);
```

- A) `[ 'a' ]`
- B) `[ 'a', 'ret' ]`
- C) `[ 'ret' ]`
- D) `[ 'a', undefined ]`

<details><summary>Ответ</summary>

**B.** Результат `yield*` — значение `return` вложенного генератора.
→ [Продвинутая теория](advanced.ru.md#yield-делегирование)

</details>

## 7. 🟡 Сколько раз вызовется `return()` итератора?

```js
for (const x of withReturn) { if (x === 1) break; }
const [first] = withReturn;
```

(`withReturn` — бесконечный итерируемый объект с методом `return()`.)

- A) `0`
- B) `1`
- C) `2`
- D) Бесконечно

<details><summary>Ответ</summary>

**C.** И досрочный `break`, и неполная деструктуризация закрывают итератор.
→ [Продвинутая теория](advanced.ru.md#закрытие-итераторов)

</details>

## 8. 🟡 Что выведет код?

```js
const log = []; const lazy = [1, 2, 3].values().map(x => { log.push(x); return x; }).take(2);
console.log(log);
```

- A) `[ 1, 2, 3 ]`
- B) `[ 1, 2 ]`
- C) `[]`
- D) `[ 1 ]`

<details><summary>Ответ</summary>

**C.** Хелперы итераторов ленивы: без потребителя функции не вызываются.
→ [Продвинутая теория](advanced.ru.md#хелперы-итераторов)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `for await` может перебирать и обычные итерируемые объекты
- [ ] B) `for await` запускает итерации параллельно
- [ ] C) Ошибка в `async function*` перехватывается `try/catch` вокруг `for await`
- [ ] D) `break` в `for await` вызывает `return()` итератора

<details><summary>Ответ</summary>

**A, C, D.** B неверно: `for await` последователен.
→ [Продвинутая теория](advanced.ru.md#асинхронные-итераторы-и-генераторы)

</details>

## 10. 🔴 Найдите ошибку

> «`new gen()` создаёт новый генератор, если `gen` объявлена как `function* gen() {}`».

<details><summary>Ответ</summary>

Генераторы нельзя вызывать с `new`: `TypeError: gen is not a constructor`. Генератор создаётся обычным вызовом `gen()`.
→ [Подвох 5](expert.ru.md#5-new-для-генератора)

</details>

## 11. 🔴 Что выведет код?

```js
function* y2() { const r = yield 1 + 1; console.log('r', r); }
const yy = y2(); console.log(yy.next().value); yy.next('v');
```

- A) `1`, затем `r v`
- B) `2`, затем `r v`
- C) `2`, затем `r undefined`
- D) `SyntaxError`

<details><summary>Ответ</summary>

**B.** У `yield` низкий приоритет: отдаётся `1 + 1` = `2`; `next('v')` становится результатом `yield`.
→ [Подвох 9](expert.ru.md#9-yield-без-скобок-в-выражении)

</details>

## 12. 🔴 Что выведет `[...'👨‍👩'].length`?

- A) `1`
- B) `2`
- C) `3`
- D) `6`

<details><summary>Ответ</summary>

**C.** Строка итерируется по code points: мужчина, соединитель нулевой ширины, женщина.
→ [Подвох 7](expert.ru.md#7-строка-по-code-points)

</details>

## 13. 🔴 Открытый вопрос

Объясните, как работает генератор «под капотом» и почему он подходит для ленивых и бесконечных последовательностей.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Вызов `function*` не выполняет тело, а создаёт объект в состоянии suspendedStart.
- `next()` возобновляет выполнение до `yield`; локальные переменные и точка остановки сохраняются между вызовами (конечный автомат).
- Значения вычисляются по запросу, поэтому последовательность может быть бесконечной, а потребитель сам решает, где остановиться (`take`, `break`).
- Закрытие (`return()`, `finally`) освобождает ресурсы; прототип цепочки ведёт к `Iterator.prototype` с хелперами.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
