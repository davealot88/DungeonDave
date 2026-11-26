# Game Design Document

Working title: TBD (owner decision)
Last updated: 2026-11-26

## 1. Summary

A third-person, over-the-shoulder medieval dungeon slasher for phones. Pick
a Warrior, Ranger, or Wizard, clear short dungeons, level up, spend talent
points, and chase gear that you refine into stronger versions.

## 2. Pillars

1. **Feels good under two thumbs.** Hits land hard, dodges are readable,
   and nothing needs more than two thumbs.
2. **A run fits in a break.** A dungeon takes 3 to 5 minutes.
3. **Always one upgrade away.** Every run ends with XP, loot, or refinement
   progress the player can spend immediately.

## 3. Foundations

| Area | Decision |
|---|---|
| Engine | Unity 6.3 LTS (6000.3), URP, C# |
| Platforms | Phone is the target: Android first, iOS after via cloud Mac build. A paid PC (Steam) edition is a possible later release, not a lead platform |
| Orientation | Landscape |
| Business model | Free to play, rewarded ads plus in-app purchases |
| Art | Custom style, pipeline TBD; greybox until decided |
| Connectivity | Playable offline; saves sync when online (later) |

## 4. Camera and controls

- Camera sits behind and above the right shoulder and follows the hero.
  It auto-rotates toward the current target; the player can drag the right
  half of the screen to look.
- Left thumb: virtual stick to move.
- Right thumb, four buttons in an arc:
  - **Melee**: tap to attack; repeated taps chain a 3-hit combo.
  - **Ranged**: tap to fire at the current target; hold to charge.
  - **Special**: one big ability on a cooldown.
  - **Dodge**: short dash with brief invulnerability, limited by stamina.
- Soft lock-on: attacks aim at the nearest enemy in front of the hero. No
  manual aiming is required.

### Gamepad and keyboard

The game is designed for touch. Gamepad and keyboard are also supported so
it can be played in the editor and in Windows builds during development,
and so a PC edition stays cheap later. All three feed the same five
actions; nothing may need more than the touch layout offers.

| Action | Touch | Gamepad | Keyboard and mouse |
|---|---|---|---|
| Move | Left virtual stick | Left stick | W A S D |
| Look | Drag right half of screen | Right stick | Mouse |
| Melee | Melee button | X / Square | Left mouse button |
| Ranged (hold to charge) | Ranged button | Right trigger | Right mouse button |
| Special | Special button | Y / Triangle | Q |
| Dodge | Dodge button | A / Cross | Space |

## 5. Classes

Every class has all three attack types, but each leans on one.

| Class | Leans on | Melee | Ranged | Special: AOE option | Special: single-target option |
|---|---|---|---|---|---|
| Warrior | Melee | Sword combo, third hit staggers | Thrown axe, slow and heavy | Ground Slam: shockwave around the hero | Executioner: one huge overhead strike |
| Ranger | Ranged | Twin daggers, fast, low damage | Bow, charge for a piercing shot | Arrow Rain: marked area takes hits over time | Deadeye: charged shot with a big crit bonus |
| Wizard | Special | Staff swipe that knocks enemies back | Arcane bolt, homing | Meteor: delayed blast in a wide circle | Lightning Lance: instant high-damage beam |

The special slot holds one option at a time. The player swaps between the
AOE and single-target option on the loadout screen between dungeons, not
mid-run. Later, each class gets more options for each side.

## 6. Core loop

1. Pick a dungeon and difficulty from the map.
2. Fight through 5 to 7 rooms and a boss.
3. Earn XP, gold, gear, and refinement materials.
4. Back at camp: level up, spend talent points, equip and refine gear.
5. Harder dungeons and difficulties unlock by power and by clears.

## 7. Progression

### Levels and talents

- Level cap 50 at launch. XP comes from kills and from clearing dungeons.
- One talent point per level from level 2.
- Each class has three talent trees matching its three attacks: Melee,
  Ranged, Special. Each tree has 8 nodes. A full build cannot max all
  three, so players specialise. Talents can be reset for gold.
- Each hero levels separately.

### Gear

- Five slots: Weapon, Armor, Helm, Boots, Trinket.
- Five rarities: Common, Uncommon, Rare, Epic, Legendary. Higher rarity
  means higher base stats and more bonus stat lines (0 to 4).
- Weapons are class-specific. Other slots are shared across classes.
- Core stats: Power, Health, Armor, Crit Chance, Crit Damage, Cooldown
  Reduction.

### Refinement

Two ways to make a piece of gear stronger:

- **Refine**: spend gold and materials to raise an item from +0 to +10.
  Each step raises its base stats. Refinement never fails and never
  destroys an item.
- **Ascend**: combine a +10 item with duplicates or essence of the same
  rarity to raise it one rarity tier, resetting it to +0 with higher base
  stats and one more bonus line.

Materials drop in dungeons, with higher tiers from harder difficulties.
Unwanted gear salvages into materials.

## 8. Dungeons and enemies

- Dungeons are built from hand-made room modules chained in a random order
  with a fixed boss room at the end.
- Rooms lock until cleared. Some rooms have a modifier (elite enemy, trap
  floor, timed wave).
- Enemy roles: Grunt (melee, in numbers), Archer (ranged, fragile), Brute
  (slow, armored, telegraphed heavy hits), Caster (area denial), Boss.
- Three difficulties per dungeon. Each raises enemy stats and the loot
  tier.

## 9. Monetization

Designed in from the start, kept fair enough that free players stay.

- **Rewarded ads (opt-in only)**: revive once per run, double a run's
  loot, a daily free refinement material chest. No forced interstitials at
  launch.
- **In-app purchases**: starter pack, gold and material packs, a remove-ads
  purchase that grants the ad rewards automatically, cosmetic skins.
- **Later**: seasonal battle pass, limited-time dungeons.
- Not decided, owner call: paid random rewards of any kind, an energy
  system that limits runs, prices.

## 10. Vertical slice (the first thing to build)

Goal: prove that combat is fun on a phone before building anything wide.

In scope:

- Warrior only.
- One arena-style room, then a three-room dungeon with a boss.
- Three enemies: Grunt, Archer, Brute. The boss is The Warden, a large
  Brute-style enemy.
- Melee combo, thrown axe, both special options, dodge.
- XP and levels 1 to 5 with three working talents.
- One weapon that drops, can be equipped, and refined to +3.
- HUD: health, stamina, cooldowns, touch controls.
- Gamepad and keyboard input alongside touch (section 4).
- Greybox art: capsules and simple shapes with clear hit feedback (hit
  pause, flash, knockback, camera shake, damage numbers).
- Runs on a mid-range Android phone at 60 fps.

Out of scope: Ranger and Wizard, full talent trees, ascension, ads and
purchases, saving to the cloud, real art, menus beyond the minimum.

The slice is done when David has played it on his phone and says the
combat is worth building on.

## 11. Technical approach

- Game rules in plain C# under Assets/Scripts/Core, with no UnityEngine
  dependency, covered by edit-mode tests.
- Unity-facing code under Assets/Scripts/Game.
- All balance numbers in JSON under Assets/Data.
- Scenes and prefabs generated by Editor scripts where practical.
- Object pooling for enemies, projectiles, and damage numbers.

## 12. Open questions for David

1. **Art pipeline.** Options: (a) AI-assisted 3D model tools plus a shared
   animation library, done by David; (b) a freelance 3D artist; (c)
   stylized packs reskinned to the custom look. Needed before the slice
   gets real art, not before it gets built.
2. **Audience and age rating.** This affects which ads can be shown, what
   can be sold, and store requirements.
3. **Working title.**
4. **Paid random rewards and an energy system.** See section 9.
