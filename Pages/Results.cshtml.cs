using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RussiaTourismQuiz.Pages
{
    public class ResultsModel : PageModel
    {
        public int Score { get; set; }
        public string Message { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            Score = HttpContext.Session.GetInt32("Score").GetValueOrDefault();
            var language = HttpContext.Session.GetString("Language") ?? "ru";
            var isEnglish = language == "en";

            Message = isEnglish
                ? Score switch
                {
                    >= 160 => "🏆 Expert in Russian tourism",
                    >= 130 => "💼 Advanced knowledge",
                    >= 100 => "📚 Intermediate level",
                    >= 70 => "🧭 Beginner level",
                    _ => "🔍 Needs improvement"
                }
                : Score switch
                {
                    >= 160 => "🏆 Эксперт по туризму России",
                    >= 130 => "💼 Продвинутые знания",
                    >= 100 => "📚 Средний уровень",
                    >= 70 => "🧭 Начальный уровень",
                    _ => "🔍 Требуется улучшение"
                };

            HttpContext.Session.Clear();
            HttpContext.Session.SetString("Language", language);
            return Page();
        }

        public IActionResult OnPostReset()
        {
            var language = HttpContext.Session.GetString("Language") ?? "ru";
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("Language", language);
            return RedirectToPage("/Index");
        }
    }
}