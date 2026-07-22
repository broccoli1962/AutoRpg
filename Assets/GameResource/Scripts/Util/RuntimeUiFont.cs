using UnityEngine;

namespace Backend.Util
{
    /// <summary>
    /// 런타임 생성 UI Text용 기본 폰트. LegacyRuntime.ttf 미존재 시 Arial로 대체한다.
    /// </summary>
    public static class RuntimeUiFont
    {
        private static Font _cached;

        public static Font Get()
        {
            if (_cached != null)
                return _cached;

            _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cached == null)
                _cached = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return _cached;
        }
    }
}
