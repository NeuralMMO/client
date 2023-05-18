import { Component } from "./Component";


export { Loadout, LoadoutGroup }

class Loadout extends Component {

    public level: number;
    public color: string;
}

class LoadoutGroup {

    public loadouts:{[key:string]:Loadout};
    
}