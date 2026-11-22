---
title: "Quiz: FluentValidation"
lang: en
track: csharp
kind: library
doc: quiz
slug: fluentvalidation
part: 2
tags: [fluentvalidation, validation, rules]
status: draft
updated: 2026-11-22
---

# Quiz: FluentValidation

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `validator.Validate(model)` return?

- A) `bool`
- B) a `ValidationResult` with `IsValid` and `Errors`
- C) throws on error
- D) a string

<details><summary>Answer</summary>

**B.** An exception only with `ValidateAndThrow`.
→ [Basics](basics.en.md#the-result)

</details>

## 2. 🟢 What is the validator's base class called?

- A) `Validator<T>`
- B) `AbstractValidator<T>`
- C) `RuleSet<T>`
- D) `IValidatable<T>`

<details><summary>Answer</summary>

**B.** Rules are set in the constructor via `RuleFor`.
→ [Basics](basics.en.md#the-first-validator)

</details>

## 3. 🟢 How do you validate a nested `Address` object?

- A) it works by itself
- B) `RuleFor(u => u.Address).SetValidator(new AddressValidator())`
- C) `Must(...)`
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified: the errors come with the names `Address.Zip`.
→ [Basics](basics.en.md#the-first-validator)

</details>

## 4. 🟢 How many errors did the invalid user in the example give?

- A) 1
- B) 3
- C) 5
- D) 0

<details><summary>Answer</summary>

**C.** Verified: `Name`, `Email`, `Age` and two `Address.Zip` errors.
→ [Basics](basics.en.md#the-first-validator)

</details>

## 5. 🟡 How many errors does an empty name give with `NotEmpty().MinimumLength(5)` by default and with `CascadeMode.Stop`?

- A) 2 and 1
- B) 1 and 1
- C) 1 and 2
- D) 2 and 2

<details><summary>Answer</summary>

**A.** Verified.
→ [Advanced](advanced.en.md#rule-cascade)

</details>

## 6. 🟡 What happens if you call a synchronous `Validate` on a validator with `MustAsync`?

- A) everything works
- B) `AsyncValidatorInvokedSynchronouslyException`
- C) `IsValid = true` is returned
- D) a permanent block

<details><summary>Answer</summary>

**B.** Verified; use `ValidateAsync`.
→ [Advanced](advanced.en.md#asynchronous-rules)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) an `internal` validator is not registered by `AddValidatorsFromAssemblyContaining` by default
- [ ] B) a rule with `Severity.Warning` leaves `IsValid = true`
- [ ] C) `RuleForEach` includes the index in the property name
- [ ] D) `WithName` changes the property name in the message

<details><summary>Answer</summary>

**A, C, D.** B is wrong: verified — `IsValid = False`.
→ [Advanced](advanced.en.md#severity)

</details>

## 8. 🟡 Open question: how do you organize validating Minimal API input?

<details><summary>Key points of a good answer</summary>

- the validator in DI (`AddValidatorsFromAssemblyContaining`);
- `ValidateAsync` in an endpoint filter/the endpoint;
- `Results.ValidationProblem(result.ToDictionary())` → `400`;
- uniqueness — a DB index, not only a validator.

</details>

## 9. 🔴 A test on the error text fails on CI. Why?

- A) randomness
- B) the text depends on the culture
- C) an error in the rule
- D) a cache

<details><summary>Answer</summary>

**B.** Verified: Russian and English messages. Check `ErrorCode` or fix the culture.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 A validator checked name uniqueness, but the DB has a duplicate. Why?

- A) a validator does not work with a DB
- B) a race between the check and the insert; a unique index is needed
- C) `CascadeMode`
- D) the DI cache

<details><summary>Answer</summary>

**B.** A validator is the "first line"; the final protection is a DB constraint.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>
