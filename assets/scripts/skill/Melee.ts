
import { _decorator, Component, Node, Vec3, tween, Sprite, math, SpriteFrame, Enum } from 'cc';
import { Entity } from '../entity/Entity';
import { GlobalConfig } from '../core/GlobalConfig';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { SkillItem } from "./SkillItem";
import { SkillType } from "./SkillType";

const { ccclass, property } = _decorator;

// 近战 -  当前设定为远程 
@ccclass('Melee')
export class Melee extends SkillItem {

   
   anim: SpriteFrame[] = [];
   index: number = 0;
   animSpeed: number = 60; // ms updateDelay  
   lastTime: number = 0;
   meleeTime: number = 0.5;
   
    start()
    {
        
        /*
        let dir = this.getDir();
        // 旧版设定 
        // this.node.position = this.startPos .add(this.tagetPos.subtract(this.startPos).multiplyScalar(0.5));
        this.node.position = this.startPos;
        // this.anim = ResourcesHelper.GetSkillsSpriteFrames(this.type, dir);
        this.index = 0;
        // this.view.spriteFrame = this.anim[this.index];
        this.lastTime = Date.now();
        this.index++;
        */
       
        this. flyToTaget();
        
        /**  
         * 旧版直接攻击 ，当前版设计为飞行
        let  self = this;
        tween(this.node).delay(this.meleeTime / GlobalConfig.SpeedRate).call(() => { self.clear() }).start();
        */
    }

    /*
    update(delta: number) {

        if (Date.now() - this.lastTime > (this.animSpeed / GlobalConfig.SpeedRate)) {
            this.view.spriteFrame = this.anim[this.index];
            this.lastTime = Date.now();
            this.index++;
            if (this.index >= this.anim.length) this.index = 0;
        }
    }*/

}