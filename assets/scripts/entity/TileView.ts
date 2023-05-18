
import { _decorator, Component, Node, Sprite, SpriteFrame, CCInteger, randomRange } from 'cc';
import { GlobalConfig } from '../core/GlobalConfig';
import { ResourceTileType } from '../core/ResourceTileType';
import { TileType } from '../core/TileType';
import { Utils } from '../utils/Utils';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = Tile
 * DateTime = Wed Aug 17 2022 17:22:47 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = Tile.ts
 * FileBasenameNoExtension = Tile
 * URL = db://assets/scripts/entity/Tile.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('TileView')
export class TileView extends Component {

    @property(Sprite)
    skin: Sprite;

    @property([SpriteFrame])
    inactiveSprites: SpriteFrame[] = [];

    @property([SpriteFrame])
    activeSprites: SpriteFrame[] = [];

    inactiveSprite: SpriteFrame;
    activeSprite: SpriteFrame;


    //  base  tile 
    @property({ type: TileType })
    tileType: TileType;

    @property({ type: ResourceTileType })
    resourceType: ResourceTileType;

    _index: number = 0;

    public set index(val: number) {

        this.inactiveSprite = this.inactiveSprites[val];
        this.activeSprite = this.activeSprites[val];
        this._index = val;
    }

    public set active(value: boolean) {

        if (this.activeSprite != null) {
            this.skin.spriteFrame = value ? this.activeSprite : this.inactiveSprite;
        }
        else {
            this.node.active = value;
        }
    }

    start() {
        if (this.activeSprites != null) {
            this.index = Math.floor(  randomRange(0, this.activeSprites.length));
            this.node.active && (this.skin.spriteFrame = this.activeSprite);
        }
    }

}

