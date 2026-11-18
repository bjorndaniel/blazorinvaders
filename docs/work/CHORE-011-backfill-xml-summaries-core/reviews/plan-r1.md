VERDICT: APPROVED

# Plan review: CHORE-011 Backfill XML summaries in core, round 1

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | minor | Steps 2 and 4 | Step 2 lets a trailing `//` comment "become its summary", but also says to keep every existing comment, "especially" the ones in `Scene.cs`. The two rules conflict on `Scene.cs:11-16`: the op-code operand layouts are trailing comments on public constants, and they are shared with JS (CG13). The same applies to `Boss.cs:7-9` and `DailyChallenge.cs:8-12`. Moving a trailing comment also rewrites the member's code line. Step 4's check then prints `-`/`+` code lines that someone has to clear by hand, which weakens the AC3 "comments only" proof and goes against CG17 (no reformatting-only changes). | Leave every existing `//` comment where it is. Add the `///` summary above the member, so step 4 prints nothing at all. Otherwise, explicitly exclude `Scene.cs` and the "must match" comments from the conversion. |
| P2 | minor | Step 5 | `docs/coding-guidelines.md` also describes the toolchain outside CG60. The **Tools** table (C# compiler row, Config column) and the line "CI ... fails only on build errors" do not mention that core now generates documentation and treats CS1591 as an error. The `**Last updated:**` line (line 3) still names CHORE-004. | In step 5, also add the core doc setting (`GenerateDocumentationFile`, CS1591 as an error) to the Tools table, and update `Last updated`. |

## Attacks the plan survived
- Warning counts: I copied core to a scratch directory and built it with `GenerateDocumentationFile=true`. It reports exactly 189 CS1591 and 6 CS1573, with the same per-file split as the plan. No other doc warnings (CS1570/1572/1574) appear.
- `<WarningsAsErrors>$(WarningsAsErrors);CS1591</WarningsAsErrors>`: in the scratch build every CS1591 turned into an error and CS1573 stayed a warning, as the plan intends.
- Leaking the XML into the client: I ran `dotnet publish src/BlazorInvaders.csproj -c Release` with doc generation on in core. `BlazorInvaders.Core.xml` is not in `wwwroot/_framework` and not in `service-worker-assets.js`. The offline payload does not change, which bears out the security note.
- Positional records: `DailyChallenge` has a summary but no `<param>` tags, so its generated `Date`/`Modifier` properties raise CS1591. The `<param>`-on-type rule in step 2 covers that. `GameEvent` is missing `Type`/`X`/`Y`, and step 3 covers those.
- CS1573 sites: `Bomb` ctor (`Bomb.cs:14`, missing `start` and `type`) and `GameEngine.Start` (`GameEngine.cs:180`, missing `time`) are as the plan states.
- Other consumers: the client compiles core only through `ProjectReference`, because `src/BlazorInvaders.csproj` removes `core/**` from Compile. Tests reference core, the API does not. No other project inherits the setting, since there is no `Directory.Build.props`.
- Later items breaking the build: this is declared as intended in Risks. The rebase step covers UI-track items that land first.
- Alternative (keep CS1591 a warning and rely on CG70 review): this is weaker enforcement, not simpler. The error is a reasonable, declared reading of the request's AC2 assumption.
- No protected or abstract members in core, so the scope rule "public or protected" adds nothing to miss.

## Checked and fine
- Classification: M (17 files, mechanical, one area); not security-sensitive; not UI-facing.
- Single WP that owns all touched files; plan is 8 KB, within the M budget.
- Security section names TB/T ranges from the security model; no model change needed.
- No architecture change: A7's CI commands stay the same, and building the client still builds core.
- Tests: the build check, the removal check and the unchanged test count are enough for a comment-only change; no new unit tests are needed (CG50).
- Open-question bullet and Revisions section exist in `docs/coding-guidelines.md` (lines 90-95), as step 5 assumes.
