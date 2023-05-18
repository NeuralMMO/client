import { instantiate, Sprite, UITransform, Vec3, view, Node, Prefab, SpriteFrame, Size, Component, _decorator, ccenum, Enum } from "cc";
import { Replay } from "../data/Replay";

const { ccclass, property } = _decorator;








// Lava = 0,
// Water = 1,
// Grass = 2,
// Scrub = 3,
// Forest = 4,
// Stone = 5,
// Orerock = 6,

export class Tile { 

    public r: number;
    public c: number;
    public val: number;

    constructor(r: number, c: number, val: number) {
        this.r = r;
        this.c = c;
        this.val = val;
    }
}


@ccclass("Environment")
export class Environment extends Component {


    @property(Prefab)
    tilePrefab: Prefab = null!;


    @property({ type: Node, visible: true, })
    public terrainLayer: Node;

    public tileNodes: { [key: string]: Node };
    public tiles: { [key: string]: Tile };



    public Init(data: Replay): void { }

    public GetNode(c: number, r: number) {
        return this.tileNodes && this.tileNodes[`${r}_${c}`];
    }

    public GetTile(r: number, c: number): Tile {
        return this.tiles && this.tiles[`${r}_${c}`];
    }

    public updateInactiveResources(res: number[][]): void { }


    public Clear(): void {

    }


}