﻿using SandBox.ViewModelCollection.Map;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace MainQuestNoble
{
    public class MainQuestNobleVM : ViewModel
    {
        private readonly MapMobilePartyTrackerVM _mapMobilePartyTrackerVM;
        private readonly Camera _mapCamera;
        private readonly Action<Vec2> _fastMoveCameraToPosition;

        private MobileParty _partyToTrack;
        private Army _armyToTrack;

        public MobileParty PartyToTrack
        {
            get => _partyToTrack;

            set
            {
                if (value != _partyToTrack)
                {
                    _partyToTrack = value;

                    OnPropertyChangedWithValue(value, "PartyToTrack");
                }
            }
        }

        public Army ArmyToTrack
        {
            get => _armyToTrack;

            set
            {
                if (value != _armyToTrack)
                {
                    _armyToTrack = value;

                    OnPropertyChangedWithValue(value, "ArmyToTrack");
                }
            }
        }

        public MainQuestNobleVM(MapMobilePartyTrackerVM mapMobilePartyTrackerVM, Camera mapCamera, Action<Vec2> fastMoveCameraToPosition)
        {
            _mapMobilePartyTrackerVM = mapMobilePartyTrackerVM;
            _mapCamera = mapCamera;
            _fastMoveCameraToPosition = fastMoveCameraToPosition;

            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, new Action<MobileParty, PartyBase>(OnPartyDestroyed));
            CampaignEvents.OnPartyDisbandedEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(OnPartyDisbanded));
            CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, new Action<Army, Army.ArmyDispersionReason, bool>(OnArmyDispersed));
        }

        public void SetPartyAndArmyToTrack(MobileParty partyToTrack, Army armyToTrack)
        {
            PartyToTrack = partyToTrack;
            ArmyToTrack = armyToTrack;

            Init();
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
            MBBindingList<MobilePartyTrackItemVM> trackers = _mapMobilePartyTrackerVM.Trackers;
            Clan playerClan = Clan.PlayerClan;

            trackers.Remove(trackers.FirstOrDefault(t => (!playerClan.WarPartyComponents.Contains(t.TrackedParty?.WarPartyComponent)) && (playerClan.Kingdom == null || !playerClan.Kingdom.Armies.Contains(t.TrackedArmy))));

            if (party != null)
            {
                if (army == null || !army.DoesLeaderPartyAndAttachedPartiesContain(party))
                {
                    trackers.Add(new MobilePartyTrackItemVM(party, _mapCamera, _fastMoveCameraToPosition));
                }
                else
                {
                    trackers.Add(new MobilePartyTrackItemVM(army, _mapCamera, _fastMoveCameraToPosition));
                }
            }
        }
    }
}
