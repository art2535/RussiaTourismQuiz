using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RussiaTourismQuiz.Models;
using System.Text.Json;
using FileJson = System.IO.File;

namespace RussiaTourismQuiz.Pages
{
    public class IndexModel : PageModel
    {
        public List<Station>? Stations { get; set; }

        private readonly string _answersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "user-answers.json");

        public void OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Language")))
            {
                HttpContext.Session.SetString("Language", "en");
            }
        }

        public IActionResult OnPostStartQuiz(string language)
        {
            HttpContext.Session.SetString("Language", language ?? "ru");
            ResetAnswersFile();

            var sessionStart = DateTime.UtcNow.ToString("o");
            var sessionId = Guid.NewGuid().ToString();

            HttpContext.Session.Clear();
            HttpContext.Session.SetString("Language", language ?? "ru");
            HttpContext.Session.SetString("SessionStart", sessionStart);
            HttpContext.Session.SetString("SessionId", sessionId);
            HttpContext.Session.SetInt32("Score", 0);

            return RedirectToPage("/Quiz", new { stationIndex = 0, taskIndex = 0 });
        }

        public IActionResult OnPostChangeLanguage(string language, string returnUrl)
        {
            HttpContext.Session.SetString("Language", language ?? "ru");
            if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/" || !returnUrl.StartsWith("/"))
            {
                return RedirectToPage("/Index");
            }
            return LocalRedirect(returnUrl);
        }

        private void ResetAnswersFile()
        {
            try
            {
                var sessionId = Guid.NewGuid().ToString();
                FileJson.WriteAllText(_answersFilePath, JsonSerializer.Serialize(new
                {
                    SessionId = sessionId,
                    Answers = new List<object>()
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error resetting answers file: {ex.Message}");
            }
        }
    }
}