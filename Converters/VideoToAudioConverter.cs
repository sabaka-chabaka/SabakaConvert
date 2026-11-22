using FFMpegCore;

namespace SabakaConvert.Converters;

[Converter]
public class VideoToAudioConverter : IFileConverter
{
    private static readonly HashSet<string> VideoFormats =
    [
        with(StringComparer.OrdinalIgnoreCase),
        ".mp4", ".mkv", ".avi", ".mov", ".webm", ".flv", ".wmv"
    ];

    private static readonly HashSet<string> AudioFormats =
    [
        with(StringComparer.OrdinalIgnoreCase),
        ".mp3", ".wav", ".aac", ".ogg", ".flac", ".m4a"
    ];

    public bool CanConvert(string fromExt, string toExt) =>
        !fromExt.Equals(toExt, StringComparison.OrdinalIgnoreCase) &&
        VideoFormats.Contains(fromExt) &&
        AudioFormats.Contains(toExt);

    public void Convert(string inputPath, string outputPath)
    {
        var targetExt = Path.GetExtension(outputPath).ToLowerInvariant();

        var conversion = FFMpegArguments.FromFileInput(inputPath).OutputToFile(outputPath, overwrite: true, options =>
        {
            options.DisableVideo();

            switch (targetExt)
            {
                case ".mp3":
                    options.WithAudioCodec("libmp3lame")
                        .WithAudioBitrate(192);
                    break;
                case ".aac":
                case ".m4a":
                    options.WithAudioCodec("aac")
                        .WithAudioBitrate(192);
                    break;
                case ".ogg":
                    options.WithAudioCodec("libvorbis")
                        .WithAudioBitrate(192);
                    break;
                case ".flac":
                    options.WithAudioCodec("flac");
                    break;
                case ".wav":
                    options.WithAudioCodec("pcm_s16le");
                    break;
            }
        });

        conversion.ProcessSynchronously();
    }
}