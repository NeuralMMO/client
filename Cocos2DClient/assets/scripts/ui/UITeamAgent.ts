
import { _decorator, Component, Node, Label, ProgressBar, EventHandler, Toggle, Sprite } from 'cc';
import { NPCResources } from '../components/NPCResources';
import { PlayerResources } from '../components/PlayerResources';
import { ResourceGroup } from '../components/ResourceGroup';
import { EntityData } from '../entity/EntityData';
import { Entity, EntityType } from '../entity/Entity';
const { ccclass, property } = _decorator;



@ccclass('UITeamAgent')
export class UITeamAgent extends Component {

    index: number;

    @property(Label)
    nameLabel: Label;
    @property(Node)
    selectedView: Node;

    @property(ProgressBar)
    HPBar: ProgressBar;

    @property(ProgressBar)
    foodBar: ProgressBar;

    @property(ProgressBar)
    waterBar: ProgressBar;

    // 选中
    @property(Toggle)
    toggle: Toggle = undefined;

    @property(Sprite)
    disableMask: Sprite;


    _lastUpdateTime: number = 0;
    _updateStep: number = 500;// 500ms
    entity: Entity;


    start() {
        // [3]
    }

    public BindEntity(entity: Entity): void {

        this.entity = entity;

        if (this.entity == null) {

            this.node.active = false;
        }
        else {

            this.node.active = true;

            this.nameLabel.string = "" + this.entity.data.entID;//this.data.name;

            this.toggle.interactable = !this.disable;

            this.toggle.isChecked = false;

            if (entity.type == EntityType.NPC) {

                let resources: NPCResources = this.entity.data.resources as NPCResources;
                this.HPBar.node.active = true;
                this.HPBar.progress = this.disable ? 0 : resources.health.Percentage;

                this.foodBar.node.active = false;
                this.waterBar.node.active = false;
            }
            else if (entity.type == EntityType.Player) {

                let resources: PlayerResources = this.entity.data.resources as PlayerResources;

                this.HPBar.node.active = true;
                this.HPBar.progress = this.disable ? 0 : resources.health.Percentage;

                this.foodBar.node.active = true;
                this.foodBar.progress = this.disable ? 0 : resources.food.Percentage;

                this.waterBar.node.active = true;
                this.waterBar.progress = this.disable ? 0 : resources.water.Percentage;
            }
        }
    }

    public UpdateData(): void {

        if (this.entity == null) {
            this.node.active = false;
        }
    }

    public update(data: number): void {
        this.UpdateUI();
    }

    public UpdateUI(): void {
        this.disableMask.node.active = this.disable;
        this.toggle.interactable = !this.disable;

        if (this.disable) {
            this.toggle.isChecked = false;
        }


        if (this.disable) {

            this.HPBar.progress = 0;
            this.foodBar.progress = 0;
            this.waterBar.progress = 0;
        }

        if (this.disable || Date.now() - this._lastUpdateTime < this._updateStep) return;

        this._lastUpdateTime = Date.now();

        let resources: PlayerResources = this.entity.data.resources as PlayerResources;

        this.HPBar.node.active = true;

        this.HPBar.progress = this.disable ? 0 : resources.health.Percentage;

        if (resources.food)
            this.foodBar.progress = this.disable ? 0 : resources.food.Percentage;

        if (resources.water)
            this.waterBar.progress = this.disable ? 0 : resources.water.Percentage;
    }


    public get disable() { return this.entity == null || this.entity.removed || !this.entity.data.alive; }


}

