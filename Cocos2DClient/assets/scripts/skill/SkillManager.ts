
import { _decorator, Component, Node, Vec3, Prefab, instantiate, EventMouse, EventTouch, math, UITransform, resources } from 'cc';
import { Entity } from '../entity/Entity';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { SkillItem } from './SkillItem';
import { SkillType } from './SkillType';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SkillManager
 * DateTime = Thu Apr 28 2022 12:36:41 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = SkillManager.ts
 * FileBasenameNoExtension = SkillManager
 * URL = db://assets/scripts/skill/SkillManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */


@ccclass('SkillPrefabMap')
export class skillPrefabMap {
    @property({ type: SkillType })
    type: SkillType;
    @property(Prefab)
    prefab: Prefab = null!;
}

@ccclass('SkillManager')
export class SkillManager extends Component {



    showList: SkillType[] = [
        SkillType.melee,
        SkillType.range,
        SkillType.mage, // 魔法
        /*
        SkillType.carving,// 砍树
        SkillType.fishing,
        SkillType.hunting,
        SkillType.alchemy, //采集水晶
        SkillType.herbalism,//采集蘑菇
        SkillType.water,// 采集水， 不用显示
        SkillType.prospecting, //采集矿石
        */
    ];

    @property({ type: [skillPrefabMap] })
    prefabMap: skillPrefabMap[] = [];

    _skillPrefabMap: { [key: number]: Prefab };

    get skillPrefabMap(): { [key: number]: Prefab } {
        if (this._skillPrefabMap == null) {
            this._skillPrefabMap = {};

            for (let i = 0; i < this.prefabMap.length; i++) {

                let prefab = this.prefabMap[i];

                this._skillPrefabMap[prefab.type] = prefab.prefab;
            }
        }
        return this._skillPrefabMap;
    }

    @property(Prefab)
    basetPrefab: Prefab;

    public static instance: SkillManager;

    start() {

        SkillManager.instance = this;
        // 加载资源 
        resources.loadDir(ResourcesHelper.SkillRoot + "/");
        resources.loadDir(ResourcesHelper.SkillRoot + "/melee/");
    }

    // 技能具体实现是在 SkillItem
    public showSkill(type: SkillType, startPos: Vec3 = null, targetPos: Vec3 = null, target: Entity = null): void {

        if (this.showList.indexOf(type) == -1) return;

        let prefab = this.basetPrefab;

        if (this.skillPrefabMap[type] != null) {
            prefab = this.skillPrefabMap[type];
        }

        let node = instantiate(prefab);
        node.getComponent(SkillItem).Init(type, startPos, targetPos, target);
        node.setParent(this.node);
    }


    /*
    // 测试近战 
    public testMelee(event: EventMouse | EventTouch): void {
        console.log("testMelee");


        let type: SkillType = SkillType.melee;
        let startPos: Vec3 = new Vec3(0, 0, 0);
        let touch = event.getLocation();
        let tagetPos: Vec3 = new Vec3(touch.x, touch.y, 0);

        tagetPos = this.node.getComponent(UITransform).convertToNodeSpaceAR(tagetPos);

        let target: Entity = null;

        let node = instantiate(this.item);

        node.getComponent(SkillItem).Init(type, startPos, tagetPos, target);

        node.setParent(this.node);
    }


    // 测试魔法
    public testMage(event: EventMouse | EventTouch): void {

        let type: SkillType = SkillType.mage;
        let startPos: Vec3 = new Vec3(0, 0, 0);

        let touch = event.getLocation();

        let tagetPos: Vec3 = new Vec3(touch.x, touch.y, 0);;

        tagetPos = this.node.getComponent(UITransform).convertToNodeSpaceAR(tagetPos);;

        var angle = Math.atan2((tagetPos.y - startPos.y), (tagetPos.x - startPos.x));
        let degree = math.toDegree(angle);
        if (degree < 0) degree = 360 + degree;

        console.log(Math.round(startPos.x), Math.round(startPos.y), Math.round(tagetPos.x), Math.round(tagetPos.y), angle, "toDegree " + (degree - 90));

        let target: Entity = null;

        let node = instantiate(this.item);

        node.getComponent(SkillItem).Init(type, startPos, tagetPos, target);
        node.setParent(this.node);
    }
    */

}


