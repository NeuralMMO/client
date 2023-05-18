import { ActionsDB, AttackDB, HistoryDB } from "../data/Packet";
import { Actions } from "./Actions";
import { Component } from "./Component";



export class Attack extends Component {
    target: number;
    style: string;

}


export class History extends Component {

    damage: number;
    timeAlive: number;
    attack: AttackDB;
    actions: Actions;

    public UpdateHistory(data: HistoryDB): void {
        
        this.damage = data.damage;
        this.timeAlive = data.timeAlive;
        this.attack = data.attack;

        if (data.actions == null) {
            this.actions = null;
        }
        else {
            
            if (this.actions == null) this.actions = new Actions();
            this.actions.Update(data.actions);
        }
    }
}