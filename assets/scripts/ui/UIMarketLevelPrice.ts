
import { _decorator, Component, Node, Label, CCInteger } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIMarketLevelPrice
 * DateTime = Mon Aug 22 2022 15:23:22 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIMarketLevelPrice.ts
 * FileBasenameNoExtension = UIMarketLevelPrice
 * URL = db://assets/scripts/ui/UIMarketLevelPrice.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIMarketLevelPrice')
export class UIMarketLevelPrice extends Component {
    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;

    @property(Label)
    price: Label;
    @property(Label)
    level: Label;
    @property(CCInteger)
    levelNum:number = 0;


    start() {
        // [3]
    }

    bindData(level: number, price: number) {
        this.levelNum = level;

        this.level.string = "Lv." + level;
        this.price.string = price<= 0 ?"-":"" + price;
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
