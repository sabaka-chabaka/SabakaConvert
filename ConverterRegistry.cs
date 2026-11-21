using System.Reflection;

namespace SabakaConvert;

public class ConverterRegistry
{
    private readonly List<IFileConverter> _converters = [];

    public IFileConverter FindConverter(string fromExt, string toExt) => 
        _converters.FirstOrDefault(converter => converter.CanConvert(fromExt, toExt)) 
        ?? throw new InvalidOperationException("Cannot find converter for " + fromExt + " to " + toExt);

    public void Register()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        var converterTypes = assembly.GetTypes()
            .Where(t => t.IsClass 
                        && !t.IsAbstract
                        && typeof(IFileConverter).IsAssignableFrom(t)
                        && t.IsDefined(typeof(ConverterAttribute), inherit: false));

        foreach (var type in converterTypes)
        {
            if (Activator.CreateInstance(type) is IFileConverter converter)
            {
                _converters.Add(converter);
            }
        }
    }
}