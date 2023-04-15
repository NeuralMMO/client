import { easing, Enum, SpriteFrame, tween, utils, Vec3 } from "cc";
import { Attack } from "../components/History";
import { AttackDB } from "../data/Packet";
import { EntityManager } from "../core/EntityManager";
import { GlobalConfig } from "../core/GlobalConfig";
import { SkillManager } from "../skill/SkillManager";
import { SkillType } from "../skill/SkillType";
import { HeadInfo } from "../ui/HeadInfo";
import { Utils } from "../utils/Utils";
import { EntityData } from "./EntityData";
import { EntityView } from "./EntityView";
import { ZIndex } from "./ZIndex";


export enum EntityType {
    None = 0,
    NPC = 1,
    Player = 2,
}

export class Entity {


    public data: EntityData;
    public type: EntityType;
    public view: EntityView;
    public removed: boolean;
    public headInfo: HeadInfo;
    public _zindex: ZIndex;

    public get zIndex(): ZIndex {
        if (this._zindex == null) {
            this._zindex = this.view.node.getComponent(ZIndex);
        }
        return this._zindex;
    }

    public get freeze(): number {
        if (this.data && this.data.status) {
            return this.data.status.freeze;
        }
        return 0;
    }

    constructor() {
        this.removed = false;
    }

    public get id(): number { return this.data.entID; }

    public get teamName(): string { return this.data.teamName; }

    public setSkins(skins: SpriteFrame[]): void {
        this.view.setSkins(skins);
    }
    public setNameColor(color: string): void {
        this.headInfo.setNameColor(color);
    }
    public setNameOutlineColor(color: string): void {
        this.headInfo.setNameOutlineColor(color);
    }
    

    public start(): void {
        this.view._moveTarget = Utils.RCToTileOrigin(this.data.r, this.data.c);
    }

    public set selectable(value) {
        this.view.selectable = value;
        if (value) this.view.selectedNofity = this;
        // this.view.selectedNofity = this;
    }


    public update(data: any) {
        this.headInfo.node.active = true;
        this.view.node.active = true;
        this.data.UpdateEntityData(data);
        this.headInfo.UpdateUI(this.data);
        this.view.UpdateView(this.data);
        this.removed = false;
        this.updataAttack();
        this.zIndex.r = this.data.r;
        this.zIndex.c = this.data.c;
    }

    public updataAttack(): void {


        this.view.attackEffect.active = this.freeze > 0;

        if (GlobalConfig.ScaleIndex == 0) return;

        let attack: AttackDB = this.data.history.attack;

        if (attack != null && attack.target != null) {

            let entity: Entity = EntityManager.instance.GetEntityByKey(attack.target);

            if (entity != null) {

                let startPOs = new Vec3(0, 50, 0).add(this.position);

                let targetPos = new Vec3(0, 50, 0).add(entity.position);
                let style = attack.style.toLowerCase();
                // 根据名字获得相应的枚举
                SkillManager.instance.showSkill(SkillType[style], startPOs, targetPos, entity);
            }
        }
    }

    public onHit(type: SkillType) {

       
    }

    public get position() { return this.view.node.position };

 

    public removeView(): void {

        if (this.removed) return;

        this.view.node.active = false;

        this.headInfo.node.active = false;

        this.removed = true;

    }

    public set teamSelected(value) {
        this.view.teamSelected = value;
    }

    public set selected(value) {
        this.view.selected = value;
    }

}