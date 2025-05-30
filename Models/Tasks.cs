namespace RussiaTourismQuiz.Models
{
    public class Tasks
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Question { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public List<string>? Options { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
        public Dictionary<string, string>? Matches { get; set; }
        public List<TrueFalseStatement>? TrueFalseStatements { get; set; }
        public Crossword? Crossword { get; set; }
        public List<Clue>? Clues { get; set; } // Новое свойство
        public GridSize? GridSize { get; set; } // Новое свойство
        public string? ExtraQuestion { get; set; }
        public List<string>? ExtraOptions { get; set; }
        public string? ExtraCorrectAnswer { get; set; }
    }
}