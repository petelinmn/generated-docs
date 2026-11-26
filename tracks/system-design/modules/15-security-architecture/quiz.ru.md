---
title: "Квиз: безопасность архитектуры"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: безопасность архитектуры

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что означает T в STRIDE?

- A) Timeout
- B) Tampering
- C) Tracing
- D) Token

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Пользователь вошёл и запросил чужой счёт. Что нужно?

- A) Ничего, он вошёл
- B) Проверка прав на этот объект
- C) CAPTCHA
- D) Сжатие

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Как хранить пароли?

- A) Открытым текстом
- B) SHA-256 без соли
- C) Медленным хэшем с уникальной солью
- D) В Base64

<details><summary>Ответ</summary>

**C.** Base64 — не защита. → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что рекомендует OWASP в первую очередь?

- A) Argon2id
- B) MD5
- C) SHA-1
- D) CRC32

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что защищает от повтора запроса?

- [ ] A) Nonce
- [ ] B) Метка времени с допустимым возрастом
- [ ] C) Сжатие
- [ ] D) Только HMAC от тела

<details><summary>Ответ</summary>

**A, B.** Одна подпись повтору не мешает. → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Главная идея zero trust?

- A) Внутренней сети доверяют
- B) Каждый запрос проверяется независимо от сети
- C) Никому не давать доступ
- D) Только VPN

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Сервер скачивает картинку по URL пользователя. Главный риск?

- A) XSS
- B) SSRF
- C) CSRF
- D) Переполнение стека

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что выведет код?

```csharp
var user = new User("eve", "other", IsAdmin: true);
Console.WriteLine(DocumentAccess.CanRead(user, new Document(1, "acme", "ann")));
```

<details><summary>Ответ</summary>

```text
False
```

Администратор другой организации. → [Базовая теория](basics.ru.md)

</details>

## 9. 🔴 Что выведет код на Windows?

```csharp
Console.WriteLine(Path.Combine("C:\\data", "C:\\Windows\\win.ini"));
```

<details><summary>Ответ</summary>

```text
C:\Windows\win.ini
```

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Проведите моделирование угроз для загрузки файлов пользователями.

<details><summary>Ключевые пункты ответа</summary>

- Поток данных и границы доверия.
- Угрозы по STRIDE: подмена имени, размер, содержимое, доступ к чужим файлам.
- Лимиты, генерируемые имена, проверка прав, хранение вне веб-корня.
- Аудит и остаточные риски.

</details>
