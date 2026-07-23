using System.Collections.Generic;
using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Util
{
    /// <summary>
    /// 탐험 스테이지 액터·VFX 스프라이트 로드·캐시.
    /// </summary>
    public static class RuntimeStageSprites
    {
        private static readonly Dictionary<string, Sprite> Cache = new();

        public static Sprite Get(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return null;

            if (Cache.TryGetValue(keyName, out var cached))
                return cached;

            var address = AddressableKeys.InGame.Get(keyName);
            if (string.IsNullOrEmpty(address))
                return null;

            var sprite = ResourceManager.LoadResource<Sprite>(address);
            if (sprite == null)
            {
                var fileStem = System.IO.Path.GetFileNameWithoutExtension(address);
                var subAddress = $"{address}[{fileStem}_0]";
                sprite = ResourceManager.LoadResource<Sprite>(subAddress);
            }

            if (sprite != null)
                Cache[keyName] = sprite;

            return sprite;
        }

        public static void ApplyActor(Image image, string keyName, Color tint)
        {
            if (image == null)
                return;

            var sprite = Get(keyName);
            if (sprite == null)
            {
                image.sprite = null;
                image.color = tint;
                return;
            }

            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
        }

        public static void ApplyVfx(Image image, string keyName, Color tint)
        {
            if (image == null)
                return;

            var sprite = Get(keyName);
            if (sprite == null)
            {
                image.color = tint;
                return;
            }

            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = tint;
        }
    }
}
