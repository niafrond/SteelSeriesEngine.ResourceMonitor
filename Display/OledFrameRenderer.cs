using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using SteelSeries.SysMonitor.Hardware;

namespace SteelSeries.SysMonitor.Display;

public static class OledFrameRenderer
{
    private const int ScreenWidth = 128;
    private const int ScreenHeight = 64;
    private const int TopTextHeight = 12;
    private const int BottomTextHeight = 12;

    public static void Render(string path, params ResourceStat[] stats)
    {
        using var bitmap = new Bitmap(ScreenWidth, ScreenHeight, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.None;
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        g.Clear(Color.Black);

        using var font = new Font("Consolas", 10f, FontStyle.Regular, GraphicsUnit.Pixel);
        using var timeFont = new Font("Consolas", 12f, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.White);
        using var pen = new Pen(Color.White);

        int timeHeight = 12;
        int contentTop = TopTextHeight + timeHeight + 1;
        int contentBottom = ScreenHeight - BottomTextHeight;
        int rowHeight = Math.Max(10, (contentBottom - contentTop) / Math.Max(1, stats.Length));

        for (int i = 0; i < stats.Length; i++)
            DrawRow(g, font, brush, pen, contentTop + i * rowHeight, rowHeight, stats[i]);

        DrawCentered(g, timeFont, brush, DateTime.Now.ToString("HH:mm"), ScreenWidth / 2, 1);

        bitmap.Save(path, ImageFormat.Png);
    }

    private static void DrawRow(Graphics g, Font font, Brush brush, Pen pen, int top, int rowHeight, ResourceStat stat)
    {
        int labelWidth = 24;
        int percentWidth = 20;
        int barWidth = ScreenWidth - labelWidth - percentWidth - 6;
        int barHeight = Math.Max(4, rowHeight - 3);
        int barTop = top + 1;

        g.DrawString(stat.Label, font, brush, 0, top);
        g.DrawString($"{stat.Percent:0}%", font, brush, labelWidth + 2, top);

        int barLeft = labelWidth + percentWidth + 4;
        g.DrawRectangle(pen, barLeft, barTop, barWidth, barHeight);

        float percent = Math.Clamp(stat.Percent, 0, 100);
        int fillWidth = (int)((barWidth - 2) * (percent / 100f));
        if (fillWidth > 0)
            g.FillRectangle(brush, barLeft + 1, barTop + 1, fillWidth, barHeight - 2);
    }

    private static void DrawCentered(Graphics g, Font font, Brush brush, string text, int centerX, int top)
    {
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, centerX - size.Width / 2f, top);
    }
}
