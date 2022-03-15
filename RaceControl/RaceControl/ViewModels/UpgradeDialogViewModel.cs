namespace RaceControl.ViewModels;

public class UpgradeDialogViewModel : DialogViewModelBase
{
    private ICommand _closeCommand;

    private Release _release;

    public UpgradeDialogViewModel(ILogger logger) : base(logger)
    {
    }

    public override string Title => Release?.Name;

    public ICommand CloseCommand => _closeCommand ??= new DelegateCommand<bool?>(CloseExecute);

    public Release Release
    {
        get => _release;
        set => SetProperty(ref _release, value);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        Release = parameters.GetValue<Release>(ParameterNames.Release);
        base.OnDialogOpened(parameters);
    }

    private void CloseExecute(bool? upgrade)
    {
        RaiseRequestClose(upgrade.HasValue && upgrade.Value ? ButtonResult.OK : ButtonResult.Cancel);
    }
}