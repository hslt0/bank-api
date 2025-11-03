var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder.AddAzureStorage("storage")
                   .RunAsEmulator()
                   .AddBlobs("BankStorage");

builder.AddProject<Projects.BankApi_Service_Api>("BankApiService-Api")
       .WithReference(blobs);

builder.Build().Run();
