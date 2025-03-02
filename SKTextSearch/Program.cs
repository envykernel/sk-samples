using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

var searchEngineId = Environment.GetEnvironmentVariable("GOOGLE_SEARCH_ENGINE_ID");
var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENIA_KEY");
var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENIA_ENDPOINT");

Console.WriteLine("Enter a query to search for: ");

var query = Console.ReadLine();

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

var medicalSearchPlugin = KernelPluginFactory.CreateFromFunctions(
    "MedicalSearchPlugin", "Search WHO site only",
    [textSearch.CreateGetTextSearchResults()]);

var computingSearchPlugin = KernelPluginFactory.CreateFromFunctions(
    "ComputingSearchPlugin", "Search Microsoft Developer Blogs site only",
    [textSearch.CreateGetTextSearchResults()]);

kernel.Plugins.Add(computingSearchPlugin);
kernel.Plugins.Add(medicalSearchPlugin);

// Invoke prompt and use text search plugin to provide grounding information
string promptTemplate = """
{{#with (SearchPlugin-GetTextSearchResults query)}}  
    {{#each this}}  
    Name: {{Name}}
    Value: {{Value}}
    Link: {{Link}}
    -----------------
    {{/each}}  
{{/with}}  

{{query}}
""";
OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
KernelArguments arguments = new(settings) { { "query", query } };


HandlebarsPromptTemplateFactory promptTemplateFactory = new();
Console.WriteLine(await kernel.InvokePromptAsync(
    query!,
    arguments
));

Console.ReadLine();