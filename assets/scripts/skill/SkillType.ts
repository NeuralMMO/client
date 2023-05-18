



// mage: SkillDB;// 魔法
// foog: SkillDB;// 食物, 不用显示
// carving: SkillDB;// 砍树
// melee: SkillDB;//近战
// fishing: SkillDB;// 钓鱼
// alchemy: SkillDB;//采集水晶
// herbalism: SkillDB;//采集蘑菇
// range: SkillDB;// 远程
// water: SkillDB;// 水， 不用显示
// prospecting: SkillDB;//采集矿石

import { Enum } from "cc";




export enum SkillType {

    food = 0,
    melee = 1, //近战 ，当前设计最大攻击4格 发射
    range = 2, // 远程 范围 
    carving = 3,// 砍树
    mage, // 魔法
    fishing, // 钓鱼
    alchemy, //采集水晶
    herbalism,//采集蘑菇
    water,// 采集水， 不用显示
    prospecting, //采集矿石
    
}
Enum(SkillType)

export enum SkillDir {

    RightUp = 1,
    Right = 2,
    RightDown = 3,
    Down = 4,
    LeftDown = 5,
    Left = 6,
    LeftUp = 7,
    Up = 8,
}