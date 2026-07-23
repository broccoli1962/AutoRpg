using Backend.GameSystems.Exploration;
using Backend.Util;
using UnityEngine;

namespace Backend.Object.Controller
{
    /// <summary>
    /// 탐험 실시간 틱 누적·처리 Controller. ExplorationSystem.AccumulateTick 을 구동한다.
    /// </summary>
    public sealed class ExplorationTickController : CachedMonobehaviour
    {
        private void Update()
        {
            if (GameStateUtil.IsQuitting)
                return;

            ExplorationSystem.AccumulateTick(Time.deltaTime);
        }
    }
}
