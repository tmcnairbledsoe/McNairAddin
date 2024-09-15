using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace FillPatternEditor.Dialogues
{
    /// <summary>
    /// A class representing the settings used for Custom Dialogs.
    /// </summary>
    public class CustomDialogSettings
    {
        public CustomDialogSettings()
        {
            OwnerCanCloseWithDialog = false;
            AffirmativeButtonText = "OK";
            NegativeButtonText = "Cancel";
            AnimateShow = AnimateHide = true;
            MaximumBodyHeight = double.NaN;
            DefaultText = string.Empty;
            DefaultButtonFocus = MessageDialogResult.Negative;
            CancellationToken = CancellationToken.None;
            DialogTitleFontSize = double.NaN;
            DialogMessageFontSize = double.NaN;
            DialogResultOnCancel = null;
        }

        public bool OwnerCanCloseWithDialog { get; set; }
        public string AffirmativeButtonText { get; set; }
        public bool AnimateHide { get; set; }
        public bool AnimateShow { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public MessageDialogResult DefaultButtonFocus { get; set; }
        public string DefaultText { get; set; }
        public double DialogMessageFontSize { get; set; }
        public MessageDialogResult? DialogResultOnCancel { get; set; }
        public double DialogTitleFontSize { get; set; }
        public string FirstAuxiliaryButtonText { get; set; }
        public double MaximumBodyHeight { get; set; }
        public string NegativeButtonText { get; set; }
        public string SecondAuxiliaryButtonText { get; set; }
        public bool FullHeight { get; set; }
    }

    /// <summary>
    /// A base class for dialogs that handles some core functionality like animations.
    /// </summary>
    public abstract class BaseDialog : ContentControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(BaseDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogTopProperty = DependencyProperty.Register(
            nameof(DialogTop), typeof(object), typeof(BaseDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogBottomProperty = DependencyProperty.Register(
            nameof(DialogBottom), typeof(object), typeof(BaseDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogTitleFontSizeProperty = DependencyProperty.Register(
            nameof(DialogTitleFontSize), typeof(double), typeof(BaseDialog), new PropertyMetadata(26.0));

        public static readonly DependencyProperty DialogMessageFontSizeProperty = DependencyProperty.Register(
            nameof(DialogMessageFontSize), typeof(double), typeof(BaseDialog), new PropertyMetadata(15.0));

        public CustomDialogSettings DialogSettings { get; private set; }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object DialogTop
        {
            get => GetValue(DialogTopProperty);
            set => SetValue(DialogTopProperty, value);
        }

        public object DialogBottom
        {
            get => GetValue(DialogBottomProperty);
            set => SetValue(DialogBottomProperty, value);
        }

        public double DialogTitleFontSize
        {
            get => (double)GetValue(DialogTitleFontSizeProperty);
            set => SetValue(DialogTitleFontSizeProperty, value);
        }

        public double DialogMessageFontSize
        {
            get => (double)GetValue(DialogMessageFontSizeProperty);
            set => SetValue(DialogMessageFontSizeProperty, value);
        }

        protected Window ParentDialogWindow { get; set; }
        protected Window OwningWindow { get; set; }
        internal SizeChangedEventHandler SizeChangedHandler { get; set; }

        protected BaseDialog(Window owningWindow, CustomDialogSettings settings)
        {
            Initialize(owningWindow, settings);
        }

        private void Initialize(Window owningWindow, CustomDialogSettings settings)
        {
            OwningWindow = owningWindow;
            DialogSettings = settings ?? new CustomDialogSettings();
            Loaded += BaseDialogLoaded;
        }

        private void BaseDialogLoaded(object sender, RoutedEventArgs e) => OnLoaded();

        protected virtual void OnLoaded() { }

        public Task WaitForLoadAsync()
        {
            Dispatcher.VerifyAccess();

            if (IsLoaded)
                return Task.CompletedTask;

            if (!DialogSettings.AnimateShow)
                Opacity = 1.0;

            var tcs = new TaskCompletionSource<object>();
            RoutedEventHandler handler = null;
            handler = (_, _) =>
            {
                Loaded -= handler;
                Focus();
                tcs.SetResult(null);
            };
            Loaded += handler;
            return tcs.Task;
        }

        public Task WaitForCloseAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            if (DialogSettings.AnimateHide)
            {
                var closingStoryboard = TryFindResource("DialogCloseStoryboard") as Storyboard;
                if (closingStoryboard == null)
                    throw new InvalidOperationException("Unable to find closing storyboard.");

                EventHandler handler = null;
                handler = (_, _) =>
                {
                    closingStoryboard.Completed -= handler;
                    tcs.SetResult(null);
                };

                closingStoryboard = closingStoryboard.Clone();
                closingStoryboard.Completed += handler;
                closingStoryboard.Begin(this);
            }
            else
            {
                Opacity = 0.0;
                tcs.SetResult(null);
            }
            return tcs.Task;
        }

        protected override AutomationPeer OnCreateAutomationPeer() => new FrameworkElementAutomationPeer(this);
    }

    /// <summary>
    /// A custom dialog class that inherits base functionality from BaseDialog.
    /// </summary>
    public class CustomDialog : BaseDialog
    {
        public CustomDialog() : this(null, null) { }

        public CustomDialog(Window parentWindow) : this(parentWindow, null) { }

        public CustomDialog(CustomDialogSettings settings) : this(null, settings) { }

        public CustomDialog(Window parentWindow, CustomDialogSettings settings) : base(parentWindow, settings)
        {
            PreviewKeyDown += (sender, args) =>
            {
                if (args.Key == Key.Escape && Window.GetWindow(this) is Window window)
                {
                    window.Hide();
                }
            };
        }

        public void Close()
        {
            Window.GetWindow(this).Hide();
        }
    }

    public enum MessageDialogResult
    {
        Affirmative,
        Negative
    }
}
