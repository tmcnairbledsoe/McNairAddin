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
{
    // Handles external Revit events
    public class RevitEvent : IExternalEventHandler
    {
        private Action _doAction;              // The action to perform during the event
        private Document _doc;                 // The Revit document context
        private readonly ExternalEvent _exEvent; // The Revit external event instance
        private bool _skipFailures;            // Flag to determine if failures should be skipped
        private string _transactionName;       // Name of the transaction

        // Constructor to initialize the ExternalEvent
        public RevitEvent()
        {
            _exEvent = ExternalEvent.Create(this);
        }

        // Method to raise the event and specify the action to perform
        public void Run(Action doAction, bool skipFailures, Document doc = null, string transactionName = null)
        {
            _doAction = doAction;
            _skipFailures = skipFailures;
            _doc = doc;
            _transactionName = transactionName;
            _exEvent.Raise();
        }

        // Executes the action inside a Revit transaction
        public void Execute(UIApplication app)
        {
            try
            {
                if (_doAction == null) return;  // No action specified, return

                if (_doc == null)
                    _doc = app.ActiveUIDocument.Document;  // Use active document if none specified

                // If skipping failures, hook up the failure processing event
                if (_skipFailures)
                    app.Application.FailuresProcessing += Application_FailuresProcessing;

                // Start a new transaction
                using (Transaction transaction = new Transaction(_doc, _transactionName ?? "Fill Patterns"))
                {
                    transaction.Start();  // Begin transaction
                    _doAction();          // Execute the action
                    transaction.Commit();  // Commit transaction
                }

                // If skipping failures, unhook the failure processing event
                if (_skipFailures)
                    app.Application.FailuresProcessing -= Application_FailuresProcessing;
            }
            catch (Exception ex)
            {
                // Show the error message if an exception occurs
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);

                // Unhook failure processing if an exception occurs while skipping failures
                if (_skipFailures)
                    app.Application.FailuresProcessing -= Application_FailuresProcessing;
            }
        }

        // Event handler for failure processing
        private static void Application_FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            var failuresAccessor = e.GetFailuresAccessor();
            if (!failuresAccessor.GetFailureMessages().Any()) return;  // No failure messages, return

            failuresAccessor.DeleteAllWarnings();   // Delete all warnings
            e.SetProcessingResult(FailureProcessingResult.Continue);  // Continue processing
        }

        // Gets the name of the event handler
        public string GetName() => nameof(RevitEvent);
    }
}
