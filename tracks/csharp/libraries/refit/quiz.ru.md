---
title: "Квиз: Refit"
lang: ru
track: csharp
kind: library
doc: quiz
slug: refit
part: 2
tags: [refit, http-client, rest, json-context]
status: draft
updated: 2026-11-22
---

# Квиз: Refit

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка: 🟢 [основы](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [эксперт](expert.ru.md).

## 1. 🟢 Что делает Refit?

- A) запускает HTTP-сервер
- B) превращает интерфейс с атрибутами в типизированный HTTP-клиент
- C) кэширует ответы
- D) генерирует документацию API

<details><summary>Ответ</summary>

**B.** Реализацию строит source generator.
→ [Основы](basics.ru.md#что-это-и-зачем)

</details>

## 2. 🟢 Куда попадёт параметр `int page`, которого нет в шаблоне `[Get("/users")]`?

- A) в тело
- B) в строку запроса (`?page=2`)
- C) в заголовок
- D) будет ошибка

<details><summary>Ответ</summary>

**B.** Проверено: `name=ann&page=2`.
→ [Основы](basics.ru.md#интерфейс-клиента)

</details>

## 3. 🟢 Что бросит вызов при ответе сервера `404`?

- A) `null`
- B) `ApiException` со `StatusCode = 404`
- C) `HttpRequestException` без деталей
- D) ничего

<details><summary>Ответ</summary>

**B.** Проверено: `404` и `Content = {"error":"no user"}`.
→ [Основы](basics.ru.md#ошибки-http)

</details>

## 4. 🟢 Что нужно клиенту Refit 17 для JSON?

- A) ничего
- B) `RefitSettings.ForJsonContext(...)` с source-generated `JsonSerializerContext`
- C) Newtonsoft.Json
- D) атрибут `[Json]`

<details><summary>Ответ</summary>

**B.** Без контекста — `NotSupportedException` внутри `ApiException` (проверено).
→ [Основы](basics.ru.md#создание-клиента-и-json-контекст)

</details>

## 5. 🟡 Как получить ответ на `404` без исключения?

- A) `try/catch` всегда
- B) вернуть `Task<IApiResponse<T>>`
- C) `Task<T?>`
- D) нельзя

<details><summary>Ответ</summary>

**B.** Проверено: `IsSuccessStatusCode = False`, `Content = null`, `Error = ApiException`.
→ [Продвинутый](advanced.ru.md#ответ-без-исключений)

</details>

## 6. 🟡 Как зарегистрировать клиент в DI в Refit 17?

- A) `AddRefitClient<T>()` из основного пакета
- B) `AddRefitGeneratedClient<T>(settings)` (Refit.HttpClientFactory)
- C) `services.AddSingleton<T>()`
- D) `new T()`

<details><summary>Ответ</summary>

**B.** Проверено; `AddRefitClient` вынесен в `Refit.Reflection`.
→ [Продвинутый](advanced.ru.md#внедрение-зависимостей)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `[Header("Authorization")]` в параметре передаёт заголовок
- [ ] B) Refit сам выполняет ретраи
- [ ] C) `AliasAs` меняет имя параметра в запросе
- [ ] D) форма отправляется через `[Body(BodySerializationMethod.UrlEncoded)]`

<details><summary>Ответ</summary>

**A, C, D.** B неверно: ретраи — задача обработчика `IHttpClientFactory`.
→ [Продвинутый](advanced.ru.md#заголовки-и-авторизация)

</details>

## 8. 🟡 Открытый вопрос: как вы организуете клиента внешнего API в сервисе?

<details><summary>Ключевые пункты хорошего ответа</summary>

- интерфейс Refit + JSON-контекст с `Web`-настройками;
- регистрация через `AddRefitGeneratedClient` и `IHttpClientBuilder`;
- обработчик устойчивости (таймауты, ретраи), токен через `DelegatingHandler`;
- `IApiResponse<T>` там, где 4xx — норма; мок интерфейса в тестах.

</details>

## 9. 🔴 Поля `UserDto` пришли нулями без ошибки. Причина?

- A) сервер вернул пустой ответ
- B) контекст регистрозависим, а сервер отдаёт camelCase
- C) нет `[Body]`
- D) нет `await`

<details><summary>Ответ</summary>

**B.** Проверено; `JsonSerializerDefaults.Web` решает проблему.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Пример со `RestService.For<IApi>()` не компилируется в Refit 17. Почему?

- A) метод удалён навсегда
- B) рефлексивный путь вынесен в `Refit.Reflection`; основной — `ForGenerated`
- C) нужен .NET Framework
- D) опечатка

<details><summary>Ответ</summary>

**B.** Проверено: CS0117 в проекте с одним `Refit`.
→ [Эксперт](expert.ru.md#внутреннее-устройство)

</details>
