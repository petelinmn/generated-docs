---
title: "Квиз: Spectre.Console"
lang: ru
track: csharp
kind: library
doc: quiz
slug: spectre-console
part: 2
tags: [spectre-console, console, markup, prompts]
status: draft
updated: 2026-11-22
---

# Квиз: Spectre.Console

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка: 🟢 [основы](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [эксперт](expert.ru.md).

## 1. 🟢 Как вывести красный жирный текст?

- A) `Console.WriteLine("red bold")`
- B) `AnsiConsole.MarkupLine("[bold red]text[/]")`
- C) `AnsiConsole.Red("text")`
- D) `Console.ForegroundColor`

<details><summary>Ответ</summary>

**B.** Теги `[стиль]…[/]` в разметке.
→ [Основы](basics.ru.md#разметка-markup)

</details>

## 2. 🟢 Как вывести буквальные квадратные скобки?

- A) `\[ \]`
- B) удвоить: `[[` и `]]`
- C) нельзя
- D) `&#91;`

<details><summary>Ответ</summary>

**B.** Проверено: `[[not a tag]]` → `[not a tag]`.
→ [Основы](basics.ru.md#разметка-markup)

</details>

## 3. 🟢 Что делает `Markup.Escape(input)`?

- A) удаляет скобки
- B) экранирует разметку в пользовательском тексте
- C) делает текст красным
- D) шифрует

<details><summary>Ответ</summary>

**B.** Проверено: `[red]x[/]` выводится буквально.
→ [Основы](basics.ru.md#разметка-markup)

</details>

## 4. 🟢 Что произойдёт при неизвестном стиле `[unknown]`?

- A) тег проигнорируется
- B) `InvalidOperationException`
- C) красный текст
- D) пустая строка

<details><summary>Ответ</summary>

**B.** Проверено: «Could not find color or style 'unknown'».
→ [Основы](basics.ru.md#разметка-markup)

</details>

## 5. 🟡 Как протестировать вывод и prompts?

- A) перехватом `Console.Out`
- B) `TestConsole` из `Spectre.Console.Testing` и `IAnsiConsole` как зависимость
- C) `Process.Start`
- D) нельзя

<details><summary>Ответ</summary>

**B.** `Output` и `Input.PushTextWithEnter/PushKey` (проверено).
→ [Продвинутый](advanced.ru.md#тестирование-вывода-testconsole)

</details>

## 6. 🟡 Какой результат у `greet Ann --times 0` при `Validate()` с проверкой `Times < 1`?

- A) выполнится один раз
- B) ошибка `times must be >= 1`
- C) молча ничего
- D) исключение `NullReferenceException`

<details><summary>Ответ</summary>

**B.** Проверено.
→ [Продвинутый](advanced.ru.md#spectreconsolecli-команды-и-аргументы)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `SelectionPrompt` вернул `two` после `↓` и `Enter`
- [ ] B) prompts работают в неинтерактивной консоли
- [ ] C) `TextPrompt<int>` повторяет запрос при вводе `abc`
- [ ] D) `Panel` и `Table` — это `IRenderable`

<details><summary>Ответ</summary>

**A, C, D.** B неверно: проверено — `InvalidOperationException`.
→ [Основы](basics.ru.md#запросы-пользователю)

</details>

## 8. 🟡 Открытый вопрос: как построить CLI-утилиту с тестируемым выводом?

<details><summary>Ключевые пункты хорошего ответа</summary>

- `Spectre.Console.Cli`: настройки, команды, `Validate()`;
- `IAnsiConsole` внедряется в команды (не статический `AnsiConsole`);
- тесты на `TestConsole` с проверкой `Output`;
- коды возврата, обработка ошибок.

</details>

## 9. 🔴 Почему `TestConsole.Output` пуст, хотя команда выполнилась?

- A) тестовая консоль сломана
- B) команда писала в статический `AnsiConsole`, а не в сконфигурированную консоль
- C) нужен `Flush`
- D) нужен `Width`

<details><summary>Ответ</summary>

**B.** Проверено; внедряйте `IAnsiConsole`.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Почему один и тот же код даёт и цветной вывод, и чистый текст?

- A) два разных режима компиляции
- B) вывод строится из `IRenderable`-сегментов и адаптируется к возможностям терминала (`Profile.Capabilities`)
- C) пользователь выбирает вручную
- D) цвета хранятся в файле

<details><summary>Ответ</summary>

**B.** Проверено на `NoColors`/`Ansi = false`.
→ [Эксперт](expert.ru.md#внутреннее-устройство)

</details>
