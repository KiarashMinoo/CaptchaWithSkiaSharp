using SkiaSharp;

namespace CaptchaWithSkiaSharp
{
    public class CaptchaOptions
    {
        public bool IsEnabled { get; set; }
        public int LifespanInMinutes { get; set; } = 2;
        public ushort Width { get; set; } = 180;
        public ushort Height { get; set; } = 50;
        public int ImageQuality { get; set; } = 80;
        public SKEncodedImageFormat EncoderType { get; set; } = SKEncodedImageFormat.Png;
        public string[] FontFamilies { get; set; } = [];
        public byte FontSize { get; set; } = 29;
        public string FontStyle { get; set; } = nameof(SKFontStyle.Italic);
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string[] TextColor { get; set; } =
        [
            "#0000FF" /*Blue*/,
            "#000000" /*Black*/,
            "#A52A2A" /*Brown*/,
            "#808080" /*Grey*/,
            "#008000" /*Green*/
        ];
        public byte DrawLines { get; set; } = 5;
        public string[] DrawLinesColor { get; set; } =
        [
            "#0000FF" /*Blue*/,
            "#000000" /*Black*/,
            "#A52A2A" /*Brown*/,
            "#808080" /*Grey*/,
            "#008000" /*Green*/
        ];
        public float MinLineThickness { get; set; } = 0.7f;
        public float MaxLineThickness { get; set; } = 2.0f;
        public ushort NoiseRate { get; set; } = 800;
        public string[] NoiseRateColor { get; set; } = ["#808080" /*Grey*/];
    }
}