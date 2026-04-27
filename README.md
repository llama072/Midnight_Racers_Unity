<div align="center">

# 🏁 MIDNIGHT RACERS — UNITY

<p>
  <img src="https://img.shields.io/badge/Unity-2022%20LTS-000000?style=for-the-badge&logo=unity&logoColor=white" alt="Unity"/>
  <img src="https://img.shields.io/badge/C%23-11-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#"/>
  <img src="https://img.shields.io/badge/Pixel%20Art-2D-FF69B4?style=for-the-badge&logo=aseprite&logoColor=white" alt="Pixel Art"/>
  <img src="https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Windows"/>
  <img src="https://img.shields.io/badge/Genre-Street%20Racing-8A2BE2?style=for-the-badge" alt="Street Racing"/>
</p>

**🌃 Az aszfalt sosem alszik. 🌃**

*A Midnight Racers — egy pixel art indie street racing játék — hivatalos Unity projektje.*

</div>

---

## 📖 A projektről

A **Midnight Racers** egy free-to-play indie street racing játék, ami az éjszakai underground versenyek hangulatát idézi. Ez a repó magát a játékot tartalmazza — a **Unity** projektet, a forráskódot, a sprite-okat, a pályákat és a buildelési scriptet.

A játék **pixel art** stílusban készült, **Unity 2022 LTS** alapokon, **C#** scriptekkel. A játékos egy testreszabható autóval versenyezhet az éjszakai város utcáin, gyűjtheti a pontokat és felkerülhet a globális leaderboardra.

> 🔗 **Frontend repó (weboldal):** [Midnight Racers Frontend](https://github.com/Bomba343/Midnight_Racers_FrontEnd)
> 🔗 **Backend repó (API):** [Midnight Racers Backend](https://github.com/llama072/Midnight_Racers_BackEnd)

## ✨ Funkciók

- 🏎️ **Több autó választható** — különböző sebesség / kormányzás / gyorsulás statokkal
- 🌃 **Éjszakai city pályák** — neonfényes utcák, pixel art környezet
- 🕹️ **Klasszikus arcade racing** — könnyen tanulható, nehezen mestelhető irányítás
- 🏁 **Time trial mód** — versenyezz az óra ellen
- 💨 **Nitro / boost rendszer** — gyorsulás kritikus pillanatokban
- 🏆 **Online leaderboard** — backendre felküldött scoreok, top játékosok listája
- 👤 **Login a játékban** — ugyanazzal a fiókkal, mint a weboldalon (JWT auth)
- 🎵 **Synthwave soundtrack** — eredeti zene a hangulathoz
- 💾 **Local save** — beállítások, progress, legjobb körök


## 🛠️ Használt technológiák

| Kategória | Eszközök |
|-----------|----------|
| **Engine** | Unity 2022 LTS (2022.3.x) |
| **Programozási nyelv** | C# 11 |
| **Grafika** | 2D pixel art (Aseprite, Photoshop) |
| **Hang** | Unity Audio + ingyenes synthwave loopok |
| **Input** | Unity Input System (billentyűzet + gamepad) |
| **Backend kommunikáció** | `UnityWebRequest` + Bearer token (JWT) |
| **Verziókezelés** | Git + Git LFS (a nagyobb asseteknek) |

## 📂 Mappastruktúra

```
Midnight_Racers_Unity/
├── Assets/
│   ├── Animations/          # Sprite animációk (autók, karakterek)
│   ├── Audio/
│   │   ├── Music/           # Háttérzene (synthwave loops)
│   │   └── SFX/             # Motor, fék, ütközés hangok
│   ├── Materials/           # 2D anyagok, shader-ek
│   ├── Prefabs/
│   │   ├── Cars/            # Autó prefabok
│   │   ├── Tracks/          # Pálya elemek
│   │   └── UI/              # UI prefabok
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Garage.unity
│   │   ├── Race_Downtown.unity
│   │   └── Race_Highway.unity
│   ├── Scripts/
│   │   ├── Car/             # CarController, CarStats, Nitro
│   │   ├── Game/            # GameManager, RaceManager, Timer
│   │   ├── UI/              # MenuController, HUD, LoginPanel
│   │   ├── Network/         # ApiClient, AuthManager, ScoreUploader
│   │   └── Utility/         # Helper-ek, extensionek
│   ├── Sprites/             # Pixel art sprite-ok
│   └── Fonts/               # Orbitron, pixel font
├── Packages/
│   └── manifest.json        # Unity package dependenciák
├── ProjectSettings/         # Unity projekt beállítások
├── Builds/                  # Buildelt verziók (gitignore)
└── README.md
```

## 🎮 Irányítás

| Művelet | Billentyűzet | Gamepad |
|---------|--------------|---------|
| Gyorsítás | `W` / `↑` | RT / R2 |
| Fékezés / hátramenet | `S` / `↓` | LT / L2 |
| Kormányzás | `A` `D` / `←` `→` | Bal stick |
| Nitro / Boost | `Shift` | A / X |
| Kézifék | `Space` | B / O |
| Kamera váltás | `C` | Y / □ |
| Pause | `Esc` | Start |

## 🚀 Telepítés és futtatás

### Előfeltételek

- **Unity Hub** legfrissebb verzió
- **Unity 2022.3 LTS** (a projekt ezzel készült)
- **Git** + **Git LFS** (a nagyobb asseteknek)
- **Visual Studio 2022** vagy **Rider** (script szerkesztéshez)

### Lépések fejlesztőknek

```bash
# 1. Klónozd a repót (LFS-sel együtt)
git lfs install
git clone https://github.com/llama072/Midnight_Racers_Unity.git
cd Midnight_Racers_Unity

# 2. Nyisd meg Unity Hubban
#    - Open → Add project from disk → válaszd ki a klónozott mappát
#    - Unity 2022.3 LTS-t használj

# 3. Várd meg, hogy a Unity importálja az asseteket
#    (első indításkor 5–10 perc is lehet)

# 4. Nyisd meg a MainMenu.unity scene-t
#    Assets/Scenes/MainMenu.unity

# 5. Nyomd meg a Play gombot ▶
```

### Játékosoknak (kész build)

A legfrissebb buildet a [Midnight Racers weboldal Download oldaláról](https://nodejs216.dszcbaross.edu.hu) töltheted le, vagy a repó [Releases](https://github.com/llama072/Midnight_Racers_Unity/releases) szekciójából.

```
1. Töltsd le a MidnightRacers_v1.x.zip fájlt
2. Csomagold ki egy mappába
3. Indítsd el a MidnightRacers.exe-t
4. Jelentkezz be a fiókoddal (vagy regisztrálj a weboldalon)
5. Nyomd a gázt 🏁
```

### Build készítése Unityből

```
File → Build Settings
   ↓
Platform: PC, Mac & Linux Standalone
Target Platform: Windows
Architecture: x86_64
   ↓
Build → válaszd a /Builds mappát
```

## ⚙️ Konfiguráció

A backend URL és egyéb beállítások a `Assets/Scripts/Network/ApiConfig.cs` fájlban találhatók:

```csharp
public static class ApiConfig
{
    // Fejlesztéshez:
    public const string BASE_URL_DEV = "http://localhost:3000";

    // Élesben:
    public const string BASE_URL_PROD = "https://nodejs216.dszcbaross.edu.hu";

#if UNITY_EDITOR
    public static string BaseUrl => BASE_URL_DEV;
#else
    public static string BaseUrl => BASE_URL_PROD;
#endif
}
```

> 💡 Ha saját szerverre szeretnéd kötni a játékot, csak állítsd át a fenti konstansokat.

## 🔌 Backend API végpontok (a játékból hívva)

| Metódus | Útvonal | Funkció |
|---------|---------|---------|
| `POST` | `/belepes` | Bejelentkezés a játékon belülről |
| `GET`  | `/me` | Aktuális user info (token validáció) |
| `POST` | `/score-upload` | Eredmény feltöltése a leaderboardra |
| `GET`  | `/leaderboard` | Top játékosok listája |
| `GET`  | `/profil-adatok` | Játékos profil adatok |

## 🎨 Dizájn és hangulat

A játék vizuális világa a 80-as évek synthwave / outrun esztétikájához nyúl vissza:

- 🌃 **Pixel art grafika** — kézzel rajzolt sprite-ok, ~32×32 / 64×64 felbontás
- 🌈 **Neon paletta** — cián, magenta, lila, mély kék árnyalatok
- 🌙 **Éjszakai város** — csillagos égbolt, neon táblák, vizes aszfalt tükröződések
- 🎵 **Synthwave OST** — eredeti zenei aláfestés a versenyekhez
- 🔤 **Tipográfia** — Orbitron a UI-on, pixel font a HUD-on
- 💎 **CRT shader effekt** — opcionálisan bekapcsolható retro hangulathoz

## 🐛 Ismert hibák / TODO

- [ ] AI ellenfelek finomhangolása nehezebb pályákon
- [ ] Több pálya hozzáadása (Harbor, Mountain, Industrial)
- [ ] Multiplayer (split-screen lokálisan, később online)
- [ ] Replay rendszer
- [ ] Achievement / kihívás rendszer
- [ ] Linux és Mac build tesztelése

## 👥 Csapat

| Tag | Szerep |
|-----|--------|
| [**Pap Teofil**](https://github.com/llama072) | Mindenes (kód, design, asset) |
| [**Földi Márk**](https://github.com/Bomba343) | Mindenes (kód, pálya, hang) |

---

<div align="center">

Made with ❤️ and 🏁 by the **Midnight Racers** team

*„Select your destiny."*

</div>
