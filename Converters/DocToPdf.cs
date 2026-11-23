using Aspose.Words;

namespace SabakaConvert.Converters;

[Converter]
public class DocxToPdfConverter : IFileConverter
{
    private const string DocxExt = ".docx";
    private const string PdfExt = ".pdf";

    public bool CanConvert(string fromExt, string toExt) =>
        fromExt.Equals(DocxExt, StringComparison.OrdinalIgnoreCase) &&
        toExt.Equals(PdfExt, StringComparison.OrdinalIgnoreCase);

    public void Convert(string inputPath, string outputPath)
    {
        var doc = new Document(inputPath);
        doc.Save(outputPath, SaveFormat.Pdf);
    }
}