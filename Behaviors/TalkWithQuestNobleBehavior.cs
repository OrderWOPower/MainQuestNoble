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
            _ = new MainQuestNobleTrackerVM(null, null, null, null, false, true);
            _ = new MainQuestNobleNameplateVM(null);
        }
    }
}
