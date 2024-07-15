using System.Runtime.InteropServices;

namespace JdScoring
{
    public class ScoreManager
    {
        private readonly int scoringID = -1;
#if (DEBUGX86 || DEBUGX64)
        private const string dllPath = @"Assemblies\jdScoring.dll";
#elif (RELEASEX86 || RELEASEX64)
        private const string dllPath = @"jdScoring.dll";
#endif

        [DllImport($"{dllPath}")]
        private static extern int init();

        public ScoreManager()
        {
            scoringID = init();
        }

        public int GetScoringID() { return scoringID; }

        [DllImport($"{dllPath}", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool loadClassifier(int scoringID, string moveName, [MarshalAs(UnmanagedType.LPArray)] byte[] source, int sourceLength);

        public bool LoadClassifier(string moveName, byte[] source) => loadClassifier(scoringID, moveName, source, source.Length);

        [DllImport($"{dllPath}", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool loadMove(int scoringID, string moveName, int start, int duration, bool isGold, bool isLastOne);

        public bool LoadMove(string moveName, int start, int duration, bool isGold, bool isLastOne) => loadMove(scoringID, moveName, start, duration, isGold, isLastOne);

        [DllImport($"{dllPath}", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool addSample(int scoringID, float t, float x, float y, float z);

        public bool AddSample(float x, float y, float z, float t) => addSample(scoringID, t, x, y, z);

        [DllImport($"{dllPath}", CallingConvention = CallingConvention.Cdecl)]
        private static extern ScoreResult getLastScore(int scoringID);

        public ScoreResult GetLastScore() => getLastScore(scoringID);

        [DllImport($"{dllPath}", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool endScore(int scoringID);

        public bool EndScore() => endScore(scoringID);
    }

    public struct ScoreResult
    {
        public int moveNum;
        public float totalCalories;
        public float totalScore;
        public int rating;
        [MarshalAs(UnmanagedType.U1)]
        public bool playerIsOnFire;
        [MarshalAs(UnmanagedType.U1)]
        public bool isGoldMove;
    }
}
