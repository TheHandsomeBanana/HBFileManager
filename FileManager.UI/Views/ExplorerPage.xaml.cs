﻿using FileManager.UI.ViewModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Services;
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
/// Interaction logic for ExplorerPage.xaml
/// </summary>
public partial class ExplorerPage : Page {
    public ExplorerPage() {
        InitializeComponent();

        IViewModelCache viewModelCache = UnityBase.GetChildContainer(nameof(FileManager)).Resolve<IViewModelCache>();
        ExplorerPageViewModel viewModel = viewModelCache.GetOrNew<ExplorerPageViewModel>();

        this.DataContext = viewModel;
    }
}
