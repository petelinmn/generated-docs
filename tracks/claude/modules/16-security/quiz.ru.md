---
title: "Безопасность и compliance: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
status: draft
updated: 2026-11-25
---

# Безопасность и compliance: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Как относиться к аргументам инструмента от модели?

- A) как к коду своего приложения
- B) как к настройкам
- C) как к недоверенному вводу
- D) как к проверенным

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Где хранить API-ключ?

- A) в хранилище секретов или переменной окружения
- B) в репозитории
- C) в JavaScript на странице
- D) в системном промпте

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что делать с необратимым действием агента?

- A) выполнять быстрее
- B) описать в промпте
- C) ничего
- D) требовать подтверждения человека

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Подозрение на утечку ключа:

- A) подождать
- B) отключить или удалить ключ
- C) сменить модель
- D) очистить кэш

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Куда помещать содержимое чужого письма?

- A) в `stop_sequences`
- B) в `tool_result`
- C) в системный промпт
- D) в имя инструмента

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что верно о защите от инъекций?

- [ ] A) ограничение прав обязательно
- [ ] B) решается одной фразой в промпте
- [ ] C) не нужна для чтения веб-страниц
- [ ] D) строится слоями

<details><summary>Ответ</summary>

**A, D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Sandbox Claude Code на Windows без WSL:

- A) команды выполняются без него
- B) включён по умолчанию
- C) использует Seatbelt
- D) блокирует всё

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Значение `inference_geo` по умолчанию?

- A) `eu`
- B) регион ключа
- C) `global`
- D) `us`

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что даёт Workload Identity Federation?

- A) бесплатные запросы
- B) ZDR
- C) шифрование промптов на клиенте
- D) короткоживущие токены вместо статического ключа

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Почему `startswith` плох для проверки пути?

- A) требует абсолютных путей
- B) пропускает соседний каталог с тем же префиксом
- C) медленный
- D) не работает в Python

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что не подпадает под ZDR?

- [ ] A) обычный Messages API
- [ ] B) подсчёт токенов
- [ ] C) Managed Agents
- [ ] D) MCP connector

<details><summary>Ответ</summary>

**C, D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: агент читает входящие письма и может отправлять ответы и искать в CRM. Какие меры?

<details><summary>Ответ</summary>

Письма — только в `tool_result` в JSON; узкий доступ к CRM по отправителю; отправка с подтверждением или только шаблоны и только отправителю; фильтр входа и выхода; никаких произвольных ссылок и вложений наружу; журнал действий; тесты с инъекциями; лимиты.
→ [Продвинутая теория](advanced.ru.md)

</details>
