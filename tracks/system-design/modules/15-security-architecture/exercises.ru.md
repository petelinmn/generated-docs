---
title: "Упражнения: безопасность архитектуры"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: безопасность архитектуры

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Скрыть секреты в заголовках

**Задача:** заменить значения чувствительных заголовков на `***` независимо от регистра.

**Ожидаемый результат:**

```text
authorization -> ***; Accept stays
```

<details><summary>Подсказка</summary>

`StringComparer.OrdinalIgnoreCase`.

</details>

<details><summary>Решение</summary>

```csharp
headers.ToDictionary(
    h => h.Key,
    h => Sensitive.Contains(h.Key, StringComparer.OrdinalIgnoreCase) ? "***" : h.Value);
```

Список запрещённого легко отстаёт; надёжнее логировать только разрешённые заголовки. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Allow-list для исходящих адресов

**Задача:** разрешить только `https` на точно перечисленные хосты, без userinfo и нестандартного порта.

**Ожидаемый результат:**

```text
https://api.example.com/v1 -> true; look-alike hosts, http, other ports -> false
```

<details><summary>Подсказка</summary>

Разбирайте `Uri`, не сравнивайте строки.

</details>

<details><summary>Решение</summary>

```csharp
Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
&& uri.Scheme == Uri.UriSchemeHttps
&& uri.IsDefaultPort
&& uri.UserInfo.Length == 0
&& allowedHosts.Contains(uri.Host);
```

Это один слой: нужно также контролировать редиректы и адреса, в которые разрешается имя.

</details>

## 3. 🔴 Путь внутри корня

**Задача:** вернуть полный путь, только если он остаётся внутри корневой папки.

**Ожидаемый результат:**

```text
sub/report.txt -> allowed; ../secret.txt, ../data-evil/x -> null
```

<details><summary>Подсказка</summary>

Нормализуйте путь и сравнивайте с корнем, оканчивающимся разделителем.

</details>

<details><summary>Решение</summary>

```csharp
string full = Path.GetFullPath(Path.Combine(fullRoot, name));
// Ordinal is the strict choice; symbolic links are not resolved here.
return full.StartsWith(fullRoot, StringComparison.Ordinal) ? full : null;
```

Лучше вообще не использовать имя пользователя как путь, а выдавать своё.

</details>
