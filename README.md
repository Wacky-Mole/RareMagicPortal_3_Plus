# Rare Magic Portal Plus

**Support Me!**  
If you enjoy this mod and want to support its continued development, you can **[Buy Me a Coffee on Ko-Fi](https://ko-fi.com)**! Every bit of support helps keep the portals running smoothly!

---

## **Overview**
**Rare Magic Portal Plus** is a feature-rich mod designed to overhaul how portals function in Valheim. It adds depth, challenge, and immersion to an otherwise straightforward teleportation mechanic. Whether you want to create a thriving portal economy, encourage multiplayer cooperation, enforce item restrictions, or simply customize your portals with vibrant colors, this mod is for you.

Portals become strategic tools rather than limitless shortcuts. With customizable **Crystals**, **Keys**, and **Magical Portal Fluid**, servers can implement scarcity to foster teamwork, competition, and a thriving in-game economy. From **biome-themed portal colors** to **VIP player restrictions**, this mod offers a broad array of options to make portals as unique and dynamic as your world.

If you're tired of the default portal mechanics and want more flexibility and control, **Rare Magic Portal Plus** is the solution!

---

## **Key Features**

### **1. Magical Portal Fluid**
- Introduces **Magical Portal Fluid** as a required resource for portal construction.
- Fluid scarcity limits the number of portals players can build, creating a more thoughtful and cooperative multiplayer experience.  
- **Fully Configurable**: Adjust drop rates, crafting requirements, and more.  

</br>

### **2. Crystal and Key Economy**
This system adds a layer of strategy and progression to portal use.

- **Crystals** are consumed upon entering portals, making their use a calculated decision.
- **Keys** grant one-time or permanent access to specific portals without consumption.

Prefab IDs (Crystals):  
- `PortalCrystalRed`, `PortalCrystalGold`, `PortalCrystalTan`, `PortalCrystalGreen`, `PortalCrystalBlue`, `PortalCrystalPurple`, `PortalCrystalCyan`, `PortalCrystalOrange`, `PortalCrystalBlack`, `PortalCrystalWhite`, `PortalCrystalYellow`

Prefab IDs (Keys):  
- `PortalKeyGold`, `PortalKeyRed`, `PortalKeyTan`, `PortalKeyGreen`, `PortalKeyBlue`, `PortalKeyPurple`, `PortalKeyCyan`, `PortalKeyOrange`, `PortalKeyBlack`, `PortalKeyWhite`, `PortalKeyYellow`

---

### **3. Dynamic Portal Colors**
Portals don’t have to be dull anymore! Customize portals with **vibrant colors** to match biomes, themes, or server events.

#### **Default Colors by Biome:**
- **Meadows**: Tan
- **Black Forest**: Blue
- **Swamp**: Green****
- **Plains**: Orange
- **Mistlands**: Purple
- **Deep North**: Cyan
- **Ashlands**: Red
- **Mountain**: Black

#### **Special Colors:**
- **Gold**: Endgame or Master portals.
- **White**: Special-use portals for admins or VIPs.
- **Rainbow**: Dynamic cycling colors when Portal Drink is active.

#### **Custom Features:**
- **Admin-Only Colors**: Restrict some portal colors to server admins.
- **Biome-Forced Colors**: Automatically assign colors based on the portal’s location.
- **Manual Cycling**: Use shortcut keys to change portal colors on the fly.

---

### **4. Portal Restrictions and Freedom**
Take control of what players can transport through portals:

#### **Restrict Items:**
- Block specific items like **wood, stone, or metals** from teleporting.
- Define additional restrictions per portal via YAML configuration.

#### **Teleport Anything Mode:**
- Use the **Portal Drink** to bypass inventory checks temporarily.
- Configurable duration and restricted items for balance.

---

### **5. Portal Limits and Player Management**
Promote fairness and balance by setting limits on portal creation:

- **Maximum Portals per Player:** Configurable caps for regular and VIP players.
- **World Portal Limit:** Restrict the total number of portals in the world.
- **Server Sync Enforcement:** Ensure all players follow the same rules.

---

### **6. Portal Economy Integration**
Add Crystals, Keys, and Portal Fluid to your world through various mods and tools:

#### **Recommended Mods for Drop Integration:**
- **[Drop That](https://valheim.thunderstore.io/package/ASharpPen/Drop_That/):** Configure loot tables for boss drops and creatures.
- **[Better Trader](https://valheim.thunderstore.io/package/OdinPlus/Better_Trader_Remake/):** Add items to trader inventories for multiplayer economies.
- **[Epic Loot](https://valheim.thunderstore.io/package/RandyKnapp/EpicLoot/):** Make these resources rewards for completing quests or bounties.
- **[KG Marketplace](https://valheim.thunderstore.io/package/KGvalheim/Marketplace_And_Server_NPCs_Revamped/):** Include them in player-run marketplaces.

---

### **7. Portal Drink**
A game-changing potion that allows **unrestricted teleportation** for a limited time:
- Bypasses normal teleportation rules for items and inventory.
- Active portals temporarily display **Rainbow Mode** or a **White glow.**
- **Fully Configurable:** Adjust duration and item restrictions for balance.

---

## **Configuration Options**
Rare Magic Portal Plus offers a robust set of configuration options through the `RareMagicPortal.cfg` file. Here's a comprehensive list:

Here's the updated README with all your configuration sections and options integrated.

---

# Rare Magic Portal Plus

**Support Me!**  
If you enjoy this mod and want to support its continued development, you can **[Buy Me a Coffee on Ko-Fi](https://ko-fi.com)**! Every bit of support helps keep the portals running smoothly!

---

## **Overview**
**Rare Magic Portal Plus** is a feature-rich mod designed to overhaul how portals function in Valheim. It offers extensive customization and management options, allowing you to create dynamic and immersive teleportation mechanics.

---

## **Key Features**
- **Portal Modes:** Normal, TargetPortal, Rainbow, Password Lock, One-Way, and more.
- **Dynamic Portal Colors:** Match biomes, server events, or player preferences.
- **Portal Economy:** Introduces Magical Portal Fluid, Crystals, and Keys for strategic teleportation.
- **Player Management:** Limit the number of portals per player, enforce restrictions, or enable admin-only settings.
- **YAML Configuration Support:** Easily manage portal-specific settings like colors, access, and behavior.

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
| `Modifier key for Color`        | `CapsLock`  | Sets the modifier key for changing portal colors.                                               |
| `Modifier key for PortalMode`   | `LeftCtrl`  | Sets the modifier key for changing portal modes.                                                |

---

### **Portal Modes**
| **Option**                       | **Default**           | **Description**                                                                                  |
|-----------------------------------|-----------------------|--------------------------------------------------------------------------------------------------|
| `Default Mode for New Portals`   | `Normal`              | Sets the default portal mode for newly placed portals.                                           |
| `Prevent Target Portal Change`   | `On`                 | Disallows non-creators from changing TargetPortal mode.                                          |
| `Prevent Creator of TargetPortalChange` | `Off`        | Restricts changes to TargetPortal mode to admins only.                                           |
| `Force Portal Animation`         | `Off`                | Forces portal animation for TargetPortal mode.                                                  |

---

### **Teleportation Enhancements**
| **Option**                       | **Default** | **Description**                                                                                  |
|-----------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Fly on Warp`                    | `On`        | Enables flight during teleportation to avoid fall damage.                                        |
| `Hide Name`                      | `On`        | Hides the name of portals.                                                                      |
| `Show Warp Hint Left`            | `On`        | Displays teleportation hints on the left-hand side.                                             |

---

### **Portal Colors**
| **Option**                        | **Default** | **Description**                                                                                  |
|------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Enabled Colors for Portals`      | All Colors  | Defines available portal colors: Yellow, Red, Green, Blue, etc.                                 |
| `Default Color`                   | `Yellow`    | Sets the default color for new portals.                                                         |
| `Prevent Color Changing`          | `Off`       | Prevents users from changing portal colors (Admins/Owners override).                            |
| `Use Biome Colors by Default`     | `On`        | Overrides default color with biome-related colors.                                              |
| `Prevent Portal Creators from Changing Biome Color` | `On` | Prevents creators from changing portal colors if biome colors are active.                       |
| `Biome Colors`                    | `Meadows:Tan,BlackForest:Blue,Swamp:Green,...` | Maps biomes to specific colors.                                                                |

---

### **Portal Fluid Settings**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Add PortalFluid to Wood Portal`   | `Off`       | Requires PortalFluid for crafting wood portals.                                                 |
| `Add PortalFluid to Stone Portal`  | `Off`       | Requires PortalFluid for crafting stone portals.                                                |
| `Fluid Per Wood Portal`            | `1`         | Sets the fluid requirement for wood portals.                                                    |
| `Fluid Per Stone Portal`           | `2`         | Sets the fluid requirement for stone portals.                                                   |
| `Portal Magic Fluid Spawn`         | `0`         | Defines the starting quantity of PortalFluid for new players.                                   |

---

### **Crystal and Key Settings**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Use Gold as Portal Master`        | `On`        | Enables Gold Crystals/Keys as master teleportation resources.                                   |
| `Crystal Consume Per Transit`      | `1`         | Sets the number of crystals consumed per portal use.                                            |
| `Use Top Left Message`             | `Off`       | Displays portal tags/messages in the top-left corner instead of center.                         |
| `Free Passage Color`               | `None`      | Specifies the color for portals allowing free passage.                                          |

---

### **Player and Admin Controls**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Only Creator Can Deconstruct`     | `true`      | Restricts portal deconstruction to creators and admins.                                         |
| `Max Weight Allowed for Portals`   | `0`         | Limits the maximum weight of items transported through portals.                                 |
| `Only Admin Can Build`             | `Off`       | Restricts portal construction to admins.                                                        |
| `MaxAmountOfPortals`               | `0`         | Limits the total number of portals per player.                                                  |
| `MaxAmountOfPortals_VIP`           | `0`         | Sets a higher portal limit for VIP players.                                                     |

---

### **Portal Drink**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Portal Drink Timer`               | `120`       | Sets the duration of the Portal Drink effect in seconds.                                        |
| `Portal Drink Wont Allow`          | `None`      | Specifies items disallowed even with Portal Drink.                                              |

---

### **Portal Images**
| **Option**                         | **Default** | **Description**                                                                                  |
|-------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `Activate Portal Images`           | `Off`       | Enables animated portal images during teleportation.                                            |
| `Full Screen Image Only`           | `Off`       | Displays random full-screen images instead of animations.                                       |

---

### **Crystal Selector**
| **Option**                         | **Default**          | **Description**                                                                                  |
|-------------------------------------|----------------------|--------------------------------------------------------------------------------------------------|
| `Use for Crystal Gold`             | `PortalCrystalGold`  | Assigns item prefabs for Gold Crystals.                                                         |
| `Use for Crystal Red`              | `PortalCrystalRed`   | Assigns item prefabs for Red Crystals.                                                          |
| `Use for Crystal Green`            | `PortalCrystalGreen` | Assigns item prefabs for Green Crystals.                                                        |
| `Use for Crystal Blue`             | `PortalCrystalBlue`  | Assigns item prefabs for Blue Crystals.                                                         |
| `Use for Crystal Purple`           | `PortalCrystalPurple`| Assigns item prefabs for Purple Crystals.                                                       |

---

Let me know if additional details or adjustments are needed!

## **Prefab IDs**
Use these prefab IDs for custom configurations or integrations with other mods:

- **Fluid and Drink:** `PortalMagicFluid`, `PortalDrink`
- **Crystals:** `PortalCrystalRed`, `PortalCrystalGold`, `PortalCrystalTan`, etc.
- **Keys:** `PortalKeyGold`, `PortalKeyRed`, etc.

---

## **Compatibility**
Rare Magic Portal Plus works seamlessly with many popular Valheim mods, including:

- **[TargetPortal](https://valheim.thunderstore.io/package/Smoothbrain/TargetPortal/):** A highly recommended mod for advanced portal mechanics.
- **[WayShrine](https://valheim.thunderstore.io/package/Azumatt/Wayshrine/):** Compatible for cross-server travel and shrine functionality.
- **Epic Loot, Drop That, Better Trader, KG Marketplace, and more!**

---

## **Credits**
- **Author:** WackyMole  
- Special thanks to:
  - The **OdinPlus Team** for their guidance and support.
  - Contributors like **Zeall** for improving the documentation and **GraveBear** for updated icons.
- Assets provided by the **Unity Asset Store**:
  - [Alchemy and Magic Pack](https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991)
  - [Translucent Crystals](https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274)

---

## **Changelog**
- **v3.0.0:** Full rewrite and modularization of features.
- **v2.x.x:** Introduced Portal Colors, Biome Modes, and Portal Drink.
- **v1.x.x:** Initial release with core functionality.

---

For feedback, support, or feature suggestions, feel free to **[Buy Me a

 Coffee](https://ko-fi.com)** or leave a comment. Your input makes the portals better for everyone!

--- 

Let me know if you'd like to add even more details!