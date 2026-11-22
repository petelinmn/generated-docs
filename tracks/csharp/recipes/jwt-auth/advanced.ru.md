---
title: "JWT-аутентификация: production"
lang: ru
track: csharp
kind: recipe
doc: advanced
slug: jwt-auth
part: 2
tags: [jwt, refresh-token, authorization-policies, key-management, token-validation, claims]
versions: ".NET 11, C# 15; Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0"
status: draft
updated: 2026-11-22
---

# JWT-аутентификация: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~1 ч.

## Диагностика отказов

Сервер при отказе кладёт причину в заголовок `WWW-Authenticate` — по нему клиент и разработчик понимают, что не так. Проверено:

| Ситуация | Статус | `WWW-Authenticate` |
|---|---|---|
| нет токена | 401 | `Bearer` |
| токен просрочен | 401 | `Bearer error="invalid_token", error_description="The token lifetime is invalid; NotBefore: …, Expires: …"` |
| подпись другим ключом | 401 | `error="invalid_token", error_description="The signature key was not found"` |
| не тот `aud` | 401 | `error="invalid_token", error_description="The audience … is invalid"` |
| мусор вместо токена | 401 | `error="invalid_token"` |

В продакшене причину клиенту подробно не раскрывают; для диагностики логируйте события `JwtBearerEvents.OnAuthenticationFailed` (на сервере).

## Ключи и секреты

- Симметричный ключ (HMAC) — один секрет и для подписи, и для проверки: годится, когда выдаёт и проверяет один сервис. Длина ключа для HS256 — не меньше 32 байт.
- Асимметричные ключи (RSA/ECDSA): выдающий хранит закрытый ключ, проверяющие получают открытый (через JWKS-адрес `.well-known/jwks.json`). Так работают внешние поставщики (Entra ID, Auth0, Keycloak).
- Ключ берут из конфигурации/секретов ([13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/advanced.ru.md#options-проверка-и-перезагрузка)), а не из кода; предусмотрите **ротацию**: принимайте несколько ключей (`IssuerSigningKeys`) на время перехода.

```csharp
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));    // Issuer, Audience, Key, LifetimeMinutes
builder.Services.AddOptions<JwtOptions>().ValidateDataAnnotations().ValidateOnStart();
```

## Срок жизни, refresh-токены и отзыв

JWT нельзя отозвать до `exp`: сервер не хранит состояния. Поэтому:

- **access-токен** живёт недолго (минуты), его присылают с каждым запросом;
- **refresh-токен** живёт долго, хранится на сервере (в БД, хэшированный), обменивается на новую пару токенов при `/refresh` и **ротируется** (старый становится недействительным);
- при компрометации отзывают refresh-токен; access-токен «дожидается» истечения (короткий срок) либо используют список отозванных идентификаторов (`jti`).

`ClockSkew` — допустимое расхождение часов серверов (по умолчанию 5 минут); в примере выставлено 5 секунд, и токен, просроченный на минуту, отвергнут (проверено).

## Политики и утверждения

Роли — частный случай утверждений. Политики выражают правила точнее:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole("admin"))
    .AddPolicy("CanRefund", p => p.RequireClaim("permission", "refund"))
    .AddPolicy("Adult", p => p.RequireAssertion(ctx => int.TryParse(ctx.User.FindFirstValue("age"), out var a) && a >= 18));
```

Проверено: роль `user` на `/admin` даёт `403`, роль `admin` — `200`. Для проверок над ресурсом («редактировать можно только свой заказ») используют ресурсную авторизацию (`IAuthorizationService`, `AuthorizationHandler`).

## Безопасность

| Правило | Зачем |
|---|---|
| Только HTTPS | токен — «ключ от двери»: перехват = доступ |
| Проверять `iss`, `aud`, подпись, срок | иначе принимается чужой токен |
| Явно задать алгоритм, запретить `none` | защита от подмены алгоритма |
| Короткий access-токен | ограничивает ущерб от кражи |
| Хранить в браузере в `HttpOnly`-cookie, а не `localStorage` | защита от XSS |
| Пароли — только хэши (`PasswordHasher`) | утечка БД не раскрывает пароли |
| Лимит попыток входа ([rate limiting](../rate-limiting/overview.ru.md)) | защита от перебора |

## Лучшие практики

- Выдачу токенов выносят в сервис (`ITokenService`), а не держат в обработчике.
- Для настоящих пользователей используют ASP.NET Core Identity или внешнего поставщика (OpenID Connect), а не самописную проверку пароля.
- Тесты: подменяют ключ и время ([17 · Интеграционные тесты](../../modules/17-testing-integration/overview.ru.md)), проверяют все строки таблицы выше.

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [Rate limiting](../rate-limiting/overview.ru.md) — ограничение попыток входа.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — конвейер.
- [17 · Тестирование: интеграционные тесты](../../modules/17-testing-integration/overview.ru.md) — тесты авторизации.

## Источники

- [Конфигурация JWT Bearer](https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Ресурсная авторизация](https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased)
- [OWASP: JWT cheat sheet](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html)
