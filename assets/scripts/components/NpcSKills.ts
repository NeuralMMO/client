import { Profession } from "../core/Profession";
import { Skill, SkillGroup } from "./SkillGroup";


export { NpcSkills }

class NpcSkills extends SkillGroup {

    public melee: Skill;
    public range: Skill;
    public mage: Skill;



    constructor() {
        super();

        this.skills = {};
        this.melee = this.AddSkill("melee");
        this.range = this.AddSkill("range");
        this.mage = this.AddSkill("mage");
    }

    public getProfession(): Profession {
        // let max = this.melee;
        // if (this.range.level > max.level) {
        //     max = this.range;
        // }
        // if (this.mage.level > max.level) {
        //     max = this.mage;
        // }
        // if (max.level <= 1) return Profession.None;

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

        // if (max.level <= 1) return Profession.None;
        if (max == this.melee) return Profession.Warrior;
        if (max == this.range) return Profession.Archer;
        if (max == this.mage) return Profession.Mage;

        return Profession.None;
    }
}