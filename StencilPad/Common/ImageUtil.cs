namespace StencilPad.Common;

public static class ImageUtil
{
    public static ImageFormat GetImageFormat(byte[] data)
    {
        if (data.Length >= 4 &&
            data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return ImageFormat.Png;
        }

        if (data.Length >= 2 &&
            data[0] == 0xFF && data[1] == 0xD8)
        {
            return ImageFormat.Jpeg;
        }

        return ImageFormat.Unknown;
    }
    
    public static string GetMimeType(byte[] data)
    {
        return GetMimeType(GetImageFormat(data));
    }

    public static string GetMimeType(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Png => "image/png",
            ImageFormat.Jpeg => "image/jpeg",
            _ => "application/octet-stream"
        };
    }
}
