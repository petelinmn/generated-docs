---
title: "Квиз: ООП: классы и объекты"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 04-oop-basics
part: 1
tags: [class, constructor, property, inheritance, polymorphism, virtual-override, access-modifiers]
status: verified
updated: 2026-11-27
---

# Квиз: ООП: классы и объекты

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой доступ по умолчанию у поля класса и у класса верхнего уровня без модификатора?

- A) `public` и `public`
- B) `private` и `internal`
- C) `private` и `private`
- D) `internal` и `public`

<details><summary>Ответ</summary>

**B.** Член класса без модификатора — `private`, тип верхнего уровня — `internal` (проверено через рефлексию). Принцип инкапсуляции: открывать только то, что нужно снаружи.
→ [Базовая теория](basics.ru.md#модификаторы-доступа)

</details>

## 2. 🟢 Что выведет код?

```csharp
Animal animal = new Dog();
Console.WriteLine(animal.Speak());

public class Animal { public virtual string Speak() => "..."; }
public class Dog : Animal { public override string Speak() => "Woof"; }
```

- A) `...`
- B) `Woof`
- C) ошибка компиляции
- D) `Dog`

<details><summary>Ответ</summary>

**B.** Для `virtual`/`override` вызов определяет **фактический тип** объекта (`Dog`), а не тип переменной (`Animal`). Это и есть полиморфизм.
→ [Базовая теория](basics.ru.md#полиморфизм)

</details>

## 3. 🟢 Что даст сравнение двух разных объектов класса без переопределения `Equals`, если все их поля одинаковы?

- A) `true`: сравнивается содержимое
- B) `false`: по умолчанию сравниваются ссылки
- C) исключение
- D) зависит от числа полей

<details><summary>Ответ</summary>

**B.** `Equals` из `System.Object` сравнивает ссылки; для значимого равенства переопределяют `Equals`/`GetHashCode` (или используют запись, см. [05](../05-interfaces-structs-records/overview.ru.md)). Проверено запуском.
→ [Базовая теория](basics.ru.md#наследование)

</details>

## 4. 🟢 Выберите все верные утверждения

- [ ] A) класс может наследоваться от двух базовых классов
- [ ] B) `init`-свойство задаётся в конструкторе или инициализаторе объекта
- [ ] C) `required` заставляет вызывающую сторону задать свойство
- [ ] D) если объявить свой конструктор, пустой без параметров всё равно создаётся компилятором

<details><summary>Ответ</summary>

**B, C.** A неверно: у класса один базовый класс (несколько интерфейсов — можно). D неверно: как только объявлен свой конструктор, пустой по умолчанию не создаётся.
→ [Базовая теория](basics.ru.md#конструкторы)

</details>

## 5. 🟡 В каком порядке выполнится код при `new Derived()`?

```csharp
class Base    { int f = Trace.Add("base field");    public Base()    { Trace.Add("base ctor"); } }
class Derived : Base { int g = Trace.Add("derived field"); public Derived() { Trace.Add("derived ctor"); } }
```

- A) `base field | base ctor | derived field | derived ctor`
- B) `derived field | base field | base ctor | derived ctor`
- C) `derived field | derived ctor | base field | base ctor`
- D) `base ctor | base field | derived ctor | derived field`

<details><summary>Ответ</summary>

**B.** Сначала инициализаторы полей наследника, затем поля базы, тело конструктора базы, тело конструктора наследника (проверено запуском). Это особенность C#: в Java или C++ порядок иной.
→ [Продвинутая теория](advanced.ru.md#порядок-инициализации-объекта)

</details>

## 6. 🟡 Что выведет код?

```csharp
Animal a = new Dog();
Console.WriteLine($"{a.Speak()} {a.Name()} {((Dog)a).Name()}");

class Animal { public virtual string Speak() => "..."; public string Name() => "Animal"; }
class Dog : Animal { public override string Speak() => "Woof"; public new string Name() => "Dog"; }
```

<details><summary>Ответ</summary>

```text
Woof Animal Dog
```

`override` решает по фактическому типу, а `new` только **скрывает** член: при вызове через переменную `Animal` выбирается `Name` базового класса. Приведение к `Dog` даёт версию наследника.
→ [Продвинутая теория](advanced.ru.md#virtual-override-и-new)

</details>

## 7. 🟡 Что вызовется?

```csharp
new Printer().Print(5);

class PrinterBase { public void Print(int x) => Console.WriteLine("int in base"); }
class Printer : PrinterBase { public void Print(double x) => Console.WriteLine("double in derived"); }
```

- A) `int in base`
- B) `double in derived`
- C) ошибка неоднозначности
- D) оба метода

<details><summary>Ответ</summary>

**B.** Если у наследника есть подходящий метод (`5` неявно приводится к `double`), методы базового класса при разрешении перегрузки не рассматриваются. Не смешивайте перегрузки в иерархиях.
→ [Продвинутая теория](advanced.ru.md#перегрузка-и-методы-наследника)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) `static class` в метаданных одновременно `abstract` и `sealed`
- [ ] B) `sealed override` запрещает дальнейшее переопределение метода
- [ ] C) экземпляр `abstract`-класса можно создать через `new`
- [ ] D) значение `const` подставляется в код вызывающей сборки при компиляции

<details><summary>Ответ</summary>

**A, B, D.** C неверно: абстрактный класс создать нельзя. D: поэтому после изменения константы в библиотеке потребителей надо пересобирать (в отличие от `static readonly`).
→ [Продвинутая теория](advanced.ru.md#абстрактные-классы-и-запечатывание)

</details>

## 9. 🟡 Открытый вопрос: когда лучше композиция, чем наследование?

<details><summary>Ключевые пункты хорошего ответа</summary>

- наследование — только при отношении «является» и когда подкласс можно подставить вместо базы без сюрпризов;
- композиция (поле другого типа) даёт гибкость: поведение меняется подстановкой объекта, нет жёсткой связи с реализацией базы;
- наследование реализации раскрывает детали базового класса (хрупкий базовый класс), а в C# доступен только один базовый класс;
- для общего контракта используют интерфейсы, для повторного использования кода — композицию.

</details>

## 10. 🔴 Что запишет `Seen` в этом коде и что изменится, если `string name;` заменить на `string name = "set";`?

```csharp
class A { public A() { Hook(); } protected virtual void Hook() { } }
class C : A
{
    string name;
    public string Seen = "-";
    public C() { name = "set"; }
    protected override void Hook() { Seen = name is null ? "null" : name; }
}
```

<details><summary>Ответ</summary>

`new C().Seen` — `"null"`: виртуальный метод вызван из конструктора базы, когда присваивание в теле конструктора `C` ещё не выполнено. С инициализатором `string name = "set";` результат станет `"set"`, потому что инициализаторы полей наследника выполняются раньше конструктора базы (проверено). Вывод: не вызывайте виртуальные методы из конструкторов.
→ [Экспертная теория](expert.ru.md#виртуальный-метод-в-конструкторе)

</details>
