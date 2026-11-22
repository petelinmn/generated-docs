---
title: "Квиз: Async/await"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 10-async-await
part: 2
tags: [async-await, task, configureawait, deadlock, valuetask, cancellation-token]
status: draft
updated: 2026-11-22
---

# Квиз: Async/await

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что происходит с потоком, когда метод доходит до `await` незавершённой задачи?

- A) поток блокируется до завершения задачи
- B) метод приостанавливается, управление возвращается вызывающему, поток свободен
- C) создаётся новый поток, который ждёт задачу
- D) метод продолжает выполняться, а результат подставляется позже

<details><summary>Ответ</summary>

**B.** `await` не блокирует: метод превращается в конечный автомат, возвращает вызывающему незавершённую задачу, а продолжение запускается после завершения ожидания. A — это `.Wait()`/`.Result`. C неверно: ожидание I/O не занимает потока вовсе. D описывает fire-and-forget, а не `await`.
→ [Базовая теория](basics.ru.md#что-происходит-на-await)

</details>

## 2. 🟢 Что выведет код?

```csharp
static async Task Sync()
{
    Console.WriteLine("before await");
    await Task.Yield();
    Console.WriteLine("after await");
}

var t = Sync();
Console.WriteLine("returned to caller");
await t;
```

<details><summary>Ответ</summary>

```text
before await
returned to caller
after await
```

Код до первого незавершённого `await` выполняется синхронно в потоке вызывающего, поэтому «before await» печатается раньше. `Task.Yield()` принудительно приостанавливает метод — вызывающий получает управление и печатает вторую строку.
→ [Базовая теория](basics.ru.md#что-происходит-на-await)

</details>

## 3. 🟢 Две независимые операции по 200 мс. Как выполнить их за ~200 мс?

- A) `await a(); await b();`
- B) `var ta = a(); var tb = b(); await Task.WhenAll(ta, tb);`
- C) `await Task.WhenAny(a(), b());`
- D) `Task.Run(() => a().Wait()); await b();`

<details><summary>Ответ</summary>

**B.** Обе операции стартуют сразу («горячие» задачи), `WhenAll` ждёт обе. A выполняет их последовательно, ~400 мс (проверено). C вернёт управление после первой завершённой — вторая остаётся без ожидания, исключения потеряются. D блокирует поток пула ожиданием.
→ [Базовая теория](basics.ru.md#последовательно-и-параллельно)

</details>

## 4. 🟢 Выберите все верные утверждения

- [ ] A) `async void` подходит для обработчиков событий
- [ ] B) исключение из асинхронного метода без `await` на его задаче будет сразу брошено вызывающему
- [ ] C) `Task.Run` нужен, чтобы сделать I/O асинхронным
- [ ] D) метод, который может работать долго, должен принимать `CancellationToken`

<details><summary>Ответ</summary>

**A, D.** A: у обработчиков событий сигнатура с `void`, это единственное оправданное применение. B неверно: исключение сохраняется в задаче и выбрасывается только при `await` (или теряется, если ждать не будут). C неверно: `Task.Run` занимает поток пула и не делает I/O асинхронным — нужен настоящий асинхронный API. D верно: токен — стандартный способ кооперативной отмены.
→ [Базовая теория](basics.ru.md#ошибки-новичка)

</details>

## 5. 🟢 Какой тип исключения будет в `catch`?

```csharp
using var cts = new CancellationTokenSource(100);
try { await Task.Delay(1000, cts.Token); }
catch (OperationCanceledException e) { Console.WriteLine(e.GetType().Name); }
```

- A) `OperationCanceledException`
- B) `TaskCanceledException`
- C) `TimeoutException`
- D) `AggregateException`

<details><summary>Ответ</summary>

**B.** `Task.Delay` с отменённым токеном бросает `TaskCanceledException` — наследника `OperationCanceledException` (поэтому `catch` по базовому типу его ловит). `TimeoutException` бросает `Task.WaitAsync`, но не токен. `AggregateException` появляется только при `Wait()`/`Result`.
→ [Базовая теория](basics.ru.md#отмена-cancellationtoken)

</details>

## 6. 🟡 Что не так с кодом (обработчик UI с контекстом синхронизации)?

```csharp
void Button_Click(object sender, EventArgs e)
{
    var data = LoadAsync().Result;
    label.Text = data;
}

async Task<string> LoadAsync()
{
    await Task.Delay(100);
    return "ok";
}
```

<details><summary>Ответ</summary>

Deadlock. UI-поток блокируется на `.Result`, а продолжение `LoadAsync` после `await` хочет вернуться именно в UI-контекст, который занят. Проверено на однопоточном контексте: `Wait(500)` вернул `False`. Исправление: сделать обработчик `async void` и использовать `await LoadAsync()`. Альтернатива для библиотечного кода — `ConfigureAwait(false)` в `LoadAsync` (тогда продолжение не требует контекста), но блокировка потока остаётся плохой практикой.
→ [Продвинутая теория](advanced.ru.md#sync-over-async-и-deadlock)

</details>

## 7. 🟡 Что выведет код?

```csharp
static async Task F(string m) { await Task.Delay(10); throw new Exception(m); }

var all = Task.WhenAll(F("one"), F("two"));
try { await all; }
catch (Exception e) { Console.WriteLine($"{e.Message}; {all.Exception!.InnerExceptions.Count}"); }
```

- A) `one; 1`
- B) `one; 2`
- C) `two; 2`
- D) `One or more errors occurred. (…)`, затем `2`

<details><summary>Ответ</summary>

**B.** `await` разворачивает `AggregateException` и бросает **первое** исключение (`one`); все исключения остаются в `Task.Exception` — их две. D было бы при `all.Wait()`: там приходит `AggregateException`, сообщение которого начинается с «One or more errors occurred.» (проверено: порядок внутренних сообщений не гарантирован).
→ [Продвинутая теория](advanced.ru.md#исключения-и-aggregateexception)

</details>

## 8. 🟡 Выберите все верные утверждения о `ValueTask<T>`

- [ ] A) позволяет не выделять память, когда операция завершается синхронно
- [ ] B) один экземпляр можно безопасно `await`-ить из нескольких мест параллельно
- [ ] C) `AsTask()` даёт обычную задачу, если результат нужно сохранить
- [ ] D) всегда быстрее `Task`

<details><summary>Ответ</summary>

**A, C.** A — основная цель типа. B неверно: контракт — не более одного `await`. C верно. D неверно: структура копируется, а в асинхронном случае выигрыша нет; применяйте после замеров.
→ [Продвинутая теория](advanced.ru.md#valuetask)

</details>

## 9. 🟡 Где нужен `ConfigureAwait(false)`?

- A) в обработчиках кнопок UI, которые меняют элементы формы после `await`
- B) в коде библиотек, которая не знает окружения вызывающего
- C) в каждом методе контроллера ASP.NET Core, иначе будет deadlock
- D) нигде: он устарел

<details><summary>Ответ</summary>

**B.** Библиотека не должна навязывать возврат в контекст вызывающего. A неверно: там после `await` нужен UI-поток, и `ConfigureAwait(false)` сломает код. C неверно: в ASP.NET Core нет контекста синхронизации, deadlock невозможен, а `ConfigureAwait(false)` ничего не меняет. D неверно: в библиотеках он остаётся рекомендованной практикой.
→ [Продвинутая теория](advanced.ru.md#контекст-синхронизации-и-configureawait)

</details>

## 10. 🟡 Открытый вопрос: как добавить таймаут к асинхронной операции, которая принимает `CancellationToken`, и отличить его от отмены вызывающим?

<details><summary>Ключевые пункты хорошего ответа</summary>

- создать связанный источник: `CancellationTokenSource.CreateLinkedTokenSource(outerToken)`;
- вызвать `cts.CancelAfter(timeout)` и передать `cts.Token` операции;
- поймать `OperationCanceledException` с фильтром `when (!outerToken.IsCancellationRequested)` — это значит, что сработал именно таймаут — и бросить `TimeoutException`;
- не путать с `Task.WaitAsync(timeout)`: он прекращает ожидание, но не отменяет саму операцию;
- освободить источник через `using`.

</details>

## 11. 🔴 Что выведет код?

```csharp
var al = new AsyncLocal<string>();
al.Value = "outer";

static async Task SetIt(AsyncLocal<string> a) { a.Value = "set in async method"; await Task.Yield(); }
static void SetSync(AsyncLocal<string> a) { a.Value = "set in sync method"; }

await SetIt(al);
Console.WriteLine(al.Value);
SetSync(al);
Console.WriteLine(al.Value);
```

- A) `set in async method` / `set in sync method`
- B) `outer` / `set in sync method`
- C) `outer` / `outer`
- D) `set in async method` / `outer`

<details><summary>Ответ</summary>

**B.** `ExecutionContext` течёт вниз по цепочке, но `async`-метод работает на собственной копии: после возврата изменение не видно вызывающему. Синхронный метод меняет контекст вызывающего напрямую. Проверено запуском.
→ [Экспертная теория](expert.ru.md#контекст-выполнения-executioncontext-и-asynclocalt)

</details>

## 12. 🔴 Найдите ошибку

```csharp
await Task.Factory.StartNew(async () =>
{
    await Task.Delay(500);
    Console.WriteLine("work done");
});
Console.WriteLine("continue");
```

<details><summary>Ответ</summary>

`StartNew` с `async`-лямбдой возвращает `Task<Task>`: `await` ждёт только внешнюю задачу, которая завершается, когда лямбда дошла до первого `await`. Поэтому «continue» печатается раньше «work done» (проверено: `await` вернул управление раньше 400 мс). Исправление — `await Task.Run(async () => …)`, который распаковывает вложенную задачу, либо `await (await Task.Factory.StartNew(…))`.
→ [Экспертная теория](expert.ru.md#taskrun-и-taskfactorystartnew)

</details>
