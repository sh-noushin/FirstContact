using System.Globalization;
using System.Text;
using FirstContact.Domain.Requests;

namespace FirstContact.Evaluation;

/// <summary>Collects the results of a run and turns them into a readable report.</summary>
public class Scorecard(double threshold)
{
    private readonly List<Result> _results = [];

    public void Add(LabelledMessage labelled, Classification classification) =>
        _results.Add(new Result(
            labelled.Message,
            labelled.Expected,
            classification.Type.ToString(),
            classification.Confidence.Value));

    /// <summary>How often the classifier picked the type a human would have picked.</summary>
    public double Accuracy =>
        _results.Count == 0 ? 0 : (double)_results.Count(result => result.Correct) / _results.Count;

    /// <summary>
    /// How often it was both right and confident enough to act without a person.
    /// This is the number that matters: a correct answer below the threshold still costs
    /// somebody's time.
    /// </summary>
    public double AutomationRate =>
        _results.Count == 0
            ? 0
            : (double)_results.Count(result => result.Correct && result.Confidence >= threshold) / _results.Count;

    /// <summary>
    /// How often it was confidently wrong. These are the dangerous ones: nobody checks them.
    /// </summary>
    public IReadOnlyList<Result> ConfidentlyWrong =>
        _results.Where(result => !result.Correct && result.Confidence >= threshold).ToList();

    public string ToMarkdown()
    {
        var report = new StringBuilder();

        report.AppendLine("# Classification evaluation");
        report.AppendLine();
        report.AppendLine(Line("Messages", _results.Count.ToString(CultureInfo.InvariantCulture)));
        report.AppendLine(Line("Accuracy", Percent(Accuracy)));
        report.AppendLine(Line("Handled automatically and correctly", Percent(AutomationRate)));
        report.AppendLine(Line("Sent to a person", Percent(BelowThreshold)));
        report.AppendLine(Line("Confidently wrong", ConfidentlyWrong.Count.ToString(CultureInfo.InvariantCulture)));
        report.AppendLine();

        report.AppendLine("## By type");
        report.AppendLine();
        report.AppendLine("| Expected | Messages | Correct | Average confidence |");
        report.AppendLine("|---|---|---|---|");

        foreach (IGrouping<string, Result> group in _results.GroupBy(result => result.Expected).OrderBy(g => g.Key))
        {
            report.AppendLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"| {group.Key} | {group.Count()} | {group.Count(r => r.Correct)} "
                    + $"| {group.Average(r => r.Confidence):0.00} |"));
        }

        AppendMistakes(report);

        return report.ToString();
    }

    private double BelowThreshold =>
        _results.Count == 0
            ? 0
            : (double)_results.Count(result => result.Confidence < threshold) / _results.Count;

    private void AppendMistakes(StringBuilder report)
    {
        List<Result> wrong = _results.Where(result => !result.Correct).ToList();

        if (wrong.Count == 0)
        {
            return;
        }

        report.AppendLine();
        report.AppendLine("## Mistakes");
        report.AppendLine();
        report.AppendLine("| Message | Expected | Got | Confidence | Caught by the threshold |");
        report.AppendLine("|---|---|---|---|---|");

        foreach (Result result in wrong.OrderByDescending(result => result.Confidence))
        {
            string caught = result.Confidence < threshold ? "yes" : "**no**";

            report.AppendLine(string.Create(
                CultureInfo.InvariantCulture,
                $"| {result.Message} | {result.Expected} | {result.Actual} "
                + $"| {result.Confidence:0.00} | {caught} |"));
        }
    }

    private static string Line(string name, string value) => $"- **{name}:** {value}";

    private static string Percent(double value) =>
        (value * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";

    public record Result(string Message, string Expected, string Actual, double Confidence)
    {
        public bool Correct => Expected == Actual;
    }
}
