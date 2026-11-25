namespace DungeonDave.Core
{
    public sealed class RunRewards
    {
        public int xp;
        public int gold;
        public int materials;
        /// <summary>Item that dropped, or null.</summary>
        public ItemInstance drop;
    }

    /// <summary>Tallies what a run pays out: XP as enemies die, loot when the dungeon is cleared.</summary>
    public sealed class RunTally
    {
        private readonly LootDefinition loot;
        private readonly IRandom rng;

        public RunRewards Rewards { get; private set; }
        public int Kills { get; private set; }
        public bool IsCompleted { get; private set; }

        public RunTally(LootDefinition loot, IRandom rng)
        {
            this.loot = loot;
            this.rng = rng;
            Rewards = new RunRewards();
        }

        public void RecordKill(EnemyDefinition enemy)
        {
            Kills++;
            Rewards.xp += enemy.xp;
        }

        /// <summary>
        /// Call when the boss dies. A failed run keeps its kill XP but gets no loot.
        /// Calling it again changes nothing, so loot can never be paid twice.
        /// </summary>
        public RunRewards CompleteDungeon()
        {
            if (IsCompleted) return Rewards;
            IsCompleted = true;
            Rewards.xp += loot.clearXp;
            Rewards.gold += rng.Range(loot.goldMin, loot.goldMax);
            Rewards.materials += rng.Range(loot.materialsMin, loot.materialsMax);
            if (!string.IsNullOrEmpty(loot.weaponDropId) && rng.Chance(loot.weaponDropChance))
                Rewards.drop = new ItemInstance { definitionId = loot.weaponDropId, refineLevel = 0 };
            return Rewards;
        }
    }
}
