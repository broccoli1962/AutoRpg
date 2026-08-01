using Backend.Chronicle;
using Backend.GameSystems.Performance;
using Backend.Meta.StoreCompliance;
using Backend.Object.UI;
using Backend.Services;
using Backend.Util.Localization;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Backend.Object.Management
{
    public class GameManager : SingletonGameObject<GameManager>
    {
        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private async UniTask InitializeCore_Internal()
        {
            LocalizationService.Initialize();
            var phraseBank = await ChroniclePhraseBankLoader.LoadAsync();
            NarrationProvider.SetSource(new ChronicleNarrationSource(phraseBank));
            LogStripPipeline.Reset();
            await AudioManager.InitMixer();
            TableManager.Init();
            await UIManager.EnsureReadyAsync();
            await StoreComplianceManager.InitializeAsync();
            await PerformanceManager.InitializeAsync();
            await BackendManager.BootstrapAsync();
        }

        private void StartGameplay_Internal()
        {

        }

        private void EndGameplay_Internal()
        {
        }

        private void GameOver_Internal()
        {
        }

        private void StageClear_Internal()
        {
        }

        private void SetPhase_Internal(){
        }

#region Static Public Methods
        public static void EndGameplay() => Instance.EndGameplay_Internal();
        public static void GameOver() => Instance.GameOver_Internal();
        public static void StageClear() => Instance.StageClear_Internal();
        public static void StartGameplay() => Instance.StartGameplay_Internal();
        public static void SetPhase() => Instance.SetPhase_Internal();
        public static UniTask InitializeCore() => Instance.InitializeCore_Internal();
#endregion
    }
}
