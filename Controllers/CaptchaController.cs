using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace CaptchaWithSkiaSharp.Controllers;

public class CaptchaController : Controller
{
    private readonly CaptchaGenerator _captchaGenerator;
    private readonly CaptchaOptions _captchaOptions;
    private readonly string _mimeType;

    public CaptchaController(IOptionsSnapshot<CaptchaOptions> captchaOptions, CaptchaGenerator captchaGenerator)
    {
        _captchaGenerator = captchaGenerator;
        _captchaOptions = captchaOptions.Value;

        _mimeType = _captchaOptions.EncoderType switch
        {
            SKEncodedImageFormat.Bmp => "image/bmp",
            SKEncodedImageFormat.Gif => "image/gif",
            SKEncodedImageFormat.Ico => "image/x-icon",
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Wbmp => "image/x-wbmp",
            SKEncodedImageFormat.Webp => "image/webp",
            SKEncodedImageFormat.Pkm => "image/pkm",
            SKEncodedImageFormat.Ktx => "image/ktx",
            SKEncodedImageFormat.Astc => "image/astc",
            SKEncodedImageFormat.Dng => "image/dng",
            SKEncodedImageFormat.Heif => "image/heif",
            SKEncodedImageFormat.Avif => "image/avif",
            SKEncodedImageFormat.Jpegxl => "image/jpeg-xl",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
    public IActionResult Index()
    {
        if (!_captchaOptions.IsEnabled)
            throw new InvalidOperationException();

        var code = Random.Shared.Next(100001, 999999).ToString();
        var captchaImage = _captchaGenerator.GenerateImageAsStream(code);

        return File(captchaImage, _mimeType);
    }

    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
    public IActionResult Base64()
    {
        if (!_captchaOptions.IsEnabled)
            throw new InvalidOperationException();

        var code = Random.Shared.Next(100001, 999999).ToString();
        var captchaImage = _captchaGenerator.GenerateImageAsByteArray(code);

        return Content(Convert.ToBase64String(captchaImage));
    }
}