
import { _decorator, Component, Node, Vec3, tween, Sprite, math, SpriteFrame, Enum } from 'cc';
import { Entity } from '../entity/Entity';
import { GlobalConfig } from '../core/GlobalConfig';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { SkillDir, SkillType } from './SkillType';
const { ccclass, property } = _decorator;



@ccclass('SkillItem')
export class SkillItem extends Component {

    @property(Sprite)
    view: Sprite;

    type: SkillType;

    startPos: Vec3;
    tagetPos: Vec3;
    target: Entity;
    angle: number = NaN;


    Init(type: SkillType, startPos: Vec3, tagetPos: Vec3, target: Entity): void {
        this.type = type;
        this.startPos = startPos;
        this.tagetPos = tagetPos;
        this.target = target;
        this.node.position = this.startPos;
    }



    public getDir(): SkillDir {

        var angle = Math.atan2((this.tagetPos.y - this.startPos.y), (this.tagetPos.x - this.startPos.x));
        let degree = math.toDegree(angle);
        //  right 
        if (degree < 22.5 && degree >= -22.5) {
            return SkillDir.Right;
        }
        if (degree >= 22.5 && degree < 67.5) {
            return SkillDir.RightUp;
        }
        if (degree >= 67.5 && degree < 112.5) {
            return SkillDir.Up;
        }
        if (degree >= 112.5 && degree < 157.5) {
            return SkillDir.LeftUp;
        }
        //
        if (degree >= 157.5 || degree < -157.5) {
            return SkillDir.Left;
        }
        if (degree >= -157.5 && degree < -112.5) {
            return SkillDir.LeftDown;
        }

        if (degree >= -112.5 && degree < -67.5) {
            return SkillDir.Down;
        }

        if (degree >= -67.5 && degree < -22.5) {
            return SkillDir.RightDown;
        }

    }

    
    public clear(): void {
        this.node && this.node.destroy();
        this.target && this.target.onHit(this.type);
        this.target = null;

    }

    public  flyToTaget()
    {
        let flyTime = GlobalConfig.BASE_FLY_TIME / GlobalConfig.SpeedRate;
        var angle = Math.atan2((this.tagetPos.y - this.startPos.y), (this.tagetPos.x - this.startPos.x));

        let degree = math.toDegree(angle);

        if (degree < 0) degree = 360 + degree;

        this.view.node.setRotationFromEuler(new Vec3(0, 0, (degree - 90)));

        let self = this;

        tween(this.node).to(flyTime, { position: this.tagetPos }, {
            onComplete: () => {
                self.clear();
            }
        }).start();
    }

}

