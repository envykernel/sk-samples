using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;

var searchEngineId = Environment.GetEnvironmentVariable("GOOGLE_SEARCH_ENGINE_ID");
var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENIA_KEY");
var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENIA_ENDPOINT");

// Create a kernel with AzureOpenAI chat completion
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o-mini",
       endpoint: azureOpenAIEndpoint ?? throw new ArgumentNullException("AZURE_OPENIA_ENDPOINT"),
        apiKey: azureOpenAIKey ?? throw new ArgumentNullException("AZURE_OPENIA_KEY"));

Kernel kernel = kernelBuilder.Build();

var textSearch = new GoogleTextSearch(
    searchEngineId: searchEngineId?? throw new ArgumentNullException("GOOGLE_SEARCH_ENGINE_ID"),
    apiKey: apiKey ?? throw new ArgumentNullException("GOOGLE_API_KEY"));

var searchPlugin = textSearch.CreateWithSearch("SearchPlugin");
kernel.Plugins.Add(searchPlugin);

var query = "What is the Semantic Kernel?";
var prompt = "{{SearchPlugin.Search $query}}. {{$query}}";

KernelArguments arguments = new KernelArguments(){{"query", query}};
var result = await kernel.InvokePromptAsync(prompt, arguments);
Console.WriteLine(result);

Console.ReadLine();