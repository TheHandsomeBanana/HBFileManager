using FileManager.UI.Models.Job;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.Jobs;
public abstract class JobItemStepViewModel : ViewModelBase<JobItemStepModel> {
	public string Name {
		get { return Model.Name; }
		set { 
			Model.Name = value;
			NotifyPropertyChanged();
		}
	}


	public JobItemStepViewModel(JobItemStepModel model) : base(model) {
    }
}
