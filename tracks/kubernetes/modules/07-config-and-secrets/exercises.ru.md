---
title: "Упражнения: ConfigMap и Secret"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, volume, subpath, immutable, rollout]
status: draft
updated: 2026-11-22
---

# Упражнения: ConfigMap и Secret

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-07`. Убрать за собой: `kubectl delete namespace learn-07`.

## 1. 🟢 Создать и подключить

**Задача:** создай ConfigMap `app-config` (`MODE=prod`, `TIMEOUT=30`) и Secret `db` (`username=admin`, `password=S3cr3t!`). Запусти под на `alpine:3.22`, которому `MODE` приходит как `APP_MODE`, а пароль — как `DB_PASSWORD`. Проверь переменные.

**Ожидаемый результат:**

```text
APP_MODE=prod
DB_PASSWORD=S3cr3t!
```

<details><summary>Подсказка</summary>

`env[].valueFrom.configMapKeyRef` и `secretKeyRef`. Проверка: `kubectl exec app -- env`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-07 create configmap app-config --from-literal=MODE=prod --from-literal=TIMEOUT=30
kubectl -n learn-07 create secret generic db --from-literal=username=admin --from-literal=password='S3cr3t!'
```

```yaml
apiVersion: v1
kind: Pod
metadata: {name: app}
spec:
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "sleep 3600"]
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

</details>

## 2. 🟢 base64 и describe

**Задача:** выведи Secret `db` в YAML, декодируй пароль и покажи, что показывает `kubectl describe secret`. Объясни, почему base64 — не защита.

**Ожидаемый результат:** в YAML `UzNjcjN0IQ==`, после декодирования `S3cr3t!`, в `describe` только `7 bytes`.

<details><summary>Подсказка</summary>

`kubectl get secret db -o jsonpath='{.data.password}' | base64 -d`.

</details>

<details><summary>Решение</summary>

```text
$ kubectl -n learn-07 get secret db -o yaml
data:
  password: UzNjcjN0IQ==
  username: YWRtaW4=
$ kubectl -n learn-07 get secret db -o jsonpath='{.data.password}' | base64 -d
S3cr3t!
$ kubectl -n learn-07 describe secret db
password:  7 bytes
username:  5 bytes
```

base64 — обратимая кодировка без ключа: декодирует любой, кто читает объект. `describe` скрывает значения только в своём выводе.

</details>

## 3. 🟡 Файлы, права и обновление

**Задача:** подключи ConfigMap `app-config` томом в `/etc/cfg`, а Secret `db` — в `/etc/sec` только ключ `password` по пути `nested/pw.txt` с правами `0400`. Затем измени `MODE` в ConfigMap и замерь, через сколько секунд файл `/etc/cfg/MODE` изменится, и изменится ли переменная `APP_MODE`.

**Ожидаемый результат:** `-r--------` у `pw.txt`; файл обновился примерно через минуту, переменная — нет.

<details><summary>Подсказка</summary>

`secret.items` и `defaultMode: 0400`. Для замера — цикл с `sleep 5` и `kubectl exec … cat /etc/cfg/MODE`.

</details>

<details><summary>Решение</summary>

```yaml
volumes:
  - name: cfg
    configMap: {name: app-config}
  - name: sec
    secret:
      secretName: db
      defaultMode: 0400
      items:
        - {key: password, path: nested/pw.txt}
```

```text
$ kubectl -n learn-07 exec modes -- sh -c 'ls -lL /etc/sec/nested; ls /etc/sec'
-r--------    1 root     root             7 Nov 22 18:26 pw.txt
nested
$ kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"dev"}}'
volume updated to 'dev' after ~57s
env still: prod
```

Файлы тома kubelet обновляет периодически, переменные фиксируются при старте контейнера.

</details>

## 4. 🟡 Отсутствующие объекты

**Задача:** создай три пода: со ссылкой на несуществующий ConfigMap, на несуществующий ключ существующего ConfigMap и с `optional: true`. Объясни статусы и сообщения, затем создай недостающий ConfigMap и проследи за первым подом.

**Ожидаемый результат:** `CreateContainerConfigError`, `CreateContainerConfigError`, `Running`; после создания объекта первый под становится `Running`.

<details><summary>Подсказка</summary>

Причину ищи в `kubectl describe pod`. `optional: true` ставится в `configMapKeyRef`.

</details>

<details><summary>Решение</summary>

```text
$ kubectl -n learn-07 get pods missing badkey optional
NAME       READY   STATUS                       RESTARTS   AGE
missing    0/1     CreateContainerConfigError   0          16s
badkey     0/1     CreateContainerConfigError   0          15s
optional   1/1     Running                      0          15s
$ kubectl -n learn-07 describe pod missing
  Warning  Failed  kubelet  Error: configmap "nope" not found
$ kubectl -n learn-07 describe pod badkey
  Warning  Failed  kubelet  Error: couldn't find key nokey in ConfigMap learn-07/app-config
$ kubectl -n learn-07 create configmap nope --from-literal=k=v
missing pod: Running after ~20s
```

Без `optional` отсутствие объекта или ключа блокирует старт; под сам стартует, когда объект появляется.

</details>

## 5. 🔴 Обновление Deployment при смене ConfigMap

**Задача:** запусти Deployment `web`, который получает переменные из ConfigMap через `envFrom`. Измени ConfigMap и покажи, что новый ReplicaSet не появился. Затем заставь Deployment подхватить значение и объясни ещё два способа.

**Ожидаемый результат:** до `rollout restart` число ReplicaSet прежнее и в поде старое значение; после — новое.

<details><summary>Подсказка</summary>

`kubectl set env deploy/web --from=configmap/app-config`, затем `kubectl rollout restart deploy/web`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-07 create deployment web --image=alpine:3.22 -- sleep 3600
kubectl -n learn-07 set env deploy/web --from=configmap/app-config
kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"changed"}}'
kubectl -n learn-07 get rs --no-headers | wc -l
kubectl -n learn-07 exec deploy/web -- sh -c 'echo MODE=$MODE'
kubectl -n learn-07 rollout restart deploy/web
kubectl -n learn-07 rollout status deploy/web
```

```text
RS count before=2 after=2
MODE=dev
```

После `rollout restart` и `rollout status` повторная проверка `exec` показывает `MODE=changed`, а ReplicaSet стало 3.

Значение осталось старым (`dev`), потому что шаблон пода не менялся. `rollout restart` добавляет аннотацию в шаблон и запускает rolling update. Ещё два способа: хэш содержимого ConfigMap в аннотации шаблона (как `checksum/config` в Helm) и новое имя ConfigMap при каждом изменении с правкой ссылки в Deployment.

</details>
