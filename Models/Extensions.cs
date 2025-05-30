using System.Text.Json;

namespace RussiaTourismQuiz.Models
{
    public static class Extensions
    {
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}