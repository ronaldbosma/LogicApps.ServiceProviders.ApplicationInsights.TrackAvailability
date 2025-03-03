# LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability

This package contains a custom built-in connector for Standard Logic Apps. It contains an action that can send an availability test result to Azure Application Insights.

> [!IMPORTANT]  
> This NuGet package is still under development and is not yet available on nuget.org

## Local Development

1. Build the [LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability](/src/LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability.csproj) project. This will generate the NuGet package in the `/bin/Debug` folder of the project.

1. Execute the [add-extension.ps1](/script/add-extension.ps1) PowerShell script to install the extension locally. This script:

    1. Adds the NuGet package to the [project file](/samples/nuget-package-based/LogicApp/LogicApp.csproj) of the sample LogicApp project.
    1. Adds an entry to the local extension bundle file of your Azure Functions Core Tools installation. 
       E.g. `%USERPROFILE%\.azure-functions-core-tools\Functions\ExtensionBundles\Microsoft.Azure.Functions.ExtensionBundle.Workflows\1.94.69\bin\extensions.json`
    1. Copies the extension DLL to the extension bundle directory.

You can now use the connector in the Visual Studio Code Logic App Designer.

## Deploy Sample

There's a sample Logic App in `/samples/nuget-package-based/LogicApp` that uses the custom connector. Because of the custom connector, it's a NuGet package-based Logic App with a `.csproj` file. To deploy this sample, follow these steps:

1. Deploy an Azure Logic App Standard connected to an Azure Application Insights instance, with .NET version `v6.0` or `v8.0`.)  

   > You can use the [Azure Integration Services Quickstart](https://github.com/ronaldbosma/azure-integration-services-quickstart) Azure Developer CLI template to deploy the necessary resources. 
   > Use `azd provision` to only deploy the infra and specify `true` for the `includeLogicApp` parameter. Other parameters can be `false`.  
   > Note this template sets the .NET version `v9.0`, but it will automatically be changed to `v8.0` in the following step when using `func azure functionapp publish`.

1. Deploy the [Sample Logic App](/samples/nuget-package-based/LogicApp/).
   1. Open a prompt and navigate to the `/samples/nuget-package-based/LogicApp` directory.
   1. Execute the following command. Replace `<logicAppName>` with your Logic App name. This will build and deploy the Logic App.

      ```bash
      func azure functionapp publish <logicAppName>
      ```

   A big downside of having to use a NuGet-package based project is that you have to deploy the `Microsoft.Azure.Workflows.WebJobs.Extension` package with all its dependencies, which is more than 300MB. So the deployment might take awhile. 


## Links

- [Custom connectors in Azure Logic Apps](https://learn.microsoft.com/en-us/azure/logic-apps/custom-connector-overview)
- [Create custom built-in connectors for Standard logic apps in single-tenant Azure Logic Apps](https://learn.microsoft.com/en-us/azure/logic-apps/create-custom-built-in-connector-standard)
- [ Sample custom built-in Azure Cosmos DB connector - Azure Logic Apps Connector Extensions](https://github.com/Azure/logicapps-connector-extensions/tree/CosmosDB/src/CosmosDB)