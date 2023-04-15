
import { _decorator, Component, Node, Label, CCString } from 'cc';
import { Skill } from '../components/SkillGroup';
import { EntityView } from '../entity/EntityView';
import { EntityData } from '../entity/EntityData';
import { EventManager, BattleEvent } from '../event/Event';
import { UIAgentStatusItem } from './UIAgentStatusItem';
import { Entity } from '../entity/Entity';
import { UISkillPanel } from './UISkillPanel';
import { UIPropertyPanel } from './UIPropertyPanel';
import {  UIItemPanel } from './UIItemPanel';
import { UIInfo } from './UIInfo';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UITeamInfo
 * DateTime = Fri Apr 22 2022 23:38:07 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UITeamInfo.ts
 * FileBasenameNoExtension = UITeamInfo
 * URL = db://assets/scripts/ui/UITeamInfo.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIAgentStatus')
export class UIAgentStatus extends Component {
    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;
    @property(Label)
    agentName: Label;
    // Property


    @property({ type: UIPropertyPanel} )
    propertyPanel:UIPropertyPanel;
    
    @property({ type: UISkillPanel} )
    skillPanel:UISkillPanel;

    @property({ type: UIItemPanel} )
    itemPanel:UIItemPanel;
    @property({ type: UIInfo} )
    infoPanel:UIInfo;

    _entityData: EntityData;

    _delay: number = 0.5 * 100;
    _lastUpdateTime: number = 0;

    start() {
      
        EventManager.view.on(BattleEvent.RemoveEntity, this.OnRemoveEntity, this);
        EventManager.view.on(BattleEvent.LookAtEntity, this.onLookAt, this);

    }

    public BindEntityData(data: EntityData): void {
        this._entityData = data;
        if (data != null) {
            this.agentName.string = data.name;
        }
        this.UpdateUI();
    }

    public update(delta: number): void {
        if (Date.now() - this._lastUpdateTime < this._delay) return;
        this._lastUpdateTime = Date.now();
        this.UpdateUI();
    }

    public UpdateUI(): void {
        if (this._entityData == null) {
            this.node.active = false;
        }
        else 
        {
            this.node.active = true;
            this.itemPanel.UpdateData(this._entityData.inventory);
            this.propertyPanel.UpdateData(this._entityData.resources);
            this.skillPanel.UpdateData(this._entityData.skills)
            this.infoPanel.UpdateData(this._entityData.metrics)
        }
         
    }
 

    public OnRemoveEntity(entity: Entity): void {
        if (entity && entity.data == this._entityData) {
            this._entityData = null;
            this.UpdateUI();
        }
    }


    public onLookAt(lookAt): void {

        if (lookAt == null || lookAt.entity == null) {
            this.BindEntityData(null);
            
            return;
        }
        else {
            this.BindEntityData(lookAt.entity.data);
        }

    }
}

 
