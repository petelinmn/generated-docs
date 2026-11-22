---
title: "Квиз: локальный кластер и kubectl"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, apply, diff]
status: draft
updated: 2026-11-22
---

# Квиз: локальный кластер и kubectl

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что такое нода в кластере kind?

- A) Виртуальная машина VirtualBox
- B) Контейнер Docker, внутри которого работают kubelet и containerd
- C) Облачный сервер
- D) Процесс на хосте без изоляции

<details><summary>Ответ</summary>

**B.** kind запускает каждую ноду как контейнер Docker; `docker ps` покажет, например, `learn-control-plane`. Ни VM, ни облака для этого не нужно.
→ [Базовая теория](basics.ru.md#создать-кластер)

</details>

## 2. 🟢 Что хранит контекст kubeconfig?

- A) Только адрес API server
- B) Связку «кластер + пользователь + namespace по умолчанию»
- C) Список подов
- D) Образы контейнеров

<details><summary>Ответ</summary>

**B.** Адрес и сертификат CA лежат в `clusters`, учётные данные — в `users`, а контекст связывает их и задаёт namespace по умолчанию. Список подов и образы kubeconfig не содержит.
→ [Базовая теория](basics.ru.md#kubeconfig-и-контексты)

</details>

## 3. 🟢 Что произойдёт при `kubectl get pods`, если вы забыли `-n learn-03`?

- A) Команда покажет поды из всех namespace
- B) Команда выполнится в namespace контекста (по умолчанию `default`) и «не найдёт» ваши поды
- C) Ошибка: namespace не указан
- D) Команда создаст namespace

<details><summary>Ответ</summary>

**B.** Без `-n` используется namespace из контекста; если он не задан — `default`. Для всех сразу нужен флаг `-A`. Namespace по умолчанию можно записать в контекст: `kubectl config set-context kind-learn --namespace=learn-03`.
→ [Базовая теория](basics.ru.md#namespace)

</details>

## 4. 🟢 Под ссылается на образ `hello:multi`, который есть только в локальном Docker. Какой статус вы увидите?

- A) `Running`
- B) `ErrImagePull`
- C) `Completed`
- D) `Pending` без событий

<details><summary>Ответ</summary>

**B.** kubelet ноды kind идёт за образом в Docker Hub и получает `pull access denied, repository does not exist`. Образ нужно загрузить командой `kind load docker-image hello:multi --name learn`.
→ [Базовая теория](basics.ru.md#первый-манифест)

</details>

## 5. 🟡 Что выведут две команды подряд?

```bash
kubectl -n learn-03 create -f pod.yaml
kubectl -n learn-03 create -f pod.yaml
```

- A) `pod/hello created` и `pod/hello unchanged`
- B) `pod/hello created` и `Error from server (AlreadyExists): … pods "hello" already exists`
- C) Две строки `pod/hello created`
- D) Две ошибки

<details><summary>Ответ</summary>

**B.** `create` не идемпотентен. Вариант A описывает не `create`, а `apply` (второй запуск `apply` даёт `unchanged`).
→ [Продвинутая теория](advanced.ru.md#create-и-apply-императивно-и-декларативно)

</details>

## 6. 🟡 Что делает `kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml`?

- A) Создаёт Deployment
- B) Печатает манифест, не обращаясь к кластеру и ничего не создавая
- C) Проверяет Deployment на сервере и сохраняет его
- D) Удаляет Deployment `demo`

<details><summary>Ответ</summary>

**B.** Это способ получить заготовку манифеста. Режим `--dry-run=server` отличается тем, что запрос проверяет API server, но объект тоже не сохраняется.
→ [Продвинутая теория](advanced.ru.md#diff-и-dry-run)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) `kubectl apply` повторно запущенный с тем же файлом сообщит `unchanged`
- [ ] B) `kind create cluster` не меняет текущий контекст kubectl
- [ ] C) Селектор `-l 'tier in (frontend)'` выбирает объекты с меткой `tier=frontend`
- [ ] D) Образ, собранный в локальном Docker, автоматически доступен всем нодам kind

<details><summary>Ответ</summary>

**A, C.** B неверно: kind пишет `Set kubectl context to "kind-learn"` и переключает контекст. D неверно: образ нужно загрузить `kind load docker-image`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Как получить список контекстов из двух файлов kubeconfig на Windows?

- A) `KUBECONFIG="a.yaml:b.yaml"`
- B) `KUBECONFIG="a.yaml;b.yaml"`
- C) Указать оба файла через запятую
- D) Это невозможно

<details><summary>Ответ</summary>

**B.** На Windows разделитель — точка с запятой, на Linux и macOS — двоеточие. Запись с двоеточием на Windows даёт `error loading config file … The filename, directory name, or volume label syntax is incorrect`.
→ [Продвинутая теория](advanced.ru.md#несколько-кластеров-и-kubeconfig)

</details>

## 9. 🔴 Найди ошибку

> «Я создал многоузловой кластер командой `kind create cluster --wait 120s` и сразу запустил на нём Deployment на 3 реплики. Команда `--wait` же гарантирует, что кластер готов».

Что неверно в рассуждении и что нужно было сделать?

<details><summary>Ответ</summary>

`--wait` ждёт готовности control plane, а не рабочих нод: в опыте сразу после создания рабочие ноды были `NotReady` (через ~15 секунд `Ready`). Перед запуском нагрузки нужно дождаться нод: `kubectl wait --for=condition=Ready node --all --timeout=120s`.
→ [Экспертная теория](expert.ru.md#подвох-рабочие-ноды-notready-сразу-после-kind-create)

</details>

## 10. 🔴 Открытый вопрос

Объясни, чем отличаются `kubectl apply -f`, `kubectl diff -f` и `kubectl apply -f --dry-run=server` и как ты используешь их в безопасном процессе изменений.

<details><summary>Ключевые пункты ответа</summary>

- `apply` меняет кластер; идемпотентен, хранит последний применённый манифест в аннотации `last-applied-configuration`.
- `diff` показывает разницу между файлом и состоянием кластера, не меняя его; для неизменяемых полей дополнительно выдаёт ошибку валидации (в опыте — `The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than …`).
- `--dry-run=server` отправляет запрос на API server, который проверяет его (валидация, admission), но не сохраняет.
- Процесс: манифест в Git → `diff` и `dry-run=server` в CI или локально → `apply` → проверка (`rollout status`, `get events`).
- Перед выполнением — проверка `kubectl config current-context`.

</details>

→ [Продвинутая теория](advanced.ru.md#diff-и-dry-run)
