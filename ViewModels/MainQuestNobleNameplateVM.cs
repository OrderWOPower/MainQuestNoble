using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using SandBox.ViewModelCollection.Nameplate;
using MainQuestNoble.Behaviors;

namespace MainQuestNoble.ViewModels
{
    [HarmonyPatch(typeof(SettlementNameplateVM), "RefreshDynamicProperties")]
    public class MainQuestNobleNameplateVM
    {
        // If the tracked noble is in a settlement and is not a prisoner, add a yellow exclamation mark icon to the settlement's nameplate.
        public static void Postfix(SettlementNameplateVM __instance)
        {
            if (__instance.IsInRange)
            {
                SettlementEventsVM settlementEvents = __instance.SettlementEvents;
                SettlementNameplateEventItemVM settlementNameplateEventItem = settlementEvents.EventsList.FirstOrDefault((SettlementNameplateEventItemVM e) => e.EventType == SettlementNameplateEventItemVM.SettlementEventType.AvailableQuest);
                if (__instance.Settlement == NobleToTrack?.CurrentSettlement && !NobleToTrack.IsPrisoner)
                {
                    if (!settlementEvents.EventsList.Contains(settlementNameplateEventItem))
                    {
                        settlementEvents.EventsList.Add(new SettlementNameplateEventItemVM(SettlementNameplateEventItemVM.SettlementEventType.AvailableQuest));
                    }
                }
                else
                {
                    settlementEvents.EventsList.Remove(settlementNameplateEventItem);
                }
            }
        }
        public static Hero NobleToTrack { get => MainQuestNobleBehavior.TrackedNoble; set => MainQuestNobleBehavior.TrackedNoble = value; }
    }
}
