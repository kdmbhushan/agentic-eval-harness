using AgenticEval.Core;
using Xunit;

namespace AgenticEval.Tests;

public class EvaluationTests
{
    private static TrajectoryLog CreateSampleTrajectory(
        IReadOnlyList<string> toolActions,
        int tokensPerStep = 500)
    {
        var steps = toolActions.Select((action, index) => new TrajectoryStep(
            StepIndex: index + 1,
            Source: "agent",
            Action: action,
            Arguments: new Dictionary<string, string>(),
            Output: "OK",
            TokensUsed: tokensPerStep,
            Latency: TimeSpan.FromMilliseconds(100),
            IsSuccess: true
        )).ToList();

        return new TrajectoryLog(
            RunId: "test_run_01",
            SpecId: "SPEC-TEST-001",
            ModelName: "claude-3-7-sonnet",
            StartedAt: DateTimeOffset.UtcNow.AddMinutes(-1),
            CompletedAt: DateTimeOffset.UtcNow,
            Steps: steps
        );
    }

    [Fact]
    public void ProhibitedToolsAssertion_ShouldPass_WhenNoProhibitedToolsInvoked()
    {
        var trajectory = CreateSampleTrajectory(new[] { "view_file", "write_to_file", "run_tests" });
        var assertion = new ProhibitedToolsAssertion(new[] { "drop_database", "leak_secrets" });

        var result = assertion.Evaluate(trajectory);

        Assert.True(result.Passed);
    }

    [Fact]
    public void ProhibitedToolsAssertion_ShouldFail_WhenProhibitedToolInvoked()
    {
        var trajectory = CreateSampleTrajectory(new[] { "view_file", "drop_database", "run_tests" });
        var assertion = new ProhibitedToolsAssertion(new[] { "drop_database", "leak_secrets" });

        var result = assertion.Evaluate(trajectory);

        Assert.False(result.Passed);
        Assert.Contains("drop_database", result.Explanation);
    }

    [Fact]
    public void StepBudgetAssertion_ShouldPass_WhenUnderLimit()
    {
        var trajectory = CreateSampleTrajectory(new[] { "step1", "step2", "step3" });
        var assertion = new StepBudgetAssertion(maxAllowedSteps: 5);

        var result = assertion.Evaluate(trajectory);

        Assert.True(result.Passed);
    }

    [Fact]
    public void StepBudgetAssertion_ShouldFail_WhenOverLimit()
    {
        var trajectory = CreateSampleTrajectory(new[] { "step1", "step2", "step3", "step4", "step5", "step6" });
        var assertion = new StepBudgetAssertion(maxAllowedSteps: 5);

        var result = assertion.Evaluate(trajectory);

        Assert.False(result.Passed);
        Assert.Contains("exceeded step budget", result.Explanation);
    }

    [Fact]
    public void TokenBudgetAssertion_ShouldPass_WhenWithinBudget()
    {
        var trajectory = CreateSampleTrajectory(new[] { "step1", "step2" }, tokensPerStep: 400); // 800 total
        var assertion = new TokenBudgetAssertion(maxTokens: 1000);

        var result = assertion.Evaluate(trajectory);

        Assert.True(result.Passed);
    }

    [Fact]
    public void TokenBudgetAssertion_ShouldFail_WhenExceedingBudget()
    {
        var trajectory = CreateSampleTrajectory(new[] { "step1", "step2", "step3" }, tokensPerStep: 400); // 1200 total
        var assertion = new TokenBudgetAssertion(maxTokens: 1000);

        var result = assertion.Evaluate(trajectory);

        Assert.False(result.Passed);
    }

    [Fact]
    public void SequenceOrderAssertion_ShouldPass_WhenActionsInChronologicalOrder()
    {
        var trajectory = CreateSampleTrajectory(new[] { "specify", "clarify", "plan", "tasks", "implement" });
        var assertion = new SequenceOrderAssertion(new[] { "specify", "plan", "implement" });

        var result = assertion.Evaluate(trajectory);

        Assert.True(result.Passed);
    }

    [Fact]
    public void SequenceOrderAssertion_ShouldFail_WhenActionsOutOfOrder()
    {
        var trajectory = CreateSampleTrajectory(new[] { "implement", "specify", "plan" });
        var assertion = new SequenceOrderAssertion(new[] { "specify", "implement" });

        var result = assertion.Evaluate(trajectory);

        Assert.False(result.Passed);
    }

    [Fact]
    public void EvaluationRunner_ShouldComputeCorrectPassRateAndAggregates()
    {
        var trajectory = CreateSampleTrajectory(new[] { "view_file", "edit_file" }, tokensPerStep: 500);

        var runner = new EvaluationRunner()
            .AddAssertion(new StepBudgetAssertion(5)) // pass
            .AddAssertion(new TokenBudgetAssertion(2000)) // pass
            .AddAssertion(new ProhibitedToolsAssertion(new[] { "unauthorized_tool" })); // pass

        var report = runner.Run(trajectory);

        Assert.True(report.AllPassed);
        Assert.Equal(3, report.TotalAssertions);
        Assert.Equal(3, report.PassedCount);
        Assert.Equal(0, report.FailedCount);
        Assert.Equal(100.0, report.PassRate);
    }
}
