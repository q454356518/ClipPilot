using copyutil.Models;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace copyutil.Services;

public sealed class ClipboardImageService
{
    private readonly string _imageDirectory = Path.Combine(Path.GetTempPath(), "ClipPilot");

    public PathItem? SaveClipboardImage()
    {
        try
        {
            if (!System.Windows.Clipboard.ContainsImage())
            {
                return null;
            }

            var image = System.Windows.Clipboard.GetImage();
            if (image is null || image.PixelWidth <= 0 || image.PixelHeight <= 0)
            {
                return null;
            }

            Directory.CreateDirectory(_imageDirectory);
            var path = Path.Combine(_imageDirectory, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
            var bitmap = CopyToBgr32(image);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using var stream = File.Create(path);
            encoder.Save(stream);
            return new PathItem(path, PathItemType.File, true);
        }
        catch
        {
            return null;
        }
    }

    private static BitmapSource CopyToBgr32(BitmapSource source)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgr32, null, 0);
        var stride = converted.PixelWidth * 4;
        var pixels = new byte[stride * converted.PixelHeight];
        converted.CopyPixels(pixels, stride, 0);

        var bitmap = BitmapSource.Create(
            converted.PixelWidth,
            converted.PixelHeight,
            converted.DpiX,
            converted.DpiY,
            PixelFormats.Bgr32,
            null,
            pixels,
            stride);
        bitmap.Freeze();
        return bitmap;
    }
}
