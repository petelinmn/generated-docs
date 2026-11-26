---
title: "Масштабирование и балансировка: экспертная теория"
lang: ru
track: system-design
kind: module
doc: expert
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Масштабирование и балансировка: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Закон Амдала: если доля `s` работы выполняется последовательно, ускорение на `n` узлах равно `n / (1 + s(n-1))` и не превышает `1/s`. Universal Scalability Law (USL) добавляет стоимость согласования узлов, из-за которой производительность после пика *падает*.

```csharp
public static double Amdahl(double serialFraction, int n) => n / (1 + serialFraction * (n - 1));

// contention: waiting for a shared resource; coherency: nodes keeping each other up to date.
public static double Universal(double contention, double coherency, int n) =>
    n / (1 + contention * (n - 1) + coherency * n * (n - 1));
```

| Узлов | Амдал, s=0,05 | USL, 0,05 и 0,001 |
|---|---|---|
| 1 | 1,0 | 1,0 |
| 10 | 6,9 | 6,5 |
| 31 | 12,4 | 9,0 |
| 100 | 16,8 | 6,3 |

Коэффициенты здесь учебные: для реальной системы их получают подгонкой по измерениям.

```mermaid
flowchart LR
    C[Client] --> LB[L7 load balancer]
    LB --> A[API 1]
    LB --> B[API 2]
    A --> S[(Shared state)]
    B --> S
```

### Rendezvous hashing

Кольцо не нужно: для ключа считают хэш с каждым узлом и берут максимум.

```csharp
public static string NodeFor(string key, IReadOnlyList<string> nodes) =>
    nodes.MaxBy(node => StableHash.Of($"{node}\n{key}"))!;
```

При удалении узла переезжают только его ключи (в тесте из пяти узлов — около пятой части). Цена — O(n) на поиск. Файл: [Ex05_Rendezvous.cs](code/Examples/Ex05_Rendezvous.cs).

### Maglev hashing

Балансировщик Google строит таблицу фиксированного размера `M`. Каждому узлу два хэша задают `offset` и `skip`, а они — порядок предпочтения слотов `(offset + j × skip) mod M`. Узлы по очереди занимают первый свободный слот из своего списка. Поиск — O(1).

```csharp
// Nodes take turns, each claiming the first free slot of its own preference order.
for (int filled = 0, i = 0; filled < size; i = (i + 1) % nodes.Count)
{
    int slot;
    do slot = (int)((offset[i] + (long)next[i]++ * skip[i]) % size);
    while (_table[slot] is not null);
```

`M` должно быть простым, чтобы любой `skip` обходил все слоты; пример отклоняет составной размер. Доли узлов различаются не более чем на один слот. В статье приоритет отдан равномерности: при изменении состава меняется небольшая часть «чужих» слотов, но не ноль, и поэтому Maglev дополнительно отслеживает соединения. Размер по умолчанию в статье — 65537. Файл: [Ex06_Maglev.cs](code/Examples/Ex06_Maglev.cs).

### Корректная остановка в ASP.NET Core

По сигналу остановки хост вызывает `ApplicationStopping`, веб-сервер перестаёт принимать новые соединения и даёт активным запросам время завершиться. `HostOptions.ShutdownTimeout` по умолчанию 30 секунд (проверено на .NET 11); не успевшие запросы обрываются.

```csharp
builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = shutdownTimeout);
```

Тесты показывают оба исхода: запрос, отпущенный во время остановки, завершается успешно; запрос дольше таймаута получает `HttpRequestException`. Файл: [Ex07_Draining.cs](code/Examples/Ex07_Draining.cs).

В Kubernetes остановка pod и удаление его из EndpointSlice идут одновременно, поэтому новые запросы ещё могут приходить некоторое время. Отведённое на остановку время (`terminationGracePeriodSeconds`) по умолчанию 30 секунд и включает `preStop`: таймаут приложения должен укладываться в него.

### Балансировка на клиенте

Клиент сам получает список адресов (resolver) и выбирает адрес для вызова (load balancer). В `Grpc.Net.Client` по умолчанию `pick_first`; `round_robin` включают через `ServiceConfig`, а адрес записывают со схемой `dns:///`. Канал нужно переиспользовать. Выигрыш — нет лишнего прыжка; цена — клиент должен знать узлы и правильно настраиваться.

## Справочник

| Политика YARP | Поведение |
|---|---|
| `PowerOfTwoChoices` | два случайных адреса, из них менее загруженный; по умолчанию |
| `RoundRobin` | по кругу |
| `LeastRequests` | минимум запросов, просматривает все адреса |
| `Random` | случайный |
| `FirstAlphabetical` | первый по имени; удобно для failover |

Своя политика реализует `ILoadBalancingPolicy`.

| Настройка YARP | Значение по умолчанию |
|---|---|
| `HealthCheck:Active:Enabled` | `false` |
| `HealthCheck:Active:Interval` / `Timeout` | 15 с / 10 с |
| порог `ConsecutiveFailures` | 2 |
| `HealthCheck:Passive:Enabled` | `false` |
| `HealthCheck:AvailableDestinationsPolicy` | `HealthyOrPanic` |
| `SessionAffinity:Enabled` | `false` |
| `SessionAffinity:Policy` | `HashCookie` |
| `SessionAffinity:FailurePolicy` | `Redistribute` |

`HealthyOrPanic` при отсутствии здоровых адресов использует все; `HealthyAndUnknown` в этом случае отвечает 503.

| Уровень балансировки | Примеры механизма |
|---|---|
| DNS | несколько адресов на имя; кэширование мешает быстрому переключению |
| Географический | anycast, глобальные балансировщики |
| L4 | распределение соединений |
| L7 | reverse proxy, ingress |
| На клиенте | клиент сам выбирает адрес (например, gRPC client-side balancing) |

Автомасштабирование в Kubernetes (HPA): `desiredReplicas = ceil(currentReplicas × currentMetricValue / desiredMetricValue)`, с допуском 0,1 по умолчанию.

| Хэширование | Поиск | Память | Особенность |
|---|---|---|---|
| `hash % N` | O(1) | нет | переезжает почти всё |
| Кольцо с виртуальными узлами | O(log V) | V точек | веса через число точек |
| Rendezvous | O(n) | нет | простота, переезжают только ключи удалённого узла |
| Maglev | O(1) | таблица M | равномерность, небольшие лишние переезды |

| Поведение HPA по умолчанию | Вверх | Вниз |
|---|---|---|
| Окно стабилизации | 0 с | 300 с |
| Политики за 15 с | +100% или +4 pod (берётся большее) | до −100% |

При уменьшении HPA берёт наибольшее из желаемых значений за окно, чтобы не удалять pod-ы и тут же не создавать снова.

| Вид автомасштабирования | Когда |
|---|---|
| Реактивное по метрике | нагрузка растёт плавнее, чем запускаются экземпляры |
| По расписанию | известные пики |
| По длине очереди | воркеры и фоновая обработка |
| Предварительный запас | резкие всплески и долгий прогрев |

| Глобальное распределение | Как выбирается площадка | Особенность |
|---|---|---|
| DNS по географии или задержке | ответ сервера имён | переключение ограничено кэшированием DNS |
| Anycast | один адрес объявляется из нескольких мест, путь выбирает маршрутизация | клиенту ничего не нужно менять |

## Подвохи

### GetHashCode меняется между запусками

```csharp
Console.WriteLine("user:1".GetHashCode());
```

Два запуска напечатали `-1500850359` и `1795110506`. Хэш строк рандомизирован для процесса, поэтому разные экземпляры направили бы один ключ на разные шарды. Используйте стабильный хэш.

### Переполнение счётчика round robin

```csharp
int counter = int.MaxValue;
int next = Interlocked.Increment(ref counter);
Console.WriteLine($"{next} {next % 3}");
```

Результат: `-2147483648 -2` — отрицательный индекс и исключение при обращении к списку. `Math.Abs` не спасает: `Math.Abs(int.MinValue)` бросает `OverflowException`. Решение — приведение к `uint`.

### Остановка обрывает долгие запросы

Приложение с `ShutdownTimeout` 200 мс и запросом, который ещё выполняется: `StopAsync` завершается, а клиент получает `HttpRequestException`. Долгие операции выносите в фон или увеличивайте таймаут вместе с таймаутом платформы.

### Размер таблицы Maglev должен быть простым

Если `skip` и размер таблицы имеют общий делитель, узел обходит лишь часть слотов, и поиск свободного может не закончиться. Поэтому пример бросает `ArgumentException` для размера 65536.

## Компромиссы и альтернативы

| Подход | Когда | Цена |
|---|---|---|
| Scale up | база данных, небольшие системы | потолок и простой при замене |
| Копии stateless-сервиса | веб-API | нагрузка переезжает в общие ресурсы |
| Шардирование | данные не помещаются на узел | сложные запросы и ребалансировка |
| Очередь и воркеры | неравномерная фоновая работа | асинхронность |
| Managed load balancer | облако | меньше контроля |

Альтернатива кольцу — rendezvous hashing (выбирается узел с максимальным хэшем пары «ключ, узел»): проще, но выбор стоит O(N). Orleans и другие акторные фреймворки решают задачу размещения состояния на своём уровне.

## Вопросы с собеседований

**1. Чем scale up отличается от scale out?**

<details><summary>Ответ</summary>

Первый увеличивает машину, второй — число машин. Второй требует распределения запросов и состояния.

</details>

**2. Почему gRPC плохо балансируется через L4?**

<details><summary>Ответ</summary>

Запросы мультиплексируются в долгоживущем HTTP/2-соединении, а L4 выбирает узел один раз на соединение.

</details>

**3. Зачем виртуальные узлы в consistent hashing?**

<details><summary>Ответ</summary>

Чтобы выровнять доли и чтобы нагрузка выбывшего узла распределилась по многим, а не досталась одному соседу.

</details>

**4. Сколько ключей переезжает при добавлении пятого узла?**

<details><summary>Ответ</summary>

С кольцом — в среднем около 1/5, с `hash % N` — около 4/5.

</details>

**5. Чем power of two choices лучше случайного выбора?**

<details><summary>Ответ</summary>

Резко уменьшает разброс нагрузки, оставаясь O(1) и не требуя точной глобальной картины.

</details>

**6. Что случится, если readiness проверяет общую базу?**

<details><summary>Ответ</summary>

При сбое базы все экземпляры станут неготовыми одновременно, и пропадут даже операции, не требующие базы.

</details>

**7. Почему удвоение узлов не удваивает пропускную способность?**

<details><summary>Ответ</summary>

Последовательные участки (Амдал) и расходы на согласование (USL).

</details>

**8. Когда допустимы sticky sessions?**

<details><summary>Ответ</summary>

Как временная мера или оптимизация локальности, если потеря привязки не ломает корректность.

</details>

**9. Что такое горячий ключ?**

<details><summary>Ответ</summary>

Ключ с непропорциональной долей запросов. Consistent hashing не помогает: ключ целиком живёт на одном узле. Помогают локальный кэш, копии ключа или разбиение.

</details>

**10. Подвох: у балансировщика пять backend-ов. Система отказоустойчива?**

<details><summary>Ответ</summary>

Неизвестно: балансировщик, база, регион или конфигурация могут быть единой точкой отказа.

</details>

**11. Чем rendezvous hashing отличается от кольца?**

<details><summary>Ответ</summary>

Не хранит структуру: для каждого ключа сравнивает все узлы. Проще, но O(n).

</details>

**12. Что оптимизирует Maglev по сравнению с кольцом?**

<details><summary>Ответ</summary>

Равномерность и скорость поиска ценой небольшого числа лишних переездов.

</details>

**13. Что делает приложение при остановке, чтобы не терять запросы?**

<details><summary>Ответ</summary>

Перестаёт получать новые, дожидается активных в пределах таймаута, затем освобождает ресурсы.

</details>

**14. Почему HPA уменьшает число pod-ов не сразу?**

<details><summary>Ответ</summary>

Окно стабилизации (300 секунд по умолчанию) защищает от колебаний.

</details>

**15. Подвох: включили `round_robin` в gRPC, но весь трафик на одном сервере. Почему?**

<details><summary>Ответ</summary>

Возможно, resolver возвращает один адрес (например, виртуальный адрес сервиса), а не адреса экземпляров.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [YARP: load balancing](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/load-balancing)
- [YARP: health checks](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/dests-health-checks)
- [Kubernetes: Horizontal Pod Autoscaling](https://kubernetes.io/docs/concepts/workloads/autoscaling/horizontal-pod-autoscale/)
- [Maglev: A Fast and Reliable Software Network Load Balancer](https://research.google/pubs/maglev-a-fast-and-reliable-software-network-load-balancer/)
- [.NET Generic Host: остановка](https://learn.microsoft.com/dotnet/core/extensions/generic-host)
- [gRPC: балансировка на клиенте](https://learn.microsoft.com/aspnet/core/grpc/loadbalancing)
- [Kubernetes: жизненный цикл pod](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/)
