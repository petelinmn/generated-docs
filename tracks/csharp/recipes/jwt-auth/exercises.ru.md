---
title: "Упражнения: JWT-аутентификация"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: jwt-auth
part: 2
tags: [jwt, authentication, authorization-policies, testing]
status: draft
updated: 2026-11-22
---

# Упражнения: JWT-аутентификация

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения опираются на приложение из [минимального решения](basics.ru.md) и проверены запуском (Kestrel + `HttpClient`, пакет `Microsoft.AspNetCore.Authentication.JwtBearer` 11.0.0). Запускаемых версий в `code/` нет (уровень 5 не создан).

## 1. 🟢 Вход и защищённая точка

**Задача:** реализуй `POST /login` (`ann`/`pw` → токен, иначе 401) и `GET /me` с `RequireAuthorization()`. Проверь коды.

**Ожидаемый результат:**

```text
login верный → 200, токен из 3 частей;  неверный → 401
/me без токена → 401;  /me с токеном → 200 (имя ann)
```

<details><summary>Подсказка</summary>

`Results.Ok(new { access_token = ... })`, `Results.Unauthorized()`; клиенту — `AuthenticationHeaderValue("Bearer", token)`.

</details>

<details><summary>Решение</summary>

См. код `Issue`, `/login`, `/me` в [минимальном решении](basics.ru.md#минимальное-решение).

```csharp
// Exercise 1: sending the token from the client.
var req = new HttpRequestMessage(HttpMethod.Get, "/me");
req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
var r = await http.SendAsync(req);   // 200 "ann"; without the header — 401
```

Проверено: `bad login: 401`, `/me` без токена `401`, с токеном `200`.

</details>

## 2. 🟡 Роли и политика

**Задача:** добавь политику `AdminOnly` и точку `/admin`. Покажи, что `user` получает 403, а `admin` — 200.

**Ожидаемый результат:** `403` для роли `user`, `200 secret` для `admin`.

<details><summary>Подсказка</summary>

`AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("admin"))`; роль кладётся в утверждение `ClaimTypes.Role`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: a role-based policy.
builder.Services.AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("admin"));
app.MapGet("/admin", () => "secret").RequireAuthorization("AdminOnly");
```

Проверено: `admin, user role: 403`, `admin, admin role: 200 secret`.

</details>

## 3. 🟡 Набор негативных проверок

**Задача:** напиши проверки, где токен отвергается: просрочен, подписан другим ключом, выдан для другой аудитории, мусор. Для каждого убедись в 401 и содержимом `WWW-Authenticate`.

**Ожидаемый результат:** во всех четырёх случаях `401` и `error="invalid_token"`.

<details><summary>Подсказка</summary>

Расширь `Issue` параметрами `signWith`, `audience`; срок — отрицательный `TimeSpan`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: negative cases — each must be rejected with 401.
await Show("expired",        "/me", Issue("ann", ["user"], TimeSpan.FromSeconds(-60)));
await Show("wrong key",      "/me", Issue("ann", ["user"], TimeSpan.FromMinutes(5), new SymmetricSecurityKey(Encoding.UTF8.GetBytes("another-key-another-key-another-key-123456"))));
await Show("wrong audience", "/me", Issue("ann", ["user"], TimeSpan.FromMinutes(5), audience: "other"));
await Show("garbage",        "/me", "not.a.jwt");
```

Проверено: все четыре — `401`; сообщения: срок недействителен, ключ подписи не найден, аудитория недействительна, `invalid_token`.

</details>

## 4. 🔴 Ротация ключей

**Задача:** настрой проверку так, чтобы принимались токены, подписанные старым **и** новым ключом (`IssuerSigningKeys`), а выдача шла новым. Проверь, что токен старого ключа проходит, а токен чужого — нет.

**Ожидаемый результат:** старый ключ → 200, новый → 200, чужой → 401.

<details><summary>Подсказка</summary>

`TokenValidationParameters.IssuerSigningKeys = [newKey, oldKey]` вместо `IssuerSigningKey`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: accept several signing keys during rotation.
o.TokenValidationParameters = new TokenValidationParameters
{
    ValidIssuer = Issuer, ValidAudience = Audience,
    IssuerSigningKeys = [newKey, oldKey],      // issue with newKey; oldKey is dropped after old tokens expire
    ValidateIssuerSigningKey = true,
};
```

Проверено: токен старого ключа — `200`, нового — `200`, чужого — `401`. После истечения всех токенов старого ключа его убирают из списка.

</details>
