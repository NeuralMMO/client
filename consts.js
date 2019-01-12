const tick = 0.6;
const nAnim = 16;
const worldWidth = 64, worldDepth = 64;
const worldHalfWidth = worldWidth / 2;
const worldHalfDepth = worldDepth / 2;
const sz = 64;
const tileSz = 64;

const tiles = {
    0: "lava",
    1: "water",
    2: "grass",
    3: "scrub",
    4: "forest",
    5: "stone",
    6: "orerock",
}
const tileColors = {
    "lava": 0xfff000, // orange
    "water": 0x0000bb, // darkblue
    "grass": 0x0fff0f, // green
    "scrub": 0x0ff00f, // lightgreen
    "forest": 0x00ff00, // darkgreen
    "stone": 0x0f0f0f, // darkgrey
    "orerock": 0x808080 // grey
}

const tileHeights = {
   0: 1,
   1: 0,
   2: 1,
   3: 1,
   4: 1,
   5: 2,
   6: 1
}
