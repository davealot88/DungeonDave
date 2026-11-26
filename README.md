# DungeonDave

Third-person medieval dungeon slasher for phones. Design is in `docs/GDD.md`.

## Code layout

| Path | What it is |
|---|---|
| `Assets/Scripts/Core` | Game rules in plain C#, no Unity dependency |
| `Assets/Scripts/Game` | Unity-facing code (not started) |
| `Assets/Data` | All balance numbers, as JSON |
| `Assets/Tests/EditMode` | Rule tests; run in Unity's Test Runner or with `dotnet` |
| `tools/` | Projects that build and test the rules without opening Unity |

To run the tests without Unity (needs the .NET 8 SDK): `dotnet test tools/CoreTests`.
It also loads the real files in `Assets/Data` and checks the slice's balance.

# Starter kit

## What is here

- `CLAUDE.md`: standing orders for the project manager session.
- `.claude/agents/`: the six specialist agents and the model each uses.
- `docs/GDD.md`: the game design.
- `docs/DECISIONS.md`: the decision log.
- `docs/STATUS.md`: where things stand, plus the one-time setup checklist.

## Prompts

Start the project manager session on Fable or Opus, in the repo folder.

Kickoff, first session only:

```
Read CLAUDE.md, docs/GDD.md, docs/DECISIONS.md and docs/STATUS.md. Confirm
the Unity project is set up, then start the vertical slice in the order
listed under Next in STATUS.md. Bring me only decisions that meet the
escalation rule, one at a time, with your recommendation.
```

Every session after that:

```
Continue from STATUS.md.
```

When you want to know what happened:

```
Synopsis please.
```

After you playtest:

```
Playtest notes: <what felt good, what felt bad, what broke>
```

To overrule something:

```
Change decision: <what you want instead>. Log it and update the GDD.
```
