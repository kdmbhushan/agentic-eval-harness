namespace AgenticEval.Core;

public sealed record TrajectoryStep(
    int StepIndex,
    string Source,
    string Action,
    IReadOnlyDictionary<string, string> Arguments,
    string Output,
    int TokensUsed,
    TimeSpan Latency,
    bool IsSuccess
);

public sealed record TrajectoryLog(
    string RunId,
    string SpecId,
    string ModelName,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    IReadOnlyList<TrajectoryStep> Steps
)
{
    public int TotalTokens => Steps.Sum(s => s.TokensUsed);
    public TimeSpan TotalDuration => CompletedAt - StartedAt;
}

public sealed record AssertionResult(
    string CriterionName,
    bool Passed,
    string Explanation,
    IReadOnlyDictionary<string, object>? Details = null
);

public sealed record EvaluationReport(
    string RunId,
    string SpecId,
    IReadOnlyList<AssertionResult> Results
)
{
    public bool AllPassed => Results.Count > 0 && Results.All(r => r.Passed);
    public int TotalAssertions => Results.Count;
    public int PassedCount => Results.Count(r => r.Passed);
    public int FailedCount => Results.Count(r => !r.Passed);
    public double PassRate => Results.Count == 0 ? 0.0 : (double)PassedCount / Results.Count * 100.0;
}
