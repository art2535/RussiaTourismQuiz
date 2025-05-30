using System.Diagnostics;

namespace RussiaTourismQuiz
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавление служб
            builder.Services.AddRazorPages();
            builder.Services.AddDistributedMemoryCache(); // Хранилище для сессий
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(100); // Время жизни сессии
                options.Cookie.HttpOnly = true; // Защита куки от клиентских скриптов
                options.Cookie.IsEssential = true; // Обязательная куки для GDPR
            });

            var app = builder.Build();

            // Настройка конвейера обработки запросов
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession(); // Включение middleware сессий
            app.MapRazorPages();

            // Добавление кода для автоматического открытия браузера
            var url = "http://localhost:5000";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

            app.Run();
        }
    }
}