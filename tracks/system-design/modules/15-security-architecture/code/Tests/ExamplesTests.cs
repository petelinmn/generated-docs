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

public class PkceTests
{
    [Fact]
    public void Matches_the_example_of_rfc_7636() =>
        Assert.Equal("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM",
            Pkce.Challenge("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"));

    [Fact]
    public void Verifiers_are_long_enough_and_different()
    {
        string a = Pkce.CreateVerifier(), b = Pkce.CreateVerifier();

        Assert.Equal(43, a.Length);
        Assert.NotEqual(a, b);
    }
}

public class EnvelopeTests
{
    [Fact]
    public void Rotation_rewraps_the_key_and_leaves_the_data()
    {
        var ring = new KeyRing();
        Envelope old = ring.Encrypt("secret"u8.ToArray());
        ring.Rotate();
        Envelope rewrapped = ring.Rewrap(old);

        Assert.Equal((1, 2), (old.KeyVersion, rewrapped.KeyVersion));
        Assert.Same(old.Data, rewrapped.Data);
        Assert.NotEqual(old.WrappedKey, rewrapped.WrappedKey);
        Assert.Equal("secret"u8.ToArray(), ring.Decrypt(rewrapped));
        Assert.Equal("secret"u8.ToArray(), ring.Decrypt(old));
    }
}

public class TenantTests
{
    [Fact]
    public void A_tenant_cannot_read_or_write_another_tenants_rows()
    {
        var table = new Dictionary<(string, int), Invoice>();
        new TenantInvoices(table, "acme").Save(new("acme", 1, 10));
        var other = new TenantInvoices(table, "globex");

        Assert.Null(other.Find(1));
        Assert.Throws<UnauthorizedAccessException>(() => other.Save(new("acme", 1, 0)));
    }
}

public class CertificateTests
{
    [Fact]
    public void During_rotation_both_roots_are_trusted()
    {
        var now = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        using var oldRoot = Certificates.CreateRoot("old", now);
        using var newRoot = Certificates.CreateRoot("new", now);
        using var oldClient = Certificates.Issue(oldRoot, "client", now);
        using var newClient = Certificates.Issue(newRoot, "client", now);

        Assert.False(Certificates.IsTrusted(newClient, now, oldRoot));
        Assert.True(Certificates.IsTrusted(oldClient, now, oldRoot, newRoot));
        Assert.True(Certificates.IsTrusted(newClient, now, oldRoot, newRoot));
        Assert.False(Certificates.IsTrusted(oldClient, now, newRoot));
        Assert.False(Certificates.IsTrusted(oldClient, now.AddDays(31), oldRoot)); // expired
    }
}
