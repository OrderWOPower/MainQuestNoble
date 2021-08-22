using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using SandBox.ViewModelCollection.Nameplate;

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
                SettlementEventsVM settlementEventsVM = __instance.SettlementEvents;
                SettlementNameplateEventItemVM settlementNameplateEventItemVM = settlementEventsVM.EventsList.FirstOrDefault((SettlementNameplateEventItemVM e) => e.EventType == SettlementNameplateEventItemVM.SettlementEventType.AvailableQuest);
                if (__instance.Settlement == NobleToTrack?.CurrentSettlement && !NobleToTrack.IsPrisoner)
                {
                    if (!settlementEventsVM.EventsList.Contains(settlementNameplateEventItemVM))
                    {
                        settlementEventsVM.EventsList.Add(new SettlementNameplateEventItemVM(SettlementNameplateEventItemVM.SettlementEventType.AvailableQuest));
                    }
                }
                else
                {
                    settlementEventsVM.EventsList.Remove(settlementNameplateEventItemVM);
                }
            }
        }
        public MainQuestNobleNameplateVM(Hero nobleToTrack) => NobleToTrack = nobleToTrack;
        public static Hero NobleToTrack { get; set; }
    }
}
