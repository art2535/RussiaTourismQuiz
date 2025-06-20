using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RussiaTourismQuiz
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ���������� �����
            builder.Services.AddRazorPages();
            builder.Services.AddDistributedMemoryCache(); // ��������� ��� ������
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(100); // ����� ����� ������
                options.Cookie.HttpOnly = true; // ������ ���� �� ���������� ��������
                options.Cookie.IsEssential = true; // ������������ ���� ��� GDPR
            });

            var app = builder.Build();

            // ��������� ��������� ��������� ��������
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession(); // ��������� middleware ������
            app.MapRazorPages();

            // ���������� ���� ��� ��������������� �������� �������� (������ �� Windows, ��� Docker)
            var url = "http://localhost:5000";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !IsRunningInDocker())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }

            app.Run();
        }

        private static bool IsRunningInDocker()
        {
            return File.Exists("/.dockerenv") || Directory.Exists("/.dockerinit");
        }
    }
}