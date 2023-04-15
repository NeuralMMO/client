
import { _decorator, Component, Node, Label, Sprite, SpriteFrame } from 'cc';
import { Tile } from '../core/Environment';
import { BattleEvent, EventManager } from '../event/Event';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TileInfoPanel
 * DateTime = Sun Apr 17 2022 22:20:10 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = TileInfoPanel.ts
 * FileBasenameNoExtension = TileInfoPanel
 * URL = db://assets/scripts/TileInfoPanel.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('TileInfoPanel')
export class TileInfoPanel extends Component {
    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;
    @property(Label)
    tileInfo: Label;

    @property(Label)
    resource: Label;


    @property(Sprite)
    icon: Sprite;

    @property({ type: [SpriteFrame], visible: true, })
    public tiles: SpriteFrame[] = [];

    constructor() {
        super();
        EventManager.view.on(BattleEvent.SelectedTile, this.BinTileInfo, this);
    }
    start() {
        // [3]

    }

    public BinTileInfo(tile: Tile): void {
        if (tile == null) {
            this.node.active = false;
        }
        else {
            this.node.active = true;
            this.icon.spriteFrame = this.tiles[tile.val];
            this.tileInfo.string = "tile_" + tile.r + "_"+ tile.c;
        }

    }

    // update (deltaTime: number) {
    //     // [4]
    // }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/zh/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/zh/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/zh/scripting/life-cycle-callbacks.html
 */
