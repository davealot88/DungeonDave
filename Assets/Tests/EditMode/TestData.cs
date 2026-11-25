using DungeonDave.Core;

namespace DungeonDave.Core.Tests
{
    /// <summary>Always returns the same roll. 0.99 = never crit / never drop, 0 = always.</summary>
    public sealed class FixedRandom : IRandom
    {
        private readonly float value;
        public FixedRandom(float value) { this.value = value; }
        public float NextFloat() { return value; }
    }

    /// <summary>Round numbers for rule tests, kept apart from the real balance in Assets/Data.</summary>
    public static class TestData
    {
        public static readonly IRandom NoLuck = new FixedRandom(0.99f);
        public static readonly IRandom AllLuck = new FixedRandom(0f);

        public static CombatDefinition Combat()
        {
            return new CombatDefinition { armorConstant = 100f, maxCooldownReduction = 0.5f };
        }

        public static ComboDefinition Combo()
        {
            return new ComboDefinition
            {
                chainWindow = 0.4f,
                steps = new[]
                {
                    new ComboStepDefinition { damageMultiplier = 1f, duration = 0.3f, poiseDamage = 10f },
                    new ComboStepDefinition { damageMultiplier = 1.2f, duration = 0.3f, poiseDamage = 10f },
                    new ComboStepDefinition { damageMultiplier = 1.8f, duration = 0.3f, poiseDamage = 60f },
                },
            };
        }

        public static StaminaDefinition Stamina()
        {
            return new StaminaDefinition { max = 100f, regenPerSecond = 25f, regenDelay = 0.6f };
        }

        public static DodgeDefinition Dodge()
        {
            return new DodgeDefinition { staminaCost = 30f, duration = 0.35f, invulnerableTime = 0.25f, distance = 4f };
        }

        public static RangedDefinition Ranged()
        {
            return new RangedDefinition { cooldown = 1.5f, minMultiplier = 1f, maxMultiplier = 3f, maxChargeTime = 1f };
        }

        public static StatBlock Attacker(float power = 20f)
        {
            return new StatBlock { power = power, critChance = 0.05f, critDamage = 1.5f };
        }

        public static Combatant Target(int health = 100, float armor = 0f, float poise = 100f)
        {
            return new Combatant
            {
                Stats = new StatBlock { armor = armor, maxHealth = health },
                Health = new Health(health),
                Poise = new PoiseMeter(poise, 3f),
            };
        }

        public static HeroDefinition Hero()
        {
            return new HeroDefinition
            {
                id = "warrior",
                baseStats = new StatBlock { power = 20f, maxHealth = 200f, armor = 20f, critChance = 0.05f, critDamage = 1.5f },
                perLevel = new StatBlock { power = 3f, maxHealth = 25f, armor = 2f },
            };
        }

        public static ProgressionDefinition Progression()
        {
            return new ProgressionDefinition
            {
                levelCap = 5,
                xpBase = 100f,
                xpExponent = 1f,
                talentResetGold = 100,
                talents = new[]
                {
                    new TalentDefinition { id = "melee", tree = "melee", effect = "MeleeDamage", valuePerRank = 0.1f, maxRank = 3 },
                    new TalentDefinition { id = "ranged", tree = "ranged", effect = "RangedDamage", valuePerRank = 0.15f, maxRank = 3 },
                    new TalentDefinition { id = "special", tree = "special", effect = "SpecialCooldown", valuePerRank = 0.08f, maxRank = 3 },
                },
            };
        }

        public static GearDefinition Gear()
        {
            return new GearDefinition
            {
                maxRefineLevel = 3,
                refineStatPerLevel = 0.08f,
                refineGoldBase = 100f,
                refineGoldExponent = 1.5f,
                refineMaterialsPerLevel = 2,
                items = new[]
                {
                    new ItemDefinition { id = "sword", slot = "weapon", rarity = "common", classId = "warrior", stats = new StatBlock { power = 10f } },
                },
                loot = new LootDefinition { weaponDropChance = 0.35f, weaponDropId = "sword", clearXp = 25, goldMin = 40, goldMax = 80, materialsMin = 1, materialsMax = 3 },
            };
        }
    }
}
