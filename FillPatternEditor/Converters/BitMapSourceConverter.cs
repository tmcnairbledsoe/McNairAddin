using System;
using System.Drawing; // For Bitmap and Icon
using System.Runtime.InteropServices; // For external method calls to gdi32.dll
using System.Windows;
using System.Windows.Media.Imaging; // For BitmapSource

namespace FillPatternEditor.Converters
{
    public class BitmapSourceConverter
    {
        // Import the DeleteObject function from gdi32.dll to release resources used by GDI objects.
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a WPF BitmapSource.
        /// This allows interoperability between GDI+ (used by System.Drawing) and WPF (which uses BitmapSource).
        /// </summary>
        /// <param name="image">The Bitmap image to convert.</param>
        /// <returns>A BitmapSource that can be used in WPF.</returns>
        public static BitmapSource ConvertFromImage(Bitmap image)
        {
            // Lock the bitmap to avoid multithreading issues during the conversion.
            lock (image)
            {
                // Create a GDI HBitmap from the Bitmap object.
                IntPtr hbitmap = image.GetHbitmap();
                try
                {
                    // Create a BitmapSource from the HBitmap.
                    // Int32Rect.Empty specifies that the entire image will be used.
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    // Clean up and delete the HBitmap to release unmanaged resources.
                    DeleteObject(hbitmap);
                }
            }
        }

        /// <summary>
        /// Converts a System.Drawing.Icon to a WPF BitmapSource.
        /// </summary>
        /// <param name="icon">The Icon to convert.</param>
        /// <returns>A BitmapSource that can be used in WPF.</returns>
        public static BitmapSource ConvertFromIcon(Icon icon)
        {
            try
            {
                // Create a BitmapSource from the Icon's handle.
                // Int32Rect specifies the dimensions of the icon.
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    new Int32Rect(0, 0, icon.Width, icon.Height),
                    BitmapSizeOptions.FromWidthAndHeight(icon.Width, icon.Height));
            }
            finally
            {
                // Clean up the Icon handle and dispose the icon to release resources.
                DeleteObject(icon.Handle);
                icon.Dispose();
                icon = null;
            }
        }
    }
}
