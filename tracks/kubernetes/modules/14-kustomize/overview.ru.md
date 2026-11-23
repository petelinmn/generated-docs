---
title: "Kustomize"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 14-kustomize
part: 2
levels: [1, 2, 3, 4]
tags: [kustomize, overlay, patch, configmap-generator, base, environments]
versions: "Kustomize 5.8.1 (в составе kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize

🌐 **Русский** · [English](overview.en.md)

## Коротко

Kustomize настраивает обычные манифесты без шаблонов: есть **base** (общий YAML) и **overlays** (окружения), которые меняют base с помощью патчей, префиксов имён, меток, образов и генераторов ConfigMap. Он встроен в kubectl: `kubectl kustomize <папка>` печатает результат, `kubectl apply -k <папка>` применяет его. В отличие от [Helm](../13-helm/overview.ru.md) здесь нет релизов и истории, зато исходные файлы остаются валидным YAML.

## Ключевые идеи

- **base + overlays.** Общий набор манифестов лежит в `base/`, различия окружений — в `overlays/dev`, `overlays/prod`.
- **Файл `kustomization.yaml`** перечисляет `resources` и преобразования; сборка не меняет исходные файлы.
- **Трансформеры:** `namespace`, `namePrefix`, `labels`, `images`, `replicas`, `patches`, `replacements`.
- **Генераторы ConfigMap/Secret добавляют суффикс-хеш** к имени и подменяют ссылки на него: изменение конфигурации вызывает rolling update.
- **Патчи двух видов:** strategic merge (кусок манифеста) и JSON 6902 (список операций); цель выбирается по `target`.
- **Компоненты (`Component`)** — переиспользуемые куски, подключаемые в разные overlay.
- **`kubectl apply -k` не удаляет прежние ConfigMap:** старые версии с другим хешем остаются.

## Минимальный пример

```text
$ cat overlays/prod/kustomization.yaml
resources: [../../base]
namespace: learn-14-prod
namePrefix: prod-
replicas: [{name: web, count: 3}]
images: [{name: nginx, newTag: "1.29"}]
$ kubectl kustomize overlays/prod        # посмотреть результат
$ kubectl apply -k overlays/prod         # применить
configmap/prod-app-config-dfk4bdbtkk created
service/prod-web created
deployment.apps/prod-web created
```

## Когда применять, а когда нет

| Ситуация | Решение |
|---|---|
| Свой сервис в dev/stage/prod с небольшими различиями | Kustomize |
| Нужны релизы, откаты, хуки, готовые сторонние чарты | [Helm](../13-helm/overview.ru.md) |
| Правка чужого Helm-чарта без форка | Helm + Kustomize |

- ❌ Не превращайте overlays в копии base: меняйте только различия.
- ❌ Не рассчитывайте на историю и откат: их нет, их даёт Git или GitOps.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | base и overlay, `kustomization.yaml`, namespace, префикс, метки, образы, реплики, `apply -k` |
| [2. Продвинутая теория](advanced.ru.md) | патчи, генераторы с хешем, replacements, компоненты, диагностика ошибок |
| [3. Экспертная теория](expert.ru.md) | справочник полей, порядок вывода, ограничения загрузки, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 14-kustomize level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 14-kustomize level 6` |

## Связанные материалы

- Пререквизиты: [05 · Deployment](../05-deployments/overview.ru.md), [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Предыдущий модуль: [13 · Helm](../13-helm/overview.ru.md)
- Дальше: [15 · Автомасштабирование](../15-autoscaling/overview.ru.md)
- Выкатка при смене конфигурации: [07 · ConfigMap и Secret](../07-config-and-secrets/advanced.ru.md)
- GitOps и Argo CD: [20 · GitOps](../20-gitops/overview.ru.md)
