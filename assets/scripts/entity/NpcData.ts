import { LoadoutGroup } from "../components/LoadoutGroup";
import { ResourceGroup } from "../components/ResourceGroup";
import { SkillGroup } from "../components/SkillGroup";
import { Status } from "../components/Status";
import { EntityData } from "./EntityData";
import { History } from "../components/History";
import { NPCResources } from "../components/NPCResources";
import { NpcSkills } from "../components/NpcSKills";
import { Inventory } from "../components/Inventory";

export class NpcData extends EntityData {

    constructor(data:any) {
        super();
        this.status = new Status();
        this.skills = new NpcSkills();
        this.resources = new NPCResources();
        this.history = new History();
        this.inventory = new Inventory();
        
        this.UpdateEntityData(data);
    }



}