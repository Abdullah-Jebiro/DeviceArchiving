using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;
using DeviceArchiving.Service;
using DeviceArchiving.Data.Contexts;
using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data;
using DeviceArchiving.Service.AccountServices;
using DeviceArchiving.Service.OperationServices;
using DeviceArchiving.Service.DeviceServices;
using DeviceArchiving.Service.OperationTypeServices;
using static DeviceArchiving.Service.DataAccessLayer;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Configuration;
using Humanizer.Configuration;

namespace DeviceArchiving.WindowsForm.Forms;

static class Program
{
    public static IServiceProvider Services { get; private set; }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        string pathDb = configuration["PathDb"]
            ?? throw new ArgumentNullException(nameof(configuration), "المفتاح 'PathDb' غير موجود في الإعدادات.");
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        try
        {

            DatabaseFileChecker.CheckAndConnect(pathDb);
          
        }
        catch (FileNotFoundException ex)
        {
            MessageBox.Show(
                "ملف قاعدة البيانات غير موجود. الرجاء التأكد من توصيل القرص أو فك التشفير.",
                "تحذير",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "تحذير",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<CustomUserIdInterceptor>(sp => new CustomUserIdInterceptor(() => AppSession.CurrentUserId));
        services.AddDbContextFactory<DeviceArchivingContext>(options =>
            options.UseSqlServer(connectionString)
                   .AddInterceptors(services.BuildServiceProvider().GetRequiredService<CustomUserIdInterceptor>()));

        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<IAccountService, AccountProcedureService>();
        services.AddScoped<IOperationService, OperationProcedureService>();
        services.AddScoped<IOperationTypeService, OperationTypeProcedureService>();
        services.AddScoped<IDeviceService, DeviceProcedureService>();

        Services = services.BuildServiceProvider();

        using (var scope = Services.CreateScope())
        {
            var accountService = scope.ServiceProvider.GetService<IAccountService>();
            Application.Run(new LoginForm(accountService , configuration));
        }
    }


}
