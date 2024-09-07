using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace FillPatternEditor
{
    public partial class PatternListWindow : Window
    {
        private readonly UIApplication _uiApp;
        private List<FillPattern> _fillPatterns;

        // Enum representing the units of measurement
        public enum FillUnits
        {
            MM,
            Inch
        }

        // Enum representing the pattern target type
        public enum FillPatternTargetType
        {
            MODEL,
            DRAFTING
        }

        public PatternListWindow(UIApplication uiApp, UIDocument doc, string filepath)
        {
            InitializeComponent();
            _uiApp = uiApp;

            // Load patterns from the .pat file
            _fillPatterns = LoadPatternsFromFile(filepath);

            // Populate the PatternGrid with pattern names and preview images
            PopulatePatternGrid();
        }

        private void AddPreviewControl(ElementId viewId)
        {
            // Create the PreviewControl instance with the required constructor
            var previewControl = new PreviewControl(_uiApp.ActiveUIDocument.Document, viewId)
            {
                Width = double.NaN,  // Allow it to fill the available space
                Height = double.NaN  // Allow it to fill the available space
            };

            // Add the PreviewControl to the PreviewGrid as a child
            PreviewGrid.Children.Clear();  // Clear any existing children
            PreviewGrid.Children.Add(previewControl);
        }

        private List<FillPattern> LoadPatternsFromFile(string filePath)
        {
            var patterns = new List<FillPattern>();

            if (ReadPatterns(filePath))
            {
                foreach (var inputPattern in InputPatterns)
                {
                    var fillPattern = new FillPattern(inputPattern.Name, FillPatternTarget.Model, FillPatternHostOrientation.ToHost);

                    var grids = ParsePatternLines(inputPattern.PatLines);
                    fillPattern.SetFillGrids(grids);

                    patterns.Add(fillPattern);
                }
            }

            return patterns;
        }

        private List<FillGrid> ParsePatternLines(List<string> patternLines)
        {
            var grids = new List<FillGrid>();

            foreach (var line in patternLines)
            {
                var segments = line.Split(',').Select(double.Parse).ToArray();
                var fillGrid = CreateGrid(new UV(segments[0], segments[1]), segments[2], segments[3], segments[4], segments.Skip(5).ToArray());
                grids.Add(fillGrid);
            }

            return grids;
        }

        private FillGrid CreateGrid(UV origin, double offset, double angle, double shift, params double[] segments)
        {
            FillGrid fillGrid = new FillGrid
            {
                Origin = origin,
                Offset = offset,
                Angle = angle,
                Shift = shift
            };

            List<double> segmentsList = segments.ToList();
            fillGrid.SetSegments(segmentsList);

            return fillGrid;
        }

        private void PopulatePatternGrid()
        {
            PatternGrid.RowDefinitions.Clear();
            PatternGrid.ColumnDefinitions.Clear();

            PatternGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            PatternGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            PatternGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });

            for (int i = 0; i < _fillPatterns.Count; i++)
            {
                var pattern = _fillPatterns[i];

                PatternGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var patternName = new TextBlock { Text = pattern.Name };
                System.Windows.Controls.Grid.SetRow(patternName, i);
                System.Windows.Controls.Grid.SetColumn(patternName, 0);
                PatternGrid.Children.Add(patternName);
                string caption = patternName.Text;

                var swatchBitmap = DrawSwatch(pattern, 100, 75, 1.0, System.Drawing.Color.White);

                var patternPreview = new System.Windows.Controls.Image
                {
                    Source = BitmapToImageSource(swatchBitmap),
                    Width = 200,
                    Height = 100
                };
                System.Windows.Controls.Grid.SetRow(patternPreview, i);
                System.Windows.Controls.Grid.SetColumn(patternPreview, 1);
                PatternGrid.Children.Add(patternPreview);

                var editButton = new Button { Content = "Edit", Width = 60 };
                editButton.Click += EditButton_Click;
                System.Windows.Controls.Grid.SetRow(editButton, i);
                System.Windows.Controls.Grid.SetColumn(editButton, 2);
                PatternGrid.Children.Add(editButton);

                // Add selection handling
                patternPreview.MouseLeftButtonUp += (sender, e) => SelectPattern(pattern);
            }
        }

        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static Bitmap DrawSwatch(FillPattern pattern, int width, int height, double zoom, System.Drawing.Color backColor)
        {
            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(backColor);

            if (pattern.GetFillGrids().Count == 0)
            {
                // Draw a solid color if no grids are defined
                g.Clear(System.Drawing.Color.Black);
                g.DrawString("SOLID", new Font("Arial", 10), System.Drawing.Brushes.Yellow, 20f, 30f);
                return bitmap;
            }

            // Determine scaling factor
            double xMin = double.MaxValue, yMin = double.MaxValue;
            double xMax = double.MinValue, yMax = double.MinValue;

            // Calculate extents of the pattern
            foreach (var grid in pattern.GetFillGrids())
            {
                var origin = grid.Origin;
                UpdateExtents(origin.U, origin.V, ref xMin, ref xMax, ref yMin, ref yMax);

                foreach (var segment in grid.GetSegments())
                {
                    double length = Math.Abs(segment);
                    double cos = Math.Cos(grid.Angle);
                    double sin = Math.Sin(grid.Angle);

                    var endPoint = new PointD(origin.U + length * cos, origin.V + length * sin);
                    UpdateExtents(endPoint.X, endPoint.Y, ref xMin, ref xMax, ref yMin, ref yMax);
                }
            }

            double sx = Math.Max((xMax - xMin) * zoom / width, (yMax - yMin) * zoom / height);
            System.Drawing.Point offset = new System.Drawing.Point(width / 2, height / 2);

            // Now we draw the grids with transformations and dash pattern handling
            foreach (var grid in pattern.GetFillGrids())
            {
                var angle = grid.Angle;
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);

                var origin = grid.Origin;
                var segments = grid.GetSegments();
                if (segments.Count == 0)
                {
                    continue; // Skip if no segments
                }

                // Setup the transformation matrix for scaling and rotation
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate((float)RadianToGradus(angle));

                Matrix matrix = new Matrix(1, 0, 0, -1, offset.X, offset.Y); // Reflect along x-axis
                matrix.Scale((float)sx, (float)sx);
                matrix.Translate((float)origin.U, (float)origin.V);
                matrix.Multiply(rotateMatrix);

                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black);
                pen.DashPattern = segments.Select(s => Math.Max(float.Epsilon, Convert.ToSingle(s))).ToArray();

                g.Transform = matrix;

                // Calculate segment positions and draw lines
                bool drawing = true;
                int safety = 500; // Safety limit to avoid infinite loops
                double alternator = 0;
                float dashLength = pen.DashPattern.Sum();
                double shift = grid.Shift;
                double offsetTranslation = -10 * dashLength;

                // Draw lines along the grid shift and offset
                while (drawing && safety > 0)
                {
                    g.DrawLine(pen, new PointF(0, 0), new PointF(100, 0));  // Adjust the line length as necessary

                    if (!LineIntersectsRect(matrix, new System.Drawing.Rectangle(0, 0, width, height)))
                    {
                        drawing = false; // Stop drawing if the line is out of bounds
                    }

                    matrix.Translate((float)shift, (float)grid.Offset);

                    alternator += shift;
                    if (Math.Abs(alternator) > Math.Abs(offsetTranslation))
                    {
                        matrix.Translate((float)offsetTranslation, 0);
                        alternator = 0;
                    }

                    safety--;
                }

                // Reset the transformation to continue with the next grid
                g.ResetTransform();
            }

            g.Dispose();
            return bitmap;
        }

        private static double RadianToGradus(double radian)
        {
            return radian * 180 / Math.PI;
        }

        public static bool LineIntersectsRect(Matrix rayMatrix, System.Drawing.Rectangle r)
        {
            Matrix m = rayMatrix.Clone();
            m.Translate(200, 0); // Adjust this translation as necessary
            return LineIntersectsRect(
                new System.Drawing.Point((int)rayMatrix.OffsetX, (int)rayMatrix.OffsetY),
                new System.Drawing.Point((int)m.OffsetX, (int)m.OffsetY),
                r);
        }
        public static bool LineIntersectsRect(
          System.Drawing.Point p1,
          System.Drawing.Point p2,
          System.Drawing.Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X, r.Y), new System.Drawing.Point(r.X + r.Width, r.Y))
              || LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X + r.Width, r.Y), new System.Drawing.Point(r.X + r.Width, r.Y + r.Height))
              || LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X + r.Width, r.Y + r.Height), new System.Drawing.Point(r.X, r.Y + r.Height))
              || LineIntersectsLine(p1, p2, new System.Drawing.Point(r.X, r.Y + r.Height), new System.Drawing.Point(r.X, r.Y))
              || (r.Contains(p1) && r.Contains(p2));
        }
        private static bool LineIntersectsLine(
          System.Drawing.Point l1p1,
          System.Drawing.Point l1p2,
          System.Drawing.Point l2p1,
          System.Drawing.Point l2p2)
        {
            try
            {
                Int64 d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);
                if (d == 0) return false;

                Int64 q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
                Int64 r = q / d;

                Int64 q1 = (Int64)(l1p1.Y - l2p1.Y) * (Int64)(l1p2.X - l1p1.X);
                Int64 q2 = (Int64)(l1p1.X - l2p1.X) * (Int64)(l1p2.Y - l1p1.Y);

                q = q1 - q2;
                Int64 s = q / d;

                if (r < 0 || r > 1 || s < 0 || s > 1)
                    return false;

                return true;
            }
            catch (OverflowException err)
            {
                Debug.Print("----------------------------------");
                Debug.Print(err.Message);
                Debug.Print(l1p1.ToString());
                Debug.Print(l1p2.ToString());
                Debug.Print(l2p1.ToString());
                Debug.Print(l2p2.ToString());
                return false;
            }
        }
        // Helper method to update the min/max extents of the pattern
        private static void UpdateExtents(double x, double y, ref double xMin, ref double xMax, ref double yMin, ref double yMax)
        {
            if (x < xMin) xMin = x;
            if (x > xMax) xMax = x;
            if (y < yMin) yMin = y;
            if (y > yMax) yMax = y;
        }


        private void SelectPattern(FillPattern pattern)
        {
            // Create a new view and wall, and get the view's ID
            ElementId viewId = CreatePreviewElementWithPattern(pattern);

            // Add the PreviewControl to the PreviewGrid
            AddPreviewControl(viewId);
        }

        private ElementId CreatePreviewElementWithPattern(FillPattern pattern)
        {
            Document doc = _uiApp.ActiveUIDocument.Document;

            using (Transaction trans = new Transaction(doc, "Create Preview Element"))
            {
                trans.Start();

                // Create a new 3D view for the preview
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);

                if (viewFamilyType == null)
                {
                    throw new InvalidOperationException("No 3D view family type found.");
                }

                View3D view = View3D.CreateIsometric(doc, viewFamilyType.Id);
                view.Name = "Preview View - " + pattern.Name;

                // Create a wall in the new view
                Level level = GetLevel(doc);

                Wall wall = Wall.Create(doc, Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)), level.Id, false);

                // Create a FillPatternElement
                FillPatternElement fpe = FillPatternElement.Create(doc, pattern);

                // Set the wall's projection fill pattern in the new view
                OverrideGraphicSettings ORGS = new OverrideGraphicSettings();
                ORGS.SetSurfaceForegroundPatternId(fpe.Id);
                view.SetElementOverrides(wall.Id, ORGS);

                trans.Commit();

                // Return the View3D element's ID to be used in the PreviewControl
                return view.Id;
            }
        }

        private Level GetLevel(Document doc)
        {
            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            if (levels.Count == 0)
            {
                throw new InvalidOperationException("No levels found in the project.");
            }

            // Use the first available level as a fallback
            return levels.First();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for edit functionality
        }

        private bool ReadPatterns(string sFile)
        {
            var PatName = "";
            var PatComment = "";
            var PatUnits = FillUnits.MM;  // Default unit
            var FT = FillPatternTargetType.MODEL;  // Default target type
            List<string> PatLines = null;
            var fromHatchKit = false;
            var flag = true;

            try
            {
                using (StreamReader streamReader = File.OpenText(sFile))
                {
                    string str3;
                    while ((str3 = streamReader.ReadLine()) != null)
                    {
                        if (str3.Trim() != "")
                        {
                            string str4 = str3.Trim();
                            if (str4.Substring(0, 1) == "*")
                            {
                                // New pattern detected, add the previous pattern if it exists
                                if (PatName != "" && PatLines != null && PatLines.Count > 0)
                                {
                                    AddPatternToInputTable(PatName, PatComment, FT, PatUnits, new List<string>(PatLines));
                                }

                                // Initialize a new list for the new pattern
                                PatLines = new List<string>();

                                string str5 = str4.Substring(1);
                                if (str5.Contains(","))
                                {
                                    int length2 = str5.IndexOf(",");
                                    PatName = str5.Substring(0, length2).Trim();
                                    PatComment = str5.Substring(length2 + 1).Trim();
                                }
                                else
                                {
                                    PatName = str5;
                                    PatComment = "";
                                }
                            }
                            else if (str4.Substring(0, 1) == ";")
                            {
                                if (str4.ToUpper().Contains(";%"))
                                {
                                    if (str4.ToUpper().Contains("UNITS"))
                                    {
                                        int num7 = str4.IndexOf("=");
                                        var unitsValue = str4.Substring(num7 + 1).Trim().ToUpper();
                                        PatUnits = unitsValue == "MM" ? FillUnits.MM : FillUnits.Inch;
                                    }
                                    else if (str4.ToUpper().Contains("TYPE"))
                                    {
                                        int num8 = str4.IndexOf("=");
                                        var targetType = str4.Substring(num8 + 1).Trim().ToUpper();
                                        FT = targetType == "MODEL" ? FillPatternTargetType.MODEL : FillPatternTargetType.DRAFTING;
                                    }
                                }
                            }
                            else if (PatLines != null)
                            {
                                // Add lines to the current pattern's PatLines list
                                PatLines.Add(str4);
                            }
                        }
                    }
                    // Add the last pattern if any
                    if (PatName != "" && PatLines != null && PatLines.Count > 0)
                    {
                        AddPatternToInputTable(PatName, PatComment, FT, PatUnits, new List<string>(PatLines));
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Cannot read file", "Error: " + ex.Message);
                return false;
            }
            return flag;
        }

        private bool AddPatternToInputTable(string patName, string patComment, FillPatternTargetType target, FillUnits units, List<string> patLines)
        {
            try
            {
                InputPattern inputPattern = new InputPattern
                {
                    Name = patName,
                    Comment = patComment,
                    Target = target,
                    Units = units,
                    PatLines = patLines
                };

                InputPatterns.Add(inputPattern);
                return true;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to add pattern: {ex.Message}");
                return false;
            }
        }

        public class InputPattern
        {
            public string Name { get; set; }
            public string Comment { get; set; }
            public FillPatternTargetType Target { get; set; }
            public FillUnits Units { get; set; }
            public List<string> PatLines { get; set; }
        }

        private List<InputPattern> InputPatterns = new List<InputPattern>();
    }
}

// PointD class for 2D point operations
public class PointD
{
    public double X { get; set; }
    public double Y { get; set; }

    public PointD(double x, double y)
    {
        X = x;
        Y = y;
    }

    // Converts world coordinates to screen coordinates
    public System.Drawing.Point ToScreen(double scaleFactor, System.Drawing.Point offset)
    {
        return new System.Drawing.Point(
            offset.X + (int)((X) / scaleFactor),
            offset.Y - (int)((Y) / scaleFactor)
        );
    }
}
