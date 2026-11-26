// Example: object level authorization: check the object, not only the login.
// Module: system-design/15-security-architecture (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Authorization"
namespace SecurityDesign.Examples;

public sealed record User(string Id, string Tenant, bool IsAdmin = false);
public sealed record Document(int Id, string Tenant, string OwnerId);

public static class DocumentAccess
{
    // Deny by default: access exists only where a rule grants it.
    public static bool CanRead(User user, Document document) =>
        user.Tenant == document.Tenant && (user.Id == document.OwnerId || user.IsAdmin);

    // "Not found" for a forbidden object does not reveal that it exists.
    public static Document? Find(User user, int id, IReadOnlyDictionary<int, Document> documents) =>
        documents.TryGetValue(id, out Document? d) && CanRead(user, d) ? d : null;
}
