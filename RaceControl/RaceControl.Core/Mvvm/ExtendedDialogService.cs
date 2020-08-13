using Prism.Ioc;
using Prism.Services.Dialogs;
using System;

namespace RaceControl.Core.Mvvm
{
    public class ExtendedDialogService : DialogService, IExtendedDialogService
    {
        public ExtendedDialogService(IContainerExtension containerExtension) : base(containerExtension)
        {
        }

        public void Show(string name, IDialogParameters parameters, Action<IDialogResult> callback, bool hasOwner)
        {
            ShowDialogInternal(name, parameters, callback, false, hasOwner);
        }

        private void ShowDialogInternal(string name, IDialogParameters parameters, Action<IDialogResult> callback, bool isModal, bool hasOwner, string windowName = null)
        {
            var dialogWindow = CreateDialogWindow(windowName);
            ConfigureDialogWindowEvents(dialogWindow, callback);
            ConfigureDialogWindowContent(name, dialogWindow, parameters);

            if (!hasOwner && dialogWindow.Owner != null)
            {
                dialogWindow.Owner = null;
            }

            if (isModal)
            {
                dialogWindow.ShowDialog();
            }
            else
            {
                dialogWindow.Show();
            }
        }
    }
}