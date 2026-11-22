---
title: "ConfigMap и Secret"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 07-config-and-secrets
part: 1
levels: [1, 2, 3, 4]
tags: [configmap, secret, environment-variables, volume, base64, immutable, configuration, encryption-at-rest]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# ConfigMap и Secret

🌐 **Русский** · [English](overview.en.md)

## Коротко

Конфигурация не должна жить внутри образа: один и тот же образ запускают в разных окружениях с разными настройками. В Kubernetes для этого есть два объекта: **ConfigMap** хранит обычные настройки, **Secret** — чувствительные данные (пароли, токены, сертификаты). Оба подключаются к поду как переменные окружения или как файлы. Важно: Secret по умолчанию только закодирован в base64, но не зашифрован.

## Ключевые идеи

- **ConfigMap и Secret — пары «ключ → значение»,** созданные отдельно от пода; один объект можно подключить ко многим подам.
- **Два способа подключения:** переменные окружения (`env`, `envFrom`) и тома (файлы). Переменные фиксируются при старте контейнера, файлы в томе обновляются без перезапуска.
- **base64 — не шифрование.** Значение Secret декодируется одной командой, а в etcd по умолчанию лежит открытым текстом: защита строится на RBAC и шифровании etcd.
- **Изменение ConfigMap не перезапускает поды:** новые значения подхватят только поды, пересозданные после изменения (или том, но не `subPath`).
- **Отсутствующий ConfigMap или ключ блокирует запуск:** статус `CreateContainerConfigError`, если ссылка не помечена `optional: true`.
- **`immutable: true`** запрещает менять объект: защита от случайных правок и разгрузка API server.
- **Ограничение размера — 1 МиБ** на объект; имена ключей — `[-._a-zA-Z0-9]+`.

## Минимальный пример

```bash
kubectl -n learn-07 create configmap app-config --from-literal=MODE=prod --from-literal=TIMEOUT=30
kubectl -n learn-07 create secret generic db --from-literal=username=admin --from-literal=password='S3cr3t!'
```

```yaml
env:
  - name: APP_MODE
    valueFrom: {configMapKeyRef: {name: app-config, key: MODE}}
  - name: DB_PASSWORD
    valueFrom: {secretKeyRef: {name: db, key: password}}
```

```text
$ kubectl -n learn-07 exec app -- env | grep -E "^(APP_MODE|DB_PASSWORD)="
APP_MODE=prod
DB_PASSWORD=S3cr3t!
```

## Когда применять, а когда нет

- ✅ Любые настройки окружения: адреса сервисов, уровень логирования, флаги, конфигурационные файлы.
- ✅ Secret — для паролей, токенов и ключей (с RBAC и, желательно, шифрованием etcd).
- ❌ Не храните секреты в образе, в Git в открытом виде или в ConfigMap.
- ❌ Не рассчитывайте, что Secret «защищён» сам по себе: base64 легко декодируется.
- ❌ Для больших файлов (больше 1 МиБ) нужны тома и хранилища (модуль 08), а не ConfigMap.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | создание ConfigMap и Secret, base64, подключение через `env`, `envFrom` и тома |
| [2. Продвинутая теория](advanced.ru.md) | обновление значений, `subPath`, `immutable`, отсутствующие ссылки, права файлов, типы Secret, перезапуск при изменении |
| [3. Экспертная теория](expert.ru.md) | хранение Secret в etcd, шифрование, RBAC, справочник типов и полей, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 07-config-and-secrets level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 07-config-and-secrets level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- Дальше: [08 · Хранилище](../08-storage/overview.ru.md)
- В других треках: [DI, конфигурация и логирование в C#](../../../csharp/modules/13-di-config-logging/overview.ru.md) — как приложение читает переменные окружения и файлы конфигурации
