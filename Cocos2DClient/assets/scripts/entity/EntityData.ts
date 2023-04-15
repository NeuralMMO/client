import { History } from "../components/History";
import { Inventory } from "../components/Inventory";
import { LoadoutGroup } from "../components/LoadoutGroup";
import { Metrics } from "../components/Metrics";
import { ResourceGroup } from "../components/ResourceGroup";
import { SkillGroup } from "../components/SkillGroup";
import { Status } from "../components/Status";
import { BaseDB, HistoryDB, InventoryDB, MetricsDB, NPCPacketDB, PlayerPacketDB, ResourceDB, SkillsDB, StatusDB } from "../data/Packet";



// Entity 实际为数据集合 

export class EntityData {

    public alive: boolean; //  true,
    public entID: number; // 38, agent ID 
    public annID: number; //4,

    
    // base  
    public r: number = NaN; // 72, x 
    public c: number = NaN; //143, y 
    public name: string;//  Neural_38,
    public color: string;//#8000ff,
    public population: number; //4,
    public self: number; //1;

    public status: Status;
    public skills: SkillGroup;
    public resources: ResourceGroup;
    public history: History;
    public inventory:Inventory;

    public metrics: Metrics;
    public teamName: string;
    public level: number;
    public  item_level: number;// 所有装备的等级之和, int
    // move 
    public oR: number;
    public oC: number;

    
    public UpdateEntityData(data: PlayerPacketDB | NPCPacketDB): void {

        if (data.base != null) {
            this.UpdateBase(data.base);
        }

        this.alive = data.alive;
        this.annID = data.annID;
        this.entID = data.entID;
        
        this.UpdateSkills(data.skills);
        this.UpdateStatus(data.status);
        this.UpdateResources(data.resource);
        this.UpdateHistory(data.history);
        this.UpdateMetrics(data.metrics);
        this.UpdateInventory(data.inventory)
    }

    public UpdateStatus(status: StatusDB): void {
        this.status.Update(status);
    }
    public UpdateSkills(skills: SkillsDB): void {
        this.skills.UpdateSkills(skills);
    }
    public UpdateResources(resource: { [key: string]: ResourceDB }): void {
        this.resources.UpdateResources(resource)
    }


    public UpdateMetrics(metrics: MetricsDB): void {
        if (metrics) {
            this.metrics == null && (this.metrics = new Metrics());
            this.metrics.bindData(metrics);
        }
    }

    public UpdateHistory(data:HistoryDB): void {
        this.history.UpdateHistory(data);
    }

    public  UpdateInventory(data:InventoryDB):void{
        this.inventory.Update(data);
    }

    public UpdateBase(base: BaseDB): void {

        if (this.r != null && this.r != NaN) {

            this.oR = this.r;
            this.oC = this.c;
        }

        this.r = base.r;// number; // 72,
        this.c = base.c;// number; //143,
        this.name = base.name;//+ `_${this.r}_${this.c}`;// string;//  Neural_38,
        this.color = base.color;// string;//#8000ff,
        this.population = base.population;// number; //4,
        this.self = base.self;// number; //1;
        // this.teamName = base.name.split("_")[0];
        this.teamName = base.name.slice(0,base.name.lastIndexOf("_"));
        this.level = base.level;
        this.item_level = base.item_level;
    }

    public InGrid(r: number, c: number): boolean {
        return this.c == c && this.r == r;
    }

    public GetPopulation(): number {
        return this.population;
    }

    public GetTeamName(): string {
        return this.teamName;
    }



}