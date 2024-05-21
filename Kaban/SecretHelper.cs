using Microsoft.Extensions.FileProviders;

namespace Kaban;

public static class SecretHelper
{
    // public static string? GetVar(IHostApplicationBuilder builder, string key)
    // {
    //     var secret = GetSecret(builder, key);
    //     if (secret != null)
    //         return secret;
    //
    //     var env = Environment.GetEnvironmentVariable(key);
    //     return env;
    // }

    public static string? GetSecret(IHostApplicationBuilder builder, string key)
    {
        var movieApiKey = builder.Configuration[key];

        if (movieApiKey != null)
            return movieApiKey;

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