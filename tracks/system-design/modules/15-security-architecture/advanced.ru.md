---
title: "Безопасность архитектуры: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Безопасность архитектуры: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Между сервисами

| Способ | Идея |
|---|---|
| mTLS | обе стороны предъявляют сертификаты |
| Токены сервиса | короткоживущие, с аудиторией и правами |
| Подпись запроса (HMAC) | общий ключ, проверка целостности |

## Подпись и защита от повтора

```csharp
if (!CryptographicOperations.FixedTimeEquals(expected, Encoding.ASCII.GetBytes(r.Signature))) return false;

TimeSpan age = clock.GetUtcNow() - DateTimeOffset.FromUnixTimeSeconds(r.Timestamp);
if (age.Duration() > MaxAge) return false;

return _usedNonces.Add(r.Nonce); // only a valid signature may consume a nonce
```

Подписывают метод, путь, время, nonce и хэш тела. Изменение любой части, повтор и устаревший запрос отклоняются. Файл: [Ex02_SignedRequests.cs](code/Examples/Ex02_SignedRequests.cs). Nonce здесь копятся в памяти; в системе нужно общее хранилище со сроком.

## Zero trust

Сеть не даёт доверия: каждый запрос аутентифицируется и авторизуется, права минимальны, предполагается, что нарушитель уже внутри.

## SSRF

Если сервер ходит по адресу из запроса, его можно направить во внутреннюю сеть. Нужны allow-list, запрет редиректов или проверка каждого, контроль на уровне сети.

## Лучшие практики

- Границы доверия отмечайте на схеме.
- Ключи ротируйте, срок токенов делайте коротким.
- Ограничивайте размер и частоту запросов.
- Ведите аудит действий, важных для расследования.

## Частые ошибки

- ❌ Доверять заголовку пользователя от кого угодно → ✅ только от проверенного шлюза.
- ❌ Один общий ключ на все сервисы → ✅ свой у каждого.
- ❌ Возвращать подробности исключения клиенту → ✅ общий ответ и запись в лог.
- ❌ Десериализовать напрямую в доменную сущность → ✅ DTO только с разрешёнными полями.

## Сравнение с другими треками

- [Безопасность Kubernetes](../../../kubernetes/modules/17-security/overview.ru.md), [RBAC](../../../kubernetes/modules/10-namespaces-and-rbac/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [OWASP API Security Top 10, 2023](https://owasp.org/API-Security/editions/2023/en/0x11-t10/)
