
import { _decorator, Component, Node, Label, Sprite, Prefab, instantiate } from 'cc';
import { MarketItemDB } from '../data/Packet';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { ItemName } from './UIItem';
import { UIMarketLevelPrice } from './UIMarketLevelPrice';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIMarketItem
 * DateTime = Sun Aug 14 2022 17:11:28 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIMarketItem.ts
 * FileBasenameNoExtension = UIMarketItem
 * URL = db://assets/scripts/ui/UIMarketItem.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */


export class MarketItemData {
    name: string;
    priceDic: { [level: number]: number };

}
@ccclass('UIMarketItem')
export class UIMarketItem extends Component {

    @property({ type: ItemName })
    itemName: ItemName;

    @property(Node)
    contents: Node;
    @property(Label)
    title: Label;
    @property(Sprite)
    icon: Sprite;
    @property(Label)
    level: Label;
    @property(Label)
    price: Label;
    @property(Prefab)
    pricePrefab: Prefab;

    prices: UIMarketLevelPrice[];

    static readonly MAX = 10;

    priceDic: { [key: number]: UIMarketLevelPrice }

    start() {
        // [3]
    }

    public bindData(data: MarketItemData): void {

        if (this.prices == null || this.prices.length == 0) {
            this.prices = [];
            for (let i = 0; i < UIMarketItem.MAX; i++) {
                let node = instantiate(this.pricePrefab);
                let item = node.getComponent(UIMarketLevelPrice);
                this.prices.push(item);
                item.bindData((i + 1), 0);

                node.setParent(this.contents);
            }
        }
        if (data == null) {
            for (let i = 0; i < this.prices.length; i++) {
                this.prices[i].bindData((i + 1), 0)
            }
        } else {
            this.title.string = data.name;
            // this.itemName = ItemName[data.name.trim()];
            // this.icon.spriteFrame = ResourcesHelper.GetItemIcon(data.name);
            // if (this.priceDic == null) this.priceDic = {};

            for (let i = 0; i < this.prices.length; i++) {

                let key = "" + (i + 1);
                let level = (i + 1);
                if (data.priceDic[key] != null) {
                    this.prices[i].bindData(level, +data.priceDic[key])
                }
                else {
                    this.prices[i].bindData(level, 0)
                }
            }
        }


    }
}
