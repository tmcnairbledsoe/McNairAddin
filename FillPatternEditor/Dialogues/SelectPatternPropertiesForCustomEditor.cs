
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FillPatternEditor.Dialogues
{
    /// <summary>
    /// A custom editor control for selecting pattern properties.
    /// </summary>
    public class SelectPatternPropertiesForCustomEditor : UserControl, IComponentConnector
    {
        private bool _contentLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectPatternPropertiesForCustomEditor"/> class.
        /// </summary>
        public SelectPatternPropertiesForCustomEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the XAML components for this control.
        /// </summary>
        public void InitializeComponent()
        {
            if (_contentLoaded)
                return;

            _contentLoaded = true;
            Application.LoadComponent(this, new Uri("/CustomPatterns;component/views/dialogs/selectpatternpropertiesforcustomeditor.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Part of the IComponentConnector interface. Connects the component with its XAML.
        /// </summary>
        /// <param name="connectionId">The ID of the connection point.</param>
        /// <param name="target">The target object to connect.</param>
        [DebuggerNonUserCode]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            _contentLoaded = true;
        }
    }
}
