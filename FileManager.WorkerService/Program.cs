namespace FileManager.WorkerService;

public class Program {
    public static void Main(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        builder.Services.AddWindowsService(options => {
            options.ServiceName = "HBFileManagerWorker";
        });

        IHost host = builder.Build();
        host.Run();
    }
}