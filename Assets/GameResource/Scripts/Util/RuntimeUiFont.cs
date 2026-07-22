using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;

namespace Backend.Util
{
    /// <summary>
    /// HUD용 폰트. BMJUA(Addressable) 우선, 없으면 LegacyRuntime.
    /// </summary>
    public static class RuntimeUiFont
    {
        private const string HudFontKey = "BMJUA";
        private static Font _cached;

        public static Font Get()
        {
            if (_cached != null)
                return _cached;

            var address = AddressableKeys.InGame.Get(HudFontKey);
            if (!string.IsNullOrEmpty(address))
                _cached = ResourceManager.LoadResource<Font>(address);

            if (_cached == null)
                _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (_cached == null)
                _cached = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return _cached;
        }
    }
}
