using EXILED;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED.Extensions;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;
using MEC;
using Mirror;
using Scp914;
using UnityEngine;

namespace TestPlugin
{
    public class EventHandlers
    {
        private int playerCount;
        private int infectedCount;
        private Plugin Plugin;
        private bool chopperArrived;
        private bool chosenFirstInfected;

        public EventHandlers(Plugin plugin) => Plugin = plugin;

        internal void OnPlayerJoin(PlayerJoinEvent ev)
        {
            ev.Player.Broadcast(time: 10, message: "\n<b>GAMEMODE: INFECTED</b>\nOne player will be infected. The rest must survive.", false);
            playerCount++;
            Log.Info("Player Count Logged As: " + playerCount + " (+1)");
        }

        //X MUST BE GREATER THAN 165 TO ESCAPE

        internal void OnMedicalCancel(MedicalItemEvent ev)
        {
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                Log.Info(Player.GetPosition(hub).ToString());
                GameObject g = GameObject.Instantiate(PlayerManager.players[0], hub.transform.position, Quaternion.identity);
                foreach(Component x in g.GetComponents(typeof(Component)))
                {
                    UnityEngine.Object.Destroy(x);
                }
                NetworkServer.Spawn(g);
                Timing.RunCoroutine(AutoRandomMove(g));
            }
        }

        public IEnumerator<float> AutoRandomMove(GameObject go)
        {
            Log.Info("COROUTINE STARTED");
            System.Random movementDecision = new System.Random();
            int x = movementDecision.Next(0, 2);
            if (x == 0)
            {
                for (int i = 0; i < 60; i++)
                {
                    go.transform.position = new Vector3(go.transform.position.x + 2, go.transform.position.y, go.transform.position.z);
                    yield return Timing.WaitForOneFrame;
                }
                Timing.RunCoroutine(AutoRandomMove(go));
            }
            if (x == 1)
            {
                for (int i = 0; i < 60; i++)
                {
                    go.transform.position = new Vector3(go.transform.position.x - 2, go.transform.position.y, go.transform.position.z);
                    yield return Timing.WaitForOneFrame;
                }
                Timing.RunCoroutine(AutoRandomMove(go));
            }
            Log.Info(x.ToString());
        }

        internal void OnPlayerDeath(ref PlayerDeathEvent ev)
        {
            ev.Player.characterClassManager.SetPlayersClass(RoleType.Scp0492, ev.Player.gameObject, true);
            infectedCount++;
            if (infectedCount == playerCount)
            {
                Log.Info("RESTART ROUND!");

                foreach (ReferenceHub hub in Player.GetHubs())
                {
                    Timing.RunCoroutine(RestartMessage(hub, true));
                }
            }
            else
            {
                Log.Info("Infected: " + infectedCount);
                Log.Info("Total: " + playerCount);
            }
        }

        public IEnumerator<float> RestartMessage(ReferenceHub hub, bool infectedWon)
        {
            if (infectedWon == true)
            {
                Player.Broadcast(hub, 15, "<b>THE <color=red>INFECTED</color> HAVE WON</b>\n<i>That Slaps</i>", false);
            }
            else
            {
                Player.Broadcast(hub, 15, "<b>THE <color=blue>SURVIVORS</color> HAVE WON</b>\n<i>That Slaps</i>", false);
            }
            yield return Timing.WaitForSeconds(15f);
            PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
        }

        internal void OnPlayerLeave(PlayerLeaveEvent ev)
        {
            playerCount--;
            Log.Info("Player Count Logged As: " + playerCount + " (-1)");
        }

        public IEnumerator<float> StartInfected(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(1f);
            if (chosenFirstInfected == false)
            {
                chosenFirstInfected = true;
                hub.characterClassManager.SetPlayersClass(RoleType.Scp0492, hub.gameObject, true);
                Player.Broadcast(hub, 9999, "<b>YOU ARE AN <color=red>INFECTED</color></b>\nKill the survivors to infect them.", false);
                hub.SetPosition(Map.GetRandomSpawnPoint(RoleType.NtfCommander));
            }
            else
            {
                hub.characterClassManager.SetPlayersClass(RoleType.FacilityGuard, hub.gameObject, true);
                hub.inventory.AddNewItem(ItemType.GunUSP, 0, 0, 0, 0);
                hub.gameObject.GetComponent<AmmoBox>().SetOneAmount(2, "250");
                Player.Broadcast(hub, 9999, "<b>YOU ARE A <color=blue>SURVIVOR</color></b>\nStay Alive until the Chopper arrives.", false);
                hub.SetPosition(Map.GetRandomSpawnPoint(RoleType.ChaosInsurgency));
            }
            RoundSummary.RoundLock = true;
        }

        public IEnumerator<float> StartChopperDrop()
        {
            int chopperArrivalTime = 90;
            yield return Timing.WaitForSeconds(0.9f * chopperArrivalTime);
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                if (hub.GetRole() == RoleType.FacilityGuard)
                {
                    Player.Broadcast(hub, 9999, "<b><color=blue>RESCUE CHOPPER</color> IS ALMOST HERE!</b>\nDefend the MTF Spawn until it arrives.", false);
                }
            }
            yield return Timing.WaitForSeconds(chopperArrivalTime + 2.5f);
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                if (hub.GetRole() == RoleType.FacilityGuard)
                {
                    Player.Broadcast(hub, 9999, "<b><color=blue>RESCUE CHOPPER</color> HAS ARRIVED!</b>\nGet to the MTF Spawn to escape!", false);
                }
                else
                {
                    Player.Broadcast(hub, 9999, "<b><color=blue>RESCUE CHOPPER</color> HAS ARRIVED!</b>\nStop the Survivors from Escaping!", false);
                }
            }
            ChopperAutostart ca = UnityEngine.Object.FindObjectOfType<ChopperAutostart>(); // Get the chopper (Thank to KadeDev's code)
            ca.SetState(true);
            yield return Timing.WaitForSeconds(5f);
            chopperArrived = true;
            while (chopperArrived == true)
            {
                foreach (ReferenceHub hub in Player.GetHubs())
                {
                    if (Player.GetPosition(hub).x > 170 && hub.GetRole() == RoleType.FacilityGuard)
                    {
                        Log.Info("A PLAYER HAS ESCAPED!");
                        Player.Broadcast(hub, 15, "<b>THE <color=blue>SURVIVORS</color> HAVE WON</b>\n<i>That Slaps</i>", false);
                        yield return Timing.WaitForSeconds(15f);
                        PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
                    }
                }
            }
        }

        internal void OnRoundStart()
        {
            chosenFirstInfected = false;
            foreach (ReferenceHub hub in Player.GetHubs())
            {
                Player.Broadcast(hub, 30, "\n<b>CUSTOM GAMEMODE: <color=red>INFECTED</color>", false);
                Timing.RunCoroutine(StartInfected(hub));
            }
            foreach (Lift lift in Lift.FindObjectsOfType<Lift>())
            {
                lift.Networklocked = true;
            }
            Timing.RunCoroutine(StartChopperDrop());
        }
    }
}
