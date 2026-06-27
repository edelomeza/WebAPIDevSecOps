namespace UnitTest.Common;

public static class JwtTestConfig
{
    public const string Key = "01123581321345589144233377610987";
    public const string Issuer = "edelmeza.com";
    public const string Audience = "edelmeza.com";

    public static string AdminToken =>
        TokenHelper.GenerateValidToken(Key, Issuer, Audience);

    public static string UserToken =>
        TokenHelper.GenerateTokenWithRole(Key, Issuer, Audience, "User");
}
