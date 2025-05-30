using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;

namespace RussiaTourismQuiz.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30) }
            );
            HttpContext.Session.SetString("Language", culture); // Для совместимости с Quiz.cshtml
            return LocalRedirect(returnUrl);
        }
    }
}