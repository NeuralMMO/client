import { ItemDB } from "../data/Packet";
import { Component } from "./Component";


export class ItemComponent extends Component {

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

    public Update(data: ItemDB): void {
        data.color != null && (this.color = data.color);// string; // str（可能有也可能没有）
        data.item != null && (this.item = data.item);// string; //物品名，str
        data.level != null && (this.level = data.level);// number);// 物品等级， int
        data.capacity != null && (this.capacity = data.capacity);// number);// 没啥用， int
        data.quantity != null && (this.quantity = data.quantity);// number);// 数量，int
        data.melee_attack != null && (this.melee_attack = data.melee_attack);// number);// 近战攻击力加成，int
        data.range_attack != null && (this.range_attack = data.range_attack);// number);// 远程攻击力加成，int
        data.mage_attack != null && (this.mage_attack = data.mage_attack);// number);//魔法攻击力加成，int
        data.melee_defense != null && (this.melee_defense = data.melee_defense);// number);//近战防御力加成，int
        data.range_defense != null && (this.range_defense = data.range_defense);// number);//远程防御力加成，int
        data.mage_defense != null && (this.mage_defense = data.mage_defense);// number);//魔法防御力加成，int
        data.health_restore != null && (this.health_restore = data.health_restore);// number);// 回复血量值， int
        data.resource_restore != null && (this.resource_restore = data.resource_restore);// number);//回复食物/水值， int
        data.price != null && (this.price = data.price);// number;//没啥用
    }
}