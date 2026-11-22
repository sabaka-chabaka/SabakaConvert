using FFMpegCore;

namespace SabakaConvert.Converters;

[Converter]
public class VideoConverter : IFileConverter
{
    private static readonly HashSet<string> VideoFormats =
    [
        with(StringComparer.OrdinalIgnoreCase),
        ".mp4", ".mkv", ".avi", ".mov", ".webm", ".flv", ".wmv", ".m4v"
    ];
    
    public bool CanConvert(string fromExt, string toExt) =>
        !fromExt.Equals(toExt, StringComparison.OrdinalIgnoreCase) &&
        VideoFormats.Contains(fromExt) &&
        VideoFormats.Contains(toExt);

    public void Convert(string inputPath, string outputPath)
    {
       var targetExt = Path.GetExtension(inputPath).ToLowerInvariant();
       
       var conversion = FFMpegArguments
           .FromFileInput(inputPath)
           .OutputToFile(outputPath, overwrite: true, options =>
           {
               switch (targetExt)
               {
                   case ".mp4":
                   case ".m4v":
                   case ".mov": 
                       options.WithVideoCodec("libx264")
                           .WithConstantRateFactor(23)
                           .WithCustomArgument("-pix_fmt yuv420p")
                           .WithAudioCodec("aac")
                           .WithAudioBitrate(192)
                           .WithFastStart();
                       break;
                   
                   case ".mkv":
                       options.WithVideoCodec("libx264")
                           .WithConstantRateFactor(23)
                           .WithCustomArgument("-pix_fmt yuv420p")
                           .WithAudioCodec("aac")
                           .WithAudioBitrate(192);
                       break;

                   case ".webm":
                       options.WithVideoCodec("libvpx-vp9")
                           .WithConstantRateFactor(32)
                           .WithCustomArgument("-b:v 0 -row-mt 1")
                           .WithAudioCodec("libopus")
                           .WithAudioBitrate(128);
                       break;

                   case ".avi":
                       options.WithVideoCodec("mpeg4")
                           .WithCustomArgument("-q:v 4")
                           .WithAudioCodec("libmp3lame")
                           .WithAudioBitrate(192);
                       break;

                   case ".flv":
                       options.WithVideoCodec("libx264")
                           .WithConstantRateFactor(23)
                           .WithCustomArgument("-pix_fmt yuv420p")
                           .WithAudioCodec("aac")
                           .WithAudioBitrate(128);
                       break;

                   case ".wmv":
                       options.WithVideoCodec("wmv2")
                           .WithVideoBitrate(2500)
                           .WithAudioCodec("wmav2")
                           .WithAudioBitrate(192);
                       break;
               }
           });

       conversion.ProcessSynchronously();
    }
}