﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Craft;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System.Runtime.CompilerServices;
using ModKit;
using Kingmaker.Blueprints.Items.Weapons;
using HarmonyLib;

namespace ToyBox {

    public static partial class BlueprintExensions {
        public static Settings settings => Main.settings;

        private static readonly ConditionalWeakTable<object, List<string>> cachedCollationNames = new() { };
        private static void AddOrUpdateCachedNames(SimpleBlueprint bp, List<string> names) {
            if (cachedCollationNames.TryGetValue(bp, out _)) {
                cachedCollationNames.Remove(bp);
            }
            cachedCollationNames.Add(bp, names);
        }
        public static string GetDisplayName(this SimpleBlueprint bp) => bp.name;
        public static string GetDisplayName(this BlueprintSpellbook bp) {
            var name = bp.DisplayName;
            if (name == null || name.Length == 0) name = bp.name.Replace("Spellbook", "");
            return name;
        }
        public static IEnumerable<string> Attributes(this SimpleBlueprint bp) {
            List<string> modifers = new();
            var traverse = Traverse.Create(bp);
            foreach (var property in Traverse.Create(bp).Properties()) {
                if (property.StartsWith("Is")) {
                    try {
                        var value = traverse.Property<bool>(property)?.Value;
                        if (value.HasValue && value.GetValueOrDefault()) {
                            modifers.Add(property); //.Substring(2));
                        }
                    }
                    catch { }
                }
            }
            return modifers;
        }
        private static List<string> DefaultCollationNames(this SimpleBlueprint bp, params string[] extras) {
            cachedCollationNames.TryGetValue(bp, out var names);
            if (names != null) return names;
            names = extras?.ToList() ?? new List<string> {};
            var typeName = bp.GetType().Name.Replace("Blueprint", "");
            //var stripIndex = typeName.LastIndexOf("Blueprint");
            //if (stripIndex > 0) typeName = typeName.Substring(stripIndex + "Blueprint".Length);
            names.Add(typeName);
            if (settings.showAttributes) {
                foreach (var attribute in bp.Attributes()) 
                    names.Add(attribute.orange());
            }
            cachedCollationNames.Add(bp, names);
            return names;
        }
        public static List<string> CollationNames(this SimpleBlueprint bp, params string[] extras) => DefaultCollationNames(bp, extras);
        public static List<string> CollationNames(this BlueprintSpellbook bp, params string[] extras) {
            var names = DefaultCollationNames(bp, extras);
            if (bp.CharacterClass.IsDivineCaster) names.Add("Divine");
            AddOrUpdateCachedNames(bp, names);
            return names;
        }
        public static List<string> CollationNames(this BlueprintBuff bp, params string[] extras) {
            var names = DefaultCollationNames(bp, extras);
            if (bp.Harmful) names.Add("Harmful");
            if (bp.RemoveOnRest) names.Add("Rest Removes");
            if (bp.RemoveOnResurrect) names.Add("Res Removes");
            if (bp.Ranks > 0) names.Add($"{bp.Ranks} Ranks");

            AddOrUpdateCachedNames(bp, names);
            return names;
        }

        public static List<string> CollationNames(this BlueprintIngredient bp, params string[] extras) {
            var names = DefaultCollationNames(bp, extras);
            if (bp.Destructible) names.Add("Destructible");
            if (bp.FlavorText != null) names.Add(bp.FlavorText);
            AddOrUpdateCachedNames(bp, names);
            return names;
        }
        public static List<string> CollationNames(this BlueprintArea bp, params string[] extras) {
            var names = DefaultCollationNames(bp, extras);
            var typeName = bp.GetType().Name.Replace("Blueprint", "");
            if (typeName == "Area") names.Add($"Area CR{bp.CR}");
            AddOrUpdateCachedNames(bp, names);
            return names;
        }

        private static readonly Dictionary<Type, List<SimpleBlueprint>> blueprintsByType = new();
        public static List<SimpleBlueprint> BlueprintsOfType(Type type) {
            if (blueprintsByType.ContainsKey(type)) return blueprintsByType[type];
            var blueprints = BlueprintBrowser.GetBlueprints();
            if (blueprints == null) return new List<SimpleBlueprint>();
            var filtered = blueprints.Where((bp) => bp.GetType().IsKindOf(type)).ToList();
            blueprintsByType[type] = filtered;
            return filtered;
        }

        public static List<SimpleBlueprint> BlueprintsOfType<BPType>() where BPType : SimpleBlueprint {
            var type = typeof(BPType);
            if (blueprintsByType.ContainsKey(type)) return blueprintsByType[type];
            var blueprints = BlueprintBrowser.GetBlueprints();
            if (blueprints == null) return new List<SimpleBlueprint>();
            var filtered = blueprints.Where((bp) => (bp is BPType)).ToList();
            blueprintsByType[type] = filtered;
            return filtered;
        }

        public static List<SimpleBlueprint> GetBlueprints<T>() where T : SimpleBlueprint => BlueprintsOfType<T>();
        public static int GetSelectableFeaturesCount(this BlueprintFeatureSelection selection, UnitDescriptor unit) {
            var count = 0;
            var component = selection.GetComponent<NoSelectionIfAlreadyHasFeature>();
            if (component == null)
                return count;
            if (component.AnyFeatureFromSelection) {
                foreach (var allFeature in selection.AllFeatures) {
                    if (!unit.Progression.Features.HasFact((BlueprintFact)allFeature)) {
                        count++;
                    }
                }
            }
            foreach (var feature in component.Features) {
                if (!unit.Progression.Features.HasFact((BlueprintFact)feature)) {
                    count++;
                }
            }
            return count;
        }
    }
}