---
title: "Упражнения: контейнеры и Docker"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 01-containers
part: 1
tags: [container, docker, dockerfile, multi-stage-build, pid1, dockerignore]
status: draft
updated: 2026-11-22
---

# Упражнения: контейнеры и Docker

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Для упражнений нужен Docker; приложение из [базовой теории](basics.ru.md) (`main.go` и `go.mod`) лежит в папке `app/`.

## 1. 🟢 Два контейнера из одного образа

**Задача:** собери многостадийный образ приложения, запусти два контейнера с разными приветствиями на портах 18080 и 18081 и убедись, что хосты в ответах разные. Затем останови и удали оба.

**Ожидаемый результат:**

```text
Hello from 74d74856967e
Privet from 53b97f96b876
```

(идентификаторы хостов у тебя будут другие)

<details><summary>Подсказка</summary>

Переменная окружения `GREETING` задаётся флагом `-e`, порт хоста — первым числом в `-p`.

</details>

<details><summary>Решение</summary>

```bash
docker build -t hello:multi .
docker run -d --name h1 -p 18080:8080 hello:multi
docker run -d --name h2 -p 18081:8080 -e GREETING=Privet hello:multi
curl -s localhost:18080
curl -s localhost:18081
docker stop h1 h2 && docker rm h1 h2
```

Контейнеры используют один образ, но у каждого свой слой для записи, свои переменные и свой идентификатор (он же hostname).

</details>

## 2. 🟡 Кэш и .dockerignore

**Задача:** в папке лежат `a.txt`, `.env` и `node_modules/big.bin` (5 МБ). Напиши Dockerfile, который копирует папку в образ и выводит её содержимое, и добейся, чтобы `.env` и `node_modules` в образ не попали и не передавались в контекст сборки.

**Ожидаемый результат:** при сборке `transferring context` — порядка 100 байт, в выводе `ls` нет `.env` и `node_modules`.

<details><summary>Подсказка</summary>

Нужен файл `.dockerignore` рядом с Dockerfile. Сравни размер контекста до и после: используй `docker build --no-cache --progress=plain .`.

</details>

<details><summary>Решение</summary>

```dockerfile
FROM alpine:3.22
COPY . /ctx
RUN ls -A /ctx
```

```text
# .dockerignore
.env
node_modules
```

```text
#5 transferring context: 111B done
.dockerignore
Dockerfile
a.txt
```

Без `.dockerignore` контекст был 5.00MB, а `.env` и `node_modules` оказывались в образе.

</details>

## 3. 🟡 Быстрая остановка

**Задача:** контейнер `CMD ["sleep", "300"]` останавливается 10 секунд. Покажи два способа остановить его быстро, не меняя `sleep`: запуск с `--init` и shell-обёртка с `trap`. Измерь время остановки.

**Ожидаемый результат:** `docker stop` занимает меньше секунды.

<details><summary>Подсказка</summary>

Нужно, чтобы кто-то в контейнере реагировал на `SIGTERM`: либо init-процесс, либо оболочка с `trap`. Для `trap` оболочке нужно ждать `sleep` через `wait`, иначе сигнал не обработается, пока `sleep` не завершится.

</details>

<details><summary>Решение</summary>

Способ 1: `--init`.

```bash
docker run -d --init --name p3 t:exec    # t:exec — образ с CMD ["sleep", "300"]
time docker stop p3                      # real 0m0.336s
```

Способ 2: оболочка с `trap`.

```dockerfile
FROM alpine:3.22
CMD ["sh","-c","trap \"echo bye; exit 0\" TERM; sleep 300 & wait"]
```

```text
$ time docker stop tr
real    0m0.333s
$ docker logs tr
bye
```

В обоих случаях сигнал `SIGTERM` обрабатывает процесс, который умеет его перехватывать, а не PID 1 без обработчика.

</details>

## 4. 🔴 Образ без мусора и без секретов

**Задача:** есть два Dockerfile-а. В первом `COPY big.bin /big.bin` (20 МБ), затем `RUN rm /big.bin`; образ весит 32.8 МБ. Сделай два исправленных варианта, чтобы итоговый образ весил как базовый `alpine:3.22` (12.8 МБ): удаление в том же слое и multi-stage. Затем проверь, что секрет сборки, переданный через `--secret`, не виден в `docker history`.

**Ожидаемый результат:**

```text
g:base 12.8MB
g:one  12.8MB
g:ms   12.8MB
```

<details><summary>Подсказка</summary>

Файл нельзя удалить из уже созданного слоя. Либо создай и удали его в одном `RUN`, либо используй его в промежуточной стадии, а в финальную скопируй только результат. Для секрета: `RUN --mount=type=secret,id=…` и `docker build --secret id=…,env=…`.

</details>

<details><summary>Решение</summary>

Вариант 1: создание и удаление в одном `RUN`:

```dockerfile
FROM alpine:3.22
RUN head -c 20000000 /dev/zero > /big.bin && rm /big.bin
```

Вариант 2: multi-stage:

```dockerfile
FROM alpine:3.22 AS build
COPY big.bin /big.bin
RUN echo result > /out.txt

FROM alpine:3.22
COPY --from=build /out.txt /out.txt
```

```text
$ docker images g --format '{{.Repository}}:{{.Tag}} {{.Size}}' | sort
g:base 12.8MB
g:ms 12.8MB
g:one 12.8MB
g:rm 32.8MB
```

Секрет сборки:

```dockerfile
# syntax=docker/dockerfile:1
FROM alpine:3.22
RUN --mount=type=secret,id=token echo "token length: $(wc -c < /run/secrets/token)"
```

```text
$ TOKEN=abc docker build --secret id=token,env=TOKEN -t g:sec .
token length: 3
$ docker history g:sec --no-trunc | grep -c abc
0
$ docker run --rm g:sec ls /run/secrets
ls: /run/secrets: No such file or directory
```

Секрет доступен только во время инструкции `RUN`, в историю и в образ он не попадает.

</details>
