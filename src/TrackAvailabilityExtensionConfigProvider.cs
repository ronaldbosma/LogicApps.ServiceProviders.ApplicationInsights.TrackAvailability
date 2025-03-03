using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;

namespace LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability
{
    /// <summary>
    /// Registers the track availability service provider implementation as an Azure Functions extension with the Azure Logic Apps engine
    /// </summary>
    /// <see href="https://learn.microsoft.com/en-us/azure/logic-apps/create-custom-built-in-connector-standard#register-the-service-provider">Register the service provider</see>
    [Extension("TrackAvailabilityExtensionConfigProvider", configurationSection: "TrackAvailabilityExtensionConfigProvider")]
    internal class TrackAvailabilityExtensionConfigProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackAvailabilityExtensionConfigProvider"/> class.
        /// </summary>
        /// <param name="serviceOperationsProvider">Provides functionality to manage service providers.</param>
        /// <param name="operationsProvider">The track availability service provider to register as an extension.</param>
        public TrackAvailabilityExtensionConfigProvider(
            ServiceOperationsProvider serviceOperationsProvider, 
            TrackAvailabilityServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(
                serviceName: TrackAvailabilityServiceOperationProvider.ServiceName, 
                serviceOperationsProviderId: TrackAvailabilityServiceOperationProvider.ServiceId, 
                serviceOperationsProviderInstance: operationsProvider);
        }

        /// <remarks>
        /// In the Initialize method, you can add any custom implementation.
        /// For example adding a converter.
        /// </remarks>
        public void Initialize(ExtensionConfigContext context)
        {
        }
    }
}
