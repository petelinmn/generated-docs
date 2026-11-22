---
title: "Квиз: DI, конфигурация, логирование"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, service-lifetime, options-pattern, configuration, logging]
status: draft
updated: 2026-11-22
---

# Квиз: DI, конфигурация, логирование

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Сколько экземпляров будет создано?

```csharp
sc.AddSingleton<SingletonSvc>(); sc.AddScoped<ScopedSvc>(); sc.AddTransient<TransientSvc>();
// two scopes; in each scope every service is requested twice
```

- A) singleton=1, scoped=1, transient=1
- B) singleton=1, scoped=2, transient=4
- C) singleton=2, scoped=2, transient=2
- D) singleton=1, scoped=4, transient=4

<details><summary>Ответ</summary>

**B.** Singleton — один на приложение; Scoped — один на область (две области → два); Transient — каждый запрос новый (2 × 2 = 4). Проверено запуском.
→ [Базовая теория](basics.ru.md#время-жизни-сервисов)

</details>

## 2. 🟢 Что бросит `GetRequiredService<IGreeter>()`, если сервис не зарегистрирован, и что вернёт `GetService<IGreeter>()`?

- A) `NullReferenceException` / `null`
- B) `InvalidOperationException` / `null`
- C) `KeyNotFoundException` / исключение
- D) `null` / `null`

<details><summary>Ответ</summary>

**B.** `GetRequiredService` бросает `InvalidOperationException` («No service for type … has been registered»), `GetService` возвращает `null` (проверено).
→ [Базовая теория](basics.ru.md#внедрение-зависимостей)

</details>

## 3. 🟢 Какой источник конфигурации победит, если один и тот же ключ задан в `appsettings.json`, переменной окружения и командной строке?

- A) `appsettings.json`
- B) переменная окружения
- C) командная строка
- D) побеждает первый добавленный

<details><summary>Ответ</summary>

**C.** Побеждает провайдер, добавленный **позже**; в типичном хосте порядок — файлы, переменные окружения, командная строка (проверено на слоях: окружение перекрыло память, `--Mail:Host=cli` — тоже).
→ [Базовая теория](basics.ru.md#конфигурация)

</details>

## 4. 🟢 Почему нельзя писать `log.LogInformation($"User {id} logged in")`?

- A) не компилируется
- B) теряется структура сообщения и текст форматируется всегда, даже если уровень выключен
- C) не работает с уровнем `Information`
- D) исключения не логируются

<details><summary>Ответ</summary>

**B.** Шаблон `"User {UserId} logged in"` с аргументом сохраняет именованное свойство для поиска и агрегации, а форматирование идёт только при включённом уровне.
→ [Базовая теория](basics.ru.md#логирование)

</details>

## 5. 🟡 Что произойдёт при сборке контейнера?

```csharp
sc.AddScoped<IScoped, Impl>(); sc.AddSingleton<Needs>();   // class Needs(IScoped s)
using var p = sc.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
```

- A) всё нормально
- B) исключение: нельзя получить scoped-сервис в singleton
- C) `Needs` станет scoped автоматически
- D) предупреждение компилятора

<details><summary>Ответ</summary>

**B.** Проверка находит «захваченную зависимость»: «Cannot consume scoped service 'IScoped' from singleton 'Needs'» (проверено). Без этих опций в `Production` ошибки при сборке не будет.
→ [Продвинутая теория](advanced.ru.md#проблема-захваченной-зависимости)

</details>

## 6. 🟡 Что вернёт код?

```csharp
sc.AddSingleton<IGreeter, EnGreeter>();
sc.AddSingleton<IGreeter, RuGreeter>();
Console.WriteLine(provider.GetRequiredService<IGreeter>().Hello());
Console.WriteLine(provider.GetServices<IGreeter>().Count());
```

<details><summary>Ответ</summary>

```text
привет
2
```

`GetRequiredService` отдаёт последнюю регистрацию (`RuGreeter`), `GetServices` — все (проверено).
→ [Продвинутая теория](advanced.ru.md#несколько-реализаций-одного-интерфейса)

</details>

## 7. 🟡 Какой интерфейс использовать в singleton-сервисе, которому нужны свежие настройки после изменения файла?

- A) `IOptions<T>`
- B) `IOptionsSnapshot<T>`
- C) `IOptionsMonitor<T>`
- D) любой

<details><summary>Ответ</summary>

**C.** `IOptionsMonitor<T>` — singleton с немедленным обновлением и событием `OnChange`. `IOptions<T>` не обновляется, а `IOptionsSnapshot<T>` — scoped и обновляется только в новой области (проверено: в той же области значение осталось старым).
→ [Продвинутая теория](advanced.ru.md#options-проверка-и-перезагрузка)

</details>

## 8. 🟡 Выберите все верные утверждения о `ValidateOnStart`

- [ ] A) проверяет настройки при запуске хоста
- [ ] B) при ошибке хост не запускается (`OptionsValidationException`)
- [ ] C) заменяет атрибуты `[Required]`/`[Range]`
- [ ] D) для `ValidateDataAnnotations` нужен отдельный пакет `Microsoft.Extensions.Options.DataAnnotations`

<details><summary>Ответ</summary>

**A, B, D.** C неверно: `ValidateOnStart` лишь выбирает **момент** проверки, а сами правила задают атрибуты или `Validate(...)`. Проверено: с `Port = 0` и без `Host` запуск падает с 2 проблемами.
→ [Продвинутая теория](advanced.ru.md#проверка-настроек)

</details>

## 9. 🟡 Открытый вопрос: singleton-сервису нужен `DbContext` (scoped). Как это правильно сделать?

<details><summary>Ключевые пункты хорошего ответа</summary>

- напрямую внедрять нельзя — «захваченная зависимость»: один `DbContext` на все запросы, он не потокобезопасен;
- внедрить `IServiceScopeFactory`, на каждую операцию создавать область и получать `DbContext` из неё; область освободить через `using`;
- либо сделать сам сервис Scoped, если он не обязан быть singleton;
- включить `ValidateScopes`/`ValidateOnBuild`, чтобы ошибка находилась рано ([15 · EF Core](../15-ef-core/overview.ru.md)).

</details>

## 10. 🔴 Что получится?

```csharp
var sc = new ServiceCollection(); sc.AddTransient<D>();   // D : IDisposable
var p = sc.BuildServiceProvider();
for (int i = 0; i < 3; i++) p.GetRequiredService<D>();
Console.WriteLine($"created={D.Created} disposed={D.Disposed}");
```

- A) `created=3 disposed=3`
- B) `created=3 disposed=0`
- C) `created=1 disposed=0`
- D) `created=0 disposed=0`

<details><summary>Ответ</summary>

**B.** Контейнер хранит созданные одноразовые Transient-экземпляры до освобождения провайдера или области (после `p.Dispose()` стало `disposed=3`). В долгоживущем корневом провайдере это утечка: запрашивайте такие сервисы в короткой области.
→ [Экспертная теория](expert.ru.md#освобождение)

</details>

## 11. 🔴 Какой конструктор выберет контейнер?

```csharp
class Many { public Many() {} public Many(X x) {} public Many(X x, Y y) {} }   // X and Y are registered
```

- A) беспараметрический
- B) `Many(X x)`
- C) `Many(X x, Y y)`
- D) бросит исключение

<details><summary>Ответ</summary>

**C.** Выбирается конструктор с наибольшим числом параметров, которые можно разрешить (проверено: `X,Y`). Неоднозначность возникает, когда подходят два конструктора, ни один из которых не включает другой.
→ [Экспертная теория](expert.ru.md#как-устроен-контейнер)

</details>
