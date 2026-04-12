using System.Text;
using System.Text.Json;
using Oci.Common.Auth;
using Oci.SecretsService;
using Oci.SecretsService.Requests;

namespace AstralDiaryApi.Services.Implementations
{
    public class OciSecretService
    {
        //private readonly IConfiguration _configuration;

        //public OciSecretService(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        //public async Task<string> GetSecretContentAsync(string secretId)
        //{
        //    var provider = new InstancePrincipalsAuthenticationDetailsProvider();

        //    using var client = new SecretsClient(provider);
        //    client.SetRegion(_configuration["OCI:Region"]);

        //    var request = new GetSecretBundleRequest { SecretId = secretId };

        //    var response = await client.GetSecretBundle(request);
        //    var base64Content = (
        //        (Oci.SecretsService.Models.Base64SecretBundleContentDetails)
        //            response.SecretBundle.SecretBundleContent
        //    ).Content;

        //    var bytes = Convert.FromBase64String(base64Content);
        //    return Encoding.UTF8.GetString(bytes);
        //}

        private readonly ILogger<OciSecretService> _logger;
        private readonly string _region;

        public OciSecretService(ILogger<OciSecretService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _region = configuration["OCI:Region"] ?? "ap-singapore-1";
        }

        public async Task<Dictionary<string, JsonElement>> GetSecretAsJsonAsync(string secretId)
        {
            try
            {
                var provider = new InstancePrincipalsAuthenticationDetailsProvider();

                using var client = new SecretsClient(provider);
                client.SetRegion(_region);

                var request = new GetSecretBundleRequest { SecretId = secretId };

                var response = await client.GetSecretBundle(request);
                var base64Content = (
                    (Oci.SecretsService.Models.Base64SecretBundleContentDetails)
                        response.SecretBundle.SecretBundleContent
                ).Content;

                var bytes = Convert.FromBase64String(base64Content);
                var jsonString = Encoding.UTF8.GetString(bytes);

                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString)
                    ?? new Dictionary<string, JsonElement>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secret from OCI Vault");
                throw;
            }
        }

        public async Task<string> GetSecretContentAsync(string secretId)
        {
            try
            {
                var provider = new InstancePrincipalsAuthenticationDetailsProvider();

                using var client = new SecretsClient(provider);
                client.SetRegion(_region);

                var request = new GetSecretBundleRequest { SecretId = secretId };

                var response = await client.GetSecretBundle(request);
                var base64Content = (
                    (Oci.SecretsService.Models.Base64SecretBundleContentDetails)
                        response.SecretBundle.SecretBundleContent
                ).Content;

                var bytes = Convert.FromBase64String(base64Content);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secret from OCI Vault");
                throw;
            }
        }
    }
}
