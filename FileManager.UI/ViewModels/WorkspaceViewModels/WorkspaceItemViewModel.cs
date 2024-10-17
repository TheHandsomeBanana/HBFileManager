using FileManager.Core;
using FileManager.Core.Workspace;
using FileManager.UI.Views;
using FileManager.UI.Views.WorkspaceViews;
using HBLibrary.Common;
using HBLibrary.Common.Account;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Workspace;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.WorkspaceViewModels;

public class WorkspaceItemViewModel : ViewModelBase<HBFileManagerWorkspace>, IDisposable {
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    private readonly IUnityContainer container;

    public string Owner { get => Model.Owner!.Username; }
    public string Name { get => Model.Name!; }
    public bool UsesEncryption { get => Model.UsesEncryption; }
    public string FullPath { get => Model.FullPath!; }


    private string? searchText;
    public string? SearchText {
        get => searchText;
        set {
            searchText = value;
            NotifyPropertyChanged();
            accessControlView.Refresh();
        }
    }

    private AccountInfo? selectedAccessControl;

    public AccountInfo? SelectedAccessControl {
        get { return selectedAccessControl; }
        set {
            selectedAccessControl = value;
            NotifyPropertyChanged();
        }
    }


    public ObservableCollection<AccountInfo> AccessControlList;
    private readonly ICollectionView accessControlView;
    public ICollectionView AccessControlView => accessControlView;
    public AsyncRelayCommand ShareAccessCommand { get; set; }
    public AsyncRelayCommand<AccountInfo> RevokeAccessCommand { get; set; }

    public WorkspaceItemViewModel(HBFileManagerWorkspace model) : base(model) {
        container = UnityBase.Registry.Get(DIContainerGuids.FileManagerContainerGuid);
        workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();

        ShareAccessCommand = new AsyncRelayCommand(ShareAccessAsync, _ => true, OnShareAccessException);
        RevokeAccessCommand = new AsyncRelayCommand<AccountInfo>(RevokeAccessAsync, _ => true, OnRevokeAccessException);
        AccessControlList = new ObservableCollection<AccountInfo>(Model.SharedAccess);

        accessControlView = CollectionViewSource.GetDefaultView(AccessControlList);
        accessControlView.Filter = FilterAccounts;
        accessControlView.CollectionChanged += AccessControlView_CollectionChanged;
    }

    private void AccessControlView_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        SelectedAccessControl = accessControlView.Cast<AccountInfo>().FirstOrDefault();
    }

    private bool FilterAccounts(object obj) {
        if (obj is AccountInfo accountInfo) {
            return string.IsNullOrEmpty(SearchText) || accountInfo.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private async Task RevokeAccessAsync(AccountInfo? obj) {
        if (obj is null) {
            return;
        }

        IAccountService accountService = container.Resolve<IAccountService>();

        await Model.OpenAsync(accountService.Account!);
        try {

            Result revokeResult = workspaceManager.RevokeAccess(Model, obj);
            if (revokeResult.IsFaulted) {
                HBDarkMessageBox.Show("Revoke access error",
                    revokeResult.Exception!.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else {
                AccessControlList.Remove(obj);
            }
        }
        finally {
            await Model.CloseAsync();
        }
    }

    private void OnRevokeAccessException(Exception exception) {
        HBDarkMessageBox.Show("Share access error",
            exception.Message,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private async Task ShareAccessAsync(object? obj) {
        IAccountService accountService = container.Resolve<IAccountService>();

        await Model.OpenAsync(accountService.Account!);

        try {
            ShareWorkspaceAccessView view = new ShareWorkspaceAccessView();
            ShareWorkspaceAccessViewModel viewModel = new ShareWorkspaceAccessViewModel(Model.Owner!, Model.SharedAccess);

            IDialogService dialogService = container.Resolve<IDialogService>();
            bool dialogResult = dialogService.ShowCompactDialog(view, viewModel, "Access Control");

            if (dialogResult) {
                foreach (AccountInfo account in viewModel.NewAccountsToRevokeAccess) {
                    Result result = workspaceManager.RevokeAccess(Model, account);
                    if (result.IsFaulted) {
                        HBDarkMessageBox.Show("Revoke access error",
                            result.Exception!.Message,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    else {
                        AccessControlList.Remove(account);
                    }
                }

                foreach (AccountInfo account in viewModel.NewAccountsToShareAccess) {
                    Result result = workspaceManager.ShareAccess(Model, account);

                    if (result.IsFaulted) {
                        HBDarkMessageBox.Show("Share access error",
                            result.Exception!.Message,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    else {
                        AccessControlList.Add(account);
                    }
                }
            }
        }
        finally {
            await Model.CloseAsync();
        }
    }

    private void OnShareAccessException(Exception exception) {
        HBDarkMessageBox.Show("Share access error",
            exception.Message,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    public void Dispose() {
        accessControlView.CollectionChanged -= AccessControlView_CollectionChanged;
    }
}
