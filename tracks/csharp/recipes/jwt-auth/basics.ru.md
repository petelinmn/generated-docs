---
title: "JWT-аутентификация: минимальное решение"
lang: ru
track: csharp
kind: recipe
doc: basics
slug: jwt-auth
part: 2
tags: [jwt, authentication, authorization, bearer-token, aspnet-core]
versions: ".NET 11, C# 15; Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0"
status: draft
updated: 2026-11-22
---

# JWT-аутентификация: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md), [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md). **Время:** ~45 мин.

## Задача

API должен знать, кто его вызывает, и закрывать часть конечных точек. Решение: клиент входит по логину и паролю, получает **JWT** (JSON Web Token) и присылает его в заголовке `Authorization: Bearer <токен>`. Сервер проверяет подпись и срок, не обращаясь в базу.

## Что такое JWT

Токен из трёх частей через точки: `заголовок.полезная нагрузка.подпись` (каждая — Base64Url). Нагрузка — набор **утверждений** (claims): `sub` (кто), `iss` (кто выдал), `aud` (для кого), `exp` (когда истекает), роли и т. д. Проверено: токен состоит из 3 частей, а нагрузка выглядит как
`{"aud":"shop-api","iss":"https://shop.local","exp":…,"sub":"ann","…/role":"user","…/name":"ann"}`. Подпись гарантирует, что нагрузку не изменили; **шифрования нет** — нагрузку может прочитать любой, секретов в ней быть не должно.

## Минимальное решение

Пакет `Microsoft.AspNetCore.Authentication.JwtBearer`. Настройка проверки и выдача токена:

```csharp
const string Issuer = "https://shop.local", Audience = "shop-api";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-key-for-tests-only-32+chars!!"));   // from configuration in real life

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = Issuer, ValidAudience = Audience, IssuerSigningKey = key,
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromSeconds(5),
    };
});
builder.Services.AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("admin"));

app.UseAuthentication();
app.UseAuthorization();

string Issue(string user, string[] roles, TimeSpan lifetime)
{
    var now = DateTime.UtcNow;
    var descriptor = new SecurityTokenDescriptor
    {
        Issuer = Issuer, Audience = Audience,
        Subject = new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r))
            .Append(new Claim(JwtRegisteredClaimNames.Sub, user)).Append(new Claim(ClaimTypes.Name, user))),
        NotBefore = now, IssuedAt = now, Expires = now.Add(lifetime),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
    };
    return new JsonWebTokenHandler().CreateToken(descriptor);
}

app.MapPost("/login", (LoginDto d) => d is { User: "ann", Password: "pw" }
    ? Results.Ok(new { access_token = Issue("ann", ["user"], TimeSpan.FromMinutes(15)) })
    : Results.Unauthorized());
app.MapGet("/public", () => "anyone");
app.MapGet("/me", (ClaimsPrincipal u) => $"{u.Identity?.Name}").RequireAuthorization();
app.MapGet("/admin", () => "secret").RequireAuthorization("AdminOnly");
```

Порядок важен: `UseAuthentication` перед `UseAuthorization`. `RequireAuthorization()` закрывает точку; `RequireAuthorization("AdminOnly")` — политикой по роли. (`using Microsoft.IdentityModel.JsonWebTokens; using Microsoft.IdentityModel.Tokens;`.)

## Как это работает: коды ответов

Проверено на запущенном приложении:

| Запрос | Ответ |
|---|---|
| `POST /login` с верными данными | `200`, `access_token` (3 части) |
| `POST /login` с неверным паролем | `401` |
| `GET /public` без токена | `200` |
| `GET /me` без токена | `401`, заголовок `WWW-Authenticate: Bearer` |
| `GET /me` с токеном | `200`, имя пользователя |
| `GET /admin` с ролью `user` | `403` |
| `GET /admin` с ролью `admin` | `200` |

Различайте: **401** — «не аутентифицирован» (нет или неверный токен), **403** — «аутентифицирован, но нет прав».

## Ошибки новичка

- ❌ Ключ подписи прямо в коде → ✅ в конфигурации или хранилище секретов.
- ❌ Пароль или секреты в нагрузке токена → ✅ только идентификаторы и роли (нагрузку читает любой).
- ❌ `UseAuthorization` без `UseAuthentication` → ✅ сначала аутентификация.
- ❌ Отключённая проверка срока или подписи «для удобства» → ✅ все проверки включены.
- ❌ Токен без срока жизни → ✅ короткое время жизни и обновление.
- ❌ Путать 401 и 403 → ✅ 401 — кто вы?, 403 — вам нельзя.

## Связанные материалы

- Дальше: [production-версия](advanced.ru.md) — ключи, проверка ошибок, refresh-токены, политики.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — конвейер и `RequireAuthorization`.
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — секреты и Options.
- [Python](../../../python/overview.ru.md) — JWT в FastAPI.

## Источники

- [Аутентификация JWT Bearer в ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/)
- [Авторизация на основе политик](https://learn.microsoft.com/aspnet/core/security/authorization/policies)
- [RFC 7519: JSON Web Token](https://www.rfc-editor.org/rfc/rfc7519)
