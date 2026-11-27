---
title: "Серверный рендеринг"
lang: ru
track: react
kind: module
doc: overview
slug: 14-server-rendering
part: 3
levels: [1, 2, 3, 4]
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Серверный рендеринг

🌐 **Русский** · [English](overview.en.md)

## Коротко

React умеет рендерить на сервере в HTML. Браузер показывает страницу до прихода JavaScript, а затем гидратация (hydration) подключает React к этому HTML. Потоковая отдача (streaming) присылает части страницы по мере готовности. Server Components идут дальше: часть компонентов выполняется только на сервере, и их код в браузер не попадает.

## Ключевые идеи

- Серверный рендер даёт HTML; эффекты и обработчики событий там не работают.
- `hydrateRoot` делает серверный HTML интерактивным и ждёт того же вывода.
- Streaming с `Suspense` сначала шлёт каркас (shell), а медленные части — позже.
- Расхождение при гидратации — это баг, даже если React восстановился.
- У Server Components нет состояния и эффектов; `'use client'` отмечает границу.
- Server Functions — публичные точки входа: проверяйте пользователя и аргументы.

## Минимальный пример

```tsx
function Counter() {
  const [n, setN] = useState(0);
  useEffect(() => { console.log('effect ran'); }, []);
  return <button onClick={() => setN(n + 1)}>count {n}</button>;
}

renderToString(<Counter />);
// <button>count <!-- -->0</button>      — no handler, the effect did not run
```

```tsx
// browser; the container already holds the server HTML
hydrateRoot(document.getElementById('root')!, <Counter />);
// the same <button> node is kept, the effect runs now, clicks work
```

## Когда применять, а когда нет

- ✅ Контентные страницы, поисковики, слабые устройства и сети.
- ✅ Через фреймворк, берущий на себя маршруты, данные и сборку.
- ❌ Внутренний инструмент за логином, где хватает клиентского приложения.
- ❌ Своя настройка Server Components: нужен совместимый сборщик.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | рендер в строку, гидратация |
| [2. Продвинутая теория](advanced.ru.md) | streaming, расхождения, Server Components |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 14-server-rendering level 5` |
| 6. Тесты | ещё не создан — `/learn react 14-server-rendering level 6` |

## Связанные материалы

- Пререквизиты: [06 · Эффекты](../06-effects/overview.ru.md), [12 · Конкурентные возможности](../12-concurrent-features/overview.ru.md), [13 · Actions](../13-actions/overview.ru.md)
- Дальнейшие темы: [дорожная карта React](../../overview.ru.md)
