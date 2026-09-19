using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace MathDuel
{
    /// <summary>
    /// Generates a <see cref="QuestionData"/> based on current <see cref="GameSettings"/>.
    /// Supports basic arithmetic and a user-defined f(x) expression.
    /// </summary>
    public static class QuestionGenerator
    {
        public static QuestionData Generate()
        {
            switch (GameSettings.Mode)
            {
                case OperationMode.Addition:       return GenerateArithmetic("+");
                case OperationMode.Subtraction:    return GenerateArithmetic("-");
                case OperationMode.Multiplication: return GenerateArithmetic("*");
                case OperationMode.Division:       return GenerateDivision();
                case OperationMode.CustomFunction: return GenerateCustomFunction();
                default:                           return GenerateArithmetic("+");
            }
        }

        // ── basic +, -, × ──────────────────────────────────────────────
        private static QuestionData GenerateArithmetic(string op)
        {
            int max = GameSettings.MaxNumber;
            int a = Random.Range(1, max + 1);
            int b = Random.Range(1, max + 1);

            // for subtraction keep result >= 0
            if (op == "-" && a < b) { int t = a; a = b; b = t; }

            int answer;
            string symbol;
            switch (op)
            {
                case "+": answer = a + b; symbol = "+"; break;
                case "-": answer = a - b; symbol = "−"; break;
                case "*": answer = a * b; symbol = "×"; break;
                default:  answer = a + b; symbol = "+"; break;
            }

            return BuildQuestion($"{a} {symbol} {b} = ?", answer);
        }

        // ── division (always exact) ────────────────────────────────────
        private static QuestionData GenerateDivision()
        {
            int max = GameSettings.MaxNumber;
            int b = Random.Range(1, max + 1);
            int answer = Random.Range(1, max + 1);
            int a = b * answer; // guarantees whole-number result

            return BuildQuestion($"{a} ÷ {b} = ?", answer);
        }

        // ── custom f(x) ───────────────────────────────────────────────
        private static QuestionData GenerateCustomFunction()
        {
            int max = GameSettings.MaxNumber;
            int x = Random.Range(1, max + 1);
            string expr = GameSettings.CustomFunction;

            int answer = EvaluateExpression(expr, x);

            string displayExpr = expr.Replace("*", "·");
            return BuildQuestion($"f(x) = {displayExpr}\nx = {x}\nf({x}) = ?", answer);
        }

        /// <summary>
        /// Evaluates a simple math expression containing 'x' by replacing 'x' with
        /// the given value and using DataTable.Compute for evaluation.
        /// Supports: +, -, *, /, parentheses.
        /// </summary>
        private static int EvaluateExpression(string expression, int x)
        {
            try
            {
                // Handle implicit multiplication: "2x" → "2*x"
                string processed = System.Text.RegularExpressions.Regex.Replace(
                    expression, @"(\d)(x)", "$1*$2");

                // Replace x with value
                processed = processed.Replace("x", x.ToString());

                DataTable dt = new DataTable();
                object result = dt.Compute(processed, "");
                return System.Convert.ToInt32(System.Math.Round(System.Convert.ToDouble(result)));
            }
            catch
            {
                Debug.LogWarning($"Could not evaluate expression '{expression}' with x={x}. Falling back to addition.");
                int b = Random.Range(1, GameSettings.MaxNumber + 1);
                return x + b;
            }
        }

        // ── helpers ────────────────────────────────────────────────────
        private static QuestionData BuildQuestion(string text, int correct)
        {
            int[] answers = new int[3];
            answers[0] = correct;

            // generate 2 unique distractors close to the correct answer
            HashSet<int> used = new HashSet<int> { correct };
            for (int i = 1; i <= 2; i++)
            {
                int distractor;
                int tries = 0;
                do
                {
                    int offset = Random.Range(1, 6) * (Random.value > 0.5f ? 1 : -1);
                    distractor = correct + offset;
                    tries++;
                } while (used.Contains(distractor) && tries < 50);

                used.Add(distractor);
                answers[i] = distractor;
            }

            // Fisher-Yates shuffle
            for (int i = answers.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = answers[i];
                answers[i] = answers[j];
                answers[j] = tmp;
            }

            return new QuestionData
            {
                QuestionText = text,
                CorrectAnswer = correct,
                AllAnswers = answers
            };
        }
    }
}
