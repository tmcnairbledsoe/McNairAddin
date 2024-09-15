using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FillPatternEditor.Views
{
    public partial class PickCrosshairWindow : Window
    {
        private const int WH_MOUSE_LL = 14;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelMouseProc _proc;
        private Autodesk.Revit.DB.Rectangle _viewRectangle;

        public PickCrosshairWindow()
        {
            InitializeComponent();

            if (Screen.AllScreens.Length > 1)
            {
                System.Drawing.Rectangle rectangle = ScreenTotalSize();
                this.Width = rectangle.Width;
                this.Height = rectangle.Height;
                this.Left = 0.0;
                this.Top = 0.0;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
            this.Closing += (sender, args) => this.StopShowCrosshair();
        }

        public void InitViewRectangle(Autodesk.Revit.DB.Rectangle viewRect)
        {
            this._viewRectangle = viewRect;
        }

        public void StartShowCrosshair() => this.SubscribeGlobal();

        public void StopShowCrosshair()
        {
            this.ClearCanvas();
            this.Unsubscribe();
        }

        public void ClearCanvas() => this.Canvas.Children.Clear();

        private void SubscribeGlobal()
        {
            this.Unsubscribe();
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void Unsubscribe()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                Point point = new Point(hookStruct.pt.x, hookStruct.pt.y);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ClearCanvas();
                    if (_viewRectangle != null && IsInViewRectangle(new System.Drawing.Point(hookStruct.pt.x, hookStruct.pt.y)))
                    {
                        foreach (var line in GetLinesByViewRectangle(new System.Drawing.Point(hookStruct.pt.x, hookStruct.pt.y)))
                        {
                            Canvas.Children.Add(line);
                        }
                    }
                    else
                    {
                        foreach (var line in GetLinesByScreen(new System.Drawing.Point(hookStruct.pt.x, hookStruct.pt.y)))
                        {
                            Canvas.Children.Add(line);
                        }
                    }
                });
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static System.Drawing.Rectangle ScreenTotalSize()
        {
            System.Drawing.Rectangle result = System.Drawing.Rectangle.Empty;
            foreach (Screen screen in Screen.AllScreens)
            {
                result = System.Drawing.Rectangle.Union(result, screen.Bounds);
            }
            return result;
        }

        private bool IsInViewRectangle(System.Drawing.Point point)
        {
            return _viewRectangle != null &&
                   point.X >= _viewRectangle.Left && point.X <= _viewRectangle.Right &&
                   point.Y >= _viewRectangle.Top && point.Y <= _viewRectangle.Bottom;
        }

        private IEnumerable<Line> GetLinesByScreen(System.Drawing.Point point)
        {
            var screenSize = ScreenTotalSize();

            yield return CreateLine(0, point.Y, point.X - 10, point.Y); // Left Horizontal
            yield return CreateLine(point.X + 10, point.Y, screenSize.Width, point.Y); // Right Horizontal
            yield return CreateLine(point.X, point.Y + 10, point.X, screenSize.Height); // Top Vertical
            yield return CreateLine(point.X, 0, point.X, point.Y - 10); // Bottom Vertical
        }

        private IEnumerable<Line> GetLinesByViewRectangle(System.Drawing.Point point)
        {
            yield return CreateLine(_viewRectangle.Left, point.Y, point.X - 10, point.Y); // Left Horizontal
            yield return CreateLine(point.X + 10, point.Y, _viewRectangle.Right, point.Y); // Right Horizontal
            yield return CreateLine(point.X, point.Y + 10, point.X, _viewRectangle.Bottom); // Top Vertical
            yield return CreateLine(point.X, _viewRectangle.Top, point.X, point.Y - 10); // Bottom Vertical
        }

        private Line CreateLine(double x1, double y1, double x2, double y2)
        {
            return new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Red,
                StrokeDashArray = new DoubleCollection { 10, 5 }
            };
        }

        // P/Invoke declarations
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
