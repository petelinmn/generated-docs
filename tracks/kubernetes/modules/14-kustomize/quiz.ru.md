---
title: "Квиз: Kustomize"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 14-kustomize
part: 2
tags: [kustomize, overlay, patch, configmap-generator]
status: draft
updated: 2026-11-23
---

# Квиз: Kustomize

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой командой посмотреть результат сборки, не применяя его?

- A) `kubectl apply -k <dir>`
- B) `kubectl kustomize <dir>`
- C) `kubectl get -k <dir>`
- D) `kubectl delete -k <dir>`

<details><summary>Ответ</summary>

**B.** `apply -k` применяет, `get -k` и `delete -k` работают с объектами из результата.
→ [Базовая теория](basics.ru.md#применение)

</details>

## 2. 🟢 Что делает `namePrefix: prod-` в overlay?

- A) Добавляет префикс к именам объектов и обновляет ссылки на них
- B) Добавляет префикс только к Deployment
- C) Меняет namespace
- D) Создаёт новый namespace `prod-`

<details><summary>Ответ</summary>

**A.** В опыте `web` стал `prod-web` во всех объектах.
→ [Базовая теория](basics.ru.md#overlay)

</details>

## 3. 🟢 `kubectl apply -k` в namespace, которого нет. Что будет?

- A) Namespace создастся
- B) Ошибка `namespaces "…" not found`
- C) Объекты уйдут в `default`
- D) Применится только Service

<details><summary>Ответ</summary>

**B.** Kustomize только подставляет namespace в объекты.
→ [Базовая теория](basics.ru.md#применение)

</details>

## 4. 🟢 Как в overlay сменить тег образа `nginx` на `1.29`?

- A) `images: [{name: nginx, newTag: "1.29"}]`
- B) Скопировать Deployment и править вручную
- C) `replicas: [{name: nginx, count: 1}]`
- D) Нельзя

<details><summary>Ответ</summary>

**A.** В опыте результат `image: nginx:1.29`.
→ [Базовая теория](basics.ru.md#основные-трансформеры)

</details>

## 5. 🟡 Патч написан на `Deployment/ghost`, которого нет. Что будет?

- A) Патч молча пропустится
- B) Ошибка `no resource matches strategic merge patch`
- C) Объект `ghost` будет создан
- D) Применится к первому Deployment

<details><summary>Ответ</summary>

**B.** Патч на несуществующий объект не игнорируется.
→ [Продвинутая теория](advanced.ru.md#ошибки-патчей)

</details>

## 6. 🟡 Вы изменили `LOG_LEVEL` в `configMapGenerator` и сделали `apply -k`. Что произойдёт с подами?

- A) Ничего, ConfigMap обновится на месте
- B) Создастся новый ConfigMap с новым хешем, Deployment получит новое имя и перезапустит поды
- C) Применение завершится ошибкой
- D) Старый ConfigMap удалится

<details><summary>Ответ</summary>

**B.** В опыте имя сменилось с `…dfk4bdbtkk` на `…47668c6k28`, прошёл rolling update. Старый ConfigMap остался.
→ [Продвинутая теория](advanced.ru.md#генераторы-configmap-и-secret)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) JSON 6902 патч — список операций `op/path/value`
- [ ] B) Strategic merge патч ищет контейнер по полю `name`
- [ ] C) `labels` с `includeSelectors: true` добавляет метку и в селекторы
- [ ] D) `commonLabels` — рекомендуемый способ добавить метки

<details><summary>Ответ</summary>

**A, B, C.** D неверно: `commonLabels` устарел (`Please use 'labels' instead`) и меняет селекторы.
→ [Продвинутая теория](advanced.ru.md#патчи)

</details>

## 8. 🟡 Что произойдёт при `disableNameSuffixHash: true`, если поменять данные ConfigMap?

- A) Поды перезапустятся
- B) Имя не изменится, поды не перезапустятся
- C) Ошибка
- D) ConfigMap будет удалён

<details><summary>Ответ</summary>

**B.** Шаблон пода не изменился; нужен ручной рестарт.
→ [Продвинутая теория](advanced.ru.md#генераторы-configmap-и-secret)

</details>

## 9. 🔴 Найдите ошибку

> «Мы добавили `commonLabels: {team: a}` к работающему Deployment, и всё применилось без проблем».

<details><summary>Ответ</summary>

`commonLabels` меняет и `selector`; у Deployment селектор неизменяем, поэтому для уже существующего объекта apply будет отвергнут (в опыте сборка добавила метку в пять мест, включая селекторы). Используйте `labels` без `includeSelectors`.
→ [Подвох 1](expert.ru.md#1-commonlabels-меняет-селекторы)

</details>

## 10. 🔴 Что выведет сборка?

```yaml
resources: [deployment.yaml, service.yaml, deployment.yaml]
```

- A) Два Deployment
- B) Ошибка `may not add resource with an already registered id`
- C) Один Deployment, дубликат отброшен
- D) Предупреждение и успешная сборка

<details><summary>Ответ</summary>

**B.** Идентификатор ресурса уникален в сборке.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 11. 🔴 Открытый вопрос

Сравните Kustomize и Helm и скажите, как вы выберете между ними.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Kustomize: чистый YAML, base/overlay и патчи, встроен в kubectl; нет релизов, истории и пакетов.
- Helm: шаблоны и values, релизы с историей и откатами, хуки, готовые чарты; цена — сложность шаблонов.
- Свои сервисы по окружениям — Kustomize; сторонние компоненты и откаты — Helm; можно сочетать (`--enable-helm`, не проверялось).

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
