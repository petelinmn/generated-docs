---
title: "Квиз: Фоновые задачи"
lang: ru
track: csharp
kind: recipe
doc: quiz
slug: background-jobs
part: 2
tags: [background-service, hosted-service, channel, graceful-shutdown]
status: draft
updated: 2026-11-22
---

# Квиз: Фоновые задачи

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка показывает уровень вопроса: 🟢 [минимум](basics.ru.md) · 🟡 [production](advanced.ru.md) · 🔴 [масштаб и грани](expert.ru.md).

## 1. 🟢 Какой базовый класс подходит для долгоживущей фоновой работы?

- A) `Thread`
- B) `BackgroundService`
- C) `Task.Run` в контроллере
- D) `Timer` в статическом поле

<details><summary>Ответ</summary>

**B.** Хостируемый сервис запускается и останавливается вместе с приложением и получает `CancellationToken` остановки. `Task.Run` из запроса теряет работу при остановке.
→ [Минимум](basics.ru.md#задача)

</details>

## 2. 🟢 Как правильно выполнять периодическую работу в воркере?

- A) `while (true) Thread.Sleep(…)`
- B) `PeriodicTimer` и `await timer.WaitForNextTickAsync(ct)`
- C) `async void`
- D) рекурсивный `Task.Run`

<details><summary>Ответ</summary>

**B.** Ожидание асинхронное, поддерживает отмену и не накапливает пропущенные тики.
→ [Минимум](basics.ru.md#периодическая-работа-backgroundservice--periodictimer)

</details>

## 3. 🟢 Что вернёт `TryWrite` третьего элемента в ограниченный канал ёмкости 2?

- A) `True`
- B) `False`
- C) бросит исключение
- D) затрёт первый элемент

<details><summary>Ответ</summary>

**B.** Проверено: `True True False`. А `await WriteAsync` в этом случае ждёт, пока читатель освободит место.
→ [Минимум](basics.ru.md#ограниченная-очередь-и-обратное-давление)

</details>

## 4. 🟢 Что произойдёт при `FullMode = DropOldest`, если записать 1..4 в канал ёмкости 2?

- A) останутся `1,2`
- B) останутся `3,4`
- C) исключение
- D) канал заблокируется

<details><summary>Ответ</summary>

**B.** Проверено: остались `3,4` — старые вытесняются новыми.
→ [Минимум](basics.ru.md#ограниченная-очередь-и-обратное-давление)

</details>

## 5. 🟡 Как получить scoped-сервис (например `DbContext`) в воркере?

- A) внедрить в конструктор
- B) создать scope через `IServiceScopeFactory` на единицу работы
- C) сделать его static
- D) зарегистрировать как singleton

<details><summary>Ответ</summary>

**B.** Воркер — singleton; конструкторное внедрение scoped-сервиса — captive dependency. Проверено: из `CreateScope()` сервис получен.
→ [Production](advanced.ru.md#scoped-зависимости)

</details>

## 6. 🟡 Что произойдёт по умолчанию, если `ExecuteAsync` бросит необработанное исключение?

- A) ничего
- B) хост остановится (`StopHost`)
- C) воркер перезапустится сам
- D) исключение проглотится

<details><summary>Ответ</summary>

**B.** Проверено: сработал `ApplicationStopping`. При `Ignore` остановки не будет, воркер просто умрёт.
→ [Production](advanced.ru.md#исключения-в-воркере)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) токен `ct` отменяется при остановке хоста
- [ ] B) `try/catch` внутри цикла на единицу работы защищает воркер от одного сбойного задания
- [ ] C) `ShutdownTimeout` принудительно убивает воркер
- [ ] D) неограниченный канал при медленном потребителе растёт в памяти

<details><summary>Ответ</summary>

**A, B, D.** C неверно: проверено — `StopAsync` вернулся без исключения за ~300 мс, а воркер, игнорирующий токен, продолжил работу.
→ [Production](advanced.ru.md#graceful-shutdown)

</details>

## 8. 🟡 Открытый вопрос: как бы вы обрабатывали очередь четырьмя потоками с обратным давлением?

<details><summary>Ключевые пункты хорошего ответа</summary>

- ограниченный `Channel<T>` (`Wait`) — издатель ждёт;
- `Parallel.ForEachAsync` по `ReadAllAsync` с `MaxDegreeOfParallelism = 4` или 4 читателя;
- scope на каждый элемент, `ct` во все ожидания;
- `try/catch` на элемент, логирование.

</details>

## 9. 🔴 Приложение запущено в 3 экземплярах, `BackgroundService` шлёт письма раз в час. Сколько писем получит клиент?

- A) одно
- B) три — таймер работает в каждом экземпляре
- C) ноль
- D) зависит от `PeriodicTimer`

<details><summary>Ответ</summary>

**B.** Нужна блокировка/lease, единственный воркер-процесс или планировщик с координацией.
→ [Масштаб и грани](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Почему `Channel<T>` не подходит для заданий, которые нельзя терять?

- A) он медленный
- B) очередь в памяти пропадает при падении/перезапуске и принадлежит одному экземпляру
- C) он не поддерживает async
- D) он не потокобезопасен

<details><summary>Ответ</summary>

**B.** Для критичных заданий нужна durable-очередь: outbox в БД, брокер сообщений.
→ [Масштаб и грани](expert.ru.md#масштаб-когда-channelt-недостаточно)

</details>
