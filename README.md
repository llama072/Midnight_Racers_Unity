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
- 🏆 **Online leaderboard** — backendre felküldött scoreok, top játékosok listája
- 👤 **Login a játékban** — ugyanazzal a fiókkal, mint a weboldalon (JWT auth)
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


## 🖼️ Képernyőképek
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/9a693eb0-15d6-47c0-9c66-6c236a95288e" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/84be504c-2669-4468-a93e-2483d24486f4" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/8207de2f-7476-4e8f-be14-4ec88ec23e12" />



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

| Művelet | Billentyűzet |
|---------|--------------|
| Gyorsítás | `W` / `↑`| 
| Fékezés / hátramenet | `S` / `↓` |
| Kormányzás | `A` `D` / `←` `→` |
| Pause | `Esc` | Start  |

## 🚀 Telepítés és futtatás


### Játékosoknak (kész build)

A legfrissebb buildet a [Midnight Racers weboldal Download oldaláról](https://nodejs216.dszcbaross.edu.hu) töltheted le, vagy a repó [Releases](https://github.com/llama072/Midnight_Racers_Unity/releases) szekciójából.

```
1. Töltsd le a MidnightRacers_v1.x.zip fájlt
2. Csomagold ki egy mappába
3. Indítsd el a MidnightRacers.exe-t
4. Jelentkezz be a fiókoddal (vagy regisztrálj a weboldalon)
5. Nyomd a gázt 🏁
```



## 🔌 Backend API végpontok (a játékból hívva)

| Metódus | Útvonal | Funkció |
|---------|---------|---------|
| `POST` | `/belepes` | Bejelentkezés a játékon belülről |
| `GET`  | `/me` | Aktuális user info (token validáció) |
| `POST` | `/score-upload` | Eredmény feltöltése a leaderboardra |
| `GET`  | `/leaderboard` | Top játékosok listája |
| `GET`  | `/profil-adatok` | Játékos profil adatok |


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
