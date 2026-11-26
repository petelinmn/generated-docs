---
title: "Безопасность подов и сети"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 17-security
part: 3
levels: [1, 2, 3, 4]
tags: [networkpolicy, pod-security-admission, pod-security-standards, securitycontext, capabilities, serviceaccount, validatingadmissionpolicy, seccomp]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Безопасность подов и сети

🌐 **Русский** · [English](overview.en.md)

## Коротко

По умолчанию в Kubernetes любой под может обратиться к любому другому, контейнер работает от root с набором привилегий, а в под монтируется токен ServiceAccount. Защита строится из нескольких независимых слоёв: **NetworkPolicy** ограничивает сетевые соединения, **securityContext** задаёт, с какими правами работает процесс, **Pod Security Admission (PSA)** не пускает в namespace небезопасные поды, **ValidatingAdmissionPolicy** добавляет собственные правила на языке CEL. Права доступа к API — отдельная тема: [RBAC](../10-namespaces-and-rbac/overview.ru.md).

## Ключевые идеи

- **Сеть по умолчанию открыта.** Пока ни одна NetworkPolicy не выбирает под, он принимает и отправляет всё (в опыте все клиенты получили ответ).
- **Первая политика закрывает остальное.** Как только под выбран политикой направления `Ingress`, разрешено только перечисленное в политиках; `default-deny` — политика с пустым правилом.
- **NetworkPolicy выполняет сетевой плагин (CNI),** а не API-сервер. В kind (kindnet) политики работают.
- **`securityContext` сужает права процесса:** непривилегированный пользователь, без лишних capabilities, корневая файловая система только для чтения.
- **PSA проверяет поды при создании** по меткам namespace: уровни `privileged`, `baseline`, `restricted` и режимы `enforce`, `warn`, `audit`.
- **PSA не трогает уже работающие поды** и блокирует не Deployment, а создаваемые им поды.
- **Токен ServiceAccount монтируется по умолчанию,** его можно отключить (`automountServiceAccountToken: false`).
- **Свои правила — ValidatingAdmissionPolicy:** выражение CEL, проверяемое самим API-сервером.

## Минимальный пример

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: default-deny-ingress, namespace: learn-17}
spec:
  podSelector: {}
  policyTypes: [Ingress]
```

```text
$ kubectl -n learn-17 exec client-ok -- wget -q -T 3 -O /dev/null http://server.learn-17.svc
wget: download timed out
```

До применения политики тот же запрос возвращал ответ.

## Когда применять, а когда нет

| Задача | Инструмент |
|---|---|
| Ограничить, кто с кем общается | NetworkPolicy |
| Запретить root, привилегированные контейнеры | securityContext + PSA |
| Не пустить небезопасные поды в namespace | PSA (`enforce`) |
| Собственное правило (например, запрет тега `latest`) | ValidatingAdmissionPolicy |
| Ограничить действия в API | RBAC ([модуль 10](../10-namespaces-and-rbac/overview.ru.md)) |

- ❌ Не считайте NetworkPolicy брандмауэром на уровне приложения: она работает на уровне IP, портов и меток.
- ❌ Не включайте `enforce: restricted` в namespace с работающими приложениями без предварительной проверки: новые поды без нужных полей не создадутся.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | NetworkPolicy: default deny и разрешение по меткам; securityContext; метки PSA |
| [2. Продвинутая теория](advanced.ru.md) | namespaceSelector (AND и OR), egress и DNS, режимы PSA, capabilities, токен ServiceAccount, ValidatingAdmissionPolicy |
| [3. Экспертная теория](expert.ru.md) | справочник полей, уровни PSS, устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 17-security level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 17-security level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [06 · Service и сеть](../06-services/overview.ru.md), [10 · Namespaces и RBAC](../10-namespaces-and-rbac/overview.ru.md)
- Секреты: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)
- Предыдущий модуль: [16 · Наблюдаемость](../16-observability/overview.ru.md)
- Дальше: [18 · Планирование подов](../18-scheduling/overview.ru.md)
- Аналоги в других треках: [Linux](../../../linux/overview.ru.md) — права, пользователи, capabilities на уровне ОС
- CRD и операторы: [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md)
- GitOps и Argo CD: [20 · GitOps](../20-gitops/overview.ru.md)
- Диагностика: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
- [System design: безопасность архитектуры](../../../system-design/modules/15-security-architecture/overview.ru.md)
