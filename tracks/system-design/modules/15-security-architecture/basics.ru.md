---
title: "Безопасность архитектуры: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Безопасность архитектуры: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** HTTP, веб-API · **Время:** ~2 ч

## Зачем это нужно

Исправить уязвимую архитектуру после инцидента намного дороже, чем задать вопросы заранее.

## Моделирование угроз: STRIDE

| Угроза | Суть | Типичная защита |
|---|---|---|
| Spoofing | выдать себя за другого | аутентификация |
| Tampering | изменить данные | подписи, права, TLS |
| Repudiation | отрицать действие | аудит |
| Information disclosure | раскрыть данные | шифрование, авторизация |
| Denial of service | сделать недоступным | лимиты |
| Elevation of privilege | получить лишние права | минимальные привилегии |

## Авторизация на уровне объекта

Проверить, что пользователь вошёл, недостаточно: он может подставить чужой идентификатор.

```csharp
// "Not found" for a forbidden object does not reveal that it exists.
public static Document? Find(User user, int id, IReadOnlyDictionary<int, Document> documents) =>
    documents.TryGetValue(id, out Document? d) && CanRead(user, d) ? d : null;
```

Администратор другой организации доступа не получает. Файл: [Ex03_ObjectAuthorization.cs](code/Examples/Ex03_ObjectAuthorization.cs).

## Хранение паролей

```csharp
byte[] salt = RandomNumberGenerator.GetBytes(16);
byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
```

Соль уникальна для каждого пароля, параметры хранятся вместе с хэшем, сравнение — `CryptographicOperations.FixedTimeEquals`. Файл: [Ex01_Passwords.cs](code/Examples/Ex01_Passwords.cs). Это учебный пример: в ASP.NET Core берите Identity.

| Алгоритм (рекомендации OWASP) | Параметры |
|---|---|
| Argon2id (предпочтительно) | минимум 19 МиБ, 2 итерации, 1 поток |
| scrypt | N=2^17, r=8, p=1 |
| PBKDF2-HMAC-SHA256 (когда нужен FIPS) | 600 000 итераций |

## Ошибки новичка

- ❌ `SHA256(password)` → ✅ медленный алгоритм с солью.
- ❌ Проверять только факт входа → ✅ права на объект.
- ❌ Секрет в репозитории → ✅ хранилище секретов.
- ❌ Скрывать кнопку вместо проверки на сервере → ✅ проверять на сервере.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [JWT-аутентификация](../../../csharp/recipes/jwt-auth/overview.ru.md)

## Источники

- [OWASP: Password Storage](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [STRIDE](https://learn.microsoft.com/azure/security/develop/threat-modeling-tool-threats)
