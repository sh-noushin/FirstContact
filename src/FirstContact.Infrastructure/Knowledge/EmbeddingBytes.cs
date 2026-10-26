namespace FirstContact.Infrastructure.Knowledge;

/// <summary>
/// SQLite has no array column, so an embedding is stored as the raw bytes of its floats.
/// 1536 numbers become 6144 bytes, which is small enough to keep beside the article.
/// </summary>
public static class EmbeddingBytes
{
    public static byte[] From(float[] vector)
    {
        byte[] bytes = new byte[vector.Length * sizeof(float)];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);

        return bytes;
    }

    public static float[] To(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0 || bytes.Length % sizeof(float) != 0)
        {
            return [];
        }

        float[] vector = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, vector, 0, bytes.Length);

        return vector;
    }
}
