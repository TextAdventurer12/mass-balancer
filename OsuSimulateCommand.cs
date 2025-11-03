// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace PerformanceCalculator.Simulate
{
    public class OsuSimulateCommand
    {
        public string Beatmap { get; }

        public int Combo { get; }

        public string[] Mods { get; }

        public int Misses { get; }

        public int Mehs { get; }

        public int Goods { get; }

        public Ruleset Ruleset => new OsuRuleset();

        public bool NoClassicMod => true;
        public OsuSimulateCommand(string Beatmap, int Combo, string[] Mods, int Misses, int Mehs, int Goods)
        {
            this.Beatmap = Beatmap;
            this.Combo = Combo;
            this.Mods = Mods;
            this.Misses = Misses;
            this.Mehs = Mehs;
            this.Goods = Goods;
        }

        protected int GetMaxCombo(IBeatmap beatmap) => beatmap.GetMaxCombo();

        protected Dictionary<HitResult, int> GenerateHitResults(IBeatmap beatmap, int countMiss, int countMeh, int countGood)
        {
            int countGreat;

            var totalResultCount = beatmap.HitObjects.Count;

            countGreat = totalResultCount - countGood - countMeh - countMiss;

            return new Dictionary<HitResult, int>
            {
                { HitResult.Great, countGreat },
                { HitResult.Ok, countGood  },
                { HitResult.Meh, countMeh  },
                { HitResult.Miss, countMiss }
            };
        }

        protected double GetAccuracy(Dictionary<HitResult, int> statistics)
        {
            var countGreat = statistics[HitResult.Great];
            var countGood = statistics[HitResult.Ok];
            var countMeh = statistics[HitResult.Meh];
            var countMiss = statistics[HitResult.Miss];
            var total = countGreat + countGood + countMeh + countMiss;

            return (double)((6 * countGreat) + (2 * countGood) + countMeh) / (6 * total);
        }
        protected Mod[] GetMods(Ruleset ruleset)
        {
            if (Mods == null)
                return Array.Empty<Mod>();

            var availableMods = ruleset.CreateAllMods().ToList();
            var mods = new List<Mod>();

            foreach (var modString in Mods)
            {
                Mod newMod = availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase));
                if (newMod == null)
                    throw new ArgumentException($"Invalid mod provided: {modString}");

                mods.Add(newMod);
            }

            return mods.ToArray();
        }
        public OsuPerformanceAttributes CalculatePerformance()
        {

            var workingBeatmap = ProcessorWorkingBeatmap.FromFileOrId(Beatmap);
            var mods = NoClassicMod ? GetMods(Ruleset) : LegacyHelper.ConvertToLegacyDifficultyAdjustmentMods(workingBeatmap.BeatmapInfo, Ruleset, GetMods(Ruleset));
            var beatmap = workingBeatmap.GetPlayableBeatmap(Ruleset.RulesetInfo, mods);

            var beatmapMaxCombo = GetMaxCombo(beatmap);
            var statistics = GenerateHitResults(beatmap, Misses, Mehs, Goods);
            var scoreInfo = new ScoreInfo(beatmap.BeatmapInfo, Ruleset.RulesetInfo)
            {
                Accuracy = GetAccuracy(statistics),
                MaxCombo = Combo == 0 ? beatmapMaxCombo : Combo,
                Statistics = statistics,
                Mods = mods,
            };

            var difficultyCalculator = Ruleset.CreateDifficultyCalculator(workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);
            var performanceCalculator = Ruleset.CreatePerformanceCalculator();
            return (OsuPerformanceAttributes)performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);
        }
    }
}
