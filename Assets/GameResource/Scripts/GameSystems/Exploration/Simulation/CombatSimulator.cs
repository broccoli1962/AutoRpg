using Backend.GameSystems.Character;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Simulation
{
    public static class CombatSimulator
    {
        private const float RetreatHpThreshold = 0.3f;

        public static CombatResultPayload Simulate(
            PartyState party,
            ZoneDefinitions.MonsterDefinition monster,
            DeterministicRandom random,
            string zoneId)
        {
            var enemyHp = monster.Hp;
            var totalDamageDealt = 0;
            var totalDamageTaken = 0;
            var durationTicks = 0;
            var injuries = new System.Collections.Generic.List<CombatInjuryEntry>();
            var loot = new System.Collections.Generic.List<LootEntry>();
            var expGained = new System.Collections.Generic.Dictionary<string, int>();
            var partyIds = new System.Collections.Generic.List<string>();
            var bondMultiplier = RelationshipManager.GetBondCombatMultiplier(party);
            var riskMultiplier = ZoneDefinitions.GetRiskMultiplier(zoneId);
            var rewardMultiplier = ZoneDefinitions.GetRewardMultiplier(zoneId);

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

                    var damage = (int)(CalculateDamage(member, monster, random) * bondMultiplier);
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

                var incoming = (int)(CalculateIncomingDamage(monster, target, random) * riskMultiplier);
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

            var gold = (int)((monster.GoldReward + random.NextInt(6)) * rewardMultiplier);
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
                CharacterRole.Warrior => EquipmentService.GetEffectiveStr(attacker) * 1.2f + EquipmentService.GetEffectiveVit(attacker) * 0.2f,
                CharacterRole.Rogue => EquipmentService.GetEffectiveAgi(attacker) * 1.1f + EquipmentService.GetEffectiveStr(attacker) * 0.5f,
                CharacterRole.Mage => EquipmentService.GetEffectiveInt(attacker) * 1.3f,
                CharacterRole.Bard => EquipmentService.GetEffectiveInt(attacker) * 1.0f +
                                      EquipmentService.GetEffectiveAgi(attacker) * 0.6f +
                                      attacker.Luk * 0.2f,
                _ => EquipmentService.GetEffectiveStr(attacker)
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
                CharacterRole.Warrior => EquipmentService.GetEffectiveVit(target) * 0.8f + EquipmentService.GetEffectiveStr(target) * 0.3f,
                CharacterRole.Rogue => EquipmentService.GetEffectiveAgi(target) * 0.4f + EquipmentService.GetEffectiveVit(target) * 0.3f,
                CharacterRole.Mage => EquipmentService.GetEffectiveInt(target) * 0.2f + EquipmentService.GetEffectiveVit(target) * 0.2f,
                CharacterRole.Bard => EquipmentService.GetEffectiveAgi(target) * 0.35f + EquipmentService.GetEffectiveVit(target) * 0.25f,
                _ => EquipmentService.GetEffectiveVit(target) * 0.3f
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
