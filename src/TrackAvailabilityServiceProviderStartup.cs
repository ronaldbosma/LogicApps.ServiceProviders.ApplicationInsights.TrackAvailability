using LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: WebJobsStartup(typeof(TrackAvailabilityServiceProviderStartup))]

namespace LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability
{
    /// <summary>
    /// Startup class for the track availability service provider.
    /// </summary>
    /// <see href="https://learn.microsoft.com/en-us/azure/logic-apps/create-custom-built-in-connector-standard#create-the-startup-job">Create the startup job</see>
    public class TrackAvailabilityServiceProviderStartup : IWebJobsStartup
    {
        /// <summary>
        /// Registers the track availability extension with the web jobs host and
        /// registers the <see cref="TrackAvailabilityServiceOperationProvider"/> in the service collection.
        /// </summary>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<TrackAvailabilityExtensionConfigProvider>();
            builder.Services.TryAddSingleton<TrackAvailabilityServiceOperationProvider>();
        }
    }
}
