---
title: "Упражнения: метапрограммирование"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 18-metaprogramming
part: 3
tags: [symbol, proxy, reflect, to-primitive, tagged-template]
status: draft
updated: 2026-11-23
---

# Упражнения: метапрограммирование

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Код запускается в файле ES-модуля (`package.json` с `{"type": "module"}`).

## 1. 🟢 Скрытый идентификатор

**Задача:** добавьте объекту `{ name: 'Ann' }` скрытое поле `id` со значением `42` так, чтобы оно не попадало в `Object.keys` и `JSON.stringify`, но читалось по ключу.

**Ожидаемый результат:** `[ 'name' ] {"name":"Ann"} 42 2` (ключи, JSON, значение, число всех собственных ключей).

<details><summary>Подсказка</summary>

Ключ-символ: `const ID = Symbol('id')`; `{ [ID]: 42 }`; число ключей — `Reflect.ownKeys`.

</details>

<details><summary>Решение</summary>

```js
const ID = Symbol('id'); const user = { name: 'Ann', [ID]: 42 };
console.log(Object.keys(user), JSON.stringify(user), user[ID], Reflect.ownKeys(user).length);
```

```text
[ 'name' ] {"name":"Ann"} 42 2
```

</details>

## 2. 🟢 Счётчик чтений

**Задача:** оберните `{ a: 1, b: 2 }` в `Proxy`, который считает, сколько раз читалось каждое свойство. Прочитайте `a` дважды и `b` один раз.

**Ожидаемый результат:** `{ a: 2, b: 1 }`.

<details><summary>Подсказка</summary>

Ловушка `get` и `Reflect.get(t, k, r)`.

</details>

<details><summary>Решение</summary>

```js
const counts = {};
const watched = new Proxy({ a: 1, b: 2 }, { get(t, k, r) { counts[k] = (counts[k] ?? 0) + 1; return Reflect.get(t, k, r); } });
watched.a; watched.a; watched.b; console.log(counts);
```

```text
{ a: 2, b: 1 }
```

</details>

## 3. 🟡 Деньги с `Symbol.toPrimitive`

**Задача:** класс `Money(cents, currency)`: в строковом контексте даёт `19.99 USD`, в числовом — число центов. Проверьте `` `${m}` ``, `m + 1`, `m > 1000` и `String(m)` для `1999` центов.

**Ожидаемый результат:** `19.99 USD 2000 true 19.99 USD`.

<details><summary>Подсказка</summary>

Метод `[Symbol.toPrimitive](hint)`: при `hint === 'string'` вернуть строку, иначе число.

</details>

<details><summary>Решение</summary>

```js
class Money { constructor(c, cur) { this.c = c; this.cur = cur } [Symbol.toPrimitive](hint) { return hint === 'string' ? `${(this.c / 100).toFixed(2)} ${this.cur}` : this.c } }
const m = new Money(1999, 'USD'); console.log(`${m}`, m + 1, m > 1000, String(m));
```

```text
19.99 USD 2000 true 19.99 USD
```

</details>

## 4. 🟡 Представление только для чтения

**Задача:** функция `readonly(obj)` возвращает прокси, который при записи и удалении свойства бросает `TypeError('read-only')`, но читает значения.

**Ожидаемый результат:** `80`, `read-only`, `read-only`.

<details><summary>Подсказка</summary>

Ловушки `set` и `deleteProperty`, в которых нужно бросить ошибку.

</details>

<details><summary>Решение</summary>

```js
const readonly = obj => new Proxy(obj, { set() { throw new TypeError('read-only') }, deleteProperty() { throw new TypeError('read-only') } });
const cfg = readonly({ port: 80 }); console.log(cfg.port);
try { cfg.port = 1 } catch (e) { console.log(e.message) }
try { delete cfg.port } catch (e) { console.log(e.message) }
```

```text
80
read-only
read-only
```

Представление неглубокое: вложенные объекты остаются изменяемыми.

</details>

## 5. 🔴 Безопасный SQL-запрос

**Задача:** теговая функция `sql` превращает шаблон в `{ text, values }`: вместо подстановок ставит `$1`, `$2`, …, а значения кладёт в массив (защита от внедрения SQL). Проверьте на `id = 7` и вредоносной строке.

**Ожидаемый результат:** `{ text: 'SELECT * FROM users WHERE id = $1 AND name = $2', values: [ 7, "x'; DROP TABLE users;--" ] }`.

<details><summary>Подсказка</summary>

`strings.reduce((acc, s, i) => acc + '$' + i + s)` — без начального значения первым берётся `strings[0]`, а индекс начинается с `1`.

</details>

<details><summary>Решение</summary>

```js
const sql = (strings, ...values) => ({ text: strings.reduce((acc, s, i) => acc + '$' + i + s), values });
const id = 7, name = "x'; DROP TABLE users;--";
console.log(sql`SELECT * FROM users WHERE id = ${id} AND name = ${name}`);
```

```text
{
  text: 'SELECT * FROM users WHERE id = $1 AND name = $2',
  values: [ 7, "x'; DROP TABLE users;--" ]
}
```

Значения не попадают в текст запроса: драйвер БД передаст их отдельно ([модуль 20](../../overview.ru.md)).

</details>
