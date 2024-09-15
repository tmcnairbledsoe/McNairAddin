
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FillPatternEditor.Dialogues
{
    public class SelectPatPatternDialog : UserControl, IComponentConnector
    {
        // Tracks whether the content is loaded
        private bool _contentLoaded;

        // Constructor
        public SelectPatPatternDialog()
        {
            InitializeComponent();
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
            Application.LoadComponent(this, new Uri("/mprPatterns_2025;component/views/dialogs/selectpatpatterndialog.xaml", UriKind.Relative));
        }

        // WPF generated connection method for UI elements
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "8.0.7.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            // Marks content as loaded
            _contentLoaded = true;
        }
    }
}
