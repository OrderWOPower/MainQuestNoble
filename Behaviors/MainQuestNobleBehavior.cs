using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MainQuestNoble.Behaviors
{
    public class MainQuestNobleBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents() { }
        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("_trackedParty", ref _trackedParty);
                dataStore.SyncData("_trackedArmy", ref _trackedArmy);
                dataStore.SyncData("_trackedNoble", ref _trackedNoble);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("Exception at MainQuestNobleBehavior.SyncData(): " + ex.Message));
            }
        }
        public static MobileParty TrackedParty { get => _trackedParty; set => _trackedParty = value; }
        public static Army TrackedArmy { get => _trackedArmy; set => _trackedArmy = value; }
        public static Hero TrackedNoble { get => _trackedNoble; set => _trackedNoble = value; }
        private static MobileParty _trackedParty;
        private static Army _trackedArmy;
        private static Hero _trackedNoble;
    }
}
