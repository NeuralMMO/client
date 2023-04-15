import { Enum } from "cc";

// 基础类型
export enum TileType {
    None = 0,  //  岩浆 - None 
    Water = 1,  //  水 base 3 
    Grass = 2, //  草地  base  1
    Land = 3,  //泥地 - 地面  //  base 5 
    Stone = 4, //  石头 // base 4
}

Enum(TileType);