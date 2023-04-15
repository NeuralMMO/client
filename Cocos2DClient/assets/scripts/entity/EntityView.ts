
import { _decorator, Component, Node, Sprite, Vec3, Label, ProgressBar, Input, EventTouch, EventMouse, director, game, math, Tween, UITransform, TweenAction, TweenSystem, tween, easing, SpriteFrame, Prefab, instantiate, resources, serializeTag, ccenum, Enum, view } from 'cc';
import { Skill } from '../components/SkillGroup';
import { NPCPacketDB, PlayerPacketDB } from '../data/Packet';
import { BattleEvent, EventManager } from '../event/Event';
import { GlobalConfig } from '../core/GlobalConfig';
import { Profession } from '../core/Profession';
import { HeadInfo } from '../ui/HeadInfo';
import { Utils } from '../utils/Utils';
import { World } from '../core/World';
import { EntityData } from './EntityData';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = Entity
 * DateTime = Fri Apr 15 2022 16:01:29 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = Entity.ts
 * FileBasenameNoExtension = Entity
 * URL = db://assets/scripts/entity/Entity.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */




@ccclass('EntityView')

export class EntityView extends Component {

    static readonly LEFT_HAT_X: number = -16;
    static readonly RIGHT_HAT_X: number = 16;
    // @property(Node)
    // skinRoot: Node;

    skins: SpriteFrame[] = [];

    @property(Sprite)
    skin: Sprite;

    @property(Node)
    attackEffect: Node;

    @property(Node)
    teamSelectedSprite: Node;

    @property(Node)
    selectedSprite: Node;


    @property(Sprite)
    hat: Sprite;

    @property(SpriteFrame)
    archer: SpriteFrame; // 弓箭手
    @property(SpriteFrame)
    mage: SpriteFrame;  // 法师
    @property(SpriteFrame)
    warrior: SpriteFrame;  // 战士 



    _profession: Profession;

    set profession(val: Profession) {

        if (this._profession != val) {
            this._profession = val;
            switch (val) {
                case Profession.None:
                    {
                        this.hat.node.active = false;
                        break;
                    }
                case Profession.Archer:
                    {
                        this.hat.node.active = true;
                        this.hat.spriteFrame = this.archer;
                        break;
                    }
                case Profession.Mage:
                    {
                        this.hat.node.active = true;
                        this.hat.spriteFrame = this.mage;
                        break;
                    }
                case Profession.Warrior:
                    {
                        this.hat.node.active = true;
                        this.hat.spriteFrame = this.warrior;

                        this.UpdateHat();
                        break;
                    }
                default:
                    break;
            }
        }
    }
    dir: number = 0;

    isDelete: boolean;
    _uiTransform: UITransform;

    get uiTransform(): UITransform {
        if (this._uiTransform == null) this._uiTransform = this.node.getComponent(UITransform);
        return this._uiTransform;
    }


    _moveTarget: Vec3;

    selectable: boolean;
    selectedNofity: any;

    _r: number;
    _c: number;

    constructor() {
        super();
    }

    public set seletAble(value: boolean) {
        if (this.selectable) {
            const self = this;
            this.skin.spriteFrame = this.skins[this.dir];
            this.node.on(Node.EventType.TOUCH_END, this.onTouch, this);
        }
    }


    public setSkins(skins: SpriteFrame[]): void {
        this.skins = skins;
    }


    start() {
        const self = this;
        this.skin.spriteFrame = this.skins[this.dir];
        this.UpdateHat();
        if (this.selectable) {
            this.node.on(Node.EventType.TOUCH_END, this.onTouch, this);
        }

        if (this._moveTarget != null) {
            this.node.position = this._moveTarget;
        }


    }


    public onTouch(): void {
        EventManager.view.emit(BattleEvent.SelectedEntity, this.selectedNofity);
    }


    public UpdateView(data: EntityData): void {

        this._moveTarget = Utils.RCToTileOrigin(data.r, data.c);
        this._r = data.r;
        this._c = data.c;
        if (data.r != data.oR || data.c != data.oC) {

            this._moveTarget = Utils.RCToTileOrigin(data.r, data.c);
        }
        else {

            this._moveTarget = null;
        }

        if (data.oC != data.c) {

            this.SetDir(data.c > data.oC ? 1 : 0);
        }
        //   melee、range、mage 
        // public melee: Skill;
        // public range: Skill;
        // public mage: Skill;
        // 近战


        this.UpdateMove(true);
        this.UpdateAttack();
        this.profession = data.skills.getProfession()


    }

    public update(delta: number): void {
     

        if (this.node.active == false) return;
       
        this.UpdateMove(true, delta);

    }


    public UpdateAttack(): void {

    }


    /*
        选中某只队伍后，当摄像机高度在最高位置时，在所有角色上方增加一个图标（层级最高），用于标注角色位置。（仅在最高角度显示）
        最低档：100%比例UI，显示名字和状态 ；index 
        中间档：100%显示名字； 0.01 index = 1
        最高档：只显示角色头顶标志；  0.04 index = 2
    */
    //  0  right  1 left
    public SetDir(dir: number): void {
        if (dir != this.dir) {
            this.dir = dir;
            this.skin.spriteFrame = this.skins[this.dir];
            this.UpdateHat();
        }
    }

    public UpdateHat(): void {
        if (this.hat.node.active) {
            let node: Node = this.hat.node;

            if (this.dir == 0) {
                node.setPosition(EntityView.RIGHT_HAT_X, node.position.y, node.position.z);
                node.setScale(1, 1, 1);
            }
            else {
                node.setPosition(EntityView.LEFT_HAT_X, node.position.y, node.position.z);
                node.setScale(-1, 1, 1);

            }
        }
    }


    // smooth 缓慢变化 
    public UpdateMove(smooth: boolean = true, delta: number = 0): void {

        if (this._moveTarget == null) return;


        if (smooth) {
            let ratio = math.clamp(0.3 + delta / (GlobalConfig.TickFrac / 1000), 0, 1);
            this.node.position = Vec3.lerp(this.node.position, this.node.position, this._moveTarget, ratio);

        }
        else {
            this.node.position = this._moveTarget;
        }

    }

    // 队伍被选中状态
    public set teamSelected(value: boolean) {
        this.teamSelectedSprite && (this.teamSelectedSprite.active = value)
    }

    public get teamSelected() {
        return this.teamSelectedSprite && (this.teamSelectedSprite.active)
    }

    //   自身被选中状态
    public set selected(value: boolean) {
        this.selectedSprite && (this.selectedSprite.active = value)
    }

    public get selected() {
        return this.selectedSprite && this.selectedSprite.active;
    }

    /*
    public updatZIndex(): void {

        let zIndex = NaN;

        let cur = `${this._r}_${this._c}`;
        let offset = GlobalConfig.MapSize;

        if (GlobalConfig.PosZIndexMap[cur]) {

            zIndex = GlobalConfig.PosZIndexMap[cur].value + offset;;
        }

        let left = `${this._r - 1}_${this._c}`;

        if (GlobalConfig.PosZIndexMap[left]) {

            zIndex = GlobalConfig.PosZIndexMap[left].value + offset;;
        }

        let leftUP = `${this._r - 1}_${this._c}`;

        if (GlobalConfig.PosZIndexMap[leftUP]) {

            zIndex = GlobalConfig.PosZIndexMap[leftUP].value + offset;;
        }

        let right = `${this._r + 1}_${this._c}`;

        if (GlobalConfig.PosZIndexMap[right]) {

            zIndex = GlobalConfig.PosZIndexMap[right].value + offset;;
        }



        let up = `${this._r}_${this._c - 1}`;
        if (GlobalConfig.PosZIndexMap[up]) {

            zIndex = GlobalConfig.PosZIndexMap[up].value + offset;;
        }

        let down = `${this._r}_${this._c + 1}`;
        if (GlobalConfig.PosZIndexMap[down]) {

            zIndex = GlobalConfig.PosZIndexMap[down].value + offset;;
        }

        if (zIndex != NaN) {
            this.uiTransform.priority = zIndex;

        }

    }
    */


    // public  setSiblingIndex(Utils.GetDepthByPos(this.node.position) + 15);
}


