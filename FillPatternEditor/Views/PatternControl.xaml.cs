using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using FillPatternEditor.Models;
using System.ComponentModel;

namespace FillPatternEditor.Views
{
    public partial class PatternControl : UserControl, INotifyPropertyChanged
    {
        public PatternControl()
        {
            InitializeComponent();
            // You can initialize resources manually if needed.
            //this.DataContextChanged += OnDataContextChanged;
            //this.GridPatternControls.Visibility = Visibility.Hidden;
        }

        //// Handle DataContext changes and visibility control.
        //private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (DataContext is CustomPattern)
        //    {
        //        GridPatternControls.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        GridPatternControls.Visibility = Visibility.Hidden;
        //    }
        //}

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
