using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using MainQuestNoble.ViewModels;

namespace MainQuestNoble.Behaviors
{
    public class MainQuestNobleBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameLoaded));
            CampaignEvents.TickEvent.AddNonSerializedListener(this, new Action<float>(OnTick));
        }
        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("_partyToTrack", ref _partyToTrack);
                dataStore.SyncData("_armyToTrack", ref _armyToTrack);
                dataStore.SyncData("_nobleToTrack", ref _nobleToTrack);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("Exception at MainQuestNobleBehavior.SyncData(): " + ex.Message));
            }
        }
        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            _ = new MainQuestNobleTrackerVM(_partyToTrack, _armyToTrack, null, null, false, false);
            _ = new MainQuestNobleNameplateVM(_nobleToTrack);
        }
        public void OnTick(float dt)
        {
            _partyToTrack = MainQuestNobleTrackerVM.PartyToTrack;
            _armyToTrack = MainQuestNobleTrackerVM.ArmyToTrack;
            _nobleToTrack = MainQuestNobleNameplateVM.NobleToTrack;
        }
        private MobileParty _partyToTrack;
        private Army _armyToTrack;
        private Hero _nobleToTrack;
    }
}
