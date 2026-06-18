using Microsoft.AspNetCore.DataProtection;

namespace Blog.Web.Services;

public interface IFtpCredentialProtector
{
    string Protect(string plainText);
    string Unprotect(string protectedText);
}

public class FtpCredentialProtector : IFtpCredentialProtector
{
    private readonly IDataProtector _protector;

    public FtpCredentialProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Blog.Web.FtpCredentials.v1");
    }

    public string Protect(string plainText) => _protector.Protect(plainText);

    public string Unprotect(string protectedText) => _protector.Unprotect(protectedText);
}
