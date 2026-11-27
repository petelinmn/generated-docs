---
title: "Формы: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 05-forms
part: 1
tags: [react, forms, controlled-components, validation, form-data]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Формы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Для управляемого поля React записывает ваше значение обратно в DOM после каждого обновления, поэтому неизменное состояние сохраняет старый текст. Для неуправляемого React задаёт только начальное значение, остальное оставляет браузеру. Режим определяется тем, задано ли определённое значение `value` (или `checked`).

### Предупреждения, говорящие, что не так

```tsx
<input value="x" />
// You provided a `value` prop to a form field without an `onChange` handler. This will render a read-only field. ...

const [v, setV] = useState<string | undefined>(undefined);
<input value={v} onChange={e => setV(e.target.value)} />
// A component is changing an uncontrolled input to be controlled. ...
```

## Справочник

| Элемент | Управляемый | Неуправляемый | Чтение в обработчике |
|---|---|---|---|
| text, email, number, textarea | `value` | `defaultValue` | `e.target.value` |
| checkbox | `checked` | `defaultChecked` | `e.target.checked` |
| radio | `checked` + `name` + `value` | `defaultChecked` | `e.target.value` |
| select | `value` | `defaultValue` | `e.target.value` |
| select multiple | `value` (массив) | `defaultValue` (массив) | `selectedOptions` |
| file | — | читать `files` | `e.target.files` |

| Возможность формы | Действие |
|---|---|
| `onSubmit` | вызывается после прохождения встроенной проверки |
| `action={fn}` | получает `FormData`, выполняется в transition, сбрасывает неуправляемые поля |
| `noValidate` | без встроенной проверки |
| `<button>` | по умолчанию `type="submit"` |
| `form.reset()` | вернуть значения по умолчанию |
| `form.requestSubmit()` | отправка с проверкой, в отличие от `submit()` |
| `readOnly` / `disabled` | нельзя редактировать; отключённые поля не отправляются |

Значение файлового поля нельзя задать из кода, поэтому им пользуются как неуправляемым.

## Подвохи

### Значение, которое не меняется

```tsx
<input value="fixed" onChange={() => {}} />   // typing "other" leaves "fixed"
```

### defaultValue используется один раз

```tsx
function Def({ initial }: { initial: string }) { return <input defaultValue={initial} />; }
// render initial="one", then initial="two":  the field still shows "one"
```

Чтобы показать другую запись, смените `key` формы.

### Любая кнопка отправляет

```tsx
<form onSubmit={count}>
  <button>default</button>              {/* submits */}
  <button type="button">plain</button>  {/* does not */}
</form>
// click both: one submission
```

### Сброс

```tsx
const ref = useRef<HTMLFormElement>(null);
ref.current!.reset();      // uncontrolled fields return to their defaultValue
```

Состояние управляемых полей `reset()` не затрагивает.

## Компромиссы и альтернативы

| Подход | Плюс | Минус |
|---|---|---|
| Управляемые | полный контроль во время ввода | рендер на каждое нажатие |
| Неуправляемые с `FormData` | мало кода, быстро | значения только по запросу |
| `action` | ожидание и сброс берёт на себя React | нужен React 19 |
| Библиотека форм | проверка, массивы полей, много полей | зависимость и её API |
| Общая с сервером схема проверки | один набор правил | настройка сборки |

## Вопросы с собеседований

**1. Управляемое против неуправляемого?**

<details><summary>Ответ</summary>

Управляемое берёт значение из состояния, неуправляемое хранит его в DOM.

</details>

**2. Почему ввод может игнорироваться?**

<details><summary>Ответ</summary>

Поле управляемое, а `onChange` не обновляет состояние.

</details>

**3. Из-за чего предупреждение о переходе из неуправляемого в управляемое?**

<details><summary>Ответ</summary>

`value` сначала был `undefined` (или `null`), потом строкой. Начинайте с `''`.

</details>

**4. Какого типа значение `<input type="number">`?**

<details><summary>Ответ</summary>

Строка.

</details>

**5. Каких элементов нет в `FormData`?**

<details><summary>Ответ</summary>

Отключённых, без `name` и неотмеченных флажков.

</details>

**6. Что делает `defaultValue` после первого рендера?**

<details><summary>Ответ</summary>

Его изменение не меняет содержимое поля.

</details>

**7. Как сбросить форму?**

<details><summary>Ответ</summary>

`form.reset()` для неуправляемых, установка состояния для управляемых или смена `key`.

</details>

**8. Для чего `useId` и для чего нет?**

<details><summary>Ответ</summary>

Для идентификаторов, связывающих элементы; не для ключей списка.

</details>

**9. Подвох: `action` формы вернул ошибку. Поля сохранятся?**

<details><summary>Ответ</summary>

Неуправляемые поля сбрасываются, если функция не бросила исключение.

</details>

**10. Подвох: достаточно ли проверки на клиенте?**

<details><summary>Ответ</summary>

Нет: запрос можно отправить без вашей формы.

</details>

**11. Что по умолчанию делает `<button>` внутри формы?**

<details><summary>Ответ</summary>

Отправляет её.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: input](https://react.dev/reference/react-dom/components/input)
- [React: select](https://react.dev/reference/react-dom/components/select)
- [React: textarea](https://react.dev/reference/react-dom/components/textarea)
- [React: form](https://react.dev/reference/react-dom/components/form)
- [React: useId](https://react.dev/reference/react/useId)
- [MDN: FormData](https://developer.mozilla.org/en-US/docs/Web/API/FormData/FormData)
- [MDN: requestSubmit](https://developer.mozilla.org/en-US/docs/Web/API/HTMLFormElement/requestSubmit)
