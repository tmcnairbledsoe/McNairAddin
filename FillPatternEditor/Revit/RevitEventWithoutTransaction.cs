using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FillPatternEditor.Revit
{// This class handles Revit external events without a transaction
    public class RevitEventWithoutTransaction : IExternalEventHandler
    {
        private Action _doAction;
        private readonly ExternalEvent _externalEvent;
        private bool _skipFailures;

        // Constructor to initialize the external event
        public RevitEventWithoutTransaction()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        // Method to run an action and specify if failures should be skipped
        public void Run(Action doAction, bool skipFailures)
        {
            _doAction = doAction;
            _skipFailures = skipFailures;
            _externalEvent.Raise();
        }

        // Executes the action within the Revit environment
        public void Execute(UIApplication app)
        {
            try
            {
                // If no action is defined, exit
                if (_doAction == null) return;

                // Attach failure processing event if needed
                if (_skipFailures)
                {
                    app.Application.FailuresProcessing += Application_FailuresProcessing;
                }

                // Perform the action
                _doAction();

                // Detach failure processing event if it was attached
                if (_skipFailures)
                {
                    app.Application.FailuresProcessing -= Application_FailuresProcessing;
                }
            }
            catch (Exception ex)
            {
                // Show the exception message in a message box
                MessageBox.Show($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");

                // Detach failure processing event if skipping failures
                if (_skipFailures)
                {
                    app.Application.FailuresProcessing -= Application_FailuresProcessing;
                }
            }
        }

        // Handles failure processing by deleting warnings
        private static void Application_FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            var failureAccessor = e.GetFailuresAccessor();

            // If there are no failure messages, do nothing
            if (!failureAccessor.GetFailureMessages().Any()) return;

            // Delete all warnings
            failureAccessor.DeleteAllWarnings();

            // Set the result to proceed with the next steps in Revit
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        // Returns the name of the event handler
        public string GetName() => nameof(RevitEventWithoutTransaction);
    }
}
