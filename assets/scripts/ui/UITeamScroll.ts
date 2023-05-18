
import { _decorator, Component, Node, Prefab, instantiate, Toggle } from 'cc';
import { Dropdown } from './dropdown/Dropdown';
import { UITeamAgent } from './UITeamAgent';
import { EntityManager } from '../core/EntityManager';
import { BattleEvent, EventManager } from '../event/Event';
import { UITeamOptionData } from './UITeamOptionData';
import { Entity } from '../entity/Entity';
const { ccclass, property } = _decorator;



@ccclass('UITeamScroll')
export class UITeamScroll extends Component {


    @property(Prefab)
    itemPrefab: Prefab;

    @property(Node)
    agentContent: Node;

    @property(Dropdown)
    teamDropdown: Dropdown;

    public teams: string[];

    public items: UITeamAgent[];

    public curTeam: string = undefined;
    // 当前选中项
    public curItem: UITeamAgent;

    // 选中的Agent
    public selectedIndex: number = -1;

    start() {
        this.items = [];
        this.teams = [];

        let option = new UITeamOptionData({ label: "Clear", invalid: true, alwaysBottom: true });
        this.teamDropdown.addOptionDatas([option], false);

        EventManager.view.on(BattleEvent.AddTeam, this.AddTeam, this);
        EventManager.view.on(BattleEvent.RemoveEntity, this.OnRemoveEntity, this);
        EventManager.view.on(BattleEvent.SelectedEntity, this.OnSelectedEntity, this);

    }


    public bindTeam(team: string, selected: number = 0): void {

        this.curTeam = team;
        this.selectedIndex = -1;

        for (let i = 0; i < this.items.length; i++) {
            let item = this.items[i];

            item.entity && (item.entity.teamSelected = false);
            item.entity && (item.entity.selected = false);

            item.node.off(Toggle.EventType.TOGGLE, this.onAgentSelected, this);
            item.node.destroy();
        }
        this.items = []
        this.selectedIndex = -1;
        this.curItem = null;
        if (team != undefined) {

            let entities: Entity[] = EntityManager.instance.GetPlayersByTeam(team);

            for (let i = 0; i < entities.length; i++) {
                let entity: Entity = entities[i];

                let node: Node = instantiate(this.itemPrefab);
                let item = node.getComponent(UITeamAgent);
                this.items.push(item);
                item.BindEntity(entity);
                item.node.on(Toggle.EventType.TOGGLE, this.onAgentSelected, this);
                this.agentContent.addChild(node);

            }
        }

        this.UpdateSelected();
    }

    public UpdateSelected(): void {

        let entity = this.curItem != null && this.curItem.entity != null && this.curItem.entity;
        let lookAt = { entity: entity, focus: true };

        EventManager.view.emit(BattleEvent.LookAtEntity, lookAt);

        let entities: Entity[] = EntityManager.instance.GetPlayersByTeam(this.curTeam);

        if (entities == null || entities.length == 0) return;

        for (let i = 0; i < entities.length; i++) {

            entities[i].teamSelected = true;

            entities[i].selected = this.curItem && (this.curItem.entity == entities[i]);
        }

    }

    public AddTeam(team: string): void {

        if (this.teams.indexOf(team) == -1) {
            this.teams.push(team);
            let option = new UITeamOptionData({ label: team });
            this.teamDropdown.addOptionDatas([option]);
        }
    }

    // 下拉框改变
    public OnTeamDropChanged(data: UITeamOptionData): void {
        if (data.invalid) {
            this.bindTeam(undefined);
        }
        else {
            this.bindTeam(data.label);
        }
    }

    public onAgentSelected(toggle: Toggle): void {

        let parent = toggle.node.parent;

        if (this.curItem != null && this.curItem.node != parent) {
            this.curItem.toggle && (this.curItem.toggle.isChecked = false);
            this.curItem = null;
        }

        for (let i = 0; i < parent.children.length; i++) {

            if (parent.children[i] == toggle.node) {

                this.curItem = toggle.node.getComponent(UITeamAgent);

                this.selectedIndex = i - 1;
                break;
            }
        }

        this.UpdateSelected();
    }

    public OnRemoveEntity(entity: Entity): void {

        if (entity != null && this.curItem != null && this.curItem.entity == entity) {

            this.bindTeam(this.curTeam);
        }
    }


    public OnSelectedEntity(entity: Entity): void {
        let team = entity.teamName;
        let entitys = EntityManager.instance.GetPlayersByTeam(team);

        this.bindTeam(team, entitys.indexOf(entity));

        if (this.curItem != null && this.curItem.entity != entity) {
            this.curItem.toggle.isChecked = false;
        }

        for (let i = 0; i < this.items.length; i++) {

            let item = this.items[i];

            if (item.entity == entity) {

                item.toggle.isChecked = true;

                this.selectedIndex = i;

                break;
            }
        }
        EventManager.view.emit(BattleEvent.LookAtEntity, { entity: entity, focus: false });

        this.teamDropdown.Select(team);

    }





}

