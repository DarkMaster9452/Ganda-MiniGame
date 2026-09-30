# Gandža Rolka

60-sekundová minihra v Unity 6 (URP). Prejdi 4 fázy a získaj čo najvyššie skóre (0–100 %, 1–3 hviezdy).

**Ovládanie:** iba `W` `A` `S` `D` + `Space`.

| Fáza | Čas | Mechanika |
|---|---|---|
| 1. Drvenie | 15 s | striedavo `A`/`D`, drž jemnosť v zelenej zóne, `Space` = hotovo |
| 2. Usporiadanie | 12 s | `WASD` pohybuje kurzorom, drž `Space` = sypanie, cieľ rovnomerných 8 segmentov |
| 3. Balenie | 15 s | `W`/`S` rytmus (napätie), `A`/`D` QTE, `Space` zlepenie |
| 4. Dym | 10 s | drž `Space` = nádych, pusti v zóne = krúžok, preplnenie = kašeľ |

Skóre: váhy 20 / 25 / 35 / 20 %, bonus +1 % za ušetrenú sekundu (max +10 %), hviezdy ≥ 50 / 75 / 90 %. High score sa ukladá cez `PlayerPrefs`.

## Spustenie

1. Otvor projekt v Unity `6000.6.3f1`.
2. Otvor `Assets/Scenes/SampleScene` a daj Play. `Bootstrap` si sám vytvorí `GameManager`, kameru, pozadie, fázy a HUD, takže scénu netreba nastavovať.

## Štruktúra

```
Assets/_Project/Scripts/
  Core/    Bootstrap, GameManager, GameTimer, GameContext, ScoreManager, InputReader, Draw, Sfx
  Phases/  PhaseBase, GrindPhase, ArrangePhase, RollPhase, SmokePhase, RollVisual
  UI/      HUD, MeterUI, ResultsScreen
```

Sprity a zvuky sa generujú v kóde (`Draw`, `Sfx`), takže hra nepotrebuje žiadne importované assety. Dajú sa neskôr nahradiť vlastnými.

Rating: obsah zobrazuje fajčenie/drogy, počítaj s PEGI 16/18 a pri publikácii skontroluj pravidlá platformy.
