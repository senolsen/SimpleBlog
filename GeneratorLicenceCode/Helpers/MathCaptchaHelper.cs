namespace GeneratorLicenceCode.Helpers;

public static class MathCaptchaHelper
{
    private const string SessionKey = "MathCaptchaAnswer";

    public static string GenerateQuestion(HttpContext context)
    {
        var a = Random.Shared.Next(1, 10);
        var b = Random.Shared.Next(1, 10);
        context.Session.SetInt32(SessionKey, a + b);
        return $"{a} + {b}";
    }

    public static bool Validate(HttpContext context, int? answer)
    {
        var expected = context.Session.GetInt32(SessionKey);
        context.Session.Remove(SessionKey);
        return expected.HasValue && answer.HasValue && expected.Value == answer.Value;
    }
}
