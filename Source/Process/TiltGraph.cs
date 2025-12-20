using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public class PlotPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public Color c { get; set; }
        public bool DrawDot { get; set; }   // 추가

        public bool DrawLine { get; set; }

    }
    public sealed class TiltGraph : Control
    {
        readonly object _lock = new object();
        readonly List<PlotPoint> _points = new List<PlotPoint>();

        public double range { get; set; } = 50;
        public double[] ringRad { get; set; } = Array.Empty<double>();
        public string title { get; set; } = "Tilt X, Y";
        public string xLabel { get; set; } = "Tilt X(min)";
        public string yLabel { get; set; } = "Tilt Y(min)";

        readonly Padding _pad = new Padding(70, 50, 40, 70);

        public TiltGraph()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            BackColor = Color.White;
            Font = new Font("Arial", 9f, FontStyle.Bold);
            ResizeRedraw = true;

        }
        public void SetPoints(double[] xs, double[] ys, Color c)
        {
            if (xs == null || ys == null || xs.Length != ys.Length) return;

            lock (_lock)
            {
                _points.Clear();
                for (int i = 0; i < xs.Length; i++)
                {
                    _points.Add(new PlotPoint
                    {
                        x = xs[i],
                        y = ys[i],
                        c = c,
                        DrawDot = false,   // 점 표시 안함
                        DrawLine = true
                    });
                }
            }
            Invalidate();
        }

        public void SetPoint(double xs, double ys, Color c)
        {

            lock (_lock)
            {
                _points.Add(new PlotPoint
                {
                    x = xs,
                    y = ys,
                    c = c,
                    DrawDot = true,  // 점 표시
                    DrawLine = false
                });
            }
            Invalidate();
        }
        public void ClearPoint()
        {
            lock (_lock) _points.Clear();
            Invalidate();
        }
        public void SetRings(params double[] radii)
        {
            if (radii == null || radii.Length == 0)
            {
                ringRad = Array.Empty<double>();
            }
            else
            {
                ringRad = radii
                    .Where(r => r > 0)
                    .OrderBy(r => r)
                    .ToArray();
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (Width <= 10 || Height <= 10)
                return;

            // ---- 스케일 계산 (컨트롤 크기에 따라 선/글자 크기 보정) ----
            float minSide = Math.Min(Width, Height);
            float scale = minSide / 400f;
            if (scale < 0.4f) scale = 0.4f;
            if (scale > 1.5f) scale = 1.5f;

            var titleFont = new Font(Font.FontFamily, 12f * scale, FontStyle.Bold);
            var labelFont = new Font(Font.FontFamily, 9f * scale);
            var tickFont = new Font(Font.FontFamily, 9f * scale);
            var tf = TextFormatFlags.NoPadding;

            if (range <= 0) range = 1; // 방어

            // Range 텍스트
            string minStr = (-range).ToString("0");
            string zeroStr = "0";
            string maxStr = (range).ToString("0");

            // 텍스트 사이즈
            Size szTitle = Measure(title, titleFont, tf);
            Size szX = Measure(xLabel, labelFont, tf);
            Size szY = Measure(yLabel, labelFont, tf);
            Size szTickMax = Measure(maxStr, tickFont, tf);
            Size szTickMin = Measure(minStr, tickFont, tf);

            int tickW = Math.Max(szTickMax.Width, szTickMin.Width);
            int tickH = Math.Max(szTickMax.Height, szTickMin.Height);

            // 여백 계산
            int yLabelThickness = szY.Height;             // 회전 시 두께
            int gapSmall = (int)Math.Ceiling(6 * scale);
            int gapTitle = (int)Math.Ceiling(8 * scale);
            int gapAxis = (int)Math.Ceiling(6 * scale);

            int left = yLabelThickness + gapSmall + tickW + gapAxis;
            int right = (int)Math.Ceiling(16 * scale);
            int top = szTitle.Height + gapTitle;
            int bottom = szX.Height + gapAxis + tickH;

            // 사용 가능한 영역
            var avail = Rectangle.FromLTRB(left, top, Width - right, Height - bottom);
            if (avail.Width <= 4 || avail.Height <= 4)
                return;

            // 정사각형 플롯 박스
            int box = Math.Min(avail.Width, avail.Height);
            var plot = new Rectangle(
                avail.Left + (avail.Width - box) / 2,
                avail.Top + (avail.Height - box) / 2,
                box,
                box);

            // 공통 스케일: 데이터 1 단위 → 몇 픽셀
            double unit = box / (2.0 * range);

            // 중심점
            float cx = plot.Left + plot.Width / 2f;
            float cy = plot.Top + plot.Height / 2f;

            // ---- 플롯 배경 ----
            using (var back = new SolidBrush(Color.FromArgb(25, 25, 25)))
                g.FillRectangle(back, plot);

            // ---- 동심원 ----
            using (var ringPen = new Pen(Color.FromArgb(150, 150, 150), Math.Max(1f, 1f * scale)))
            {
                foreach (var r in ringRad)
                {
                    if (r <= 0) continue;
                    float pr = (float)(r * unit); // RingRadii의 값 그대로 반영
                    if (pr <= 0) continue;

                    var rect = new RectangleF(
                        cx - pr,
                        cy - pr,
                        pr * 2,
                        pr * 2);

                    g.DrawEllipse(ringPen, rect);
                }
            }

            // ---- 십자 축 (0,0) ----
            using (var axisPen = new Pen(Color.LightGray, Math.Max(1f, 1.2f * scale)))
            {
                g.DrawLine(axisPen, cx, plot.Top, cx, plot.Bottom);
                g.DrawLine(axisPen, plot.Left, cy, plot.Right, cy);
            }

            // ---- 포인트(데이터) ----
            lock (_lock)
            {
                int n = _points.Count;
                if (n == 0) return;

                Pen linePen = null;
                Dictionary<Color, SolidBrush> brushMap = null;

                try
                {
                    // 1) 선 그리기
                    if (n >= 2)
                    {
                        linePen = new Pen(Color.Lime, 1f * scale);
                        linePen.LineJoin = LineJoin.Round;
                        linePen.StartCap = LineCap.Round;
                        linePen.EndCap = LineCap.Round;

                        //   PointF[] pts = new PointF[n];
                        List<PointF> pts = new List<PointF>();
                        for (int i = 0; i < n; i++)
                        {
                            var p = _points[i];
                            if (!p.DrawLine) continue;
                            pts.Add(new PointF((float)(cx + p.x * unit),
                                (float)(cy - p.y * unit)));
                        }

                        g.DrawLines(linePen, pts.ToArray());
                    }

                    // 2) 점 그리기 (색별 브러시 재사용)
                    brushMap = new Dictionary<Color, SolidBrush>();

                    for (int i = 0; i < n; i++)
                    {
                        var p = _points[i];
                        if (!p.DrawDot) continue;   // <-- 핵심

                        SolidBrush brush;
                        if (!brushMap.TryGetValue(p.c, out brush))
                        {
                            brush = new SolidBrush(p.c);
                            brushMap[p.c] = brush;
                        }

                        float px = (float)(cx + p.x * unit);
                        float py = (float)(cy - p.y * unit);

                        g.FillEllipse(brush,
                            px - 2.5f * scale,
                            py - 2.5f * scale,
                            5f * scale,
                            5f * scale);
                    }
                }
                finally
                {
                    // Dispose 정리
                    if (linePen != null)
                        linePen.Dispose();

                    if (brushMap != null)
                    {
                        foreach (var b in brushMap.Values)
                            b.Dispose();
                    }
                }
            }

            // ---- 축 수치 (−Range, 0, +Range) ----
            var tickColor = Color.Black;

            // X축 아래
            TextRenderer.DrawText(g, minStr, tickFont,
                new Point((int)(plot.Left - tickW / 2f), (int)(plot.Bottom + 2)),
                tickColor, tf);

            TextRenderer.DrawText(g, zeroStr, tickFont,
                new Point((int)(cx - Measure(zeroStr, tickFont, tf).Width / 2f), (int)(plot.Bottom + 2)),
                tickColor, tf);

            TextRenderer.DrawText(g, maxStr, tickFont,
                new Point((int)(plot.Right - Measure(maxStr, tickFont, tf).Width / 2f), (int)(plot.Bottom + 2)),
                tickColor, tf);

            // Y축 왼쪽 (+Range, 0, -Range)
            float xTick = plot.Left - gapSmall - tickW;
            var fmtRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

            g.DrawString(maxStr, tickFont, Brushes.Black,
                xTick, plot.Top, fmtRight);

            g.DrawString(zeroStr, tickFont, Brushes.Black,
                xTick, cy - tickFont.Height / 2f, fmtRight);

            g.DrawString(minStr, tickFont, Brushes.Black,
                xTick, plot.Bottom - tickFont.Height, fmtRight);

            // ---- X축 라벨 ----
            TextRenderer.DrawText(g, xLabel, labelFont,
                new Rectangle(plot.Left, plot.Bottom + tickH + 4, plot.Width, szX.Height),
                Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | tf);

            // ---- Y축 라벨 (세로) ----
            using (var yBrush = new SolidBrush(Color.Black))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                float xLabelCenter = plot.Left - (yLabelThickness / 2f) - gapSmall;
                float yLabelCenter = plot.Top + plot.Height / 2f;

                g.TranslateTransform(xLabelCenter, yLabelCenter);
                g.RotateTransform(-90);
                g.DrawString(yLabel, labelFont, yBrush, 0, 0, sf);
                g.ResetTransform();
            }

            // ---- 타이틀 ----
            TextRenderer.DrawText(g, title, titleFont,
                new Rectangle(0, 0, Width, szTitle.Height),
                Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom | tf);
        }


        //void DrawMarker(Graphics g, Rectangle plot, PlotPoint p)
        //{
        //    double scale = plot.Width / (2.0 * range);
        //    var cx = plot.Left + plot.Width / 2f;
        //    var cy = plot.Top + plot.Height / 2f;

        //    float px = (float)(cx + p.x * scale);
        //    float py = (float)(cy - p.y * scale);
        //    //float s = 12f;

        //    var pen = new Pen(p.c, 2f);
        //    var brush = new SolidBrush(p.c);

        //    g.FillEllipse(brush, px - 2, py - 2, 4, 4);
        //}
        static Size Measure(string text, Font font, TextFormatFlags flags)
        {
            // NoPadding으로 실제 표시 크기를 타이트하게 얻기
            return TextRenderer.MeasureText(text ?? string.Empty, font, new Size(int.MaxValue, int.MaxValue), flags);
        }
    }
}
