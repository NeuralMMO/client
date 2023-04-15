import { InventoryDB } from "../data/Packet";
import { Component } from "./Component";
import { Equipment } from "./Equipment";
import { ItemComponent } from "./ItemComponent";


export class Inventory extends Component {

    items: ItemComponent[] = [];
    equipment: Equipment;
    // 存在相同名字不同数据
    // itemDic: { [key: string]: ItemComponent } = {};

    public Update(data: InventoryDB): void {
     
        this.items = [];
        if (this.equipment == null) this.equipment = new Equipment();
        this.equipment.Update(data.equipment);


        for (let i = 0; i < data.items.length; i++) {
            let itemData = data.items[i];
            let  item = new ItemComponent();
            this.items.push(item);
            item.Update(itemData);
        }

        /*
        for (let i = 0; i < data.items.length; i++) {
            let itemData = data.items[i];
            let key = itemData.item;

            keys.push(key)

            if (this.itemDic[key] == null) {
                this.itemDic[key] = new ItemComponent();
            }
            this.itemDic[key].Update(itemData);
            this.items.push(this.itemDic[key]);
        }

        let removeList = [];
        for (let key in this.itemDic) {
            if (keys.indexOf(key) == -1) removeList.push(key);
        }
        for (let i = 0; i < removeList.length; i++) {
            delete this.itemDic[removeList[i]]
        }
        */
    }
}