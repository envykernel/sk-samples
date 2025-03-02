
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

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

 // Construct the file path
string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Weather", "OpenApiSpecification.json");


// Create the OpenAPI plugin from a local file somewhere at the root of the application
await kernel.ImportPluginFromOpenApiAsync(  
    pluginName: "weather",  
    filePath: filePath,
    executionParameters: new OpenApiFunctionExecutionParameters()
    {
        AuthCallback = AuthenticateRequestAsyncCallback
    }
  ); 

OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), MaxTokens=500 };
KernelArguments arguments = new(settings);

var result = await kernel.InvokePromptAsync("what is the temperature in landon",arguments);
Console.WriteLine(result);



static Task AuthenticateRequestAsyncCallback(HttpRequestMessage request, CancellationToken cancellationToken = default)
{
    request.Headers.Add("x-rapidapi-host", "open-weather13.p.rapidapi.com");
    request.Headers.Add("x-rapidapi-key", "200b95a8c9msh36250aa73bd746ap13a2c7jsne8a99d2a6d78");
   
    return Task.CompletedTask;  
}