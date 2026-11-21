namespace SabakaConvert;

public interface IFileConverter
{
    bool CanConvert(string fromExt, string toExt);
    void Convert(string inputPath, string outputPath);
}