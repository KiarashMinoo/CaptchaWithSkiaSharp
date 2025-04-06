using Microsoft.Extensions.Options;
using SkiaSharp;

namespace CaptchaWithSkiaSharp
{
    public class CaptchaGenerator(IOptionsSnapshot<CaptchaOptions> options) : IDisposable
    {
        private readonly CaptchaOptions _options = options.Value;

        public byte[] GenerateImageAsByteArray(string captchaCode, int? width = null, int? height = null)
            => BuildImage(captchaCode, width ?? _options.Width, height ?? _options.Height)
                .Encode(_options.EncoderType, _options.ImageQuality)
                .ToArray();

        public Stream GenerateImageAsStream(string captchaCode, int? width = null, int? height = null)
            => BuildImage(captchaCode, width ?? _options.Width, height ?? _options.Height)
                .Encode(_options.EncoderType, _options.ImageQuality)
                .AsStream();

        private static float GenerateNextFloat(double min = -3.40282347E+38, double max = 3.40282347E+38)
        {
            var random = new Random();
            var range = max - min;
            var sample = random.NextDouble();
            var scaled = sample * range + min;
            var result = (float)scaled;
            return result;
        }

        private SKSurface DrawText(SKSurface plainSkSurface, string stringText, int width, int height)
        {
            if (string.IsNullOrEmpty(stringText))
                throw new ArgumentException($"'{nameof(stringText)}' cannot be null or empty.", nameof(stringText));

            if (stringText.Length == 1)
                throw new ArgumentException($"'{nameof(stringText)}' length must be greater than one character.", nameof(stringText));

            var plainCanvas = plainSkSurface.Canvas;
            plainCanvas.Clear(SKColor.Parse(_options.BackgroundColor));

            using (var paintInfo = new SKPaint())
            {
                var random = new Random();
                string? fontName = null;
                if (_options.FontFamilies.Length > 0)
                    fontName = _options.FontFamilies[random.Next(0, _options.FontFamilies.Length)];

                float? stringLength = null;
                var xToDraw = 0;
                var yToDraw = 0;
                foreach (var text in stringText.Select(c => c.ToString()))
                {
                    var font = new SKFont
                    {
                        Typeface = SKTypeface.FromFamilyName(fontName, _options.FontStyle switch
                        {
                            nameof(SKFontStyle.Normal) => SKFontStyle.Normal,
                            nameof(SKFontStyle.Bold) => SKFontStyle.Bold,
                            nameof(SKFontStyle.Italic) => SKFontStyle.Italic,
                            nameof(SKFontStyle.BoldItalic) => SKFontStyle.BoldItalic,
                            _ => SKFontStyle.Normal
                        }),
                        Size = _options.FontSize
                    };

                    paintInfo.Color = SKColor.Parse(_options.TextColor[random.Next(0, _options.TextColor.Length)]);
                    paintInfo.IsAntialias = true;

                    if (stringLength == null)
                    {
                        stringLength = font.MeasureText(stringText);
                        if (stringLength > width)
                            throw new ArgumentException($"'{nameof(width)}' must be greater than {stringLength}.", nameof(width));

                        xToDraw = (width - (int)stringLength.Value) / 2;
                        yToDraw = (height - _options.FontSize) / 2 + _options.FontSize;
                    }

                    plainCanvas.DrawText(text, xToDraw, yToDraw, font, paintInfo);

                    var charLength = font.MeasureText(text);
                    xToDraw += (int)charLength;
                }
            }

            plainCanvas.Flush();

            return plainSkSurface;
        }

        private SKSurface DrawLines(SKSurface plainSkSurface, int width, int height)
        {
            var captchaCanvas = plainSkSurface.Canvas;

            using (new SKPaint())
            {
                var random = new Random();
                var center = width / 2;
                var middle = height / 2;

                Parallel.For(0, _options.DrawLines, _ =>
                {
                    var x0 = random.Next(0, center);
                    var y0 = random.Next(0, middle);
                    var x1 = random.Next(center, width);
                    var y1 = random.Next(middle, height);
                    var color = SKColor.Parse(_options.DrawLinesColor[random.Next(0, _options.DrawLinesColor.Length)]);
                    var thickness = GenerateNextFloat(_options.MinLineThickness, _options.MaxLineThickness);

                    captchaCanvas.DrawLine(x0, y0, x1, y1, new SKPaint
                    {
                        Color = color,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = thickness
                    });
                });
            }

            captchaCanvas.Flush();

            return plainSkSurface;
        }

        private SKSurface DrawNoises(SKSurface plainSkSurface, int width, int height)
        {
            var captchaCanvas = plainSkSurface.Canvas;

            using (new SKPaint())
            {
                var random = new Random();

                Parallel.For(0, _options.NoiseRate, _ =>
                {
                    var x0 = random.Next(0, width);
                    var y0 = random.Next(0, height);
                    var color = SKColor.Parse(_options.NoiseRateColor[random.Next(0, _options.NoiseRateColor.Length)]);
                    var thickness = GenerateNextFloat(0.5, 1.5);

                    captchaCanvas.DrawPoint(x0, y0, new SKPaint()
                    {
                        Color = color,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = thickness
                    });
                });
            }

            captchaCanvas.Flush();

            return plainSkSurface;
        }

        private SKImage BuildImage(string captchaCode, int width, int height)
        {
            var imageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

            using var captchaSkSurface = SKSurface.Create(imageInfo);

            _ = DrawText(captchaSkSurface, captchaCode, width, height);

            if (_options.DrawLines > 0)
                _ = DrawLines(captchaSkSurface, width, height);

            if (_options.NoiseRate > 0)
                _ = DrawNoises(captchaSkSurface, width, height);

            return captchaSkSurface.Snapshot();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}