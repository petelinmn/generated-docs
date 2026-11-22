---
title: "Квиз: архитектура Kubernetes"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 02-architecture
part: 1
tags: [kubernetes, architecture, control-plane, etcd, scheduler, controller]
status: draft
updated: 2026-11-22
---

# Квиз: архитектура Kubernetes

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой компонент единственный обращается к etcd?

- A) kube-scheduler
- B) kubelet
- C) kube-apiserver
- D) kube-controller-manager

<details><summary>Ответ</summary>

**C.** Все остальные компоненты и клиенты работают через API server, он проверяет запрос и читает или пишет etcd. Прямой доступ к etcd обходит авторизацию и валидацию.
→ [Базовая теория](basics.ru.md#из-чего-состоит-кластер)

</details>

## 2. 🟢 Кто запускает контейнеры пода на ноде?

- A) kube-scheduler
- B) kubelet через контейнерный рантайм
- C) kube-proxy
- D) etcd

<details><summary>Ответ</summary>

**B.** Scheduler лишь записывает выбранную ноду в под, kube-proxy настраивает сетевые правила для Service, etcd хранит состояние. Контейнеры создаёт kubelet, обращаясь к рантайму (в нашем кластере — containerd) по интерфейсу CRI.
→ [Базовая теория](basics.ru.md#что-происходит-при-создании-deployment)

</details>

## 3. 🟢 Вы удалили один под из Deployment на 2 реплики. Что произойдёт?

- A) Ничего, останется один под
- B) ReplicaSet создаст новый под
- C) Deployment будет удалён
- D) kubelet перезапустит удалённый под

<details><summary>Ответ</summary>

**B.** ReplicaSet сравнивает желаемое число реплик (2) с фактическим (1) и создаёт недостающий под. В опыте новый под появился с возрастом 3 секунды. Kubelet удалённый под не восстанавливает (D неверно), Deployment не удаляется (C неверно).
→ [Базовая теория](basics.ru.md#самовосстановление)

</details>

## 4. 🟢 Где работают компоненты control plane в кластере kind?

- A) Как обычные процессы на вашем ноутбуке
- B) Как статические поды на ноде, из манифестов в `/etc/kubernetes/manifests`
- C) В облаке
- D) В отдельной виртуальной машине VirtualBox

<details><summary>Ответ</summary>

**B.** В kind нода — контейнер Docker; kubelet внутри неё запускает `etcd`, `kube-apiserver`, `kube-controller-manager` и `kube-scheduler` как статические поды из четырёх yaml-файлов этой папки.
→ [Базовая теория](basics.ru.md#что-видно-в-kind)

</details>

## 5. 🟡 Scheduler остановлен, вы увеличили `replicas` с 2 до 3. Что вы увидите?

- A) Три пода `Running`
- B) Два пода `Running` и один `Pending`, события `FailedScheduling` отсутствуют
- C) Ошибку при `kubectl scale`
- D) Два пода `Running` и один `CrashLoopBackOff`

<details><summary>Ответ</summary>

**B.** Контроллеры создают под, но ноду назначить некому. События `FailedScheduling` порождает сам scheduler, поэтому их нет. После возврата scheduler под стал `Running`. `kubectl scale` успешен: API server и etcd работают.
→ [Продвинутая теория](advanced.ru.md#что-бывает-когда-компонент-недоступен)

</details>

## 6. 🟡 Что выведет эта команда, если объект изменился после чтения?

```bash
kubectl -n learn-02 get deploy web -o json \
  | sed 's/"resourceVersion": "[0-9]*"/"resourceVersion": "1"/' \
  | kubectl replace -f -
```

- A) Объект заменён без ошибок
- B) `Error from server (Conflict): … the object has been modified; please apply your changes to the latest version and try again`
- C) `NotFound`
- D) `Forbidden`

<details><summary>Ответ</summary>

**B.** Версия `resourceVersion: "1"` устарела, и сервер отклоняет обновление: так работает оптимистичная блокировка. Клиент должен перечитать объект и повторить.
→ [Продвинутая теория](advanced.ru.md#оптимистичная-блокировка)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) Controller-manager остановлен: самовосстановление не работает, `READY` у Deployment может быть устаревшим
- [ ] B) Если API server остановлен, уже запущенные контейнеры немедленно останавливаются
- [ ] C) Цепочка владельцев пода: Deployment → ReplicaSet → Pod
- [ ] D) Запрос проходит этапы: аутентификация, авторизация, admission, валидация, запись в etcd

<details><summary>Ответ</summary>

**A, C, D.** A подтверждён опытом (`3/3` при двух подах). C показан через `ownerReferences`. D — путь запроса. B неверно: контейнеры работают под управлением kubelet и рантайма и продолжают работать; без API server нельзя вносить изменения.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что покажет команда `kubectl -n kube-system get lease`?

- A) Список сертификатов
- B) Объекты `Lease`, в том числе кто сейчас лидер у `kube-controller-manager` и `kube-scheduler`
- C) Список нод
- D) Журнал аудита

<details><summary>Ответ</summary>

**B.** Копии controller-manager и scheduler борются за `Lease`; `HOLDER` — текущий лидер. Строка `apiserver-…` — запись об экземпляре API server.
→ [Продвинутая теория](advanced.ru.md#leader-election)

</details>

## 9. 🔴 Найди ошибку в рассуждении

> «Я указал в поде `spec.nodeName: worker-1`, значит scheduler проверил, что на `worker-1` хватает ресурсов и нет конфликтов с taints».

<details><summary>Ответ</summary>

Рассуждение неверно: при заданном `nodeName` под минует scheduler. В опыте у такого пода нет события `Scheduled` от `default-scheduler`, только события kubelet. Проверки ресурсов, taints и affinity не выполняются, и kubelet запускает под сразу. Для выбора ноды используйте `nodeSelector`, affinity и tolerations (модуль 18 «Планирование подов»).
→ [Экспертная теория](expert.ru.md#подвох-nodename-в-обход-scheduler)

</details>

## 10. 🔴 Открытый вопрос

Объект ConfigMap удаляют командой `kubectl delete`, команда отвечает «deleted», но `kubectl get` всё ещё показывает объект с непустым `deletionTimestamp`. Объясни, что происходит и как действовать.

<details><summary>Ключевые пункты ответа</summary>

- `delete` выставил `deletionTimestamp`, но в `metadata.finalizers` есть запись: объект остаётся в `Terminating`, пока список не опустеет.
- Finalizer обычно снимает контроллер после очистки внешних ресурсов; сначала найти и починить его.
- Если контроллера нет или он удалён, можно вручную: `kubectl patch … -p '{"metadata":{"finalizers":null}}'`, но только убедившись, что внешние ресурсы не требуют очистки.
- В опыте объект исчез сразу после снятия finalizer.

</details>

→ [Экспертная теория](expert.ru.md#подвох-объект-застрял-в-terminating)
