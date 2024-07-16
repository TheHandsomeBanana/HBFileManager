using FileManager.UI.Models.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Services.JobService;
public interface IJobService {
    public void AddOrUpdate(JobItemModel jobItem);
    public void Delete(JobItemModel jobItem);
    public void Delete(Guid jobId);
    public JobItemModel[] GetAll();
    public JobItemModel? GetById(Guid jobId);
}
