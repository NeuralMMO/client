import { LoadoutGroup } from "../components/LoadoutGroup";
import { ResourceGroup } from "../components/ResourceGroup";
import { SkillGroup } from "../components/SkillGroup";
import { Status } from "../components/Status";
import { EntityData as EntityData } from "./EntityData";
import { History } from "../components/History";
import { PlayerResources } from "../components/PlayerResources";
import { PlayerSkills } from "../components/PlayerSkills";
import { Inventory } from "../components/Inventory";

export class PlayerData extends EntityData {

    constructor(data:any) {
        super();
        this.status = new Status();
        this.skills = new PlayerSkills();
        this.resources = new PlayerResources();
        this.history = new History();
        this.inventory = new Inventory();

        this.UpdateEntityData(data);
    }

}