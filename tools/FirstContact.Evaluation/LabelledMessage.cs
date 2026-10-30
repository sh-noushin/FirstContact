namespace FirstContact.Evaluation;

/// <summary>One test message and the type it should be classified as.</summary>
public record LabelledMessage(string Message, string Expected);
