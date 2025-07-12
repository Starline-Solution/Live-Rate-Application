using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedPanel : Panel
{
    private int _cornerRadius = 15;
    private int _shadowDepth = 5;
    private Color _shadowColor = Color.FromArgb(100, 0, 0, 0);
    private Color _borderColor = Color.Transparent;
    private int _borderWidth = 1;

    public RoundedPanel()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
    }

    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    public int ShadowDepth
    {
        get => _shadowDepth;
        set { _shadowDepth = Math.Max(0, value); Invalidate(); }
    }

    public Color ShadowColor
    {
        get => _shadowColor;
        set { _shadowColor = value; Invalidate(); }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    public int BorderWidth
    {
        get => _borderWidth;
        set { _borderWidth = Math.Max(0, value); Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Calculate the base rectangle (accounting for shadow and border)
        int effectiveShadowDepth = Math.Min(ShadowDepth, 10); // Limit shadow depth
        var baseRect = new Rectangle(
            effectiveShadowDepth + BorderWidth,
            effectiveShadowDepth + BorderWidth,
            Width - (effectiveShadowDepth * 2) - (BorderWidth * 2),
            Height - (effectiveShadowDepth * 2) - (BorderWidth * 2));

        // Draw shadow if enabled
        if (effectiveShadowDepth > 0 && ShadowColor.A > 0)
        {
            DrawShadow(e.Graphics, baseRect);
        }

        // Draw the main panel
        using (var path = CreateRoundedPath(baseRect, CornerRadius))
        {
            // Fill panel background
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw border if enabled
            if (BorderWidth > 0 && BorderColor.A > 0)
            {
                using (var pen = new Pen(BorderColor, BorderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }
    }

    private void DrawShadow(Graphics g, Rectangle baseRect)
    {
        int shadowSteps = Math.Min(ShadowDepth, 10);
        int maxAlpha = ShadowColor.A;

        for (int i = shadowSteps; i >= 1; i--)
        {
            int alpha = maxAlpha * i / shadowSteps / 3; // Divided by 3 to make shadow more subtle
            if (alpha <= 0) continue;

            var shadowRect = new Rectangle(
                baseRect.X - i,
                baseRect.Y - i,
                baseRect.Width + (i * 2),
                baseRect.Height + (i * 2));

            using (var path = CreateRoundedPath(shadowRect, CornerRadius + i))
            using (var brush = new SolidBrush(Color.FromArgb(alpha, ShadowColor)))
            {
                g.FillPath(brush, path);
            }
        }
    }

    private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();

        try
        {
            // Ensure radius is valid
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top left
            path.AddArc(arc, 180, 90);

            // Top right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
        }
        catch
        {
            // Fallback to simple rectangle if something goes wrong
            path.Dispose();
            path = new GraphicsPath();
            path.AddRectangle(rect);
        }

        return path;
    }
}