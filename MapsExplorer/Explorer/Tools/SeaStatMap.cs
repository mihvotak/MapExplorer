using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

public static class SeaStatMap
{
    public const int ImageSize = 1000;
    public const int MaxDistance = 24;
    public const int SmallSeaMaxDistance = 16;
    private const float Margin = 40;
    private const float MapTop = 80;
    private static readonly SeaPosition Origin = new SeaPosition(0, 0, 0);
    public static void Save(string path, IEnumerable<SeaPosition> positions, string title, int maxDistance = MaxDistance,
        IEnumerable<SeaPosition> lonelyPositions = null)
    {
        if (maxDistance < 1 || maxDistance > MaxDistance)
            throw new ArgumentOutOfRangeException(nameof(maxDistance));
        var marked = new HashSet<SeaPosition>(positions);
        var lonely = new HashSet<SeaPosition>(lonelyPositions ?? Enumerable.Empty<SeaPosition>());
        if (marked.Concat(lonely).Any(p => p.X + p.Y + p.Z != 0 || p.DistanceTo(Origin) > maxDistance))
            throw new ArgumentOutOfRangeException(nameof(positions), "Sea map coordinates must be within " + maxDistance + " hexes of the port.");
        var rings = new[] { 7, 14, 21 }.Where(r => r <= maxDistance).ToArray();

        using (var bitmap = new Bitmap(ImageSize, ImageSize))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var seaBrush = new SolidBrush(Color.FromArgb(82, 150, 172)))
        using (var mark1Brush = new SolidBrush(Color.FromArgb(100, 255, 100)))
        using (var mark2Brush = new SolidBrush(Color.FromArgb(200, 80, 200)))
        using (var gridPen = new Pen(Color.FromArgb(48, 88, 108), 0.9f))
        using (var ringPen = new Pen(Color.FromArgb(172, 225, 233), 1f))
        using (var portPen = new Pen(Color.FromArgb(255, 255, 255), 2f))
        using (var titleFont = new Font("Arial", 19, FontStyle.Regular, GraphicsUnit.Pixel))
        using (var captionFont = new Font("Arial", 14, FontStyle.Regular, GraphicsUnit.Pixel))
        using (var format = new StringFormat { Alignment = StringAlignment.Center })
        {
            graphics.Clear(Color.White);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawString(title, titleFont, Brushes.Black, new RectangleF(0, 15, ImageSize, 30), format);
            graphics.DrawString("Разломы с левиафаном (фиолетовый): " + marked.Count + " · Одинокие (зелёный): " + lonely.Count,
                captionFont, Brushes.Black, new RectangleF(0, 46, ImageSize, 25), format);

            var cells = new List<SeaPosition>();
            for (int x = -maxDistance; x <= maxDistance; x++)
                for (int z = -maxDistance; z <= maxDistance; z++)
                {
                    var position = new SeaPosition(x, -x - z, z);
                    if (position.DistanceTo(Origin) > maxDistance)
                        continue;
                    cells.Add(position);
                    var polygon = GetPolygon(position);
                    graphics.FillPolygon(marked.Contains(position) ? mark2Brush
                        : lonely.Contains(position) ? mark1Brush : seaBrush, polygon);
                    graphics.DrawPolygon(gridPen, polygon);
                }

            graphics.DrawPolygon(portPen, GetPolygon(new SeaPosition(0, 0, 0)));
            foreach (int radius in rings)
                foreach (var position in cells.Where(p => p.DistanceTo(Origin) == radius))
                    graphics.DrawPolygon(ringPen, GetPolygon(position));
            bitmap.Save(path, ImageFormat.Png);
        }
    }

    // Same pointy-top orientation as the sea log renderer: X points right, Z down-right.
    public static PointF GetCenter(SeaPosition position)
    {
        float size = HexSize;
        return new PointF(ImageSize / 2f + (float)(Math.Sqrt(3) * (position.X + position.Z / 2.0)) * size,
            (MapTop + ImageSize - Margin) / 2f + 1.5f * position.Z * size);
    }

    // Keep the radius-24 scale even when drawing the smaller radius-16 disk.
    private static float HexSize => Math.Min(
        (ImageSize - 2 * Margin) / (float)(Math.Sqrt(3) * (2 * MaxDistance + 1)),
        (ImageSize - MapTop - Margin) / (3 * MaxDistance + 2));

    private static PointF[] GetPolygon(SeaPosition position)
    {
        var center = GetCenter(position);
        var points = new PointF[6];
        for (int i = 0; i < points.Length; i++)
        {
            double angle = (60 * i - 30) * Math.PI / 180;
            points[i] = new PointF(center.X + HexSize * (float)Math.Cos(angle), center.Y + HexSize * (float)Math.Sin(angle));
        }
        return points;
    }
}
