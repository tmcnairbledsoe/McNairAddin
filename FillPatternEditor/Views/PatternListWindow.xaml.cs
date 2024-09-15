using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FillPatternEditor.ViewModels;

namespace FillPatternEditor.Views
{
    public partial class PatternListWindow : Window, INotifyPropertyChanged
    {
        public PatternListWindow()
        {
            InitializeComponent();
            this.Title = "Pattern Editor"; // Set a custom title directly or use bindings for localization.
            this.DataContext = new PatternEditorViewModel(); // Set the DataContext to your view model.
            this.Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (this.DataContext is PatternEditorViewModel dataContext)
            {
                // Ensure proper resource cleanup if needed.
                dataContext.PickCrosshairWindow?.Close();
            }
        }

        // Implement INotifyPropertyChanged for data bindings
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
