using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER
{
    public enum XAssetType : int
    {
        PHYSPRESET = 0,
        PHYSCOLLMAP = 1,
        XANIMPARTS = 2,
        XMODEL_SURFS = 3,
        XMODEL = 4,
        MATERIAL = 5,
        PIXELSHADER = 6,
        TECHNIQUE_SET = 7,
        IMAGE = 8,
        SOUND = 9,
        SND_VOLUME_FALLOFF_CURVE = 0xA,
        SND_LPF_CURVE = 0xB,
        LOADED_SOUND = 0xC,
        CLIPMAP_SP = 0xD,
        CLIPMAP_MP = 0xE,
        COMWORLD = 0xF,
        GAMEWORLD_SP = 0x10,
        GAMEWORLD_MP = 0x11,
        MAP_ENTS = 0x12,
        FXWORLD = 0x13,
        GFXWORLD = 0x14,
        LIGHT_DEF = 0x15,
        UI_MAP = 0x16,
        FONT = 0x17,
        MENULIST = 0x18,
        MENU = 0x19,
        LOCALIZE_ENTRY = 0x1A,
        WEAPON = 0x1B,
        SNDDRIVER_GLOBALS = 0x1C,
        FX = 0x1D,
        IMPACT_FX = 0x1E,
        SURFACE_FX = 0x1F,
        AITYPE = 0x20,
        MPTYPE = 0x21,
        CHARACTER = 0x22,
        XMODELALIAS = 0x23,
        RAWFILE = 0x24,
        STRINGTABLE = 0x25,
        LEADERBOARD = 0x26,
        STRUCTURED_DATA_DEF = 0x27,
        TRACER = 0x28,
        LASER = 0x29,
        VEHICLE = 0x2A,
        ADDON_MAP_ENTS = 0x2B,
        SHOCKFILE = 0x2C,
        VOLUMESETTING = 0x2D,
        REVERBPRESET = 0x2E,
        FOG = 0x2F,
        COUNT = 0x30,
        STRING = 0x30,
        ASSETLIST = 0x31
    }
    public abstract class AssetPool(GameInstance instance, string name, int index, int headerSize)
    {
        public GameInstance Instance { get; set; } = instance;

        public string Name { get; set; } = name;

        public int Index { get; set; } = index;

        public int HeaderSize { get; set; } = headerSize;

        public List<Asset> Assets { get; set; } = [];

        public abstract void LoadGeneric();

        public abstract void Export(Asset asset);
    }
}
