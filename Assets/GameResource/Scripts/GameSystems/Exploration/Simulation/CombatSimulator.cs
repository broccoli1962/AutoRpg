using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Simulation
{
    public static class CombatSimulator
    {
        private const float RetreatHpThreshold = 0.3f;

        public static CombatResultPayload Simulate(
            PartyState party,
            ZoneDefinitions.MonsterDefinition monster,
            DeterministicRandom random)
        {
            var enemyHp = monster.Hp;
            var totalDamageDealt = 0;
            var totalDamageTaken = 0;
            var durationTicks = 0;
            var injuries = new System.Collections.Generic.List<CombatInjuryEntry>();
            var loot = new System.Collections.Generic.List<LootEntry>();
            var expGained = new System.Collections.Generic.Dictionary<string, int>();
            var partyIds = new System.Collections.Generic.List<string>();

            foreach (var member in party.Members)
            {
                if (member.CurrentHp > 0)
                    partyIds.Add(member.CharacterId);
            }

            while (enemyHp > 0 && durationTicks < 24)
            {
                durationTicks++;

                var livingMembers = 0;
                var partyHpSum = 0;
                var partyMaxHpSum = 0;

                foreach (var member in party.Members)
                {
                    if (member.CurrentHp <= 0)
                        continue;

                    livingMembers++;
                    partyHpSum += member.CurrentHp;
                    partyMaxHpSum += member.MaxHp;

                    var damage = CalculateDamage(member, monster, random);
                    enemyHp -= damage;
                    totalDamageDealt += damage;
                }

                if (livingMembers == 0)
                    break;

                if (partyMaxHpSum > 0 && partyHpSum / (float)partyMaxHpSum <= RetreatHpThreshold)
                {
                    return BuildResult(
                        partyIds,
                        monster,
                        CombatOutcome.Retreat,
                        durationTicks,
                        totalDamageDealt,
                        totalDamageTaken,
                        injuries,
                        loot,
                        expGained,
                        0);
                }

                if (enemyHp <= 0)
                    break;

                var target = SelectTarget(party, random);
                if (target == null)
                    break;

                var incoming = CalculateIncomingDamage(monster, target, random);
                target.CurrentHp = System.Math.Max(0, target.CurrentHp - incoming);
                totalDamageTaken += incoming;

                if (incoming > 0 && target.CurrentHp > 0 && random.RollChance(0.12f))
                {
                    target.Injury = InjurySeverity.Light;
                    injuries.Add(new CombatInjuryEntry
                    {
                        CharacterId = target.CharacterId,
                        Severity = InjurySeverity.Light
                    });
                }

                if (target.CurrentHp <= 0)
                    target.Injury = InjurySeverity.Fatal;
            }

            var livingCount = 0;
            foreach (var member in party.Members)
            {
                if (member.CurrentHp > 0)
                    livingCount++;
            }

            if (livingCount == 0)
            {
                return BuildResult(
                    partyIds,
                    monster,
                    CombatOutcome.Defeat,
                    durationTicks,
                    totalDamageDealt,
                    totalDamageTaken,
                    injuries,
                    loot,
                    expGained,
                    0);
            }

            if (enemyHp > 0)
            {
                return BuildResult(
                    partyIds,
                    monster,
                    CombatOutcome.Retreat,
                    durationTicks,
                    totalDamageDealt,
                    totalDamageTaken,
                    injuries,
                    loot,
                    expGained,
                    0);
            }

            var gold = monster.GoldReward + random.NextInt(6);
            var expPerMember = 8 + monster.Hp / 4;

            foreach (var member in party.Members)
            {
                if (member.CurrentHp <= 0)
                    continue;

                expGained[member.CharacterId] = expPerMember;
            }

            if (monster.Rarity >= MonsterRarity.Notable && random.RollChance(0.35f))
            {
                loot.Add(new LootEntry { ItemId = "mana_shard", Quantity = 1 });
            }

            return BuildResult(
                partyIds,
                monster,
                CombatOutcome.Victory,
                durationTicks,
                totalDamageDealt,
                totalDamageTaken,
                injuries,
                loot,
                expGained,
                gold);
        }

        private static CharacterState SelectTarget(PartyState party, DeterministicRandom random)
        {
            var candidates = new System.Collections.Generic.List<CharacterState>();
            foreach (var member in party.Members)
            {
                if (member.CurrentHp > 0)
                    candidates.Add(member);
            }

            if (candidates.Count == 0)
                return null;

            return candidates[random.NextInt(candidates.Count)];
        }

        private static int CalculateDamage(CharacterState attacker, ZoneDefinitions.MonsterDefinition monster, DeterministicRandom random)
        {
            var attack = attacker.Role switch
            {
                CharacterRole.Warrior => attacker.Str * 1.2f + attacker.Vit * 0.2f,
                CharacterRole.Rogue => attacker.Agi * 1.1f + attacker.Str * 0.5f,
                CharacterRole.Mage => attacker.Int * 1.3f,
                _ => attacker.Str
            };

            var critMultiplier = random.RollChance(0.15f) ? 1.5f : 1f;
            var variance = random.NextRange(0.9f, 1.1f);
            var raw = (attack - monster.Defense * 0.5f) * critMultiplier * variance;
            return System.Math.Max(1, (int)raw);
        }

        private static int CalculateIncomingDamage(
            ZoneDefinitions.MonsterDefinition monster,
            CharacterState target,
            DeterministicRandom random)
        {
            var defense = target.Role switch
            {
                CharacterRole.Warrior => target.Vit * 0.8f + target.Str * 0.3f,
                CharacterRole.Rogue => target.Agi * 0.4f + target.Vit * 0.3f,
                CharacterRole.Mage => target.Int * 0.2f + target.Vit * 0.2f,
                _ => target.Vit * 0.3f
            };

            var variance = random.NextRange(0.9f, 1.1f);
            var raw = (monster.Attack - defense * 0.5f) * variance;
            return System.Math.Max(0, (int)raw);
        }

        private static CombatResultPayload BuildResult(
            System.Collections.Generic.List<string> partyIds,
            ZoneDefinitions.MonsterDefinition monster,
            CombatOutcome outcome,
            int durationTicks,
            int damageDealt,
            int damageTaken,
            System.Collections.Generic.List<CombatInjuryEntry> injuries,
            System.Collections.Generic.List<LootEntry> loot,
            System.Collections.Generic.Dictionary<string, int> expGained,
            int goldGained)
        {
            return new CombatResultPayload
            {
                Party = partyIds,
                EnemyGroup = { monster.Id },
                Outcome = outcome,
                DurationTicks = durationTicks,
                DamageDealt = damageDealt,
                DamageTaken = damageTaken,
                Injuries = injuries,
                Loot = loot,
                ExpGained = expGained,
                GoldGained = goldGained,
                MonsterDisplayName = monster.DisplayName
            };
        }
    }
}
