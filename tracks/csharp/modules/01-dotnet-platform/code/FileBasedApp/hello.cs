// Example: a file-based app: a single .cs file without a .csproj (.NET 10 SDK or newer).
// Module: csharp/01-dotnet-platform (basics)
// Run: dotnet run hello.cs -- Alice
#:property AssemblyName=HelloFileApp

// Top-level statements: the compiler generates the Program class and Main for us.
var name = args.Length > 0 ? args[0] : "world";
var assemblyName = typeof(Program).Assembly.GetName().Name;
Console.WriteLine($"Hello, {name}! Running {assemblyName} on .NET {Environment.Version.Major}");
