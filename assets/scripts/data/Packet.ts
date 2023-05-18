

// Pcaket 数据结构


export interface PacketDB {

    border: number;//= 16;  //  熔岩环境，边界 
    size: number;//= 160;  //  地图大小 矩形
    resource: number[][];   //  资源的数据 
    player: { [key: string]: PlayerPacketDB };  // 玩家数据 
    npc: { [key: string]: NPCPacketDB };  //  NPC  数据
    pos: number[];  // 陀螺， 视野中心位置 
    wilderness: number;// = 0;
    market: MarketDB;  // 市场 
    config: ConfigDB;
}


export interface MarketItemDB {
    price: number;
    supply: number; //数量
    name?: string;
    level?: number;
}

// 市场
export interface MarketDB {
    [key: string]: MarketItemDB;
}

//所有配置项 
export interface ConfigDB {
    PLAYER_DEATH_FOG: number;//240 # 缩毒开始时间
    PLAYER_DEATH_FOG_FINAL_SIZE: number;//= 15 # 最后的安全区的半径，安全区是个正方形
    PLAYER_DEATH_FOG_SPEED: number;//= 1 / 16 # 缩毒速度，每16步缩一格

}


export interface StatusDB {
    freeze: number;
}
export interface HistoryDB {
    damage: number;
    timeAlive: number;//存活时间
    attack: AttackDB;// 如果有攻击的话，会有这个字段
    actions: ActionsDB;// 执行的动作，dict 

}


export interface LoadoutDB {
    freeze: number;
}


export interface ResourceDB {
    val: number;
    max: number;
}

export interface PlayerPacketDB {

    status: StatusDB;
    history: HistoryDB;
    loadout: LoadoutDB;
    alive: boolean;
    entID: number;//玩家编号
    annID: number;//种群编号
    base: BaseDB;
    self: number;//（是否为当前控制）：int（0/1） 
    resource: { [key: string]: ResourceDB }
    // skills: { [key: string]: SkillDB | number };
    skills: SkillsDB;
    metrics: MetricsDB;// （用于 显示 leaderboard 和 player info)
    inventory: InventoryDB;

}

export interface SkillsDB {
    mage: SkillDB;// 魔法
    food: SkillDB;// 食物, 不用显示
    carving: SkillDB;// 砍树
    melee: SkillDB;//近战
    fishing: SkillDB;// 钓鱼
    alchemy: SkillDB;//采集水晶
    herbalism: SkillDB;//采集蘑菇
    range: SkillDB;// 远程
    water: SkillDB;// 水， 不用显示
    prospecting: SkillDB;//采集矿石
    level: number;// 最高技能等级（不确定）

}

export interface InventoryDB {
    items: ItemDB[];
    equipment: EquipmentDB;
}
export interface EquipmentDB {

    item_level: number;// 所有装备的等级之和
    meleee_attack: number;// 所有装备的近战攻击力加成之和
    range_attack: number;//所有装备的远程攻击力加成之和
    mage_attack: number;//所有装备的魔法攻击力加成之和
    meleee_defense: number;//所有装备的近战防御力加成之和
    range_defense: number;//所有装备的远程防御力加成之和
    mage_defense: number;//所有装备的魔法防御力加成之和
    held: ItemDB;//如果有手持装备的话，就会有这个字段
    hat: ItemDB; //如果装备头盔，就会有这个字段
    top: ItemDB;//如果装备护胸，就会有这个字段
    bottom: ItemDB;//如果装备护腿，就会有这个字段
    ammunition: ItemDB;//如果装备弹药，就会有这个字段

}


export interface ItemDB {
    color: string; // str（可能有也可能没有）
    item: string; //物品名，str
    level: number;// 物品等级， int
    capacity: number;// 没啥用， int
    quantity: number;// 数量，int
    melee_attack: number;// 近战攻击力加成，int
    range_attack: number;// 远程攻击力加成，int
    mage_attack: number;//魔法攻击力加成，int
    melee_defense: number;//近战防御力加成，int
    range_defense: number;//远程防御力加成，int
    mage_defense: number;//魔法防御力加成，int
    health_restore: number;// 回复血量值， int
    resource_restore: number;//回复食物/水值， int
    price: number;//没啥用

}

export interface SkillDB {
    exp: number;
    level: number;
}

export interface NPCPacketDB {
    status: StatusDB;
    history: HistoryDB;
    loadout: LoadoutDB;
    chestplate: ChestplateDB;
    platelegs: PlatelegsDB;
    base: BaseDB;
    alive: boolean;
    entID: number;
    annID: number;
    resource: { [key: string]: ResourceDB }
    skills: SkillsDB;
    metrics: MetricsDB;
    items: ItemDB[];
    inventory: InventoryDB;
}


export interface StatusDB {
    freeze: number;
}



export interface ActionsDB {
    Move: { Direction: string }
    Use: { [key: string]: ItemDB };
    Sell: { Item: ItemDB, Price: string };
    Buy: { [key: string]: ItemDB };
}


export interface LoadoutDB {
    chestplate: ChestplateDB;
    platelegs: PlatelegsDB;
}

export interface ChestplateDB {
    level: number;
    color: string;
}

export interface PlatelegsDB {
    level: number;
    color: string;
}

export interface BaseDB {
    r: number;
    c: number;
    name: string;
    color: string;
    population: number;
    self: number;
    level: number;
    item_level: number;// 所有装备的等级之和, int

}


export interface AttackDB {
    target: number;
    style: string;
}


// 每个玩家的统计 
export interface MetricsDB {
    PlayerDefeats: number;// 击杀数
    TimeAlive: number;//存活时长
    Gold: number;// 金币数
    DamageTaken: number;// 总计承受伤害
}


// export interface FinalMetricsDB {

//     AliveScore: number; // 8,
//     DefeatScore: number; //1.5,
//     TotalScore: number; //9.5,
//     TimeAlive: number; // 800,
//     Gold: number; //34,
//     DamageTaken: number; // 1026.9,
// }

export interface FinalMetricsesDB {
    // [name: string]: FinalMetricsDB;
    AliveScore: { [population: number]: number };
    DamageTaken: { [population: number]: number };
    DefeatScore: { [population: number]: number };
    Gold: { [population: number]: number };
    TimeAlive: { [population: number]: number };
    TotalScore: { [population: number]: number };
}