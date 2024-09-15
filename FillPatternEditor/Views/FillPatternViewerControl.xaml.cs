using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FillPatternEditor.Converters;
using FillPatternEditor.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Markup;

namespace FillPatternEditor.Views
{
    public partial class FillPatternViewerControl : UserControl, INotifyPropertyChanged
    {
        private const float Length = 100f;
        private BitmapSource _fillPatternImg;
        public static readonly DependencyProperty FillPatternProperty = DependencyProperty.RegisterAttached(nameof(FillPattern), typeof(FillPattern), typeof(FillPatternViewerControl), (PropertyMetadata)new UIPropertyMetadata((object)null, new PropertyChangedCallback(FillPatternViewerControl.OnFillPatternChanged)));
        public static readonly DependencyProperty ColorBrushProperty = DependencyProperty.Register(nameof(ColorBrush), typeof(SolidColorBrush), typeof(FillPatternViewerControl), new PropertyMetadata((object)new SolidColorBrush(Colors.Black)));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(nameof(Scale), typeof(float), typeof(FillPatternViewerControl), new PropertyMetadata((object)75f, new PropertyChangedCallback(FillPatternViewerControl.OnScaleChanged)));

        private static void OnFillPatternChanged(
          DependencyObject d,
          DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FillPatternViewerControl patternViewerControl))
                return;
            patternViewerControl.OnPropertyChanged("FillPattern");
            patternViewerControl.CreateFillPatternImage();
        }

        public FillPattern FillPattern
        {
            get => (FillPattern)this.GetValue(FillPatternViewerControl.FillPatternProperty);
            set => this.SetValue(FillPatternViewerControl.FillPatternProperty, (object)value);
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FillPatternViewerControl patternViewerControl))
                return;
            patternViewerControl.Regenerate();
        }

        public float Scale
        {
            get => (float)this.GetValue(FillPatternViewerControl.ScaleProperty);
            set => this.SetValue(FillPatternViewerControl.ScaleProperty, (object)value);
        }

        public SolidColorBrush ColorBrush
        {
            get => (SolidColorBrush)this.GetValue(FillPatternViewerControl.ColorBrushProperty);
            set => this.SetValue(FillPatternViewerControl.ColorBrushProperty, (object)value);
        }

        public FillPattern GetFillPattern(DependencyObject obj)
        {
            return (FillPattern)obj.GetValue(FillPatternViewerControl.FillPatternProperty);
        }

        public void SetFillPattern(DependencyObject obj, FillPattern value)
        {
            obj.SetValue(FillPatternViewerControl.FillPatternProperty, (object)value);
        }

        public void Regenerate() => this.CreateFillPatternImage();

        public FillPatternViewerControl()
        {
            this.InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => this.Regenerate();

        public BitmapSource FillPatternImage
        {
            get
            {
                if (this._fillPatternImg == null)
                    this.CreateFillPatternImage();
                return this._fillPatternImg;
            }
        }

        private void CreateFillPatternImage()
        {
            try
            {
                double num1 = (this.ActualWidth == 0.0 ? this.Width : this.ActualWidth) == 0.0 ? 100.0 : (this.ActualWidth == 0.0 ? this.Width : this.ActualWidth);
                if (double.IsNaN(num1))
                    num1 = 100.0;
                double num2 = (this.ActualHeight == 0.0 ? this.Height : this.ActualHeight) == 0.0 ? 30.0 : (this.ActualHeight == 0.0 ? this.Height : this.ActualHeight);
                if (double.IsNaN(num2))
                    num2 = 30.0;
                Bitmap image = new Bitmap((int)num1, (int)num2);
                using (Graphics g = Graphics.FromImage((System.Drawing.Image)image))
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, (int)num1, (int)num2);
                    g.FillRectangle(System.Drawing.Brushes.Transparent, rect);
                    this.DrawFillPattern(g);
                }
                this._fillPatternImg = BitmapSourceConverter.ConvertFromImage(image);
                this.OnPropertyChanged("FillPatternImage");
            }
            catch
            {
            }
        }

        private void DrawFillPattern(Graphics g)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            FillPattern fillPattern = this.FillPattern;
            if (fillPattern == null)
                return;
            float num1 = fillPattern.Target != FillPatternTarget.Model ? this.Scale * 10f : this.Scale;
            try
            {
                double num2 = (this.ActualWidth == 0.0 ? this.Width : this.ActualWidth) == 0.0 ? 100.0 : (this.ActualWidth == 0.0 ? this.Width : this.ActualWidth);
                if (double.IsNaN(num2))
                    num2 = 100.0;
                double num3 = (this.ActualHeight == 0.0 ? this.Height : this.ActualHeight) == 0.0 ? 30.0 : (this.ActualHeight == 0.0 ? this.Height : this.ActualHeight);
                if (double.IsNaN(num3))
                    num3 = 30.0;
                System.Drawing.Rectangle r1 = new System.Drawing.Rectangle(0, 0, (int)num2, (int)num3);
                int dx = (r1.Left + r1.Left + r1.Width) / 2;
                int dy = (r1.Top + r1.Top + r1.Height) / 2;
                g.TranslateTransform((float)dx, (float)dy);
                g.ResetTransform();
                IList<FillGrid> fillGrids = fillPattern.GetFillGrids();
                System.Windows.Media.Color color = this.ColorBrush.Color;
                int a = (int)color.A;
                color = this.ColorBrush.Color;
                int r2 = (int)color.R;
                color = this.ColorBrush.Color;
                int g1 = (int)color.G;
                color = this.ColorBrush.Color;
                int b = (int)color.B;
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(a, r2, g1, b))
                {
                    Width = 1f / num1
                };
                foreach (FillGrid fillGrid in (IEnumerable<FillGrid>)fillGrids)
                {
                    float degree = (float)Utils.Utils.RadianToDegree(fillGrid.Angle);
                    float num4 = 1f;
                    IList<double> segments = fillGrid.GetSegments();
                    if (segments.Any<double>((Func<double, bool>)(s => s > 0.0)))
                    {
                        pen.DashPattern = segments.Select<double, float>((Func<double, float>)(s => Math.Max(float.Epsilon, Convert.ToSingle(s)))).ToArray<float>();
                        num4 = ((IEnumerable<float>)pen.DashPattern).Sum();
                    }
                    g.ResetTransform();
                    System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
                    matrix.Rotate(degree);
                    System.Drawing.Drawing2D.Matrix rayMatrix1 = new System.Drawing.Drawing2D.Matrix(1f, 0.0f, 0.0f, -1f, (float)dx, (float)dy);
                    rayMatrix1.Scale(num1, num1);
                    rayMatrix1.Translate((float)fillGrid.Origin.U, (float)fillGrid.Origin.V);
                    System.Drawing.Drawing2D.Matrix rayMatrix2 = rayMatrix1.Clone();
                    rayMatrix2.Multiply(matrix);
                    rayMatrix1.Multiply(matrix);
                    float offsetX = -10f * num4;
                    rayMatrix1.Translate(offsetX, 0.0f);
                    rayMatrix2.Translate(offsetX, 0.0f);
                    bool flag1 = true;
                    bool flag2 = true;
                    int num5 = 500;
                    double num6 = 0.0;
                    while (flag1 | flag2)
                    {
                        if (flag1 && this.LineIntersectsRect(rayMatrix1, r1))
                        {
                            g.Transform = rayMatrix1;
                            g.DrawLine(pen, new PointF(0.0f, 0.0f), new PointF(100f, 0.0f));
                        }
                        else
                            flag1 = false;
                        if (flag2 && this.LineIntersectsRect(rayMatrix2, r1))
                        {
                            g.Transform = rayMatrix2;
                            g.DrawLine(pen, new PointF(0.0f, 0.0f), new PointF(100f, 0.0f));
                        }
                        else
                            flag2 = false;
                        if (num5 != 0)
                        {
                            --num5;
                            rayMatrix1.Translate((float)fillGrid.Shift, (float)fillGrid.Offset);
                            rayMatrix2.Translate(-(float)fillGrid.Shift, -(float)fillGrid.Offset);
                            num6 += fillGrid.Shift;
                            if (Math.Abs(num6) > (double)Math.Abs(offsetX))
                            {
                                rayMatrix1.Translate(offsetX, 0.0f);
                                rayMatrix2.Translate(offsetX, 0.0f);
                                num6 = 0.0;
                            }
                        }
                        else
                            break;
                    }
                }
                stopwatch.Stop();
                g.ResetTransform();
                new System.Drawing.Pen(System.Drawing.Color.Black).Width = 1f / num1;
            }
            catch (Exception ex)
            {
            }
        }

        public bool LineIntersectsRect(System.Drawing.Drawing2D.Matrix rayMatrix, System.Drawing.Rectangle r)
        {
            System.Drawing.Drawing2D.Matrix matrix = rayMatrix.Clone();
            matrix.Translate(200f, 0.0f);
            return this.LineIntersectsRect(new System.Drawing.Point((int)rayMatrix.OffsetX, (int)rayMatrix.OffsetY), new System.Drawing.Point((int)matrix.OffsetX, (int)matrix.OffsetY), r);
        }

        public bool LineIntersectsRect(System.Drawing.Point p1, System.Drawing.Point p2, System.Drawing.Rectangle r)
        {
            return this.LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X, r.Y), new System.Drawing.Point(r.X + r.Width, r.Y)) || this.LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X + r.Width, r.Y), new System.Drawing.Point(r.X + r.Width, r.Y + r.Height)) || this.LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X + r.Width, r.Y + r.Height), new System.Drawing.Point(r.X, r.Y + r.Height)) || this.LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X, r.Y + r.Height), new System.Drawing.Point(r.X, r.Y)) || r.Contains(p1) && r.Contains(p2);
        }

        private bool LineIntersectsLine(System.Drawing.Point l1p1, System.Drawing.Point l1p2, System.Drawing.Point l2p1, System.Drawing.Point l2p2)
        {
            try
            {
                long num1 = (long)((l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X));
                if (num1 == 0L)
                    return false;
                long num2 = (long)((l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y)) / num1;
                long num3 = ((long)(l1p1.Y - l2p1.Y) * (long)(l1p2.X - l1p1.X) - (long)(l1p1.X - l2p1.X) * (long)(l1p2.Y - l1p1.Y)) / num1;
                return num2 >= 0L && num2 <= 1L && num3 >= 0L && num3 <= 1L;
            }
            catch (OverflowException ex)
            {
                return false;
            }
        }

        private double GetDistance(PointF point1, PointF point2)
        {
            double num1 = (double)point2.X - (double)point1.X;
            double num2 = (double)point2.Y - (double)point1.Y;
            return Math.Sqrt(num1 * num1 + num2 * num2);
        }

        // Property Changed Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
