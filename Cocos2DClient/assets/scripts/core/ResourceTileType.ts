import { Enum } from "cc";

// 资源 
export enum ResourceTileType {
    None = 0,
    Forest = 4, //  森林
    Stone = 5, //  石头
    Slag = 6,// 矿渣
    Ore = 7,//  矿石
    Stump = 8,// 树桩
    Tree = 9,// 树木
    Fragment = 10,// 碎片
    Crystal = 11, //水晶
    Weeds = 12,// 野草
    Herb = 13,// 药草
    Ocean = 14,// 海洋
    Fish = 15,//  
}
Enum(ResourceTileType);