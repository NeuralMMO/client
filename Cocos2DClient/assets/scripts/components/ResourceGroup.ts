import { math } from "cc";
import { Component } from "./Component";

export { Resource, ResourceGroup }


class Resource extends Component {

    public val: number;
    public max: number;

    public get Percentage() { return math.clamp(this.val / this.max, 0, 1); };


    public OnEnable(): void {

    }

    public OnDisable(): void {

    }

    public UpdateResource(data: { val: number, max: number }) {
        this.max = data.max;
        this.val = data.val;
    }

}

// 资源组件
class ResourceGroup extends Component {

    public resources: { [key: string]: Resource };

    constructor() {
        super();
        this.resources = {};
    }


    public AddResource(name: string): Resource {
        if (this.resources[name] == null) {
            this.resources[name] = new Resource();
        }
        return this.resources[name];
    }

    public UpdateResources(resources) {

        for (let key in resources) {

            if (this.resources[key] == null) {
                this.resources[key] = new Resource();
            }
            this.resources[key].UpdateResource(resources[key]);
        }
    }

    public GetResource(name: string): Resource {
        return this.resources && this.resources[name];
    }
}

