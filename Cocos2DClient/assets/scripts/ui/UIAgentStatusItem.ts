
import { _decorator, Component, Node, Label, CCString } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIStatusItem
 * DateTime = Sun Apr 24 2022 07:07:04 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIStatusItem.ts
 * FileBasenameNoExtension = UIStatusItem
 * URL = db://assets/scripts/ui/UIStatusItem.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIAgentStatusItem')
export class UIAgentStatusItem extends Component {
    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;
    @property(Label)
    labelLabel: Label;

    @property(Label)
    valueLabel: Label;

    _label: string = "label";
    set label(value) {
        this._label = value;
        this.labelLabel.string = value;
    }
    get label() :string{ return this._label; }


    _value: string= "0";
    set value(value) {
        this._value = value;
        this.valueLabel.string = value;
    }
    get value() :string{ return this._value; }


    start() {

    }

    // update (deltaTime: number) {
    //     // [4]
    // }
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
