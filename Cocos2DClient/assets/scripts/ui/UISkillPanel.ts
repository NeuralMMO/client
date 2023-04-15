
import { _decorator, Component, Node, Prefab, Label } from 'cc';
import { SkillGroup } from '../components/SkillGroup';
import { SkillsDB } from '../data/Packet';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UISkillPanel
 * DateTime = Sat Aug 20 2022 14:05:20 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UISkillPanel.ts
 * FileBasenameNoExtension = UISkillPanel
 * URL = db://assets/scripts/ui/UISkillPanel.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UISkillPanel')
export class UISkillPanel extends Component {



    @property(Label)
    melee: Label;// = 1, //近战 ，当前设计最大攻击4格 发射
    @property(Label)
    range: Label;// = 2, // 远程 范围 
    @property(Label)
    carving: Label;//= 3,// 砍树
    @property(Label)
    mage: Label;//, // 魔法
    @property(Label)
    fishing: Label;//, // 钓鱼
    @property(Label)
    alchemy: Label;//, //采集水晶
    @property(Label)
    herbalism: Label;//,//采集蘑菇
    @property(Label)
    prospecting: Label;//, //采集矿石


    skillDic: object;

    start() {
        // [3]
    }


    public UpdateData(data: SkillGroup): void {


        let melee = data.GetSkill("melee");// = 1, //近战 ，当前设计最大攻击4格 发射
        this.melee.string = melee != null ? "" + melee.level : "0";

        let range = data.GetSkill("range");// = 2, // 远程 范围 
        this.range.string = range != null ? "" + range.level : "0";

        let carving = data.GetSkill("carving");//= 3,// 砍树
        this.carving.string = carving != null ? "" + carving.level : "0";

        let mage = data.GetSkill("mage");//, // 魔法
        this.mage.string = mage != null ? "" + mage.level : "0";

        let fishing = data.GetSkill("fishing");//, // 钓鱼
        this.fishing.string = fishing != null ? "" + fishing.level : "0";

        let alchemy = data.GetSkill("alchemy");//, //采集水晶
        this.alchemy.string = alchemy != null ? "" + alchemy.level : "0";

        let herbalism = data.GetSkill("herbalism");//,//采集蘑菇
        this.herbalism.string = herbalism != null ? "" + herbalism.level : "0";

        let prospecting = data.GetSkill("prospecting");//, //采集矿石

        this.prospecting.string = prospecting != null ? "" + prospecting.level : "0";
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
