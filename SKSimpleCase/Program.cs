
using Azure.Identity;
using Microsoft.SemanticKernel;

var azureOpenAIDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENIA_DEPLOYMENT_NAME");
var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENIA_KEY");
var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENIA_ENDPOINT");

// Create Azure credential with interactive credentials
var credential = new DefaultAzureCredential(
    new DefaultAzureCredentialOptions { 
        ExcludeEnvironmentCredential= true,
        ExcludeManagedIdentityCredential = true,
        ExcludeInteractiveBrowserCredential = false,});

var kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
    deploymentName: azureOpenAIDeploymentName!,
    endpoint: azureOpenAIEndpoint!,
    apiKey: azureOpenAIKey ?? throw new ArgumentNullException(azureOpenAIKey));

var kernel = kernelBuilder.Build();

var result = await kernel.InvokePromptAsync("What is the capital of France?");
Console.WriteLine(result);