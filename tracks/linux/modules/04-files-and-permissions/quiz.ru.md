---
title: "Квиз: файлы и права"
lang: ru
track: linux
kind: module
doc: quiz
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Квиз: файлы и права

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что такое `rw-r-----` в числах?

- A) `644`
- B) `640`
- C) `460`
- D) `600`

<details><summary>Ответ</summary>

**B.** 4+2, 4, 0.

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что разрешает `x` у каталога?

- A) Перечислять имена
- B) Проходить и обращаться к записям по имени
- C) Запускать все файлы внутри
- D) Удалить каталог

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Кто может передать файл другому пользователю через `chown`?

- A) Владелец
- B) Только root
- C) Любой из группы
- D) Любой с `w`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Какая команда добавит выполнение только владельцу?

- A) `chmod +x f`
- B) `chmod u+x f`
- C) `chmod 111 f`
- D) `chown u+x f`

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 При `umask 027` новый обычный файл получит

- A) `750`
- B) `640`
- C) `027`
- D) `644`

<details><summary>Ответ</summary>

**B.** `666` без битов `027`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Файл с режимом `060`, владелец `ann`, и `ann` состоит в его группе. Прочитает ли `ann`?

- A) Да, через группу
- B) Нет
- C) Только через `sudo`, и никак иначе
- D) Да, владелец может всегда

<details><summary>Ответ</summary>

**B.** Применяется только класс владельца, хотя `ann` может изменить режим.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все верные утверждения о каталоге с режимом `2775`

- [ ] A) Новые файлы получают его группу
- [ ] B) Новые подкаталоги наследуют setgid
- [ ] C) Удалять свои файлы могут только владельцы
- [ ] D) Остальные могут создавать в нём файлы

<details><summary>Ответ</summary>

**A, B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Найди проблему

```bash
chmod -R 644 /var/www/site
```

<details><summary>Ответ</summary>

Каталоги теряют `x`, и по ним не пройти. Задайте через `find` отдельно каталогам (`755`) и файлам (`644`).

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что останется от режима `6755` после `chown ann` от root на обычном файле?

- A) `6755`
- B) `755`
- C) `4755`
- D) `000`

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 У файла ACL `user:bob:r`. После `chmod g-r`

- A) bob всё ещё читает
- B) маска теряет `r`, и bob читать не может
- C) ACL удаляется
- D) ничего не меняется

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 `find -perm /6000` находит файлы

- A) ровно с режимом `6000`
- B) с setuid или setgid
- C) с обоими сразу
- D) без них

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: веб-сервер отвечает «permission denied» на файл с режимом `644`. Что проверить?

<details><summary>Ключевые пункты ответа</summary>

- Под каким пользователем работает сервер (`ps`, `id`).
- `x` на каждом каталоге пути (`namei -l`).
- ACL, атрибуты, параметры монтирования.
- Сообщения AppArmor или SELinux в журналах.

→ [Экспертная теория](expert.ru.md)

</details>
