---
title: "Квиз: сокращатель ссылок"
lang: ru
track: system-design
kind: recipe
doc: quiz
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# Квиз: сокращатель ссылок

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой код по умолчанию отдаёт `Results.Redirect(url)`?

- A) 301
- B) 302
- C) 307
- D) 200

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Чем случайные коды лучше 1, 2, 3 для неопубликованных ссылок?

- A) Они короче
- B) Их нельзя обойти по порядку
- C) Они не совпадают
- D) Им не нужно хранилище

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что разрешает ситуацию, когда два запроса получили один код?

- A) Проверка перед вставкой
- B) Атомарная вставка с уникальным ключом
- C) Больший таймаут
- D) Кэш

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟡 Выбери все адреса, которые рецепт отклоняет

- [ ] A) `ftp://example.com`
- [ ] B) `https://a@evil.example/`
- [ ] C) `https://example.com/a?b=1`
- [ ] D) Адрес на собственном хосте

<details><summary>Ответ</summary>

**A, B, D.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 5. 🟡 Зачем при создании удалять ключ кэша?

- A) Освободить память
- B) Код может быть закэширован как отсутствующий
- C) Считать переходы
- D) Этого требует SQLite

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что происходит при полной очереди переходов?

- A) Перенаправление ждёт
- B) Новый переход отбрасывается и учитывается
- C) Сервис падает
- D) Отключается старый код

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Найди ошибку

```csharp
var link = await cache.GetOrCreateAsync(code, async _ => await store.FindAsync(code));
return link?.Url;
```

<details><summary>Ответ</summary>

Ссылка из кэша отдаётся и после истечения срока; проверяйте `IsLive` после кэша.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🔴 Экземпляр A отключил ссылку. У B она в локальном кэше. B ответит

- A) сразу 404
- B) перенаправлением, пока не истечёт локальная запись
- C) 500
- D) спросит у A

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 9. 🔴 Три экземпляра, по 30 созданий в минуту. Клиент может создать до

- A) 30
- B) 90
- C) 10
- D) без ограничений

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Какой код постоянный и сохраняет метод?

- A) 301
- B) 302
- C) 307
- D) 308

<details><summary>Ответ</summary>

**D.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Открытый вопрос: перенаправлений стало 50 000 в секунду. Что менять?

<details><summary>Ключевые пункты ответа</summary>

- Измерить долю попаданий; добавить экземпляры и распределённый уровень кэша.
- Партиционировать или реплицировать хранилище.
- Вынести переходы в журнал.
- Обслуживать близко к пользователям; защитить хранилище от промахов.

→ [Экспертная теория](expert.ru.md)

</details>
