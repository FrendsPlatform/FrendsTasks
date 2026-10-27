# Frends Task Harmonization — Agent Instructions

- You are updating an existing Frends Task (C#/.NET) to bring it in line with current standards.
- For #FrendsTaskTemplate references you should **always** check code in `.tmp/FrendsTasks/FrendsTaskTemplate/Frends.Template/Party.Echo.Execute`
- Follow every section below.
- Plan your work and execute it. 
- Work through the checklist at the end before finishing.

## 1. [REQUIRED] Versioning

- The `.csproj` version has three numbers: `MAJOR.MINOR.PATCH`.
- **Never change PATCH.** It stays fixed.
- Breaking change → bump **MAJOR**.
- If you have changed Task method signature by renaming or deleting any of the existing properties, then **IT IS** a Breaking Change
- If you have changed Task method signature by adding new property, and it's default value do not change existing code behavior / logic, then **IT IS NOT** a Breaking Change
- Any other change (fix, improvement, new optional parameter, etc.) → bump **MINOR**.
- If you are unsure whether a change is breaking, treat it as breaking and bump MAJOR.

## 2. [REQUIRED] CHANGELOG.md

- Add one new entry per change, at the top of the file, matching the version you just set.
- Write it for the **Task's end user**, not for a developer reading a diff.
- In a list of changes, if change described in a row is breaking it should start with "- **[Breaking Change]** " exactly; otherwise just "- "
- Examples: 
  - ✅ Good example: "- Fixed an issue where the Task would fail silently when the input list was empty."
  - ✅ Good example: "- **[Breaking Change]** Renamed `InputFilePath` property to `SourceFilePath` to better reflect its purpose."
  - ❌ Bad example: "Refactored null-check branch in `Execute()` to short-circuit earlier."
  - ❌ Bad example: "- **BREAKING:** Renamed `InputFilePath` property to `SourceFilePath` to better reflect its purpose."

## 3. [OPTIONAL] Error Handling — Required Pattern

- Whenever you touch error-handling code, it must match ErrorHandler.cs code from #FrendsTaskTemplate.
- Do not reimplement it, just copy/paste it and adapt only the namespace and class name to match this Task.

### 3a. You MUST actually call the Error Handler — do not reimplement it

This is the single most common mistake: adding the `Options.ThrowErrorOnFailure` / `ErrorMessageOnFailure` properties, but then writing your own inline try/catch logic in `Execute()` instead of routing through `ErrorHandler.Handle`.

**Wrong** — duplicates the handler's logic instead of using it:
```csharp
catch (Exception ex)
{
    if (options.ThrowErrorOnFailure)
        throw new Exception(options.ErrorMessageOnFailure ?? ex.Message, ex);
    return new Result { Success = false, Error = new Error { Message = ex.Message } };
}
```

**Correct** — every catch block in `Execute()` delegates to the shared handler:
```csharp
catch (Exception ex)
{
    return ex.Handle(options);
}
```

Wire every catch block in code through `exception.Handle(options)`. Do not leave any catch block with hand-rolled error logic once `ErrorHandler` exists.

## 4. [OPTIONAL] Parameter Validation — Required Pattern

- Whenever you touch validation-handling code, it must match ValidationHandler.cs code from #FrendsTaskTemplate.
- Do not reimplement it, just copy/paste it and adapt only the namespace and class name to match this Task.
- Validation should be handled by adding ValidationAttributes to class property You should use default .net attributes e.g. RequiredAttribue() RangeAttribute() etc. Alternatively you can create custom one if default ones are not sufficient, e.g.: see RequiredIfAttribute() in #FrendsTaskTemplate.

## 5. [REQUIRED] `Options` Class — Minimum Shape

- If the Task is missing an `Options` parameter, add one based on Options.cs from #FrendsTaskTemplate code, with **at least** these properties. 
- Treat `Delimiter` as an example of a task-specific setting, not a mandatory field — keep it only if it's relevant to this Task
- `ThrowErrorOnFailure` and `ErrorMessageOnFailure` are **always** required.

## 6. [OPTIONAL] `Result` Class — One Constructor Only

- When adding error handling to a Task, you will typically need `Result` to be able to represent both success and failure. **Do not add a second constructor next to an existing parameterless/success one** — this leaves two ways to build the same object and Copilot-generated code tends to pick the wrong one inconsistently.
- Instead, converge on a **single constructor** where `Error` defaults to `null` on success. Use Result.cs from #FrendsTaskTemplate as a starting point
- If the Task already has an existing `Result` constructor in use elsewhere, **remove the redundant one you were about to add** and update call sites to the single-constructor form instead of leaving both.

## 7. [REQUIRED] Error Handler Tests

- You should respect this rule only if you've added or modified `ErrorHandler`
- Whenever `ErrorHandler` is added or modified, also add a matching test class.
- Use #FrendsTaskTemplate ErrorHandlerTest.cs and TestBase.cs files as the template, adapting the namespace, class-under-test name, and the default helper methods to match this Task's actual test scaffolding.
- **Before finishing, resolve the `TODO`**: replace it with an actual scenario that makes `DefaultInput()`/`DefaultConnection()` invalid for this specific Task (e.g., an empty required field, an unreachable endpoint, a malformed value) — don't leave the placeholder TODO comment in committed code.

## 8. [REQUIRED] XML Doc `<example>` Tags — Use Real Values, Never Null/Empty Placeholders

- Every public property's `<example>` tag must show a **realistic, meaningful value** a user would actually enter — not `null`, not `""`, not `0`, unless that literally is the only sensible example (e.g., a boolean flag, or `Error` which is genuinely `null` on success).
- Examples must be logically coherent with task functionality. E.g. "Unable to join strings" is not a good example for error message in task for reading files.
- Examples:
  - ❌ `/// <example></example>`
  - ❌ `/// <example>string.Empty</example>`
  - ✅ `/// <example>https://api.example.com/v1/orders</example>`
  - ✅ `/// <example>Order validation failed: missing customer ID</example>`
  - ✅ `/// <example>3</example>` (for a retry-count style field)
- If you're unsure what a realistic value looks like, infer it from the property's name, type, and how it's used in `Execute()` — don't default to an empty placeholder just because it's easy.
- In case of main task method xml doc <returns> tag should follow pattern from #FrendsTaskTemplate Party.Echo.Execute.cs

## 9. [REQUIRED] Project File Requirements

- Every `.csproj` file must include PackageReference for StyleCop.Analyzers as in #FrendsTaskTemplate
- Task `.csproj` must include PackageReference for FrendsTaskAnalyzers as in #FrendsTaskTemplate
- Check #FrendsTaskTemplate for actual versions of those packages we are using nowadays
- Confirm that both .csproj files (Task and Unit Test) are using the same versions of StyleCop.Analyzers and FrendsTaskAnalyzers packages as in #FrendsTaskTemplate. If not, update them to match.

## 10. [OPTIONAL] Framework Upgrades

- If you upgrade the main Task project to a new target framework, **you must also upgrade the unit test project** to match. Do not leave them on different target frameworks.

## 11. [OPTIONAL] Reference Template

- Structure and conventions (folder layout, naming, test project setup) should follow the official template #FrendsTaskTemplate

## 12. [REQUIRED] TODO's
- Make sure you didn't introduce any new TODOs commentes or sections into codebase.

## 13. [REQUIRED] Suppressions

- Make sure solution has 2 GlobalSuppressions.cs files (one per project). They should match EXACTLY GlobalSuppressions.cs files from #FrendsTaskTemplate
- This step is mandatory before you go to step 13.

## 14. [REQUIRED] Build Warnings

- Your solutions should be buildable.
- Before you check build warnings, make sure you have added correct GlobalSuppressions.cs files to both projects.
- If any build warnings occurs, you should resolve them.
- DO NOT any additional suppressions after you've added GlobalSuppressions files.
- DO NOT add inline #pragma or class-level warning suppressions
- DO NOT add any additional suppressions to the .csproj file

# Before You Finish — Checklist

- [ ] Version bumped correctly (MAJOR or MINOR only — PATCH untouched)
- [ ] CHANGELOG.md entry added, written for the Task's end user, not a dev changelog
- [ ] `ErrorHandler` matches the required pattern exactly (namespace adapted)
- [ ] Every catch block in `Execute()` calls `exception.Handle(options)` — no hand-rolled error logic left behind
- [ ] `ValidationHandler` matches the required pattern exactly (namespace adapted)
- [ ] `Options` class has `ThrowErrorOnFailure` and `ErrorMessageOnFailure` at minimum
- [ ] `Result` has exactly one constructor (error defaults to `null` on success) — no duplicate/redundant constructor left in place
- [ ] `ErrorHandlerTest` class added/updated, and its `TODO` placeholder replaced with a real invalid-input scenario
- [ ] Every `<example>` tag shows a realistic value, not a null/empty placeholder (unless null/empty is genuinely the only sensible example)
- [ ] `.csproj` has `StyleCop.Analyzers` and `FrendsTaskAnalyzers` package references with correct versions
- [ ] If framework was upgraded: unit test project upgraded too
- [ ] Structure matches #FrendsTaskTemplate conventions
- [ ] Any code that became obsolete, unused or otherwise unnecessary is removed
- [ ] GlobalSuppressions files exist and match #FrendsTaskTemplate
- [ ] Build succeed with 0 warning
- [ ] Codebase doesn't have any new TODO comments

# Acceptance criteria

- All checklist points are covered.
- The solution builds and existing tests pass.
