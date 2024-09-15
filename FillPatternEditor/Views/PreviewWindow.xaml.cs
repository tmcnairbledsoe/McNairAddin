using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
    public partial class PreviewWindow : Window
    {
        private readonly Document _document;
        private readonly ElementId _viewId;

        public PreviewWindow(Document document, ElementId viewId)
        {
            _document = document;
            _viewId = viewId;

            InitializeComponent();
            this.ContentRendered += new EventHandler(this.OnContentRendered);
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            this.MainGrid.Children.Add((UIElement)new PreviewControl(this._document, this._viewId));
        }
    }
}
