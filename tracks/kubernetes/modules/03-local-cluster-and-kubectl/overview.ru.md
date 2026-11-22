---
title: "Локальный кластер и kubectl"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 03-local-cluster-and-kubectl
part: 1
levels: [1, 2, 3, 4]
tags: [kind, kubectl, kubeconfig, context, namespace, manifest, declarative]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Локальный кластер и kubectl

🌐 **Русский** · [English](overview.en.md)

## Коротко

Чтобы учиться Kubernetes, не нужно арендовать облако: kind (Kubernetes in Docker) поднимает настоящий кластер из контейнеров Docker за секунды. Общаются с кластером через `kubectl` — клиент REST API: он читает адрес и учётные данные из файла kubeconfig, отправляет запросы и красиво показывает ответы. Модуль учит создавать кластер, переключаться между кластерами и пространствами имён и выполнять базовые команды.

## Ключевые идеи

- **kind запускает ноды как контейнеры Docker.** Команда `kind create cluster --name learn` создаёт одноузловой кластер; конфигурационный файл позволяет задать несколько нод.
- **kubeconfig хранит кластеры, пользователей и контексты.** Контекст — это тройка «кластер + пользователь + namespace по умолчанию»; `kubectl config use-context` переключает её.
- **Всегда знайте, в какой кластер идёт команда.** `kind create cluster` сам переключает текущий контекст на новый кластер.
- **Два стиля работы:** императивный (`kubectl create`, `run`) и декларативный (`kubectl apply -f манифест.yaml`). Декларативный повторяем: `apply` можно запускать много раз.
- **Выводом управляют флаги:** `-o wide`, `-o yaml`, `-o jsonpath=…`, `-o custom-columns=…`; описание любого поля — `kubectl explain`.
- **Образы из локального Docker кластеру не видны.** Их загружают командой `kind load docker-image` и используют явный тег, а не `latest`.
- **Эксперименты изолируйте namespace-ами** (`learn-NN`) и удаляйте их целиком: `kubectl delete namespace learn-03`.

## Минимальный пример

```bash
kind create cluster --name learn
kubectl get nodes
kubectl apply -f pod.yaml
kubectl get pods
kubectl logs hello
kubectl delete -f pod.yaml
```

```text
NAME                  STATUS   ROLES           AGE   VERSION
learn-control-plane   Ready    control-plane   22s   v1.37.0
```

Манифест `pod.yaml` и вывод остальных команд — в [базовой теории](basics.ru.md).

## Когда применять, а когда нет

- ✅ kind — для обучения, проверки манифестов и тестов в CI: кластер одноразовый.
- ✅ `kubectl apply -f` — для всего, что должно быть воспроизводимым и храниться в Git.
- ✅ `kubectl create`, `run`, `--dry-run=client -o yaml` — чтобы быстро сгенерировать заготовку манифеста.
- ❌ kind не подходит для продакшена и для проверки нагрузки: нет реального HA, ноды — контейнеры на одной машине.
- ❌ Не правьте боевой кластер императивными командами без записи в Git: изменения не воспроизвести.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | установка kind и kubectl, создание кластера, kubeconfig и контексты, namespace, манифест пода, get/describe/logs/port-forward/delete |
| [2. Продвинутая теория](advanced.ru.md) | `create` и `apply`, `diff`, `--dry-run`, `explain`, форматы вывода, селекторы, несколько кластеров и `KUBECONFIG`, многоузловой kind, загрузка образов |
| [3. Экспертная теория](expert.ru.md) | устройство kubeconfig, как kubectl общается с API, плагины, справочник команд и форматов вывода, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 03-local-cluster-and-kubectl level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 03-local-cluster-and-kubectl level 6` |

## Связанные материалы

- Пререквизиты: [01 · Контейнеры и Docker](../01-containers/overview.ru.md), [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)
- Дальше: [04 · Pod](../04-pods/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — shell и работа с WSL2
