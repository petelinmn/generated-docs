---
title: "Квиз: браузер — DOM и события"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 17-browser-dom-and-events
part: 3
tags: [dom, events, fetch, local-storage, cors]
status: draft
updated: 2026-11-23
---

# Квиз: браузер — DOM и события

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что вернёт `document.querySelectorAll('.item')` после добавления в документ ещё одного `.item`?

- A) Обновлённый список с новым элементом
- B) Прежний статический список без нового элемента
- C) `null`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** `querySelectorAll` возвращает статический `NodeList`; `getElementsByClassName` — живой.
→ [Базовая теория](basics.ru.md#поиск-элементов)

</details>

## 2. 🟢 Что безопаснее для вставки пользовательского текста?

- A) `el.innerHTML = text`
- B) `el.textContent = text`
- C) `document.write(text)`
- D) `el.outerHTML = text`

<details><summary>Ответ</summary>

**B.** `textContent` вставляет текст, `innerHTML` разбирает разметку (риск XSS).
→ [Базовая теория](basics.ru.md#textcontent-и-innerhtml)

</details>

## 3. 🟢 В каком порядке сработают слушатели при клике на кнопку внутри `#inner` внутри `#outer`, если на `#outer` есть слушатель перехвата и всплытия, на `#inner` — всплытия?

- A) outer-bubble, inner-bubble, outer-capture
- B) outer-capture, inner-bubble, outer-bubble
- C) inner-bubble, outer-capture, outer-bubble
- D) outer-capture, outer-bubble, inner-bubble

<details><summary>Ответ</summary>

**B.** Сначала перехват сверху вниз, затем цель, затем всплытие снизу вверх.
→ [Базовая теория](basics.ru.md#события)

</details>

## 4. 🟢 Что вернёт `fetch('/missing')`, если сервер ответил `404`?

- A) Отклонённый промис
- B) Выполненный промис с `response.ok === false`
- C) `null`
- D) Исключение при вызове

<details><summary>Ответ</summary>

**B.** `fetch` отклоняется только на сетевые ошибки; проверяйте `response.ok`.
→ [Базовая теория](basics.ru.md#fetch)

</details>

## 5. 🟡 Что произойдёт при `localStorage.setItem('n', 5)` и `typeof localStorage.getItem('n')`?

- A) `'number'`
- B) `'string'`
- C) `'undefined'`
- D) Ошибка при записи

<details><summary>Ответ</summary>

**B.** `localStorage` хранит только строки.
→ [Базовая теория](basics.ru.md#хранилища)

</details>

## 6. 🟡 Что произойдёт после `input.value = 'x'`?

- A) Сработает `input`
- B) Сработает `change`
- C) Ни одно событие не сработает
- D) Сработают оба

<details><summary>Ответ</summary>

**C.** Присвоение из кода событий не порождает; их вызывают пользователь или `dispatchEvent`.
→ [Продвинутая теория](advanced.ru.md#события-ввода-и-фокуса)

</details>

## 7. 🟡 Для какого события делегирование через родителя не работает без перехвата?

- A) `click`
- B) `input`
- C) `focus`
- D) `keydown`

<details><summary>Ответ</summary>

**C.** `focus` не всплывает; используйте `focusin` или перехват.
→ [Продвинутая теория](advanced.ru.md#события-ввода-и-фокуса)

</details>

## 8. 🟡 Что делает `<script defer>…код…</script>` (встроенный, без `src`)?

- A) Откладывает выполнение до конца разбора
- B) Ничего: `defer` игнорируется, код выполняется сразу
- C) Выполняет код после `load`
- D) Ошибка разбора

<details><summary>Ответ</summary>

**B.** `defer` действует только у внешних скриптов; модульный скрипт откладывается сам.
→ [Продвинутая теория](advanced.ru.md#порядок-загрузки-скриптов)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `form.requestSubmit()` вызывает событие `submit`
- [ ] B) `form.submit()` вызывает событие `submit`
- [ ] C) `new FormData(form)` даёт значения-строки
- [ ] D) Снятый чекбокс попадает в `FormData` со значением `'off'`

<details><summary>Ответ</summary>

**A, C.** B неверно; D неверно: неотмеченный чекбокс в данные не входит.
→ [Продвинутая теория](advanced.ru.md#формы)

</details>

## 10. 🔴 Найдите ошибку

> «`fetch(url, { mode: 'no-cors' })` позволяет прочитать ответ чужого сайта без разрешения сервера».

<details><summary>Ответ</summary>

Ответ будет непрозрачным (`type: 'opaque'`, `status: 0`): прочитать его нельзя. `no-cors` не обходит CORS.
→ [Подвох 10](expert.ru.md#10-no-cors-не-обходит-cors)

</details>

## 11. 🔴 Что вернёт `document.querySelectorAll('my-card p')`, если `<p>` находится в Shadow DOM элемента `my-card`?

- A) Этот `<p>`
- B) Пустой список (`0`)
- C) Ошибку
- D) `null`

<details><summary>Ответ</summary>

**B.** Селекторы документа не проникают в тень; доступ — через `card.shadowRoot`.
→ [Продвинутая теория](advanced.ru.md#пользовательские-элементы-и-shadow-dom)

</details>

## 12. 🔴 Почему `removeEventListener('click', () => count++)` не работает?

- A) `removeEventListener` не существует
- B) Это другая функция: нужна та же ссылка (или `{ signal }`)
- C) Нужно указывать `capture`
- D) Клики нельзя снять

<details><summary>Ответ</summary>

**B.** Слушатель снимается только по ссылке на ту же функцию.
→ [Подвох 4](expert.ru.md#4-анонимная-функция-не-снимается)

</details>

## 13. 🔴 Открытый вопрос

Объясните, как построить список задач с добавлением, удалением и сохранением в `localStorage`: какие события и API вы используете и на что обращаете внимание в безопасности.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Один слушатель клика на списке с делегированием (`closest`), форма с `submit` и `preventDefault`/`requestSubmit`.
- Вставка текста через `textContent` (не `innerHTML`).
- Сохранение: `JSON.stringify` в `localStorage`, загрузка с `JSON.parse` и значением по умолчанию.
- Секреты в `localStorage` не хранить; ответы сервера проверять через `response.ok`.

</details>

→ [Базовая теория](basics.ru.md)
