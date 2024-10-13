using FileManager.Core;
using FileManager.Core.Workspace;
using FileManager.UI.Views;
using HBLibrary.Common;
using HBLibrary.Common.Account;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Workspace;
using HBLibrary.Wpf.Commands;
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

public class WorkspaceItemViewModel : ViewModelBase<HBFileManagerWorkspace> {
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
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
    public RelayCommand ShareAccessCommand { get; set; }
    public RelayCommand<AccountInfo> RevokeAccessCommand { get; set; }

    public WorkspaceItemViewModel(HBFileManagerWorkspace model) : base(model) {
        IUnityContainer container = UnityBase.Registry.Get(DIContainerGuids.FileManagerContainerGuid);
        workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();

        ShareAccessCommand = new RelayCommand(ShareAccess, true);
        RevokeAccessCommand = new RelayCommand<AccountInfo>(RevokeAccess, true);
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

    private void RevokeAccess(AccountInfo? obj) {
        if(obj is null) {
            return;
        }

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

    private void ShareAccess(object? obj) {
        
    }
}
