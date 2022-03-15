namespace RaceControl.Core.Mvvm;

public class ViewModelBase : BindableBase
{
    protected readonly ILogger Logger;

    private bool _isBusy;

    protected ViewModelBase(ILogger logger)
    {
        Logger = logger;
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    protected void HandleNonCriticalError(Exception ex)
    {
        Logger.Warn(ex, "A non-critical error occurred.");
    }

    protected void HandleCriticalError(Exception ex)
    {
        Logger.Error(ex, "A critical error occurred.");
        Application.Current.Dispatcher.Invoke(() => MessageBoxHelper.ShowError(ex.Message));
        SetNotBusy();
    }

    protected void SetNotBusy()
    {
        IsBusy = false;
    }
}