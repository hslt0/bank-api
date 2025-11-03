var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder.AddAzureStorage("storage")
                   .RunAsEmulator()
                   .AddBlobs("BankStorage");

var api = builder.AddProject<Projects.BankApi_Service_Api>("BankApiService-Api")
       .WithReference(blobs);

builder.AddProject<Projects.BankApi_Client>("BankApiClient-Client")
    .WithReference(api);

builder.Build().Run();
