import { Profession } from "../core/Profession";
import { Component } from "./Component";

export { SkillGroup, Skill }



class Skill extends Component {

    public level: number = 0;
    public exp: number = 0;

    public UpdateSkill(data: { level: number, exp: number }): void {
        this.level = data.level;
        this.exp = data.exp;
    }

    // 小于  return  -1
    public compare(other: Skill): number {

        if (other.level < this.level) return 1;
        if (other.level == this.level && other.exp < this.exp) return 1;
        if (other.level == this.level && other.exp == this.exp) return 0;
        
        return -1;
    }
}


//  技能组件 

class SkillGroup extends Component {

    public skills: { [key: string]: Skill };
    public level: number;

    constructor() {
        super();
        this.skills = {};
        this.level = 0;
    }

    public AddSkill(name: string): Skill {
        if (this.skills[name] == null) {
            this.skills[name] = new Skill();
        }
        return this.skills[name];
    }

    public UpdateSkill(name: string, data: { level: number, exp: number }): Skill {


        if (this.skills[name] == null) return;
        //     this.skills[name] = new Skill();
        // }
        this.skills[name].level = data.level;
        this.skills[name].exp = data.exp;

        return this.skills[name];
    }

    public UpdateSkills(skills): void {

        for (let key in skills) {
            this.UpdateSkill(key, skills[key]);
        }
        let removes: string[] = [];

        for (let key in this.skills) {
            if (skills[key] == null)
                removes.push(key);
        }
        for (let i = 0; i < removes.length; i++) {
            delete this.skills[removes[i]];
        }
    }

    public UpdateUI() {

    }

    public GetSkill(name: string): Skill {

        return this.skills && this.skills[name]
    }

    // 获得职业 
    public getProfession(): Profession {
        return Profession.None;
    }


}