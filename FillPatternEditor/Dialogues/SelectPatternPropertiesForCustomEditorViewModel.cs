using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using FillPatternEditor.Enums;
using FillPatternEditor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FillPatternEditor.Dialogues
{
    /// <summary>
    /// ViewModel for selecting properties of a custom pattern editor.
    /// </summary>
    public class SelectPatternPropertiesForCustomEditorViewModel : ObservableObject
    {
        private FillPatternTarget _fillPatternTarget;
        private PatUnits _patUnits;

        /// <summary>
        /// Constructor for initializing the ViewModel with an action to close the dialog.
        /// </summary>
        /// <param name="closeHandler">Action that will handle the closing of the dialog.</param>
        public SelectPatternPropertiesForCustomEditorViewModel(Action<SelectPatternPropertiesForCustomEditorViewModel> closeHandler)
        {
            AcceptCommand = new RelayCommand(() =>
            {
                IsCanceled = false;
                closeHandler(this);
            });

            CancelCommand = new RelayCommand(() =>
            {
                IsCanceled = true;
                closeHandler(this);
            });
        }

        /// <summary>
        /// Gets or sets whether the action was canceled.
        /// </summary>
        public bool IsCanceled { get; private set; }

        /// <summary>
        /// Gets the command to accept and close the dialog.
        /// </summary>
        public ICommand AcceptCommand { get; }

        /// <summary>
        /// Gets the command to cancel and close the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Gets or sets the selected pattern units.
        /// </summary>
        public PatUnits PatUnits
        {
            get => _patUnits;
            set
            {
                if (_patUnits != value)
                {
                    _patUnits = value;
                    OnPropertyChanged(nameof(PatUnits));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected pattern target (Drafting or Model).
        /// </summary>
        public FillPatternTarget FillPatternTarget
        {
            get => _fillPatternTarget;
            set
            {
                if (_fillPatternTarget != value)
                {
                    _fillPatternTarget = value;
                    OnPropertyChanged(nameof(FillPatternTarget));
                }
            }
        }
    }
}
