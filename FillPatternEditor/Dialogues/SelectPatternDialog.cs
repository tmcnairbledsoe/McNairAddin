
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FillPatternEditor.Dialogues
{
    public class SelectPatternsDialog : UserControl, IComponentConnector
    {
        // Tracks whether the component is already loaded to avoid re-initialization
        private bool _contentLoaded;

        // Constructor
        public SelectPatternsDialog()
        {
            // Initialize the dialog component and apply style/language settings
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
            // Loads the XAML for the dialog
            Application.LoadComponent(this, new Uri("/mprPatterns_2025;component/views/dialogs/selectpatternsdialog.xaml", UriKind.Relative));
        }

        // WPF generated connection method
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "8.0.7.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target) => _contentLoaded = true;
    }
}
