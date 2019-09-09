using Meyer.Common.Extensions.Configuration.ConsulVault;

namespace Microsoft.Extensions.Configuration
{
    public static class ConsulConfigurationExtensions
    {
        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, string serviceName, string address, string token, bool optional)
        {
            return configurationBuilder.Add(new ConsulConfigurationSource(serviceName, address, token, optional));
        }
    }
}