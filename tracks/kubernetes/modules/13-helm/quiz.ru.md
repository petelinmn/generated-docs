---
title: "Квиз: Helm"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 13-helm
part: 2
tags: [helm, chart, values, release]
status: draft
updated: 2026-11-23
---

# Квиз: Helm

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что такое релиз Helm?

- A) Версия приложения в Git
- B) Установленный экземпляр чарта в кластере с историей ревизий
- C) Архив `.tgz`
- D) Файл `values.yaml`

<details><summary>Ответ</summary>

**B.** Архив — это упакованный чарт, `values.yaml` — значения по умолчанию.
→ [Базовая теория](basics.ru.md#жизненный-цикл-релиза)

</details>

## 2. 🟢 Как посмотреть итоговые манифесты, не обращаясь к кластеру?

- A) `helm get manifest`
- B) `helm template`
- C) `helm status`
- D) `helm test`

<details><summary>Ответ</summary>

**B.** `get manifest` показывает уже установленное, `status` — состояние релиза, `test` запускает тестовые хуки.
→ [Базовая теория](basics.ru.md#посмотреть-результат-без-установки)

</details>

## 3. 🟢 Откат `helm rollback web 1` при двух ревизиях. Что будет в истории?

- A) Ревизия 2 удалится
- B) Появится ревизия 3 «Rollback to 1»
- C) Ревизия 1 станет текущей без изменений истории
- D) Откат невозможен

<details><summary>Ответ</summary>

**B.** В опыте история: `1 superseded`, `2 superseded`, `3 deployed (Rollback to 1)`.
→ [Базовая теория](basics.ru.md#жизненный-цикл-релиза)

</details>

## 4. 🟢 Где задаётся образ по умолчанию, если `image.tag` пуст?

- A) В `Chart.yaml` в `appVersion` (если так написан шаблон)
- B) Всегда `latest`
- C) В `Release.Name`
- D) Helm сам выбирает

<details><summary>Ответ</summary>

**A.** Шаблон использует `.Values.image.tag | default .Chart.AppVersion`; в опыте получился `nginx:1.29`.
→ [Базовая теория](basics.ru.md#шаблоны)

</details>

## 5. 🟡 Порядок: `-f v.yaml` задаёт `replicaCount: 5`, а `--set replicaCount=7`. Что получится?

- A) 5
- B) 7
- C) 12
- D) Ошибка

<details><summary>Ответ</summary>

**B.** `--set` имеет наивысший приоритет.
→ [Продвинутая теория](advanced.ru.md#приоритет-values)

</details>

## 6. 🟡 Вы сделали `helm upgrade web mini --set replicaCount=4`, затем `helm upgrade web mini --set image.tag=1.28`. Сколько реплик?

- A) 4
- B) Значение по умолчанию из `values.yaml`
- C) 1
- D) Ошибка

<details><summary>Ответ</summary>

**B.** Без `--reuse-values` прошлые `--set` не наследуются (`replicaCount=4` пропал в `helm get values`).
→ [Продвинутая теория](advanced.ru.md#--reuse-values-и-сброс-при-апгрейде)

</details>

## 7. 🟡 Апгрейд с `--wait` и несуществующим образом завершился по таймауту. Что в истории?

- A) Ревизия `deployed`
- B) Ревизия `failed`, автоматического отката нет
- C) Откат на предыдущую ревизию
- D) История не изменилась

<details><summary>Ответ</summary>

**B.** Откат даёт `--rollback-on-failure` (`--atomic`); тогда появляется ревизия «Rollback to N».
→ [Продвинутая теория](advanced.ru.md#неудачное-обновление---wait-и---rollback-on-failure)

</details>

## 8. 🟡 Выберите все верные утверждения о хуках

- [ ] A) `pre-install` Job выполняется до создания Deployment
- [ ] B) С `hook-succeeded` Job удаляется после успеха
- [ ] C) Хук-ресурс входит в обычный манифест релиза
- [ ] D) Тестовый хук запускается `helm test`

<details><summary>Ответ</summary>

**A, B, D.** C неверно: хуки управляются отдельно и видны в `helm get hooks`.
→ [Продвинутая теория](advanced.ru.md#хуки)

</details>

## 9. 🟡 Как отключить подчарт `mini` через значения?

- A) Удалить `charts/mini`
- B) `--set mini.enabled=false` при `condition: mini.enabled`
- C) Нельзя
- D) `helm uninstall mini`

<details><summary>Ответ</summary>

**B.** В опыте при `mini.enabled=false` манифестов нет.
→ [Продвинутая теория](advanced.ru.md#зависимости-subchart)

</details>

## 10. 🔴 Найдите ошибку

> «Секреты в `values.yaml` безопасны, ведь Helm хранит релиз в закрытом формате».

<details><summary>Ответ</summary>

Релиз хранится в Secret как gzip и два слоя base64 — это кодирование, а не шифрование: любой, кто читает Secret, достаёт значения. К тому же `values.yaml` в Git виден всем.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 11. 🔴 `helm upgrade` падает: `conflict with "kubectl.exe" with subresource "scale"`. Причина и решение?

- A) Ошибка сети
- B) Поле `.spec.replicas` захвачено `kubectl scale`; применять через `--force-conflicts` или менять `replicaCount`
- C) Нет прав RBAC
- D) Устаревший чарт

<details><summary>Ответ</summary>

**B.** Helm 4 использует server-side apply; конфликт возникает с менеджером полей.
→ [Продвинутая теория](advanced.ru.md#server-side-apply-в-helm-4)

</details>

## 12. 🔴 Открытый вопрос

Когда вы выберете Helm, а когда Kustomize?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Helm: готовые сторонние чарты, параметризация, история релизов и откаты, хуки и тесты; цена — шаблоны на Go template и Secret с релизом.
- Kustomize: чистый YAML и оверлеи окружений, встроен в kubectl; нет истории и пакетов.
- Часто сочетают: Helm для чужих компонентов, Kustomize или GitOps для своих.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
