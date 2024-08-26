using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Fatura_Front
{
    public partial class App : System.Windows.Application
    {
        private readonly ConfigHelper _configHelper = new ConfigHelper();
        private readonly ApiService _apiService = new ApiService();
        private DispatcherTimer _tokenTimer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                girisKontrol();
            }
            catch (Exception ex)
            {
                LogError("OnStartup Error: " + ex.Message);
            }
        }

        private async void girisKontrol()
        {
            try
            {
                string email = ConfigurationManager.AppSettings["Email"];
                string password = ConfigurationManager.AppSettings["Password"];
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    return;
                }

                string licenseID = ConfigurationManager.AppSettings["LicenseID"];
                if (string.IsNullOrEmpty(licenseID))
                {
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    return;
                }
                if (await IsLicenseValidAsync(licenseID) == false)
                {
                    LogError("License invalid.");
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    return;
                }

                string token = ConfigurationManager.AppSettings["Token"];
                if (string.IsNullOrEmpty(token) || !TokenManager.IsTokenValid(token))
                {
                    try
                    {
                        var (newToken, newLicenseID) = await _apiService.GetTokenAsync(email, password);
                        _configHelper.SaveTokenToConfig(newToken);
                        _configHelper.SaveLicenseIDToConfig(newLicenseID);
                        LogError($"Token updated: {newToken}");
                        token = newToken;
                    }
                    catch (Exception ex)
                    {
                        LogError("Token renewal error: " + ex.Message);
                    }
                }
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                LogError("girisKontrol Error: " + ex.Message);
            }
        }

        private async Task<bool> IsLicenseValidAsync(string licenseID)
        {
            try
            {
                var apiService = new ApiService();
                var userInfoList = await apiService.GetCustomerInfoAsync(licenseID);
                return userInfoList != null && userInfoList.Count > 0;
            }
            catch (Exception ex)
            {
                LogError("IsLicenseValidAsync Error: " + ex.Message);
                return false;
            }
        }

        private void LogError(string message)
        {
            try
            {
                File.AppendAllText("log.txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message + Environment.NewLine);
            }
            catch
            {
                // Eğer log yazma başarısız olursa, hata bastırılır çünkü loglama işlemi zaten hataları yakalamak içindir.
            }
        }
    }
}
