using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Fatura_Front.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fatura_Front
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<(string token,string licenseID)> GetTokenAsync (string email,string password)
        {
            var requestData = new
            {
                Email= email,
                Password= password
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:3000/api/auth/login", requestData);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var jsonResult = JObject.Parse(result);
            string token = jsonResult["token"].ToString();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var licenseID = jwtToken.Claims.First(claim => claim.Type == "LicenseID").Value;
            return (token, licenseID);
        }

        public async Task<List<UserInfo>> GetCustomerInfoAsync(string licenseID) 
        {

            var response = await _httpClient.GetAsync($"http://localhost:3000/api/login/{licenseID}"); //token gerek yok
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var userInfoList = JsonConvert.DeserializeObject<List<UserInfo>>(content);
            return userInfoList;
        }

        public async Task<List<Invoices>> GetInvoicesAsync(string token,string licenseID)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"http://localhost:3000/api/invoices/{licenseID}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Invoices>>();
        }

        public async Task<List<Invoices>> UpdateInvoicesPrint(string token,int id)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestUri = $"http://localhost:3000/api/invoices/{id}/print";
            var response = await _httpClient.PutAsync(requestUri, null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Invoices>>();
        }

        public async Task UpdatePrinterNameAsync(string licenseID, string printerName)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            var requestUri = $"http://localhost:3000/api/login/printer/{licenseID}";
            var requestData = new
            {   licenseID = licenseID,
                printerName = printerName
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(requestUri, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetPrinterNameAsync(string licenseID)
        {
            var requestUri = $"http://localhost:3000/api/login/printer/{licenseID}";

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            return jsonResponse["PrinterName"].ToString();

        }



    }
}
