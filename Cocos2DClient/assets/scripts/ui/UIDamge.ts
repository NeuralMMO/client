
import { _decorator, Component, Node, Label, tween, easing, Vec3, math, UITransform, Color } from 'cc';
import { Entity } from '../entity/Entity';
import { EntityView } from '../entity/EntityView';
import { GlobalConfig } from '../core/GlobalConfig';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIDamge
 * DateTime = Wed Apr 27 2022 22:50:28 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIDamge.ts
 * FileBasenameNoExtension = UIDamge
 * URL = db://assets/scripts/ui/UIDamge.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIDamge')
export class UIDamge extends Component {

    @property(Label)
    text: Label;

    follow: EntityView;

    start() {

        let toPos = new Vec3(math.randomRangeInt(-20, 20), 80, 0).add(this.text.node.position);
        
        let node = this.node;

        let time = 0.6 / GlobalConfig.SpeedRate;

        time = math.clamp(time, 0.1, 0.6) / 2;

        let self = this;

        // tween(this.text.node).delay(GlobalConfig.FlyTime).to(time, { position: toPos }, {
            tween(this.text.node).to(time, { position: toPos }, {
            easing: easing.sineInOut
        }).to(time, {}, {
            onComplete: () => {
                node.parent = (null);
                node.destroy();
                node = null;
                self.follow = null;
            }
        }).start();
    }


    update(delta: number) {
        if (this.follow != null) {
            let pos = new Vec3(this.follow.node.worldPosition);
            pos = this.node.parent.getComponent(UITransform).convertToNodeSpaceAR(pos);
            pos.z = 0;
            pos.y += 100;
            this.node.position = pos;
        }
    }


    public setText(text: string,color:Color): void {
        this.text.string = text;
        this.text.color = color;
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
