using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FillPatternEditor.Views
{
    public partial class PatFromElementsWindow : Window
    {
        public PatFromElementsWindow()
        {
            InitializeComponent();
        }

        // Event handler for the Accept button
        private void BtAccept_OnClick(object sender, RoutedEventArgs e)
        {
            // Set the dialog result to true (accepted)
            DialogResult = true;
        }

        // Event handler for the Cancel button
        private void BtCancel_OnClick(object sender, RoutedEventArgs e)
        {
            // Set the dialog result to false (cancelled)
            DialogResult = false;
        }
    }
}
