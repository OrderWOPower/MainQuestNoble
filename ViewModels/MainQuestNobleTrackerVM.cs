using System;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using SandBox.ViewModelCollection.MobilePartyTracker;

namespace MainQuestNoble.ViewModels
{
    [HarmonyPatch(typeof(MobilePartyTrackerVM))]
    public class MainQuestNobleTrackerVM
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Camera), typeof(Action<Vec2>) })]
        public static void Postfix1(Camera ____mapCamera, Action<Vec2> ____fastMoveCameraToPosition, MobilePartyTrackerVM __instance)
        {
            _mapCamera = ____mapCamera;
            _fastMoveCameraToPosition = ____fastMoveCameraToPosition;
            _mobilePartyTrackerVM = __instance;
            Init();
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnPartyDestroyed")]
        public static void Postfix2(MobileParty mobileParty)
        {
            if (mobileParty == PartyToTrack)
            {
                PartyToTrack = null;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnPartyDisbanded")]
        public static void Postfix3(MobileParty disbandedParty)
        {
            if (disbandedParty == PartyToTrack)
            {
                PartyToTrack = null;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnArmyDispersed")]
        public static void Postfix4(Army army)
        {
            if (army == ArmyToTrack)
            {
                ArmyToTrack = null;
                Init();
            }
        }
        public MainQuestNobleTrackerVM(MobileParty partyToTrack, Army armyToTrack, string startedTrackingText, string failedTrackingText, bool talkedToAnyNoble, bool talkedToQuestNoble)
        {
            PartyToTrack = partyToTrack;
            ArmyToTrack = armyToTrack;
            _startedTrackingText = startedTrackingText;
            _failedTrackingText = failedTrackingText;
            _talkedToAnyNoble = talkedToAnyNoble;
            _talkedToQuestNoble = talkedToQuestNoble;
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(OnConversationEnded));
        }
        // If a quest noble can be tracked, start tracking the quest noble after talking to any non-quest noble. If not, do nothing.
        // Stop tracking the quest noble after talking to any quest noble.
        public void OnConversationEnded(CharacterObject character)
        {
            if (_talkedToAnyNoble)
            {
                if (PartyToTrack != null || ArmyToTrack != null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                    InformationManager.DisplayMessage(new InformationMessage(_startedTrackingText));
                    Init();
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage(_failedTrackingText));
                }
                _talkedToAnyNoble = false;
            }
            else if (_talkedToQuestNoble)
            {
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                Init();
                _talkedToQuestNoble = false;
            }
        }
        private static void Init() => RemoveAndAdd(PartyToTrack, ArmyToTrack);
        // Update the party/army to track.
        private static void RemoveAndAdd(MobileParty party, Army army)
        {
            for (int i = 0; i < _mobilePartyTrackerVM.Trackers.Count; i++)
            {
                if (!Clan.PlayerClan.WarPartyComponents.Contains(_mobilePartyTrackerVM.Trackers[i].TrackedParty?.WarPartyComponent))
                {
                    _mobilePartyTrackerVM.Trackers.RemoveAt(i);
                }
            }
            MobilePartyTrackItemVM mobilePartyTrackItemVM = _mobilePartyTrackerVM.Trackers.FirstOrDefault((MobilePartyTrackItemVM t) => t.TrackedArmy != null && Clan.PlayerClan.Kingdom == null || (Clan.PlayerClan.Kingdom != null && !Clan.PlayerClan.Kingdom.Armies.Contains(t.TrackedArmy)));
            _mobilePartyTrackerVM.Trackers.Remove(mobilePartyTrackItemVM);
            if (party != null)
            {
                _mobilePartyTrackerVM.Trackers.Add(new MobilePartyTrackItemVM(party, _mapCamera, _fastMoveCameraToPosition));
            }
            else if (army != null)
            {
                _mobilePartyTrackerVM.Trackers.Add(new MobilePartyTrackItemVM(army, _mapCamera, _fastMoveCameraToPosition));
            }
        }
        public static MobileParty PartyToTrack { get; set; }
        public static Army ArmyToTrack { get; set; }
        private static Camera _mapCamera;
        private static Action<Vec2> _fastMoveCameraToPosition;
        private static MobilePartyTrackerVM _mobilePartyTrackerVM;
        private string _startedTrackingText;
        private string _failedTrackingText;
        private bool _talkedToAnyNoble;
        private bool _talkedToQuestNoble;
    }
}
