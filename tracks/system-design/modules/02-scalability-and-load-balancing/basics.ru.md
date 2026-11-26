---
title: "Масштабирование и балансировка: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Масштабирование и балансировка: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** HTTP, ASP.NET Core на уровне Minimal API · **Время:** ~2 ч

## Зачем это нужно

Один сервер — это потолок по нагрузке и единая точка отказа. Несколько копий решают обе проблемы, если между ними умеют распределять запросы.

## Вертикальное и горизонтальное масштабирование

| | Вертикальное (scale up) | Горизонтальное (scale out) |
|---|---|---|
| Что делаем | больше CPU и памяти | больше экземпляров |
| Изменения в коде | обычно не нужны | нужен stateless-сервис |
| Потолок | самая большая машина | координация и общие ресурсы |
| Отказ машины | простой | остальные работают |

## Stateless-сервис

Экземпляр не должен хранить то, что понадобится следующему запросу: сессии, загруженные файлы, счётчики. Это переносят в общее хранилище (база, распределённый кэш, объектное хранилище). Тогда любой запрос может обработать любой экземпляр.

## Балансировщик L4 и L7

| | L4 | L7 |
|---|---|---|
| Видит | IP и порты, TCP/UDP | HTTP: путь, заголовки, cookie |
| Решение принимается | на соединение | на запрос |
| Возможности | быстро и просто | маршрутизация, TLS termination, повторы, преобразования |

С HTTP/2 и gRPC это различие важно: множество запросов идёт по одному соединению, и балансировщик L4 отправит их все на один экземпляр.

## Round robin

Запросы раздаются по кругу. Счётчик общий для всех потоков, поэтому он увеличивается атомарно.

```csharp
public string Pick()
{
    // The uint cast keeps the index non-negative after the counter overflows.
    uint i = unchecked((uint)Interlocked.Increment(ref _next));
    return nodes[(int)(i % (uint)nodes.Count)];
}
```

Файл: [Ex01_Balancers.cs](code/Examples/Ex01_Balancers.cs).

## Балансировщик на YARP

YARP (Yet Another Reverse Proxy) — библиотека reverse proxy для ASP.NET Core. Конфигурация состоит из маршрутов и кластеров; кластер — это набор адресов назначения.

```csharp
var cluster = new ClusterConfig
{
    ClusterId = "api",
    LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
    Destinations = backendAddresses
        .Index()
        .ToDictionary(d => $"d{d.Index}", d => new DestinationConfig { Address = d.Item }),
};
builder.Services.AddReverseProxy().LoadFromMemory([route], [cluster]);

var app = builder.Build();
app.MapReverseProxy();
```

Пример [Ex04_YarpGateway.cs](code/Examples/Ex04_YarpGateway.cs) поднимает два backend-а и шлюз на свободных портах `127.0.0.1`; тест делает четыре запроса и получает по два ответа от каждого. Обычно ту же конфигурацию задают в `appsettings.json` и подключают через `LoadFromConfig`.

## Ошибки новичка

- ❌ Хранить сессию в памяти при нескольких экземплярах → ✅ общее хранилище.
- ❌ Один балансировщик без резерва → ✅ он тоже должен быть избыточным.
- ❌ Масштабировать приложение, когда не справляется база → ✅ сначала найти узкое место.
- ❌ Считать, что порядок запросов сохранится → ✅ не зависеть от порядка между экземплярами.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Kubernetes: Services](../../../kubernetes/modules/06-services/overview.ru.md)

## Источники

- [YARP: load balancing](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/load-balancing)
- [YARP: обзор](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/yarp-overview)
