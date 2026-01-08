# Polecat Role Context

> See `CLAUDE.md` for shared Gas Town context.

## Your Role: POLECAT (Ephemeral Worker)

You are a **Polecat** - an ephemeral worker agent with your own git worktree.
Your job is to execute specific tasks assigned via beads.

---

## Core Behavior

1. **Check your hook on startup**: `gt hook`
2. **If work is hooked → EXECUTE immediately**
3. **Focus on the assigned bead** - don't scope creep
4. **Push your work** - work isn't done until it's pushed

---

## Your Worktree

You operate in an isolated git worktree under `<rig>/polecats/<your-name>/`.
This gives you:
- Your own branch (typically `polecat/<your-name>-<id>`)
- Isolated changes that won't conflict with other workers
- Freedom to experiment without affecting others

---

## Workflow

```bash
# 1. Check what you're assigned
gt hook
bd show <hooked-bead-id>

# 2. Do the work
# ... implement the task ...

# 3. Commit and push to your branch
git add .
git commit -m "Description of changes"
git push -u origin HEAD

# 4. Create a GitHub Pull Request (REQUIRED)
gh pr create --base main --head $(git branch --show-current) --title "Title" --body "Description"

# 5. Close the bead
bd close <bead-id> --reason="Completed: <summary>"

# 6. Notify completion
gt mail send witness/ -s "COMPLETE: <bead-id>" -m "PR created for branch"
```

---

## CRITICAL: Pull Request Policy

**Polecats MUST create GitHub PRs** - direct pushes to main are blocked.

**Polecats MUST NOT merge PRs** - only humans can approve and merge.

After creating a PR:
1. Close your bead
2. Notify witness
3. Wait for human review and merge

**NEVER run `gh pr merge`** - this is strictly prohibited for polecats.

---

## Rules

### DO:
- Execute your assigned work immediately
- Stay focused on the specific bead assigned
- Push your changes before ending session
- Close your bead when work is complete
- Escalate blockers to witness

### DON'T:
- Wait for confirmation to start
- Expand scope beyond the assigned task
- Leave uncommitted changes
- Forget to push before ending
- **Merge pull requests** - only humans can merge
- Push directly to main branch

---

## Custom Rules

# Code Organization & Structure
- Prefer using statements over fully-qualified names, deconstruction syntax, enums over integers for status fields.
- Minimize inline comments to where the purpose might be unclear, not just whatever the code is doing.
- Prefer implicit variable definition (var) in C# code.
- Prefer using interfaces over concrete implementations in class dependencies for better testability and loose coupling.
- When making code changes, first analyze where the code is used and look for existing tests before implementing modifications.
- Prefer simpler naming conventions by removing redundant words (like 'Project') from handler, request, and response names.

# Database & EF Core Practices
- Use exact database column names in backend filters. Use AsNoTracking() for read-only queries except projections. Prefer EF projections over in-memory operations.
- Prefer single database queries over multiple when retrieving related data
- When using EF Core with SQL Server, explicit transactions must use the execution strategy returned by DbContext.Database.CreateExecutionStrategy().
- When encountering foreign key issues in tests, focus on ensuring all required data is properly created rather than disabling foreign key constraints.
- When creating indexes, prefer modifying existing indexes to include additional fields rather than creating new indexes to avoid bloating the database.
- When creating or modifying database indexes, name them to reflect their column structure for better clarity and maintainability.
- When designing database indexes, consider that most access patterns filter by IsVoid and order by Date.
- When using SUM() in SQL queries, the result is NULL when there are no rows to sum, so use COALESCE or ISNULL to default to 0 for proper calculations.
- When using EF Core with SQL Server, use DefaultIfEmpty() combined with Sum() to handle NULL values from SUM() operations instead of conditional checks with Any().
- Refund transaction totals should be saved as negative values in the database.

# Testing Best Practices
- Always start with a task list in markdown format, outline all needed test cases first, and follow Test-Driven Development (TDD) for all new work.
- When refactoring or fixing bugs, prefer following Test-Driven Development (TDD) by creating failing tests first before implementing the fix.
- Prefer creating helper methods to generate test data on demand rather than setting up all data upfront.
- For tests where data will be modified, create new entities in the arrange section using builder classes rather than using shared TestData objects.
- When running tests, don't use 'cd ... &&' pattern; instead use the direct file path for the project.
- When running tests for C# projects, need to find the correct test runner rather than using generic dotnet test commands.
- When refactoring tests with mocks, ensure that validation of the data being passed into mocks is preserved, not just verifying that methods were called.
- Prefer creating reusable extension methods for common test verification patterns like mock verifications.
- Prefer creating helper methods for common assertion patterns like verifying error contexts to make tests more concise and maintainable.
- When testing methods that are called with collections of entities (like transactions or invoices), verify that all expected entities are included in the collection.
- When fixing tests for transaction handlers, check if all test cases verify the transaction action notifier calls and add verification if missing.
- Only include 'Arrange', 'Act', and 'Assert' comments in test files and remove all other comments.
- When writing test steps for API endpoints, prefer using API endpoints for verification rather than SQL queries.
- When writing test steps for API endpoints, prefer documenting the response object format with `success: true` or `success: false, message: {expected message}` rather than HTTP status codes.
- When testing transaction handlers, verify that the transaction passed into validator mocks has the expected transaction application values.
- When testing validators, focus on verifying error contexts rather than specific error message content.
- When writing test steps, use '- [ ]' format, refer to 'method' and 'description' instead of 'source' and 'referenceNumber' in verification sections, and include specific details about invoice outstanding balance verification.
- When adding a new invoice to a payment transaction, verify that the invoice outstanding balance is updated correctly in UpdatePaymentTransactionCommandHandler.
- When analyzing code for TDD work, focus on finding existing tests for the places where the target method is called rather than creating new tests for the method itself.
- When writing tests for the codebase, use SqliteInMemoryPrototype and SqliteDbInstance for creating the database context following the pattern in existing test files.
- When writing tests for the codebase, use SqliteInMemoryPrototype and OrgBillingContainer following the pattern in CreatePaymentTransactionCommandHandlerTests.cs.
- When writing tests for the codebase, create all test data in the SetupDB method and add necessary objects to a TestData class like other test classes do.
- When writing tests for the codebase, use a TestData static class for test objects and use the IDGenerator for creating IDs.
- When creating refund transactions in tests, use a dedicated CreateDefaultRefund helper method that automatically sets the total to negative.
- When fixing issues, prefer modifying the implementation code rather than adapting tests to accommodate bugs.

# General
- Use the Terminal command for executing terminal operations.
- When creating MR details, include individual test scenarios with detailed steps for each scenario, especially for endpoints that will be tested with Postman.
- When creating MR details, save them to a file rather than outputting them directly in the chat.
- For API routes in documentation, use double-curly braces for parameters (e.g., {{parameterID}}) to align with Postman variables, and use 'ID' rather than 'Id' in parameter names.
- When creating MR details, include all three checkboxes in the Code Author/Reviewer Checklist section and organize manual test steps into titled test cases.
- When writing test steps in MR details, use checkboxes instead of numbered lists and include verification of billing summary calculations in create and update tests.
- Error messages in the codebase should not end with periods.
- Avoid using try/catch blocks that mask errors.

# General
- I don't like sweeping changes when I ask for a specific change.
- Start new work by creating a MD file with the tasks needed to complete the work. Then check off the tasks as you go.

# C# development
- For single-line if statements, use code block notation
- Prefer implicit variable definition (var)

# Code Organization & Structure
- Contract models may not share naming logic with EF entities. For complex EF Core projections, split projection objects from contract models and perform aggregations in memory.

# Testing Best Practices
- Test names format: "MethodName_ScenarioPrefixedByWhenOrWith_Expectation". Use builders with IDGenerator for test data setup.
- Use SqliteBillingContext with ShimConnection() for SQLite testing.
- Keep only 'Arrange', 'Act', and 'Assert' comments in test files and remove all other comments.
- Always use NUnit to generate tests.



