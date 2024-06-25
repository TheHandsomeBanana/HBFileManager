﻿using FileManager.UI.ViewModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.Services.FrameNavigationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unity;

namespace FileManager.UI.Views;
/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : Page {
    public SettingsPage() {
        InitializeComponent();
        //IFrameNavigationService navigationService = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IFrameNavigationService>();
        //navigationService.RegisterFrame("SettingsPageFrame", SettingsPageFrame);

        IViewModelCache viewModelCache = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IViewModelCache>();
        SettingsPageViewModel viewModel = viewModelCache.GetOrNew<SettingsPageViewModel>();

        this.DataContext = viewModel;
    }
}
