# Status

Last updated: 2026-11-25

## Phase

Vertical slice, step 1 of 4: rules done, nothing playable yet.

## David's setup checklist (one time)

1. ~~Create a private GitHub repo.~~ Done. Still to do: clone it to the
   Windows machine (GitHub Desktop is the easy way).
2. ~~Copy the starter kit into the repo root.~~ Done, except the
   `.claude/agents/` folder did not come through the web upload. Not
   needed for now.
3. Install Unity Hub, then **Unity 6.3 LTS** (6.0 LTS support ended
   October 2026) with the Android Build Support module, including the
   OpenJDK and Android SDK & NDK Tools options.
4. Do **not** create a Unity project by hand. Tell the PM the exact editor
   version the Hub shows; the PM adds the project files to the repo and
   David opens it with "Add project from disk".
5. Enable Developer Options and USB debugging on an Android phone.

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

## Not proven yet

- Nothing has run inside Unity. The data classes are shaped for Unity's
  JSON loader but that load has not been exercised.
- No number here has been felt on a phone. All balance is a first guess.

## Next

1. Unity project files in the repo (needs David's editor version).
2. Unity-facing layer under `Assets/Scripts/Game`: data loading, hero
   controller, follow camera, soft lock-on, enemy AI for the four enemies.
3. Editor script that builds the greybox arena scene.
4. Touch controls and HUD.
5. First Android build to David's phone, then his playtest.

## Waiting on David

- Unity 6.3 LTS installed, and the exact editor version number.
- Which Android phone is the test device.
- Art pipeline choice (GDD section 12). Not blocking the slice.
- Audience and age rating.
- Working title.
