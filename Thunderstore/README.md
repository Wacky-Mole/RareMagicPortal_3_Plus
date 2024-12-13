# Rare Magic Portal Plus

<img src="https://wackymole.com/hosts/newportals2.png" width="1000"/>  


**Support Me!**  
<a href="https://www.buymeacoffee.com/WackyMole" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" height='36' style="height: 36px;" ></a>  [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/H2H6LL5GA)</br>
If you enjoy this mod and want to support its continued development. </br> Every bit of support helps keep the portals running smoothly!

---

## **Overview**
**Rare Magic Portal Plus** (RMPP) is a feature-rich mod designed to overhaul how portals function in Valheim. It adds depth, challenge, and immersion to an otherwise straightforward teleportation mechanic. Whether you want to create a thriving portal economy, encourage multiplayer cooperation, enforce item restrictions, or simply customize your portals with vibrant colors, this mod is for you.

Portals become strategic tools for server admins rather than limitless shortcuts. With customizable **Crystals**, **Keys**, and **Magical Portal Fluid**, servers can implement scarcity to foster teamwork, competition, and a thriving in-game economy. From **biome-themed portal colors** to **VIP player restrictions**, this mod offers a broad array of options to make portals as unique and dynamic as your world.

If you're tired of the default portal mechanics and want more flexibility and control, **Rare Magic Portal Plus** is the solution!

This is a complex mod targeting multiplayer servers, but by default most of the settings are disabled.

Another, more basic version, might be released in the future. RMP without the Plus. 

---
<img src="https://wackymole.com/hosts/FireandIce.png" width="500"/>  <img src="https://wackymole.com/hosts/GoldPortal.png" width="500"/>
## **Key Features**
- **Portal Modes:** Normal, TargetPortal, Rainbow, Password Lock, One-Way, and more.
- **Dynamic Portal Colors:** Match biomes, server events, or player preferences.
- **Portal Economy:** Introduces Magical Portal Fluid, Crystals, and Keys for strategic teleportation. The colors can consume any item that you choose. 
- **Player Management:** Limit the number of portals per player, enforce weight restrictions, or enable admin-only settings.
- **YAML Configuration Support:** Easily manage portal-specific settings like colors, access, and behavior.
- **More Portals:** This mod unlocks the default stone portal and adds 5 more new portals types!

---

## **The Vision**

The idea behind Rare Magic Portal Plus (RMP) was straightforward: portals felt too overpowered, and I wanted to introduce mechanics that balanced their convenience with meaningful strategy. At the same time, I understood how crucial portals are for navigating dungeons and exploring the world. When Target Portal was released, I saw an opportunity to support its functionality while expanding on my vision. 

For large servers, the demand for more portal prefab options was clear, and I realized this mod could potentially become a valuable contribution to the community. After dedicating over two years of work—conceptualizing, refining, and developing—RMP has become the feature-rich mod it is today. 

Though the mod is completely free for several reasons, your contributions are always appreciated. If you’d like to support my efforts, your generosity helps keep the magic flowing and inspires me to continue enhancing the Valheim experience.


---
<summary><h2> 1)  Portal Modes </h2></summary>

</br>
Portal modes in **Rare Magic Portal Plus** redefine how portals function, enabling dynamic behavior tailored to specific use cases or server setups. Each mode introduces unique gameplay mechanics, from simple visual enhancements to intricate player and item restrictions. Below are the available portal modes and their functionalities:

---

#### **A. Normal Mode**
The default behavior of portals in Valheim. Players can teleport between linked portals without additional restrictions or special effects.

---

#### **B. TargetPortal Mode**
- Requires the **TargetPortal mod** to function. Otherwise default to Normal behavior
- Allows precise targeting of destination portals.
- Ideal for creating networks where only certain players or groups can utilize specific portals.
- Has configs to lock TargetPortal's more open features. 

---

#### C. Crystal and Key 
- Normal mode but with Crystal and Keys
---

#### **D. Rainbow Mode**
- Applies a visually dynamic, color-changing effect to the portal.
- **Portal Drink** temporarily activates this mode, enabling unrestricted teleportation for players under its influence.
- Great for events or showcasing unique portal designs. Has a On/Off config for different rainbow effect for some portals

---

#### **E. Password Lock Mode**
- Players must enter a password to activate the portal.
- Successful entry adds the player to an allowed list for future access.
- Encourages team play and prevents unauthorized use.

---

#### **F. One-Way Portal**
- Converts the portal into a one-way transportation mechanism.
- Only the most recently updated portal in this mode remains active, while others deactivate.
- Dungeon system portal

---

#### **G. One-Way Password Locked**
- Combines One-Way Portal functionality with Password Locking.
- Players must enter a password to use the portal.
- Ensures controlled and restricted one-way travel.

---

#### **H. Allowed Users Mode**
- Only players listed in the YAML configuration can access the portal.
- Ideal for guilds, VIP areas, or personal player bases.
- Admins manage access via server configuration files.
- You Guild mod support. You can add the guild name to only allow guild members to use this mod. 

---

#### **I. Transport Network Mode**
- By default a nameless portal that appears inactive. Is named by admin
- When a player gets close to a TN portal the /warp feature becomes available. 
- If a player guess or knows the a TN portal name they can do /warp portalName to be transported to that TN portal

---

#### **I. Coordinates Portal Mode**
- Admins set specific world coordinates as the destination.
- Ideal for creating shortcuts to hard-to-reach areas or custom-built hubs.
- Players teleport to precise locations without requiring a paired portal.

---

#### **J. Random Teleport Mode**
- Sends players to a random location upon activation. Player is very likly to die! (most of the map is water)
- Adds unpredictability and risk, suitable for adventure servers or event-based challenges.
- Will only transported within the default map size

---

#### **K. Admin-Only Mode**
- Restricts portal use to server admins.
- Ensures exclusive access for server management, events, or private builds.
- Configurable for specific portal colors or types (e.g., Gold or White).

---

## **Advanced Usage**
Most Portal modes can be combined with **Portal Crystals**, **Keys**,  for even more control and strategy:

- **Crystals and Keys:** Enable Crystal and Keys mode for this portal to access the portal
- **Quick Teleport:** If you want the game to transport players as quickly as possible then enable this. If the portal is close by, this is fine. Longer distance teleports areas will not be fully loaded.
- **Hover Text:** Yellow Text with the portal Name is displayed when a player hovers over the portal, useful for certain things. Dispalys "Danger, Danger" for random Teleport Mode

### **Customization Tips**

---



### **2. Dynamic Portal Colors**
Portals don’t have to be dull anymore! Customize portals with **vibrant colors** to match biomes, themes, or server events.

#### **Default Colors by Biome:**
These can all be changed or manually changed
- **Meadows**: Tan
- **Black Forest**: Blue
- **Swamp**: Green
- **Plains**: Orange
- **Mistlands**: Purple
- **Deep North**: Cyan
- **Ashlands**: Red
- **Mountain**: Black

#### **Special Colors:**
- **Gold**: Endgame or Master portals.
- **White**: Special-use portals for admins or VIPs.
- **Rainbow**: Dynamic cycling colors when Portal Drink is active. OR rainbow Mode

#### **Custom Features:**
- **Admin-Only Colors**: Restrict some portal colors to server admins.
- **Biome-Forced Colors**: Automatically assign colors based on the portal’s Biome location.
- **Manual Cycling**: Use shortcut keys to change portal colors on the fly.

---

### **3. Crystal and Key Economy**
This system adds a layer of strategy and progression to portal use.

- **Crystals** are consumed upon entering portals, making their use a calculated decision. // can be anything prefab, from any mod
- **Keys** grant permanent access to specific color portals without consumption. 
- 
<img src="https://wackymole.com/hosts/typesofcrystals.png" width="1000"/> <img src="https://wackymole.com/hosts/nored.png" width="500"/> 
<img src="https://wackymole.com/hosts/goldPortal.png" width="300"/>

---


### **3. Magical Portal Fluid**
- Introduces **Magical Portal Fluid** as a required resource for portal construction.
- Fluid scarcity limits the number of portals players can build, creating a more thoughtful and cooperative multiplayer experience.  
- **Fully Configurable**: Adjust drop rates, crafting requirements, and more.  

</br>

### **4. Portal Restrictions and Freedom**
Take control of what players can transport through portals:

#### **Restrict Items:**
- Block specific items like **wood, stone, or metals** from teleporting.
- Define additional restrictions per portal via YAML configuration.

#### **Teleport Anything Mode:**
- Use the **Portal Drink** to bypass inventory checks temporarily.
- Configurable duration and restricted items for balance.

---

### **5. Portals**
Promote fairness and balance by setting limits on portal creation:

- **Maximum Portals per Player:** Configurable caps for regular and VIP players.
- **World Portal Limit:** Restrict the total number of portals in the world.
- **Server Sync Enforcement:** Ensure all players follow the same rules.


---
<img src="https://wackymole.com/hosts/portal5.7.sizes.png" width="500"/>  
<img src="https://wackymole.com/hosts/portal.5.8.png" width="500"/>  

#### **Recommended Mods for Drop Integration:**
- **[Drop That](https://valheim.thunderstore.io/package/ASharpPen/Drop_That/):** Configure loot tables for boss drops and creatures.
- **[TradersExtended](https://thunderstore.io/c/valheim/p/shudnal/TradersExtended/):** Add items to trader inventories for multiplayer economies.
- **[SimpleTrader](https://thunderstore.io/c/valheim/p/coemt/SimpleTrader/):** Add items to trader inventories for multiplayer economies. Up to 10 items
- **[TheCodFathersLegacy](https://thunderstore.io/c/valheim/p/blacks7ar/TheCodFathersLegacy/):** a mod to immortalized a loving grandfather as an NPC trader that sells various things.
- **[KG Marketplace](https://valheim.thunderstore.io/package/KGvalheim/Marketplace_And_Server_NPCs_Revamped/):** Include them in player-run marketplaces.

---

### **7. Portal Drink**
A game-changing potion that allows **unrestricted teleportation** for a limited time:
- Bypasses normal teleportation rules for items and inventory.
- Active portals temporarily display **Rainbow Mode** or a **White glow.**
- **Fully Configurable:** Adjust duration and item restrictions for balance.


<img src="https://wackymole.com/hosts/White2.png" width="700"/>  <img src="https://wackymole.com/hosts/OdinsBlessing.png" width="200"/>
---


## **Configuration Options**

Rare Magic Portal Plus includes numerous configuration options categorized by their functionality. Below is a detailed list.

---

### **General Settings**
| **Option**                      | **Default** | **Description**                                                                                  |
|----------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Force Server Config`           | `true`      | Forces server settings to be applied globally.                                                  |
| `YML Portal Logs`               | `Off`       | Enables detailed YAML portal logging.                                                           |
| `Risky Server Save`             | `Off`       | Saves YAML updates only during server shutdown.                                                 |
| `Use Small Server Updates`      | `On`        | Sends only small parts of the YAML to clients.                                                  |
| `Clients Save Data`             | `Off`       | Allows clients to save YAML data locally.                                                       |
| `Modifier key for Color`        | `LeftAlt`  | Sets the modifier key for changing portal colors.                                               |
| `Modifier key for PortalMode`   | `LeftCtrl`  | Sets the modifier key for changing portal modes.                                                |

---

### **1.Portal Modes**
| **Option**                       | **Default**           | **Description**                                                                                  |
|-----------------------------------|-----------------------|--------------------------------------------------------------------------------------------------|
| `Default Mode for New Portals`   | `Normal`              | Sets the default portal mode for newly placed portals.                                           |

---
### **1.2 Target Portal**
| **Option**                       | **Default** | **Description**                                                                                  |
|-----------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Prevent Target Portal Change`   | `On`                 | Disallows non-creators from changing TargetPortal mode.                                          |
| `Prevent Creator of TargetPortalChange` | `Off`        | Restricts changes to TargetPortal mode to admins only.                                           |
| `Force Portal Animation`         | `Off`                | Forces portal animation for TargetPortal mode.                                                  |

---

### **1.7 Teleportation Enhancements**
| **Option**                       | **Default** | **Description**                                                                                  |
|-----------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Fly on Warp`                    | `On`        | Enables flight during teleportation to avoid fall damage.                                        |
| `Hide Name`                      | `On`        | Hides the name of portals.                                                                      |
| `Show Warp Hint Left`            | `On`        | Displays teleportation hints on the left-hand side.                                             |

---

### **1.9 Rainbow Mode Settings**

This section includes options for configuring the **Rainbow Mode** feature, which is activated when using a Portal Drink.

| **Config Option**         | **Default** | **Description**                                                                                  |
|----------------------------|-------------|--------------------------------------------------------------------------------------------------|
| Portal Drink Color Alt     | `Off`       | Changes the portal effect when using a Portal Drink. Default is a Rainbow effect; toggling this changes it to a SwordCheat-like effect.|

---

### **1.9.1 Random Teleport Settings**

This section focuses on the **Random Teleport** mode, where portals can send players to random locations.

| **Config Option**       | **Default** | **Description**                                                                                  |
|--------------------------|-------------|--------------------------------------------------------------------------------------------------|
| Always Active?           | `true`      | Determines whether the portal's visual effects (glow and flames) are always active, or only glow when inactive. | 

---


### **2.Portal Colors**
| **Option**                        | **Default** | **Description**                                                                                  |
|------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Enabled Colors for Portals`      | All Colors  | Defines available portal colors: Yellow, Red, Green, Blue, etc.                                 |
| `Default Color`                   | `Yellow`    | Sets the default color for new portals.                                                         |
| `Prevent Color Changing`          | `Off`       | Prevents users from changing portal colors (Admins/Owners override).                            |
| `Use Biome Colors by Default`     | `On`        | Overrides default color with biome-related colors.                                              |
| `Prevent Portal Creators from Changing Biome Color` | `On` | Prevents creators from changing portal colors if biome colors are active.                       |
| `Biome Colors`                    | `Meadows:Tan,BlackForest:Blue,Swamp:Green,...` | Maps biomes to specific colors.                                                                |

---
### **3.Crystal and Key Settings**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Use Gold as Portal Master`        | `On`        | Enables Gold Crystals/Keys as master teleportation resources.                                   |
| `Crystal Consume Per Transit`      | `1`         | Sets the number of crystals consumed per portal use.                                            |
| `Use Top Left Message`             | `Off`       | Displays portal tags/messages in the top-left corner instead of center.                         |
| `Free Passage Color`               | `None`      | Specifies the color for portals allowing free passage.                                          |

---

### **4.Portal Fluid Settings**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Add PortalFluid to Wood Portal`   | `Off`       | Requires PortalFluid for crafting wood portals.                                                 |
| `Add PortalFluid to Stone Portal`  | `Off`       | Requires PortalFluid for crafting stone portals.                                                |
| `Fluid Per Wood Portal`            | `1`         | Sets the fluid requirement for wood portals.                                                    |
| `Fluid Per Stone Portal`           | `2`         | Sets the fluid requirement for stone portals.                                                   |
| `Portal Magic Fluid Spawn`         | `0`         | Defines the starting quantity of PortalFluid for new players.                                   |

---

<img src="https://wackymole.com/hosts/portal5.5.png" width="500"/>  
<img src="https://wackymole.com/hosts/portal5.6.sizes.png" width="500"/>  

### 5.Portals 
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Only Creator Can Deconstruct`     | `true`      | Restricts portal deconstruction to creators and admins.                                         |
| `Max Weight Allowed for Portals`   | `0`         | Limits the maximum weight of items transported through portals.                                 |
| `Only Admin Can Build`             | `Off`       | Restricts portal construction to admins.                                                        |
| `MaxAmountOfPortals`               | `0`         | Limits the total number of portals per player.                                                  |
| `MaxAmountOfPortals_VIP`           | `0`         | Sets a higher portal limit for VIP players.                                                     |

---

### **5.1 Wood Portal Settings**

This section defines configurations specific to **Wood Portals**, such as crafting requirements, health, and crafting station dependencies.

| **Config Option**              | **Default**         | **Description**                                                                                                                                              |
|---------------------------------|---------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Station Requirement Wood        | `$piece_workbench` | The crafting station required nearby for placing a Wood Portal. Options include `Workbench = $piece_workbench`, `Forge = $piece_forge`, or `Artisan Station = $piece_artisanstation`. |
| Level of CraftingStation Req    | `1`                 | The level of the crafting station required to place a Wood Portal.                                                                                           |
| Portal Health Wood              | `400f`             | The health value of a Wood Portal.                                                                                                                           |

---

### **5.2 Stone Portal Settings**

This section defines configurations specific to **Stone Portals**, including crafting requirements, health, and crafting station dependencies.

| **Config Option**              | **Default**           | **Description**                                                                                                                                              |
|---------------------------------|-----------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Portal Health Stone             | `1000f`              | The health value of a Stone Portal.                                                                                                                          |
| Station Requirement Stone       | `$piece_stonecutter` | The crafting station required nearby for placing a Stone Portal. Options include `Workbench = $piece_workbench`, `Forge = $piece_forge`, or `Artisan Station = $piece_artisanstation`. |

---

### **5.3 Original Stone Portal Settings**

This section allows customization of the **Original Stone Portal** (if used) by defining its crafting station and recipe.

| **Config Option**                         | **Default**                                   | **Description**                                                                                                                                              |
|-------------------------------------------|-----------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Original Stone Crafting Station           | `$piece_workbench`                           | The crafting station required nearby for placing the Original Stone Portal.                                                                                  |
| Original Stone Recipe                     | `GreydwarfEye:20,SurtlingCore:10,Obsidian:100,CheatSword:1` | The crafting recipe for the Original Stone Portal. Format: `id:amount,id:amount,...`.                                                                        |

---


### **6.Portal Drink**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Portal Drink Timer`               | `120`       | Sets the duration of the Portal Drink effect in seconds.                                        |
| `Portal Drink Wont Allow`          | `None`      | Specifies items disallowed even with Portal Drink.                                              |

---
### **7. Colors Allow**

This section configures additional items that can be transported through portals of specific colors when **CrystalActive** is enabled or **Prevent Color Changing** is set to true. These overrides apply to individual portal colors.

| **Config Option**       | **Default** | **Description**                                                                                          |
|--------------------------|-------------|----------------------------------------------------------------------------------------------------------|
| Color Yellow Allows      | `""`        | Additional items allowed for Yellow portals. Example: `"Iron,Copper"`                                    |
| Color Blue Allows        | `""`        | Additional items allowed for Blue portals. Example: `"Iron,Copper"`                                      |
| Color Green Allows       | `""`        | Additional items allowed for Green portals. Example: `"Iron,Copper"`                                     |
| Color Purple Allows      | `""`        | Additional items allowed for Purple portals. Example: `"Iron,Copper"`                                    |
| Color Tan Allows         | `""`        | Additional items allowed for Tan portals. Example: `"Iron,Copper"`                                       |
| Color Cyan Allows        | `""`        | Additional items allowed for Cyan portals. Example: `"Iron,Copper"`                                      |
| Color Orange Allows      | `""`        | Additional items allowed for Orange portals. Example: `"Iron,Copper"`                                    |
| Color Black Allows       | `""`        | Additional items allowed for Black portals. Example: `"Iron,Copper"`                                     |
| Color White Allows       | `""`        | Additional items allowed for White portals. Example: `"Iron,Copper"`                                     |
| Color Gold Allows        | `""`        | Additional items allowed for Gold portals. Example: `"Iron,Copper"`                                      |

---
<img src="https://wackymole.com/hosts/quadraframeblack.png" width="700"/>  

Some black versions have different materials and even images.


### **8. Crystal Selector**

This section allows you to replace the default portal crystals with custom items. Integration with mods like **JewelCrafting** can provide unique crystals for specific portals.

| **Config Option**       | **Default**             | **Description**                                                                                                                                                                                |
|--------------------------|-------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Use for Crystal Gold     | `PortalCrystalMaster`  | Replace the default Gold crystal with a custom item. Example: `"Shattered_Yellow_Crystal"`                                                                                                     |
| Use for Crystal Red      | `PortalCrystalRed`     | Replace the default Red crystal with a custom item. Example: `"Uncut_Yellow_Stone"`                                                                                                            |
| Use for Crystal Green    | `PortalCrystalGreen`   | Replace the default Green crystal with a custom item. Example: `"Simple_Yellow_Socket"`                                                                                                        |
| Use for Crystal Blue     | `PortalCrystalBlue`    | Replace the default Blue crystal with a custom item. Example: `"Advanced_Yellow_Socket"`                                                                                                       |
| Use for Crystal Yellow   | `PortalCrystalYellow`  | Replace the default Yellow crystal with a custom item. Example: `"Perfect_Yellow_Socket"`                                                                                                      |
| Use for Crystal Purple   | `PortalCrystalPurple`  | Replace the default Purple crystal with a custom item. Example: `"Uncut_Purple_Stone"`                                                                                                         |
| Use for Crystal Tan      | `PortalCrystalTan`     | Replace the default Tan crystal with a custom item. Example: `"Simple_Tan_Socket"`                                                                                                             |
| Use for Crystal Cyan     | `PortalCrystalCyan`    | Replace the default Cyan crystal with a custom item. Example: `"Advanced_Cyan_Socket"`                                                                                                         |
| Use for Crystal Orange   | `PortalCrystalOrange`  | Replace the default Orange crystal with a custom item. Example: `"Perfect_Orange_Socket"`                                                                                                      |
| Use for Crystal White    | `PortalCrystalWhite`   | Replace the default White crystal with a custom item. Example: `"Shattered_White_Crystal"`                                                                                                     |
| Use for Crystal Black    | `PortalCrystalBlack`   | Replace the default Black crystal with a custom item. Example: `"Uncut_Black_Stone"`                                                                                                           |

---


### **9. Portal Images**
<img src="https://wackymole.com/hosts/portal.tp.images.png" width="800"/>  </br>

This section allows you to replace the default portal transporting screen (black background) with a random background image and a inner transport circle with a biome specific image. Very Cool! Restart Required

| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Activate Portal Images`           | `Off`       | Enables animated portal images during teleportation.      YOU SHOULD TURN THIS ON, it's AWESOME   |
| `Full Screen Image Only`           | `Off`       | Displays random full-screen images instead of animations.                                       |

---


## **Prefab IDs**
Use these prefab IDs for custom configurations or integrations with other mods:

- **Fluid and Drink:** `PortalMagicFluid`, `PortalDrink`

Prefab IDs (Crystals):  
- `PortalCrystalRed`, `PortalCrystalGold`, `PortalCrystalTan`, `PortalCrystalGreen`, `PortalCrystalBlue`, `PortalCrystalPurple`, `PortalCrystalCyan`, `PortalCrystalOrange`, `PortalCrystalBlack`, `PortalCrystalWhite`, `PortalCrystalYellow`

Prefab IDs (Keys):  
- `PortalKeyGold`, `PortalKeyRed`, `PortalKeyTan`, `PortalKeyGreen`, `PortalKeyBlue`, `PortalKeyPurple`, `PortalKeyCyan`, `PortalKeyOrange`, `PortalKeyBlack`, `PortalKeyWhite`, `PortalKeyYellow`

---

## **Compatibility**
Rare Magic Portal Plus works seamlessly with many popular Valheim mods, including:

- **[TargetPortal](https://valheim.thunderstore.io/package/Smoothbrain/TargetPortal/):** A highly recommended mod for advanced portal mechanics. This mod has tight integration with TP.
- **Epic Loot, JewelCrafting, Drop That, KG Marketplace, and more!**
- **Not Compatible with the old RMP 2.0**
- **Not Compatible with Advanced Portals**

---

## **Target Portal**
So Target Portal is an intersting mod. I really wanted to add it to RMP, so I did. 

I works awesome with RMPP. 

RMPP actually fixes a lot of my personal gripes with Target Portal through the extra configs too!

Download TargetPortal and RMPP together, you won't be disappointed. 
<img src="https://wackymole.com/hosts/TargetPortalRMP2.png" width="700"/> 
---

## **Acknowledgments**
- **My Wife:**   
- **My Wife again for putting up with all my modding time and being awesome.**    
- Special thanks to:
  - The **OdinPlus Team** for their guidance and support.
  - The **Blaxx** for making TargetPortal easy to patch. Seriously, I probably overrode half of the patches in this Target Portal to get compatibility. I am glad it worked out.
  -  **GraveBear** for updated icons.
- Assets provided by the **Unity Asset Store**:
  - [Alchemy and Magic Pack](https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991)
  - [Translucent Crystals](https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274)

---

## **Changelog**
- **v3.0.0:** Full rewrite and modularization of features + extra portals.
- **v2.x.x:** Introduced Portal Colors, Biome Modes, and Portal Drink.
- **v1.x.x:** Initial release with core functionality.

---



For feedback, support, or feature suggestions, feel free to <a href="https://www.buymeacoffee.com/WackyMole" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" height='36' style="height: 36px;" ></a>  [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/H2H6LL5GA)

--- 

## **KeyManager**
 I put over a year of effort into making this mod, I don't want it to be misused or uncredited. 

[![KeyManager Disclaimer](https://noobtrap.eu/images/keymanager_disclaimer_server.png)](https://key.sayless.eu/faq.php)
---