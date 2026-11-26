using SecurityDesign.Examples;

public class PasswordTests
{
    [Fact]
    public void Same_password_gives_different_hashes_that_both_verify()
    {
        string a = Passwords.Hash("correct horse", iterations: 1_000); // low only to keep the test fast
        string b = Passwords.Hash("correct horse", iterations: 1_000);

        Assert.NotEqual(a, b);
        Assert.True(Passwords.Verify("correct horse", a));
        Assert.True(Passwords.Verify("correct horse", b));
        Assert.False(Passwords.Verify("wrong", a));
    }

    [Fact]
    public void Default_work_factor_is_stored() => Assert.StartsWith("600000.", Passwords.Hash("x"));
}

public class SignedRequestTests
{
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = DateTimeOffset.UnixEpoch.AddDays(1);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void Tampering_replay_and_old_requests_are_rejected()
    {
        var clock = new Clock();
        var signer = new RequestSigner([.. Enumerable.Range(0, 32).Select(i => (byte)i)], clock);
        SignedRequest request = signer.Sign("POST", "/pay", """{"amount":10}""");

        Assert.False(signer.Verify(request with { Body = """{"amount":1000}""" }));
        Assert.False(signer.Verify(request with { Path = "/refund" }));
        Assert.True(signer.Verify(request));
        Assert.False(signer.Verify(request)); // replay

        SignedRequest old = signer.Sign("POST", "/pay", "{}");
        clock.Now += TimeSpan.FromMinutes(6);
        Assert.False(signer.Verify(old));
    }
}

public class AuthorizationTests
{
    private static readonly Dictionary<int, Document> Documents = new() { [1] = new(1, "acme", "ann") };

    [Theory]
    [InlineData("ann", "acme", false, true)]
    [InlineData("bob", "acme", false, false)]
    [InlineData("bob", "acme", true, true)]
    [InlineData("eve", "other", true, false)]
    public void Only_the_owner_or_a_tenant_admin(string user, string tenant, bool admin, bool expected) =>
        Assert.Equal(expected, DocumentAccess.Find(new User(user, tenant, admin), 1, Documents) is not null);
}
