import { Resource, ResourceGroup } from "./ResourceGroup";


export class PlayerResources extends ResourceGroup {

    public health: Resource;
    public food: Resource;
    public water: Resource;

    constructor() {
        super();

        this.health = this.AddResource("health");
        this.food   = this.AddResource("food");
        this.water  = this.AddResource("water");
    }

}