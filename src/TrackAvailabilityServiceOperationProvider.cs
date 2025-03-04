using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Net;

namespace LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability
{
    /// <summary>
    /// Service operations provider for the Track Availability service.
    /// </summary>
    /// <remarks>
    /// This service provider implements the IServiceOperationsProvider interface.
    /// It includes the methods that provide the operations manifest, which provide the metadata about the implemented operations in this custom built-in connector to the designer.
    /// It also includes methods for operation invocations, which are the implementations of the triggers and actions used during workflow execution.
    /// </remarks>
    /// <see href="https://learn.microsoft.com/en-us/azure/logic-apps/custom-connector-overview#iserviceoperationsprovider">Customer Connector Overview - IServiceOperationsProvider</see>
    /// <see href="https://github.com/Azure/logicapps-connector-extensions/blob/main/src/CosmosDB/Providers/CosmosDbServiceOperationProvider.cs">Sample CosmosDbServiceOperationProvider.cs</see>
    [ServiceOperationsProvider(Id = ServiceId, Name = ServiceName)]
    public class TrackAvailabilityServiceOperationProvider : IServiceOperationsProvider
    {
        /// <summary>
        /// The name of the service.
        /// </summary>
        public const string ServiceName = "trackAvailabilityInAppInsights";

        /// <summary>
        /// The ID of the service.
        /// </summary>
        public const string ServiceId = "/serviceProviders/trackAvailabilityInAppInsights";

        private readonly List<ServiceOperation> _serviceOperations;
        private readonly List<ServiceOperation> _serviceOperationsWithManifest;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackAvailabilityServiceOperationProvider"/> class.
        /// </summary>
        public TrackAvailabilityServiceOperationProvider()
        {
            // Create a dictionary with available service operations (triggers and actions).
            _serviceOperations = new List<ServiceOperation>()
            { 
                { GetTrackAvailabilityServiceOperation() }
            };

            // Create a list of available service operations (triggers and actions) including there manifests.
            _serviceOperationsWithManifest = _serviceOperations.Select(s => s.CloneWithManifest(GetTrackAvailabilityServiceOperationManifest())).ToList();
        }

        /// <summary>
        /// Get high-level metadata of this service.
        /// </summary>
        /// <remarks>
        /// The designer requires this method to get the high-level metadata for your service, 
        /// including the service description, connection input parameters, capabilities, brand color, icon URL and so on.
        /// </remarks>
        /// <see href="https://learn.microsoft.com/en-us/azure/logic-apps/custom-connector-overview#getservice">Customer Connector Overview - GetService()</see> 
        public ServiceOperationApi GetService()
        {
            return GetServiceOperationApi();
        }

        /// <summary>
        /// Get the operations implemented by this service, which are the triggers and actions.
        /// </summary>
        /// <param name="expandManifest">A boolean value indicating whether to include the service operation manifest.</param>
        /// <remarks>
        /// The designer requires this method to get the operations implemented by your service. 
        /// The operations list is based on Swagger schema. 
        /// The designer also uses the operation metadata to understand the input parameters for specific operations 
        /// and generate the outputs as property tokens, based on the schema of the output for an operation.
        /// </remarks>
        /// <see href="https://learn.microsoft.com/en-us/azure/logic-apps/custom-connector-overview#getoperations">Customer Connector Overview - GetOperations()</see> 
        public IEnumerable<ServiceOperation> GetOperations(bool expandManifest)
        {
            return expandManifest ? _serviceOperationsWithManifest : _serviceOperations;
        }

        /// <summary>
        /// Not implemented because it is used by the runtime in Azure Logic Apps to provide 
        /// the required connection parameters information to the Azure Functions trigger binding
        /// </summary>
        /// <see href="https://learn.microsoft.com/en-us/azure/logic-apps/custom-connector-overview#getbindingconnectioninformation">Customer Connector Overview - GetBindingConnectionInformation()</see> 
        public string GetBindingConnectionInformation(string operationId, InsensitiveDictionary<JToken> connectionParameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Is called for each action in this connector that runs during workflow execution.
        /// </summary>
        /// <remarks>
        /// This method is called for each action in your connector that runs during workflow execution. 
        /// You can use any client, such as FTPClient, HTTPClient, and so on, as required by your connector's actions.
        /// If the custom built-in connector only has a trigger, you don't have to implement this method. 
        /// </remarks>
        /// <param name="operationId">The ID of the operation (action) to invoke.</param>
        /// <param name="connectionParameters">The connection parameters for the operation.</param>
        /// <param name="serviceOperationRequest">The request for the service operation, containing the input values and other parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the service operation response.</returns>
        /// <exception cref="ServiceOperationsProviderException">Thrown when the operation ID is missing or not supported.</exception>
        /// <see href="https://learn.microsoft.com/en-us/azure/logic-apps/custom-connector-overview#getbindingconnectioninformation">Customer Connector Overview - GetBindingConnectionInformation</see>
        public Task<ServiceOperationResponse> InvokeOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            if (operationId == null)
            {
                throw new ServiceOperationsProviderException(HttpStatusCode.BadRequest, ServiceOperationsErrorResponseCode.ServiceOperationIdMissing, ServiceOperationsMessages.ServiceOperationIdMissing);
            }
            if (operationId != "trackAvailabilityInAppInsights")
            {
                throw new ServiceOperationsProviderException(HttpStatusCode.BadRequest, ServiceOperationsErrorResponseCode.ServiceOperationIdNotSupported, ServiceOperationsMessages.ServiceOperationNotSupported);
            }

            var telemetryClient = new TelemetryClient(new TelemetryConfiguration()
            {
                ConnectionString = ServiceOperationsProviderUtilities.GetParameterValue("ConnectionString", connectionParameters).ToValue<string>(),
                TelemetryChannel = new InMemoryChannel()
            });

            var availability = new AvailabilityTelemetry
            {
                Name = serviceOperationRequest.Parameters["testName"].ToValue<string>(),
                RunLocation = GetRunLocation(serviceOperationRequest),
                Success = serviceOperationRequest.Parameters["success"].ToValue<bool>(),
                Timestamp = DateTimeOffset.UtcNow
            };

            telemetryClient.TrackAvailability(availability);
            telemetryClient.Flush();

            return Task.FromResult(new ServiceOperationResponse(null, HttpStatusCode.OK));
        }

        private static string GetRunLocation(ServiceOperationRequest serviceOperationRequest)
        {
            if (serviceOperationRequest.Parameters.ContainsKey("runLocation"))
            {
                return serviceOperationRequest.Parameters["runLocation"].ToValue<string>();
            }

            return Environment.GetEnvironmentVariable("REGION_NAME") ?? "Unknown";
        }

        /// <summary>
        /// Gets the service operation API of this custom connector.
        /// </summary>
        private static ServiceOperationApi GetServiceOperationApi()
        {
            return new ServiceOperationApi
            {
                Name = ServiceName,
                Id = ServiceId,
                Type = DesignerApiType.ServiceProvider,
                Properties = new ServiceOperationApiProperties
                {
                    BrandColor = Color.LightSkyBlue.ToHexColor(),
                    DisplayName = "Track availability in App Insights",
                    Description = "Track availability of a service in Application Insights.",
                    IconUri = new Uri(Resources.Icon),
                    Capabilities = new[] { ApiCapability.Actions },
                    ConnectionParameters = new ConnectionParameters
                    {
                        ConnectionString = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.SecureString,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Connection String",
                                Description = "Application Insights Connection String",
                                Tooltip = "Provide Application Insights Connection String",
                                Constraints = new Constraints
                                {
                                    Required = "true"
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Gets the service operation of the Track Availability action.
        /// </summary>
        private static ServiceOperation GetTrackAvailabilityServiceOperation()
        {
            return new ServiceOperation
            {
                Name = "trackAvailabilityInAppInsights",
                Id = "trackAvailabilityInAppInsights",
                Type = "trackAvailabilityInAppInsights",
                Properties = new ServiceOperationProperties
                {
                    Api = GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Track availability in App Insights",
                    Description = "Track availability of a service in Application Insights",
                    Visibility = Visibility.Important,
                    OperationType = OperationType.ServiceProvider,
                    Capabilities = new[] { ApiCapability.Actions },
                    BrandColor = Color.LightSkyBlue.ToHexColor(),
                    IconUri = new Uri(Resources.Icon)
                }
            };
        }

        /// <summary>
        /// Gets the service operation manifest of the Track Availability action.
        /// </summary>
        private static ServiceOperationManifest GetTrackAvailabilityServiceOperationManifest()
        {
            return new ServiceOperationManifest
            {
                ConnectionReference = new ConnectionReferenceFormat
                {
                    ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                },
                Settings = new OperationManifestSettings
                {
                    SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                    TrackedProperties = new OperationManifestSetting
                    {
                        Scopes = new[] { OperationScope.Action }
                    }
                },
                InputsLocation = new InputsLocation[]
                {
                    InputsLocation.Inputs, // TODO: can we remove this, seems like a trigger setting
                    InputsLocation.Parameters
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "testName", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Test Name",
                                Description = "Test Name"
                            }
                        },
                        {
                            "runLocation", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Run Location",
                                Description = "Run Location",
                            }
                        },
                        {
                            "success", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.Boolean,
                                Title = "Success",
                                Description = "Success",
                            }
                        }
                    },
                    Required = new[]
                    {
                        "testName",
                        "success"
                    }
                },
                Connector = GetServiceOperationApi(),
                //TODO: can we remove this, seems like a trigger setting
                Recurrence = new RecurrenceSetting
                {
                    Type = RecurrenceType.None,
                }
            };
        }
    }
}
