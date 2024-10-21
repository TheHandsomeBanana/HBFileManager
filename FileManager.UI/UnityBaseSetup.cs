using HBLibrary.Core;
using HBLibrary.Interface.DI;
using HBLibrary.Interface.Security.Account;
using HBLibrary.Interface.Security.Authentication;
using HBLibrary.Security.Account;
using HBLibrary.Security.Authentication;
using HBLibrary.Security.Authentication.Microsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;

namespace FileManager.UI;
public class UnityBaseSetup : IUnitySetup {
    public void Build(IUnityContainer container) {
        AddConfiguration(container);
        AddAuthentication(container);
    }

    private static void AddAuthentication(IUnityContainer container) {
        AzureAdOptions azureAdOptions = container.Resolve<AzureAdOptions>();
        CommonAppSettings commonAppSettings = container.Resolve<CommonAppSettings>();

        LocalAuthenticationService localAuthenticationService = new LocalAuthenticationService();


        IPublicClientApplication app = PublicClientApplicationBuilder.Create(azureAdOptions.ClientId)
           .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
           .WithRedirectUri(azureAdOptions.RedirectUri)
           .WithWindowsEmbeddedBrowserSupport()
           //.WithWindowsDesktopFeatures(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
           .Build();

        // Token cache for handling accounts across sessions
        MSTokenStorage.Create(app);

        PublicMSAuthenticationService publicMSAuthenticationService
            = new PublicMSAuthenticationService(app);

        container.RegisterInstance<ILocalAuthenticationService>(localAuthenticationService, new SingletonLifetimeManager());
        container.RegisterInstance<IPublicMSAuthenticationService>(publicMSAuthenticationService, new SingletonLifetimeManager());

        container.RegisterType<IAccountStorage, AccountStorage>();
        container.RegisterSingleton<IAccountService, AccountService>();
    }

    private static void AddConfiguration(IUnityContainer container) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .Build();

        AzureAdOptions azureAdOptions = new AzureAdOptions();
        configuration.GetSection("AzureAd").Bind(azureAdOptions);

        CommonAppSettings commonAppSettings = new CommonAppSettings();
        configuration.GetSection("Application").Bind(commonAppSettings);

        container.RegisterInstance(azureAdOptions, new SingletonLifetimeManager());
        container.RegisterInstance(commonAppSettings, new SingletonLifetimeManager());
    }
}
