﻿using HarmonyLib;
using Helpers;
using StoryMode.Quests.FirstPhase;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace MainQuestNoble
{
    [HarmonyPatch(typeof(BannerInvestigationQuest))]
    public class BannerInvestigationQuestPatches
    {
        // Get the quest noble after talking to any non-quest noble.
        [HarmonyPatch("talk_with_any_noble_condition")]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> codesToInsert = new List<CodeInstruction>();
            int index = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand is "HERO" && (MethodInfo)codes[i + 3].operand == AccessTools.Method(typeof(Hero), "get_CharacterObject"))
                {
                    index = i + 4;
                }
            }
            codesToInsert.Add(new CodeInstruction(OpCodes.Dup));
            codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BannerInvestigationQuestPatches), "TrackNoble", new Type[] { typeof(CharacterObject) })));
            codes.InsertRange(index, codesToInsert);
            return codes;
        }

        [HarmonyPatch("talk_with_quest_noble_consequence")]
        private static void Postfix() => _ = new MainQuestNobleVM(null, null, null, false, true);

        // Set the party/army to track and the noble to track. Set the noble's name to display in the debug message.
        private static void TrackNoble(CharacterObject characterObject)
        {
            TextObject textObject = new TextObject("{HERO.LINK}", null);
            StringHelpers.SetCharacterProperties("HERO", characterObject, textObject);
            MobileParty party = characterObject.HeroObject.PartyBelongedTo;
            Army army = party?.Army;
            string text = textObject.ToString();
            _ = new MainQuestNobleVM(party, army, text, true, false);
        }
    }
}