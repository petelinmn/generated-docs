---
title: "Квиз: кэширование"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-26
---

# Квиз: кэширование

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 В cache-aside при промахе

- A) кэш сам читает базу
- B) приложение читает источник и кладёт значение в кэш
- C) запрос завершается ошибкой
- D) запись идёт сначала в кэш

<details><summary>Ответ</summary>

**B.** A — это read-through, D — стратегии записи. → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 10 000 запросов в секунду, доля попаданий 95%. Нагрузка на базу?

- A) 50
- B) 500
- C) 950
- D) 9500

<details><summary>Ответ</summary>

**B.** 5% от 10 000. → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что вернёт второй вызов?

```csharp
service.GetPrice(1);          // the store has no price yet
store.Write(1, 10m);          // bypasses the service
Console.WriteLine(service.GetPrice(1));
```

<details><summary>Ответ</summary>

```text
0
```

Кэш не знает о записи в обход сервиса; значение обновится после TTL. → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что должно входить в ключ?

- A) Только идентификатор
- B) Всё, что влияет на результат
- C) Текущее время
- D) Имя сервера

<details><summary>Ответ</summary>

**B.** Иначе один пользователь получит чужой вариант ответа. C и D уничтожат попадания. → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Выбери все средства против stampede

- [ ] A) Single-flight
- [ ] B) Jitter в TTL
- [ ] C) Одинаковый TTL для всех ключей
- [ ] D) Отдача устаревшего значения во время обновления

<details><summary>Ответ</summary>

**A, B, D.** C синхронизирует истечения и усугубляет проблему. → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что означает `Cache-Control: no-cache`?

- A) Не сохранять
- B) Сохранять можно, но проверять перед использованием
- C) Хранить только в CDN
- D) Хранить вечно

<details><summary>Ответ</summary>

**B.** A — это `no-store`. → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 LRU ёмкостью 2: set a, set b, get a, set c. Что вытеснено?

- A) a
- B) b
- C) c
- D) ничего

<details><summary>Ответ</summary>

**B.** Обращение к `a` сделало `b` самым давним. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Какой риск у write-behind?

- A) Медленные чтения
- B) Потеря ещё не записанных в источник данных
- C) Невозможность задать TTL
- D) Двойное чтение

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что выведет код?

```csharp
mc.Set("list", new List<int> { 1 });
mc.Get<List<int>>("list")!.Add(2);
Console.WriteLine(mc.Get<List<int>>("list")!.Count);
```

<details><summary>Ответ</summary>

```text
2
```

`IMemoryCache` хранит ссылку. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Как старое значение может остаться в кэше, если после каждой записи ключ удаляется?

<details><summary>Ключевые пункты ответа</summary>

- Читатель прочитал v1 из базы до записи, а положил в кэш после удаления.
- Средства: версии, аренда, отложенное повторное удаление.
- TTL ограничивает длительность.

</details>
