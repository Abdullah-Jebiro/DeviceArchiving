using DeviceArchiving.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows.Forms;

namespace DeviceArchiving
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ����� DbContext
            var options = new DbContextOptionsBuilder<DeviceArchivingContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=DeviceArchiving;Trusted_Connection=True;")
                .Options;

            using var context = new DeviceArchivingContext(options);
            context.Database.EnsureCreated();

            // ����� ���� IDeviceService
            var deviceService = new DeviceArchivingService(context);

            // ��� ������ �� ������� Main
            Application.Run(new Main(deviceService));
        }
    }
}