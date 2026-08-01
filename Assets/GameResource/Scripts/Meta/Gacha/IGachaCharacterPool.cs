using Backend.Chronicle;
using Backend.Meta.Characters;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 배너별 등급→캐릭터 풀 조회·추첨 인터페이스.
    /// </summary>
    public interface IGachaCharacterPool
    {
        /// <summary>
        /// 배너 식별자를 반환한다.
        /// </summary>
        string BannerId { get; }

        /// <summary>
        /// 등급 풀에서 캐릭터 1명을 추첨한다.
        /// </summary>
        string PickCharacter(ExplorerGrade grade, IRandomSource random);
    }
}
