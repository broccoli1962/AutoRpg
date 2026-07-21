using System.Collections.Generic;
using Backend.GameSystems.DynamicEvent.Data;

namespace Backend.GameSystems.DynamicEvent.LLM
{
    /// <summary>
    /// LLM 실패 시 사용하는 동적 이벤트 템플릿 연출 문구.
    /// </summary>
    public static class DynamicEventTemplateFallback
    {
        public static DynamicEventLlmNarration BuildScene(DynamicEventTemplate template, string leaderName, int floor)
        {
            var narration = template.EventId switch
            {
                DynamicEventDefinitions.Fork002Id =>
                    $"{leaderName}(은)는 {floor}층에서 갈림길에 멈춰 섰다. 왼쪽은 습한 바람, 오른쪽은 고요한 정적이 흐른다.",
                DynamicEventDefinitions.EncounterMerchantId =>
                    $"{floor}층 통로 끝에서 낡은 망토를 두른 상인이 손짓한다. \"거래할 시간이 있나?\"",
                DynamicEventDefinitions.TrapPressurePlateId =>
                    $"{floor}층 바닥의 돌판이 살짝 내려앉았다. 앞쪽으로는 좁은 통로, 뒤로 물러설 여지도 있다.",
                DynamicEventDefinitions.ArtifactMuralId =>
                    $"{floor}층 벽면에 빛나는 벽화가 드러났다. 고대 문자와 함께 무언가의 기운이 감돈다.",
                DynamicEventDefinitions.HazardGasId =>
                    $"{floor}층에서 연녹색 가스가 밀려온다. 숨을 참고 돌파할지, 후퇴할지 판단해야 한다.",
                DynamicEventDefinitions.ForkWaterSoundId =>
                    $"{floor}층 갈림길에서 물소리와 메아리가 겹친다. 한쪽은 젖은 계단, 다른 쪽은 마른 암벽이다.",
                DynamicEventDefinitions.EncounterScholarId =>
                    $"{floor}층에서 필기구를 든 학자가 지도를 펼치고 있다. \"이 근처에 흥미로운 기록이 있소.\"",
                DynamicEventDefinitions.TrapPitId =>
                    $"{floor}층 바닥이 갑자기 꺼져 깊은 구덩이가 드러났다. 건너뛰거나 벽을 타고 올라갈 수 있다.",
                _ => $"{leaderName}(은)는 {floor}층에서 예상치 못한 상황에 맞닥뜨렸다."
            };

            return new DynamicEventLlmNarration
            {
                Narration = narration,
                Choices = BuildChoiceTexts(template)
            };
        }

        public static string BuildResult(
            DynamicEventTemplate template,
            string choiceId,
            DynamicEventOutcomeEffect outcome)
        {
            return template.EventId switch
            {
                DynamicEventDefinitions.Fork002Id =>
                    choiceId == "left_path"
                        ? outcome == DynamicEventOutcomeEffect.MinorResource
                            ? "왼쪽 길 끝에서 작은 자원 더미를 발견했다."
                            : "왼쪽 길에서 함정에 발을 헛디뎌 경미한 부상을 입었다."
                        : outcome == DynamicEventOutcomeEffect.RareEncounter
                            ? "오른쪽 정적 속에서 예상치 못한 존재와 마주쳤다."
                            : "오른쪽 길은 고요했고, 특별한 일 없이 지나갔다.",
                DynamicEventDefinitions.EncounterMerchantId =>
                    choiceId == "trade"
                        ? outcome == DynamicEventOutcomeEffect.GoldBonus
                            ? "상인과 거래해 유용한 물건과 골드를 얻었다."
                            : "거래는 성사됐지만 예상보다 손해를 봤다."
                        : "상인을 지나쳤고, 길은 조용히 이어졌다.",
                DynamicEventDefinitions.TrapPressurePlateId =>
                    choiceId == "step_back"
                        ? "돌아서며 함정을 피했지만 시간을 조금 잃었다."
                        : outcome == DynamicEventOutcomeEffect.MinorTrapDamage
                            ? "앞으로 밀고 가다 돌화살에 스침을 입었다."
                            : "조심스럽게 통과해 무사히 지나갔다.",
                DynamicEventDefinitions.ArtifactMuralId =>
                    choiceId == "study"
                        ? outcome == DynamicEventOutcomeEffect.MinorResource
                            ? "벽화를 해독해 숨겨진 단서와 작은 보상을 얻었다."
                            : "벽화를 건드리자 가스가 분출해 다쳤다."
                        : "벽화를 지나쳤고, 길은 평온했다.",
                DynamicEventDefinitions.HazardGasId =>
                    choiceId == "hold_breath"
                        ? outcome == DynamicEventOutcomeEffect.InjuryLight
                            ? "가스를 돌파했지만 호흡기에 자극이 남았다."
                            : "숨을 참고 빠르게 통과했다."
                        : "후퇴해 안전한 경로를 찾았다.",
                DynamicEventDefinitions.ForkWaterSoundId =>
                    choiceId == "wet_stairs"
                        ? outcome == DynamicEventOutcomeEffect.MinorResource
                            ? "젖은 계단 아래에서 빛나는 광물을 주웠다."
                            : "미끄러운 계단에서 넘어져 다쳤다."
                        : "마른 암벽 길을 택해 무사히 지나갔다.",
                DynamicEventDefinitions.EncounterScholarId =>
                    choiceId == "talk"
                        ? outcome == DynamicEventOutcomeEffect.RareEncounter
                            ? "학자의 이야기 끝에 수상한 존재가 나타났다."
                            : "유용한 지식을 얻고 길을 재개했다."
                        : "학자를 외면하고 길을 계속했다.",
                DynamicEventDefinitions.TrapPitId =>
                    choiceId == "jump"
                        ? outcome == DynamicEventOutcomeEffect.InjuryLight
                            ? "뛰어넘다 발목을 삐었다."
                            : "가볍게 뛰어넘어 무사히 건넜다."
                        : "벽을 타고 올라가며 시간을 들였지만 안전했다.",
                _ => $"선택({choiceId})의 결과: {outcome}"
            };
        }

        private static List<DynamicEventChoiceText> BuildChoiceTexts(DynamicEventTemplate template)
        {
            var list = new List<DynamicEventChoiceText>();
            foreach (var choice in template.Choices)
            {
                list.Add(new DynamicEventChoiceText
                {
                    Id = choice.Id,
                    Text = GetDefaultChoiceLabel(template.EventId, choice.Id)
                });
            }

            return list;
        }

        private static string GetDefaultChoiceLabel(string eventId, string choiceId)
        {
            return (eventId, choiceId) switch
            {
                (DynamicEventDefinitions.Fork002Id, "left_path") => "왼쪽 길로 간다",
                (DynamicEventDefinitions.Fork002Id, "right_path") => "오른쪽 길로 간다",
                (DynamicEventDefinitions.EncounterMerchantId, "trade") => "거래한다",
                (DynamicEventDefinitions.EncounterMerchantId, "ignore") => "지나친다",
                (DynamicEventDefinitions.TrapPressurePlateId, "step_back") => "뒤로 물러선다",
                (DynamicEventDefinitions.TrapPressurePlateId, "force_through") => "앞으로 밀고 간다",
                (DynamicEventDefinitions.ArtifactMuralId, "study") => "벽화를 조사한다",
                (DynamicEventDefinitions.ArtifactMuralId, "pass") => "그냥 지나간다",
                (DynamicEventDefinitions.HazardGasId, "hold_breath") => "숨 참고 돌파한다",
                (DynamicEventDefinitions.HazardGasId, "retreat") => "후퇴한다",
                (DynamicEventDefinitions.ForkWaterSoundId, "wet_stairs") => "젖은 계단으로 간다",
                (DynamicEventDefinitions.ForkWaterSoundId, "dry_wall") => "마른 암벽 길로 간다",
                (DynamicEventDefinitions.EncounterScholarId, "talk") => "이야기를 듣는다",
                (DynamicEventDefinitions.EncounterScholarId, "ignore") => "외면한다",
                (DynamicEventDefinitions.TrapPitId, "jump") => "뛰어넘는다",
                (DynamicEventDefinitions.TrapPitId, "climb") => "벽을 타고 올라간다",
                _ => choiceId
            };
        }
    }
}
