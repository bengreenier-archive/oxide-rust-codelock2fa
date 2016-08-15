using System;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Plugins;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("oxide-codelock-2fa", "bengreenier", "0.1.0")]
    [Description("Make codelocks use actual 2fa")]
    public class Codelock2FA : CovalencePlugin
    {
        /// <summary>
        /// acl hash for all codelocked doors on the server
        /// </summary>
        private Dictionary<string, List<string>> acl = new Dictionary<string, List<string>>();

        /// <summary>
        /// a list doorId+requestorId strings for all the doors with pending 2FA requests
        /// </summary>
        private List<string> pending2FA = new List<string>();

        /// <summary>
        /// Loaded hook - called when plugin has loaded
        /// </summary>
        void Loaded()
        {
            Puts("Hi! Track issues @ github.com/bengreenier/oxide-rust-codelock2fa/issues. Enjoy!");
        }

        /// <summary>
        /// Can use door hook - called when someone tries to use a door with a codelock
        /// </summary>
        /// <param name="player">the player who tried to use the door</param>
        /// <param name="door">the door in question</param>
        /// <returns>can we open the door</returns>
        bool CanUseDoor(BasePlayer player, CodeLock lck)
        {
            // reflect to get the private code
            var codeField = typeof(CodeLock).GetField("code",
                BindingFlags.NonPublic | 
                BindingFlags.Instance);

            var codeValue = (string)codeField.GetValue(lck);
            
            // if it's not locked or the code isn't all 0s there's nothing for us to do
            if (!lck.IsLocked() || codeValue != "0000")
            {
                return true;
            }

            var door = (Door)lck.GetParentEntity();
            var doorId = GetDoorId(door);
            var doorOwnerId = Convert.ToString(door.OwnerID);
            var playerId = Convert.ToString(player.userID);

            if (!acl.ContainsKey(doorId))
            {
                acl[doorId] = new List<string>();
            }

            if (acl[doorId].IndexOf(doorOwnerId) > -1)
            {
                return true;
            }
            else
            {
                if (pending2FA.IndexOf(doorId + playerId) ==  -1)
                {
                    pending2FA.Add(doorId + playerId);
                    GetRequest(doorOwnerId, doorId, playerId);
                }
                return false;
            }
        }

        /// <summary>
        /// Get a door id, given the door
        /// </summary>
        /// <param name="door">the door</param>
        /// <returns>the door id</returns>
        private string GetDoorId(BaseEntity door)
        {
            var pos = door.transform.position;

            return pos.x + "," + pos.y + "," + pos.z;
        }

        /// <summary>
        /// Make the service call to trigger 2FA
        /// </summary>
        /// <param name="playerId">the player to 2fa</param>
        /// <param name="doorId">the door in question</param>
        /// <param name="requesterId">the player who wanted access</param>        
        void GetRequest(string playerId, string doorId, string requesterId)
        {
            string uri = "https://rust2fa.azurewebsites.net/2fa?door=" + doorId + "&player=" + playerId;

            Puts(uri);

            webrequest.EnqueueGet(uri, (code, response) => GetCallback(code, response, playerId, doorId, requesterId), this, null, 60 * 1000);
        }

        /// <summary>
        /// Callback for the service call to trigger 2FA
        /// </summary>
        /// <param name="code">the http response code</param>
        /// <param name="response">the http response</param>
        /// <param name="playerId">the player to 2fa</param>
        /// <param name="doorId">the door in question</param>
        /// <param name="requesterId">the player who wanted access</param>
        void GetCallback(int code, string response, string playerId, string doorId, string requesterId)
        {
            Puts("{0},{1},{2},{3}", code, response, playerId, doorId, requesterId);

            if (!acl.ContainsKey(doorId))
            {
                acl[doorId] = new List<string>();
            }

            pending2FA.Remove(doorId + playerId);

            if (code == 200)
            {
                // grant the user access
                if (!acl[doorId].Contains(requesterId))
                {
                    acl[doorId].Add(requesterId);
                }

                // let the access expire after 10m
                timer.Once(1000 * 60 * 10, () =>
                {
                    acl[doorId].Remove(requesterId);
                });
            }
        }
    }
}