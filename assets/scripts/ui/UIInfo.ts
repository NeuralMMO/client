
import { _decorator, Component, Node, Label } from 'cc';
import { MetricsDB } from '../data/Packet';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIInfo
 * DateTime = Mon Aug 22 2022 16:01:18 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIInfo.ts
 * FileBasenameNoExtension = UIInfo
 * URL = db://assets/scripts/ui/UIInfo.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIInfo')
export class UIInfo extends Component {
    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;

    @property(Label)
    damageTaken: Label;

    @property(Label)
    defeats: Label;


    start() {
        // [3]
    }

    UpdateData(data: MetricsDB): void {
        //this.damageTaken.string = "" + data.DamageTaken;
        //this.defeats.string = "" + data.PlayerDefeats;
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
