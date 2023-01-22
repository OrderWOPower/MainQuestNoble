using HarmonyLib;
using Helpers;
using SandBox.ViewModelCollection.Map;
using StoryMode.Quests.FirstPhase;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MainQuestNoble
{
    [HarmonyPatch]
    public class MainQuestNobleBehavior : CampaignBehaviorBase
    {
        private static MobileParty _partyToTrack;
        private static Army _armyToTrack;
        private static string _nobleName;
        private static bool _hasTalkedToAnyNoble;
        private static bool _hasTalkedToQuestNoble;
        private static MainQuestNobleVM _mainQuestNobleVM;

        // Get the quest noble after talking to any non-quest noble.
        [HarmonyPatch(typeof(BannerInvestigationQuest), "talk_with_any_noble_continue_condition")]
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
            codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MainQuestNobleBehavior), "TrackNoble", new Type[] { typeof(CharacterObject) })));
            codes.InsertRange(index, codesToInsert);
            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BannerInvestigationQuest), "talk_with_quest_noble_consequence")]
        private static void Postfix1()
        {
            _partyToTrack = null;
            _armyToTrack = null;
            _nobleName = null;
            _hasTalkedToAnyNoble = false;
            _hasTalkedToQuestNoble = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapMobilePartyTrackerVM), MethodType.Constructor, new Type[] { typeof(Camera), typeof(Action<Vec2>) })]
        public static void Postfix2(MapMobilePartyTrackerVM __instance, Camera ____mapCamera, Action<Vec2> ____fastMoveCameraToPosition)
        {
            _mainQuestNobleVM = new MainQuestNobleVM(__instance, ____mapCamera, ____fastMoveCameraToPosition);
            _mainQuestNobleVM.PropertyChangedWithValue += OnViewModelPropertyChangedWithValue;
            _mainQuestNobleVM.SetPartyAndArmyToTrack(_partyToTrack, _armyToTrack);
        }

        // Set the party/army to track and the noble to track. Set the noble's name to display in the debug message.
        private static void TrackNoble(CharacterObject characterObject)
        {
            TextObject textObject = new TextObject("{HERO.LINK}", null);
            StringHelpers.SetCharacterProperties("HERO", characterObject, textObject);
            _partyToTrack = characterObject.HeroObject.PartyBelongedTo;
            _armyToTrack = _partyToTrack?.Army;
            _nobleName = textObject.ToString();
            _hasTalkedToAnyNoble = true;
            _hasTalkedToQuestNoble = false;
        }

        private static void OnViewModelPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == "PartyToTrack")
            {
                _partyToTrack = (MobileParty)e.Value;
            }
            else if (e.PropertyName == "ArmyToTrack")
            {
                _armyToTrack = (Army)e.Value;
            }
        }

        public override void RegisterEvents() => CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<IEnumerable<CharacterObject>>(OnConversationEnded));

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("_partyToTrack", ref _partyToTrack);
                dataStore.SyncData("_armyToTrack", ref _armyToTrack);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(ex.Message + "\r\n" + ex.StackTrace));
            }
        }

        // If a quest noble can be tracked, start tracking the quest noble after talking to any non-quest noble. If not, do nothing.
        // Stop tracking the quest noble after talking to any quest noble.
        private void OnConversationEnded(IEnumerable<CharacterObject> character)
        {
            if (_hasTalkedToAnyNoble)
            {
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                InformationManager.DisplayMessage(new InformationMessage((_partyToTrack != null ? "Started tracking position of " : "Failed to track position of ") + _nobleName + "!"));
                _hasTalkedToAnyNoble = false;
            }
            else if (_hasTalkedToQuestNoble)
            {
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                _hasTalkedToQuestNoble = false;
            }
            _mainQuestNobleVM.SetPartyAndArmyToTrack(_partyToTrack, _armyToTrack);
        }
    }
}
