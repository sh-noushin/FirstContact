namespace FirstContact.Infrastructure.Knowledge;

/// <summary>
/// Measures how close two embeddings are in meaning: 1 means the same direction,
/// 0 means unrelated. This is what lets a question find an article that shares no words with it.
/// </summary>
public static class CosineSimilarity
{
    public static double Between(float[] first, float[] second)
    {
        if (first.Length != second.Length || first.Length == 0)
        {
            return 0;
        }

        double dot = 0;
        double lengthOfFirst = 0;
        double lengthOfSecond = 0;

        for (int i = 0; i < first.Length; i++)
        {
            dot += first[i] * second[i];
            lengthOfFirst += first[i] * first[i];
            lengthOfSecond += second[i] * second[i];
        }

        double divisor = Math.Sqrt(lengthOfFirst) * Math.Sqrt(lengthOfSecond);

        return divisor == 0 ? 0 : dot / divisor;
    }
}
