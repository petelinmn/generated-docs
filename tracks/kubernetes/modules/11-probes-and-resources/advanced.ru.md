---
title: "Probes и ресурсы: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 11-probes-and-resources
part: 2
tags: [startup-probe, probes, cgroups, cpu-throttling, oomkilled, cpu-limits, node-allocatable, overcommit]
versions: "Kubernetes 1.37.0 (kind v0.34, cgroup v2), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Probes и ресурсы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): три вида проб, их параметры по умолчанию, requests, limits и QoS-классы не повторяются.

## Медленный старт и startup-проба

Приложение, которое стартует 25 секунд (имитация: `sleep 25` перед запуском nginx), и liveness-проба с `periodSeconds: 3`, `failureThreshold: 2`. Два пода: без startup-пробы и с ней.

```yaml
# со startup-пробой
startupProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
  failureThreshold: 15
livenessProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
  failureThreshold: 2
readinessProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
```

Состояние каждые 12 секунд:

```text
t~12s: slow-nostartup[1/1 Running r=0]  slow-startup[0/1 Running r=0]
t~24s: slow-nostartup[1/1 Running r=0]  slow-startup[0/1 Running r=0]
t~36s: slow-nostartup[1/1 Running r=0]  slow-startup[1/1 Running r=0]
t~48s: slow-nostartup[1/1 Running r=1]  slow-startup[1/1 Running r=0]
```

- **Без startup:** liveness падает с `connection refused` (приложение ещё не слушает), после двух неудач kubelet перезапускает контейнер (`r=1`), и цикл повторяется: приложение никогда не успеет стартовать. У пода без readiness-пробы `READY 1/1` с первой секунды, хотя порт закрыт.
- **Со startup:** пока она не пройдена, liveness и readiness не выполняются; запас 15 × 3 с = 45 с. Две неудачные попытки запуска, затем успех, `started=true ready=true restarts=0`. События: `Startup probe failed: … connection refused` (2 раза).

```text
Warning  Unhealthy  6s (x4 over 45s)  kubelet  Liveness probe failed: Get "http://10.244.0.15:80/": dial tcp 10.244.0.15:80: connect: connection refused
```

Раньше для этого выставляли большой `initialDelaySeconds` у liveness, но он задерживает проверку даже у быстрых запусков. Startup-проба ждёт ровно столько, сколько нужно, и защищает от перезапусков.

## Как проектировать пробы

**Liveness** отвечает на вопрос «приложение безнадёжно зависло?». Она должна быть простой и не зависеть от внешних систем: если liveness проверяет базу, то при падении базы Kubernetes перезапустит все поды сразу, и они все станут недоступны.

**Readiness** отвечает на вопрос «могу ли я сейчас обслужить запрос?». Она может учитывать зависимости, прогрев кэша, очередь запросов: провал снимает трафик, но не убивает.

**Startup** — для долгого запуска.

Практика:

- отдельные адреса для проб (`/livez` и `/readyz`), лёгкие и без побочных эффектов;
- `timeoutSeconds` и `failureThreshold` с запасом: кратковременный всплеск нагрузки не должен приводить к перезапуску;
- `exec`-проба запускает процесс в контейнере при каждой проверке, это дороже `httpGet` и `tcpSocket`;
- пробы выполняются kubelet с ноды: проба по адресу пода идёт по сети подов (поэтому `tcpSocket` на закрытый порт даёт `connection refused`).

Другие механизмы на закрытых портах:

```text
$ kubectl -n learn-11 get pod tcp-bad        # readinessProbe tcpSocket на порт 9999
0/1 Running
  Warning  Unhealthy  kubelet  Readiness probe failed: dial tcp 10.244.0.14:9999: connect: connection refused
$ kubectl -n learn-11 describe pod grpc-probe  # livenessProbe grpc на порт 9000 в поде без gRPC
    Liveness:       grpc <pod>:9000  delay=0s timeout=1s period=3s #success=1 #failure=3
  Warning  Unhealthy  Liveness probe failed: timeout: failed to connect service "10.244.0.27:9000" within 1s: context deadline exceeded
```

Под с провалившейся readiness остаётся `Running` бесконечно, но не получает трафика. gRPC-проба ожидает сервис `grpc.health.v1.Health`.

## Ресурсы через cgroups

Requests и limits превращаются в настройки cgroup v2 контейнера. Значения изнутри подов (QoS `Guaranteed` — 100m/64Mi; `Burstable` — requests 50m/32Mi и лимит памяти 128Mi; `BestEffort` — без ресурсов):

```text
-- guaranteed:  cpu.max=10000 100000   memory.max=67108864
-- burstable:   cpu.max=max 100000     memory.max=134217728
-- besteffort:  cpu.max=max 100000     memory.max=max
```

- `cpu.max` — квота: `10000 100000` означает «10 000 мкс CPU за период 100 000 мкс» = 10% одного ядра = лимит `100m`. Без CPU-лимита — `max`.
- `memory.max` — лимит памяти в байтах: 64 МиБ = 67 108 864; без лимита — `max`.

CPU-заявка (`requests.cpu`) превращается в **вес** `cpu.weight` для распределения при конкуренции:

```text
guaranteed cpu.weight=17     # requests 100m
burstable  cpu.weight=11     # requests 50m
besteffort cpu.weight=1      # без requests
onlyreq    cpu.weight=17     # requests 100m
```

Заявка не резервирует CPU: пока на ноде есть свободные ядра, контейнер без лимита использует их сколько угодно; вес работает, когда ядер не хватает. Память же при превышении лимита убивает контейнер.

## CPU throttling

CPU — «сжимаемый» ресурс: при превышении лимита контейнер не убивают, а притормаживают. Опыт: цикл, жгущий CPU 20 секунд в двух подах, один с лимитом `100m`, второй без лимита. Статистика cgroup после цикла:

```text
=== burn-limited (limit 100m)
usage_usec 2023496        # использовано 2.0 с CPU за 20 с
nr_periods 202
nr_throttled 202          # притормаживали в каждом периоде
throttled_usec 8562744    # 8.6 с ожидания
=== burn-free (без лимита)
usage_usec 20017330       # 20 с CPU за 20 с — целое ядро
nr_periods 0
nr_throttled 0
throttled_usec 0
```

Под с лимитом получил 10% ядра (2.0 из 20 секунд) и был притормаживаем в каждом из 202 периодов, хотя нода почти свободна: **лимит работает независимо от загрузки ноды**. Отсюда практика: чувствительным к задержкам сервисам не ставят CPU-лимит (оставляют только requests), либо ставят с запасом и следят за метрикой throttling.

> ⚠️ Проверить: рекомендация «не ставить CPU-лимит» зависит от политики кластера; в этом материале проверен только эффект throttling.

## Память и OOMKilled

Память — «несжимаемый» ресурс: при превышении лимита ядро убивает процесс (OOM killer). Контейнер с `limits.memory: 20Mi` и жадной командой завершился с `reason=OOMKilled exit=137` ([модуль 04](../04-pods/expert.ru.md#коды-выхода)). Практика: лимит памяти равен заявке или близок к ней; следите за реальным потреблением и запасом.

## Ёмкость ноды

Планировщик сравнивает суммарные requests подов с **allocatable** ноды:

```text
$ kubectl get node learn-control-plane -o jsonpath='cpu={.status.allocatable.cpu} mem={.status.allocatable.memory} pods={.status.allocatable.pods}'
cpu=12 mem=32736456Ki pods=110
```

У ноды 12 ядер, около 31 ГиБ памяти и предел 110 подов. Пока сумма заявок не превышает allocatable, новые поды планируются; в `describe node` видны суммарные requests и limits. Суммы limits могут превышать ёмкость (`Total limits may be over 100 percent, i.e., overcommitted`): потолки не резервируют ресурсы, а заявки — резервируют (для планирования).

## Лучшие практики

- Всегда указывайте `requests`; для памяти ставьте `limits` равным или близким к `requests`.
- Для приложений за Service обязательна readiness-проба; liveness — только если приложение действительно может зависнуть.
- Медленный старт — startup-проба, а не большой `initialDelaySeconds`.
- Проектируйте liveness без внешних зависимостей, а пробы — лёгкими.
- Применяйте LimitRange и ResourceQuota на namespace ([модуль 10](../10-namespaces-and-rbac/advanced.ru.md#limitrange-значения-по-умолчанию-и-границы)).
- Меряйте фактическое потребление (metrics-server, Prometheus) и подгоняйте requests по данным.

## Частые ошибки

- ❌ Liveness проверяет базу данных → ✅ внешние зависимости — в readiness.
- ❌ Большой `initialDelaySeconds` у liveness вместо startup → ✅ startup-проба.
- ❌ Ждать, что `requests.cpu` ограничит потребление → ✅ ограничивает только `limits`; заявка определяет вес и планирование.
- ❌ Ставить CPU-лимит ниже реальной нагрузки → ✅ throttling и всплески задержек даже на свободной ноде.
- ❌ Считать `READY 1/1` признаком работающего приложения без readiness-пробы → ✅ это лишь «контейнер запущен».

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): `cpu.max`, `cpu.weight` и `memory.max` — файлы cgroup v2; Kubernetes только записывает в них значения.
- [C#](../../../csharp/modules/22-observability/overview.ru.md): health-эндпоинты `/livez` и `/readyz` приложения ASP.NET Core соответствуют liveness и readiness.
- [Docker](../01-containers/expert.ru.md#справочник): флаги `docker run --memory` и `--cpus` — те же cgroups.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Выкатка и готовность: [05 · Deployment и ReplicaSet](../05-deployments/advanced.ru.md#застрявший-деплой), [06 · Service и сеть](../06-services/advanced.ru.md#готовность-подов-и-endpoints)

## Источники

- [Liveness, Readiness, and Startup Probes: startup probe](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/#startup-probe)
- [Resource Management: how Pods with resource limits are run](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/#how-pods-with-resource-limits-are-run)
- [Node Allocatable](https://kubernetes.io/docs/tasks/administer-cluster/reserve-compute-resources/)
- [Linux kernel: cgroup v2 CPU controller](https://docs.kernel.org/admin-guide/cgroup-v2.html#cpu)
- [Configure Quality of Service for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/quality-service-pod/)
