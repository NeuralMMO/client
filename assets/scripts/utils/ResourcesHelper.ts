import { Color, misc, Prefab, randomRange, resources, SpriteFrame } from "cc";
import { ResourceTileType } from "../core/ResourceTileType";
import { EntityType } from "../entity/Entity";
import { SkillDir, SkillType } from "../skill/SkillType";


export class ResourcesHelper {


    public static playerSkins = {};
    public static NPC_SKIN_IDS: number[] = [1, 2];
    public static PAWNS_IDS: number[] = [1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
    public static NPC_COLOR_DIC =
        {
            "passive": "#fbd148",
            "P": "#fbd148",
            "neutral": "#f78812" ,
            "N": "#f78812",
            "hostile": "#e02401",
            "H": "#e02401",
        };

        // 描边：51050f
        // 字色
        // passive：fbd148
        // neutral：f78812
        // hostile：e02401

    public static NPC_OUTLINE_COLOR_DIC =
        {
            "hostile": "#51050f",
            "H": "#51050f",
            "neutral": "#51050f",
            "N": "#51050f",
            "passive": "#51050f",
            "P": "#51050f",
        };



    public static character = "character/";
    public static NPCPath = "character/NPC";
    public static Pawn = "character/Pawn";
    public static Tiles = "map/resources/";
    public static Map = "map";
    public static ItemIconsPath = "items/";

    public static _TilePathSprites;

    public static get TilePathSprites() {

        if (ResourcesHelper._TilePathSprites == null) {

            ResourcesHelper._TilePathSprites = {};

            ResourcesHelper._TilePathSprites[ResourceTileType.Crystal] = ["prop_8_1", "prop_8_2", "prop_8_3", "prop_8_4",];

            ResourcesHelper._TilePathSprites[ResourceTileType.Fish] = ["prop_4_1", "prop_4_2",];

            ResourcesHelper._TilePathSprites[ResourceTileType.Forest] = ["prop_1_1", "prop_1_2", "prop_1_3", "prop_1_4",];

            ResourcesHelper._TilePathSprites[ResourceTileType.Herb] = ["prop_5_1", "prop_5_2",];

            ResourcesHelper._TilePathSprites[ResourceTileType.Ore] = ["prop_6_1", "prop_6_2", "prop_6_3", "prop_6_4",];

            ResourcesHelper._TilePathSprites[ResourceTileType.Stone] = ["prop_3_1", "prop_3_2", "prop_3_3"];

            ResourcesHelper._TilePathSprites[ResourceTileType.Tree] = ["prop_7_1",];


        }

        return ResourcesHelper._TilePathSprites;
    }



    public static ItemSprteDic: { [name: string]: SpriteFrame };



    public static SkillRoot = "skills";

    public static Team_Color = {
        1: "#d0322e",
        2: "#d02ea3",
        3: "#372ed0",
        4: "#2e7ad0",
        5: "#65d03d",
        6: "#1c8821",
        7: "#9d4a23",
        8: "#974b68",
        9: "#841332",
        10: "#4e5e00",
        11: "#d47880",
        12: "#d0832e",
        13: "#bf21ec",
        14: "#445b7f",
        15: "#00bfa8",
        16: "#998cde",
    }

    public static NPC_COLOR =
        {
            "P": "848484",
            "H": "848484",
            "N": "848484",
        };

    public static SkillSpriteMap: { [key: number]: { [dir: number]: SpriteFrame[] } } = {};


    public static TileSpriteMap: { [key: number]: SpriteFrame[] } = {};


    //  Mainz之后进行预加载
    public static PrepareLoad(): void {
        resources.loadDir(this.Map);
        resources.loadDir(this.ItemIconsPath);
        resources.loadDir(this.character);  
        resources.loadDir(this.SkillRoot);  
    }


    public static GetNpcSkinByID(team: number): string[] {
        let prefix = `${ResourcesHelper.NPCPath}/NPC_${team < 10 ? "0" + team : team}`;
        return [`${prefix}_a/spriteFrame`, `${prefix}_b/spriteFrame`];
    }

    public static GetPawnsByID(team: number): string[] {
        let prefix = `${ResourcesHelper.Pawn}/pawn_${team < 10 ? "0" + team : team}`;
        return [`${prefix}_a/spriteFrame`, `${prefix}_b/spriteFrame`];
    }


    public static GetSkinPathByTeam(skinID: number, type: EntityType): string[] {

        switch (type) {

            case EntityType.NPC:

                return ResourcesHelper.GetNpcSkinByID(skinID);

            case EntityType.Player:
                return ResourcesHelper.GetPawnsByID(skinID);
        }

        return ResourcesHelper.GetPawnsByID(skinID);
    }


    public static GetColorBySkinID(skinID: number, type: EntityType): string {

        switch (type) {

            case EntityType.NPC:

                return "#848484";

            case EntityType.Player:
                return ResourcesHelper.Team_Color[skinID];
        }
        return "#848484";
    }

    public static GetNPCNameColorByName(name: string): string {

        if (this.NPC_COLOR_DIC[name] == null) {
            return "#848484";
        }
        return this.NPC_COLOR_DIC[name];
    }


    public static GetNPCNameOutlineColorByName(name: string): string {

        if (this.NPC_OUTLINE_COLOR_DIC[name] == null) {
            return "#848484";
        }
        return this.NPC_OUTLINE_COLOR_DIC[name];
    }





    public static GetSkins(skinID: number, type: EntityType): SpriteFrame[] {

        let paths: string[] = ResourcesHelper.GetSkinPathByTeam(skinID, type);
        return [ResourcesHelper.GetSpriteFrame(paths[0]), ResourcesHelper.GetSpriteFrame(paths[1])];

    }

    public static GetSpriteFrame(path: string): SpriteFrame {
        return resources.get<SpriteFrame>(path);
    }


    public static GetSkillsSpriteFrames(type: SkillType, dir: SkillDir): SpriteFrame[] {

        let sfs: SpriteFrame[] = [];

        switch (type) {

            case SkillType.mage:
                {
                    break;
                }

            case SkillType.melee:
                {
                    if (ResourcesHelper.SkillSpriteMap[type] == null) {
                        ResourcesHelper.SkillSpriteMap[type] = {};
                    }

                    if (ResourcesHelper.SkillSpriteMap[type][dir] == null
                        || ResourcesHelper.SkillSpriteMap[type][dir].length == 0) {

                        ResourcesHelper.SkillSpriteMap[type][dir] = [];

                        for (let i = 1; i <= 5; i++) {

                            let fileName = `${ResourcesHelper.SkillRoot}/melee/${dir * 1000 + i}`

                            let path = fileName + "/spriteFrame";
                            let sp = resources.get<SpriteFrame>(path);

                            if (sp != null) {
                                ResourcesHelper.SkillSpriteMap[type][dir].push(sp);
                            }
                            else {
                                console.error("GetSkillsSpriteFrames Error ", path);
                                resources.load(fileName, () => {
                                    sp = resources.get<SpriteFrame>(path);
                                    ResourcesHelper.SkillSpriteMap[type][dir].push(sp);
                                });
                            }
                        }
                    }
                    sfs = ResourcesHelper.SkillSpriteMap[type][dir];
                    break;
                }
            case SkillType.range:
                {
                    break;
                }
            default:
                {
                    break;
                }
        }
        return sfs;
    }




    //---------------------- tile -------------------------
    public static GetBaseTile() {
        return;
    }


    public static GetTileFullPath(fileName: string): string {
        return ResourcesHelper.Tiles + fileName + "/spriteFrame";
    }

    public static GetTileSprites(type: number): SpriteFrame[] {

        let arr = ResourcesHelper.TileSpriteMap[type];

        if (arr == null || arr.length == 0) {

            let paths = ResourcesHelper.TilePathSprites[type];

            arr = [];

            for (let i = 0; i < paths.length; i++) {

                let path = ResourcesHelper.GetTileFullPath(paths[i]);

                let sp = ResourcesHelper.GetSpriteFrame(path);

                if (sp == null) {
                    console.error("sp = null path= ", path);
                    continue;
                }

                arr.push(sp);
            }

            ResourcesHelper.TileSpriteMap[type] = arr;

        }
        return arr;
    }

    public static GetTileSprite(type: number): SpriteFrame {

        let sprites = ResourcesHelper.GetTileSprites(type);

        if (sprites != null && sprites.length != 0) {
            let index = Math.floor(randomRange(0, sprites.length));

            let sp = sprites[index];
            if (sp == null) {
                console.log("sp == null", index, sprites.length);

            }
            return sp;
        }
        else {
            console.log("sprites == null type=  ", type);
        }

        return null;
    }


    //---------------------------- item ------------------------
    public static GetItemFullPath(name: string): string {
        return `${this.ItemIconsPath}icon_item_${name}`;
    }

    public static GetItemIcon(name: string): SpriteFrame {

        if (this.ItemSprteDic == null) this.ItemSprteDic = {};

        if (this.ItemSprteDic[name] == null) {

            let fullName = this.GetItemFullPath(name);

            let sp = resources.get<SpriteFrame>(fullName + "/spriteFrame");

            if (sp != null) {

                this.ItemSprteDic[name] = sp;
            }
        }
        return this.ItemSprteDic[name];
    }

}