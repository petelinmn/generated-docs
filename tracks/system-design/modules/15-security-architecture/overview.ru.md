---
title: "Безопасность архитектуры"
lang: ru
track: system-design
kind: module
doc: overview
slug: 15-security-architecture
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Безопасность архитектуры

🌐 **Русский** · [English](overview.en.md)

## Коротко

Безопасность закладывают при проектировании: понять, что защищаем и от чего, где проходят границы доверия и что произойдёт, если один из рубежей не устоит.

## Ключевые идеи

- Моделирование угроз начинается со схемы данных и границ доверия.
- Аутентификация говорит «кто», авторизация — «можно ли».
- Права проверяют на уровне конкретного объекта.
- По умолчанию доступ запрещён, права минимальны.
- Пароли хранят медленным хэшем с солью.
- Защита строится слоями.
- Секреты не попадают в код и логи.

## Минимальный пример

```csharp
public static bool CanRead(User user, Document document) =>
    user.Tenant == document.Tenant && (user.Id == document.OwnerId || user.IsAdmin);
```

## Когда применять, а когда нет

- ✅ Всегда, соразмерно ценности данных.
- ✅ Новый внешний интерфейс или поток персональных данных.
- ❌ Собственные криптографические алгоритмы.
- ❌ Своя система входа, когда есть проверенная.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | угрозы, аутентификация и авторизация, пароли |
| [2. Продвинутая теория](advanced.ru.md) | подписи, сервис–сервис, zero trust, SSRF, секреты |
| [3. Экспертная теория](expert.ru.md) | OWASP API Top 10, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | PBKDF2, HMAC-подпись, авторизация объектов |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [проектирование API](../05-api-design/overview.ru.md), [паттерны надёжности](../09-reliability-patterns/overview.ru.md)
- В других треках: [JWT-аутентификация](../../../csharp/recipes/jwt-auth/overview.ru.md), [безопасность Kubernetes](../../../kubernetes/modules/17-security/overview.ru.md), [RBAC](../../../kubernetes/modules/10-namespaces-and-rbac/overview.ru.md)
