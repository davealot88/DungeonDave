using System;
using System.IO;
using System.Text.Json;
using DungeonDave.Core;
using NUnit.Framework;

namespace DungeonDave.Core.Tests
{
    /// <summary>
    /// Loads the real balance files from Assets/Data and checks they are complete and that the
    /// slice plays the way the GDD says. These run only here (they need System.Text.Json), not in Unity.
    /// </summary>
    public class DataFileTests
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
        };

        private static T Load<T>(string file)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Data", file);
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options);
        }

        private static HeroDefinition Warrior() { return Load<HeroDefinition>("warrior.json"); }
        private static EnemyFile Enemies() { return Load<EnemyFile>("enemies.json"); }
        private static ProgressionDefinition Progression() { return Load<ProgressionDefinition>("progression.json"); }
        private static CombatDefinition Combat() { return Load<CombatDefinition>("combat.json"); }
        private static GearDefinition Gear() { return Load<GearDefinition>("gear.json"); }

        [Test]
        public void WarriorHasEverythingTheSliceNeeds()
        {
            var warrior = Warrior();
            Assert.AreEqual(3, warrior.combo.steps.Length, "3-hit combo");
            Assert.IsTrue(warrior.FindSpecial("ground_slam").IsAreaOfEffect);
            Assert.IsFalse(warrior.FindSpecial("executioner").IsAreaOfEffect);
            Assert.Greater(warrior.ranged.maxMultiplier, warrior.ranged.minMultiplier);
            Assert.Less(warrior.dodge.invulnerableTime, warrior.dodge.duration);
            Assert.GreaterOrEqual(warrior.stamina.max / warrior.dodge.staminaCost, 3f, "three dodges from a full bar");
        }

        [Test]
        public void CombatRulesAreSane()
        {
            var combat = Combat();
            Assert.Greater(combat.armorConstant, 0f);
            Assert.That(combat.maxCooldownReduction, Is.InRange(0f, 0.9f));
        }

        [Test]
        public void OnlyTheThirdHitStaggersGruntsAndArchers()
        {
            var warrior = Warrior();
            var weak = new StatBlock { power = 1f, critDamage = 1f };
            foreach (var id in new[] { "grunt", "archer" })
            {
                var enemy = Combatant.FromEnemy(Enemies().Find(id));
                enemy.Health = new Health(100000);
                var steps = warrior.combo.steps;
                Assert.IsFalse(HitResolver.Resolve(weak, 1f, steps[0].poiseDamage, enemy, Combat(), new FixedRandom(0.99f)).staggered, id);
                Assert.IsFalse(HitResolver.Resolve(weak, 1f, steps[1].poiseDamage, enemy, Combat(), new FixedRandom(0.99f)).staggered, id);
                Assert.IsTrue(HitResolver.Resolve(weak, 1f, steps[2].poiseDamage, enemy, Combat(), new FixedRandom(0.99f)).staggered, id);
            }
        }

        [Test]
        public void BossIsAFightButBeatable()
        {
            var warrior = Warrior();
            var boss = Enemies().Find("warden");
            Assert.IsNotNull(boss);
            Assert.AreEqual("boss", boss.role);
            var stats = HeroBuild.ComputeStats(warrior, 3, null, null);
            float comboDamage = 0f;
            foreach (var step in warrior.combo.steps)
                comboDamage += stats.power * step.damageMultiplier * (1f - DamageCalc.Mitigation(boss.armor, Combat().armorConstant));
            Assert.That(boss.maxHealth / comboDamage, Is.InRange(8f, 20f), "full combos to kill the boss at level 3");
        }

        [Test]
        public void ClearingPaysBonusXp()
        {
            Assert.Greater(Gear().loot.clearXp, 0);
        }

        [Test]
        public void SliceHasGruntArcherAndBrute()
        {
            var enemies = Enemies();
            foreach (var id in new[] { "grunt", "archer", "brute" })
            {
                var enemy = enemies.Find(id);
                Assert.IsNotNull(enemy, id);
                Assert.Greater(enemy.maxHealth, 0f, id);
                Assert.Greater(enemy.xp, 0, id);
                Assert.Greater(enemy.attackWindup, 0f, id + " needs a readable windup");
            }
        }

        [Test]
        public void EveryTalentHasAKnownEffect()
        {
            var progression = Progression();
            Assert.AreEqual(3, progression.talents.Length);
            foreach (var talent in progression.talents)
            {
                Assert.AreNotEqual(TalentEffect.None, TalentEffects.Parse(talent.effect), talent.id);
            }
        }

        [Test]
        public void FullBuildCannotMaxEveryTalent()
        {
            var progression = Progression();
            int ranks = 0;
            foreach (var talent in progression.talents) ranks += talent.maxRank;
            Assert.Greater(ranks, TalentState.EarnedPoints(progression.levelCap), "players must specialise");
        }

        [Test]
        public void LevelOneWarriorKillsAGruntWithOneFullCombo()
        {
            var warrior = Warrior();
            var stats = HeroBuild.ComputeStats(warrior, 1, null, null);
            var grunt = Combatant.FromEnemy(Enemies().Find("grunt"));
            foreach (var step in warrior.combo.steps)
                HitResolver.Resolve(stats, step.damageMultiplier, step.poiseDamage, grunt, Combat(), new FixedRandom(0.99f));
            Assert.IsTrue(grunt.Health.IsDead);
        }

        [Test]
        public void FinisherStaggersAGruntButOneComboDoesNotStaggerABrute()
        {
            var warrior = Warrior();
            var enemies = Enemies();
            var weak = new StatBlock { power = 1f, critDamage = 1f };

            var grunt = Combatant.FromEnemy(enemies.Find("grunt"));
            var finisher = warrior.combo.steps[2];
            Assert.IsTrue(HitResolver.Resolve(weak, 1f, finisher.poiseDamage, grunt, Combat(), new FixedRandom(0.99f)).staggered);

            var brute = Combatant.FromEnemy(enemies.Find("brute"));
            foreach (var step in warrior.combo.steps)
                Assert.IsFalse(HitResolver.Resolve(weak, 1f, step.poiseDamage, brute, Combat(), new FixedRandom(0.99f)).staggered);
        }

        [Test]
        public void EitherSpecialStaggersAFreshBrute()
        {
            var warrior = Warrior();
            var weak = new StatBlock { power = 1f, critDamage = 1f };
            foreach (var special in warrior.specials)
            {
                var brute = Combatant.FromEnemy(Enemies().Find("brute"));
                Assert.IsTrue(HitResolver.Resolve(weak, 1f, special.poiseDamage, brute, Combat(), new FixedRandom(0.99f)).staggered, special.id);
            }
        }

        [Test]
        public void BruteSurvivesOneExecutionerAtLevelOne()
        {
            var warrior = Warrior();
            var stats = HeroBuild.ComputeStats(warrior, 1, null, null);
            var brute = Combatant.FromEnemy(Enemies().Find("brute"));
            var special = warrior.FindSpecial("executioner");
            HitResolver.Resolve(stats, special.damageMultiplier, special.poiseDamage, brute, Combat(), new FixedRandom(0.99f));
            Assert.IsFalse(brute.Health.IsDead, "the brute should be a fight, not a one-shot");
        }

        [Test]
        public void WarriorTakesSeveralBruteHitsToDie()
        {
            var warrior = Warrior();
            var stats = HeroBuild.ComputeStats(warrior, 1, null, null);
            var hero = new Combatant { Stats = stats, Health = new Health((int)stats.maxHealth) };
            var bruteStats = Combatant.FromEnemy(Enemies().Find("brute")).Stats;
            int hits = 0;
            while (!hero.Health.IsDead && hits < 100)
            {
                HitResolver.Resolve(bruteStats, 1f, 0f, hero, Combat(), new FixedRandom(0.99f));
                hits++;
            }
            Assert.That(hits, Is.InRange(5, 12));
        }

        [Test]
        public void LevelCapIsReachableInAHandfulOfRuns()
        {
            var progression = Progression();
            int total = 0;
            for (int level = 1; level < progression.levelCap; level++) total += LevelCurve.XpToNext(progression, level);
            var enemies = Enemies();
            // Rough slice dungeon: three rooms of mixed enemies.
            int runXp = 12 * enemies.Find("grunt").xp + 5 * enemies.Find("archer").xp + 2 * enemies.Find("brute").xp;
            Assert.That((float)total / runXp, Is.InRange(2f, 8f));
        }

        [Test]
        public void DroppedWeaponExistsAndRefinesToPlusThree()
        {
            var gear = Gear();
            Assert.AreEqual(3, gear.maxRefineLevel);
            var item = gear.FindItem(gear.loot.weaponDropId);
            Assert.IsNotNull(item);
            Assert.AreEqual("weapon", item.slot);
            Assert.AreEqual("warrior", item.classId);

            var sword = new ItemInstance { definitionId = item.id };
            var wallet = new Wallet(1000000, 1000);
            while (Refinement.TryRefine(sword, gear, wallet)) { }
            Assert.AreEqual(3, sword.refineLevel);
            Assert.Greater(Refinement.Stats(sword, gear).power, item.stats.power);
        }
    }
}
