using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.SunsetIsland.Managers
{
    //Simple wrapper for Resources in case I do asset bundle stuff later
    public class AssetManager
    {
        private static readonly Dictionary<Type, string> _pathLookup = new Dictionary<Type, string>
        {
            {typeof(TextAsset), "Config"},
            {typeof(Texture2D), "Textures"}
        };

        public static T Load<T>([NotNull] string name) where T : Object
        {
            var path = Path.Combine(_pathLookup[typeof(T)], name);
            return Resources.Load<T>(path);
        }

        public static ResourceRequest LoadAsync<T>([NotNull] string name) where T : Object
        {
            var path = Path.Combine(_pathLookup[typeof(T)], name);
            return Resources.LoadAsync<T>(path);
        }
    }
}