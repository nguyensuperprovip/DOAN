namespace MathDuel
{
    /// <summary>
    /// Immutable data for one question: the display text, the correct answer, and
    /// a shuffled array of 3 answer choices (1 correct + 2 distractors).
    /// </summary>
    public class QuestionData
    {
        public string QuestionText;
        public int CorrectAnswer;
        public int[] AllAnswers; // length 3, shuffled
    }
}
