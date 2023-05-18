import { _decorator, Component, Node, Label, Sprite, Toggle, SpriteFrame, CCString } from 'cc';
const { ccclass, property } = _decorator;

@ccclass("DropdownOptionData")
export default class DropdownOptionData {

    public label: string = "";
    public icon: SpriteFrame = undefined;
    public alwaysBottom: boolean = false;
    constructor(label?: string, icon?: SpriteFrame, alwaysBottom: boolean = false) {
        this.label = label;
        this.icon = icon;
        this.alwaysBottom = alwaysBottom
    }
}
