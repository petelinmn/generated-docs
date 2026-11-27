---
title: "Квиз: props и композиция"
lang: ru
track: react
kind: module
doc: quiz
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: props и композиция

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Кто может менять props компонента?

- A) Сам компонент
- B) Родитель, отрендерив с новыми значениями
- C) Любой сосед
- D) Никто и никогда

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что передаёт `<input disabled />`?

- A) `disabled={true}`
- B) `disabled=""`, ложное значение
- C) Ничего
- D) `disabled={undefined}`

<details><summary>Ответ</summary>

**A.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Куда попадает текст между `<Card>` и `</Card>`?

- A) `props.content`
- B) `props.children`
- C) `props.body`
- D) Отбрасывается

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Как потомок сообщает родителю о нажатии кнопки?

- A) Пишет в props
- B) Вызывает функцию из props
- C) Возвращает значение из рендера
- D) Через глобальную переменную

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Какую ширину получит `<Avatar name="E" size={null} />` при значении по умолчанию `size = 40`?

- A) 40
- B) Атрибута ширины не будет
- C) 0
- D) Ошибка при выполнении

<details><summary>Ответ</summary>

**B.** Значение по умолчанию действует только для `undefined`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какой `type` получит кнопка?

```tsx
<button type="button" {...{ type: 'submit' }} />
```

<details><summary>Ответ</summary>

`submit`: побеждает spread, записанный позже.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все хорошие причины передавать элементы через props

- [ ] A) Содержимое решает вызывающий
- [ ] B) Меньше настроечных флагов
- [ ] C) Компонент может свободно переписать содержимое
- [ ] D) Переданные элементы могут не требовать повторного рендера

<details><summary>Ответ</summary>

**A, B, D.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Найди ошибку

```tsx
function Field(props) {
  props.value = props.value.trim();
  return <input defaultValue={props.value} />;
}
```

<details><summary>Ответ</summary>

Props только для чтения; вычислите локальную переменную.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Как в React 19 функциональный компонент получает `ref`?

- A) Только через `forwardRef`
- B) Как обычный prop
- C) Никак
- D) Через context

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Обёртка с состоянием выводит `{children}`. Сколько раз выполнится `Slow` после двух обновлений обёртки, если он передан как дочерний?

- A) 1
- B) 2
- C) 3
- D) 0

<details><summary>Ответ</summary>

**A.** Только первый рендер: элемент остаётся тем же объектом.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что покажет это после того, как родитель сменит `name` с Ann на Bob?

```tsx
const [copy] = useState(name);
return <p>{copy}|{name}</p>;
```

<details><summary>Ответ</summary>

```text
Ann|Bob
```

Начальное значение состояния используется один раз.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: значение `theme` проходит через шесть компонентов, которым оно не нужно. Что сделать?

<details><summary>Ключевые пункты ответа</summary>

- Передавать сверху готовые элементы, чтобы средние слои лишь размещали `children`.
- Context для данных, действительно нужных во многих местах.
- Проверить, нельзя ли сделать дерево компонентов площе.

→ [Экспертная теория](expert.ru.md)

</details>
