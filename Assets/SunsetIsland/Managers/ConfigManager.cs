using System;
using System.Collections.Generic;
using System.Text;
using Assets.SunsetIsland.Config;
using Assets.SunsetIsland.Config.Data;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.SunsetIsland.Managers
{
    public static class ConfigManager
    {
        private static readonly JsonConverter[] _converters = {new BlockDataConverter()};
        private static readonly Dictionary<Type, string> PathLookup = new Dictionary<Type, string>
        {
            {typeof(BlockData), "Blocks"}
        };

        private static readonly Dictionary<Type, object> Cached = new Dictionary<Type, object>();

        //For convenience
        public static UnityProperties UnityProperties { get; private set; }

        public static Properties Properties { get; private set; }

        public static void Initialize(UnityProperties unityProperties)
        {
            LoadProperties();
            UnityProperties = unityProperties;
        }

        private static void LoadProperties()
        {
            var resource = AssetManager.Load<TextAsset>("Properties");
            Properties = JsonConvert.DeserializeObject<Properties>(resource.text);
        }

        public static Dictionary<string, T> LoadAll<T>()
        {
            return InnerLoad<T>();
        }

        public static T Load<T>(string objectName)
        {
            var data = InnerLoad<T>();
            return data[objectName];
        }

        private static Dictionary<string, T> InnerLoad<T>()
        {
            var type = typeof(T);
            if (!Cached.ContainsKey(type))
            {
                var resource = AssetManager.Load<TextAsset>(PathLookup[type]);
                var cleanText = Clean(resource.text);
                Cached[type] = JsonConvert.DeserializeObject<Dictionary<string, T>>(cleanText, _converters);
            }
            var data = (Dictionary<string, T>) Cached[type];
            return data;
        }

        [NotNull]
        private static string Clean([NotNull] string source)
        {
            var text = new StringBuilder();
            var depth = 0;
            for (var i = 0; i < source.Length; i++)
                if (i < source.Length - 1 && source[i] == '/' && source[i + 1] == '*')
                    depth++;
                else if (i > 0 && source[i] == '/' && source[i - 1] == '*')
                    depth--;
                else if (depth == 0)
                    text.Append(source[i]);
            return text.ToString();
        }
    }
}