---
title: "Exercises: props and composition"
lang: en
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

# Exercises: props and composition

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 A badge

**Task:** `Badge` shows a label; `tone` is `info` by default or `warning`, and becomes a class.

<details><summary>Hint</summary>

A default value in destructuring.

</details>

<details><summary>Solution</summary>

```tsx
type BadgeProps = { label: string; tone?: 'info' | 'warning' };

function Badge({ label, tone = 'info' }: BadgeProps) {
  return <span className={`badge ${tone}`}>{label}</span>;
}
// <Badge label="New" />  ->  <span class="badge info">New</span>
```

</details>

## 2. 🟡 A panel with slots

**Task:** `Panel` has a title, content and an optional footer; without a footer there is no `<footer>` element.

<details><summary>Hint</summary>

Element props and `&&`.

</details>

<details><summary>Solution</summary>

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

`footer && ...` would hide the number `0` too; if numbers are possible, compare with `!= null`.

</details>

## 3. 🔴 A link that looks like a button

**Task:** accept every attribute of `<a>`, add the class `btn` to the caller's classes, and with `external` open in a new tab safely.

<details><summary>Hint</summary>

`ComponentProps<'a'>`, rest, and the order of spreads.

</details>

<details><summary>Solution</summary>

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

Because `{...rest}` comes after the external defaults, the caller can override `target` or `rel`.

</details>
