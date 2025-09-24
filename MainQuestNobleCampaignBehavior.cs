using HarmonyLib;
using Helpers;
using SandBox.ViewModelCollection.Map.Tracker;
using StoryMode.Quests.FirstPhase;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MainQuestNoble
{
    [HarmonyPatch]
    public class MainQuestNobleCampaignBehavior : CampaignBehaviorBase
    {
        private static MapTrackerProvider _mapTrackerProvider;
        private static object _trackerContainer;
        private static MobileParty _partyToTrack;
        private static Army _armyToTrack;
        private static string _nobleName;
        private static bool _hasTalkedToAnyNoble, _hasTalkedToQuestNoble;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BannerInvestigationQuest), "talk_with_any_noble_continue_condition")]
        private static void Postfix1(bool ____allNoblesDead, Dictionary<Hero, bool> ____noblesToTalk)
        {
            TextObject textObject;

            if (!____allNoblesDead)
            {
                KeyValuePair<Hero, bool> keyValuePair;

                textObject = new TextObject("{HERO.LINK}", null);

                if (____noblesToTalk.Any(n => !n.Value && n.Key.IsAlive && n.Key.Culture == Hero.OneToOneConversationHero.Culture))
                {
                    keyValuePair = ____noblesToTalk.First(n => !n.Value && n.Key.IsAlive && n.Key.Culture == Hero.OneToOneConversationHero.Culture);
                }
                else
                {
                    keyValuePair = ____noblesToTalk.First(n => !n.Value && n.Key.IsAlive);
                }

                StringHelpers.SetCharacterProperties("HERO", keyValuePair.Key.CharacterObject, textObject, false);

                // Set the party/army to track.
                _partyToTrack = keyValuePair.Key.CharacterObject.HeroObject.PartyBelongedTo;
                _armyToTrack = _partyToTrack?.Army;
                // Set the noble's name to display in the debug message.
                _nobleName = textObject.ToString();
                _hasTalkedToAnyNoble = true;
                _hasTalkedToQuestNoble = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BannerInvestigationQuest), "talk_with_quest_noble_consequence")]
        private static void Postfix2()
        {
            _partyToTrack = null;
            _armyToTrack = null;
            _nobleName = null;
            _hasTalkedToAnyNoble = false;
            _hasTalkedToQuestNoble = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapTrackerProvider), MethodType.Constructor)]
        public static void Postfix3(MapTrackerProvider __instance, object ____trackerContainer)
        {
            _mapTrackerProvider = __instance;
            _trackerContainer = ____trackerContainer;

            RemoveAndAdd(_partyToTrack, _armyToTrack);
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
                InformationManager.DisplayMessage(new InformationMessage(ex.ToString()));
            }
        }

        private void OnConversationEnded(IEnumerable<CharacterObject> character)
        {
            if (_hasTalkedToAnyNoble)
            {
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                // If a quest noble can be tracked, start tracking the quest noble after talking to any non-quest noble. If not, do nothing.
                InformationManager.DisplayMessage(new InformationMessage((_partyToTrack != null ? "Started tracking position of " : "Failed to track position of ") + _nobleName + "!"));

                _hasTalkedToAnyNoble = false;
            }
            else if (_hasTalkedToQuestNoble)
            {
                // Stop tracking the quest noble after talking to any quest noble.
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));

                _hasTalkedToQuestNoble = false;
            }

            RemoveAndAdd(_partyToTrack, _armyToTrack);
        }

        private static void RemoveAndAdd(MobileParty party, Army army)
        {
            AccessTools.Method(typeof(MapTrackerProvider), "ResetTrackers").Invoke(_mapTrackerProvider, null);

            if (party != null)
            {
                Type typeofTrackerContainer = AccessTools.TypeByName("TrackerContainer");

                if (army == null || !army.DoesLeaderPartyAndAttachedPartiesContain(party))
                {
                    if (!(bool)AccessTools.Method(typeofTrackerContainer, "HasTrackerFor").Invoke(_trackerContainer, new object[] { party }))
                    {
                        AccessTools.Method(typeofTrackerContainer, "AddTracker").Invoke(_trackerContainer, new object[] { new MapMobilePartyTrackItemVM(party) });
                    }
                }
                else
                {
                    if (!(bool)AccessTools.Method(typeofTrackerContainer, "HasTrackerFor").Invoke(_trackerContainer, new object[] { army }))
                    {
                        AccessTools.Method(typeofTrackerContainer, "AddTracker").Invoke(_trackerContainer, new object[] { new MapArmyTrackItemVM(army) });
                    }
                }
            }
        }
    }
}
