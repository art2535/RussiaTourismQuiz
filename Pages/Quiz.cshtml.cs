using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RussiaTourismQuiz.Models;
using System.Text.Json;
using FileJson = System.IO.File;

namespace RussiaTourismQuiz.Pages
{
    public class QuizModel : PageModel
    {
        public List<Station>? Stations { get; set; }
        public Station? Station { get; set; }
        public Tasks? Tasks { get; set; }
        public int StationIndex { get; set; }
        public int TaskIndex { get; set; }
        public int Score { get; set; }
        public string? Feedback { get; set; }
        public string? SelectedAnswer { get; set; }
        public string? ExtraFeedback { get; set; }
        public string? ExtraSelectedAnswer { get; set; }
        public bool IsLastTask { get; set; }
        public int PrevStationIndex { get; set; }
        public int PrevTaskIndex { get; set; }
        public int NextStationIndex { get; set; }
        public int NextTaskIndex { get; set; }
        public double SessionTimeRemainingSeconds { get; set; }

        private readonly string _answersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "user-answers.json");

        public IActionResult OnGet(int stationIndex, int taskIndex = 0, double sessionDurationMinutes = 45)
        {
            Initialize(stationIndex, taskIndex);

            var sessionStart = HttpContext.Session.GetString("SessionStart");
            if (string.IsNullOrEmpty(sessionStart))
            {
                sessionStart = DateTime.UtcNow.ToString("o");
                HttpContext.Session.SetString("SessionStart", Guid.NewGuid().ToString());
                HttpContext.Session.SetInt32("Score", 0);
                SessionTimeRemainingSeconds = sessionDurationMinutes * 60;
            }
            else
            {
                try
                {
                    var startTime = DateTime.Parse(sessionStart, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();
                    var timeElapsed = DateTime.UtcNow - startTime;
                    var totalSessionSeconds = sessionDurationMinutes * 60;
                    SessionTimeRemainingSeconds = Math.Max(0, totalSessionSeconds - timeElapsed.TotalSeconds);
                }
                catch (FormatException)
                {
                    sessionStart = DateTime.UtcNow.ToString("o");
                    HttpContext.Session.SetString("SessionStart", sessionStart);
                    HttpContext.Session.SetString("SessionId", Guid.NewGuid().ToString());
                    HttpContext.Session.SetInt32("Score", 0);
                    SessionTimeRemainingSeconds = sessionDurationMinutes * 60;
                }
            }
            HttpContext.Session.SetString("SessionStart", sessionStart); // Сохраняем SessionStart

            LoadPreviousAnswer(stationIndex, taskIndex);
            return Page();
        }

        public IActionResult OnPostAnswer(int stationIndex, int taskIndex, string answer)
        {
            Initialize(stationIndex, taskIndex);
            var isEnglish = HttpContext.Session.GetString("Language") == "en";
            if (Tasks?.Type == "multiple-choice")
            {
                SelectedAnswer = answer;
                bool isCorrect = answer == Tasks.CorrectAnswer;
                Feedback = isCorrect
                    ? (isEnglish ? "Correct!" : "Правильно!")
                    : (isEnglish
                        ? $"Incorrect."
                        : $"Неправильно.");
                if (isCorrect)
                {
                    HttpContext.Session.SetInt32("Score", HttpContext.Session.GetInt32("Score").GetValueOrDefault() + 10);
                }
                SaveAnswer(stationIndex, taskIndex, Tasks.Type, answer, Tasks.CorrectAnswer, isCorrect);
            }
            if (Tasks?.Type == "text-input")
            {
                SelectedAnswer = answer;
                bool isCorrect = string.Equals(answer?.Trim(), Tasks.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
                if (string.IsNullOrEmpty(answer))
                {
                    Feedback = isEnglish ? "No answer entered." : "Ответ не введён.";
                }
                else if (!string.IsNullOrEmpty(answer))
                {
                    Feedback = isCorrect
                        ? (isEnglish ? "Correct!" : "Правильно!")
                        : (isEnglish ? "Incorrect." : "Неправильно.");
                    if (isCorrect)
                    {
                        HttpContext.Session.SetInt32("Score", HttpContext.Session.GetInt32("Score").GetValueOrDefault() + 10);
                    }
                }
                SaveAnswer(stationIndex, taskIndex, Tasks.Type, answer, Tasks.CorrectAnswer, isCorrect);
            }
            Score = HttpContext.Session.GetInt32("Score").GetValueOrDefault();
            return Page();
        }

        public IActionResult OnPostExtraAnswer(int stationIndex, int taskIndex, string extraAnswer)
        {
            Initialize(stationIndex, taskIndex);
            var isEnglish = HttpContext.Session.GetString("Language") == "en";
            if (Tasks?.ExtraOptions != null)
            {
                ExtraSelectedAnswer = extraAnswer;
                bool isCorrect = extraAnswer == Tasks.ExtraCorrectAnswer;
                ExtraFeedback = isCorrect
                    ? (isEnglish ? "Correct!" : "Правильно!")
                    : (isEnglish
                        ? $"Incorrect."
                        : $"Неправильно.");
                if (isCorrect)
                {
                    HttpContext.Session.SetInt32("Score", HttpContext.Session.GetInt32("Score").GetValueOrDefault() + 10);
                }
                SaveAnswer(stationIndex, taskIndex, "extra-" + Tasks.Type, extraAnswer, Tasks.ExtraCorrectAnswer, isCorrect);
            }
            else if (!string.IsNullOrEmpty(extraAnswer))
            {
                ExtraSelectedAnswer = extraAnswer;
                bool isCorrect = extraAnswer.Equals(Tasks?.ExtraCorrectAnswer, StringComparison.OrdinalIgnoreCase);
                ExtraFeedback = isCorrect
                    ? (isEnglish ? "Correct!" : "Правильно!")
                    : (isEnglish
                        ? $"Incorrect."
                        : $"Неправильно.");
                if (isCorrect)
                {
                    HttpContext.Session.SetInt32("Score", HttpContext.Session.GetInt32("Score").GetValueOrDefault() + 10);
                }
                SaveAnswer(stationIndex, taskIndex, "extra-text-input", extraAnswer, Tasks?.ExtraCorrectAnswer, isCorrect);
            }
            Score = HttpContext.Session.GetInt32("Score").GetValueOrDefault();
            return Page();
        }

        public IActionResult OnPostDragAndDrop(int stationIndex, int taskIndex, Dictionary<string, string> answers)
        {
            Initialize(stationIndex, taskIndex);
            var isEnglish = HttpContext.Session.GetString("Language") == "en";
            SelectedAnswer = JsonSerializer.Serialize(answers);
            bool isCorrect = Tasks.Matches.All(kv => answers.ContainsKey(kv.Key) && answers[kv.Key] == kv.Value);
            Feedback = isCorrect ? (isEnglish ? "Correct!" : "Правильно!")
                : (isEnglish ? $"Incorrect." : $"Неправильно");
            /*Feedback = isCorrect
                ? (isEnglish ? "Correct!" : "Правильно!")
                : (isEnglish
                    ? $"Incorrect. Correct pairs: {string.Join(", ", Tasks.Matches.Select(kv => $"{kv.Key} -> {kv.Value}"))}"
                    : $"Неправильно. Правильные пары: {string.Join(", ", Tasks.Matches.Select(kv => $"{kv.Key} -> {kv.Value}"))}");*/
            if (isCorrect)
            {
                HttpContext.Session.SetInt32("Score", HttpContext.Session.GetInt32("Score").GetValueOrDefault() + 10);
            }
            SaveAnswer(stationIndex, taskIndex, Tasks.Type, SelectedAnswer, JsonSerializer.Serialize(Tasks.Matches), isCorrect);
            Score = HttpContext.Session.GetInt32("Score").GetValueOrDefault();
            return Page();
        }

        public IActionResult OnPostTrueFalse(int stationIndex, int taskIndex, List<string> answers)
        {
            Initialize(stationIndex, taskIndex);
            var isEnglish = HttpContext.Session.GetString("Language") == "en";
            SelectedAnswer = JsonSerializer.Serialize(answers);
            bool isCorrect = true;
            for (int i = 0; i < Tasks.TrueFalseStatements.Count && i < answers.Count; i++)
            {
                bool userAnswer = answers[i].ToLower() == "true";
                if (userAnswer != Tasks.TrueFalseStatements[i].IsTrue)
                {
                    isCorrect = false;
                    break;
                }
            }
            Feedback = isCorrect
                ? (isEnglish ? "Correct!" : "Правильно!")
                : (isEnglish
                    ? $"Incorrect."
                    : $"Неправильно.");
            if (isCorrect)
            {
                HttpContext.Session.SetInt32("Score", HttpContext.Session.GetInt32("Score").GetValueOrDefault() + 10);
            }
            SaveAnswer(stationIndex, taskIndex, Tasks.Type, SelectedAnswer, JsonSerializer.Serialize(Tasks.TrueFalseStatements), isCorrect);
            Score = HttpContext.Session.GetInt32("Score").GetValueOrDefault();
            return Page();
        }

        public IActionResult OnPostCrossword(int stationIndex, int taskIndex, Dictionary<int, string> crosswordAnswers)
        {
            Initialize(stationIndex, taskIndex);
            var isEnglish = HttpContext.Session.GetString("Language") == "en";
            if (Tasks?.Crossword == null || Tasks.Crossword.Clues == null || Tasks.Crossword.GridSize == null)
            {
                Feedback = isEnglish ? "Error: The crossword puzzle data is unavailable." : "Ошибка: данные кроссворда недоступны.";
                return Page();
            }

            var normalizedAnswers = crosswordAnswers.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.ToUpper() ?? ""
            );
            SelectedAnswer = JsonSerializer.Serialize(normalizedAnswers);

            bool isCorrect = true;
            var grid = new char[Tasks.Crossword.GridSize.Rows, Tasks.Crossword.GridSize.Cols];
            var filledCells = new HashSet<int>();

            foreach (var clue in Tasks.Crossword.Clues)
            {
                if (clue == null) continue;
                int row = clue.Row - 1;
                int col = clue.Col - 1;
                for (int i = 0; i < clue.Answer.Length; i++)
                {
                    int cellIndex = clue.Direction == "across"
                        ? row * Tasks.Crossword.GridSize.Cols + (col + i)
                        : (row + i) * Tasks.Crossword.GridSize.Cols + col;

                    if (filledCells.Contains(cellIndex) && clue.Direction == "down")
                    {
                        continue;
                    }

                    string expectedChar = clue.Answer[i].ToString().ToUpper();
                    string receivedChar = normalizedAnswers.ContainsKey(cellIndex) ? normalizedAnswers[cellIndex] : "";
                    if (!normalizedAnswers.ContainsKey(cellIndex) || receivedChar != expectedChar)
                    {
                        isCorrect = false;
                    }

                    if (clue.Direction == "across")
                        grid[row, col + i] = clue.Answer[i];
                    else
                        grid[row + i, col] = clue.Answer[i];
                    filledCells.Add(cellIndex);
                }
            }

            Feedback = isCorrect
                ? (isEnglish ? "Correct!" : "Правильно!")
                : (isEnglish
                    ? $"Incorrect. Correct answers: {string.Join(", ", Tasks.Crossword.Clues.Where(c => c != null).Select(c => $"{c.Number}. {c.Answer}"))}"
                    : $"Неправильно. Правильные ответы: {string.Join(", ", Tasks.Crossword.Clues.Where(c => c != null).Select(c => $"{c.Number}. {c.Answer}"))}");
            if (isCorrect)
            {
                HttpContext.Session.SetInt32("Score", HttpContext.Session.GetInt32("Score").GetValueOrDefault() + 10);
            }
            SaveAnswer(stationIndex, taskIndex, Tasks.Type, SelectedAnswer, JsonSerializer.Serialize(Tasks.Crossword.Clues), isCorrect);
            Score = HttpContext.Session.GetInt32("Score").GetValueOrDefault();
            return Page();
        }

        private void Initialize(int stationIndex, int taskIndex)
        {
            var isEnglish = HttpContext.Session.GetString("Language") == "en";
            Stations = GetStations(isEnglish);
            StationIndex = stationIndex < 0 || stationIndex >= Stations.Count ? 0 : stationIndex;
            Station = Stations[StationIndex];
            TaskIndex = taskIndex < 0 || taskIndex >= Station.Tasks.Count ? 0 : taskIndex;
            Tasks = Station.Tasks[TaskIndex];
            Score = HttpContext.Session.GetInt32("Score").GetValueOrDefault();

            IsLastTask = stationIndex == Stations.Count - 1 && taskIndex == Station.Tasks.Count - 1;

            if (taskIndex > 0)
            {
                PrevStationIndex = stationIndex;
                PrevTaskIndex = taskIndex - 1;
            }
            else if (stationIndex > 0)
            {
                PrevStationIndex = stationIndex - 1;
                PrevTaskIndex = Stations[PrevStationIndex].Tasks.Count - 1;
            }
            else
            {
                PrevStationIndex = 0;
                PrevTaskIndex = 0;
            }

            if (taskIndex < Station.Tasks.Count - 1)
            {
                NextStationIndex = stationIndex;
                NextTaskIndex = taskIndex + 1;
            }
            else if (stationIndex < Stations.Count - 1)
            {
                NextStationIndex = stationIndex + 1;
                NextTaskIndex = 0;
            }
            else
            {
                NextStationIndex = stationIndex;
                NextTaskIndex = taskIndex;
            }
        }

        private void SaveAnswer(int stationIndex, int taskIndex, string type, string selectedAnswer, string correctAnswer, bool isCorrect)
        {
            var sessionId = HttpContext.Session.GetString("SessionId") ?? Guid.NewGuid().ToString();
            var answersData = FileJson.Exists(_answersFilePath)
                ? JsonSerializer.Deserialize<AnswersData>(FileJson.ReadAllText(_answersFilePath)) ?? new AnswersData { SessionId = sessionId, Answers = new List<Answer>() }
                : new AnswersData { SessionId = sessionId, Answers = new List<Answer>() };

            answersData.Answers.RemoveAll(a => a.SessionId == sessionId && a.StationIndex == stationIndex && a.TaskIndex == taskIndex && a.Type == type);

            answersData.Answers.Add(new Answer
            {
                SessionId = sessionId,
                StationIndex = stationIndex,
                TaskIndex = taskIndex,
                Type = type,
                SelectedAnswer = selectedAnswer,
                CorrectAnswer = correctAnswer,
                IsCorrect = isCorrect,
                Timestamp = DateTime.UtcNow.ToString("o")
            });

            try
            {
                FileJson.WriteAllText(_answersFilePath, JsonSerializer.Serialize(answersData, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                return;
            }
        }

        private void LoadPreviousAnswer(int stationIndex, int taskIndex)
        {
            if (!FileJson.Exists(_answersFilePath))
            {
                return;
            }

            var isEnglish = HttpContext.Session.GetString("Language") == "en";
            try
            {
                var sessionId = HttpContext.Session.GetString("SessionId");
                var jsonContent = FileJson.ReadAllText(_answersFilePath);
                var answersData = JsonSerializer.Deserialize<AnswersData>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (answersData?.Answers == null)
                {
                    return;
                }

                var answer = answersData.Answers.FirstOrDefault(a => a.SessionId == sessionId && a.StationIndex == stationIndex && a.TaskIndex == taskIndex && a.Type == Tasks.Type);
                if (answer != null)
                {
                    SelectedAnswer = answer.SelectedAnswer;
                    Feedback = answer.IsCorrect
                        ? (isEnglish ? "Correct!" : "Правильно!")
                        : (isEnglish
                            ? $"Incorrect."
                            : $"Неправильно.");
                }

                var extraAnswer = answersData.Answers.FirstOrDefault(a => a.SessionId == sessionId && a.StationIndex == stationIndex && a.TaskIndex == taskIndex && a.Type.StartsWith("extra-"));
                if (extraAnswer != null)
                {
                    ExtraSelectedAnswer = extraAnswer.SelectedAnswer;
                    ExtraFeedback = extraAnswer.IsCorrect
                        ? (isEnglish ? "Correct!" : "Правильно!")
                        : (isEnglish
                            ? $"Incorrect."
                            : $"Неправильно.");
                }
            }
            catch (JsonException)
            {
                ResetAnswersFile();
            }
        }

        private void ResetAnswersFile()
        {
            var sessionId = HttpContext.Session.GetString("SessionId") ?? Guid.NewGuid().ToString();
            try
            {
                FileJson.WriteAllText(_answersFilePath, JsonSerializer.Serialize(new AnswersData
                {
                    SessionId = sessionId,
                    Answers = new List<Answer>()
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                return;
            }
        }

        private List<Station> GetStations(bool isEnglish)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", isEnglish ? "stations-en.json" : "stations.json");
            if (!FileJson.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {(isEnglish ? "stations-en.json" : "stations.json")}");
            }
            var json = FileJson.ReadAllText(filePath);
            var stations = JsonSerializer.Deserialize<List<Station>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Station>();
            return stations;
        }
    }
}