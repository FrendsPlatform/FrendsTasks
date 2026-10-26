// Replace <TASK_NAME> with actual task full name eg. Frends.GraphQl.ExecuteQuery
// Replace <BRANCH_NAME> witch actual branch name eg. main
// Replace <FINDINGS> with Findings sections from taskmanager default input
*Note: Lines starting with "//" in this file are for human, don't read it

# Refactoring Task

> 🛑 **MANDATORY PRE-REQUISITE & AUTONOMY DIRECTIVE:**
> - Execute the terminal command in STEP 1 as your VERY FIRST action. Do NOT inspect or edit files before this.
> - **AUTONOMOUS MODE:** Do NOT stop to ask for reconfirmation or confirmation. Fix both the findings AND any build warnings automatically in one continuous pass.

## Context
- **Target Folder:** `<TASK_NAME>/`
- **Template Repo:** `https://github.com/FrendsPlatform/FrendsTasks.git`
- **Template Branch:** `<BRANCH_NAME>`
- **Standards Document:** `.tmp/FrendsTasks/harmonization_rules.md`
- **EditorConfig File:** `.tmp/FrendsTasks/.editorconfig`

---

## STEP 1: Mandatory Environment Setup [REQUIRED] (DO THIS FIRST)

Execute the following shell command immediately in your terminal before doing anything else:

```bash
git clone --depth 1 --branch <BRANCH_NAME> --single-branch https://github.com/FrendsPlatform/FrendsTasks.git .tmp/FrendsTasks
```

*Note: If `.tmp/FrendsTasks` already exists, delete it first and run the command above to ensure you have fresh templates.*

---

## GATE 1 REQUIREMENT [REQUIRED]
- You must respect this gate before going any further
- In your first response, you must write a brief, 1-sentencte per rule summary of all the rules from `.tmp/FrendsTasks/harmonization_rules.md` BEFORE making any code edits.
- Note which rules are **REQUIRED** and which are **OPTIONAL**. This summary is mandatory to proceed to Step 2.
---

## STEP 2: Findings to Fix [REQUIRED]

<FINDINGS>

---

## STEP 3: Execution Rules & Mandatory Build Cleanup [REQUIRED]

1. **Read Rules:** Read and strictly follow `.tmp/FrendsTasks/harmonization_rules.md` (cloned in Step 1).
2. **Apply Fixes:** Resolve the findings listed in Step 2. Focus on `<TASK_NAME>/`.
3. **Mandatory Build & Warning Resolution:** Run the build command below:
   ```bash
   dotnet build /p:EnforceCodeStyleInBuild=true /p:EditorConfigFile=.tmp/FrendsTasks/.editorconfig <TASK_NAME>/
   ```
  - Resolving ALL build warnings surfaced by `.editorconfig` IS MANDATORY and WITHIN SCOPE. You are explicitly authorized and REQUIRED to fix all build warnings without asking for confirmation.
4. **Fix Warnings Loop:** Repeat code edits and build checks until the output shows **0 Warnings and 0 Errors**.
5. **Review:** Review your changes against the rules in `.tmp/FrendsTasks/harmonization_rules.md` to ensure compliance, and fix if not compliant.
   - **CRITICAL:** If rule is labeled as **[REQUIRED]**, it MUST BE fully implemented.
   - Repeat this step until all changes are compliant with the rules.
   - **CRITICAL:** You can't skip this step; it is mandatory to ensure all changes are compliant with the rules, especially required one.
6. **Final Cleanup:** Remove the `.tmp/` directory as your absolute last action before concluding.
