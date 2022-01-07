using HarmonyLib;
using SandBox.ViewModelCollection.MobilePartyTracker;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace MainQuestNoble
{
    [HarmonyPatch(typeof(MobilePartyTrackerVM), MethodType.Constructor, new Type[] { typeof(Camera), typeof(Action<Vec2>) })]
    public class MainQuestNobleBehavior : CampaignBehaviorBase
    {
        public static void Postfix() => _ = new MainQuestNobleVM(_partyToTrack, _armyToTrack, null, false, false);
        public override void RegisterEvents()
        {
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, new Action<CharacterObject>(OnConversationEnded));
            CampaignEvents.TickEvent.AddNonSerializedListener(this, new Action<float>(OnTick));
        }
        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("_partyToTrack", ref _partyToTrack);
                dataStore.SyncData("_armyToTrack", ref _armyToTrack);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("Exception at MainQuestNobleBehavior.SyncData(): " + ex.Message));
            }
        }
        public void OnConversationEnded(CharacterObject character) => Update();
        public void OnTick(float dt) => Update();
        public void Update()
        {
            _partyToTrack = MainQuestNobleVM.PartyToTrack;
            _armyToTrack = MainQuestNobleVM.ArmyToTrack;
        }
        private static MobileParty _partyToTrack;
        private static Army _armyToTrack;
    }
}
