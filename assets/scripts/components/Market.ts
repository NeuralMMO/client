import { EquipmentDB, ItemDB, MarketDB, MarketItemDB } from "../data/Packet";
import { Component } from "./Component";
import { ItemComponent } from "./ItemComponent";

export class Market extends Component {

    itemDic: { [key: string]: MarketItemDB };

    constructor() {
        super();
        this.itemDic = {};
    }

    public Update(data: MarketDB): void {

        for (let key in data.market) {
            this.itemDic[key] = data.market[key];
        }

        for (let key in this.itemDic) {
            if (data.market[key] == null)
                delete this.itemDic[key];
        }

    }
}