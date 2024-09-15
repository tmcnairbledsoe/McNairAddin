using CommunityToolkit.Mvvm.ComponentModel;
using FillPatternEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FillPatternEditor.Dialogues
{
    public class SelectPatPatternDialogViewModel : ObservableObject
    {
        // Backing fields
        private bool _isEnableAccept;
        private PatPattern _selectedPatPattern;

        // Constructor
        public SelectPatPatternDialogViewModel(
            Action<SelectPatPatternDialogViewModel> closeHandler,
            IEnumerable<PatPattern> patPatterns)
        {
            // Initialize Accept and Cancel commands
            this.AcceptCommand = new Commands.RelayCommand(() =>
            {
                this.IsCanceled = false;
                closeHandler(this);
            });

            this.CancelCommand = new Commands.RelayCommand(() =>
            {
                this.IsCanceled = true;
                closeHandler(this);
            });

            // Store available patterns
            this.PatPatterns = patPatterns;
        }

        // Command for accepting the selection
        public ICommand AcceptCommand { get; }

        // Command for canceling the dialog
        public ICommand CancelCommand { get; }

        // Property indicating whether the user canceled the dialog
        public bool IsCanceled { get; private set; }

        // Property to enable or disable the Accept button
        public bool IsEnableAccept
        {
            get => this._isEnableAccept;
            set
            {
                if (this._isEnableAccept != value)
                {
                    this._isEnableAccept = value;
                    OnPropertyChanged(nameof(IsEnableAccept));
                }
            }
        }

        // Collection of available patterns
        public IEnumerable<PatPattern> PatPatterns { get; }

        // Property representing the selected pattern
        public PatPattern SelectedPatPattern
        {
            get => this._selectedPatPattern;
            set
            {
                if (this._selectedPatPattern != value)
                {
                    this._selectedPatPattern = value;
                    // Enable the accept button only when a pattern is selected
                    this.IsEnableAccept = value != null;
                    OnPropertyChanged(nameof(SelectedPatPattern));
                }
            }
        }
    }
}
