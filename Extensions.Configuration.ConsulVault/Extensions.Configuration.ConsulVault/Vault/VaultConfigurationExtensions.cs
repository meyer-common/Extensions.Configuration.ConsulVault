using Meyer.Common.Extensions.Configuration.ConsulVault;

namespace Microsoft.Extensions.Configuration
{
    public static class VaultConfigurationExtensions
    {
        public static IConfigurationBuilder AddVault(this IConfigurationBuilder configurationBuilder, string serviceName, string address, string token, bool optional)
        {
            return configurationBuilder.Add(new VaultConfigurationSource(serviceName, address, token, optional));
        }
    }
}