Here’s the readme with all the config options included for full customization:

---

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

### **2. Crystal and Key Economy**
This system adds a layer of strategy and progression to portal use.

- **Crystals** are consumed upon entering portals, making their use a calculated decision.
- **Keys** grant one-time or permanent access to specific portals without consumption.

#### **Crystal and Key Colors:**
- 11 unique colors: **Yellow, Red, Green, Blue, Purple, Tan, Cyan, Orange, White, Black, and Gold.**
- **Color-Tiered System:** Each color corresponds to specific portal types or uses:
  - **Gold**: The Master Key and Master Crystal allow endgame portal travel.
  - **White**: Special portals for rare, limited-access uses.
  - **Rainbow Mode**: Activated with the Portal Drink for unrestricted teleportation.

#### **Advanced Use Cases:**
- **Team-Only Portals:** Restrict access to specific players or groups.
- **Toll Portals:** Charge Crystals for access to strategic locations or boss areas.
- **Interactive Economy:** Promote resource trading, competition, or cooperation through server events and marketplace integration.

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

### **General Settings**
| **Config Option**                | **Default** | **Description**                                                                                  |
|-----------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| Force Server Config              | `true`      | Ensures server settings are enforced globally.                                                  |
| YML Portal Logs                  | `false`     | Enables logging of YAML updates for portals.                                                    |
| Risky Server Save                | `false`     | Delays YAML updates until server shutdown for better performance.                               |
| Max Portals Per Player           | `0`         | Limits the number of portals a player can build. 0 disables the limit.                          |
| Only Admin Can Build Portals     | `false`     | Restricts portal construction to server admins.                                                 |

### **Portal Fluid Settings**
| **Config Option**             | **Default** | **Description**                                                                                  |
|--------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| Add Portal Fluid (Wood)       | `false`     | Adds Portal Fluid as a crafting requirement for wood portals.                                   |
| Add Portal Fluid (Stone)      | `false`     | Adds Portal Fluid as a crafting requirement for stone portals.                                  |
| Fluid Per Wood Portal         | `1`         | Amount of fluid required for wood portals.                                                      |
| Fluid Per Stone Portal        | `2`         | Amount of fluid required for stone portals.                                                     |

### **Portal Colors**
| **Config Option**                 | **Default** | **Description**                                                                                  |
|------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| Default Portal Color              | `Yellow`    | Sets the default color for new portals.                                                         |
| Enabled Portal Colors             | `All`       | Specifies which portal colors are available for use.                                            |
| Biome Colors                      | `true`      | Enables biome-based portal colors.                                                              |

### **Crystal and Key Settings**
| **Config Option**                 | **Default** | **Description**                                                                                  |
|------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| Use Gold as Portal Master         | `true`      | Makes Gold Crystals and Keys the "master" resource for portal travel.                           |
| Crystal Consumption Per Transit   | `1`         | Number of Crystals consumed per portal use.                                                     |

---

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