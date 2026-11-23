---
title: "Квиз: CRD и операторы"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 19-crd-and-operators
part: 3
tags: [crd, operator, cel, status-subresource]
status: draft
updated: 2026-11-23
---

# Квиз: CRD и операторы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что делает CRD?

- A) Запускает контроллер для нового типа
- B) Добавляет в API кластера новый тип объектов со схемой
- C) Создаёт Deployment
- D) Устанавливает Helm-чарт

<details><summary>Ответ</summary>

**B.** Сам по себе CRD только хранит и проверяет объекты; действовать должен контроллер (вместе они образуют оператор).
→ [Обзор](overview.ru.md#коротко)

</details>

## 2. 🟢 Как называется CRD для `kind: Widget`, `plural: widgets`, `group: example.com`?

- A) `widget.example.com`
- B) `widgets.example.com`
- C) `example.com/widgets`
- D) `widgets`

<details><summary>Ответ</summary>

**B.** Формат `<plural>.<group>`.
→ [Базовая теория](basics.ru.md#создаём-crd)

</details>

## 3. 🟢 В манифесте объекта нет поля `replicas`, а в схеме `default: 1`. Что в `get -o yaml`?

- A) Поля нет
- B) `replicas: 1`
- C) Ошибка
- D) `replicas: null`

<details><summary>Ответ</summary>

**B.** Значение подставляет API-сервер.
→ [Базовая теория](basics.ru.md#создаём-объект)

</details>

## 4. 🟢 Что вернёт попытка создать объект с `replicas: 9` при `maximum: 5`?

- A) Объект создастся, значение обрежется до 5
- B) Ошибка валидации: значение должно быть не больше 5
- C) Предупреждение и создание
- D) Объект создастся с 1

<details><summary>Ответ</summary>

**B.** `spec.replicas in body should be less than or equal to 5`.
→ [Базовая теория](basics.ru.md#проверки-схемы)

</details>

## 5. 🟡 Правило CEL `self.tier != 'free' || self.replicas <= 2` и объект без `tier`. Что произойдёт?

- A) Правило выполнится как истинное
- B) Ошибка `no such key: tier`, изменение отклонено
- C) Правило пропускается
- D) `tier` получит значение `free`

<details><summary>Ответ</summary>

**B.** Нужно `!has(self.tier) || …`.
→ [Продвинутая теория](advanced.ru.md#правила-cel)

</details>

## 6. 🟡 Вы применили объект с `color: red`, которого нет в схеме, с `--validate=false`. Что в итоге?

- A) Поле сохранилось
- B) Сервер молча отбросил поле (pruning)
- C) Ошибка сервера
- D) Объект не создан

<details><summary>Ответ</summary>

**B.** Без `--validate=false` `kubectl` отказал бы со `strict decoding error`.
→ [Продвинутая теория](advanced.ru.md#pruning-неизвестные-поля)

</details>

## 7. 🟡 `kubectl patch widget w1 -p '{"status":{"phase":"Ready"}}'` при включённом subresource `status`. Результат?

- A) Статус обновлён
- B) `patched (no change)`
- C) Ошибка прав
- D) Объект удалён

<details><summary>Ответ</summary>

**B.** Нужен `--subresource=status`.
→ [Продвинутая теория](advanced.ru.md#status-subresource)

</details>

## 8. 🟡 Что произойдёт с ConfigMap, у которого `ownerReferences` указывают на Widget, после удаления Widget?

- A) Останется
- B) Будет удалён сборщиком мусора
- C) Станет `Terminating` навсегда
- D) Перейдёт к другому владельцу

<details><summary>Ответ</summary>

**B.** В опыте `configmaps "owned" not found`.
→ [Продвинутая теория](advanced.ru.md#ownerreferences-и-сборка-мусора)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) Оператору нужны права RBAC на свои ресурсы и их `status`
- [ ] B) Цикл сверки должен быть идемпотентным
- [ ] C) Оператор работает внутри API-сервера
- [ ] D) cert-manager использует CRD `Certificate` и `Issuer`

<details><summary>Ответ</summary>

**A, B, D.** C неверно: оператор — отдельное приложение (под), которое общается с API.
→ [Продвинутая теория](advanced.ru.md#игрушечный-оператор)

</details>

## 10. 🔴 Найдите ошибку

> «Оператор реагирует на события: если он пропустит событие об изменении, состояние останется неверным навсегда».

<details><summary>Ответ</summary>

Контроллеры работают «по уровню»: они на каждой сверке сравнивают фактическое и желаемое, поэтому пропущенное событие компенсируется следующей сверкой. В опыте оператор исправил ручной `scale` без события.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 11. 🔴 `kubectl patch --subresource=status` от оператора даёт `Forbidden: cannot get resource "widgets/status"`. Что исправить?

- A) Добавить роли глагол `get` на `widgets/status`
- B) Убрать `status` из CRD
- C) Дать роль cluster-admin
- D) Перезапустить API-сервер

<details><summary>Ответ</summary>

**A.** `kubectl patch` читает объект перед записью; точечные права лучше cluster-admin.
→ [Подвох 3](expert.ru.md#3-rbac-на-status-и-get)

</details>

## 12. 🔴 Открытый вопрос

Когда стоит написать оператор, а когда хватит Helm-чарта?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Оператор — если нужна постоянная сверка и логика эксплуатации (бэкапы, обновления, восстановление), а состояние описывается своим API.
- Helm — для разовой установки и параметризации без логики «дня 2».
- Сначала поискать готовый оператор; своего — лишь для предметной области.
- Цена оператора: код, версии CRD, RBAC, эксплуатация.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
