using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Helpers;
using StoryMode.Behaviors.Quests.FirstPhase;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using MainQuestNoble.ViewModels;

namespace MainQuestNoble.Behaviors
{
    [HarmonyPatch]
    public class TalkWithAnyNobleBehavior
    {
        public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.Inner(typeof(BannerInvestigationQuestBehavior), "BannerInvestigationQuest"), "talk_with_any_noble_condition");
        // Get the quest noble after talking to any non-quest noble.
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
            codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TalkWithAnyNobleBehavior), "TrackNoble", new Type[] { typeof(CharacterObject) })));
            codes.InsertRange(index, codesToInsert);
            return codes;
        }
        // Set the party/army to track and the noble to track. Set the noble's name to display in the debug message.
        public static void TrackNoble(CharacterObject characterObject)
        {
            TextObject startedTrackingTextObject = new TextObject("Started tracking position of {HERO.LINK}!", null);
            TextObject failedTrackingTextObject = new TextObject("Failed to track position of {HERO.LINK}!", null);
            StringHelpers.SetCharacterProperties("HERO", characterObject, startedTrackingTextObject);
            StringHelpers.SetCharacterProperties("HERO", characterObject, failedTrackingTextObject);
            MobileParty partyToTrack = characterObject.HeroObject.PartyBelongedTo;
            Army armyToTrack = characterObject.HeroObject.PartyBelongedTo?.Army;
            string startedTrackingText = startedTrackingTextObject.ToString();
            string failedTrackingText = failedTrackingTextObject.ToString();
            _ = new MainQuestNobleTrackerVM(armyToTrack == null ? partyToTrack : null, armyToTrack ?? null, startedTrackingText, failedTrackingText, true, false);
            _ = new MainQuestNobleNameplateVM(characterObject.HeroObject);
        }
    }
}
