
import { _decorator, Component, Node, instantiate, Sprite, Vec3, view, Prefab, SpriteFrame, Size } from 'cc';
import { Replay } from '../data/Replay';
import { Environment, Tile } from './Environment';
import { GlobalConfig } from './GlobalConfig';
import { Utils } from '../utils/Utils';
import { World } from './World';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = Environment2D
 * DateTime = Mon Apr 18 2022 22:26:22 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = Environment2D.ts
 * FileBasenameNoExtension = Environment2D
 * URL = db://assets/scripts/Environment2D.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('Environment2D')
export class Environment2D extends Environment {



    start() {


    }

    //2D 
    public Init(data: Replay): void {
        this.tileNodes = {};
        this.tiles = {};
       

        GlobalConfig.TilePixelSize = GlobalConfig.Use25D ? GlobalConfig.TILE_SIZE_25D : GlobalConfig.TILE_SIZE;

        const map = data.map;

        const mapSize = map.length;

        GlobalConfig.MapSize = mapSize;

        GlobalConfig.MapPixelSize.width = mapSize * GlobalConfig.TilePixelSize.width;

        GlobalConfig.MapPixelSize.height = mapSize * GlobalConfig.TilePixelSize.height;


        let size = view.getVisibleSize();

        let wScale = size.width / (GlobalConfig.MapPixelSize.width);

        let hScale = size.height / (GlobalConfig.MapPixelSize.height);

        let scale = Math.min(wScale, hScale);

        GlobalConfig.CurScale = scale;
        GlobalConfig.MinScale = GlobalConfig.CurScale * 0.8;
        GlobalConfig.ScaleStep = GlobalConfig.CurScale * 0.1;


        for (let r = 0; r < mapSize; r++) {

            for (let c = 0; c < mapSize; c++) {

                let key = `${r}_${c}`;

                this.tiles[key] = new Tile(r, c, map[r][c]);

                if (map[r][c] == 0) continue;

                let node: Node = instantiate(this.tilePrefab);

                node.name = `tile_${key}`;

                this.tileNodes[`${r}_${c}`] = node;

                let sp: Sprite = node.getComponent(Sprite);

                // sp.spriteFrame = this.tileSprites[map[r][c]];

                sp.node.position = Utils.GetTilePos(r, c);

                this.terrainLayer.addChild(node);
            }
        }
    }


}

