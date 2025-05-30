namespace RussiaTourismQuiz.Models
{
    public class Crossword
    {
        public List<Clue> Clues { get; set; }
        public GridSize GridSize { get; set; }
    }

    public class Clue
    {
        public int Number { get; set; }
        public string Direction { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public string Hint { get; set; }
        public string Answer { get; set; }
    }

    public class GridSize
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
    }
}