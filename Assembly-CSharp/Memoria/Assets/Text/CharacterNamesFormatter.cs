﻿using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Assets
{
    public static class CharacterNamesFormatter
    {
        public static Dictionary<CharacterId, String> CharacterDefaultNames()
        {
            Dictionary<CharacterId, String> nameDict;
            if (DefaultNamesByLang.TryGetValue(Localization.CurrentSymbol, out nameDict))
                return nameDict;
            return new Dictionary<CharacterId, String>();
        }

        public static Dictionary<CharacterId, String> CharacterScriptNames()
        {
            Boolean hasScript = _scriptNames.TryGetValue("US", out Dictionary<CharacterId, String> script);
            if (hasScript && DefaultNamesByLang.TryGetValue("US", out Dictionary<CharacterId, String> patched) && script.Count != patched.Count)
            {
                // Update script names with DataPatchers names, but only for non-vanilla characters
                foreach (var kvp in patched)
                    if (!script.ContainsKey(kvp.Key))
                        script.Add(kvp.Key, kvp.Value);
            }
            return hasScript ? script : new Dictionary<CharacterId, String>();
        }

        public static Dictionary<String, Dictionary<CharacterId, String>> DefaultNamesByLang = new Dictionary<String, Dictionary<CharacterId, String>>()
        {
            { "US", new Dictionary<CharacterId, String>
            {
                { CharacterId.Zidane, "Zidane" },
                { CharacterId.Vivi, "Vivi" },
                { CharacterId.Garnet, "Dagger" },
                { CharacterId.Steiner, "Steiner" },
                { CharacterId.Freya, "Freya" },
                { CharacterId.Quina, "Quina" },
                { CharacterId.Eiko, "Eiko" },
                { CharacterId.Amarant, "Amarant" },
                { CharacterId.Cinna, "Cinna" },
                { CharacterId.Marcus, "Marcus" },
                { CharacterId.Blank, "Blank" },
                { CharacterId.Beatrix, "Beatrix" }
            } },
            { "UK", new Dictionary<CharacterId, String>
            {
                { CharacterId.Zidane, "Zidane" },
                { CharacterId.Vivi, "Vivi" },
                { CharacterId.Garnet, "Dagger" },
                { CharacterId.Steiner, "Steiner" },
                { CharacterId.Freya, "Freya" },
                { CharacterId.Quina, "Quina" },
                { CharacterId.Eiko, "Eiko" },
                { CharacterId.Amarant, "Amarant" },
                { CharacterId.Cinna, "Cinna" },
                { CharacterId.Marcus, "Marcus" },
                { CharacterId.Blank, "Blank" },
                { CharacterId.Beatrix, "Beatrix" }
            } },
            { "JP", new Dictionary<CharacterId, String>
            {
                { CharacterId.Zidane, "ジタン" },
                { CharacterId.Vivi, "ビビ" },
                { CharacterId.Garnet, "ダガー" },
                { CharacterId.Steiner, "スタイナー" },
                { CharacterId.Freya, "フライヤ" },
                { CharacterId.Quina, "クイナ" },
                { CharacterId.Eiko, "エーコ" },
                { CharacterId.Amarant, "サラマンダー" },
                { CharacterId.Cinna, "シナ" },
                { CharacterId.Marcus, "マーカス" },
                { CharacterId.Blank, "ブランク" },
                { CharacterId.Beatrix, "ベアトリクス" }
            } },
            { "GR", new Dictionary<CharacterId, String>
            {
                { CharacterId.Zidane, "Zidane" },
                { CharacterId.Vivi, "Vivi" },
                { CharacterId.Garnet, "Lili" },
                { CharacterId.Steiner, "Steiner" },
                { CharacterId.Freya, "Freya" },
                { CharacterId.Quina, "Quina" },
                { CharacterId.Eiko, "Eiko" },
                { CharacterId.Amarant, "Mahagon" },
                { CharacterId.Cinna, "Cinna" },
                { CharacterId.Marcus, "Marcus" },
                { CharacterId.Blank, "Blank" },
                { CharacterId.Beatrix, "Beatrix" }
            } },
            { "FR", new Dictionary<CharacterId, String>
            {
                { CharacterId.Zidane, "Djidane" },
                { CharacterId.Vivi, "Bibi" },
                { CharacterId.Garnet, "Dagga" },
                { CharacterId.Steiner, "Steiner" },
                { CharacterId.Freya, "Freyja" },
                { CharacterId.Quina, "Kweena" },
                { CharacterId.Eiko, "Eiko" },
                { CharacterId.Amarant, "Tarask" },
                { CharacterId.Cinna, "Cina" },
                { CharacterId.Marcus, "Markus" },
                { CharacterId.Blank, "Frank" },
                { CharacterId.Beatrix, "Beate" }
            } },
            { "IT", new Dictionary<CharacterId, String>
            {
                { CharacterId.Zidane, "Gidan" },
                { CharacterId.Vivi, "Vivi" },
                { CharacterId.Garnet, "Daga" },
                { CharacterId.Steiner, "Steiner" },
                { CharacterId.Freya, "Freija" },
                { CharacterId.Quina, "Quina" },
                { CharacterId.Eiko, "Eiko" },
                { CharacterId.Amarant, "Amarant" },
                { CharacterId.Cinna, "Er Cina" },
                { CharacterId.Marcus, "Marcus" },
                { CharacterId.Blank, "Blank" },
                { CharacterId.Beatrix, "Beatrix" }
            } },
            { "ES", new Dictionary<CharacterId, String>
            {
                { CharacterId.Zidane, "Yitán" },
                { CharacterId.Vivi, "Vivi" },
                { CharacterId.Garnet, "Daga" },
                { CharacterId.Steiner, "Steiner" },
                { CharacterId.Freya, "Freija" },
                { CharacterId.Quina, "Quina" },
                { CharacterId.Eiko, "Eiko" },
                { CharacterId.Amarant, "Amarant" },
                { CharacterId.Cinna, "Cinna" },
                { CharacterId.Marcus, "Marcus" },
                { CharacterId.Blank, "Blank" },
                { CharacterId.Beatrix, "Beatrix" }
            } }
        };

        private static Dictionary<String, Dictionary<CharacterId, String>> _scriptNames = new Dictionary<String, Dictionary<CharacterId, String>>(DefaultNamesByLang);

        public static TxtEntry[] Build(String prefix, Dictionary<CharacterId, String> characterNames)
        {
            TxtEntry[] names = new TxtEntry[characterNames.Count];
            Int32 index = 0;
            foreach (KeyValuePair<CharacterId, String> pair in characterNames)
                names[index++] = new TxtEntry { Index = (Int32)pair.Key, Prefix = prefix, Value = pair.Value };
            return names;
        }

        public static String[] Parse(TxtEntry[] entreis)
        {
            String[] result = new String[entreis.Length];
            for (Int32 i = 0; i < result.Length; i++)
                result[i] = entreis[i].Value;
            return result;
        }
    }
}
