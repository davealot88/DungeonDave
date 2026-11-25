using DungeonDave.Core;
using NUnit.Framework;

namespace DungeonDave.Core.Tests
{
    public class DamageTests
    {
        [Test]
        public void PlainHitIsPowerTimesMultiplier()
        {
            var result = DamageCalc.Compute(TestData.Attacker(20f), 0f, 1.5f, TestData.Combat(), TestData.NoLuck);
            Assert.AreEqual(30, result.amount);
            Assert.IsFalse(result.isCrit);
        }

        [Test]
        public void ArmorOfOneHundredHalvesDamage()
        {
            var result = DamageCalc.Compute(TestData.Attacker(20f), 100f, 1f, TestData.Combat(), TestData.NoLuck);
            Assert.AreEqual(10, result.amount);
        }

        [Test]
        public void CritMultipliesByCritDamage()
        {
            var result = DamageCalc.Compute(TestData.Attacker(20f), 0f, 1f, TestData.Combat(), TestData.AllLuck);
            Assert.AreEqual(30, result.amount);
            Assert.IsTrue(result.isCrit);
        }

        [Test]
        public void HitAlwaysDoesAtLeastOne()
        {
            var result = DamageCalc.Compute(TestData.Attacker(1f), 5000f, 0.1f, TestData.Combat(), TestData.NoLuck);
            Assert.AreEqual(1, result.amount);
        }

        [Test]
        public void SeededRandomRepeatsAndStaysInRange()
        {
            var a = new SeededRandom(42);
            var b = new SeededRandom(42);
            for (int i = 0; i < 1000; i++)
            {
                float value = a.NextFloat();
                Assert.AreEqual(value, b.NextFloat());
                Assert.That(value, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
            }
        }
    }

    public class ComboTests
    {
        [Test]
        public void FirstTapStartsStepOne()
        {
            var combo = new ComboTracker(TestData.Combo());
            Assert.AreEqual(1, combo.Press());
            Assert.AreEqual(1, combo.CurrentStep);
        }

        [Test]
        public void TapDuringSwingIsBufferedAndChains()
        {
            var combo = new ComboTracker(TestData.Combo());
            combo.Press();
            Assert.AreEqual(0, combo.Press());
            Assert.AreEqual(2, combo.Tick(0.3f));
            Assert.AreEqual(2, combo.CurrentStep);
        }

        [Test]
        public void TapInsideWindowChainsAllThreeHits()
        {
            var combo = new ComboTracker(TestData.Combo());
            Assert.AreEqual(1, combo.Press());
            combo.Tick(0.35f);
            Assert.AreEqual(2, combo.Press());
            combo.Tick(0.35f);
            Assert.AreEqual(3, combo.Press());
        }

        [Test]
        public void MissingTheWindowRestartsTheCombo()
        {
            var combo = new ComboTracker(TestData.Combo());
            combo.Press();
            combo.Tick(0.3f);
            combo.Tick(0.5f);
            Assert.AreEqual(1, combo.Press());
        }

        [Test]
        public void ComboRestartsAfterTheFinisher()
        {
            var combo = new ComboTracker(TestData.Combo());
            combo.Press(); combo.Press(); combo.Tick(0.3f);
            combo.Press(); Assert.AreEqual(3, combo.Tick(0.3f));
            Assert.AreEqual(0, combo.Press(), "taps during the finisher are not buffered");
            Assert.AreEqual(0, combo.Tick(0.3f));
            Assert.IsFalse(combo.IsSwinging);
            Assert.AreEqual(1, combo.Press());
        }

        [Test]
        public void ResetDropsTheCombo()
        {
            var combo = new ComboTracker(TestData.Combo());
            combo.Press();
            combo.Tick(0.35f);
            combo.Reset();
            Assert.AreEqual(1, combo.Press());
        }
    }

    public class DodgeAndStaminaTests
    {
        [Test]
        public void DodgeSpendsStaminaAndGrantsInvulnerability()
        {
            var stamina = new Stamina(TestData.Stamina());
            var dodge = new DodgeState(TestData.Dodge());
            Assert.IsTrue(dodge.TryStart(stamina));
            Assert.AreEqual(70f, stamina.Current, 0.001f);
            Assert.IsTrue(dodge.IsInvulnerable);
        }

        [Test]
        public void InvulnerabilityEndsBeforeTheDashDoes()
        {
            var dodge = new DodgeState(TestData.Dodge());
            dodge.TryStart(new Stamina(TestData.Stamina()));
            dodge.Tick(0.3f);
            Assert.IsFalse(dodge.IsInvulnerable);
            Assert.IsTrue(dodge.IsDodging);
            dodge.Tick(0.1f);
            Assert.IsFalse(dodge.IsDodging);
        }

        [Test]
        public void CannotDodgeWhileDodgingOrWithoutStamina()
        {
            var stamina = new Stamina(TestData.Stamina());
            var dodge = new DodgeState(TestData.Dodge());
            Assert.IsTrue(dodge.TryStart(stamina));
            Assert.IsFalse(dodge.TryStart(stamina));
            for (int i = 0; i < 2; i++) { dodge.Tick(0.4f); Assert.IsTrue(dodge.TryStart(stamina)); }
            dodge.Tick(0.4f);
            Assert.IsFalse(dodge.TryStart(stamina), "10 stamina left, dodge costs 30");
        }

        [Test]
        public void StaminaRegenIsTheSameInSmallSteps()
        {
            var stamina = new Stamina(TestData.Stamina());
            stamina.TrySpend(60f);
            for (int i = 0; i < 1500; i++) stamina.Tick(0.001f);
            Assert.AreEqual(62.5f, stamina.Current, 0.1f);
        }

        [Test]
        public void StaminaWaitsThenRegenerates()
        {
            var stamina = new Stamina(TestData.Stamina());
            stamina.TrySpend(60f);
            stamina.Tick(0.5f);
            Assert.AreEqual(40f, stamina.Current, 0.001f);
            stamina.Tick(1f);
            Assert.AreEqual(62.5f, stamina.Current, 0.001f, "0.1 s of the delay was still left");
            stamina.Tick(10f);
            Assert.AreEqual(100f, stamina.Current, 0.001f);
        }
    }

    public class HitResolverTests
    {
        [Test]
        public void HitDamagesTheTarget()
        {
            var target = TestData.Target(100);
            var hit = HitResolver.Resolve(TestData.Attacker(20f), 1f, 0f, target, TestData.Combat(), TestData.NoLuck);
            Assert.IsTrue(hit.landed);
            Assert.AreEqual(20, hit.damage);
            Assert.AreEqual(80, target.Health.Current);
        }

        [Test]
        public void InvulnerableTargetTakesNothing()
        {
            var target = TestData.Target(100);
            target.Dodge = new DodgeState(TestData.Dodge());
            target.Dodge.TryStart(new Stamina(TestData.Stamina()));
            var hit = HitResolver.Resolve(TestData.Attacker(20f), 1f, 0f, target, TestData.Combat(), TestData.NoLuck);
            Assert.IsTrue(hit.dodged);
            Assert.IsFalse(hit.landed);
            Assert.AreEqual(100, target.Health.Current);
        }

        [Test]
        public void KillingBlowReportsKillAndOnlyRemainingHealth()
        {
            var target = TestData.Target(15);
            var hit = HitResolver.Resolve(TestData.Attacker(20f), 1f, 0f, target, TestData.Combat(), TestData.NoLuck);
            Assert.IsTrue(hit.killed);
            Assert.AreEqual(15, hit.damage);
            Assert.IsFalse(HitResolver.Resolve(TestData.Attacker(20f), 1f, 0f, target, TestData.Combat(), TestData.NoLuck).landed);
        }

        [Test]
        public void PoiseBreaksOnAccumulatedHitsThenResets()
        {
            var poise = new PoiseMeter(100f, 3f);
            Assert.IsFalse(poise.ApplyHit(10f));
            Assert.IsFalse(poise.ApplyHit(10f));
            Assert.IsFalse(poise.ApplyHit(60f));
            Assert.IsFalse(poise.ApplyHit(10f));
            Assert.IsTrue(poise.ApplyHit(10f));
            Assert.AreEqual(100f, poise.Current);
        }

        [Test]
        public void NegativePoiseDamageDoesNothing()
        {
            var poise = new PoiseMeter(100f, 3f);
            Assert.IsFalse(poise.ApplyHit(-50f));
            Assert.AreEqual(100f, poise.Current);
        }

        [Test]
        public void PoiseRecoversWhenLeftAlone()
        {
            var poise = new PoiseMeter(100f, 3f);
            poise.ApplyHit(60f);
            poise.Tick(3.1f);
            Assert.IsFalse(poise.ApplyHit(60f));
        }

        [Test]
        public void HeavyHitStaggersThroughResolver()
        {
            var target = TestData.Target(1000, 0f, 100f);
            Assert.IsFalse(HitResolver.Resolve(TestData.Attacker(), 1f, 10f, target, TestData.Combat(), TestData.NoLuck).staggered);
            Assert.IsTrue(HitResolver.Resolve(TestData.Attacker(), 3f, 100f, target, TestData.Combat(), TestData.NoLuck).staggered);
        }
    }

    public class CooldownAndChargeTests
    {
        [Test]
        public void CooldownBlocksUntilItRunsOut()
        {
            var cooldown = new Cooldown();
            Assert.IsTrue(cooldown.TryStart(12f, 0f, 0.5f));
            Assert.IsFalse(cooldown.TryStart(12f, 0f, 0.5f));
            cooldown.Tick(11f);
            Assert.IsFalse(cooldown.IsReady);
            cooldown.Tick(1.5f);
            Assert.IsTrue(cooldown.TryStart(12f, 0f, 0.5f));
        }

        [Test]
        public void CooldownReductionShortensAndIsCapped()
        {
            var cooldown = new Cooldown();
            cooldown.TryStart(12f, 0.25f, 0.5f);
            Assert.AreEqual(9f, cooldown.Remaining, 0.001f);
            var capped = new Cooldown();
            capped.TryStart(12f, 0.9f, 0.5f);
            Assert.AreEqual(6f, capped.Remaining, 0.001f);
        }

        [Test]
        public void ChargeScalesFromTapToFull()
        {
            var ranged = TestData.Ranged();
            Assert.AreEqual(1f, ChargeShot.Multiplier(ranged, 0f), 0.001f);
            Assert.AreEqual(2f, ChargeShot.Multiplier(ranged, 0.5f), 0.001f);
            Assert.AreEqual(3f, ChargeShot.Multiplier(ranged, 5f), 0.001f);
        }
    }
}
