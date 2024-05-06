using Microsoft.Extensions.FileProviders;

namespace Kaban;

public static class SecretHelper
{
    public static string? GetSecret(string key)
    {
        const string dockerSecretPath = "/run/secrets/";
        if (Directory.Exists(dockerSecretPath))
        {
            IFileProvider provider = new PhysicalFileProvider(dockerSecretPath);
            IFileInfo fileInfo = provider.GetFileInfo(key);
            if (fileInfo.Exists)
            {
                using (var stream = fileInfo.CreateReadStream())
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
      
        return null;
    }
}