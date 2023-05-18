
import { _decorator, Component, Node, Slider, math, Sprite, UITransform, CCFloat, Label, Vec3, Tween, tween, tweenUtil, easing, SpriteFrame, input, Input, EventTouch, EventKeyboard, KeyCode } from 'cc';
import { GlobalConfig } from '../core/GlobalConfig';
import { World } from '../core/World';
import { UIStatisticalData } from './UIStatisticalData';
import { UITeamScroll } from './UITeamScroll';
import { UIAgentStatus } from './UIAgentStatus';
import { ResourcesHelper } from '../utils/ResourcesHelper';
import { UIMarket } from './UIMarket';
import { PacketDB } from '../data/Packet';
import { BattleEvent, EventManager } from '../event/Event';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIGame
 * DateTime = Fri Apr 22 2022 22:46:26 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIGame.ts
 * FileBasenameNoExtension = UIGame
 * URL = db://assets/scripts/ui/UIGame.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIGame')
export class UIGame extends Component {


    @property(World)
    world: World;

    @property(UIStatisticalData)
    statisticalPanel: UIStatisticalData;

    @property(UITeamScroll)
    teamScroll: UITeamScroll;

    @property(UIAgentStatus)
    agentStatus: UIAgentStatus;

    @property(UIMarket)
    market: UIMarket;

    @property(Slider)
    stepSlider: Slider;

    @property(UITransform)
    sliderBar: UITransform;

    @property(CCFloat)
    sliderBarMax: number = 616;

    @property(Label)
    stepLabel: Label;

    @property(Label)
    speedLabel: Label;

    @property(Sprite)
    pauseSprite: Sprite;

    @property(Sprite)
    playerSprite: Sprite;

    @property(Node)
    btnSharik: Node;

    @property(Sprite)
    hideSp: Sprite;


    @property([SpriteFrame])
    hideSpriteFrame: SpriteFrame[] = [];

    sharkTween: Tween<any>;
    hideAll: boolean;

    curPacket: PacketDB;


    start() {

        this.sliderBar.width = Math.floor(this.sliderBarMax * this.stepSlider.progress);

        this.playerSprite.node.active = GlobalConfig.Pause;
        this.pauseSprite.node.active = !GlobalConfig.Pause;
        this.speedLabel.string = GlobalConfig.SpeedRate + "x";
        this.hideSp.spriteFrame = this.node.active ? this.hideSpriteFrame[0] : this.hideSpriteFrame[1];

        EventManager.view.on(BattleEvent.PacketChange, this.onPacketChange, this);

        input.on(Input.EventType.KEY_DOWN, this.onKeyDown, this);
        // input.on(Input.EventType.KEY_DOWN,this.onKeyDown,this)
    }




    /**
     * Pause
     */
    public Pause() {
        GlobalConfig.Pause = !GlobalConfig.Pause;

    }

    public ZoomUp() {

        this.world.UpdateScale(1);
        this.world.Resize();
    }

    public ZoomDown() {

        this.world.UpdateScale(-1);
        this.world.Resize();

    }

    update(): void {
        this.stepLabel.string = "Turn " + this.world.replayStep;
        this.playerSprite.node.active = GlobalConfig.Pause;
        this.pauseSprite.node.active = !GlobalConfig.Pause;
        this.stepSlider.progress = this.world.replayStep / this.world.totalStep;

        this.sliderBar.width = Math.floor(this.sliderBarMax * this.stepSlider.progress);


    }

    public onStepSlider(): void {
        this.world.replayStep = Math.floor(this.stepSlider.progress * this.world.totalStep);
        this.world.replayStep = math.clamp(this.world.replayStep, 0, this.world.totalStep - 1);
        this.sliderBar.width = Math.floor(this.sliderBarMax * this.stepSlider.progress);
        this.world.Render();
    }


    public SharkStatisticalPanel(): void {

        let node = this.statisticalPanel.node;
        let btnSharik = this.btnSharik;

        if (node.active) {

            let x = node.position.x + 647;

            let target = new Vec3(x, node.position.y, node.position.z);

            node.active = false;
            btnSharik.active = true;
     
        }
        else {
            node.active = true;
            btnSharik.active = false;

            let x = node.position.x - 647;

            // let target = new Vec3(x, node.position.y, node.position.z);

            // this.sharkTween = tween(node).to(0.5, { position: target }, {

            //     easing: easing.sineOut, onComplete: () => {
            //     }
            // });
        }


        // this.sharkTween.start();

    }
    public OnBtnUpdateSpeed(): void {
        this.UpdateSpeed(0);
    }

    // 0  循环  1  加 2 减
    public UpdateSpeed(dir: number = 0): void {
        switch (dir) {
            case 0:
                {
                    if (GlobalConfig.SpeedRate >= 8) GlobalConfig.SpeedRate = 0.5;
                    else GlobalConfig.SpeedRate = GlobalConfig.SpeedRate * 2;
                    break;
                }
            case 1:
                {
                    GlobalConfig.SpeedRate = GlobalConfig.SpeedRate * 2;
                    GlobalConfig.SpeedRate = Math.min(GlobalConfig.SpeedRate, 8);
                    break;
                }
            case -1:
                {
                    GlobalConfig.SpeedRate = GlobalConfig.SpeedRate / 2;
                    GlobalConfig.SpeedRate = Math.max(GlobalConfig.SpeedRate, 0.5);
                    break;
                }
            default:
                {
                    if (GlobalConfig.SpeedRate >= 8) GlobalConfig.SpeedRate = 0.5;
                    else GlobalConfig.SpeedRate = GlobalConfig.SpeedRate * 2;
                    break;
                }

        }


        this.speedLabel.string = GlobalConfig.SpeedRate + "x";
    }





    public HideAll(): void {
        this.node.active = !this.node.active;
        this.hideSp.spriteFrame = this.node.active ? this.hideSpriteFrame[0] : this.hideSpriteFrame[1]

    }

    public showMarket(active: boolean = true): void {
        this.market.node.active = active;
        this.market.bindData(this.curPacket && this.curPacket.market);
    }

    public onPacketChange(packet): void {
        this.curPacket = packet;
        this.market.node.active && this.market.bindData(this.curPacket && this.curPacket.market);
    }

    // public  shortcuts():void
    public onKeyDown(event: EventKeyboard) {
        switch (event.keyCode) {
            case KeyCode.SPACE:  //  play  - pause
                this.Pause();
                break;
            case KeyCode.COMMA:
                this.UpdateSpeed(-1);

                break;
            case KeyCode.PERIOD:
                this.UpdateSpeed(1);
                break;
            case KeyCode.KEY_Z:
                this.ZoomDown();
                break;
            case KeyCode.KEY_X:
                this.ZoomUp();
                break;
            case KeyCode.KEY_M:
                this.showMarket(!this.market.node.active)
                break;

            case KeyCode.KEY_S: // 统计 
                this.SharkStatisticalPanel();
                break;
        }
    }

}

