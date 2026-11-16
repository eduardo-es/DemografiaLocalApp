using DemografiaLocalApp.Services;
using DemografiaLocalApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace DemografiaLocalApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Camada de Serviços: HttpClient único (reaproveita conexões) + cliente das APIs do IBGE.
            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri(IbgeApiService.UrlBasePadrao),
                Timeout = TimeSpan.FromSeconds(30)
            });
            builder.Services.AddSingleton<IIbgeApiService, IbgeApiService>();

            // Camada de Apresentação: ViewModels da aplicação.
            builder.Services.AddTransient<MainViewModel>();

            // Páginas e Shell resolvidos pelo DI (construtor recebe a ViewModel).
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AppShell>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
