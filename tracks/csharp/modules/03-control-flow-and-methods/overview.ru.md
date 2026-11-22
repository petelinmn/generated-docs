---
title: "Управляющие конструкции и методы"
lang: ru
track: csharp
kind: module
doc: overview
slug: 03-control-flow-and-methods
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [control-flow, switch-expression, pattern-matching, loops, methods, ref-out-in, overloading, local-functions, closures]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Управляющие конструкции и методы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Управляющие конструкции решают, какой код выполнить и сколько раз: ветвления `if` и `switch`, циклы `for`, `while`, `do`, `foreach`. Методы упаковывают код в именованные блоки с параметрами. В современном C# к классике добавились switch expression с шаблонами, модификаторы `ref`/`out`/`in` и локальные функции.

## Ключевые идеи

- **`switch` без проваливания:** каждая секция заканчивается `break`, `return`, `throw` или `goto`.
- **Switch expression** (`x switch { … }`) возвращает значение. Ветки проверяются по порядку, есть реляционные (`< 0`) и логические (`and`, `or`) шаблоны.
- **`foreach`** — основной способ обойти коллекцию. Менять коллекцию во время обхода нельзя. С C# 15 `break` и `continue` могут указывать метку внешнего цикла.
- **Лямбда захватывает переменную, а не значение:** у `foreach` переменная своя на каждой итерации, у `for` — одна на весь цикл.
- **`ref`, `out`, `in`** передают аргумент по ссылке. `out` лежит в основе паттерна `TryXxx`, `in` защищает большие структуры от копирования.
- **Перегрузка** выбирается при компиляции по «лучшему» преобразованию типов.
- **Локальные функции** — вспомогательные методы внутри метода. `static` запрещает им захватывать переменные.

## Минимальный пример

```csharp
// A switch expression over a tuple (C# 8+): the first matching arm wins.
public static string FizzBuzz(int n) => (n % 3, n % 5) switch
{
    (0, 0) => "FizzBuzz",
    (0, _) => "Fizz",
    (_, 0) => "Buzz",
    _ => n.ToString(CultureInfo.InvariantCulture),
};
```

`FizzBuzz(15)` → `FizzBuzz`, `FizzBuzz(9)` → `Fizz`, `FizzBuzz(7)` → `7`.

## Когда применять, а когда нет

- ✅ Switch expression — когда по значению нужно выбрать результат.
- ✅ `foreach` — для обхода; `for` — когда нужен индекс или обход с конца.
- ✅ Паттерн Try с `out` — когда неудача является нормальным исходом (разбор ввода).
- ✅ `static` локальные функции — для помощников, которым не нужен контекст метода.
- ❌ `ref` ради «скорости» для маленьких типов: копирование `int` дешевле косвенного доступа.
- ❌ Изменение коллекции внутри `foreach` по ней.
- ❌ Подавление предупреждения о неисчерпывающем `switch` вместо явной ветки `_`.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `if`, `switch`, switch expression, четыре вида циклов, метки для `break`/`continue` (C# 15), методы, необязательные и именованные аргументы, `params` |
| [2. Продвинутая теория](advanced.ru.md) | `ref`, `out`, `in`, `params ReadOnlySpan<T>`, выбор перегрузки, локальные функции, ловушки циклов и замыканий |
| [3. Экспертная теория](expert.ru.md) | `SwitchExpressionException`, ref-возвраты, `foreach` по шаблону, рекурсия и стек, значения по умолчанию в месте вызова; справочник по теме, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| [5. Запускаемые примеры](code/) | `code/Examples/` и `code/Solutions/` — решение `Code.slnx` |
| [6. Тесты](code/) | `code/Tests/` — `dotnet test` в папке `code/` |


## Связанные материалы

- Пререквизиты: [02 · Типы и переменные](../02-types-and-variables/overview.ru.md)
- Раньше: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/overview.ru.md)
- Образцы и `switch`-выражения: [11 · Современный C#](../11-modern-csharp/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `if`/`match`, `for … in`, `*args`/`**kwargs`
- Рецепты и библиотеки: [Spectre.Console](../../libraries/spectre-console/overview.ru.md)
