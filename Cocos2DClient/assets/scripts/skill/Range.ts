import { _decorator, Component, Node, Vec3, tween, Sprite, math, SpriteFrame, Enum } from 'cc';
import { Entity } from '../entity/Entity';
import { GlobalConfig } from '../core/GlobalConfig';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { SkillItem } from "./SkillItem";
import { SkillType } from "./SkillType";

const { ccclass, property } = _decorator;

// 远程- 范围攻击

@ccclass('Range')
export   class  Range  extends  SkillItem
{

    start(): void {
        this. flyToTaget();
    }

}