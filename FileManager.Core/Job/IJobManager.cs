using FileManager.Core.JobSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Job;
public interface IJobManager {
    public void AddOrUpdate(Job jobItem);
    public void Delete(Job jobItem);
    public void Delete(Guid jobId);
    public Job[] GetAll();
    public Job? GetById(Guid jobId);

    public void AddOrUpdateStep(Guid jobId, JobStep step);
    public void DeleteStep(Guid jobId, JobStep step);
    public void DeleteStep(Job job, JobStep step);
    public void DeleteStep(JobStep step);
    public JobStep? GetStepById(Guid jobId, Guid stepId);
    public JobStep[] GetSteps(Guid jobId);
}
