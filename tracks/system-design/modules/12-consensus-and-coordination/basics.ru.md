---
title: "Консенсус и координация: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 12-consensus-and-coordination
part: 3
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Консенсус и координация: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** репликация, потоки · **Время:** ~2 ч

## Зачем это нужно

Если два узла считают себя лидером, данные расходятся. Если задачу по расписанию выполняют все экземпляры, клиент получает три письма.

## Большинство

Решение принимается, когда за него большинство всего кластера. Два большинства всегда пересекаются, поэтому два лидера в одном терме невозможны.

| Узлов | Большинство | Переживает отказов |
|---|---|---|
| 3 | 2 | 1 |
| 4 | 3 | 1 |
| 5 | 3 | 2 |

## Выбор лидера в Raft

Узел бывает follower, candidate или leader. Если follower долго не получает heartbeat, он увеличивает терм, голосует за себя и просит голоса.

```csharp
public bool StartElection(IEnumerable<Node> reachable, int clusterSize)
{
    (Term, _votedFor) = (Term + 1, Id);
    int votes = 1 + reachable.Count(n => n.RequestVote(Term, Id, LastLogTerm, LastLogIndex));
    return votes > clusterSize / 2;
}
```

Кандидат с двумя голосами из пяти проигрывает, с тремя побеждает. Файл: [Ex01_Election.cs](code/Examples/Ex01_Election.cs). Это только правило голосования, не полный Raft.

## Lease и fencing token

Блокировку с ограниченным сроком (lease) получает один клиент. Но клиент может «уснуть» (пауза GC, сеть), срок истечёт, другой получит lease, а первый проснётся и запишет. Проверка срока перед записью не спасает: пауза может случиться после неё.

```csharp
public bool TryWrite(long token, string value)
{
    if (token < _highestToken) return false; // a holder from the past

    _highestToken = token;
    Value = value;
    return true;
}
```

Каждая выдача lease получает больший номер, и хранилище отвергает меньший. Файл: [Ex02_Fencing.cs](code/Examples/Ex02_Fencing.cs).

## Ошибки новичка

- ❌ `lock` для защиты между серверами → ✅ он действует в одном процессе.
- ❌ Чётное число узлов ради надёжности → ✅ оно не добавляет устойчивости.
- ❌ Lease без fencing → ✅ проверка на стороне ресурса.
- ❌ Свой протокол выбора лидера → ✅ готовая система.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Согласованность и CAP](../07-consistency-and-cap/overview.ru.md)

## Источники

- [Raft: In Search of an Understandable Consensus Algorithm](https://raft.github.io/raft.pdf)
- [Kleppmann: How to do distributed locking](https://martin.kleppmann.com/2016/02/08/how-to-do-distributed-locking.html)
