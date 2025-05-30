namespace RussiaTourismQuiz.Models
{
    public class AnswersData
    {
        public string? SessionId { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}