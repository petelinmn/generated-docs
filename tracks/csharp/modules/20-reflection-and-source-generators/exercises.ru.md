---
title: "Упражнения: Рефлексия и генераторы кода"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, attributes, expression-trees, plugins, source-generators]
status: draft
updated: 2026-11-22
---

# Упражнения: Рефлексия и генераторы кода

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения проверены запуском (Release). Для упражнения 5 нужны два проекта: генератор (`netstandard2.0`, пакет `Microsoft.CodeAnalysis.CSharp` 4.14.0) и приложение. Запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Свойства типа

**Задача:** выведи имена и типы публичных свойств класса `Person(string name, int age)`.

**Ожидаемый результат:**

```text
Name:String
Age:Int32
```

<details><summary>Подсказка</summary>

`typeof(Person).GetProperties()` и `p.Name`, `p.PropertyType.Name`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: list the public properties of a type.
foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine($"{p.Name}:{p.PropertyType.Name}");
```

Проверено: вывод совпадает.

</details>

## 2. 🟢 Подписи из атрибутов

**Задача:** объяви атрибут `[Label("текст")]` для свойств; для каждого свойства `Person` выведи подпись, а если её нет — имя свойства.

**Ожидаемый результат:**

```text
Full name
Age
```

<details><summary>Подсказка</summary>

`[AttributeUsage(AttributeTargets.Property)]`, `p.GetCustomAttribute<LabelAttribute>()?.Text ?? p.Name`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: read a custom attribute with a fallback.
[AttributeUsage(AttributeTargets.Property)]
public class LabelAttribute(string text) : Attribute { public string Text { get; } = text; }

public class Person(string name, int age)
{
    [Label("Full name")] public string Name { get; set; } = name;
    public int Age { get; set; } = age;
}

foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine(p.GetCustomAttribute<LabelAttribute>()?.Text ?? p.Name);
```

Проверено: `Full name`, `Age`.

</details>

## 3. 🟡 Копирование свойств: рефлексия и скомпилированные выражения

**Задача:** напиши `Mapper.Copy<TFrom, TTo>(from, to)`, копирующий одноимённые записываемые свойства одного типа; затем `CopyFast` с кэшем делегата, построенного из дерева выражений. Сравни выделения.

**Ожидаемый результат:**

```text
Name и Age скопированы, Extra остаётся null
рефлексивный вариант: 64 B; скомпилированный: 0 B
```

<details><summary>Подсказка</summary>

Для скомпилированного варианта нужен статический класс `Cache<TFrom, TTo>` с полем `Action<TFrom, TTo>`, построенным через `Expression.Assign(Expression.Property(t, tp), Expression.Property(f, sp))` и `Expression.Block`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: reflection copy versus a compiled, cached copier.
public static class Mapper
{
    public static void Copy<TFrom, TTo>(TFrom from, TTo to)
    {
        foreach (var sp in typeof(TFrom).GetProperties())
        {
            var tp = typeof(TTo).GetProperty(sp.Name);
            if (tp is { CanWrite: true } && tp.PropertyType == sp.PropertyType) tp.SetValue(to, sp.GetValue(from));
        }
    }

    private static class Cache<TFrom, TTo>
    {
        public static readonly Action<TFrom, TTo> Copier = Build();
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

Проверено: оба метода скопировали `Name` и `Age`; выделения — `64 B` и `0 B`. В Native AOT `Compile()` недоступен — там нужен генератор.

</details>

## 4. 🟡 Загрузка плагинов

**Задача:** напиши `PluginLoader.Load<T>(Assembly)`: находит неабстрактные классы с публичным конструктором без параметров, реализующие `T`, и создаёт их, упорядочив по имени типа.

**Ожидаемый результат:** для `Alpha`, `Beta`, абстрактного `Gamma` — `alpha`, `beta`.

<details><summary>Подсказка</summary>

`asm.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false } && t.GetConstructor(Type.EmptyTypes) is not null)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: discover and instantiate implementations.
public static class PluginLoader
{
    public static IEnumerable<T> Load<T>(Assembly asm) =>
        asm.GetTypes()
           .Where(t => typeof(T).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false } && t.GetConstructor(Type.EmptyTypes) is not null)
           .OrderBy(t => t.Name)
           .Select(t => (T)Activator.CreateInstance(t)!);
}
```

Проверено: `alpha`, `beta`. Для типов с зависимостями создавайте через DI, а не `Activator`.

</details>

## 5. 🔴 Свой source generator

**Задача:** напиши генератор `[AutoToString]`: для `partial`-класса с этим атрибутом он создаёт `ToString()`, выводящий имя класса и значения публичных свойств, например `Person { Name = Ann, Age = 30 }`. Атрибут тоже должен генерироваться.

**Ожидаемый результат:**

```text
Person { Name = Ann, Age = 30 }
Point { X = 1, Y = 2 }     (класс в пространстве имён Shapes)
```

<details><summary>Подсказка</summary>

`IIncrementalGenerator`; `RegisterPostInitializationOutput` для атрибута; `ForAttributeWithMetadataName("Gen.AutoToStringAttribute", …)`; в сгенерированной интерполированной строке скобки удваивай (`{{`/`}}`). Проект генератора — `netstandard2.0`, в приложении — `<ProjectReference … OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: an incremental generator that adds ToString to marked partial classes.
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
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("AutoToStringAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

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

Проверено: приложение с `[AutoToString] public partial class Person` печатает `Person { Name = Ann, Age = 30 }`, а `Shapes.Point` — `Point { X = 1, Y = 2 }`. Закрытое свойство в вывод не попало.

</details>
