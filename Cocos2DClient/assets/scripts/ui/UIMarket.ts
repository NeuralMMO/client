
import { _decorator, Component, Node, Prefab, instantiate, Enum } from 'cc';
import { Market } from '../components/Market';
import { MarketDB, MarketItemDB } from '../data/Packet';
import { ItemName } from './UIItem';
import { MarketItemData, UIMarketItem } from './UIMarketItem';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIMarket
 * DateTime = Sun Aug 14 2022 17:11:38 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIMarket.ts
 * FileBasenameNoExtension = UIMarket
 * URL = db://assets/scripts/ui/UIMarket.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIMarket')
export class UIMarket extends Component {


    // itemDic: { [key: string]: UIMarketItem };

    @property(Node)
    contents: Node;

    @property(Prefab)
    itemPrefab: Prefab;

    @property({ type: [UIMarketItem] })
    items: UIMarketItem[] = [];

    start() { }
    public bindData(data: MarketDB): void {


        let itemDic = {};
        for (let key in data) {
            let arr = key.split("_");
            let name = arr[0];
            let level: number = + arr[1];
            let price: number = data[key].price;
            if (itemDic[name] == null) {
                itemDic[name] = new MarketItemData();
                itemDic[name].name = name;
                itemDic[name].priceDic = {};
            }
            itemDic[name].priceDic[level] = price;
        }

        for (let i = 0; i < this.items.length; i++) {
            let item: UIMarketItem = this.items[i];
            let key = ItemName[item.itemName];

            if (itemDic[key] != null) {
                item.bindData(itemDic[key]);
            }
            else {
                item.bindData(null);
            }
        }
    }

    /*
    public bindData(data: MarketDB): void {

        if (this.itemDic == null) this.itemDic = {};

        let removeList: string[] = [];

        let itemDic = {};
        for (let key in data) {
            let arr = key.split("_");
            let name = arr[0];
            let level: number = + arr[1];
            let price: number = data[key].price;
            if (itemDic[name] == null) {
                itemDic[name] = new MarketItemData();
                itemDic[name].name = name;
                itemDic[name].priceDic = {};
            }
            itemDic[name].priceDic[level] = price;
        }

        for (let key in this.itemDic) {
            if (itemDic[key] == null) removeList.push(key);
        }

        for (let i = 0; i < removeList.length; i++) {
            let key = removeList[i];
            let item = this.itemDic[key];
            item.node.setParent(null);
            item.node.destroy();
            delete this.itemDic[key];
        }


        for (let key in itemDic) {

            if (this.itemDic[key] == null) {
                let node = instantiate(this.itemPrefab);
                node.setParent(this.contents);
                this.itemDic[key] = node.getComponent(UIMarketItem);
            }

            this.itemDic[key].bindData(itemDic[key]);
        }

    }
    */

    public onClose(): void {
        this.node.active = false;
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/zh/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/zh/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/zh/scripting/life-cycle-callbacks.html
 */
