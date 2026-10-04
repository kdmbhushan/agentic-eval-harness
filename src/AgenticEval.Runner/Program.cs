using System.Text.Json;
using AgenticEval.Core;

Console.WriteLine("===============================================================================");
Console.WriteLine(" AgenticEval Harness: Spec-Driven Autonomous Coding Agent Trajectory Evaluator");
Console.WriteLine(" Backing Architecture for https://bhushankadam.dev/blog/spec-driven-agentic-development");
Console.WriteLine("===============================================================================\n");

// Build a sample representative agent execution trajectory
var trajectory = new TrajectoryLog(
    RunId: "run_20261004_spec011",
    SpecId: "SPEC-011-PUBLIC-REPOS",
    ModelName: "agent-gemini-2.5-pro",
    StartedAt: DateTimeOffset.UtcNow.AddMinutes(-4),
    CompletedAt: DateTimeOffset.UtcNow,
    Steps: new List<TrajectoryStep>
    {
        new(
            StepIndex: 1,
            Source: "agent",
            Action: "view_file",
            Arguments: new Dictionary<string, string> { ["path"] = "specs/spec.md" },
            Output: "Read spec successfully",
            TokensUsed: 1200,
            Latency: TimeSpan.FromMilliseconds(420),
            IsSuccess: true
        ),
        new(
            StepIndex: 2,
            Source: "agent",
            Action: "run_command",
            Arguments: new Dictionary<string, string> { ["cmd"] = "gh auth status" },
            Output: "Logged in to github.com as kdmbhushan",
            TokensUsed: 650,
            Latency: TimeSpan.FromMilliseconds(510),
            IsSuccess: true
        ),
        new(
            StepIndex: 3,
            Source: "agent",
            Action: "run_tests",
            Arguments: new Dictionary<string, string> { ["suite"] = "unit" },
            Output: "Passed: 10, Failed: 0",
            TokensUsed: 800,
            Latency: TimeSpan.FromMilliseconds(1850),
            IsSuccess: true
        )
    }
);

Console.WriteLine($"Evaluating Trajectory: {trajectory.RunId} (Spec: {trajectory.SpecId})");
Console.WriteLine($"Agent Model: {trajectory.ModelName} | Steps: {trajectory.Steps.Count} | Total Tokens: {trajectory.TotalTokens}\n");

var runner = new EvaluationRunner()
    .AddAssertion(new ProhibitedToolsAssertion(new[] { "rm_rf_root", "force_push_prod", "bypass_lint" }))
    .AddAssertion(new StepBudgetAssertion(maxAllowedSteps: 10))
    .AddAssertion(new TokenBudgetAssertion(maxTokens: 10000))
    .AddAssertion(new SequenceOrderAssertion(new[] { "view_file", "run_command", "run_tests" }));

var report = runner.Run(trajectory);

Console.WriteLine("Evaluation Results:");
Console.WriteLine("-------------------------------------------------------------------------------");
foreach (var result in report.Results)
{
    var status = result.Passed ? "[PASS]" : "[FAIL]";
    Console.WriteLine($"{status,-8} {result.CriterionName,-25} | {result.Explanation}");
}
Console.WriteLine("-------------------------------------------------------------------------------");
Console.WriteLine($"Overall Result: {(report.AllPassed ? "PASSED" : "FAILED")} ({report.PassedCount}/{report.TotalAssertions} Passed - {report.PassRate:F1}%)\n");
