---
title: "Квиз: FluentValidation"
lang: ru
track: csharp
kind: library
doc: quiz
slug: fluentvalidation
part: 2
tags: [fluentvalidation, validation, rules]
status: draft
updated: 2026-11-22
---

# Квиз: FluentValidation

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка: 🟢 [основы](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [эксперт](expert.ru.md).

## 1. 🟢 Что возвращает `validator.Validate(model)`?

- A) `bool`
- B) `ValidationResult` с `IsValid` и `Errors`
- C) бросает исключение при ошибке
- D) строку

<details><summary>Ответ</summary>

**B.** Исключение — только у `ValidateAndThrow`.
→ [Основы](basics.ru.md#результат)

</details>

## 2. 🟢 Как называется базовый класс валидатора?

- A) `Validator<T>`
- B) `AbstractValidator<T>`
- C) `RuleSet<T>`
- D) `IValidatable<T>`

<details><summary>Ответ</summary>

**B.** Правила задаются в конструкторе через `RuleFor`.
→ [Основы](basics.ru.md#первый-валидатор)

</details>

## 3. 🟢 Как провалидировать вложенный объект `Address`?

- A) само сработает
- B) `RuleFor(u => u.Address).SetValidator(new AddressValidator())`
- C) `Must(...)`
- D) нельзя

<details><summary>Ответ</summary>

**B.** Проверено: ошибки приходят с именами `Address.Zip`.
→ [Основы](basics.ru.md#первый-валидатор)

</details>

## 4. 🟢 Сколько ошибок дал неверный пользователь из примера?

- A) 1
- B) 3
- C) 5
- D) 0

<details><summary>Ответ</summary>

**C.** Проверено: `Name`, `Email`, `Age` и две ошибки `Address.Zip`.
→ [Основы](basics.ru.md#первый-валидатор)

</details>

## 5. 🟡 Сколько ошибок даёт пустое имя при `NotEmpty().MinimumLength(5)` по умолчанию и с `CascadeMode.Stop`?

- A) 2 и 1
- B) 1 и 1
- C) 1 и 2
- D) 2 и 2

<details><summary>Ответ</summary>

**A.** Проверено.
→ [Продвинутый](advanced.ru.md#каскад-правил)

</details>

## 6. 🟡 Что будет, если вызвать синхронный `Validate` для валидатора с `MustAsync`?

- A) всё сработает
- B) `AsyncValidatorInvokedSynchronouslyException`
- C) вернётся `IsValid = true`
- D) блокировка навсегда

<details><summary>Ответ</summary>

**B.** Проверено; используйте `ValidateAsync`.
→ [Продвинутый](advanced.ru.md#асинхронные-правила)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `internal`-валидатор по умолчанию не регистрируется `AddValidatorsFromAssemblyContaining`
- [ ] B) правило со `Severity.Warning` оставляет `IsValid = true`
- [ ] C) `RuleForEach` включает индекс в имя свойства
- [ ] D) `WithName` меняет имя свойства в сообщении

<details><summary>Ответ</summary>

**A, C, D.** B неверно: проверено — `IsValid = False`.
→ [Продвинутый](advanced.ru.md#серьёзность)

</details>

## 8. 🟡 Открытый вопрос: как вы организуете валидацию входа Minimal API?

<details><summary>Ключевые пункты хорошего ответа</summary>

- валидатор в DI (`AddValidatorsFromAssemblyContaining`);
- `ValidateAsync` в endpoint filter/эндпоинте;
- `Results.ValidationProblem(result.ToDictionary())` → `400`;
- уникальность — индекс в БД, а не только валидатор.

</details>

## 9. 🔴 Тест на текст ошибки падает на CI. Почему?

- A) случайность
- B) текст зависит от культуры
- C) ошибка в правиле
- D) кэш

<details><summary>Ответ</summary>

**B.** Проверено: русские и английские сообщения. Проверяйте `ErrorCode` или фиксируйте культуру.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Валидатор проверил уникальность имени, но в БД дубликат. Почему?

- A) валидатор не работает с БД
- B) гонка между проверкой и вставкой; нужен уникальный индекс
- C) `CascadeMode`
- D) кэш DI

<details><summary>Ответ</summary>

**B.** Валидатор — «первая линия», окончательная защита — ограничение БД.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>
