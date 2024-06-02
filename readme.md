# UDSE Rebalance

This is a /very/ opinionated rebalance mod for Space Engineers. It makes some
things harder, some things more convenient, and some things easier.

I'll try to keep the list below up to date, but I'd recommend referring to the
git log for details.

This project is available on [GitHub](https://github.com/mkaito/SE-UDSE-Rebalance).

All modifications apply to most modded blocks as appropriate as they are done programmatically on
world load. A few specific modded subtypes have been explicitly excluded to avoid game breaking
problems and eggregious balance issues. If you need more subtypes excluded, please open an issue on
GitHub.

The list below includes all numeric details, but here are some notes on /why/ I
even made some of these modifications. These are, of course, only my opinions.

- Wheels, especially large grid, are woefully underpowered. Have you ever tried
  to build a moderately sized large grid rover, only to have to put as many (or
  more) thrusters on it as if you were building a ship?
- Since I pretty much always play with a deep ores mod, increase ore detector range.
- Ship grinder radius increased so that grinder pits don't end up with blocks stuck between
  grinders.
- Ship drill radius increased so that large grid drills are actually worth using.
- Ship welder radius increased so that you can use fewer, since they are a huge
  performance problem.
- Spotlights and lights in general need way more range.
- Armour, thrusters and cockpits have had their damage multiplier reduced quite
  a bit. Heavy armour is now actually worth using, even on small grid.
- Added a recipe to craft safe zone chips. Also reduced power usage of safe zones and made
  chips last 24h.
- Jump Drives are kind of gimmicky, and I don't like them much. I genuinely prefer bumping the
  speed limit and just flying places. However, instead of removing it, I made it an end-game
  transport option. You probably only need 1 for your ship, regardless of size, but have fun trying
  to power it. Likely only viable for capital ships with a lot of modded power generation. Supports
  SG jump drives if a mod adds them.

## Thruster rebalance concept

Space flight relies on Epstein drives.

Fuel usage of REX RCS has been increased tenfold.

Epsteins can only be placed in one orientation, based on the first Epstein drive placed.

Vanilla H2 thrusters have had their fuel consumption increased dramatically, and only provide 50%
thrust in vacuum. They should be used as boosters or to get off planets.

Vanilla Atmospheric thrusters use a fair bit more power, but provide a lot more thrust. Flying in
gravity will require advanced power generation, favouring rovers and drill rigs.

Vanilla Ion thrusters use barely any power and produce barely any thrust, as realistic ion engines
would. I'm sure you can think of some way to use them anyway.

## Character

A jetpack nerf is included. Its power has been fine tuned so you can get
yourself out of a hole in 1.2G, if you angle yourself correctly and use both
upward and forward acceleration, but you wont hover in place in 1G or higher.

Oxygen consumption has been increased 8-fold in order to reduce suit autonomy.

Either of these can be disabled in the config file, found in your world storage
folder after loading the game at least once with this mod.

## Blocks

### Damage reduction

- Deformation damage is set to the absolute minimum the engine will allow. It can't be disabled,
  but this should get the job done.
- Cockpits take 50% less damage.
- Thrusters take 25% less damage.
- Suspension takes 75% less damage.
- Large grid light armor takes 15% less damage.
- Small grid light armor takes 30% less damage.
- Large grid heavy armor takes 80% less damage.
- Small grid heavy armor takes 70% less damage.

### Thrusters

- Explicitly excluded:
    - MES NPC Thrusters. Enable the appropriate block replacement profiles to avoid NPC falling out
      of the sky or running out of power etc.
    - Rider's Heli-carrier thrusters.
    - Life'Tech Fusion Engines.
    - BY's Gravity Engines.
    - Aryx Lynxon drives and thrusters.
- Hydrogen
    - Efficiency in vacuum 50%
    - Force increase +75% for LG, +25% for SG.
    - Fuel efficiency -82%
- Ion
    - Force decrease -90% for LG, -85% for SG.
    - Power usage -95%
- Atmospheric
    - Force increase +200% for LG, +100% for SG.
    - Power usage +180%.
- Epstein drives may only be placed in the same orientation as existing drives.
- REX RCS fuel usage increased 20x.

### Wheels

- Large grid max friction and propulsion x20, max power usage x2.5.
- Small grid max friction and propulsion x8.

### Lights

- Spotlights range 1250m LG, 780m SG.
- Interior lights range 200m LG, 100m SG.

### Power

- Explicitly excluded:
    - Life'Tech reactors and fuel cells

- Wind turbines produce 20% more power.
- Solar panels produce 60% more power.

### Ship tools

- Welder radius increase 25% for LG, 20% for SG.
- Grinder radius increase 25% for LG, 20% for SG.
- Drill radius increase 20%.

### Gas

- Oxygen farm produces 30x.
- Player consumes 12x oxygen (can turn off in mod config file). Suit and bottle volumes unchanged.
- O2/H2 Generators consume 2MW to produce 500L/s.
- H2 Engines consume 2500L/s to produce 5MW.

### Misc

- Ore detector range x8.
- Laser antenna -90% power usage, toggleable line of sight requirement (mod config file)
- Jump Drives
    - Power input x2000 for LG, x500 for SG.
    - Power required for jump x20000 for SG, x5000 for SG.
    - Max jump distance x10000 for LG, x500 for SG.
    - Max jump mass x500000 for LG, x2000 for SG.