---
title: "Квиз: Загрузка файлов"
lang: ru
track: csharp
kind: recipe
doc: quiz
slug: file-upload
part: 2
tags: [file-upload, iformfile, multipart, path-traversal, range-requests]
status: draft
updated: 2026-11-22
---

# Квиз: Загрузка файлов

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка показывает уровень вопроса: 🟢 [минимум](basics.ru.md) · 🟡 [production](advanced.ru.md) · 🔴 [масштаб и грани](expert.ru.md).

## 1. 🟢 Какой тип параметра принимает файл из `multipart/form-data` в Minimal API?

- A) `Stream`
- B) `IFormFile`
- C) `byte[]`
- D) `FileInfo`

<details><summary>Ответ</summary>

**B.** `IFormFile` привязывается из формы; `file.Length`, `CopyToAsync` и другие члены дают доступ к содержимому.
→ [Минимум](basics.ru.md#загрузка-через-iformfile)

</details>

## 2. 🟢 Что вернёт `Path.GetFileName("../../evil.txt")`?

- A) `../../evil.txt`
- B) `evil.txt`
- C) пустую строку
- D) исключение

<details><summary>Ответ</summary>

**B.** Каталоговая часть отбрасывается — проверено в примере. Но лучше вообще не использовать клиентское имя на диске.
→ [Минимум](basics.ru.md#загрузка-через-iformfile)

</details>

## 3. 🟢 Какой код вернётся, если тело превышает лимит Kestrel?

- A) `400`
- B) `413`
- C) `500`
- D) `200`

<details><summary>Ответ</summary>

**B.** Проверено: при `MaxRequestBodySize = 1 МБ` загрузка 2 МБ → `413 Payload Too Large`.
→ [Минимум](basics.ru.md#лимит-размера)

</details>

## 4. 🟢 Что даёт `enableRangeProcessing: true` в `Results.File`?

- A) сжатие
- B) поддержку `Range`-запросов (ответ `206`)
- C) кэширование в браузере
- D) шифрование

<details><summary>Ответ</summary>

**B.** Проверено: `Accept-Ranges: bytes`, `Range: bytes=0-99` → `206`, длина 100.
→ [Минимум](basics.ru.md#скачивание)

</details>

## 5. 🟡 Как надёжно определить, что `x.png` — действительно PNG?

- A) по `Content-Type` клиента
- B) по расширению
- C) по сигнатуре (магическим байтам) содержимого
- D) по размеру

<details><summary>Ответ</summary>

**C.** Проверено: `x.png` с произвольными байтами прошла проверку расширения, но проверка сигнатуры её отклонила.
→ [Production](advanced.ru.md#проверка-типа-файла)

</details>

## 6. 🟡 Что произойдёт при `UseAntiforgery()` и форме без токена на эндпоинте с `IFormFile`?

- A) `200`
- B) `400`
- C) `500`
- D) `404`

<details><summary>Ответ</summary>

**B.** Проверено; эндпоинт с `DisableAntiforgery()` ответил `200`.
→ [Production](advanced.ru.md#antiforgery)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) серверное имя файла (GUID) защищает от перезаписи чужих файлов
- [ ] B) `IFormFile` не буферизует форму
- [ ] C) `MultipartReader` позволяет читать секции потоком
- [ ] D) файлы удобнее хранить в `wwwroot`

<details><summary>Ответ</summary>

**A, C.** B неверно: `IFormFile` буферизует; D — небезопасно, храните вне веб-корня и отдавайте через проверяющий эндпоинт.
→ [Production](advanced.ru.md#безопасность-путей-и-хранилища)

</details>

## 8. 🟡 Открытый вопрос: опишите безопасную загрузку аватара.

<details><summary>Ключевые пункты хорошего ответа</summary>

- белый список расширений (png/jpg) и проверка сигнатуры, лимит размера;
- серверное имя, хранение вне веб-корня или в объектном хранилище;
- аутентификация + antiforgery (или токен в заголовке);
- ограничение частоты загрузок, переобработка/перекодирование изображения;
- отдача с безопасным `Content-Type`.

</details>

## 9. 🔴 Загрузка работает локально, но за балансировщиком иногда «теряет» файлы. Причина?

- A) баг `IFormFile`
- B) файл лежит на диске одного экземпляра, а следующий запрос попадает на другой
- C) слишком большой лимит
- D) нет antiforgery

<details><summary>Ответ</summary>

**B.** Нужно общее хранилище: объектное (S3/Blob) или сетевой том.
→ [Масштаб и грани](expert.ru.md#масштаб)

</details>

## 10. 🔴 Зачем отдавать загруженные файлы с `Content-Disposition: attachment`?

- A) для экономии трафика
- B) чтобы загруженный HTML/SVG не выполнялся на вашем домене (XSS)
- C) для сжатия
- D) для кэширования

<details><summary>Ответ</summary>

**B.** Иначе браузер отобразит активное содержимое от имени вашего домена.
→ [Масштаб и грани](expert.ru.md#грани-и-подвохи)

</details>
