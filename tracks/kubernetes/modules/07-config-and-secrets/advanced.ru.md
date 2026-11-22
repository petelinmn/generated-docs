---
title: "ConfigMap и Secret: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, subpath, immutable, optional, default-mode, secret-types, rollout, binarydata]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# ConfigMap и Secret: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): создание объектов и три способа подключения не повторяются.

## Обновление значений

Изменим ConfigMap, из которого под читает и переменную `APP_MODE`, и файл `/etc/cfg/MODE`, а также второй под, у которого файл подключён через `subPath`:

```yaml
volumeMounts:
  - {name: cfg, mountPath: /etc/mode.txt, subPath: MODE}
```

```text
$ kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"dev"}}'
volume updated to 'dev' after ~57s
env still: prod
subPath file: prod
```

| Способ подключения | Обновляется после правки ConfigMap |
|---|---|
| том (файлы) | да, примерно через минуту (kubelet синхронизирует периодически; в опыте 57 с) |
| том с `subPath` | нет, остаётся старое значение |
| `env`, `envFrom` | нет, только после пересоздания пода |

Файлы обновляются атомарно: kubelet создаёт новый каталог `..2026_…`, затем переставляет ссылку `..data` (так видна цепочка ссылок в `ls -la`). Приложение должно само заметить изменение файла (перечитать конфиг, отслеживать файл). Мгновенного обновления нет.

## Изменение ConfigMap не перезапускает поды

Deployment, который берёт значения из ConfigMap через `envFrom`, после правки ConfigMap не обновляется:

```text
$ kubectl -n learn-07 set env deploy/web --from=configmap/app-config
$ kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"changed"}}'
RS count before=2 after=2
$ kubectl -n learn-07 exec deploy/web -- sh -c 'echo MODE=$MODE'
MODE=dev
```

Новый ReplicaSet не появился, в поде осталось прежнее значение (`dev`). Чтобы поды подхватили изменение, нужно изменить шаблон пода. Способы:

- `kubectl rollout restart deploy/web` ([модуль 05](../05-deployments/advanced.ru.md#перезапуск-без-изменения-образа));
- записать хэш содержимого ConfigMap в аннотацию шаблона пода (так делают Helm-чарты через `checksum/config`);
- давать ConfigMap новое имя при каждом изменении (`app-config-v2`) и менять ссылку в Deployment.

`kubectl set env --from` умеет «разворачивать» ConfigMap в переменные, но ключи, недопустимые как имена переменных, переименовывает:

```text
Warning: key nginx.conf transferred to NGINX_CONF
```

## Неизменяемые объекты

Поле `immutable: true` запрещает менять `data`. Это защищает от случайных правок и снижает нагрузку на API server (kubelet не следит за такими объектами):

```text
$ kubectl -n learn-07 patch cm frozen -p '{"immutable":true}'
$ kubectl -n learn-07 patch cm frozen -p '{"data":{"a":"2"}}'
The ConfigMap "frozen" is invalid: data: Forbidden: field is immutable when `immutable` is set
```

Снять флаг нельзя: объект нужно удалить и создать заново. Практика: выкладывать неизменяемые версии с новым именем (`app-config-v2`) и менять ссылку в Deployment: это и откат, и контролируемый rollout.

> ⚠️ Проверить: утверждение о том, что kubelet не следит за неизменяемыми объектами, дано по документации и не воспроизводилось.

## Отсутствующие объекты и ключи

Если ConfigMap или ключ отсутствует, контейнер не стартует. Три пода с `configMapKeyRef`:

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
```

- `missing` ссылается на несуществующий ConfigMap `nope`;
- `badkey` — на существующий ConfigMap, но несуществующий ключ;
- `optional` имеет `optional: true`: переменная просто не задаётся, под стартует.

Под в `CreateContainerConfigError` ждёт и стартует сам, как только появится нужный объект. Для тома `optional` задаётся в `configMap.optional` и `secret.optional`.

## Права файлов, выбор ключей и путей

Для томов можно выбрать подмножество ключей, задать пути и права:

```yaml
volumes:
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
```

В томе только один выбранный ключ (`username` не подключён), он лежит по пути `nested/pw.txt`, права `0400` (`-r--------`). Права по умолчанию (`defaultMode`) — `0644`; для секретов ограничивайте их, а при запуске не от root учитывайте `fsGroup` ([модуль 17](../../overview.ru.md#часть-3-темы-для-опытных)).

## Бинарные данные

В ConfigMap двоичные файлы хранятся в поле `binaryData` (значение — base64):

```text
$ kubectl -n learn-07 create configmap bin --from-file=bin.dat
$ kubectl -n learn-07 get cm bin -o jsonpath='{.binaryData}'
{"bin.dat":"Yjp5YRak1Ws/dPo/ORayHg=="}
```

Утилита сама определила, что файл бинарный, и положила его в `binaryData`, а не в `data`.

## Типы Secret

Тип задаёт ожидаемые ключи, и API их проверяет:

```text
$ kubectl -n learn-07 create secret docker-registry regcred --docker-server=registry.example.com --docker-username=u --docker-password=p
$ kubectl -n learn-07 get secret regcred -o jsonpath='{.type}'
kubernetes.io/dockerconfigjson
$ kubectl -n learn-07 get secret regcred -o jsonpath='{.data.\.dockerconfigjson}' | base64 -d
{"auths":{"registry.example.com":{"username":"u","password":"p","auth":"dTpw"}}}
$ kubectl -n learn-07 create secret tls demo-tls --cert=tls.crt --key=tls.key
$ kubectl -n learn-07 create secret generic bad --type=kubernetes.io/tls --from-literal=a=b
error: failed to create secret Secret "bad" is invalid: [data[tls.crt]: Required value, data[tls.key]: Required value]
```

`docker-registry` создаёт секрет для скачивания образов из приватного реестра (подключается через `imagePullSecrets`), `tls` — пару сертификат и ключ (для Ingress, модуль 09). Тип `kubernetes.io/tls` требует ключей `tls.crt` и `tls.key`, иначе объект отклонён.

## Лучшие практики

- Разделяйте ConfigMap по назначению и по приложениям, а не складывайте всё в один.
- Секреты монтируйте файлами, а не переменными: переменные видны в `/proc/<pid>/environ` и часто попадают в логи и дампы.
- Давайте ограниченные права файлам секретов (`defaultMode: 0400`) и `readOnly: true` на монтировании.
- Используйте `immutable` и версионирование имён для конфигурации, которую нельзя менять на лету.
- Ограничьте RBAC: право `get`/`list` на secrets есть не у всех (в опыте `auth can-i get secrets` для ServiceAccount по умолчанию — `no`).
- Не коммитьте секреты в Git в открытом виде.

## Частые ошибки

- ❌ Править ConfigMap и ждать обновления подов → ✅ `rollout restart`, хэш в аннотации шаблона или новое имя ConfigMap.
- ❌ Монтировать через `subPath` и ждать обновления → ✅ `subPath` не обновляется; монтируйте весь каталог.
- ❌ Ссылаться на объект, которого ещё нет → ✅ создайте его раньше или задайте `optional: true`, иначе `CreateContainerConfigError`.
- ❌ Использовать `kubernetes.io/tls` без ключей `tls.crt` и `tls.key` → ✅ создавайте через `kubectl create secret tls`.
- ❌ Класть секрет в переменную окружения «для удобства» → ✅ файл с правами `0400`.

## Сравнение с другими треками

- [C#](../../../csharp/modules/13-di-config-logging/overview.ru.md): конфигурация .NET читает переменные окружения и JSON-файлы; в Kubernetes эти же источники наполняются из ConfigMap и Secret (переменная `Section__Key` для вложенных настроек).
- [Linux](../../../linux/overview.ru.md): том — обычные файлы и `tmpfs`; права файлов (`0400`) — те же права Unix.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Следующий модуль: [08 · Хранилище](../08-storage/overview.ru.md)
- Перезапуск Deployment: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)

## Источники

- [ConfigMaps: mounted ConfigMaps are updated automatically](https://kubernetes.io/docs/concepts/configuration/configmap/#mounted-configmaps-are-updated-automatically)
- [ConfigMaps: immutable](https://kubernetes.io/docs/concepts/configuration/configmap/#configmap-immutable)
- [Secrets: types](https://kubernetes.io/docs/concepts/configuration/secret/#secret-types)
- [Volumes: configMap](https://kubernetes.io/docs/concepts/storage/volumes/#configmap)
- [Volumes: secret](https://kubernetes.io/docs/concepts/storage/volumes/#secret)
- [Pull an Image from a Private Registry](https://kubernetes.io/docs/tasks/configure-pod-container/pull-image-private-registry/)
