using System.Drawing.Drawing2D;
using Curve = Dynamic.Framework.Curve;
using CurveContinuity = Dynamic.Framework.CurveContinuity;
using CurveKey = Dynamic.Framework.CurveKey;
using Vector2 = Dynamic.Framework.Vector2;

// A simple extension to the Graphics class for extended 
// graphic routines, such, 
// as for creating rounded rectangles. 
// Because, Graphics class is an abstract class, 
// that is why it can not be inherited. Although, 
// I have provided a simple constructor 
// that builds the ExtendedGraphics object around a 
// previously created Graphics object. 
// Please contact: aaronreginald@yahoo.com for the most 
// recent implementations of
// this class. 
namespace System.Drawing
{

    /// <SUMMARY> 
    /// Inherited child for the class Graphics encapsulating 
    /// additional functionality for curves and rounded rectangles. 
    /// </SUMMARY> 
    public class ExtendedGraphics
    {

        private Graphics mGraphics;
        public Graphics Graphics
        {
            get { return this.mGraphics; }
            set { this.mGraphics = value; }
        }


        public ExtendedGraphics(Graphics graphics)
        {
            this.Graphics = graphics;
        }

        #region Fills a Rounded Rectangle with integers.
        public void FillRoundRectangle(System.Drawing.Brush brush,
          int x, int y,
          int width, int height, int radius)
        {

            float fx = Convert.ToSingle(x);
            float fy = Convert.ToSingle(y);
            float fwidth = Convert.ToSingle(width);
            float fheight = Convert.ToSingle(height);
            float fradius = Convert.ToSingle(radius);
            this.FillRoundRectangle(brush, fx, fy,
              fwidth, fheight, fradius);

        }
        #endregion


        #region Fills a Rounded Rectangle with continuous numbers.
        public void FillRoundRectangle(System.Drawing.Brush brush,
          float x, float y,
          float width, float height, float radius)
        {
            RectangleF rectangle = new RectangleF(x, y, width, height);
            GraphicsPath path = this.GetRoundedRect(rectangle, radius);
            this.Graphics.FillPath(brush, path);
        }
        #endregion


        #region Draws a Rounded Rectangle border with integers.
        public void DrawRoundRectangle(System.Drawing.Pen pen, int x, int y,
          int width, int height, int radius)
        {
            float fx = Convert.ToSingle(x);
            float fy = Convert.ToSingle(y);
            float fwidth = Convert.ToSingle(width);
            float fheight = Convert.ToSingle(height);
            float fradius = Convert.ToSingle(radius);
            this.DrawRoundRectangle(pen, fx, fy, fwidth, fheight, fradius);
        }
        #endregion


        #region Draws a Rounded Rectangle border with continuous numbers.
        public void DrawRoundRectangle(System.Drawing.Pen pen,
          float x, float y,
          float width, float height, float radius)
        {
            RectangleF rectangle = new RectangleF(x, y, width, height);
            GraphicsPath path = this.GetRoundedRect(rectangle, radius);
            this.Graphics.DrawPath(pen, path);
        }
        #endregion


        #region Get the desired Rounded Rectangle path.
        private GraphicsPath GetRoundedRect(RectangleF baseRect,
           float radius)
        {
            // if corner radius is less than or equal to zero, 
            // return the original rectangle 
            if (radius <= 0.0F)
            {
                GraphicsPath mPath = new GraphicsPath();
                mPath.AddRectangle(baseRect);
                mPath.CloseFigure();
                return mPath;
            }

            // if the corner radius is greater than or equal to 
            // half the width, or height (whichever is shorter) 
            // then return a capsule instead of a lozenge 
            if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
                return GetCapsule(baseRect);

            // create the arc for the rectangle sides and declare 
            // a graphics path object for the drawing 
            float diameter = radius * 2.0F;
            SizeF sizeF = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(baseRect.Location, sizeF);
            GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            // top left arc 
            path.AddArc(arc, 180, 90);

            // top right arc 
            arc.X = baseRect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc 
            arc.Y = baseRect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = baseRect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
        #endregion

        #region Gets the desired Capsular path.
        private GraphicsPath GetCapsule(RectangleF baseRect)
        {
            float diameter;
            RectangleF arc;
            GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            try
            {
                if (baseRect.Width > baseRect.Height)
                {
                    // return horizontal capsule 
                    diameter = baseRect.Height;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 90, 180);
                    arc.X = baseRect.Right - diameter;
                    path.AddArc(arc, 270, 180);
                }
                else if (baseRect.Width < baseRect.Height)
                {
                    // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 180, 180);
                    arc.Y = baseRect.Bottom - diameter;
                    path.AddArc(arc, 0, 180);
                }
                else
                {
                    // return circle 
                    path.AddEllipse(baseRect);
                }
            }
            catch (Exception ex)
            {
                path.AddEllipse(baseRect);
            }
            finally
            {
                path.CloseFigure();
            }
            return path;
        }
        #endregion
    }

    public enum CurveSmoothness
    {
        Coarse,
        Rough,
        Medium,
        Fine
    }

    public static class GraphicsEx 
    {
        private static int[]           smoothnessTable              = new int[] { 64, 32, 8, 4 };
        private static CurveSmoothness curveSmoothness              = CurveSmoothness.Fine;

        public static void DrawCurve(this Graphics g, 
                                     Curve curve, 
                                     float x, 
                                     float y, 
                                     float width,
                                     float height,
                                     Color curveColor,
                                     float thickness)  
        {
            float lastPosition = curve.Keys[curve.Keys.Count - 1].Position;

            // get curve max value
            float top = float.MaxValue, bottom = float.MinValue;
            foreach (var v in curve.Keys) {
                if (v.Value == 0) {
                    if (v.Value > bottom)
                        bottom = v.Value;
                    if (v.Value < top)
                        top = v.Value;
                }
                else if (v.Value > 0) {
                    if (v.Value > bottom)
                        bottom = v.Value;
                }
                else {
                    if (v.Value < top)
                        top = v.Value;
                }
            }
            
            // fixed
            float curveHeight = bottom + -top;
            if (lastPosition == 0) lastPosition = width;
            if (curveHeight == 0) curveHeight = height;

            Vector2 scale = new Vector2(width / lastPosition,
                                        height / curveHeight);

            double dt0 = x;
            double dt1 = dt0 + width / scale.X;
            double step = scale.X / smoothnessTable[(int)curveSmoothness];

            Pen pen0 = new Pen(curveColor, thickness);

            if (curve.Keys.Count > 0) {
                double kt0 = curve.Keys[0].Position;
                double kt1 = curve.Keys[curve.Keys.Count - 1].Position;

                double t0 = dt0;
                double t1 = Math.Min(dt1, kt0);

                // draw fact section
                t0 = Math.Max(dt0, kt0);
                t1 = Math.Min(dt1, kt1);
                if (t0 < t1) {
                    Vector2[] p = new Vector2[2] { new Vector2(), new Vector2() };
                    // Search key and next key that includes t0 position.
                    int keyIndex = 0;
                    CurveKey key = null, nextKey = null;
                    for (; keyIndex < curve.Keys.Count; ++keyIndex)
                    {
                        key = nextKey; nextKey = curve.Keys[keyIndex];
                        if (nextKey.Position > t0) break;
                    }

                    int pIdx = 0;
                    p[pIdx] = new Vector2((float)t0 / scale.X, curve.Evaluate((float)t0) * scale.Y + y);
                    for (double t = t0; t < t1; )  {
                        double nextT = t1 + step;
                        if (nextKey != null)
                            nextT = Math.Min(t1, nextKey.Position);

                        // Draw current key and next key section.
                        if (key.Continuity == CurveContinuity.Smooth)  {
                            while (t < nextT) {
                                // If this line crosses next key position, draw line from
                                // current position to next key position.
                                t = (t < nextT && t + step > nextT) ? nextT : t + step;
                                pIdx = (pIdx + 1) & 1;
                                p[pIdx] = new Vector2((float)t / scale.X, curve.Evaluate((float)t) / scale.Y + y);
                                DrawLine(g, pen0, p[0], p[1]);
                            }
                        }
                        else {
                            // Step case,
                            // Draw, horizontal line.
                            pIdx = (pIdx + 1) & 1;
                            p[pIdx] = new Vector2(nextKey.Position / scale.X, key.Value / scale.Y + y);
                            DrawLine(g, pen0, p[0], p[1]);

                            // Draw vertical line.
                            pIdx = (pIdx + 1) & 1;
                            p[pIdx] = new Vector2(nextKey.Position / scale.X, nextKey.Value / scale.Y + y);
                            DrawLine(g, pen0, p[0], p[1]);

                            t = nextT;
                        }
                        // Advance to next key.
                        key = nextKey;
                        nextKey = (++keyIndex < curve.Keys.Count) ? curve.Keys[keyIndex] : null;
                    }
                }
            }
        }

        /// <summary>
        /// Draw Line with clipping.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private static void DrawLine(Graphics g, Pen pen, Vector2 p1, Vector2 p2)
        {
            g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        #region Draw Using System Text

        public static void PaintUsingSystemDrawing(this Graphics graphics, string text, Font font, Color fontColor, Rectangle clientRectangle)
        {
            using (Brush brush = new SolidBrush(fontColor))
            {
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    graphics.DrawString(text, font, brush, clientRectangle, format);
                }
            }
        }

        #endregion


    }
} 
