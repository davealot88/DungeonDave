using System;

namespace DungeonDave.Core
{
    public sealed class Wallet
    {
        public int Gold { get; private set; }
        public int Materials { get; private set; }

        public Wallet(int gold = 0, int materials = 0)
        {
            Gold = Math.Max(0, gold);
            Materials = Math.Max(0, materials);
        }

        public void Add(int gold, int materials)
        {
            Gold += Math.Max(0, gold);
            Materials += Math.Max(0, materials);
        }

        public bool CanAfford(int gold, int materials)
        {
            return Gold >= gold && Materials >= materials;
        }

        public bool TrySpend(int gold, int materials)
        {
            if (gold < 0 || materials < 0) return false;
            if (!CanAfford(gold, materials)) return false;
            Gold -= gold;
            Materials -= materials;
            return true;
        }
    }

    /// <summary>One owned piece of gear.</summary>
    [Serializable]
    public class ItemInstance
    {
        public string definitionId;
        public int refineLevel;
    }

    public struct RefineCost
    {
        public int gold;
        public int materials;
    }

    /// <summary>Refine: gold and materials raise an item +1 at a time. It never fails or breaks the item.</summary>
    public static class Refinement
    {
        public static bool CanRefineFurther(ItemInstance item, GearDefinition gear)
        {
            return item.refineLevel < gear.maxRefineLevel;
        }

        /// <summary>Cost of the next step from the item's current level.</summary>
        public static RefineCost NextCost(ItemInstance item, GearDefinition gear)
        {
            int next = item.refineLevel + 1;
            return new RefineCost
            {
                gold = (int)Math.Round(gear.refineGoldBase * Math.Pow(next, gear.refineGoldExponent), MidpointRounding.AwayFromZero),
                materials = gear.refineMaterialsPerLevel * next,
            };
        }

        public static bool TryRefine(ItemInstance item, GearDefinition gear, Wallet wallet)
        {
            if (!CanRefineFurther(item, gear)) return false;
            var cost = NextCost(item, gear);
            if (!wallet.TrySpend(cost.gold, cost.materials)) return false;
            item.refineLevel++;
            return true;
        }

        public static float StatMultiplier(int refineLevel, GearDefinition gear)
        {
            return 1f + gear.refineStatPerLevel * refineLevel;
        }

        /// <summary>The item's stats at its current refine level.</summary>
        public static StatBlock Stats(ItemInstance item, GearDefinition gear)
        {
            var def = gear.FindItem(item.definitionId);
            if (def == null || def.stats == null) return new StatBlock();
            return def.stats.Scaled(StatMultiplier(item.refineLevel, gear));
        }
    }
}
