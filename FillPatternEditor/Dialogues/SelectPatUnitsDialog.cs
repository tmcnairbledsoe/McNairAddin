using FillPatternEditor.Enums;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FillPatternEditor.Dialogues
{
    public class SelectPatUnitsDialog : UserControl, IComponentConnector
    {
        // Buttons for selecting units of measurement
        internal Button BtMm;
        internal Button BtInch;

        // Tracks whether the content is loaded
        private bool _contentLoaded;

        // Constructor
        public SelectPatUnitsDialog()
        {
            InitializeComponent();
        }

        // Event handler for MM button click
        private void BtMm_OnClick(object sender, RoutedEventArgs e)
        {
            ((SelectPatUnitsViewModel)this.DataContext).PatUnits = PatUnits.MM;
        }

        // Event handler for Inch button click
        private void BtInch_OnClick(object sender, RoutedEventArgs e)
        {
            ((SelectPatUnitsViewModel)this.DataContext).PatUnits = PatUnits.INCH;
        }

        // Auto-generated method to load the component from XAML
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "8.0.7.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded) // Prevents multiple initializations
                return;
            _contentLoaded = true;
            // Loads the XAML for this control
            Application.LoadComponent(this, new Uri("/mprPatterns_2025;component/views/dialogs/selectpatunitsdialog.xaml", UriKind.Relative));
        }

        // WPF generated connection method for UI elements
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "8.0.7.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.BtMm = (Button)target;
                    this.BtMm.Click += new RoutedEventHandler(BtMm_OnClick);
                    break;
                case 2:
                    this.BtInch = (Button)target;
                    this.BtInch.Click += new RoutedEventHandler(BtInch_OnClick);
                    break;
                default:
                    _contentLoaded = true;
                    break;
            }
        }
    }
}
