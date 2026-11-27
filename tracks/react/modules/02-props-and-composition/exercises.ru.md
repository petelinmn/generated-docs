---
title: "Упражнения: props и композиция"
lang: ru
track: react
kind: module
doc: exercises
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: props и композиция

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Значок

**Задача:** `Badge` показывает подпись; `tone` по умолчанию `info` или `warning` и становится классом.

<details><summary>Подсказка</summary>

Значение по умолчанию при деструктуризации.

</details>

<details><summary>Решение</summary>

```tsx
type BadgeProps = { label: string; tone?: 'info' | 'warning' };

function Badge({ label, tone = 'info' }: BadgeProps) {
  return <span className={`badge ${tone}`}>{label}</span>;
}
// <Badge label="New" />  ->  <span class="badge info">New</span>
```

</details>

## 2. 🟡 Панель со слотами

**Задача:** у `Panel` есть заголовок, содержимое и необязательный подвал; без подвала элемента `<footer>` нет.

<details><summary>Подсказка</summary>

Props с элементами и `&&`.

</details>

<details><summary>Решение</summary>

```tsx
function Panel({ title, footer, children }: { title: ReactNode; footer?: ReactNode; children: ReactNode }) {
  return (
    <article>
      <header>{title}</header>
      <div>{children}</div>
      {footer && <footer>{footer}</footer>}
    </article>
  );
}
```

`footer && ...` скроет и число `0`; если возможны числа, сравнивайте с `!= null`.

</details>

## 3. 🔴 Ссылка в виде кнопки

**Задача:** принимать любой атрибут `<a>`, добавлять класс `btn` к классам вызывающего, а с `external` безопасно открывать в новой вкладке.

<details><summary>Подсказка</summary>

`ComponentProps<'a'>`, rest и порядок spread.

</details>

<details><summary>Решение</summary>

```tsx
type LinkButtonProps = ComponentProps<'a'> & { external?: boolean };

function LinkButton({ external = false, className = '', ...rest }: LinkButtonProps) {
  return (
    <a
      {...(external && { target: '_blank', rel: 'noreferrer' })}
      {...rest}
      className={`btn ${className}`.trim()}
    />
  );
}
// <LinkButton href="/a">A</LinkButton>           ->  <a href="/a" class="btn">A</a>
// <LinkButton href="/b" external>B</LinkButton>  ->  <a target="_blank" rel="noreferrer" href="/b" class="btn">B</a>
```

Поскольку `{...rest}` идёт после значений для внешней ссылки, вызывающий может переопределить `target` или `rel`.

</details>
