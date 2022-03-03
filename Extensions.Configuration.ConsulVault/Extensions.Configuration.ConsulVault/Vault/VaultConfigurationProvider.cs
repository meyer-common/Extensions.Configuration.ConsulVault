using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vault;

namespace Meyer.Common.Extensions.Configuration.ConsulVault;

public class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly string serviceName;
    private readonly string address;
    private readonly string token;
    private readonly bool isRequired;

    public VaultConfigurationProvider(string serviceName, string address, string token, bool isRequired)
    {
        this.serviceName = serviceName;
        this.address = address;
        this.token = token;
        this.isRequired = isRequired;
    }

    public override void Load()
    {
        if (string.IsNullOrWhiteSpace(this.serviceName) || string.IsNullOrWhiteSpace(this.address) || string.IsNullOrWhiteSpace(this.token))
        {
            if (isRequired)
                throw new ArgumentException("Invalid vault connection info");

            return;
        }

        LoadAsync().GetAwaiter().GetResult();
    }

    private async Task LoadAsync()
    {
        var vaultClient = new VaultClient(new VaultOptions { Address = address, Token = token });

        await Recurse(vaultClient, serviceName);

    }

    private async Task Recurse(VaultClient vaultClient, string key)
    {
        key = key.Trim('/');

        var list = await vaultClient.Secret.List(key);

        if (list.Data == null)
        {
            var result = await vaultClient.Secret.Read<Dictionary<string, string>>(key);
            foreach (string item in result.Data.Keys.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                Data.Add(Regex.Replace(key.Replace($"{serviceName}/", "").Replace('/', ':').Trim(':'), "\\[\\d\\]", new MatchEvaluator(RemoveBrackets)), result.Data[item]);
            }
        }
        else
        {
            foreach (string item in list.Data.Keys.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                await this.Recurse(vaultClient, $"{key}/{item}");
            }
        }
    }

    private static string RemoveBrackets(Match match)
    {
        return match.Value.Replace('[', ':').Replace("]", "");
    }
}