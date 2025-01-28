using System.Reflection;

namespace Test._4Create.API;

public class ResourceHelper
{
    public static string GetEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePath = assembly.GetManifestResourceNames()
                                   .SingleOrDefault(str => str.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

        if (resourcePath == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
        }

        using (var stream = assembly.GetManifestResourceStream(resourcePath))
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}