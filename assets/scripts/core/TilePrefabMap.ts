import { _decorator, Prefab, Sprite, SpriteFrame } from 'cc';

import { ResourceTileType } from './ResourceTileType';
import { TileType } from './TileType';
const { ccclass, property } = _decorator;


@ccclass('TilePrefabMap')
export class TilePrefabMap {

    // 基础类型
    @property({ type: TileType })
    tileType: TileType = TileType.None;

    // 资源类型 
    @property({ type: ResourceTileType })
    resourceType: ResourceTileType = ResourceTileType.None;

    @property(Prefab)
    prefab: Prefab;

    

}
