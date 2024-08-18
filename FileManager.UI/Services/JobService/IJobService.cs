using FileManager.UI.Models.JobModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Services.JobService;
public interface IJobService {
    public void AddOrUpdate(JobItemModel jobItem);
    public void Delete(JobItemModel jobItem);
    public void Delete(Guid jobId);
    public JobItemModel[] GetAll();
    public JobItemModel? GetById(Guid jobId);

    public void AddOrUpdateStep(Guid jobId, JobItemStepModel step);
    public void DeleteStep(Guid jobId, JobItemStepModel step);
    public void DeleteStep(Guid jobId, Guid stepId);
    public JobItemStepModel? GetStepById(Guid jobId, Guid stepId);
    public JobItemStepModel[] GetSteps(Guid jobId);
}
