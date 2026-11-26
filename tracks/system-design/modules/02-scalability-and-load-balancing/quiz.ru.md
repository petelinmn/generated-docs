---
title: "Квиз: масштабирование и балансировка"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Квиз: масштабирование и балансировка

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что мешает горизонтально масштабировать сервис?

- A) Использование HTTP
- B) Сессии в памяти процесса
- C) Асинхронные обработчики
- D) Внешняя база

<details><summary>Ответ</summary>

**B.** Следующий запрос может попасть на другой экземпляр. D, наоборот, помогает вынести состояние. → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что видит балансировщик L7, но не видит L4?

- A) IP-адрес
- B) Порт
- C) Путь HTTP-запроса
- D) Протокол TCP

<details><summary>Ответ</summary>

**C.** L4 не разбирает содержимое соединения. → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что выведет код?

```csharp
var rr = new RoundRobin(["a", "b", "c"]);
Console.WriteLine(string.Concat(Enumerable.Range(0, 5).Select(_ => rr.Pick())));
```

<details><summary>Ответ</summary>

```text
abcab
```

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что описывает кластер в YARP?

- A) Правило сопоставления пути
- B) Набор адресов назначения и политику их выбора
- C) Pod-ы Kubernetes
- D) Список клиентов

<details><summary>Ответ</summary>

**B.** Сопоставление пути — это маршрут. → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Выбери все верные утверждения о consistent hashing

- [ ] A) При добавлении узла ключи переезжают только на новый узел
- [ ] B) Он устраняет проблему горячего ключа
- [ ] C) Виртуальные точки выравнивают доли
- [ ] D) Хэш должен быть одинаков во всех процессах

<details><summary>Ответ</summary>

**A, C, D.** B неверно: один ключ всё равно на одном узле. → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какая политика YARP действует, если не указана никакая?

- A) `RoundRobin`
- B) `Random`
- C) `PowerOfTwoChoices`
- D) `LeastRequests`

<details><summary>Ответ</summary>

**C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Health checks в YARP по умолчанию

- A) включены оба
- B) включены только пассивные
- C) выключены оба
- D) включены только активные

<details><summary>Ответ</summary>

**C.** Их нужно включать явно. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что выведет код?

```csharp
var wrr = new SmoothWeightedRoundRobin(("a", 5), ("b", 1), ("c", 1));
Console.WriteLine(string.Concat(Enumerable.Range(0, 7).Select(_ => wrr.Pick())));
```

<details><summary>Ответ</summary>

```text
aabacaa
```

Выборы `b` и `c` вставлены между выборами `a`, а не отложены в конец. → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Найди ошибку

```csharp
int i = Interlocked.Increment(ref _next);
return nodes[i % nodes.Count];
```

<details><summary>Ответ</summary>

После переполнения `i` отрицателен, остаток тоже, и будет исключение. Нужно привести к `uint`: `unchecked((uint)i) % (uint)nodes.Count`. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

После перехода с 31 на 100 узлов пропускная способность упала. Почему так бывает?

<details><summary>Ключевые пункты ответа</summary>

- USL: расходы на согласование растут как `n(n-1)`.
- Общие ресурсы: блокировки в базе, число соединений.
- Проверить измерениями, шардировать или убрать общую точку.

</details>
