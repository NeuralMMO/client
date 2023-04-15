import { Profession } from "../core/Profession";
import { Skill, SkillGroup } from "./SkillGroup";


export { PlayerSkills }

//数据
class PlayerSkills extends SkillGroup {

    public melee: Skill;
    public range: Skill;
    public mage: Skill;
    public defense: Skill;
    public fishing: Skill;
    public hunting: Skill;

    public food: Skill;//食物, 不用显示
    public carving: Skill;//砍树
    public alchemy: Skill;//采集水晶
    public herbalism: Skill; //采集蘑菇
    public water: Skill;//水， 不用显示
    public prospecting: Skill;//采集矿石

    /*
    constitution = 0,
    melee = 1, //近战 ，当前设计最大攻击4格 发射
    range = 2, // 远程 范围 
    carving = 3,// 砍树
    mage, // 魔法
    defense,// 防御 格挡
    fishing, // 钓鱼
    hunting, // 狩猎
    alchemy, //采集水晶
    herbalism,//采集蘑菇
    water,// 采集水， 不用显示
    prospecting, //采集矿石
*/

    constructor() {
        super();

        this.skills = {};
        this.melee = this.AddSkill("melee");
        this.range = this.AddSkill("range");
        this.mage = this.AddSkill("mage");
        this.defense = this.AddSkill("defense");
        this.fishing = this.AddSkill("fishing");
        this.hunting = this.AddSkill("hunting");


        this.food = this.AddSkill("food");
        this.carving = this.AddSkill("carving");
        this.alchemy = this.AddSkill("alchemy");
        this.herbalism = this.AddSkill("herbalism");
        this.water = this.AddSkill("water");
        this.prospecting = this.AddSkill("prospecting");


    }

    // 根据等级获得职业 
    public getProfession(): Profession {
        let max = this.melee;

        //// 判断条件在 compare
        let compare = max.compare(this.range);

        if (compare == -1) {
            max = this.range;
        }
        compare = max.compare(this.mage)
        if (compare == -1) {
            max = this.mage;
        }

        // 经验，等级 全相同 
        if (compare == 0) return Profession.None;

        if (max == this.melee) return Profession.Warrior;
        if (max == this.range) return Profession.Archer;
        if (max == this.mage) return Profession.Mage;

        return Profession.None;
    }

}