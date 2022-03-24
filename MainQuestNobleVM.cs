using HarmonyLib;
using SandBox.ViewModelCollection.MobilePartyTracker;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace MainQuestNoble
{
    [HarmonyPatch(typeof(MobilePartyTrackerVM), MethodType.Constructor, new Type[] { typeof(Camera), typeof(Action<Vec2>) })]
    public class MainQuestNobleVM
    {
        private readonly string _nobleName;
        private static MobilePartyTrackerVM _mobilePartyTrackerVM;
        private static Camera _mapCamera;
        private static Action<Vec2> _fastMoveCameraToPosition;
        private bool _talkedToAnyNoble;
        private bool _talkedToQuestNoble;

        public static MobileParty PartyToTrack { get; set; }

        public static Army ArmyToTrack { get; set; }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(MobilePartyTrackerVM __instance, Camera ____mapCamera, Action<Vec2> ____fastMoveCameraToPosition)
        {
            _mobilePartyTrackerVM = __instance;
            _mapCamera = ____mapCamera;
            _fastMoveCameraToPosition = ____fastMoveCameraToPosition;
        }

        public MainQuestNobleVM(MobileParty partyToTrack, Army armyToTrack, string nobleName, bool talkedToAnyNoble, bool talkedToQuestNoble)
        {
            PartyToTrack = partyToTrack;
            ArmyToTrack = armyToTrack;
            _nobleName = nobleName;
            _talkedToAnyNoble = talkedToAnyNoble;
            _talkedToQuestNoble = talkedToQuestNoble;
            Init();
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(OnConversationEnded));
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, new Action<MobileParty, PartyBase>(OnPartyDestroyed));
            CampaignEvents.OnPartyDisbandedEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(OnPartyDisbanded));
            CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, new Action<Army, Army.ArmyDispersionReason, bool>(OnArmyDispersed));
        }

        // If a quest noble can be tracked, start tracking the quest noble after talking to any non-quest noble. If not, do nothing.
        // Stop tracking the quest noble after talking to any quest noble.
        private void OnConversationEnded(CharacterObject character)
        {
            if (_talkedToAnyNoble)
            {
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                InformationManager.DisplayMessage(new InformationMessage(PartyToTrack != null ? "Started tracking position of " + _nobleName + "!" : "Failed to track position of " + _nobleName + "!"));
                _talkedToAnyNoble = false;
            }
            else if (_talkedToQuestNoble)
            {
                InformationManager.DisplayMessage(new InformationMessage("Stopped tracking positions of main quest nobles!"));
                _talkedToQuestNoble = false;
            }
        }

        private void OnPartyDestroyed(MobileParty mobileParty, PartyBase arg2)
        {
            if (mobileParty == PartyToTrack)
            {
                PartyToTrack = null;
                Init();
            }
        }

        private void OnPartyDisbanded(MobileParty disbandedParty, Settlement relatedSettlement)
        {
            if (disbandedParty == PartyToTrack)
            {
                PartyToTrack = null;
                Init();
            }
        }

        private void OnArmyDispersed(Army army, Army.ArmyDispersionReason arg2, bool arg3)
        {
            if (army == ArmyToTrack)
            {
                ArmyToTrack = null;
                Init();
            }
        }

        private void Init() => RemoveAndAdd(PartyToTrack, ArmyToTrack);

        private void RemoveAndAdd(MobileParty party, Army army)
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
                if (army == null || (army != null && !army.LeaderPartyAndAttachedParties.Contains(party)))
                {
                    _mobilePartyTrackerVM.Trackers.Add(new MobilePartyTrackItemVM(party, _mapCamera, _fastMoveCameraToPosition));
                }
                else
                {
                    _mobilePartyTrackerVM.Trackers.Add(new MobilePartyTrackItemVM(army, _mapCamera, _fastMoveCameraToPosition));
                }
            }
        }
    }
}
