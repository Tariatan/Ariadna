# AGENTS.md

## Editing Rules

- Preserve each file's existing EOF newline style.
- If a file has no trailing newline, do not add one.
- Do not append a blank line at the end of a file unless it already exists intentionally.

## Unit Tests

- Prefer `new Mock<T>()` and `.Object` over `Mock.Of<T>()` and `Mock.Get()`.
- Name mocks `[mockedObjectName]Mock`.
- Name unit test classes `[ClassUnderTest]Tests`.
- Name unit test methods `[MethodUnderTest]_[Precondition]_[ExpectedOutcome]`.
- Structure unit tests as `Arrange - Act - Assert` and include `// Arrange`, `// Act`, and `// Assert` comments.
- Prefer `[TestInitialize]` for complicated test setup. Use the test class constructor only for simple setup.

## Code Reviews

- Check whether the concept is clear in the changes, naming matches the concept, responsibilities are assigned correctly, and any pattern used fits the problem.
- Check whether related behavior is kept together and properly cleaned up, including disposing, unsubscribing, and similar lifecycle handling.
- Check whether expected functionality is missing and whether the result is convenient to use without unnecessary repeated actions.
- Check whether code blocks are easy to read and avoid unnecessary complexity or fancy constructs.
- Prefer existing functionality and established language or framework features over custom reimplementation.
- Check whether logic inside loops is limited to work that must happen inside the loop.
- Treat locally suppressed warnings as a review concern unless there is a strong reason.
- If feedback is not explicitly covered by an agreed guideline or decision, discuss it as feedback or best practice rather than presenting it as a strict rule.
- If the concept appears fundamentally mismatched and would require major rework, recommend a discussion with the author instead of only leaving isolated comments.
