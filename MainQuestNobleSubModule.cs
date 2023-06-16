using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MainQuestNoble
{
    // This mod tracks the positions of the nobles to talk to for the main quest "Investigate Neretzes' Folly".
    public class MainQuestNobleSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad() => new Harmony("mod.bannerlord.mainquestnoble").PatchAll();

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                ((CampaignGameStarter)gameStarterObject).AddBehavior(new MainQuestNobleBehavior());
            }
        }
    }
}
