﻿using FileManager.UI.ViewModels.Jobs;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels;
public class JobsViewModel : ViewModelBase {
    public JobListViewModel JobsList { get; set; } = new JobListViewModel();


}
