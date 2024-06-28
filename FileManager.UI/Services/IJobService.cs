using FileManager.UI.Models.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Services;
public interface IJobService {
    public void AddJob(JobItemModel jobItem);
    public void RemoveJob(JobItemModel jobItem);
    public JobItemModel[] GetAllJobs();
    public JobItemModel[] GetJobById(Guid jobId);
}
