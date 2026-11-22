---
title: "Рефлексия и генераторы кода: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, expression-trees, source-generators, incremental-generator, delegate-caching, system-text-json, generated-regex]
versions: ".NET 11, C# 15; Microsoft.CodeAnalysis.CSharp 4.14.0"
status: draft
updated: 2026-11-22
---

# Рефлексия и генераторы кода: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Стоимость рефлексии и ускорение

Динамические вызовы медленнее прямых и могут выделять память. Измерение выделений за вызов (Release):

| Операция | Выделено |
|---|---|
| прямое чтение свойства `person.Name` | 0 Б |
| `PropertyInfo.GetValue(person)` (строка) | 0 Б |
| `MethodInfo.Invoke(person, ["x"])` | 72 Б |
| делегат, созданный один раз через `Delegate.CreateDelegate` | 0 Б |

`Invoke` выделяет массив аргументов и упаковывает значимые аргументы; `GetValue` для значимых типов упаковывает результат. Время поиска (`GetProperty`, `GetMethod`) тоже заметно, поэтому **объекты рефлексии кэшируют**, а горячие вызовы превращают в делегаты:

```csharp
var greet = typeof(Person).GetMethod("Greet")!;
var del = (Func<Person, string, string>)Delegate.CreateDelegate(typeof(Func<Person, string, string>), greet);
Console.WriteLine(del(new Person("Dan", 5), "Yo"));   // Yo, Dan — a typed call without a per-call Invoke cost
```

## Компиляция выражений для быстрого доступа

Если доступ нужно строить динамически, дерево выражений компилируется в делегат один раз, и дальше вызов почти как прямой. Пример — копирование одноимённых свойств между объектами:

```csharp
public static class Mapper
{
    public static void Copy<TFrom, TTo>(TFrom from, TTo to)          // reflection on every call
    {
        foreach (var sp in typeof(TFrom).GetProperties())
        {
            var tp = typeof(TTo).GetProperty(sp.Name);
            if (tp is { CanWrite: true } && tp.PropertyType == sp.PropertyType) tp.SetValue(to, sp.GetValue(from));
        }
    }

    private static class Cache<TFrom, TTo>
    {
        public static readonly Action<TFrom, TTo> Copier = Build();   // built once per pair of types
        private static Action<TFrom, TTo> Build()
        {
            var f = Expression.Parameter(typeof(TFrom), "f"); var t = Expression.Parameter(typeof(TTo), "t");
            var assigns = new List<Expression>();
            foreach (var sp in typeof(TFrom).GetProperties())
            {
                var tp = typeof(TTo).GetProperty(sp.Name);
                if (tp is { CanWrite: true } && tp.PropertyType == sp.PropertyType)
                    assigns.Add(Expression.Assign(Expression.Property(t, tp), Expression.Property(f, sp)));
            }
            return Expression.Lambda<Action<TFrom, TTo>>(Expression.Block(assigns.Count == 0 ? [Expression.Empty()] : assigns), f, t).Compile();
        }
    }
    public static void CopyFast<TFrom, TTo>(TFrom from, TTo to) => Cache<TFrom, TTo>.Copier(from, to);
}
```

Проверено: оба варианта скопировали `Name` и `Age` (`Extra` остался `null`); выделения на вызов — `64 Б` у рефлексивного и `0 Б` у скомпилированного. Статический класс `Cache<TFrom, TTo>` хранит делегат для каждой пары типов: приём «статический кэш в обобщённом классе» ([06 · Коллекции и обобщения](../06-collections-and-generics/expert.ru.md#статические-поля-обобщённого-типа)).

Ограничение: `Compile()` генерирует код во время выполнения — в Native AOT это недоступно ([19 · Производительность](../19-performance/expert.ru.md#native-aot)); там нужны генераторы.

## Обнаружение плагинов

```csharp
public static class PluginLoader
{
    public static IEnumerable<T> Load<T>(Assembly asm) =>
        asm.GetTypes()
           .Where(t => typeof(T).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false } && t.GetConstructor(Type.EmptyTypes) is not null)
           .OrderBy(t => t.Name)
           .Select(t => (T)Activator.CreateInstance(t)!);
}
```

Проверено: для `IPlugin` загружены `alpha`, `beta`; абстрактный класс и типы без публичного конструктора без параметров пропущены. В реальных приложениях плагины получают через DI ([13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md)) и загружают сборки через `AssemblyLoadContext`.

## Генераторы исходного кода

Генератор реализует `IIncrementalGenerator` и живёт в проекте с целью `netstandard2.0`, подключаемом к потребителю как **анализатор**. Он выполняется внутри компилятора, читает синтаксис и семантику и добавляет файлы. Минимальный проект генератора:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IsRoslynComponent>true</IsRoslynComponent>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

Генератор, который для класса с атрибутом `[AutoToString]` создаёт `ToString`:

```csharp
[Generator]
public sealed class ToStringGenerator : IIncrementalGenerator
{
    private const string AttributeSource = """
        namespace Gen
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            internal sealed class AutoToStringAttribute : System.Attribute { }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1) emit the marker attribute itself
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("AutoToStringAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

        // 2) find classes marked with it
        var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Gen.AutoToStringAttribute",
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) =>
            {
                var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                var props = symbol.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && p.GetMethod is not null)
                    .Select(p => p.Name).ToImmutableArray();
                return (Ns: symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
                        Name: symbol.Name, Props: props);
            });

        // 3) generate a partial class with ToString
        context.RegisterSourceOutput(classes, static (spc, c) =>
        {
            var sb = new StringBuilder();
            if (c.Ns is not null) sb.AppendLine($"namespace {c.Ns} {{");
            sb.AppendLine($"partial class {c.Name}");
            sb.AppendLine("{");
            sb.AppendLine("    public override string ToString() => $\"" + c.Name + " {{ " + string.Join(", ", c.Props.Select(p => $"{p} = {{{p}}}")) + " }}\";");
            sb.AppendLine("}");
            if (c.Ns is not null) sb.AppendLine("}");
            spc.AddSource($"{c.Name}.ToString.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }
}
```

Потребитель подключает генератор ссылкой с особыми атрибутами:

```xml
<ProjectReference Include="..\Gen\Gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

```csharp
using Gen;

[AutoToString]
public partial class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    private int Secret { get; set; }       // private: not included
}

var p = new Person { Name = "Ann", Age = 30 };
Console.WriteLine(p);                       // Person { Name = Ann, Age = 30 }
```

Проверено запуском: вывод `Person { Name = Ann, Age = 30 }`; для класса в пространстве имён (`Shapes.Point`) — `Point { X = 1, Y = 2 }`. Сгенерированный файл при включённом `EmitCompilerGeneratedFiles` лежит в `Generated/…/Person.ToString.g.cs` и содержит:

```csharp
partial class Person
{
    public override string ToString() => $"Person {{ Name = {Name}, Age = {Age} }}";
}
```

Тонкости: класс должен быть `partial`; шаблон `{{ }}` экранирует фигурные скобки в генерируемой интерполированной строке (без этого генерируемый код не компилируется — так было при первой попытке). **Инкрементальность**: `ForAttributeWithMetadataName` и промежуточные модели (кортежи значений, `ImmutableArray`) позволяют компилятору кэшировать этапы, поэтому генератор не пересчитывается при каждом нажатии клавиши в IDE; не храните в модели `ISymbol`/`SyntaxNode`.

## Готовые генераторы платформы

| Генератор | Что даёт |
|---|---|
| `[LoggerMessage]` | быстрые методы логирования ([13 · DI](../13-di-config-logging/advanced.ru.md#loggermessage-быстрые-сообщения)) |
| `[GeneratedRegex]` | регулярные выражения, скомпилированные заранее |
| `JsonSerializerContext` + `[JsonSerializable]` | сериализация JSON без рефлексии, совместимая с AOT |
| `[LibraryImport]` | вызовы нативного кода |
| Request Delegate Generator | обработчики Minimal API для AOT ([14](../14-aspnet-core-minimal-api/expert.ru.md)) |

Проверено на примере JSON и Regex: `[JsonSerializable(typeof(Item))] partial class ItemContext : JsonSerializerContext` сериализует запись `Item("pen", 3)` в `{"name":"pen","qty":3}` (с `CamelCase`), а `[GeneratedRegex(@"\d+")] static partial Regex Digits();` находит `123` в `abc 123 def`. Сгенерированные файлы — `ItemContext.g.cs`, `ItemContext.Item.g.cs`, `RegexGenerator.g.cs` и другие.

## Отладка и тестирование генераторов

- Включите `EmitCompilerGeneratedFiles` и `CompilerGeneratedFilesOutputPath`, чтобы увидеть результат на диске.
- Ошибки генерации проявляются как ошибки компиляции в сгенерированном файле (как `CS1073` при неэкранированных скобках).
- Генераторы тестируют, запуская компилятор на строке исходника (`CSharpGeneratorDriver`) и сравнивая вывод со снимком.
- Для предупреждений и ошибок пользователю генератор сообщает `Diagnostic` через `ReportDiagnostic`.

## Лучшие практики

| Правило | Почему |
|---|---|
| Кэшируйте `MethodInfo`/делегаты | поиск и `Invoke` дороги |
| Перенесите повторяемую рефлексию в генератор | ноль затрат при выполнении, AOT |
| Только инкрементальные генераторы (`IIncrementalGenerator`) | быстрый IDE, кэширование |
| Не храните символы/синтаксис в моделях генератора | ломает кэширование |
| `nameof` вместо строк | безопасное переименование |
| Рефлексия вне горячих путей и вне доверенного кода — осторожно | обход инкапсуляции, безопасность |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — устройство метаданных, ограничения, подвохи.
- [19 · Производительность](../19-performance/overview.ru.md) — стоимость и AOT.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — AOT и генераторы.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — `[LoggerMessage]`.
- [11 · Современный C#](../11-modern-csharp/overview.ru.md) — `partial`-члены.

## Источники

- [Генераторы исходного кода: инкрементальные](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [Создание генераторов](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Генерация источников `System.Text.Json`](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation)
- [`GeneratedRegexAttribute`](https://learn.microsoft.com/dotnet/standard/base-types/regular-expression-source-generators)
- [Деревья выражений: `Expression.Compile`](https://learn.microsoft.com/dotnet/api/system.linq.expressions.expression-1.compile)
