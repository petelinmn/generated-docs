---
title: "Упражнения: эффекты"
lang: ru
track: react
kind: module
doc: exercises
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: эффекты

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Ширина окна

**Задача:** хук, возвращающий текущую ширину окна и отписывающийся, когда компонент исчезает.

<details><summary>Подсказка</summary>

`addEventListener` в установке, `removeEventListener` в очистке, одна и та же функция.

</details>

<details><summary>Решение</summary>

```tsx
function useWindowWidth() {
  const [width, setWidth] = useState(window.innerWidth);
  useEffect(() => {
    const onResize = () => setWidth(window.innerWidth);
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, []);
  return width;
}
```

</details>

## 2. 🟡 Секундомер

**Задача:** считать секунды, пока `running` истинно; пауза сохраняет число.

<details><summary>Подсказка</summary>

Эффект может выйти раньше. Как не читать `seconds`?

</details>

<details><summary>Решение</summary>

```tsx
function Stopwatch({ running }: { running: boolean }) {
  const [seconds, setSeconds] = useState(0);
  useEffect(() => {
    if (!running) return;
    const id = setInterval(() => setSeconds(s => s + 1), 1000);
    return () => clearInterval(id);
  }, [running]);
  return <p>{seconds}</p>;
}
```

</details>

## 3. 🔴 Загрузка пользователя

**Задача:** показывать загрузку, имя или ошибку; при быстрой смене `id` никогда не должны показаться чужие данные.

<details><summary>Подсказка</summary>

Отменяйте запрос в очистке и помните, какому `id` принадлежит состояние.

</details>

<details><summary>Решение</summary>

```tsx
function User({ id }: { id: string }) {
  const [state, setState] = useState<{ id: string; name?: string; error?: string }>({ id });

  useEffect(() => {
    const controller = new AbortController();
    fetch(`/users/${id}`, { signal: controller.signal })
      .then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); })
      .then(user => setState({ id, name: user.name }))
      .catch(e => { if (!controller.signal.aborted) setState({ id, error: String(e.message) }); });
    return () => controller.abort();
  }, [id]);

  if (state.id !== id || (!state.name && !state.error)) return <p>loading</p>;
  return state.error ? <p role="alert">{state.error}</p> : <p>{state.name}</p>;
}
```

Хранение `id` вместе с данными сразу после переключения показывает «loading», без лишнего эффекта, очищающего состояние.

</details>
