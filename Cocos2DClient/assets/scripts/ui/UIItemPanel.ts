
import { _decorator, Component, Node, Prefab, instantiate } from 'cc';
import { Equipment } from '../components/Equipment';
import { Inventory } from '../components/Inventory';
import { ItemComponent } from '../components/ItemComponent';
import { UIItem } from './UIItem';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIItemsPanel
 * DateTime = Sat Aug 20 2022 14:09:46 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIItemsPanel.ts
 * FileBasenameNoExtension = UIItemsPanel
 * URL = db://assets/scripts/ui/UIItemsPanel.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIItemPanel')
export class UIItemPanel extends Component {


    @property(Node)
    contents: Node;

    @property(Prefab)
    itemPrefab: Prefab;

    items: UIItem[];

    start() {
        // [3]
    }
    UpdateData(data: Inventory) {

        if (this.items == null || this.items.length == 0) {
            this.items = [];
            for (let i = 0; i < 12; i++) {
                let node = instantiate(this.itemPrefab);
                let item = node.getComponent(UIItem);
                this.items.push(item);
                node.setParent(this.contents);
            }
        }

        let items: ItemComponent[] = data.items;
        let equipment: Equipment = data.equipment;

        let equipmentDic: { [name: string]: number } = {};

        for (let i = 0; i < this.items.length; i++) {
            let item = items[i];
            if (i < items.length && item != null) {

                let equiped: boolean = equipment && equipment.hasEquipItem(item);
                if (equiped && equipmentDic[item.item] == null) {
                    equipmentDic[item.item] = item.level;
                }
                else {
                    equiped = false;
                }

                this.items[i].bindData(item, equiped);
            }
            else {
                this.items[i].bindData(null, false);
            }
        }
    }
    /*
    UpdateData(data: Inventory) {
        let items: ItemComponent[] = data.items;

        let equipment: Equipment = data.equipment;

        if (this.itemUIDic == null) this.itemUIDic = {};
        let keys = [];
        // 先遍历所有Key  
        for (let i = 0; i < items.length; i++) {
            let item = items[i];
            keys.push(item.item);
        }

        // 移除旧的
        let removeList = [];
        for (let key in this.itemUIDic) {
            if (keys.indexOf(key) == -1) { removeList.push(key); }
        }

        for (let i = 0; i < removeList.length; i++) {
            let item = this.itemUIDic[removeList[i]];
            item.node.setParent(null);
            item.node.destroy();

            delete this.itemUIDic[removeList[i]];
        }
        for (let i = 0; i < keys.length; i++) {
            let item = items[i];

            if (this.itemUIDic[item.item] == null) {
                let node = instantiate(this.itemPrefab);
                node.setParent(this.contents);
                this.itemUIDic[item.item] = node.getComponent(UIItem);
            }
            
            let equiped: boolean = equipment && equipment.hasEquipItem(item.item);
            this.itemUIDic[item.item].bindData(item, equiped);
        }

    }
    */
}


