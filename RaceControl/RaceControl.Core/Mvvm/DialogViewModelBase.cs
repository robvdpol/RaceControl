using Prism.Commands;
using Prism.Services.Dialogs;
using System.Windows.Input;

namespace RaceControl.Core.Mvvm;

// ReSharper disable MemberCanBeProtected.Global
public abstract class DialogViewModelBase : ViewModelBase, IDialogAware
{
    private ICommand _closeWindowCommand;
    private bool _canClose;

    protected DialogViewModelBase(ILogger logger) : base(logger)
    {
    }

    public abstract string Title { get; }

    public ICommand CloseWindowCommand => _closeWindowCommand ??= new DelegateCommand(RaiseRequestClose).ObservesCanExecute(() => CanClose);

    protected bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }

    public event Action<IDialogResult> RequestClose;

    public virtual void OnDialogOpened(IDialogParameters parameters)
    {
        CanClose = true;
    }

    public virtual void OnDialogClosed()
    {
        CanClose = false;
    }

    public bool CanCloseDialog()
    {
        return CanClose;
    }

    protected void RaiseRequestClose(ButtonResult result, IDialogParameters parameters = null)
    {
        RaiseRequestClose(new DialogResult(result, parameters));
    }

    protected void RaiseRequestClose()
    {
        RaiseRequestClose(null);
    }

    private void RaiseRequestClose(IDialogResult dialogResult)
    {
        RequestClose?.Invoke(dialogResult);
    }
}