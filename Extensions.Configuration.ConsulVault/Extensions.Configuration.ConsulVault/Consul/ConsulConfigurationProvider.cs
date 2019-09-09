using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Meyer.Common.Extensions.Configuration.ConsulVault
{
    public class ConsulConfigurationProvider : ConfigurationProvider
    {
        private readonly string serviceName;
        private readonly string address;
        private readonly string token;
        private readonly bool isRequired;

        public ConsulConfigurationProvider(string serviceName, string address, string token, bool isRequired)
        {
            this.serviceName = serviceName;
            this.address = address;
            this.token = token;
            this.isRequired = isRequired;
        }

        public override void Load()
        {
            if (this.isRequired)
            {
                if (string.IsNullOrWhiteSpace(this.serviceName) || string.IsNullOrWhiteSpace(this.address) || string.IsNullOrWhiteSpace(this.token))
                {
                    throw new ArgumentException("Invalid consul connection info");
                }
            }

            this.LoadAsync().GetAwaiter().GetResult();
        }

        private async Task LoadAsync()
        {
            ConsulClient consulClient = new ConsulClient(action =>
            {
                action.Address = new Uri(this.address);
                action.Token = this.token;
            });

            var list = await consulClient.KV.List(this.serviceName);

            var kvPairs = list.Response
                .Select(x => new KVPair(Regex.Replace(x.Key.Replace($"{this.serviceName}/", "").Replace('/', ':').Trim(':'), "\\[\\d\\]", new MatchEvaluator(RemoveBrackets))) { Value = x.Value });

            foreach (var kvPair in kvPairs)
            {
                var value = kvPair.Value == null ? string.Empty : Encoding.UTF8.GetString(kvPair.Value);
                this.Data.Add(kvPair.Key, value);
            }
        }

        private static string RemoveBrackets(Match match)
        {
            return match.Value.Replace('[', ':').Replace("]", "");
        }
    }
}