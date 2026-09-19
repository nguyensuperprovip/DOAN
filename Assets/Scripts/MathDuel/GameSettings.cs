namespace MathDuel
{
    /// <summary>
    /// Static container that persists game configuration between scenes.
    /// SetupMenuController writes values here; GameManager reads them.
    /// </summary>
    public static class GameSettings
    {
        public static OperationMode Mode = OperationMode.Addition;
        public static int MaxNumber = 10;
        public static int TargetScore = 10;
        public static float TimerDuration = 0f;   // 0 = timer off
        public static string CustomFunction = "2*x+3";
    }
}
