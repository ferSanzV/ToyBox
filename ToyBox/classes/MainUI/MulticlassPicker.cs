﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT
using UnityEngine;
using UnityModManagerNet;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Rest;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using ModKit;

namespace ToyBox {
    public class MulticlassPicker {
        public static Settings settings { get { return Main.settings; } }

        public static void OnGUI(MulticlassOptions options, float indent = 100) {
            var classes = Game.Instance.BlueprintRoot.Progression.CharacterClasses;
            var mythicClasses = Game.Instance.BlueprintRoot.Progression.CharacterMythics;
            bool showDesc = settings.toggleMulticlassShowClassDescriptions;
            foreach (var cl in classes) {
                PickerRow(cl, options, indent);
            }
            UI.Div(indent);
            if (showDesc) {
                using (UI.HorizontalScope()) {
                    UI.Space(indent); UI.Label("Mythic".cyan());
                }
            }
            foreach (var mycl in mythicClasses) {
                PickerRow(mycl, options, indent);
            }
        }

        public static bool PickerRow(BlueprintCharacterClass cl, MulticlassOptions options, float indent = 100) {
            bool changed = false;
            bool showDesc = settings.toggleMulticlassShowClassDescriptions;
            if (showDesc) UI.Div(indent);
            using (UI.HorizontalScope()) {
                UI.Space(indent);
                UI.ActionToggle(
                    cl.Name,
                    () => options.Contains(cl),
                    (v) => {
                        if (v) options.Add(cl);
                        else options.Remove(cl);
                        Main.Debug($"PickerRow - multiclassOptions - class: {cl.HashKey()} - {options}>");
                        changed = true;
                    },
                    350
                    );
                if (showDesc) UI.Label(cl.Description.RemoveHtmlTags().green());
            }
            using (UI.HorizontalScope()) {
                UI.Space(indent);
                var archetypes = cl.Archetypes;
                if (options.Contains(cl) && archetypes.Any()) {
                    UI.Space(50);
                    using (UI.VerticalScope()) {
                        var archetypeOptions = options.ArchetypeOptions(cl);
                        foreach (var archetype in cl.Archetypes) {
                            if (showDesc) UI.Div();
                            using (UI.HorizontalScope()) {
                                UI.ActionToggle(
                                archetype.Name,
                                () => archetypeOptions.Contains(archetype),
                                (v) => {
                                    if (v) archetypeOptions.AddExclusive(archetype);
                                    else archetypeOptions.Remove(archetype);
                                    Main.Log($"PickerRow - archetypeOptions - {{{archetypeOptions}}}");
                                },
                                350
                                );
                                options.SetArchetypeOptions(cl, archetypeOptions);
                                if (showDesc) UI.Label(archetype.Description.RemoveHtmlTags().green());
                            }
                        }
                    }
                }
            }
            return changed;
        }
    }
}
#if false
                    int originalArchetype = 0;
                    int selectedArchetype = originalArchetype = archetypes.FindIndex(archetype => options.Contains(archetype)) + 1;
                    var choices = new String[] { cl.Name }.Concat(archetypes.Select(a => a.Name)).ToArray();
                    UI.ActionSelectionGrid(ref selectedArchetype, choices, 6, (sel) => {
                        if (originalArchetype > 0)
                            multiclassSet.Remove(archetypes[originalArchetype - 1].AssetGuid.ToString());
                        if (selectedArchetype > 0)
                            multiclassSet.Add(archetypes[selectedArchetype - 1].AssetGuid.ToString());
                        Main.Log($"multiclassSet - archetype - <{String.Join(", ", multiclassSet)}>");
                    }, UI.AutoWidth());
#endif
