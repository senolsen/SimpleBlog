using Blog.Web.Models;
using FluentFTP;

namespace Blog.Web.Services;

public interface IFtpDeployService
{
    Task TestConnectionAsync(FtpConnectionInfo connection, CancellationToken cancellationToken = default);
    Task UploadDirectoryAsync(FtpConnectionInfo connection, string localDirectory, IEnumerable<string> relativeFiles, CancellationToken cancellationToken = default);
    Task<bool> TouchWebConfigAsync(FtpConnectionInfo connection, CancellationToken cancellationToken = default);
    Task<bool> TouchRestartFileAsync(FtpConnectionInfo connection, string relativePath, CancellationToken cancellationToken = default);
}

public class FtpDeployService : IFtpDeployService
{
    public async Task TestConnectionAsync(FtpConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        await using var client = CreateClient(connection);
        await client.Connect(cancellationToken);
        await client.Disconnect(cancellationToken);
    }

    public async Task UploadDirectoryAsync(
        FtpConnectionInfo connection,
        string localDirectory,
        IEnumerable<string> relativeFiles,
        CancellationToken cancellationToken = default)
    {
        await using var client = CreateClient(connection);
        await client.Connect(cancellationToken);

        var remoteRoot = NormalizeRemotePath(connection.RemotePath);

        foreach (var relativePath in relativeFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var localPath = Path.Combine(localDirectory, relativePath);
            var remotePath = CombineRemote(remoteRoot, relativePath.Replace('\\', '/'));

            var remoteDir = remotePath[..remotePath.LastIndexOf('/')];
            if (!string.IsNullOrEmpty(remoteDir))
                await client.CreateDirectory(remoteDir, true, cancellationToken);

            await client.UploadFile(localPath, remotePath, FtpRemoteExists.Overwrite, true, FtpVerify.None, null, cancellationToken);
        }

        await client.Disconnect(cancellationToken);
    }

    public async Task<bool> TouchWebConfigAsync(FtpConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        await using var client = CreateClient(connection);
        await client.Connect(cancellationToken);

        var remoteRoot = NormalizeRemotePath(connection.RemotePath);
        var remoteWebConfig = CombineRemote(remoteRoot, "web.config");

        if (!await client.FileExists(remoteWebConfig, cancellationToken))
        {
            await client.Disconnect(cancellationToken);
            return false;
        }

        var bytes = await client.DownloadBytes(remoteWebConfig, token: cancellationToken);
        var content = System.Text.Encoding.UTF8.GetString(bytes);
        var timestamp = DateTime.UtcNow.ToString("O");
        var comment = $"<!-- update-trigger:{timestamp} -->";

        if (content.Contains("<!-- update-trigger:", StringComparison.Ordinal))
        {
            var start = content.IndexOf("<!-- update-trigger:", StringComparison.Ordinal);
            var end = content.IndexOf("-->", start, StringComparison.Ordinal) + 3;
            content = content.Remove(start, end - start).Insert(start, comment);
        }
        else
        {
            content = content.TrimEnd() + Environment.NewLine + comment + Environment.NewLine;
        }

        var updatedBytes = System.Text.Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(updatedBytes);
        await client.UploadStream(stream, remoteWebConfig, FtpRemoteExists.Overwrite, true, progress: null, token: cancellationToken);
        await client.Disconnect(cancellationToken);
        return true;
    }

    public async Task<bool> TouchRestartFileAsync(
        FtpConnectionInfo connection,
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        await using var client = CreateClient(connection);
        await client.Connect(cancellationToken);

        var remoteRoot = NormalizeRemotePath(connection.RemotePath);
        var normalizedRelative = relativePath.Replace('\\', '/').TrimStart('/');
        var remotePath = CombineRemote(remoteRoot, normalizedRelative);

        var remoteDir = remotePath[..remotePath.LastIndexOf('/')];
        if (!string.IsNullOrEmpty(remoteDir))
            await client.CreateDirectory(remoteDir, true, cancellationToken);

        var content = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));
        using var stream = new MemoryStream(content);
        await client.UploadStream(stream, remotePath, FtpRemoteExists.Overwrite, true, progress: null, token: cancellationToken);
        await client.Disconnect(cancellationToken);
        return true;
    }

    private static AsyncFtpClient CreateClient(FtpConnectionInfo connection)
    {
        var client = new AsyncFtpClient(connection.Host, connection.Username, connection.Password, connection.Port)
        {
            Config =
            {
                EncryptionMode = connection.UseSsl ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None,
                DataConnectionType = FtpDataConnectionType.AutoPassive,
                ValidateAnyCertificate = true
            }
        };

        return client;
    }

    private static string NormalizeRemotePath(string path)
    {
        var normalized = (path ?? "/").Replace('\\', '/').Trim();
        if (string.IsNullOrEmpty(normalized))
            return "/";

        return normalized.StartsWith('/') ? normalized : "/" + normalized;
    }

    private static string CombineRemote(string root, string relative)
    {
        root = root.TrimEnd('/');
        relative = relative.TrimStart('/');
        return $"{root}/{relative}";
    }
}
