using System.Collections.Generic;
using UnityEngine;

namespace RareMagicPortal
{
    public class PortalName


    // special modes only show up for admins
    // 0 - Normal, 1 - Targetportal, 2 - Rainbow, 3 - Password lock,
    // 4 - One Way Portal, 5 One Way Password Locked,  6 - Allowed Users Only, 7 - Transport Network, 8 - Cords Portal, 9 CrystalKey, 10- Random Teleport, 11 - adminOnly
    // 0 - Normal, works like a normal portal in game
    // 1 - If Targetportal is installed default because 1, can be switched. If not on this mode, becomes invisible, switches to private for TargetPortal
    // 2 - Normal portal, but rainbow, rapid color changing
    // 3 - Normal Portal but password locked, asks for password. If password entered correctly starts up and person is added to allowed list
    // 4 - One way Portal - Two Portals, but only the one recently changed to that mode is active. The others are deactivated. 
    // 5 - One Way Portal + Password Locked
    // 6 Allowed Users Only, Admin has to add users to the yml file to allow.
    // 7 Transport Network, - Mostly meant for the on ground portals, always deactivated, speak the magic words and you go to a new location. This Portal doesn't give out information.
    // Maybe has one light powered to indicate that it's name has been set. But Players can't see it's name.
    // 8 Cordinates Portal, Transports the users to any cordinates set by admin.
    // 9 CrystalKey - moved to 3 or something
    // 10 RandomTeleport
    // 11 Admin Only Mode
    {
        public Dictionary<string, Portal> Portals { get; set; }

        
        public class ZDOP
        {

            public bool OverridePortal { get; set; } = false;
            public string Color { get; set; } = "";
            public bool CrystalActive { get; set; } = false;
            public bool FastTeleport { get; set; } = false;
            public bool RandomTeleport { get; set; } = false;
            public int SpecialMode { get; set; } = 99;
            public string BiomeColor { get; set; } // Not used
            public string Biome { get; set; } // Used
            public bool Active { get; set; } = true;
            public string Password { get; set; } = "";
            public string Coords { get; set; } = "";
            public bool ShowName { get; set; } = false;
            public string Creator { get; set; } = "";

            public ZDOP Clone()
            {
                return new ZDOP
                {
                    OverridePortal = this.OverridePortal,
                    Color = this.Color,
                    CrystalActive = this.CrystalActive,
                    SpecialMode = this.SpecialMode,
                    FastTeleport = this.FastTeleport,
                    RandomTeleport = this.RandomTeleport,
                    BiomeColor = this.BiomeColor,
                    Biome = this.Biome,
                    Active = this.Active,
                    Password = this.Password,
                    Coords = this.Coords,
                    ShowName = this.ShowName,
                    Creator = this.Creator
                };
            }
        }
        public class Portal
        {
       
            public string Color { get; set; } = "Yellow";
            public bool TransportNetwork { get; set; } = false;
            public int SpecialMode { get; set; } = 0;
            public bool Gold_Allow { get; set; } = true;
            public bool Free_Passage { get; set; } = false;
            public bool TeleportAnything { get; set; } = false;
            public bool Admin_only_Access { get; set; } = false;
            public float MaxWeight { get; set; } = 0;

            public List<string> AdditionalProhibitItems { get; set; } = new List<string>();// { "Blackmetal", "Iron" };

            public List<string> AdditionalAllowItems { get; set; } = new List<string>();// { "Blackmetal", "Iron" };

            public List<string> AllowedUsers { get; set; } = new List<string>();// { "name", "name2" };// Maybe names instead of steamid
             
            public Dictionary<string, ZDOP> PortalZDOs = new();
            public bool EndPart { get; set; } = true;
        }
    }
}