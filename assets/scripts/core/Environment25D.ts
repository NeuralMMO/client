
import { _decorator, Component, Node, instantiate, Prefab, Sprite, SpriteFrame, Vec3, view, utils, Size, CCObject, CCString, UITransform, math, Widget } from 'cc';
import { Replay } from '../data/Replay';
import { EntityView } from '../entity/EntityView';
import { ResourceView } from '../entity/ResourceView';
import { TileView } from '../entity/TileView';
import { ZIndex } from '../entity/ZIndex';
import { GlobalConfig } from './GlobalConfig';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { Utils } from '../utils/Utils';
import { World } from './World';
import { Environment, Tile } from './Environment';
import { ResourceTileType } from './ResourceTileType';
import { TileType } from './TileType';
import { TilePrefabMap } from './TilePrefabMap';
const { ccclass, property } = _decorator;





@ccclass('Environment25D')
export class Environment25D extends Environment {


    @property(Node)
    entityLayer: Node = null!;

    @property(Node)
    treeLayer: Node = null!;

    @property({ type: [TilePrefabMap] })
    prefabs: TilePrefabMap[] = [];


    tilePrefabMap: { [key: number]: TilePrefabMap } = {};
    resPrefabMap: { [key: number]: TilePrefabMap } = {};

    // 全局资源 
    public resources: { [key: string]: TileView };
    // 失效的资源 
    public inactiveResources: { [key: string]: TileView };

    public posZIndexMap: { [key: string]: TileView };


    start() {

    }


    // 重播不需要重置地图 
    public Init(data: Replay): void {
        this.InitPrefabs();
        this.InitConfig(data);
        this.InitTiles(data.map);

        window["env"] = this;
        //  不分帧耗时 2S
    }


    private InitPrefabs() {

        if (this.prefabs.length > 0) {

            for (let i = 0; i < this.prefabs.length; i++) {

                let data = this.prefabs[i];

                if (data.resourceType != ResourceTileType.None) {
                    let key = data.resourceType;

                    if (this.resPrefabMap[key] == null) {
                        this.resPrefabMap[key] = data;
                    }
                    else {
                        console.log("Init  prefabs Error ,Key " + key + " is already in map");
                    }
                }
                else if (data.tileType != TileType.None) {
                    let key = data.tileType;

                    if (this.tilePrefabMap[key] == null) {
                        this.tilePrefabMap[key] = data;
                    }
                    else {
                        console.log("Init  prefabs Error ,Key " + key + " is already in map");
                    }
                }
            }
        }
    }

    public InitConfig(data: Replay): void {

        this.tileNodes = {};
        this.tiles = {};
        this.resources = {};
        this.inactiveResources = {};
        // GlobalConfig.PosZIndexMap = {};

        GlobalConfig.TilePixelSize = GlobalConfig.TILE_SIZE_25D;
        const map = data.map;
        const mapSize = map.length;

        GlobalConfig.MapSize = mapSize;
        // 块块的像素宽高 
        GlobalConfig.RhombusSize = new Size(GlobalConfig.TilePixelSize.x, Math.round(GlobalConfig.TilePixelSize.x * Math.sin(GlobalConfig.RADIAN)));
        // 菱形长，高 的一半    像素
        GlobalConfig.HRhombusSize = new Size(GlobalConfig.RhombusSize.x / 2, GlobalConfig.RhombusSize.y / 2);
        //  地图像素
        GlobalConfig.MapPixelSize = new Size(mapSize * GlobalConfig.HRhombusSize.x * 2, mapSize * GlobalConfig.HRhombusSize.y * 2);

        GlobalConfig.RealMapPixelSize = new Size((mapSize - GlobalConfig.BORDER_SIZE * 2) * GlobalConfig.RhombusSize.x, (mapSize - GlobalConfig.BORDER_SIZE * 2) * GlobalConfig.RhombusSize.y);
        let scale = GlobalConfig.Scales[GlobalConfig.ScaleIndex];
        GlobalConfig.CurScale = scale;
        GlobalConfig.LastScale = scale;
    }


    public InitTiles(map: number[][]): void {
        let size = map.length;

        for (let r = 0; r < size; r++) {
            for (let c = 0; c < size; c++) {
                let key = `${r}_${c}`;
                let type: number = map[r][c];
                // 岩浆 ，空的掠过 
                if (type == TileType.None) continue;
                this.tiles[key] = new Tile(r, c, type);
                let prefabData = this.resPrefabMap[type];
                if (prefabData == null) prefabData = this.tilePrefabMap[type];
                if (prefabData == null) console.log("prefabData == null tileType ", type);

                // 初始化 地面层
                if (prefabData.tileType != TileType.None) {

                    if (this.tilePrefabMap[prefabData.tileType] == null) {
                        console.log("prefab == null tileType ", prefabData.tileType);
                    }
                    let prefab = this.tilePrefabMap[prefabData.tileType].prefab;
                    let tileNode: Node = instantiate(prefab);
                    tileNode.name = `tile_${key}`;
                    this.tileNodes[`${r}_${c}`] = tileNode;
                    tileNode.position = Utils.GetTilePos(r, c);
                    tileNode.setParent(this.terrainLayer);

                }

                // 初始化资源层
                if (prefabData.resourceType != ResourceTileType.None) {

                    if (this.resPrefabMap[prefabData.resourceType] == null) {
                        console.log("prefab == null resourceType ", prefabData.resourceType);
                    }

                    let prefab = this.resPrefabMap[prefabData.resourceType].prefab;

                    let resource: Node = instantiate(prefab);
                    let view: TileView = resource.getComponent(TileView);

                    let zInex: ZIndex = resource.getComponent(ZIndex);
                    zInex.r = r;
                    zInex.c = c;

                    // view.skin.spriteFrame = ResourcesHelper.GetTileSprite(prefabData.resourceType);
                    // 计算出中心点
                    resource.position = Utils.RCToTileOrigin(r, c);
                    this.resources[key] = view;

                    resource.setParent(this.entityLayer);

                    // GlobalConfig.PosZIndexMap[`${r}_${c}`] = { type: type, value: resource.getSiblingIndex() };
                }
            }
        }
    }


    //-------------------  分帧 ，避免卡顿 ---------------
    /*
    lastIndex: number = 0;
    initTilesed: boolean = false;

    update(delta: number): void {
        if (this.initTilesed == false) {
            this.InitTiles(GlobalConfig.replay.map);
        }
    }
    public InitTiles(map: number[][]): void {
        let size = map.length;
        let time = Date.now();

        for (let r = this.lastIndex; r < size; r++) {

            if (r == size -1) this.initTilesed = true;

            for (let c = 0; c < size; c++) {

                let key = `${r}_${c}`;

                let type: number = map[r][c];

                // 岩浆 ，空的掠过 
                if (type == TileType.None) continue;


                this.tiles[key] = new Tile(r, c, type);



                let prefabData = this.resPrefabMap[type];
                if (prefabData == null) prefabData = this.tilePrefab[type];
                // 初始化 地面层
                if (prefabData.tileType != TileType.None) {

                    if (this.tilePrefab[prefabData.tileType] == null) {
                        console.log("prefab == null tileType ", prefabData.tileType);
                    }
                    let prefab = this.tilePrefab[prefabData.tileType].prefab;
                    let tileNode: Node = instantiate(prefab);
                    tileNode.name = `tile_${key}`;
                    this.tileNodes[`${r}_${c}`] = tileNode;
                    tileNode.position = Utils.GetTilePos(r, c);
                    tileNode.setParent(this.terrainLayer);
                }

                // 初始化资源层
                if (prefabData.resourceType != ResourceTileType.None) {

                    if (this.resPrefabMap[prefabData.resourceType] == null) {
                        console.log("prefab == null resourceType ", prefabData.resourceType);
                    }

                    let prefab = this.resPrefabMap[prefabData.resourceType].prefab;

                    let resource: Node = instantiate(prefab);
                    let view: TileView = resource.getComponent(TileView);

                    let zInex: ZIndex = resource.getComponent(ZIndex);
                    zInex.r = r;
                    zInex.c = c;

                    view.skin.spriteFrame = ResourcesHelper.GetTileSprite(prefabData.resourceType);
                    // 计算出中心点
                    resource.position = Utils.RCToTileOrigin(r, c);
                    this.resources[key] = view;
                    resource.setParent(this.entityLayer);
                    GlobalConfig.PosZIndexMap[`${r}_${c}`] = { type: type, value: resource.getSiblingIndex() };
                }
            }

            if (Date.now() - time > 1000/30 * 5 ) {
                this.lastIndex = r;
                break;
            }
        }
    }
*/





    // 处理失效资源 
    public updateInactiveResources(res: number[][]): void {

        let keys = [];
        if (res != null) {
            for (let i = 0; i < res.length; i++) {
                keys.push(`${res[i][0]}_${res[i][1]}`);
            }
        }

        let activeList = [];
        for (let key in this.inactiveResources) {

            if (keys.indexOf(key) == -1) {
                activeList.push(key);
            }
        }

        for (let i = 0; i < activeList.length; i++) {
            this.inactiveResources[activeList[i]].active = true;
            delete this.inactiveResources[activeList[i]];
        }

        for (let i = 0; i < keys.length; i++) {
            let key = keys[i];
            if (this.resources[keys[i]] != null) {
                this.resources[keys[i]].active = false;
                this.inactiveResources[key] = this.resources[keys[i]];
            }
        }

    }





}
