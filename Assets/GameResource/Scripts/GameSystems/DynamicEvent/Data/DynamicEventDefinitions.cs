using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.DynamicEvent.Data
{
    public static class DynamicEventDefinitions
    {
        public const string Fork002Id = "fork_002";
        public const string EncounterMerchantId = "encounter_merchant_01";
        public const string TrapPressurePlateId = "trap_pressure_01";
        public const string ArtifactMuralId = "artifact_mural_01";
        public const string HazardGasId = "hazard_gas_01";
        public const string ForkWaterSoundId = "fork_water_sound_01";
        public const string EncounterScholarId = "encounter_scholar_01";
        public const string TrapPitId = "trap_pit_01";
        public const string EncounterWandererId = "encounter_wanderer_01";
        public const string ArtifactCrystalId = "artifact_crystal_01";
        public const string ForkRuneMarkId = "fork_rune_mark_01";
        public const string HazardCollapseId = "hazard_collapse_01";
        public const string EncounterFairyId = "encounter_fairy_01";
        public const string ArtifactLoreFragmentId = "artifact_lore_fragment_01";
        public const string HazardQuicksandId = "hazard_quicksand_01";

        private static readonly DynamicEventTemplate Fork002 = CreateFork002();
        private static readonly DynamicEventTemplate EncounterMerchant = CreateEncounterMerchant();
        private static readonly DynamicEventTemplate TrapPressurePlate = CreateTrapPressurePlate();
        private static readonly DynamicEventTemplate ArtifactMural = CreateArtifactMural();
        private static readonly DynamicEventTemplate HazardGas = CreateHazardGas();
        private static readonly DynamicEventTemplate ForkWaterSound = CreateForkWaterSound();
        private static readonly DynamicEventTemplate EncounterScholar = CreateEncounterScholar();
        private static readonly DynamicEventTemplate TrapPit = CreateTrapPit();
        private static readonly DynamicEventTemplate EncounterWanderer = CreateEncounterWanderer();
        private static readonly DynamicEventTemplate ArtifactCrystal = CreateArtifactCrystal();
        private static readonly DynamicEventTemplate ForkRuneMark = CreateForkRuneMark();
        private static readonly DynamicEventTemplate HazardCollapse = CreateHazardCollapse();
        private static readonly DynamicEventTemplate EncounterFairy = CreateEncounterFairy();
        private static readonly DynamicEventTemplate ArtifactLoreFragment = CreateArtifactLoreFragment();
        private static readonly DynamicEventTemplate HazardQuicksand = CreateHazardQuicksand();

        public static IReadOnlyList<DynamicEventTemplate> All { get; } = new List<DynamicEventTemplate>
        {
            Fork002,
            EncounterMerchant,
            TrapPressurePlate,
            ArtifactMural,
            HazardGas,
            ForkWaterSound,
            EncounterScholar,
            TrapPit,
            EncounterWanderer,
            ArtifactCrystal,
            ForkRuneMark,
            HazardCollapse,
            EncounterFairy,
            ArtifactLoreFragment,
            HazardQuicksand
        };

        public static DynamicEventTemplate Get(string eventId)
        {
            foreach (var template in All)
            {
                if (template.EventId == eventId)
                    return template;
            }

            return null;
        }

        private static DynamicEventTemplate CreateFork002() => new()
        {
            EventId = Fork002Id,
            Category = DynamicEventCategory.ForkChoice,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.12f, 1, 15),
            Choices = new List<DynamicEventChoice>
            {
                Choice("left_path",
                    (DynamicEventOutcomeEffect.MinorResource, 0.6f),
                    (DynamicEventOutcomeEffect.MinorTrapDamage, 0.4f)),
                Choice("right_path",
                    (DynamicEventOutcomeEffect.RareEncounter, 0.35f),
                    (DynamicEventOutcomeEffect.SafePass, 0.65f))
            }
        };

        private static DynamicEventTemplate CreateEncounterMerchant() => new()
        {
            EventId = EncounterMerchantId,
            Category = DynamicEventCategory.Encounter,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.08f, 2, 20),
            Choices = new List<DynamicEventChoice>
            {
                Choice("trade",
                    (DynamicEventOutcomeEffect.GoldBonus, 0.55f),
                    (DynamicEventOutcomeEffect.MinorTrapDamage, 0.45f)),
                Choice("ignore", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateTrapPressurePlate() => new()
        {
            EventId = TrapPressurePlateId,
            Category = DynamicEventCategory.Hazard,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.1f, 1, 18),
            Choices = new List<DynamicEventChoice>
            {
                Choice("step_back", (DynamicEventOutcomeEffect.SafePass, 1f)),
                Choice("force_through",
                    (DynamicEventOutcomeEffect.SafePass, 0.5f),
                    (DynamicEventOutcomeEffect.MinorTrapDamage, 0.5f))
            }
        };

        private static DynamicEventTemplate CreateArtifactMural() => new()
        {
            EventId = ArtifactMuralId,
            Category = DynamicEventCategory.Artifact,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.07f, 3, 22),
            Choices = new List<DynamicEventChoice>
            {
                Choice("study",
                    (DynamicEventOutcomeEffect.MinorResource, 0.6f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.4f)),
                Choice("pass", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateHazardGas() => new()
        {
            EventId = HazardGasId,
            Category = DynamicEventCategory.Hazard,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.09f, 4, 25),
            Choices = new List<DynamicEventChoice>
            {
                Choice("hold_breath",
                    (DynamicEventOutcomeEffect.SafePass, 0.55f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.45f)),
                Choice("retreat", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateForkWaterSound() => new()
        {
            EventId = ForkWaterSoundId,
            Category = DynamicEventCategory.ForkChoice,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.11f, 2, 16),
            Choices = new List<DynamicEventChoice>
            {
                Choice("wet_stairs",
                    (DynamicEventOutcomeEffect.MinorResource, 0.5f),
                    (DynamicEventOutcomeEffect.MinorTrapDamage, 0.5f)),
                Choice("dry_wall", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateEncounterScholar() => new()
        {
            EventId = EncounterScholarId,
            Category = DynamicEventCategory.Encounter,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.06f, 5, 25),
            Choices = new List<DynamicEventChoice>
            {
                Choice("talk",
                    (DynamicEventOutcomeEffect.MinorResource, 0.65f),
                    (DynamicEventOutcomeEffect.RareEncounter, 0.35f)),
                Choice("ignore", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateTrapPit() => new()
        {
            EventId = TrapPitId,
            Category = DynamicEventCategory.Hazard,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.08f, 3, 20),
            Choices = new List<DynamicEventChoice>
            {
                Choice("jump",
                    (DynamicEventOutcomeEffect.SafePass, 0.6f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.4f)),
                Choice("climb", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateEncounterWanderer() => new()
        {
            EventId = EncounterWandererId,
            Category = DynamicEventCategory.Encounter,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.07f, 4, 18),
            Choices = new List<DynamicEventChoice>
            {
                Choice("help",
                    (DynamicEventOutcomeEffect.MinorResource, 0.5f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.5f)),
                Choice("ignore", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateArtifactCrystal() => new()
        {
            EventId = ArtifactCrystalId,
            Category = DynamicEventCategory.Artifact,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.06f, 6, 22),
            Choices = new List<DynamicEventChoice>
            {
                Choice("take",
                    (DynamicEventOutcomeEffect.GoldBonus, 0.45f),
                    (DynamicEventOutcomeEffect.RareEncounter, 0.55f)),
                Choice("leave", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateForkRuneMark() => new()
        {
            EventId = ForkRuneMarkId,
            Category = DynamicEventCategory.ForkChoice,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.1f, 3, 18),
            Choices = new List<DynamicEventChoice>
            {
                Choice("follow_glow",
                    (DynamicEventOutcomeEffect.MinorResource, 0.55f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.45f)),
                Choice("ignore_runes", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateHazardCollapse() => new()
        {
            EventId = HazardCollapseId,
            Category = DynamicEventCategory.Hazard,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.07f, 5, 24),
            Choices = new List<DynamicEventChoice>
            {
                Choice("dash",
                    (DynamicEventOutcomeEffect.SafePass, 0.5f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.5f)),
                Choice("cover", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateEncounterFairy() => new()
        {
            EventId = EncounterFairyId,
            Category = DynamicEventCategory.Encounter,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.05f, 4, 20),
            Choices = new List<DynamicEventChoice>
            {
                Choice("accept_gift",
                    (DynamicEventOutcomeEffect.MinorResource, 0.6f),
                    (DynamicEventOutcomeEffect.RareEncounter, 0.4f)),
                Choice("decline", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateArtifactLoreFragment() => new()
        {
            EventId = ArtifactLoreFragmentId,
            Category = DynamicEventCategory.Artifact,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.06f, 7, 25),
            Choices = new List<DynamicEventChoice>
            {
                Choice("read",
                    (DynamicEventOutcomeEffect.GoldBonus, 0.5f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.5f)),
                Choice("leave", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTemplate CreateHazardQuicksand() => new()
        {
            EventId = HazardQuicksandId,
            Category = DynamicEventCategory.Hazard,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = CreateFloorEnterTrigger(0.08f, 2, 19),
            Choices = new List<DynamicEventChoice>
            {
                Choice("pull_free",
                    (DynamicEventOutcomeEffect.SafePass, 0.55f),
                    (DynamicEventOutcomeEffect.InjuryLight, 0.45f)),
                Choice("wait", (DynamicEventOutcomeEffect.SafePass, 1f))
            }
        };

        private static DynamicEventTrigger CreateFloorEnterTrigger(float probability, int minFloor, int maxFloor) =>
            new()
            {
                Type = DynamicEventTriggerType.FloorEnter,
                ZoneIds = new List<string> { ZoneDefinitions.MossyHollowId },
                Probability = probability,
                MinFloor = minFloor,
                MaxFloor = maxFloor
            };

        private static DynamicEventChoice Choice(
            string id,
            params (DynamicEventOutcomeEffect effect, float weight)[] effects)
        {
            var choice = new DynamicEventChoice { Id = id };
            foreach (var (effect, weight) in effects)
                choice.EffectPool[effect] = weight;

            return choice;
        }
    }
}
