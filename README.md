# Race3D - Unity Track Generator

Procedurální generátor tratí pro vesmírné závodní hry s raketou prolétávající dynamickými obručemi.

## 🎮 Ovládání

- Ovládání rakety zajišťují šipky nebo klávesy WSAD

- **1-6** - Načtení předpřipravených levelů (hlavní klávesnice nebo numpad)

## 🏁 Předpřipravené levely

### Level 1 - Tutorial
- **Typ:** Linear
- **Obruče:** 10
- **Obtížnost:** Snadná
- Bez rotace a oscilace

### Level 2 - Gentle Sine
- **Typ:** Sine
- **Obruče:** 15
- **Obtížnost:** Střední
- Jemná sinusová vlna s rotací

### Level 3 - Zigzag Challenge
- **Typ:** Zigzag
- **Obruče:** 20
- **Obtížnost:** Náročná
- Klikatá trať s vertikální oscilací

### Level 4 - Helix Spiral
- **Typ:** Helix
- **Obruče:** 25
- **Obtížnost:** Velmi náročná
- Spirálová trať s kruhovou oscilací

### Level 5 - Lissajous Madness
- **Typ:** Lissajous
- **Obruče:** 30
- **Obtížnost:** Expert
- Komplexní křivky s náhodnou oscilací

### Level 6 - Extreme Random
- **Typ:** Random
- **Obruče:** 40
- **Obtížnost:** Extrémní
- Chaotické rozmístění s variabilní oscilací


## 🏗️ Architektura projektu

### Hlavní komponenty

#### **TrackGenerator**
Hlavní script pro procedurální generování tratí. Umístěn na prázdném GameObject "TrackManager".

#### **LevelMenuManager**
Správa levelů a jejich konfigurací. Umístěn na GameObject "LevelManager".

#### **AircraftChaseCamera**
Script pro sledování hráče kamerou s plynulým pohybem a rotací.

## ⚙️ TrackGenerator - Parametry

### References (Odkazy na prefaby)
| Parametr | Typ | Popis |
|----------|-----|-------|
| `gatePrefab` | GameObject | Prefab obruče (checkpoint gate) |
| `playerPrefab` | GameObject | Prefab rakety/hráče |
| `cameraPrefab` | GameObject | Prefab kamery s AircraftChaseCamera scriptem |

### Camera Settings
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `cameraOffsetFromPlayer` | Vector3 | (0, 5, -10) | Pozice kamery relativně k hráči |
| `setCameraAsMainCamera` | bool | true | Nastaví spawnutou kameru jako main camera |

### Track Settings (Základní nastavení trati)
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `gateCount` | int | 20 | Počet obručí na trati |
| `gateSpacing` | float | 50 | Vzdálenost mezi obručemi |
| `startOffset` | float | -40 | Posun startu trati po ose Z |

### Track Type (Typ trati)
| Hodnota | Popis |
|---------|-------|
| `Linear` | Přímá trať |
| `Sine` | Sinusová vlna |
| `Zigzag` | Klikatá cik-cak trať |
| `Helix` | Spirálová trať (pružina) |
| `Lissajous` | Komplexní křivky (osmičky) |
| `Random` | Náhodné umístění obručí |

### Sine Wave Parameters (Sinusová vlna)
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `sineAmplitude` | float | 10 | Výška vlny (vertikální amplituda) |
| `sineFrequency` | float | 0.1 | Frekvence vlnění |
| `sineHorizontalOffset` | float | 5 | Boční vychýlení (horizontální amplituda) |

### Zigzag Parameters
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `zigzagAmplitude` | float | 15 | Velikost výchylky do stran |
| `zigzagSegmentLength` | float | 5 | Délka segmentu (nepoužívá se) |

### Helix/Spring Parameters (Spirála)
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `helixRadius` | float | 10 | Poloměr spirály |
| `helixPitch` | float | 5 | Výška na jeden závit spirály |

### Lissajous Parameters (Komplexní křivky)
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `lissajousA` | float | 1 | Frekvence X osy |
| `lissajousB` | float | 2 | Frekvence Y osy |
| `lissajousAmplitudeX` | float | 10 | Amplituda X osy |
| `lissajousAmplitudeY` | float | 10 | Amplituda Y osy |
| `lissajousDelta` | float | π/2 | Fázový posun mezi osami |

**Tipy pro Lissajous:**
- A=1, B=1 → kruh
- A=1, B=2 → osmička
- A=3, B=2 → složitější křivka

### Gate Customization (Úprava obručí)
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `rotateGates` | bool | true | Rotovat obruče směrem k následující |
| `gateRotationOffset` | Vector3 | (0,0,0) | Dodatečná rotace obručí (stupně) |
| `oscillateGates` | bool | false | Zapnout oscilaci (pohyb) obručí |
| `oscillationType` | enum | None | Typ oscilace |
| `oscillationAmplitude` | float | 2 | Velikost výchylky oscilace |
| `oscillationSpeed` | float | 1 | Rychlost oscilace |

### Oscillation Types (Typy oscilace)
| Hodnota | Popis |
|---------|-------|
| `None` | Bez oscilace |
| `VerticalSine` | Vertikální sinusový pohyb |
| `HorizontalSine` | Horizontální sinusový pohyb |
| `Circular` | Kruhový pohyb |
| `Random` | Náhodný Perlin noise pohyb |

### Per-Gate Variation (Variace jednotlivých obručí)
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `varyOscillationPhase` | bool | true | Každá obruč má jiný fázový posun |
| `phaseOffsetPerGate` | float | 0.5 | Fázový posun mezi obručemi |
| `varyOscillationSpeed` | bool | false | Náhodná variace rychlosti |
| `speedVariationAmount` | float | 0.2 | Rozsah variace rychlosti (±) |

**Tipy:**
- `phaseOffsetPerGate = 0.5` → vlnový efekt
- `phaseOffsetPerGate = 0` → všechny synchronně
- `speedVariationAmount = 0.2` → ±20% variace rychlosti

### Textures (Textury)
| Parametr | Typ | Popis |
|----------|-----|-------|
| `gateTextures` | Material[] | Pole materiálů pro textury obručí |
| `randomizeTextures` | bool | Náhodné nebo postupné aplikování textur |

### Background (Pozadí)
| Parametr | Typ | Default | Popis |
|----------|-----|---------|-------|
| `backgroundColor` | Color | (0.1, 0.1, 0.2) | Barva pozadí kamery |
| `applyBackgroundColor` | bool | true | Aplikovat barvu na kameru |

## 📋 TrackConfig Structure

Pro programatické vytváření levelů použij `TrackConfig` strukturu:

```csharp
TrackConfig myLevel = new TrackConfig(
    type: TrackGenerator.TrackType.Helix,
    count: 25,
    spacing: 40f
)
{
    helixRadius = 12f,
    helixPitch = 8f,
    rotateGates = true,
    oscillateGates = true,
    oscillationType = TrackGenerator.OscillationType.Circular,
    oscillationAmplitude = 2f,
    varyOscillationPhase = true,
    phaseOffsetPerGate = 0.3f,
    backgroundColor = new Color(0.1f, 0.1f, 0.25f)
};

trackGenerator.GenerateTrack(myLevel);
```

## 🎯 Použití v kódu

### Základní použití
```csharp
// Použij Inspector nastavení
trackGenerator.GenerateTrack();

// Jednoduchá verze
trackGenerator.GenerateTrackSimple(TrackType.Linear, 15, 50f);

// Plně konfigurovaná
trackGenerator.GenerateTrack(trackConfig);
```

### Vyčištění scény
```csharp
trackGenerator.ClearTrack();
```

## 🔧 Setup v Unity

### 1. Vytvoř prázdné GameObjecty
```
TrackManager (prázdný GameObject)
└── TrackGenerator script

LevelManager (prázdný GameObject)
└── LevelMenuManager script
    └── Track Generator → TrackManager (reference)
```

### 2. Připrav prefaby
- **Gate Prefab:** Obruč s TriggerZone (BoxCollider, IsTrigger=true)
- **Player Prefab:** Raketa s controllerem
- **Camera Prefab:** Kamera s AircraftChaseCamera scriptem

### 3. Přiřaď reference v Inspectoru
- TrackGenerator: Gate, Player, Camera prefaby
- LevelMenuManager: TrackGenerator reference

## 🎨 Tipy pro level design

### Obtížnost
- **Snadná:** Linear, málo obručí, velká vzdálenost
- **Střední:** Sine s malou amplitudou, střední počet
- **Těžká:** Helix, Zigzag, oscilace
- **Expert:** Lissajous, Random, variace oscilace

### Vizuální styl
- **Retro:** Tmavé pozadí (0, 0, 0), neonové textury
- **Space:** Modré odstíny (0.1, 0.2, 0.4)
- **Sunset:** Oranžovo-fialové tóny (0.3, 0.1, 0.2)

### Oscilace
- Začni bez oscilace
- Postupně přidávej `VerticalSine` → `Circular` → `Random`
- Variace fáze vytváří hezčí vlnový efekt

## 🐛 Řešení problémů

### Obruče se nevytváří
- Zkontroluj že `gatePrefab` je přiřazený
- Zkontroluj Console pro chyby

### Rotace obručí nefunguje
- Zapni `rotateGates = true`
- Ujisti se že máš více než 1 obruč

### Kamera se od hráče vzdaluje
- Ujisti se že kamera NENÍ child hráče
- AircraftChaseCamera potřebuje `target` referenci

### Input nefunguje
- Projekt používá nový Input System
- Ujisti se že máš `using UnityEngine.InputSystem;`

## 📝 Verze

- **v1.0** - Základní generování tratí
- **v1.1** - Oscilace a rotace obručí
- **v1.2** - Per-gate variace, TrackConfig struktura
- **v1.3** - Nový Input System, kompletní dokumentace

## 👨‍💻 Autoři

- Jarara - Vývoj track generátoru a level systému
- Airse002 - Pohyb rakety a herní mechaniky

## 📄 Licence

Projekt vytvořen pro studijní účely na FAI UTB.
