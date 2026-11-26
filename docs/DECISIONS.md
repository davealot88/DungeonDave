# Decision log

One line per decision. Newest at the bottom.

| Date | Decision | Reason | By |
|---|---|---|---|
| 2026-11-24 | Engine is Unity 6 LTS with URP | Official ad and purchase SDKs; profitability is the priority | David |
| 2026-11-24 | Android first, iOS later via cloud Mac build | No Mac; Android builds and tests from Windows | David |
| 2026-11-24 | Native store app, not a web game | 3D action performs poorly in a browser shell; store monetization | David |
| 2026-11-24 | Free to play with rewarded ads and in-app purchases | Maximize profitability | David |
| 2026-11-24 | Custom art style; greybox until the pipeline is chosen | Owner preference; do not block the slice on art | David |
| 2026-11-24 | Landscape orientation, virtual stick plus four buttons | Standard for third-person action; two-thumb control | PM |
| 2026-11-24 | Soft lock-on, no manual aiming | Aiming with thumbs is frustrating on a phone | PM |
| 2026-11-24 | Special option is swapped between runs, not mid-run | Fewer buttons; makes the loadout a real choice | PM |
| 2026-11-24 | Three talent trees per class, one per attack type | Maps directly onto the three-attack design | PM |
| 2026-11-24 | Refinement never fails or destroys items | Failure feels bad and adds store-policy risk | PM |
| 2026-11-24 | No forced interstitial ads at launch | Protects early retention; rewarded ads earn more per view | PM |
| 2026-11-24 | Game rules in plain C#, numbers in JSON | Lets agents test and retune without the editor | PM |
| 2026-11-24 | First build is a Warrior-only vertical slice | Prove combat feel before building wide | PM |
| 2026-11-25 | Use Unity 6.3 LTS, not 6.0 LTS | 6.0 LTS support ended October 2026; 6.3 is supported to December 2027 | PM |
| 2026-11-25 | Armor formula: armor / (armor + 100); crits start at 5% for 1.5x | Simple, never reaches 100% block, easy to reason about | PM |
| 2026-11-25 | Combo is 1.0x / 1.2x / 1.8x; one full combo kills a Grunt at level 1 | Grunts should feel like fodder; the finisher should feel like the payoff | PM |
| 2026-11-25 | Taps during a swing are buffered | Mashing on a touchscreen must not drop inputs | PM |
| 2026-11-25 | Dodge costs 30 of 100 stamina, 0.25 s invulnerable in a 0.35 s dash | Three dodges in a row, then you have to commit | PM |
| 2026-11-25 | Enemies stagger through a poise meter that refills after 3 s | Third hit staggers small enemies; a Brute needs a special or two quick combos | PM |
| 2026-11-25 | Specials: Ground Slam 3x in 4.5 m, Executioner 7x single target, both 12 s | Same cooldown so the choice is about the fight, not uptime | PM |
| 2026-11-25 | Cooldown reduction is capped at 50% | Stops a talent-plus-gear build from spamming specials | PM |
| 2026-11-25 | Slice talents: Heavy Hands, Keen Edge, Battle Rhythm, 3 ranks each, 4 points by level 5 | One per tree; forces a choice even in the slice | PM |
| 2026-11-25 | Refine is +8% item stats per step, cost rises each step | Steady, visible gains; no failure | PM |
| 2026-11-25 | Slice boss is The Warden, a large Brute-style enemy | GDD calls for a boss; reuses Brute behaviour to keep the slice small | PM |
| 2026-11-25 | Clearing a dungeon pays bonus XP on top of kill XP; a failed run keeps kill XP only | GDD section 7; failure should still feel like progress | PM |
| 2026-11-25 | Rules also build and test outside Unity (tools/) | Lets the PM verify changes without the editor | PM |
| 2026-11-25 | PM wrote the first Core pass directly; a separate reviewer agent checked it | Agent definitions were missing from the repo; review step was kept | PM |
| 2026-11-26 | Stay on Unity 6.3 LTS (6000.3.29f1), not the 6.6 update release | Fixes through December 2027 with no forced version hops; nothing in 6.6 is needed | PM, David agreed |
| 2026-11-26 | Phone stays the target platform; Steam is a possible later paid edition, not the lead | Controls, performance and the ad-based model are all built around phones | David |
| 2026-11-26 | Add gamepad and keyboard support alongside touch | Lets David play in the editor and Windows builds; keeps a PC edition cheap | David |
| 2026-11-26 | Bindings: X melee, right trigger ranged, Y special, A dodge; mouse buttons, Q and Space on keyboard | Mirrors the four-button touch arc one to one | PM |
| 2026-11-26 | The Galaxy Note 20 5G is not the test device | It is a company phone; no developer mode on employer hardware | David |
| 2026-11-26 | Early playtesting happens in the Unity editor and Windows builds until a personal Android is available | Does not block the slice; a real phone is still required before combat feel is signed off | PM |
| 2026-11-26 | Unity project settings copied from a fresh Universal 3D template made by the same editor version | Guarantees settings and package versions match 6000.3.29f1 instead of hand-writing them | PM |
| 2026-11-26 | Project locked to landscape; placeholder app ID com.dungeondave.game | GDD orientation; the real ID waits for the working title and must be set before any store upload | PM |
| 2026-11-26 | Editor scripts write compile and test results to Logs/claude and accept simple requests from a file there | The PM cannot launch Unity; this lets it see errors and run tests without David pasting output | PM |
| 2026-11-26 | David's PC folder (C:\Dev\DungeonDave) is the working copy; David commits and pushes with GitHub Desktop | Unity generates files locally, so one working copy avoids two diverging ones | PM |
