using Scripts.Runtime.Core.Modules;

namespace Scripts.Runtime.Core.Stats
{
    public class GameStats
    {
        public int ItemsSpawned { get; private set; }
        public int ItemsGrabbed { get; private set; }
        public int ItemsThrown { get; private set; }
        public int ItemsDeleted { get; private set; }

        private readonly ModulesFacade _modulesFacade;

        public GameStats(ModulesFacade modulesFacade)
        {
            _modulesFacade = modulesFacade;
        }

        public void OnItemSpawned()
        {
            ItemsSpawned++;
            _modulesFacade.GetSteamModule().IncrementStat("items_spawned");
        }

        public void OnItemGrabbed()
        {
            ItemsGrabbed++;
            _modulesFacade.GetSteamModule().IncrementStat("items_grabbed");
        }

        public void OnItemThrown()
        {
            ItemsThrown++;
            _modulesFacade.GetSteamModule().IncrementStat("items_thrown");
        }

        public void OnItemDeleted()
        {
            ItemsDeleted++;
            _modulesFacade.GetSteamModule().IncrementStat("items_deleted");
        }
    }
}
