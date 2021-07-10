using System;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using SandBox.ViewModelCollection.MobilePartyTracker;
using MainQuestNoble.Behaviors;

namespace MainQuestNoble.ViewModels
{
    [HarmonyPatch(typeof(MobilePartyTrackerVM))]
    public class MainQuestNobleTrackerVM
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Camera), typeof(Action<Vec2>) })]
        public static void Postfix1(ref Camera ____mapCamera, ref Action<Vec2> ____fastMoveCameraToPosition, MobilePartyTrackerVM __instance)
        {
            _mapCamera = ____mapCamera;
            _fastMoveCameraToPosition = ____fastMoveCameraToPosition;
            _mobilePartyTracker = __instance;
            Init();
            CampaignEvents.ConversationEnded.AddNonSerializedListener(__instance, new Action<CharacterObject>(OnConversationEnded));
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
        private static void Init() => RemoveAndAdd(PartyToTrack, ArmyToTrack);
        // If a quest noble can be tracked, start tracking the quest noble after talking to any non-quest noble. If not, do nothing.
        // Stop tracking the quest noble after talking to any quest noble.
        private static void OnConversationEnded(CharacterObject character)
        {
            if (TalkedToAnyNoble)
            {
                if (PartyToTrack != null || ArmyToTrack != null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                    InformationManager.DisplayMessage(new InformationMessage(StartedTrackingText));
                    Init();
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage(FailedTrackingText));
                }
                TalkedToAnyNoble = false;
            }
            else if (TalkedToQuestNoble)
            {
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                Init();
                TalkedToQuestNoble = false;
            }
        }
        // Update the party/army to track.
        private static void RemoveAndAdd(MobileParty party, Army army)
        {
            for (int i = 0; i < _mobilePartyTracker.Trackers.Count; i++)
            {
                if (!Clan.PlayerClan.WarPartyComponents.Contains(_mobilePartyTracker.Trackers[i].TrackedParty?.WarPartyComponent))
                {
                    _mobilePartyTracker.Trackers.RemoveAt(i);
                }
            }
            MobilePartyTrackItemVM mobilePartyTrackItem = _mobilePartyTracker.Trackers.FirstOrDefault((MobilePartyTrackItemVM t) => t.TrackedArmy != null && Clan.PlayerClan.Kingdom == null || (Clan.PlayerClan.Kingdom != null && !Clan.PlayerClan.Kingdom.Armies.Contains(t.TrackedArmy)));
            _mobilePartyTracker.Trackers.Remove(mobilePartyTrackItem);
            if (party != null)
            {
                _mobilePartyTracker.Trackers.Add(new MobilePartyTrackItemVM(party, _mapCamera, _fastMoveCameraToPosition));
            }
            else if (army != null)
            {
                _mobilePartyTracker.Trackers.Add(new MobilePartyTrackItemVM(army, _mapCamera, _fastMoveCameraToPosition));
            }
        }
        public static string StartedTrackingText { get; set; }
        public static string FailedTrackingText { get; set; }
        public static bool TalkedToAnyNoble { get; set; }
        public static bool TalkedToQuestNoble { get; set; }
        public static MobileParty PartyToTrack { get => MainQuestNobleBehavior.TrackedParty; set => MainQuestNobleBehavior.TrackedParty = value; }
        public static Army ArmyToTrack { get => MainQuestNobleBehavior.TrackedArmy; set => MainQuestNobleBehavior.TrackedArmy = value; }
        private static Camera _mapCamera;
        private static Action<Vec2> _fastMoveCameraToPosition;
        private static MobilePartyTrackerVM _mobilePartyTracker;
    }
}
