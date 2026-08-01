using System;
using Backend.Chronicle;
using Backend.Meta.Characters;
using UnityEngine;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 등급별 탐험가 ID 풀을 담는 ScriptableObject 배너 정의.
    /// </summary>
    [CreateAssetMenu(fileName = "GachaBannerPool", menuName = "Abyss Chronicle/Gacha Banner Pool")]
    public sealed class GachaBannerPool : ScriptableObject, IGachaCharacterPool
    {
        [SerializeField] private string _bannerId = "standard";
        [SerializeField] private string[] _rPool = { "explorer_r_01", "explorer_r_02" };
        [SerializeField] private string[] _srPool = { "explorer_sr_01", "explorer_sr_02" };
        [SerializeField] private string[] _ssrPool = { "explorer_ssr_01" };
        [SerializeField] private string[] _urPool = { "explorer_ur_01" };

        public string BannerId => _bannerId;

        /// <summary>
        /// 등급 풀에서 캐릭터 1명을 추첨한다.
        /// </summary>
        public string PickCharacter(ExplorerGrade grade, IRandomSource random)
        {
            var pool = GetPool(grade);
            if (pool == null || pool.Length == 0)
                throw new InvalidOperationException($"[GachaBannerPool] Empty pool for grade {grade}.");

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var index = random.NextInt(0, pool.Length);
            return pool[index];
        }

        private string[] GetPool(ExplorerGrade grade)
        {
            return grade switch
            {
                ExplorerGrade.R => _rPool,
                ExplorerGrade.SR => _srPool,
                ExplorerGrade.SSR => _ssrPool,
                ExplorerGrade.UR => _urPool,
                _ => _rPool,
            };
        }
    }
}
