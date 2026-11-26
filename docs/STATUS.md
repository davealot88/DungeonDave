# Status

Last updated: 2026-11-26

## Phase

Vertical slice, step 2 of 5: rules done and the Unity project opens.
Nothing playable yet.

## Setup (done)

- Unity 6.3 LTS (6000.3.29f1) with Android Build Support installed.
- Repo cloned to `C:\Dev\DungeonDave` (kept out of OneDrive on purpose).
- Unity project lives in the repo root; opened from Unity Hub.
- Xbox controller on the PC for playtesting.

## Done

- Foundations agreed (see DECISIONS.md).
- Starter GDD and PM instructions written.
- **Core rules** (`Assets/Scripts/Core`), 59 passing tests, independently
  reviewed and review findings fixed:
  - Damage (crit, armor), 3-hit combo with tap buffering, dodge with
    invulnerability, stamina, charged axe throw, special cooldown, enemy
    poise and stagger.
  - Levels 1 to 5, three talents, talent reset for gold.
  - Iron Sword drop, refine to +3, run rewards (kill XP, clear XP, gold,
    materials).
- **Balance data** (`Assets/Data`): combat, warrior, enemies (Grunt,
  Archer, Brute, and The Warden as the slice boss), progression, gear.
- Rules build and test without Unity: `dotnet test tools/CoreTests`.

- **Unity project** in the repo: URP, landscape, input and test packages.
  The 44 rule tests that live under `Assets/Tests` pass inside Unity
  (the other 15 check the data files and run only with `dotnet`).
- **Editor helpers** (`Assets/Editor`): compile results and test results are
  written to `Logs/claude`, and the PM can ask the open editor to refresh or
  run tests. Menu: DungeonDave > Run Rule Tests.

## Not proven yet

- The data files have not been loaded through Unity's own JSON loader yet.
- No number here has been felt on a phone. All balance is a first guess.

## Next

1. Unity-facing layer under `Assets/Scripts/Game`: data loading, hero
   controller, follow camera, soft lock-on, enemy AI for the four enemies.
2. Editor script that builds the greybox arena scene.
3. Input: touch controls plus gamepad and keyboard (GDD section 4), and
   the HUD.
4. David plays it in the editor or a Windows build with a gamepad.
5. First Android build to a personal Android phone, then the real
   playtest. Combat feel is not signed off until this happens.

## Waiting on David

- A personal Android phone for testing. The Note 20 5G is a company phone
  and is off limits. Not urgent; needed before the first real playtest.
- Already in hand: Apple developer account (from another app), so iOS
  later needs only the cloud Mac build, not a new membership.
- Art pipeline choice (GDD section 12). Not blocking the slice.
- Audience and age rating.
- Working title.
