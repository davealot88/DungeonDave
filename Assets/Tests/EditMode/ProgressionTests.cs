using DungeonDave.Core;
using NUnit.Framework;

namespace DungeonDave.Core.Tests
{
    public class LevelTests
    {
        [Test]
        public void XpCarriesOverIntoTheNextLevel()
        {
            var def = TestData.Progression();
            var hero = new HeroProgress();
            Assert.AreEqual(1, hero.AddXp(250, def));
            Assert.AreEqual(2, hero.Level);
            Assert.AreEqual(150, hero.Xp);
        }

        [Test]
        public void BigXpGainStopsAtTheCap()
        {
            var def = TestData.Progression();
            var hero = new HeroProgress();
            Assert.AreEqual(4, hero.AddXp(100000, def));
            Assert.AreEqual(5, hero.Level);
            Assert.AreEqual(0, hero.Xp);
            Assert.AreEqual(0, hero.AddXp(500, def));
            Assert.AreEqual(0, LevelCurve.XpToNext(def, 5));
        }
    }

    public class SaveLoadTests
    {
        [Test]
        public void NormalizeClampsLevelAndRollsOverXp()
        {
            var def = TestData.Progression();
            var tooHigh = new HeroProgress(9, 5000);
            tooHigh.Normalize(def);
            Assert.AreEqual(5, tooHigh.Level);
            Assert.AreEqual(0, tooHigh.Xp);

            var unspent = new HeroProgress(1, 350);
            unspent.Normalize(def);
            Assert.AreEqual(3, unspent.Level);
            Assert.AreEqual(50, unspent.Xp);
        }

        [Test]
        public void LoadedTalentsAreCheckedAgainstTheLevel()
        {
            var talents = new TalentState(TestData.Progression());
            talents.SetRank("melee", 3);
            talents.SetRank("ranged", 9);
            Assert.AreEqual(3, talents.GetRank("ranged"), "clamped to max rank");
            Assert.AreEqual(6, talents.SpentPoints);
            Assert.IsFalse(talents.IsLegalFor(5));
            talents.Clear();
            Assert.IsTrue(talents.IsLegalFor(5));
            Assert.AreEqual(4, talents.AvailablePoints(5));
        }

        [Test]
        public void EffectNamesMustBeRealNames()
        {
            Assert.AreEqual(TalentEffect.MeleeDamage, TalentEffects.Parse("meleeDamage"));
            Assert.AreEqual(TalentEffect.None, TalentEffects.Parse("1"));
            Assert.AreEqual(TalentEffect.None, TalentEffects.Parse("Sparkles"));
            Assert.AreEqual(TalentEffect.None, TalentEffects.Parse(""));
        }

        [Test]
        public void WalletRefusesNegativeSpends()
        {
            var wallet = new Wallet(10, 0);
            Assert.IsFalse(wallet.TrySpend(-500, 0));
            Assert.AreEqual(10, wallet.Gold);
        }
    }

    public class TalentTests
    {
        [Test]
        public void NoPointsAtLevelOne()
        {
            var talents = new TalentState(TestData.Progression());
            Assert.AreEqual(0, talents.AvailablePoints(1));
            Assert.IsFalse(talents.TrySpend("melee", 1));
        }

        [Test]
        public void OnePointPerLevelFromLevelTwo()
        {
            var talents = new TalentState(TestData.Progression());
            Assert.IsTrue(talents.TrySpend("melee", 3));
            Assert.IsTrue(talents.TrySpend("special", 3));
            Assert.IsFalse(talents.TrySpend("ranged", 3));
            Assert.AreEqual(0, talents.AvailablePoints(3));
        }

        [Test]
        public void TalentStopsAtMaxRank()
        {
            var talents = new TalentState(TestData.Progression());
            for (int i = 0; i < 3; i++) Assert.IsTrue(talents.TrySpend("melee", 5));
            Assert.IsFalse(talents.TrySpend("melee", 5));
            Assert.AreEqual(1, talents.AvailablePoints(5));
        }

        [Test]
        public void TalentsFeedDamageAndCooldown()
        {
            var talents = new TalentState(TestData.Progression());
            talents.TrySpend("melee", 5);
            talents.TrySpend("melee", 5);
            talents.TrySpend("special", 5);
            Assert.AreEqual(1.2f, HeroBuild.DamageBonus(AttackKind.Melee, talents), 0.001f);
            Assert.AreEqual(1f, HeroBuild.DamageBonus(AttackKind.Ranged, talents), 0.001f);
            Assert.AreEqual(0.08f, HeroBuild.SpecialCooldownReduction(new StatBlock(), talents), 0.001f);
        }

        [Test]
        public void ResetRefundsPointsForGold()
        {
            var talents = new TalentState(TestData.Progression());
            talents.TrySpend("melee", 3);
            Assert.IsFalse(talents.TryReset(new Wallet(50, 0)));
            var wallet = new Wallet(150, 0);
            Assert.IsTrue(talents.TryReset(wallet));
            Assert.AreEqual(50, wallet.Gold);
            Assert.AreEqual(2, talents.AvailablePoints(3));
            Assert.AreEqual(0, talents.GetRank("melee"));
        }
    }

    public class GearTests
    {
        [Test]
        public void RefineCostsRiseEachStep()
        {
            var gear = TestData.Gear();
            var sword = new ItemInstance { definitionId = "sword" };
            var first = Refinement.NextCost(sword, gear);
            Assert.AreEqual(100, first.gold);
            Assert.AreEqual(2, first.materials);
            sword.refineLevel = 1;
            var second = Refinement.NextCost(sword, gear);
            Assert.AreEqual(283, second.gold);
            Assert.AreEqual(4, second.materials);
        }

        [Test]
        public void RefineNeedsFundsAndLeavesItemAloneWithoutThem()
        {
            var gear = TestData.Gear();
            var sword = new ItemInstance { definitionId = "sword" };
            var wallet = new Wallet(99, 10);
            Assert.IsFalse(Refinement.TryRefine(sword, gear, wallet));
            Assert.AreEqual(0, sword.refineLevel);
            Assert.AreEqual(99, wallet.Gold);
        }

        [Test]
        public void RefineStopsAtTheMaxLevel()
        {
            var gear = TestData.Gear();
            var sword = new ItemInstance { definitionId = "sword" };
            var wallet = new Wallet(100000, 1000);
            for (int i = 0; i < 3; i++) Assert.IsTrue(Refinement.TryRefine(sword, gear, wallet));
            Assert.IsFalse(Refinement.TryRefine(sword, gear, wallet));
            Assert.AreEqual(3, sword.refineLevel);
            Assert.AreEqual(12.4f, Refinement.Stats(sword, gear).power, 0.001f);
        }

        [Test]
        public void HeroStatsCombineLevelAndWeapon()
        {
            var gear = TestData.Gear();
            var sword = new ItemInstance { definitionId = "sword" };
            var stats = HeroBuild.ComputeStats(TestData.Hero(), 3, Refinement.Stats(sword, gear), null);
            Assert.AreEqual(36f, stats.power, 0.001f);
            Assert.AreEqual(250f, stats.maxHealth, 0.001f);
            Assert.AreEqual(24f, stats.armor, 0.001f);
        }
    }

    public class RewardTests
    {
        [Test]
        public void KillsPayXpEvenWithoutAClear()
        {
            var tally = new RunTally(TestData.Gear().loot, TestData.NoLuck);
            tally.RecordKill(new EnemyDefinition { xp = 10 });
            tally.RecordKill(new EnemyDefinition { xp = 40 });
            Assert.AreEqual(50, tally.Rewards.xp);
            Assert.AreEqual(0, tally.Rewards.gold);
            Assert.IsFalse(tally.IsCompleted);
            Assert.IsNull(tally.Rewards.drop);
        }

        [Test]
        public void ClearPaysLootWithinTheTableRange()
        {
            var lucky = new RunTally(TestData.Gear().loot, TestData.AllLuck).CompleteDungeon();
            Assert.AreEqual(40, lucky.gold);
            Assert.AreEqual(1, lucky.materials);
            Assert.AreEqual("sword", lucky.drop.definitionId);

            Assert.AreEqual(25, lucky.xp, "clear bonus XP");

            var unlucky = new RunTally(TestData.Gear().loot, TestData.NoLuck).CompleteDungeon();
            Assert.AreEqual(80, unlucky.gold);
            Assert.AreEqual(3, unlucky.materials);
            Assert.IsNull(unlucky.drop);
        }

        [Test]
        public void ClearingTwiceDoesNotPayTwice()
        {
            var tally = new RunTally(TestData.Gear().loot, TestData.NoLuck);
            tally.CompleteDungeon();
            var again = tally.CompleteDungeon();
            Assert.AreEqual(80, again.gold);
            Assert.AreEqual(25, again.xp);
        }
    }
}
