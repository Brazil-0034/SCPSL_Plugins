using EXILED;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    public class Plugin : EXILED.Plugin
    {
        private EventHandlers EventHandlers;
        public override void OnEnable()
        {
            EventHandlers = new EventHandlers(plugin:this);

            Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
            Events.PlayerLeaveEvent += EventHandlers.OnPlayerLeave;
            Events.RoundStartEvent += EventHandlers.OnRoundStart;
            Events.PlayerDeathEvent += EventHandlers.OnPlayerDeath;
            Events.CancelMedicalItemEvent += EventHandlers.OnMedicalCancel;

        }

        public override void OnDisable()
        {
            Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
            Events.PlayerLeaveEvent -= EventHandlers.OnPlayerLeave;
            Events.RoundStartEvent -= EventHandlers.OnRoundStart;
            Events.PlayerDeathEvent -= EventHandlers.OnPlayerDeath;
            Events.CancelMedicalItemEvent -= EventHandlers.OnMedicalCancel;
            EventHandlers = null;
        }

        public override void OnReload()
        {

        }
        public override string getName { get; } = "TestPlugin";
    }
}
