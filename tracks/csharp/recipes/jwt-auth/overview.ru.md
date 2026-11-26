---
title: "JWT-аутентификация"
lang: ru
track: csharp
kind: recipe
doc: overview
slug: jwt-auth
part: 2
levels: [1, 2, 3, 4]
tags: [jwt, authentication, authorization, bearer-token, refresh-token, aspnet-core]
versions: ".NET 11, C# 15; Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0"
status: draft
updated: 2026-11-22
---

# JWT-аутентификация

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: выдать и проверить JWT в ASP.NET Core. Клиент входит по логину и паролю, получает подписанный токен и присылает его в `Authorization: Bearer …`; сервер проверяет подпись, срок, издателя и аудиторию без обращения к БД. Для прав используют роли и политики. Production-версия добавляет короткие access-токены, refresh-токены, ротацию ключей и диагностику отказов; на масштабе выдачу берёт внешний поставщик (OpenID Connect).

## Ключевые идеи

- JWT = заголовок.нагрузка.подпись; нагрузка **не шифруется**.
- `AddJwtBearer` + `TokenValidationParameters`: проверять подпись, срок, `iss`, `aud`.
- `UseAuthentication` перед `UseAuthorization`; `RequireAuthorization("Policy")`.
- 401 — не аутентифицирован, 403 — нет прав; причина — в `WWW-Authenticate`.
- Отозвать JWT до `exp` нельзя: короткий срок, refresh-токены, список `jti`.
- Ключи — из секретов; поддерживайте ротацию (`IssuerSigningKeys`).
- Лучше внешний IdP (OIDC), чем самописная выдача.

## Минимальный пример

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = "https://shop.local", ValidAudience = "shop-api", IssuerSigningKey = key,
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
    });
builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/me", (ClaimsPrincipal u) => u.Identity?.Name).RequireAuthorization();
```

## Когда применять, а когда нет

- ✅ Без состояния между экземплярами API, мобильные и SPA-клиенты.
- ✅ Сервис-сервис с общим центром аутентификации.
- ❌ Нужен мгновенный отзыв и простая серверная сессия — cookie-сессии.
- ❌ Секреты или персональные данные в нагрузке.
- ❌ Самописная выдача и хранение паролей вместо Identity/IdP.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | что такое JWT, настройка проверки, выдача, роли и политика, коды ответов |
| [2. Production](advanced.ru.md) | диагностика отказов, ключи и ротация, refresh-токены, политики, безопасность |
| [3. Масштаб и грани](expert.ru.md) | IdP и JWKS, отзыв, подвохи, альтернативы, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp jwt-auth level 5` |
| 6. Тесты | ещё не создан — `/learn csharp jwt-auth level 6` |

## Связанные материалы

- Пререквизиты: [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md), [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md)
- Рядом: [Rate limiting](../rate-limiting/overview.ru.md), [17 · Интеграционные тесты](../../modules/17-testing-integration/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — JWT и OAuth2 в FastAPI
- Рецепты и библиотеки: [Загрузка файлов](../file-upload/overview.ru.md), [gRPC-сервис](../grpc-service/overview.ru.md), [Real-time через SignalR](../signalr-realtime/overview.ru.md)
- [System design: безопасность архитектуры](../../../system-design/modules/15-security-architecture/overview.ru.md)
