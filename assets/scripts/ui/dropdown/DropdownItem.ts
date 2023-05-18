
import { _decorator, Component, Node, Label, Sprite, Toggle } from 'cc';
const { ccclass, property } = _decorator;

@ccclass("DropdownItem")
export default class DropdownItem extends Component {

    @property(Label)
    label: Label = undefined;

    // 高亮
    @property(Sprite)
    sprite: Sprite = undefined;

    // 勾选
    @property(Toggle)
    toggle: Toggle = undefined;

    // 已选标记
    @property(Sprite)
    checkmark: Sprite;
}