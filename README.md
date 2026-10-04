# AgenticEval Harness

[![Build & Test](https://github.com/kdmbhushan/agentic-eval-harness/actions/workflows/ci.yml/badge.svg)](https://github.com/kdmbhushan/agentic-eval-harness/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-purple.svg)](https://dotnet.microsoft.com/)

> **Spec-driven automated evaluation and regression testing harness for autonomous coding agents.**

This repository hosts the official reference implementation anchoring the technical deep-dive:  
🔗 **[Spec-Driven Agentic Engineering: Continuous Evaluation & Trajectory Verification](https://bhushankadam.dev/blog/spec-driven-agentic-development)** on [bhushankadam.dev](https://bhushankadam.dev).

---

## 🎯 Architecture & Trajectory Verification Flow

Modern AI coding agents generate multi-step decision trajectories comprising file reads, edits, tool invocations, and command executions. Traditional unit tests fail to evaluate intermediate decision paths, token budgets, or tool safety constraints.

`AgenticEval` provides deterministic trajectory assertion rules to evaluate agent reliability:

```mermaid
sequenceDiagram
    autonumber
    actor Developer
    participant Harness as AgenticEval Runner
    participant Trajectory as Trajectory Log Parser
    participant Rules as Assertion Engine
    participant Report as Evaluation Report

    Developer->>Harness: Load Agent Trajectory Log (JSON/Objects)
    Harness->>Trajectory: Ingest step sequences, token usage & tool calls
    Trajectory-->>Harness: Parsed TrajectoryLog
    Harness->>Rules: Evaluate Assertions
    Note over Rules: 1. ProhibitedToolsCheck<br/>2. StepBudgetCheck<br/>3. TokenBudgetCheck<br/>4. SequenceOrderCheck
    Rules-->>Harness: AssertionResults (Pass/Fail + Metrics)
    Harness->>Report: Generate Aggregate Evaluation Report
    Report-->>Developer: Terminal Summary & CI Status
```

---

## 🚀 Key Capabilities

- **Prohibited Tool Protection**: Assert that autonomous agents never execute hazardous actions (e.g. destructive commands, secret leaks, raw DB drops).
- **Trajectory Step & Token Budgets**: Enforce step limits and token consumption budgets to eliminate infinite loops and high API costs.
- **Chronological Sequence Ordering**: Verify that agents follow proper engineering lifecycles (e.g., `specify -> clarify -> plan -> implement -> test`).
- **Zero Cloud Credentials Required**: Complete offline local testbed with mock trajectories and reproducible assertions.

---

## 🛠️ Quickstart

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run Demonstration Harness
```bash
dotnet run --project src/AgenticEval.Runner
```

### Run Automated Tests
```bash
dotnet test
```

---

## 📄 License
This project is licensed under the [MIT License](LICENSE).
