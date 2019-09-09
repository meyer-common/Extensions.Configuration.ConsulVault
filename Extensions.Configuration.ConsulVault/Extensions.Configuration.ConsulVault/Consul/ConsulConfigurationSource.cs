using Microsoft.Extensions.Configuration;

namespace Meyer.Common.Extensions.Configuration.ConsulVault
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        private readonly string serviceName;
        private readonly string address;
        private readonly string token;
        private readonly bool isRequired;

        public ConsulConfigurationSource(string serviceName, string address, string token, bool optional)
        {
            this.serviceName = serviceName;
            this.address = address;
            this.token = token;
            this.isRequired = !optional;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(this.serviceName, this.address, this.token, this.isRequired);
        }
    }
}