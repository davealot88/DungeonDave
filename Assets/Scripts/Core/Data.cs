using System;

// Plain data classes that mirror the JSON files in Assets/Data.
// Public camelCase fields on purpose: Unity's JsonUtility fills fields by exact name.
namespace DungeonDave.Core
{
    public enum AttackKind { Melee, Ranged, Special }

    /// <summary>Rules shared by every fight (combat.json).</summary>
    [Serializable]
    public class CombatDefinition
    {
        /// <summary>Armor equal to this value blocks half of incoming damage.</summary>
        public float armorConstant;
        /// <summary>Cooldown reduction can never cut a cooldown by more than this fraction.</summary>
        public float maxCooldownReduction;
    }

    [Serializable]
    public class StaminaDefinition
    {
        public float max;
        public float regenPerSecond;
        public float regenDelay;
    }

    [Serializable]
    public class DodgeDefinition
    {
        public float staminaCost;
        public float duration;
        public float invulnerableTime;
        public float distance;
    }

    [Serializable]
    public class ComboStepDefinition
    {
        public float damageMultiplier;
        public float duration;
        public float poiseDamage;
    }

    [Serializable]
    public class ComboDefinition
    {
        public float chainWindow;
        public ComboStepDefinition[] steps;
    }

    [Serializable]
    public class RangedDefinition
    {
        public float cooldown;
        public float minMultiplier;
        public float maxMultiplier;
        public float maxChargeTime;
        public float projectileSpeed;
        public float poiseDamage;
    }

    [Serializable]
    public class SpecialDefinition
    {
        public string id;
        public string displayName;
        /// <summary>"aoe" or "single".</summary>
        public string kind;
        public float damageMultiplier;
        public float radius;
        public float range;
        public float cooldown;
        public float poiseDamage;

        public bool IsAreaOfEffect { get { return kind == "aoe"; } }
    }

    [Serializable]
    public class HeroDefinition
    {
        public string id;
        public string displayName;
        public float moveSpeed;
        public StatBlock baseStats;
        public StatBlock perLevel;
        public StaminaDefinition stamina;
        public DodgeDefinition dodge;
        public ComboDefinition combo;
        public RangedDefinition ranged;
        public SpecialDefinition[] specials;

        public SpecialDefinition FindSpecial(string specialId)
        {
            if (specials == null) return null;
            foreach (var s in specials) if (s.id == specialId) return s;
            return null;
        }
    }

    [Serializable]
    public class EnemyDefinition
    {
        public string id;
        public string displayName;
        /// <summary>"grunt", "archer", "brute", "caster" or "boss".</summary>
        public string role;
        public float maxHealth;
        public float power;
        public float armor;
        public float poise;
        public float poiseRecoverDelay;
        public float staggerDuration;
        public float moveSpeed;
        public float attackRange;
        public float attackWindup;
        public float attackCooldown;
        public int xp;
    }

    [Serializable]
    public class EnemyFile
    {
        public EnemyDefinition[] enemies;

        public EnemyDefinition Find(string enemyId)
        {
            foreach (var e in enemies) if (e.id == enemyId) return e;
            return null;
        }
    }

    [Serializable]
    public class TalentDefinition
    {
        public string id;
        public string displayName;
        /// <summary>"melee", "ranged" or "special".</summary>
        public string tree;
        /// <summary>Name of a <see cref="TalentEffect"/> value.</summary>
        public string effect;
        public float valuePerRank;
        public int maxRank;
    }

    [Serializable]
    public class ProgressionDefinition
    {
        public int levelCap;
        public float xpBase;
        public float xpExponent;
        public int talentResetGold;
        public TalentDefinition[] talents;

        public TalentDefinition FindTalent(string talentId)
        {
            foreach (var t in talents) if (t.id == talentId) return t;
            return null;
        }
    }

    [Serializable]
    public class ItemDefinition
    {
        public string id;
        public string displayName;
        /// <summary>"weapon", "armor", "helm", "boots" or "trinket".</summary>
        public string slot;
        /// <summary>"common", "uncommon", "rare", "epic" or "legendary".</summary>
        public string rarity;
        /// <summary>Hero id for class-specific items (weapons); empty when any class can use it.</summary>
        public string classId;
        public StatBlock stats;
    }

    [Serializable]
    public class LootDefinition
    {
        public float weaponDropChance;
        public string weaponDropId;
        /// <summary>Bonus XP for clearing the dungeon, on top of kill XP.</summary>
        public int clearXp;
        public int goldMin;
        public int goldMax;
        public int materialsMin;
        public int materialsMax;
    }

    [Serializable]
    public class GearDefinition
    {
        public int maxRefineLevel;
        public float refineStatPerLevel;
        public float refineGoldBase;
        public float refineGoldExponent;
        public int refineMaterialsPerLevel;
        public ItemDefinition[] items;
        public LootDefinition loot;

        public ItemDefinition FindItem(string itemId)
        {
            foreach (var i in items) if (i.id == itemId) return i;
            return null;
        }
    }
}
