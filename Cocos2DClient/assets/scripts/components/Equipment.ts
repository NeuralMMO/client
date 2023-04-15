import { EquipmentDB } from "../data/Packet";
import { Component } from "./Component";
import { ItemComponent } from "./ItemComponent";


export class Equipment extends Component {

    item_level: number;// 所有装备的等级之和
    meleee_attack: number;// 所有装备的近战攻击力加成之和
    range_attack: number;// 所有装备的远程攻击力加成之和
    mage_attack: number;// 所有装备的魔法攻击力加成之和
    meleee_defense: number;//所有装备的近战防御力加成之和
    range_defense: number;// 所有装备的远程防御力加成之和
    mage_defense: number;// 所有装备的魔法防御力加成之和

    held: ItemComponent;
    hat: ItemComponent;
    top: ItemComponent;
    bottom: ItemComponent;
    ammunition: ItemComponent;

    constructor() {
        super();

        this.item_level = 0;// : number;// 所有装备的等级之和
        this.meleee_attack = 0;// 所有装备的近战攻击力加成之和
        this.range_attack = 0;// 所有装备的远程攻击力加成之和
        this.mage_attack = 0;// 所有装备的魔法攻击力加成之和
        this.meleee_defense = 0;//所有装备的近战防御力加成之和
        this.range_defense = 0;// 所有装备的远程防御力加成之和
        this.mage_defense = 0;// 所有装备的魔法防御力加成之和

    }


    public Update(data: EquipmentDB): void {

        this.item_level = data.item_level;// 所有装备的等级之和
        this.meleee_attack = data.meleee_attack;// 所有装备的近战攻击力加成之和
        this.range_attack = data.range_attack;// 所有装备的远程攻击力加成之和
        this.mage_attack = data.mage_attack;// 所有装备的魔法攻击力加成之和
        this.meleee_defense = data.meleee_defense;//所有装备的近战防御力加成之和
        this.range_defense = data.range_defense;// 所有装备的远程防御力加成之和
        this.mage_defense = data.mage_defense;// 所有装备的魔法防御力加成之和

        if (data.held == null) this.held = null;
        else {
            if (this.held == null) this.held = new ItemComponent();
            this.held.Update(data.held);
        }

        if (data.hat == null) this.hat = null;
        else {
            if (this.hat == null) this.hat = new ItemComponent();
            this.hat.Update(data.hat);
        }

        if (data.top == null) this.top = null;
        else {
            if (this.top == null) this.top = new ItemComponent();
            this.top.Update(data.top);
        }

        if (data.bottom == null) this.bottom = null;
        else {
            if (this.bottom == null) this.bottom = new ItemComponent();
            this.bottom.Update(data.bottom);
        }

        if (data.ammunition == null) this.ammunition = null;
        else {
            if (this.ammunition == null) this.ammunition = new ItemComponent();
            this.ammunition.Update(data.ammunition);
        }

    }

    public hasEquipItem(item: ItemComponent): boolean {

        return (this.held && this.held.item == item.item &&   this.held.level == item.level)
            || (this.hat && this.hat.item == item.item &&   this.hat.level == item.level)
            || (this.top && this.top.item == item.item &&   this.top.level == item.level)
            || (this.bottom && this.bottom.item == item.item &&   this.bottom.level == item.level)
            || (this.ammunition && this.ammunition.item == item.item &&   this.ammunition.level == item.level);
    }
}