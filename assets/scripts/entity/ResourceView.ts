import { _decorator, Component, Node, CCObject, Sprite, Texture2D, SpriteFrame, resources, JsonAsset, Vec3, TiledMap, TiledTile, Vec2, instantiate, Prefab, Scene, view, input, Input, Event, EventTouch, Label, EventMouse, Slider, CCBoolean, CCFloat, UITransform, math, Size, CCString, director, EventHandler } from 'cc';
import { GlobalConfig } from '../core/GlobalConfig';
import { Utils } from '../utils/Utils';


const { ccclass, property } = _decorator;

// 树木 石头 资源的生长变化
@ccclass('ResourceView')
export class ResourceView extends Component {

    @property(Sprite)
    skin: Sprite;

    inactiveSprite: SpriteFrame;
    activeSprite: SpriteFrame;
    node: any;

    public set active(value: boolean) {

        if (this.inactiveSprite != null) {
            this.skin.spriteFrame = value ? this.activeSprite : this.inactiveSprite
        }
        else {
            this.node.active = value;
        }
    }

}


