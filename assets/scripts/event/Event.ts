import { EventTarget } from "cc";


export class BattleEvent {
    
    public static SelectedEntity: string = "SelectedEntity";

    public static SelectedTile: string = "SelectedTile";

    public static AddTeam: string = "AddTeam";

    public static RemoveTeam: string = "RemoveTeam";

    public static LookAtEntity:string = "LookAtEntity";

    public static RemoveEntity:string = "RemoveEntity";

    public static AddEntity:string = "AddEntity";

    public static SelectedTeam:string = "SelectedTeam";

    public static PacketChange:string = "PacketChange";

    public static StepChange:string = "StepChange";


    public static  FogChange:string = "FogChange";
    
}



export class EventManager {


    public static view: EventTarget = new EventTarget();
    public static battle: EventTarget = new EventTarget();

    public static Init() {
        if( this.view  == null)this.view = new EventTarget();
        if( this.battle  == null)  this.battle = new EventTarget();
    }
}

export  class   PoisonEventData
{
    lR:number;
    rR:number;
    topC:number;
    bottomC: number;

}