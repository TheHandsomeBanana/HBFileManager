﻿using FileManager.Core;
using HBLibrary.DI;
using HBLibrary.Interface.Security.Account;
using HBLibrary.Security.Account;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using Microsoft.Graph.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.UI.ViewModels.WorkspaceViewModels;

public class ShareWorkspaceAccessViewModel : AsyncInitializerViewModelBase {
    private readonly IUnityContainer container;
    private readonly IAccountInfo owner;

    public ObservableCollection<IAccountInfo> AvailableAccounts { get; set; }
    public ObservableCollection<IAccountInfo> SharedWithAccounts { get; set; }

    public AccountInfo? availableAccountsSelectedItem;
    public AccountInfo? AvailableAccountsSelectedItem {
        get => availableAccountsSelectedItem;
        set {
            availableAccountsSelectedItem = value;
            NotifyPropertyChanged();

            AddAccountToShareCommand.NotifyCanExecuteChanged();
        }
    }

    public AccountInfo? sharedWithAccountsSelectedItem;
    public AccountInfo? SharedWithAccountsSelectedItem {
        get => sharedWithAccountsSelectedItem;
        set {
            sharedWithAccountsSelectedItem = value;
            NotifyPropertyChanged();

            RemoveAccountFromShareCommand.NotifyCanExecuteChanged();
        }
    }



    public List<AccountInfo> NewAccountsToShareAccess { get; set; } = [];
    public List<AccountInfo> NewAccountsToRevokeAccess { get; set; } = [];

    public RelayCommand<Window> SaveCommand { get; set; }
    public RelayCommand<Window> CancelCommand { get; set; }
    public RelayCommand AddAccountToShareCommand { get; set; }
    public RelayCommand RemoveAccountFromShareCommand { get; set; }
    public ShareWorkspaceAccessViewModel(IAccountInfo owner, IAccountInfo[] currentSharedAccounts) {
        this.owner = owner;

        container = UnityBase.Registry.Get(DIContainerGuids.FileManagerContainerGuid);

        SaveCommand = new RelayCommand<Window>(Save);
        CancelCommand = new RelayCommand<Window>(Cancel);
        AddAccountToShareCommand = new RelayCommand(AddAccount, CanAddAccounts);
        RemoveAccountFromShareCommand = new RelayCommand(RemoveAccount, CanRemoveAccount);

        AvailableAccounts = new ObservableCollection<IAccountInfo>();
        SharedWithAccounts = new ObservableCollection<IAccountInfo>(currentSharedAccounts);
    }


    protected override async Task InitializeViewModelAsync() {
        IAccountStorage storage = new AccountStorage();
        IAccountInfo[] accounts = await storage.LoadAccountsAsync();

        foreach (AccountInfo account in accounts) {
            if (SharedWithAccounts.All(e => e.AccountId != account.AccountId) && account != (AccountInfo)owner) {
                AvailableAccounts.Add(account);
            }
        }
    }

    protected override void OnInitializeException(Exception exception) {
        HBDarkMessageBox.Show("Initialize error", exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }


    private void RemoveAccount(object? obj) {
        if (SharedWithAccountsSelectedItem is null) {
            return;
        }

        AvailableAccounts.Add(SharedWithAccountsSelectedItem);
        NewAccountsToShareAccess.Remove(SharedWithAccountsSelectedItem);

        NewAccountsToRevokeAccess.Add(SharedWithAccountsSelectedItem);

        SharedWithAccounts.Remove(SharedWithAccountsSelectedItem);
    }

    private bool CanRemoveAccount(object? obj) {
        return SharedWithAccountsSelectedItem is not null;
    }

    private void AddAccount(object? obj) {
        if (AvailableAccountsSelectedItem is null) {
            return;
        }

        SharedWithAccounts.Add(AvailableAccountsSelectedItem);
        NewAccountsToRevokeAccess.Remove(AvailableAccountsSelectedItem);

        NewAccountsToShareAccess.Add(AvailableAccountsSelectedItem);

        AvailableAccounts.Remove(AvailableAccountsSelectedItem);
    }

    private bool CanAddAccounts(object? obj) {
        return AvailableAccountsSelectedItem is not null;
    }


    private void Save(Window? obj) {
        if (obj is null) {
            return;
        }

        obj.DialogResult = true;
        obj.Close();
    }

    private void Cancel(Window? obj) {
        if (obj is null) {
            return;
        }

        obj.DialogResult = false;
        obj.Close();
    }


}
