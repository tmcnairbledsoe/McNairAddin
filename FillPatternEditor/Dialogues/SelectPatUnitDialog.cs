using FillPatternEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FillPatternEditor.Commands;

namespace FillPatternEditor.Dialogues
{
    public class SelectPatUnitsViewModel
    {
        public SelectPatUnitsViewModel(Action<SelectPatUnitsViewModel> closeHandler, string message)
        {
            SelectPatUnitsViewModel patUnitsViewModel = this;
            this.Message = message;
            this.CloseCommand = (ICommand)new RelayCommand((Action)(() => closeHandler(patUnitsViewModel)));
        }

        public string Message { get; }

        public PatUnits PatUnits { get; set; }

        public ICommand CloseCommand { get; }
    }
}
