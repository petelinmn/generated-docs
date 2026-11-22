---
title: "Mapperly: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: mapperly
part: 2
tags: [mapperly, enum-mapping, queryable-projection, nullable, ef-core]
versions: ".NET 11, C# 15; Riok.Mapperly 4.3.1"
status: draft
updated: 2026-11-22
---

# Mapperly: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Перечисления

Для enum Mapperly по умолчанию использует сопоставление **по значению**:

```csharp
public partial StatusText StatusToText(Status s);
// generated: return (StatusText)s;
```

Проверено: сгенерирован приведением `(StatusText)s`. Если порядок/числа значений в двух enum различаются, результат будет неверным без ошибки. Для сопоставления по имени:

```csharp
[MapEnum(EnumMappingStrategy.ByName)]
public partial StatusText StatusToText(Status s);
```

> ⚠️ Проверить: атрибут `[MapEnum(EnumMappingStrategy.ByName)]` и опции (`ByValueCheckDefined`, `IgnoreCase`) в этом материале не запускались — сверьтесь с документацией.

## Проекции для `IQueryable`

```csharp
public partial IQueryable<OrderDto> Project(IQueryable<Order> q);
```

Генератор строит выражение `Select` с инициализатором объекта — его транслирует провайдер (EF Core) в SQL, выбирая только нужные столбцы:

```csharp
q => q.Select(x => new OrderDto { Id = x.Id, CustomerName = x.Customer.Name, Status = x.Status, Total = x.Total })
```

Проверено: проекция работает (`Customer.Name` → `CustomerName`), а настройки вложенного пути взяты из существующего метода `ToDto`. **Подвох:** навешивание `[MapProperty]`/`[MapperIgnore…]` прямо на метод-проекцию даёт предупреждение `RMG065` («Cannot configure an object mapping on a queryable projection mapping, apply the configurations to an object mapping method instead») — настраивайте объектный метод, а проекция его использует.

> ⚠️ Проверить: трансляцию проекции в SQL через EF Core в этом материале не запускали (проверена только работа над `IQueryable` в памяти).

## Обратный маппинг и `null`

```csharp
public partial Order FromDto(OrderDto d);                 // reverse mapping needs its own method
public partial OrderDto? ToDtoOrNull(Order? o);           // nullable source -> null result
```

Проверено: для `null`-источника возвращается `null`. Обратное направление — отдельный метод со своими игнорированиями (`CustomerName` и др. помечены `MapperIgnoreSource`/`MapperIgnoreTarget`).

## Собственные преобразования

Метод в маппере с подходящей сигнатурой (из `A` в `B`) Mapperly использует автоматически для всех свойств этих типов. Так подключают нестандартные правила (форматы, значения-объекты).

> ⚠️ Проверить: пример собственного преобразования (`[UserMapping]`, `[MapProperty(Use = …)]`) в этом материале не запускали.

## Конфигурация маппера

`[Mapper]` принимает настройки: стратегия enum, `PropertyNameMappingStrategy` (регистр имён), `UseDeepCloning`, `RequiredMappingStrategy`. Уровень «серьёзности» диагностик можно повысить до ошибок в `.editorconfig` — например, превратить `RMG020` в ошибку сборки.

> ⚠️ Проверить: перечисленные опции атрибута `[Mapper]` и настройку серьёзности через `.editorconfig` в этом материале не запускали.

## Внедрение зависимостей

Маппер — обычный класс без состояния: регистрируйте как singleton (`AddSingleton<OrderMapper>()`) или создавайте статическим.

## Best practices

| Правило | Почему |
|---|---|
| Маппер без логики и побочных эффектов | предсказуемость |
| Предупреждения как ошибки | не забыть поля |
| Проекции для чтения из БД | выбираются только нужные столбцы |
| enum — явная стратегия | по значению молча ломается |
| Отдельные методы туда/обратно | разные правила и игнорирования |
| Папка сгенерированного кода — вне компиляции | нет CS0757 |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — трансляция проекций.
- [Пагинация и фильтрация](../../recipes/pagination-filtering/overview.ru.md) — проекции в API.
- [Dapper](../dapper/overview.ru.md) — результат запроса в объекты.

## Источники

- [Mapperly: Enum mapping](https://mapperly.riok.app/docs/configuration/enum/)
- [Mapperly: Queryable projections](https://mapperly.riok.app/docs/configuration/queryable-projections/)
- [Mapperly: User implemented mappings](https://mapperly.riok.app/docs/configuration/user-implemented-methods/)
