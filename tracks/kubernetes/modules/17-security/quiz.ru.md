---
title: "Квиз: безопасность подов и сети"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 17-security
part: 3
tags: [networkpolicy, pod-security-admission, securitycontext]
status: draft
updated: 2026-11-23
---

# Квиз: безопасность подов и сети

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 В namespace нет ни одной NetworkPolicy. Что можно сказать о сети?

- A) Весь входящий трафик закрыт
- B) Весь трафик разрешён
- C) Разрешён только трафик внутри namespace
- D) Разрешён только DNS

<details><summary>Ответ</summary>

**B.** В опыте все три клиента достучались до сервера. Сеть закрывают политиками.
→ [Базовая теория](basics.ru.md#networkpolicy-ограничиваем-сеть)

</details>

## 2. 🟢 Как сделать default deny для входящего трафика в namespace?

- A) `podSelector: {}` и `policyTypes: [Ingress]` без правил `ingress`
- B) `podSelector: {}` с `ingress: [{}]`
- C) Удалить Service
- D) Поставить метку на namespace

<details><summary>Ответ</summary>

**A.** Пустой селектор выбирает все поды, а отсутствие правил означает «ничего не разрешено». Вариант B, наоборот, разрешает всё.
→ [Базовая теория](basics.ru.md#default-deny)

</details>

## 3. 🟢 Что означает `runAsNonRoot: true` для образа, который запускается от root, если `runAsUser` не задан?

- A) Kubernetes сам сменит пользователя
- B) Контейнер не запустится: `CreateContainerConfigError`
- C) Ничего, поле необязательное
- D) Запуск пройдёт с предупреждением

<details><summary>Ответ</summary>

**B.** В опыте: `container has runAsNonRoot and image will run as root`.
→ [Базовая теория](basics.ru.md#securitycontext-права-процесса)

</details>

## 4. 🟢 `readOnlyRootFilesystem: true`, а приложению нужен `/tmp`. Что делать?

- A) Отключить поле
- B) Смонтировать `emptyDir` на `/tmp`
- C) Запустить под от root
- D) Ничего: `/tmp` исключён

<details><summary>Ответ</summary>

**B.** Запись остаётся возможной только в смонтированных томах (в опыте `/x` — ошибка, `/tmp` на `emptyDir` — запись удалась).
→ [Базовая теория](basics.ru.md#securitycontext-права-процесса)

</details>

## 5. 🟡 `from` содержит один элемент с `namespaceSelector` и `podSelector` вместе. Что это значит?

- A) Подходит любой под из выбранных namespace «или» любой под с выбранными метками
- B) Подходит под, который удовлетворяет обоим селекторам
- C) Ошибка валидации
- D) Селекторы игнорируются

<details><summary>Ответ</summary>

**B.** Один элемент — «И». Два отдельных элемента — «ИЛИ». В опыте `client-ok` (не из нужного namespace) в первом варианте заблокирован, во втором — нет.
→ [Продвинутая теория](advanced.ru.md#networkpolicy-from-namespaceselector-и-и-против-или)

</details>

## 6. 🟡 Egress-политика без правил выбрала под. Что случится при `wget http://server.learn-17.svc`?

- A) Запрос пройдёт
- B) Не выполнится даже резолв имени (DNS заблокирован)
- C) Заблокирован только порт 80
- D) Блокировка действует только на внешние адреса

<details><summary>Ответ</summary>

**B.** В опыте `nslookup` дал `connection timed out; no servers could be reached`. Нужно явно разрешить DNS.
→ [Продвинутая теория](advanced.ru.md#egress-и-dns)

</details>

## 7. 🟡 В namespace стоит `enforce=restricted`, и вы запускаете `rollout restart` Deployment с «нарушающим» шаблоном. Что произойдёт?

- A) Команда вернёт ошибку
- B) Появится новый ReplicaSet без подов, события покажут `FailedCreate`; старые поды останутся
- C) Все поды перезапустятся и пройдут проверку
- D) Namespace перестанет принимать любые объекты

<details><summary>Ответ</summary>

**B.** В опыте: `dep-576fd7f57b 1 0 0`, а в событиях `violates PodSecurity "restricted:latest"`.
→ [Продвинутая теория](advanced.ru.md#pod-security-admission-режимы-на-практике)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) При смене метки `enforce` существующие поды удаляются
- [ ] B) `kubectl label --dry-run=server` показывает, какие существующие поды нарушат уровень
- [ ] C) `warn` показывает предупреждение клиенту
- [ ] D) `baseline` отклоняет `privileged: true`

<details><summary>Ответ</summary>

**B, C, D.** A неверно: существующие поды остаются, появляется только предупреждение.
→ [Продвинутая теория](advanced.ru.md#pod-security-admission-режимы-на-практике)

</details>

## 9. 🟡 Что делает `capabilities: {drop: [NET_RAW]}`?

- A) Ничего не меняет
- B) Убирает право на сырые сокеты; `ping` перестаёт работать
- C) Закрывает порт 80
- D) Запрещает запись в файловую систему

<details><summary>Ответ</summary>

**B.** В опыте: `ping: permission denied (are you root?)`.
→ [Продвинутая теория](advanced.ru.md#capabilities-и-привилегии)

</details>

## 10. 🔴 Найдите ошибку

> «Мы включили `pod-security.kubernetes.io/enforce=restricted`, значит, все работающие в namespace поды теперь проверены и безопасны».

<details><summary>Ответ</summary>

PSA проверяет создание и обновление подов, а работающие поды не трогает: при смене метки показывается только предупреждение. Нарушители будут отклонены при следующем создании, когда, например, поды пересоздаются.
→ [Подвох 3](expert.ru.md#3-enforce-не-трогает-работающие-поды)

</details>

## 11. 🔴 Две NetworkPolicy выбирают один под: первая разрешает ingress от `role=frontend`, вторая — от `role=admin`. Кто пройдёт?

- A) Только `frontend`
- B) Только `admin`
- C) И `frontend`, и `admin`
- D) Никто: политики конфликтуют

<details><summary>Ответ</summary>

**C.** Политики только разрешают и складываются, конфликтов нет.
→ [Подвох 7](expert.ru.md#7-политики-только-разрешают)

</details>

## 12. 🔴 Открытый вопрос

Как бы вы защитили namespace с приложением: что включить на уровне сети, подов и допуска?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Сеть: `default-deny` для ingress (и при необходимости egress с DNS), затем точечные разрешения.
- Поды: non-root, `allowPrivilegeEscalation: false`, `drop: [ALL]`, seccomp `RuntimeDefault`, read-only rootfs.
- Допуск: PSA через `warn`/`audit`, затем `enforce: restricted`; ValidatingAdmissionPolicy для своих правил.
- Доступ к API: отдельный ServiceAccount с минимальными правами, отключённый токен там, где он не нужен.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
