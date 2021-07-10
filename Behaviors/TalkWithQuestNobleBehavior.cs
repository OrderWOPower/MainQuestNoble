using System.Reflection;
using HarmonyLib;
using StoryMode.Behaviors.Quests.FirstPhase;
using MainQuestNoble.ViewModels;


namespace MainQuestNoble.Behaviors
{
    [HarmonyPatch]
    public class TalkWithQuestNobleBehavior
    {
        public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.Inner(typeof(BannerInvestigationQuestBehavior), "BannerInvestigationQuest"), "talk_with_quest_noble_consequence");
        private static void Postfix()
        {
            MainQuestNobleTrackerVM.TalkedToQuestNoble = true;
            MainQuestNobleTrackerVM.PartyToTrack = null;
            MainQuestNobleTrackerVM.ArmyToTrack = null;
            MainQuestNobleNameplateVM.NobleToTrack = null;
        }
    }
}
