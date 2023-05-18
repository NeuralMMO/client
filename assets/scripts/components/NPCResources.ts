import { Resource, ResourceGroup } from "./ResourceGroup";

export class NPCResources extends ResourceGroup {

    public health: Resource;

    constructor() {
        super();

        this.health = this.AddResource("health");

    }

}