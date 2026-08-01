using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Backend.Services.RemoteConfig
{
    /// <summary>
    /// Firebase Remote Config 추상화.
    /// </summary>
    public interface IRemoteConfigService
    {
        /// <summary>
        /// 초기화·페치 완료 여부.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// 마지막 페치 성공 여부.
        /// </summary>
        bool LastFetchSucceeded { get; }

        /// <summary>
        /// Remote Config 를 초기화하고 페치한다. 실패 시 번들 기본값으로 동작한다.
        /// </summary>
        UniTask<bool> InitializeAndFetchAsync();

        /// <summary>
        /// 문자열 값을 조회한다.
        /// </summary>
        string GetString(string key, string defaultValue = null);

        /// <summary>
        /// 실수 값을 조회한다.
        /// </summary>
        double GetDouble(string key, double defaultValue);

        /// <summary>
        /// 불리언 값을 조회한다.
        /// </summary>
        bool GetBool(string key, bool defaultValue);

        /// <summary>
        /// 현재 활성 값 맵을 반환한다.
        /// </summary>
        IReadOnlyDictionary<string, string> GetAllValues();
    }
}
