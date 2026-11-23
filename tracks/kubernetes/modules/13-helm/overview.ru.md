---
title: "Helm"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 13-helm
part: 2
levels: [1, 2, 3, 4]
tags: [helm, chart, values, release, templates, hooks, package-manager]
versions: "Helm 4.3, Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Helm

🌐 **Русский** · [English](overview.en.md)

## Коротко

Helm — менеджер пакетов для Kubernetes. **Чарт (chart)** — набор шаблонов манифестов и значений по умолчанию; **релиз (release)** — установленный экземпляр чарта в кластере с историей ревизий. Вместо десятка YAML-файлов с копипастой между окружениями вы храните один чарт и передаёте разные `values`. Helm умеет обновлять релиз, откатывать его на прошлую ревизию, запускать хуки и тесты.

## Ключевые идеи

- **Чарт + values = манифесты.** Шаблоны на Go template получают объект `.Values`, `.Release`, `.Chart` и порождают обычные манифесты (`helm template`).
- **Релиз — это объект с историей.** Каждая установка и обновление создаёт ревизию; состояние хранится в Secret `sh.helm.release.v1.<имя>.v<N>` в namespace релиза.
- **`helm upgrade` без `--reuse-values` сбрасывает ранее заданные `--set`:** значения берутся из чарта и текущей команды.
- **`--wait` и `--rollback-on-failure` (`--atomic`) защищают обновления:** без них неудачный апгрейд остаётся ревизией `failed`.
- **Хуки (`pre-install`, `pre-upgrade`, `test`, …)** запускают Job и Pod в нужный момент жизни релиза.
- **`values.schema.json` и `required` проверяют вход** до обращения к кластеру.
- **Helm 4 применяет объекты через server-side apply:** ручные изменения тех же полей дают конфликт (`--force-conflicts`).

## Минимальный пример

```text
$ helm install web ./mini -n learn-13 --wait
NAME: web
STATUS: deployed
REVISION: 1
$ helm upgrade web ./mini -n learn-13 --set replicaCount=3
$ helm history web -n learn-13
1  superseded  Install complete
2  deployed    Upgrade complete
$ helm rollback web 1 -n learn-13
Rollback was a success! Happy Helming!
```

## Когда применять, а когда нет

| Ситуация | Решение |
|---|---|
| Установить готовое ПО (БД, ingress-контроллер) | Helm-чарт из реестра |
| Свой сервис в нескольких окружениях с разными параметрами | свой чарт или [Kustomize](../14-kustomize/overview.ru.md) |
| Нужны откаты, хуки, тесты релиза | Helm |
| Простые оверлеи без шаблонизации | Kustomize |

- ❌ Не превращайте шаблоны в программу с десятками условий: чарт становится нечитаемым.
- ❌ Не храните секреты в `values.yaml` в Git открытым текстом.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | структура чарта, шаблоны, values, install/upgrade/rollback/uninstall |
| [2. Продвинутая теория](advanced.ru.md) | приоритет values, `--reuse-values`, `--wait` и `--atomic`, схема и `required`, хуки, зависимости, упаковка |
| [3. Экспертная теория](expert.ru.md) | справочник команд и объектов шаблона, хранение релиза, server-side apply, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 13-helm level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 13-helm level 6` |

## Связанные материалы

- Пререквизиты: [05 · Deployment](../05-deployments/overview.ru.md), [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Дальше: [14 · Kustomize](../14-kustomize/overview.ru.md)
- Хуки и Job: [12 · Другие workload-ы](../12-workloads/overview.ru.md)
- Выкатка и откат: [05 · Deployment](../05-deployments/advanced.ru.md#история-ревизий-и-откат)
- Логи, события и метрики: [16 · Наблюдаемость](../16-observability/overview.ru.md)
- CRD и операторы: [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md)
- GitOps и Argo CD: [20 · GitOps](../20-gitops/overview.ru.md)
