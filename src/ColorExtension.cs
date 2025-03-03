using System.Drawing;

namespace LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability
{
    /// <summary>
    /// Color extension.
    /// </summary>
    /// <see href="https://github.com/Azure/logicapps-connector-extensions/blob/CosmosDB/src/Common/extensions/ColorExtension.cs">ColorExtension.cs</see>
    internal static class ColorExtension
    {
        /// <summary>
        /// To hex color.
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>unit.</returns>
        public static uint ToHexColor(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                   (color.G << 8) | (color.B << 0));
        }
    }
}
