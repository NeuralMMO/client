
import { _decorator, Component, Node, ProgressBar, Label, Tween, easing, tween, Vec3, Sprite, UITransform, LabelOutline, math, Color } from 'cc';
import { Resource } from '../components/ResourceGroup';
import { DamgeManager } from '../core/DamegManager';
import { EntityType } from '../entity/Entity';
import { EntityData } from '../entity/EntityData';
import { EntityView } from '../entity/EntityView';
import { GlobalConfig } from '../core/GlobalConfig';

const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = HeadInfo
 * DateTime = Wed Apr 20 2022 19:18:16 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = HeadInfo.ts
 * FileBasenameNoExtension = HeadInfo
 * URL = db://assets/scripts/HeadInfo.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */
export enum HeadVisableStatus {
    None = 0,
    TeamFlagOnly = 1,
    NameOnly = 2,
    NameAndStatus = 3,
    All = 4,
}


@ccclass('HeadInfo')
export class HeadInfo extends Component {



    @property(Node)
    bars: Node;

    @property(ProgressBar)
    HPBar: ProgressBar;

    @property(ProgressBar)
    foodBar: ProgressBar;

    @property(ProgressBar)
    waterBar: ProgressBar;

    // 票伤害
    @property(Label)
    damageLabel: Label;

    @property(Label)
    nameLabel: Label;

    @property(LabelOutline)
    nameLabeOutline: LabelOutline;

    @property(Label)
    shortName: Label;

    // 头部队伍标记
    @property(Sprite)
    teamTag: Sprite;

    // 当前选中
    @property(Sprite)
    selectedFlag: Sprite;

    damageTw: Tween<any>;

    _visableStatus: HeadVisableStatus;

    entityType: EntityType;


    normalOutlineColor: Color = Color.BLACK;// new Color().fromHEX("#4b6587")

    selectOutlineColor: Color = Color.WHITE;// new Color().fromHEX("#bdb199");// "#bdb199";

    public nameColor: string;

    public nameOutlineColor: string = "";
    // 跟随
    follow: EntityView;


    _lastScarleIndex: number = 0;
    _followSelected: boolean = false;
    _followTeamSelected: boolean = false;

    public setFollow(follow: EntityView): void {
        this.follow = follow;

    }


    start() {
        if (this.entityType != EntityType.Player) {
            this.foodBar.node.active = false;
            this.waterBar.node.active = false;
        }

        this._lastScarleIndex = GlobalConfig.ScaleIndex;
        this._followSelected = this.follow && this.follow.selected ? true : false;
        this._followTeamSelected = this.follow && this.follow.teamSelected ? true : false;

        this.node.position = this.GetHeadPos();
        this.UpdateActive();
        //   this.nameLabel.color.fromHEX(this.nameColor);
        this.nameLabel.color = new Color().fromHEX(this.nameColor);
    }

    public set lastScarleIndex(value: number) {
        if (this._lastScarleIndex != value) {
            this._lastScarleIndex = value;
            this.UpdateActive();
        }
    }

    public set followSelected(value: boolean) {
        if (this._followSelected != value) {
            this._followSelected = value;
            this.UpdateActive();
        }
    }

    public set followTeamSelected(value: boolean) {
        if (this._followTeamSelected != value) {
            this._followTeamSelected = value;
            this.UpdateActive();
        }
    }


    public setNameColor(color: string): void {
        this.nameColor = color;
        this.nameLabel && (this.nameLabel.color = new Color().fromHEX(this.nameColor));
    }

    public setNameOutlineColor(color: string): void {
        this.nameOutlineColor = color;
        this.nameLabeOutline && (this.nameLabeOutline.color = new Color().fromHEX(this.nameOutlineColor));
    }


    last: number = 0;

    update(delta: number): void {

        if (this.node.active == false) return;

        if (this.follow != null && Date.now() - this.last > 33.3) {

            this.last = Date.now();
            this.node.position = this.GetHeadPos();
            this.lastScarleIndex = GlobalConfig.ScaleIndex;
            this.followSelected = this.follow && this.follow.selected ? true : false;
            this.followTeamSelected = this.follow && this.follow.teamSelected ? true : false;
        }
    }

    public UpdateActive(): void {


        this.selectedFlag.node.active = this._followSelected && this._lastScarleIndex == 0;

        this.teamTag.node.active = !this._followSelected && this._followTeamSelected && this._lastScarleIndex == 0;


        this.bars.active = this.follow != null && this._lastScarleIndex == 2 ? true : false;

        this.shortName.node.active = this.follow != null && this.entityType == EntityType.NPC && this._lastScarleIndex == 1 ? true : false;

        this.nameLabel.node.active = this.shortName.node.active == false && this.follow != null && this._lastScarleIndex >= 1 ? true : false;

        // name Outline color 
        if (this.nameLabel.node.active && this.nameOutlineColor == "") {

            if (this._followTeamSelected) {
                if (this.nameLabeOutline.color != this.selectOutlineColor) {
                    this.nameLabeOutline.color = this.selectOutlineColor;
                    this.nameLabeOutline.width = 3;
                }
            }
            else {
                if (this.nameLabeOutline.color != this.normalOutlineColor) {
                    this.nameLabeOutline.color = this.normalOutlineColor;
                    this.nameLabeOutline.width = 1;
                }
            }

        }

    }

    public UpdateUI(data: EntityData): void {

        if (this.node == null || this.node.active == false) return;

        this.nameLabel.string = this.entityType == EntityType.NPC ? `Lvl${data.level}(${data.item_level}) ${data.name}` : data.name;
        this.shortName.string = this.nameLabel.string[0].toUpperCase();

        this.UpdateHPBar(data);
        this.UpdateFoodBar(data);
        this.UpdateWaterBar(data);
        this.UpdateDamage(data);
        this.UpdateGold(data);
        this.UpdateAction(data);

    }

    public UpdateHPBar(data: EntityData): void {

        if (this.bars.active == false || !this.HPBar.node.active) return;

        let hp: Resource = data.resources.GetResource("health");

        if (hp != null) {
            this.HPBar.progress = hp.Percentage;
        } else {
            this.HPBar.node.active = false;

        }
    }

    public UpdateFoodBar(data: EntityData): void {

        if (this.bars.active == false || !this.foodBar.node.active) return;

        let food: Resource = data.resources.GetResource("food");

        if (food != null) {
            this.foodBar.progress = food.Percentage;
        }
        else {
            this.foodBar.node.active = false;

        }
    }

    public UpdateWaterBar(data: EntityData): void {
        if (this.bars.active == false || !this.waterBar.node.active) return;

        let water: Resource = data.resources.GetResource("water");

        if (water != null) {

            this.waterBar.progress = water.Percentage;

        } else {
            this.waterBar.node.active = false;
        }
    }


    public UpdateDamage(data: EntityData): void {

        if (this.bars.active == false) return;
        if (data.history.damage != 0) {
            DamgeManager.instance.showDamge(-Math.ceil(data.history.damage), this.node.position, this.follow);
        }
    }

    public gold: number = 0;
    public UpdateGold(data: EntityData): void {
        if (this.bars.active == false) return;
        if (data.metrics != null && data.metrics.Gold > this.gold) {

            DamgeManager.instance.showGetGold(data.metrics.Gold - this.gold, this.node.position, this.follow);
            this.gold = data.metrics.Gold;
        }
    }



    public UpdateAction(data: EntityData): void {
        if (this.bars.active == false) return;
        if (data.history.actions != null) {
            let use = data.history.actions.use;
            // use != null &&  console.log("use.item " + use.item.item);
            // Poultice
            if (use != null && use.item != null && use.item.item.toLocaleLowerCase() == "poultice") {

                if (use.item.health_restore > 0) {
                    let msg = `Use Lv.${use.item.level} ${use.item.item},HP+${use.item.health_restore}`;// Use Lv.5 Poultice，HP + N;
                    DamgeManager.instance.showUsePoultice(msg, this.node.position, this.follow);
                }
            }
        }
    }

    public Delete(): void {

        if (this.damageTw != null) {
            this.damageTw.stop();
            this.damageTw = null;
        }
        this.node && (this.node.active = false)
    }

    // 是否显示队伍
    public set TeamTagVisiable(value: boolean) {
        if (this.teamTag.node.active != value)
            this.teamTag.node.active = value;
    }


    public get AllHide(): boolean {

        return !(this.nameLabel.node.active || this.bars.active || this.teamTag.node.active || this.damageLabel.node.active);

    }

    public set BarVisable(value: boolean) {
        this.bars.active = value;
    }

    public set NameVisable(value: boolean) {
        this.nameLabel.node.active = value;
    }


    public GetHeadPos(): Vec3 {

        let pos = new Vec3(this.follow.node.worldPosition);

        pos = this.node.parent.getComponent(UITransform).convertToNodeSpaceAR(pos);
        pos.z = 0;

        return pos.add3f(0, 160 * GlobalConfig.CurScale, 0);
    }

}

