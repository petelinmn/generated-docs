---
title: "Контейнеры и Docker: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 01-containers
part: 1
tags: [container, docker, image, dockerfile, registry, layers, volume]
versions: "Docker Engine 29.6.1 (BuildKit), Go 1.24 для примеров"
status: draft
updated: 2026-11-22
---

# Контейнеры и Docker: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [трек Linux](../../../linux/overview.ru.md) (процессы, файловая система) · **Время:** ~2 ч

Все команды выполнены на Docker Engine 29.6.1. Команды `docker` в Windows запускают в WSL2 или в PowerShell с Docker Desktop.

## Зачем это нужно

Приложение зависит от рантайма, библиотек и настроек окружения. «У меня работает» часто значит «у меня стоит другая версия». Контейнер упаковывает приложение вместе с его файловой системой, так что оно запускается одинаково на ноутбуке, в CI и на сервере. В отличие от виртуальной машины, контейнер не несёт собственного ядра: это процесс хоста, которому ядро показывает отдельную файловую систему, сеть и список процессов. Поэтому он стартует за доли секунды и весит мегабайты.

## Образ и контейнер

- **Образ (image)** — неизменяемый шаблон: файлы приложения плюс метаданные (команда запуска, переменные окружения, порты).
- **Контейнер (container)** — запущенный экземпляр образа. Из одного образа можно запустить много контейнеров.

Аналогия из программирования: образ — класс, контейнер — объект.

## Тестовое приложение

Для всех примеров модуля берём крошечный HTTP-сервер на Go. Язык не важен: нужен единственный исполняемый файл.

```go
package main

import (
	"fmt"
	"net/http"
	"os"
)

func main() {
	greeting := os.Getenv("GREETING")
	if greeting == "" {
		greeting = "Hello"
	}
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		host, _ := os.Hostname()
		fmt.Fprintf(w, "%s from %s\n", greeting, host)
	})
	fmt.Println("listening on :8080")
	http.ListenAndServe(":8080", nil)
}
```

Сервер читает приветствие из переменной окружения `GREETING` и отвечает именем хоста. В контейнере имя хоста — идентификатор контейнера, по нему удобно видеть, какой экземпляр ответил. В папке рядом лежит `go.mod` с содержимым `module hello` и `go 1.24`.

## Dockerfile

Dockerfile — текстовый рецепт сборки образа. Простейший вариант:

```dockerfile
FROM golang:1.24-alpine
WORKDIR /src
COPY . .
RUN go build -o /hello .
EXPOSE 8080
CMD ["/hello"]
```

| Инструкция | Что делает |
|---|---|
| `FROM` | базовый образ, с которого начинается сборка |
| `WORKDIR` | рабочая папка для следующих инструкций |
| `COPY` | копирует файлы из контекста сборки в образ |
| `RUN` | выполняет команду при сборке; результат становится слоем |
| `EXPOSE` | документирует порт; порт сам по себе не публикуется |
| `CMD` | команда по умолчанию при запуске контейнера |

## Сборка и запуск

```bash
docker build -t hello:single -f Dockerfile.single .   # простой Dockerfile из примера выше
docker build -t hello:multi .                         # многостадийный Dockerfile (см. обзор)
docker run -d --name h1 -p 18080:8080 hello:multi
```

`-t` задаёт имя и тег, `-f` — файл Dockerfile (по умолчанию `Dockerfile`), `-d` запускает в фоне, `--name` даёт контейнеру имя, `-p хост:контейнер` публикует порт. Основные команды управления:

```bash
docker ps                 # запущенные контейнеры
docker logs h1            # stdout и stderr контейнера
docker exec -it h1 sh     # команда внутри запущенного контейнера (нужен sh в образе)
docker stop h1            # остановить (SIGTERM, затем SIGKILL)
docker rm h1              # удалить остановленный контейнер
```

Образ `hello:multi` собран на `scratch`: в нём нет даже `sh`, и `docker exec h1 sh` завершится ошибкой (см. [подвохи](expert.ru.md#подвох-в-образе-на-scratch-нет-shell)). В `hello:single` оболочка есть.

Результат запроса к двум контейнерам из одного образа:

```text
$ curl -s localhost:18080
Hello from 74d74856967e
$ curl -s localhost:18081
Privet from 53b97f96b876
```

Второй контейнер запущен с `-e GREETING=Privet`: переменная окружения меняет поведение без пересборки образа. Хосты разные: это два разных контейнера.

Логи читаются из stdout:

```text
$ docker logs h1
listening on :8080
```

Контейнер — это процесс, пока он жив, контейнер «запущен». Когда главный процесс завершился, контейнер остановился.

## Слои и кэш

Каждая инструкция `RUN`, `COPY` или `ADD` создаёт слой — набор изменений файловой системы. Образ — стопка слоёв. Docker (BuildKit) запоминает результат каждого шага и при повторной сборке пропускает неизменившиеся. Как только шаг изменился, все следующие выполняются заново.

Многостадийный Dockerfile из примера (`FROM … AS build`, затем `FROM scratch`) собирается так. Повторная сборка без изменений:

```text
#9 [build 4/5] COPY *.go ./
#9 CACHED
#8 [build 5/5] RUN CGO_ENABLED=0 go build -o /hello .
#8 CACHED
```

После правки `main.go`:

```text
#8 [build 2/5] WORKDIR /src
#8 CACHED
#9 [build 3/5] COPY go.mod ./
#9 CACHED
#10 [build 4/5] COPY *.go ./
#11 [build 5/5] RUN CGO_ENABLED=0 go build -o /hello .
#12 [stage-1 1/1] COPY --from=build /hello /hello
```

Шаги до `COPY *.go` взяты из кэша, начиная с изменённого файла всё выполнено заново. Поэтому редко меняющееся (зависимости) копируют раньше часто меняющегося (исходники). Подробнее — в [продвинутой теории](advanced.ru.md#кэш-слоёв-и-dockerignore).

## Данные: слой для записи и volume

Контейнер пишет в тонкий слой поверх образа. Он удаляется вместе с контейнером:

```text
$ docker exec h3 sh -c 'echo hi > /tmp/x'
$ docker diff h3
C /tmp
A /tmp/x
```

`A` — файл добавлен, `C` — папка изменена. Чтобы данные пережили контейнер, используют **volume** — папку, которой управляет Docker вне слоя контейнера:

```text
$ docker volume create v1
$ docker run --rm -v v1:/data alpine:3.22 sh -c 'echo persisted > /data/f'
$ docker run --rm -v v1:/data alpine:3.22 cat /data/f
persisted
```

Первый контейнер удалён, данные прочитал уже второй. В Kubernetes ту же роль играют тома и PersistentVolume.

## Реестр и имя образа

Реестр (registry) — сервис хранения образов: Docker Hub, GitHub Container Registry, облачные реестры. Полное имя образа:

```text
localhost:15000 / team/hello : 1.0
 реестр          репозиторий   тег
```

Если реестр не указан, подразумевается Docker Hub, если тег не указан — `latest`. Локальный реестр для опытов запускается одной командой (`docker run -d -p 15000:5000 registry:2`):

```text
$ docker tag hello:multi localhost:15000/team/hello:1.0
$ docker push localhost:15000/team/hello:1.0
1.0: digest: sha256:f3b18a09f5f30b7befd534594e3dc3e6a27b215e42c262233e7c9b115273a60e size: 855
$ curl -s localhost:15000/v2/_catalog
{"repositories":["team/hello"]}
$ curl -s localhost:15000/v2/team/hello/tags/list
{"name":"team/hello","tags":["1.0"]}
```

`docker pull` скачивает образ из реестра. Kubernetes делает то же на каждой ноде, когда запускает под.

## Ошибки новичка

- ❌ Писать данные внутрь контейнера → ✅ выносить их в volume (в Kubernetes — в PersistentVolume): слой для записи пропадает вместе с контейнером.
- ❌ Запускать два процесса в одном контейнере «как в виртуалке» → ✅ один контейнер — один основной процесс, остальные в других контейнерах.
- ❌ Ждать, что `EXPOSE 8080` откроет порт → ✅ порт публикуется только флагом `-p` (в Kubernetes — через Service).
- ❌ Использовать тег `latest` → ✅ фиксировать версию: `latest` не говорит, какая версия внутри, и в Kubernetes меняет политику загрузки (см. [подвохи](expert.ru.md#подвох-latest-в-kubernetes)).
- ❌ Копировать в образ всё подряд, включая `.env` и `node_modules` → ✅ добавить `.dockerignore` ([продвинутая теория](advanced.ru.md#кэш-слоёв-и-dockerignore)).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — multi-stage, не-root, `PID 1`
- Следующий модуль: [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — процессы, namespaces и cgroups
- Из тестов на C#: [Testcontainers](../../../csharp/libraries/testcontainers/overview.ru.md)

## Источники

- [Dockerfile reference](https://docs.docker.com/reference/dockerfile/)
- [Docker build: кэш](https://docs.docker.com/build/cache/)
- [docker run](https://docs.docker.com/reference/cli/docker/container/run/)
- [Volumes](https://docs.docker.com/engine/storage/volumes/)
- [Registry: деплой](https://distribution.github.io/distribution/about/deploying/)
