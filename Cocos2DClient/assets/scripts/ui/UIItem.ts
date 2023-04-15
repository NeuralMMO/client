
import { _decorator, Component, Node, Sprite, Label, SpriteFrame, Enum } from 'cc';
import { ItemDB } from '../data/Packet';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { Utils } from '../utils/Utils';
const { ccclass, property } = _decorator;



export enum ItemName {

    Arcane,
    Bottom,
    Bow,
    Chisel,
    Gloves,
    Gold,
    Hat,
    Pickaxe,
    Poultice,
    Ration,
    Rod,
    Scrap,
    Shard,
    Shaving,
    Sword,
    Top,
    Wand,
}
Enum(ItemName)
/**
 * Predefined variables
 * Name = Item
 * DateTime = Sun Aug 14 2022 12:53:53 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = Item.ts
 * FileBasenameNoExtension = Item
 * URL = db://assets/scripts/ui/Item.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIItem')
export class UIItem extends Component {
    // [1]
    // dummy = '';

    static ShowNumberKey =
        [
            "gold",
            "scrap",
            "shaving",
            "shard",]

    // [2]
    // @property
    // serializableDummy = 0;
    @property(Sprite)
    bg: Sprite;
    @property(Sprite)
    icon: Sprite;

    @property(Label)
    numLabel: Label;
    @property(Label)
    levelLabel: Label;
    @property(SpriteFrame)
    equipedSprite: SpriteFrame;
    @property(SpriteFrame)
    unEquipedSprite: SpriteFrame;

    _equiped: boolean;

    public set equiped(val: boolean) {

        if (this._equiped != val) {

            this._equiped = val;
            // 切换BG 
            this.bg.spriteFrame = val ? this.equipedSprite : this.unEquipedSprite;
        }
    }

    _num: number = 0;

    public set num(val: number) {
        // if (this._num != val) {
            this._num = val;
            this.numLabel.string = "" + val;
        // }
    }





    public get num() { return this._num; }

    _level = 0;
    public set level(val: number) {
        // if (this._level != val) {
            this._level = val;
            this.levelLabel.string = "" + Utils.ConvertToRoman(val);
        // }
    }
    public get level() { return this._level; }

    _itemName: string;

    start() {
        // [3]
    }

    bindData(data: ItemDB, equiped: boolean = false) {
        
        if (data == null) {
            this._itemName = "";
            this.icon.spriteFrame = null;
            this.num = 0;
            this.numLabel.node.active = false;
            this.level = 0;
            this.levelLabel.node.active = false;
            this.equiped = false;
        }
        else {
            this._itemName = data.item;
            this.icon.spriteFrame = ResourcesHelper.GetItemIcon(data.item);
            if (this.icon.spriteFrame == null) {
                console.error(" this.icon.spriteFrame error  ", data.item);
            }
            this.num = data.quantity;
            this.numLabel.node.active = UIItem.ShowNumberKey.indexOf(this._itemName.toLowerCase()) != -1;
            this.level = data.level;
            this.levelLabel.node.active = this._itemName.toLowerCase() != "gold";
            this.equiped = equiped;
        }

    }

}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/zh/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/zh/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/zh/scripting/life-cycle-callbacks.html
 */
