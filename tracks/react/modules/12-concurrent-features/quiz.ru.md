---
title: "Квиз: конкурентные возможности"
lang: ru
track: react
kind: module
doc: quiz
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Квиз: конкурентные возможности

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что показывает `Suspense`, пока потомок грузится?

- A) Ничего
- B) `fallback`
- C) Ошибку
- D) Всегда старый DOM

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что делает `lazy`?

- A) Задерживает рендер на время
- B) Загружает код компонента при необходимости
- C) Кэширует данные
- D) Мемоизирует

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что ловит error boundary?

- A) Ошибку в `onClick`
- B) Ошибку при рендере
- C) Ошибку в `setTimeout`
- D) Свою ошибку

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Заголовок и загружающийся компонент в одном `Suspense`. Пока идёт загрузка, заголовок

- A) виден
- B) заменён на fallback
- C) задвоен
- D) посерел

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Переключение вкладки через `useTransition`, новая не готова. Что на экране?

- A) Fallback
- B) Старая вкладка, `isPending` истинно
- C) Пустая страница
- D) Часть новой вкладки

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Выбери все верные утверждения о `useDeferredValue`

- [ ] A) Перерендеривает в фоне с новым значением
- [ ] B) Ждёт фиксированные 300 мс
- [ ] C) Помогает, когда медленный потомок можно пропустить
- [ ] D) Уменьшает число запросов к серверу

<details><summary>Ответ</summary>

**A, C.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Найди ошибку

```tsx
function Name() {
  const name = use(fetchName());
  return <b>{name}</b>;
}
```

<details><summary>Ответ</summary>

Новый промис при каждом рендере; создайте снаружи или кэшируйте.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Какое обновление НЕ должно быть transition?

- A) Переключение вкладки
- B) Значение управляемого поля
- C) Фильтрация большого списка
- D) Переход на страницу

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что сохраняет скрытый `<Activity>`?

- A) Работающие эффекты
- B) Состояние
- C) Ничего
- D) Только DOM

<details><summary>Ответ</summary>

**B.** Эффекты очищаются.

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Состояние задаётся после `await` внутри `startTransition`. Что нужно, чтобы оно осталось transition?

- A) Ничего
- B) Снова обернуть в `startTransition`
- C) `flushSync`
- D) `useRef`

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Обработчик кнопки бросает ошибку внутри границы. Итог?

- A) Показан fallback
- B) Граница её не ловит
- C) Страница перезагружается
- D) Кнопка исчезает

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: как сделать переключение между вкладками с тяжёлыми данными плавным?

<details><summary>Ключевые пункты ответа</summary>

- Переключать в transition и показывать `isPending`.
- Границы вокруг содержимого, а не панели вкладок; добавить error boundaries.
- Предзагрузка по наведению и кэш промисов.
- Держать скрытые вкладки через `Activity`, если важно состояние.

→ [Экспертная теория](expert.ru.md)

</details>
