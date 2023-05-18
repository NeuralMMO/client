import { Component } from "./Component";


export class Status extends Component {
    freeze: number;

    public Update(data: any): void {
        if (data.freeze != null) {
            this.freeze = data.freeze;
        }
    }
}