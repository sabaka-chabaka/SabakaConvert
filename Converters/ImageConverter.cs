using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SabakaConvert.Converters;

[Converter]
public class ImageConverter : IFileConverter
{
    private static readonly HashSet<string> Formats = [with(StringComparer.OrdinalIgnoreCase), ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".gif"];

    public bool CanConvert(string fromExt, string toExt) =>
        !fromExt.Equals(toExt, StringComparison.OrdinalIgnoreCase) &&
        Formats.Contains(fromExt) &&
        Formats.Contains(toExt);

    public void Convert(string inputPath, string outputPath)
    {
        using var image = Image.Load(inputPath);
        
        var targetExt = Path.GetExtension(outputPath);
        if (targetExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || 
            targetExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            image.Mutate(x => x.BackgroundColor(Color.White));
        }

        image.Save(outputPath);
    }
}