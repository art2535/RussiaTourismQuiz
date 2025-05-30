namespace RussiaTourismQuiz.Models
{
    public class Answer
    {
        public string? SessionId { get; set; }
        public int StationIndex { get; set; }
        public int TaskIndex { get; set; }
        public string? Type { get; set; }
        public string? SelectedAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string? Timestamp { get; set; }
    }
}