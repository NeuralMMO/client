
import { _decorator, Component, Node, CCObject, Sprite, Texture2D, SpriteFrame, resources, JsonAsset, Vec3, TiledMap, TiledTile, Vec2, instantiate, Prefab, Scene, view, input, Input, Event, EventTouch, Label, EventMouse, Slider, CCBoolean, CCFloat, UITransform, math, Size, CCString, director, EventHandler } from 'cc';


import { PacketDB } from '../data/Packet';
import { Replay } from '../data/Replay';
import { Entity } from '../entity/Entity';
import { EntityView } from '../entity/EntityView';
import { EntityManager } from './EntityManager';
import { Environment, Tile } from './Environment';
import { Environment25D } from './Environment25D';
import { Environment2D } from './Environment2D';
import { BattleEvent, EventManager, PoisonEventData } from '../event/Event';
import { GlobalConfig } from './GlobalConfig';
import { StatisticalManager } from './StatisticalManager';
import { Utils } from '../utils/Utils';
const { ccclass, property } = _decorator;



@ccclass('World')
export class World extends Component {

    static readonly PLAYER_DEATH_FOG = 240; // # 缩毒开始时间
    static readonly PLAYER_DEATH_FOG_FINAL_SIZE = 15; // # 最后的安全区的半径，安全区是个正方形
    static readonly PLAYER_DEATH_FOG_SPEED = 1 / 16; // # 缩毒速度，每16步缩一格

    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;
    //---------- Layer --------------
    @property({ type: Node, visible: true, })
    public terrainLayer: Node;

    @property({ type: Node, visible: true, })
    public entityLayer: Node;

    @property({ type: Node, visible: true, })
    public touchLayer: Node;

    @property({ type: Node, visible: true, })
    public waterLayer: Node;

    @property({ type: Node, visible: true, })
    public borderLayer: Node;

    @property({ type: Node, visible: true, })
    public treeLayer: Node;

    @property({ type: Node, visible: true, })
    public headInfoLayer: Node;


    @property(Prefab)
    characterPrefab: Prefab;

    @property(Prefab)
    headPrefab: Prefab;

    @property(CCBoolean)
    use25D: boolean = true;

    replayStep: number = 0;
    totalStep: number = 0;


    lookAtNode: Node;
    packets: PacketDB[];
    curEntity: EntityView;

    // @property(Environment)
    env: Environment;

    constructor() {
        super();
        EntityManager.instance = new EntityManager();
        StatisticalManager.instance = new StatisticalManager();
    }

    start() {

        this.init();
    }


    public init(): void {
        GlobalConfig.Use25D = this.use25D;

        if (GlobalConfig.Use25D) {
            this.env = this.node.getComponent(Environment25D);
        }
        else {
            this.env = this.node.getComponent(Environment2D);
        }

        this.parseReplayData(GlobalConfig.replay);

        this.env.Init(GlobalConfig.replay);

        this.node.setScale(GlobalConfig.CurScale, GlobalConfig.CurScale, GlobalConfig.CurScale);

        EntityManager.instance.init(this, this.characterPrefab, this.headPrefab);

        this.InitTouch();

        EventManager.Init();

        EventManager.view.on(BattleEvent.LookAtEntity, this.LookAt, this);

        EventManager.view.on(BattleEvent.RemoveEntity, this.OnRemoveEntity, this);
        EventManager.view.on(BattleEvent.SelectedTeam, this.OnSelectedTeam, this);

    }

    private parseReplayData(replay: Replay): void {

        this.packets = replay.packets;
        this.totalStep = this.packets.length;
        this.replayStep = 0;

    }


    public LookAt(lookAt): void {

        this.lookAtNode = (lookAt != null && lookAt.entity) ? lookAt.entity.view.node : null;
        this.UpdateLookAtNode(1);
    }


    public OnRemoveEntity(entity: Entity): void {

        if (this.lookAtNode && this.lookAtNode == entity.view.node) {
            this.lookAtNode = null;
        }

    }

    public OnSelectedTeam(team: string): void {

    }

    public Render(): void {


        if (this.packets == null) return;

        if (GlobalConfig.Pause == false) {

            if (this.replayStep >= this.packets.length) this.replayStep = 0;
        }

        let packet: PacketDB = this.packets[this.replayStep];
        EventManager.view.emit(BattleEvent.PacketChange, packet);

        EventManager.view.emit(BattleEvent.StepChange, this.replayStep);

        EntityManager.instance.executePacket(packet, this.replayStep);

        this.env.updateInactiveResources(packet.resource);

        this.replayStep++;

        if (this.replayStep >= this.packets.length) {
            GlobalConfig.Pause = true;
            GlobalConfig.isEnd = true;
        }
        else {
            GlobalConfig.isEnd = false;
        }

    }


    last: number = 0;
    updateDelay: number = 0
    // 每帧 （1/60 s）更新  
    public update(delta: number): void {

        // if (Date.now() - this.updateDelay > 60) {
        //     this.updateDelay = Date.now();
        //     EntityManager.instance.updateZindex();
        // }

        if (this.packets == null || GlobalConfig.Pause) return;

        // ms 
        if (Date.now() - this.last > GlobalConfig.TickFrac) {

            this.last = Date.now();
            this.Render();
        }
        this.UpdateLookAtNode(delta);

      

    }


    public UpdateLookAtNode(delta: number): void {

        if (this.lookAtNode != null && this.lookAtNode.active && this.lookAtNode.position && GlobalConfig.ScaleIndex > 0) {

            let target = new Vec3(-1, -1, 1).multiply(this.lookAtNode.position).multiplyScalar(GlobalConfig.CurScale);
            let dis = Vec3.distance(this.node.position, target);
            if (dis > 2 * GlobalConfig.TilePixelSize.x) {

                let ratio = math.clamp(0.3 + delta / (GlobalConfig.TickFrac / 1000), 0, 1);
                this.node.position = Vec3.lerp(this.node.position, this.node.position, target, ratio);
                this.node.position = target;
            }

        }
    }

    //-----------------  拖拽地图 ----------------- 
    isMoving: boolean;

    //   this.touchLayer 不跟随地图 单独一层遮住整个屏幕
    public InitTouch(): void {

        let self = this;

        // 拖动移动地图  
        this.touchLayer.on(Node.EventType.TOUCH_MOVE, function (event: EventTouch) {
            self.isMoving = true;

            var delta = event.getDelta()

            self.node.position = Utils.GetDragPos(self.node.position.add3f(delta.x, delta.y, 0));

            self.LookAt(null);


        }, this);

    }


    // 清空重置
    public Reset(): void {

    }

    public UpdateScale(step: number): void {


        GlobalConfig.ScaleIndex += step;
        GlobalConfig.ScaleIndex = math.clamp(GlobalConfig.ScaleIndex, 0, GlobalConfig.Scales.length - 1);
        GlobalConfig.LastScale = GlobalConfig.CurScale;
        GlobalConfig.CurScale = GlobalConfig.Scales[GlobalConfig.ScaleIndex];
        this.node.setScale(GlobalConfig.CurScale, GlobalConfig.CurScale, GlobalConfig.CurScale);
    }

    //  缩放重置
    public Resize(): void {
        let old = this.node.position;
        let x = old.x * (GlobalConfig.CurScale / GlobalConfig.LastScale);
        let y = old.y * (GlobalConfig.CurScale / GlobalConfig.LastScale)
        this.node.position = new Vec3(x, y, old.z);
    }


}

