# UDSE Rebalance

This is a _very_ opinionated rebalance mod for Space Engineers. It makes some
things harder, some things more convenient, and some things easier.

I'll try to keep the list below up to date, but I'd recommend referring to the
git log for details.

The list below includes all numeric details, but here are some notes on _why_ I
even made some of these modifications. These are, of course, only my opinions.

* Power production balance in vanilla leans heavily towards nuclear power. To
  adjust this somewhat, I've doubled renewable energy, tripled H2 power, and
  halved reactor output. Reactors still have the benefit of energy density and
  duration per unit of fuel, but turbines and solar are hopefully not so weak as
  to be completely obsolete the moment you find some Uranium.
* Wheels, especially large grid, are woefully underpowered. Have you ever tried
  to build a moderately sized large grid rover, only to have to put as many (or
  more) thrusters on it as if you were building a ship?
* Since I pretty much always play with a deep ores mod, increase ore detector range.
* Ship grinder radius increased so that grinder pits don't end up with blocks stuck between grinders.
* Ship drill radius increased so that large grid drill ships can have at least one block between drills.
* Hydrogen thrusters consume way too much fuel for my taste.
* Spotlights and lights in general need way more range.

## Character

A mild jetpack nerf is included. Its power has been fine tuned so you can get
yourself out of a hole in 1.2G, if you angle yourself correctly and use both
upward and forward acceleration, but you wont hover in place in 1G or higher.

Oxygen consumption has been increased 8-fold in order to reduce suit autonomy on
foot.

Neither of these will apply if the world name contains the words "Creative" or
"DEV".

## Blocks

- Courtesy of Enenra of AQD and SEUT fame, a simple and visually pleasing overlay for cameras and turrets.
- Also courtesy of Enenra, a script that severely reduces damage taken and deformation of all armor blocks.
- Wind Turbines produce 2x the output.
- Solar panels produce 2x the output.
- Reactors produce half the output.
- Hydrogen engines produce 3x the output.
- Oxygen farms produce 10x the output.
- Large grid wheels have 20x the power, friction, use 2.5x the power, and take 5x less damage.
- Small grid wheels have 8x the power, friction and take 5x less damage.
- Ore detectors have 5x the range.
- Laser antenna uses 90% less damage while lasing.
- Ship grinder radius increased 2x for large grid, 1.25x for small grid.
- Ship drill radius increased 3x for large grid, 1.3x for small grid.
- Hydrogen thrusters consume one third of the fuel.
- Spotlight range increased to 780m on large grid, 480m on small.
- Interior lights (and corner lights, etc) range increased to 200m on large grid, 100m on small.