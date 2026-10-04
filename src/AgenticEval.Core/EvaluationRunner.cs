namespace AgenticEval.Core;

public interface IEvaluationAssertion
{
    string Name { get; }
    AssertionResult Evaluate(TrajectoryLog trajectory);
}

public sealed class ProhibitedToolsAssertion : IEvaluationAssertion
{
    private readonly HashSet<string> _prohibitedTools;

    public string Name => "ProhibitedToolsCheck";

    public ProhibitedToolsAssertion(IEnumerable<string> prohibitedTools)
    {
        _prohibitedTools = new HashSet<string>(prohibitedTools, StringComparer.OrdinalIgnoreCase);
    }

    public AssertionResult Evaluate(TrajectoryLog trajectory)
    {
        var violations = trajectory.Steps
            .Where(s => _prohibitedTools.Contains(s.Action))
            .Select(s => $"Step {s.StepIndex}: Invoked prohibited tool '{s.Action}'")
            .ToList();

        if (violations.Count > 0)
        {
            return new AssertionResult(
                Name,
                Passed: false,
                Explanation: $"Prohibited tool usage detected: {string.Join("; ", violations)}",
                Details: new Dictionary<string, object> { ["Violations"] = violations }
            );
        }

        return new AssertionResult(
            Name,
            Passed: true,
            Explanation: "No prohibited tools were invoked during trajectory execution."
        );
    }
}

public sealed class StepBudgetAssertion : IEvaluationAssertion
{
    private readonly int _maxAllowedSteps;

    public string Name => "StepBudgetCheck";

    public StepBudgetAssertion(int maxAllowedSteps)
    {
        _maxAllowedSteps = maxAllowedSteps;
    }

    public AssertionResult Evaluate(TrajectoryLog trajectory)
    {
        var stepCount = trajectory.Steps.Count;
        var passed = stepCount <= _maxAllowedSteps;
        return new AssertionResult(
            Name,
            Passed: passed,
            Explanation: passed
                ? $"Trajectory completed in {stepCount} steps (within budget of {_maxAllowedSteps})."
                : $"Trajectory exceeded step budget: {stepCount} steps taken, max allowed is {_maxAllowedSteps}."
        );
    }
}

public sealed class TokenBudgetAssertion : IEvaluationAssertion
{
    private readonly int _maxTokens;

    public string Name => "TokenBudgetCheck";

    public TokenBudgetAssertion(int maxTokens)
    {
        _maxTokens = maxTokens;
    }

    public AssertionResult Evaluate(TrajectoryLog trajectory)
    {
        var totalTokens = trajectory.TotalTokens;
        var passed = totalTokens <= _maxTokens;
        return new AssertionResult(
            Name,
            Passed: passed,
            Explanation: passed
                ? $"Total tokens consumed: {totalTokens} (budget: {_maxTokens})."
                : $"Trajectory exceeded token budget: {totalTokens} consumed, max budget was {_maxTokens}."
        );
    }
}

public sealed class SequenceOrderAssertion : IEvaluationAssertion
{
    private readonly IReadOnlyList<string> _expectedSequence;

    public string Name => "SequenceOrderCheck";

    public SequenceOrderAssertion(IReadOnlyList<string> expectedSequence)
    {
        _expectedSequence = expectedSequence;
    }

    public AssertionResult Evaluate(TrajectoryLog trajectory)
    {
        var actionList = trajectory.Steps.Select(s => s.Action).ToList();
        var currentIndex = 0;

        foreach (var expectedAction in _expectedSequence)
        {
            var foundAt = actionList.FindIndex(currentIndex, a => string.Equals(a, expectedAction, StringComparison.OrdinalIgnoreCase));
            if (foundAt == -1)
            {
                return new AssertionResult(
                    Name,
                    Passed: false,
                    Explanation: $"Expected sequence action '{expectedAction}' was not found after previous step in trajectory."
                );
            }
            currentIndex = foundAt + 1;
        }

        return new AssertionResult(
            Name,
            Passed: true,
            Explanation: $"All { _expectedSequence.Count} required sequence steps executed in correct chronological order."
        );
    }
}

public sealed class EvaluationRunner
{
    private readonly List<IEvaluationAssertion> _assertions = new();

    public EvaluationRunner AddAssertion(IEvaluationAssertion assertion)
    {
        _assertions.Add(assertion);
        return this;
    }

    public EvaluationReport Run(TrajectoryLog trajectory)
    {
        var results = _assertions.Select(a => a.Evaluate(trajectory)).ToList();
        return new EvaluationReport(trajectory.RunId, trajectory.SpecId, results);
    }
}
