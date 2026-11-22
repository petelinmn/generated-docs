---
title: "Контейнеры и Docker: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 01-containers
part: 1
tags: [container, docker, dockerfile, multi-stage-build, dockerignore, non-root, entrypoint, pid1, sigterm, digest]
versions: "Docker Engine 29.6.1 (BuildKit), Go 1.24 для примеров"
status: draft
updated: 2026-11-22
---

# Контейнеры и Docker: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): тестовое приложение и команды `docker build` и `docker run` не повторяются.

## Multi-stage сборка

Для сборки Go нужен компилятор на сотни мегабайт, для запуска — только готовый бинарник. Multi-stage разделяет эти задачи: в одном Dockerfile несколько `FROM`, финальный образ берёт из предыдущих стадий только нужные файлы через `COPY --from`.

```dockerfile
# syntax=docker/dockerfile:1
FROM golang:1.24-alpine AS build
WORKDIR /src
COPY go.mod ./
COPY *.go ./
RUN CGO_ENABLED=0 go build -o /hello .

FROM scratch
COPY --from=build /hello /hello
EXPOSE 8080
ENTRYPOINT ["/hello"]
```

`CGO_ENABLED=0` даёт статически слинкованный бинарник: без него Go-программа ищет системную библиотеку C, которой в `scratch` нет. Сравнение с одностадийной сборкой того же кода:

```text
$ docker images hello --format 'table {{.Repository}}:{{.Tag}}\t{{.Size}}'
REPOSITORY:TAG   SIZE
hello:multi      12.8MB
hello:single     516MB
$ docker image inspect hello:multi  --format 'layers={{len .RootFS.Layers}}'
layers=1
$ docker image inspect hello:single --format 'layers={{len .RootFS.Layers}}'
layers=8
```

Образ меньше в 40 раз, слой один. Меньше размер — быстрее скачивание на ноду, меньше поверхность атаки (нет компилятора, нет оболочки). Другие технологии устроены так же: [Native AOT в C#](../../../csharp/modules/01-dotnet-platform/advanced.ru.md) или `venv` в Python копируют в финальный образ только результат сборки.

## Кэш слоёв и .dockerignore

Кэш сбрасывается с первого изменившегося шага, поэтому порядок такой: сначала то, что меняется редко (манифест зависимостей и их установка), потом исходный код. В примере `go.mod` копируется отдельно от `*.go`: правка исходников не заставляет пересчитывать шаги до `COPY *.go`.

Второй источник лишних пересборок — контекст сборки: всё содержимое папки, которую передаёт `docker build`. `COPY . .` без `.dockerignore` забирает в образ и секреты, и тяжёлые папки. Опыт: папка с `.env`, `a.txt` и `node_modules` на 5 МБ.

```text
# без .dockerignore
#4 transferring context: 5.00MB 0.4s done
.env
Dockerfile
a.txt
node_modules

# с .dockerignore (в нём: .env и node_modules)
#5 transferring context: 111B done
.dockerignore
Dockerfile
a.txt
```

Контекст уменьшился с 5 МБ до 111 байт, `.env` в образ не попал. Правила `.dockerignore` похожи на `.gitignore`.

## Не запускать от root

По умолчанию процесс в контейнере работает от root. Если злоумышленник выйдет за пределы приложения, права у него будут максимальными внутри контейнера. Инструкция `USER` переключает пользователя:

```dockerfile
FROM alpine:3.22
RUN adduser -D app
USER app
CMD ["id"]
```

```text
$ docker run --rm t:user
uid=1000(app) gid=1000(app) groups=1000(app)
$ docker run --rm alpine:3.22 id
uid=0(root) gid=0(root) groups=0(root),0(root),1(bin),2(daemon),3(sys),4(adm),6(disk),10(wheel),11(floppy),20(dialout),26(tape),27(video)
```

В Kubernetes то же задаётся в `securityContext` (модуль 17); там можно запретить запуск от root на уровне кластера.

## ENTRYPOINT и CMD

- `ENTRYPOINT` — что запускать, `CMD` — аргументы по умолчанию. Аргументы после имени образа в `docker run` заменяют `CMD`; `--entrypoint` заменяет `ENTRYPOINT`.
- Если задан только `CMD`, `docker run образ команда` заменяет всю команду.

```dockerfile
FROM alpine:3.22
ENTRYPOINT ["echo", "entry"]
CMD ["default-arg"]
```

```text
$ docker run --rm t:cmd
entry default-arg
$ docker run --rm t:cmd custom-arg
entry custom-arg
$ docker run --rm --entrypoint echo t:cmd replaced
replaced
```

В Kubernetes соответствие такое: `command` в манифесте заменяет `ENTRYPOINT`, `args` — `CMD`.

## PID 1 и SIGTERM

`docker stop` посылает главному процессу `SIGTERM`, ждёт 10 секунд и, если процесс жив, посылает `SIGKILL`. Главный процесс контейнера — PID 1 в его namespace, а ядро не применяет к PID 1 действия по умолчанию для сигналов: если программа не установила обработчик `SIGTERM`, она его игнорирует.

```text
$ docker run -d --name ps2 t:exec        # CMD ["sleep", "300"]
$ time docker stop ps2
real    0m10.392s

$ docker run -d --init --name p3 t:exec
$ docker exec p3 ps -o pid,args
PID   COMMAND
    1 /sbin/docker-init -- sleep 300
    7 sleep 300
$ time docker stop p3
real    0m0.336s
```

`sleep` не обрабатывает `SIGTERM`, поэтому контейнер ждал полные 10 секунд до `SIGKILL`. Флаг `--init` запускает перед программой крошечный init-процесс, который пересылает сигналы и подбирает «зомби». Приложение на Go из нашего примера остановилось за 0.45 с: рантайм Go сам обрабатывает `SIGTERM`.

Форма команды тоже важна. Форма shell (`CMD sleep 300`) выполняется как `/bin/sh -c "sleep 300"`; в Alpine оболочка заменяет себя последней командой, поэтому PID 1 всё равно `sleep`. Но если команд несколько, PID 1 остаётся оболочка и сигнал до приложения не доходит:

```text
$ docker run -d --name p4 t:shell2     # CMD sleep 300; echo done
$ docker exec p4 ps -o pid,args
PID   COMMAND
    1 /bin/sh -c sleep 300; echo done
    7 sleep 300
```

Правило: используйте форму exec (`CMD ["программа", "арг"]`) и делайте так, чтобы приложение обрабатывало `SIGTERM`. В Kubernetes остановка работает так же: `SIGTERM`, затем ожидание `terminationGracePeriodSeconds` (по умолчанию 30 с), затем `SIGKILL`.

## Теги и digest

Тег — подвижная ссылка: `app:1.0` можно перезаписать. Digest — хэш содержимого манифеста, он не меняется. Digest видно при `docker push` и в `RepoDigests`:

```text
$ docker image inspect alpine:3.22 --format '{{index .RepoDigests 0}}'
alpine@sha256:49340b346798875b5592e5d420241f2f9cfba5f47e62ebb1446a1b34734e898f
```

Закрепить образ по digest (`alpine@sha256:…`) — единственный способ гарантировать, что завтра запустится то же самое. Практика: человекочитаемый тег для людей (`1.4.2`), digest для воспроизводимых деплоев.

## Выбор базового образа

| База | Размер и состав | Когда |
|---|---|---|
| `scratch` | пусто, только ваш бинарник | статические бинарники (Go, Rust, Native AOT) |
| `alpine` | малый, использует musl вместо glibc, есть `sh` и `apk` | когда нужен shell и компактность; возможны несовместимости с glibc-библиотеками |
| `*-slim` (Debian) | glibc, урезанный набор пакетов | интерпретируемые языки (Python, Node) без сюрпризов совместимости |
| distroless | только рантайм языка, без shell и пакетного менеджера | продакшен, где нужна минимальная поверхность атаки |

Чем меньше в образе, тем меньше в нём уязвимостей, но тем сложнее отладка: в `scratch` нельзя выполнить `docker exec … sh`. Отладка таких подов в Kubernetes — через эфемерные контейнеры (модуль 21).

## Лучшие практики

- Фиксируйте версию базового образа (`alpine:3.22`, а не `alpine` или `latest`).
- Один процесс на контейнер, логи в stdout/stderr, а не в файлы.
- Конфигурация через переменные окружения, секреты — не в образ.
- Multi-stage, `.dockerignore`, не-root пользователь.
- Объединяйте связанные команды в один `RUN`, чтобы не плодить слои, если промежуточные файлы нужно удалить в том же слое.

## Частые ошибки

- ❌ `RUN rm big.bin` в отдельном слое → ✅ файл остаётся в слое под ним. Опыт: `COPY big.bin` на 20 МБ и следующий `RUN rm` дают образ 32.8 МБ против 12.8 МБ у базового. Не добавляйте файл вовсе или удаляйте в том же `RUN`, где создали.
- ❌ Передавать секрет через `ARG` или `RUN echo … && rm` → ✅ значение остаётся в истории слоёв: `docker history --no-trunc` показывает `RUN |1 TOKEN=abc …`. Для секретов сборки используйте `RUN --mount=type=secret`, для секретов запуска — Secret Kubernetes (модуль 07).
- ❌ Ждать, что `ARG` доступен при запуске → ✅ `ARG` живёт только при сборке (в опыте `TOKEN=[]` в контейнере), `ENV` остаётся в образе (`MODE=[prod]`).
- ❌ Полагаться на `HEALTHCHECK` из Dockerfile в Kubernetes → ✅ Kubernetes его игнорирует: под с образом, у которого `HEALTHCHECK` падает, остаётся `1/1 Running`, хотя `docker inspect` показывает `unhealthy`. Используйте probes.

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): контейнер — это namespaces и cgroups; `PID 1` и сигналы — поведение ядра.
- [C#](../../../csharp/modules/01-dotnet-platform/advanced.ru.md): Native AOT даёт статический бинарник для `scratch` или минимального образа.
- Из тестов: [Testcontainers](../../../csharp/libraries/testcontainers/overview.ru.md) запускает те же образы на время теста.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — устройство образа и подвохи
- Следующий модуль: [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)

## Источники

- [Multi-stage builds](https://docs.docker.com/build/building/multi-stage/)
- [Build context и .dockerignore](https://docs.docker.com/build/concepts/context/#dockerignore-files)
- [Dockerfile: ENTRYPOINT и CMD](https://docs.docker.com/reference/dockerfile/#understand-how-cmd-and-entrypoint-interact)
- [docker stop](https://docs.docker.com/reference/cli/docker/container/stop/)
- [Build secrets](https://docs.docker.com/build/building/secrets/)
- [Kubernetes: жизненный цикл Pod, завершение](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-termination)
- [Distroless](https://github.com/GoogleContainerTools/distroless)
