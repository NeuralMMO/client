
import { _decorator, Component, Node, Label } from 'cc';
import { ResourceGroup } from '../components/ResourceGroup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIProptyPanel
 * DateTime = Sat Aug 20 2022 14:07:30 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIProptyPanel.ts
 * FileBasenameNoExtension = UIProptyPanel
 * URL = db://assets/scripts/ui/UIProptyPanel.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIPropertyPanel')
export class UIPropertyPanel extends Component {

    @property(Label)
    health: Label;

    @property(Label)
    food: Label;

    @property(Label)
    water: Label;

    start() {
        // [3]
    }

    UpdateData(data: ResourceGroup): void {
        let health = data.GetResource("health");
        this.health.string = health != null ? "" + Math.round( health.val) : "0";
        let food = data.GetResource("food");
        this.food.string = food != null ? "" +  Math.round(food.val ) : "0";
        let water = data.GetResource("water");
        this.water.string = water != null ? "" +  Math.round(water.val)  : "0";
    }
}

