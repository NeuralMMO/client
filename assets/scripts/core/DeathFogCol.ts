
import { _decorator, Component, Node, Graphics, Color, Vec2, math } from 'cc';
import { Environment } from './Environment';
import { BattleEvent, EventManager } from '../event/Event';
import { GlobalConfig } from './GlobalConfig';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DeathFogCol
 * DateTime = Wed Aug 24 2022 16:08:25 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = DeathFogCol.ts
 * FileBasenameNoExtension = DeathFogCol
 * URL = db://assets/scripts/DeathFogCol.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

// 毒圈 
@ccclass('DeathFogCol')
export class DeathFogCol extends Component {

    static readonly PLAYER_DEATH_FOG = 240; // # 缩毒开始时间
    static readonly PLAYER_DEATH_FOG_FINAL_SIZE = 15; // # 最后的安全区的半径，安全区是个正方形
    static readonly PLAYER_DEATH_FOG_SPEED = 1 / 16; // # 缩毒速度，每16步缩一格


    @property(Graphics)
    gl: Graphics;


    curReplayStep: number = 0;
    curFogStep: number = 0;
    isLast: boolean;
    a: number = 0;

    left: Vec2;
    top: Vec2;
    right: Vec2;
    bottom: Vec2;

    color: Color = new Color().fromHEX("#0d430580");
    maxStep: number;



    start() {
        this.left = new Vec2;
        this.top = new Vec2;
        this.right = new Vec2;
        this.bottom = new Vec2;

        /*
        let size = (GlobalConfig.MapSize - fogStep* 2 - GlobalConfig.BORDER_SIZE * 2)

 
        */

        let size = (GlobalConfig.MapSize - GlobalConfig.BORDER_SIZE * 2)

        let w = size * GlobalConfig.HRhombusSize.x * 2
        let h = size * GlobalConfig.HRhombusSize.y * 2

        this.left.set(-w / 2 - GlobalConfig.HRhombusSize.x, 0);
        this.top.set(0, h / 2 + GlobalConfig.HRhombusSize.y);
        this.right.set(w / 2 + GlobalConfig.HRhombusSize.x, 0);
        this.bottom.set(0, -h / 2 - GlobalConfig.HRhombusSize.y);

        this.maxStep = (GlobalConfig.MapSize - GlobalConfig.BORDER_SIZE * 2 - DeathFogCol.PLAYER_DEATH_FOG_FINAL_SIZE * 2) / 2;
        // GlobalConfig.MapSize - fogStep * 2 - GlobalConfig.BORDER_SIZE * 2
        EventManager.view.on(BattleEvent.StepChange, this.onStepChange, this)
    }

    /*
    public onStepChange(step: number): void {
        if (this.curReplayStep != step) {

            this.curReplayStep = step;
            // clear All  
            if (this.curReplayStep < DeathFogCol.PLAYER_DEATH_FOG) {
            }
            else {
                // set 
                let fogStep = Math.floor((this.curReplayStep - DeathFogCol.PLAYER_DEATH_FOG) / 16);
                if (this.curFogStep != fogStep) {

                    // 安全区大小 
                    // let size = (GlobalConfig.MapSize - fogStep / 2 - GlobalConfig.BORDER_SIZE * 2);

                    for (let c = this.xMin; c < this.xMin + fogStep; c++) {

                        for (let r = this.yMin; r < this.yMin + fogStep; r++) {
                           let node =  this.environment.GetNode(r,c)
                           if(node.get)
                        }

                        for (let r = this.yMax; r < this.yMax - fogStep; r--) {
                            let node =  this.environment.GetNode(r,c)
                        }
                    }

                }
            }
        }
        */


    public onStepChange(step: number): void {
        if (this.curReplayStep != step) {

            this.curReplayStep = step;
            if (this.curReplayStep < DeathFogCol.PLAYER_DEATH_FOG) {
                this.gl.clear();
            }
            else {

                let fogStep = Math.floor((this.curReplayStep - DeathFogCol.PLAYER_DEATH_FOG + 1) * DeathFogCol.PLAYER_DEATH_FOG_SPEED);
                // 最大步数限制
                fogStep = Math.min(this.maxStep, fogStep);

                if (this.curFogStep != fogStep) {

                    this.curFogStep = fogStep;
                    // this.drawRect(this.curFogStep, this.color);
                    this.drawRect2(this.curFogStep, this.color);
                }
            }
        }
    }

    public update(): void {

    }

    /*
    //  绘制毒圈区域 
    public draw(width: number, color: Color): void {
        this.gl.clear();
        this.gl.lineWidth = width;
        this.gl.strokeColor = color;
        
        //left -> Top 
        this.gl.moveTo(this.pLeft.x, this.pLeft.y);
        this.gl.lineTo(this.pTop.x, this.pTop.y);
 
        //top ->right 
        this.gl.moveTo(this.pTop.x, this.pTop.y);
        this.gl.lineTo(this.pRight.x, this.pRight.y);
 
        // right - > Bottom 
        this.gl.moveTo(this.pRight.x, this.pRight.y);
        this.gl.lineTo(this.pBottom.x, this.pBottom.y);
 
        this.gl.moveTo(this.pBottom.x, this.pBottom.y);
        this.gl.lineTo(this.pLeft.x, this.pLeft.y);
 
        this.gl.stroke();
        this.gl.close();
        this.gl.fill();
    }
  */

    //   画一格子宽的区域的
    public draw(x1: number, x2: number, y1: number, y2: number, width: number, color: Color): void {

        this.gl.clear();
        this.gl.lineWidth = 1;
        this.gl.fillColor = color;
        let w = width;
        let h = width / 2;
        /*
        let x1 = this.pLeft.x;
        let x2 = this.pRight.x;
        let y1 = this.pTop.y;
        let y2 = this.pBottom.y;
        */
        //  画矩形
        this.gl.moveTo(x1 - w, 0);
        this.gl.lineTo(x1, h);
        this.gl.lineTo(w, y2);
        this.gl.lineTo(0, y2 - h);



        this.gl.moveTo(x1 - w, 0);
        this.gl.lineTo(0, y1 + h);
        this.gl.lineTo(w, y1);
        this.gl.lineTo(x1, -h);



        this.gl.moveTo(0, y1 + h);
        this.gl.lineTo(x2 + w, 0);
        this.gl.lineTo(x2, -h);
        this.gl.lineTo(-w, y1);


        this.gl.moveTo(x2 + w, 0);
        this.gl.lineTo(0, y2 - h);
        this.gl.lineTo(-w, y2);
        this.gl.lineTo(x2, h);

        this.gl.close();
        this.gl.fill();
    }



    public drawRect(step: number, color: Color): void {

        this.gl.clear();
        this.gl.lineWidth = 4;
        this.gl.fillColor = color;

        let w = GlobalConfig.HRhombusSize.x;
        let h = GlobalConfig.HRhombusSize.y;

        //  画矩形
        this.gl.moveTo(this.left.x, this.left.y);
        this.gl.lineTo(this.left.x + step * w, step * h);
        this.gl.lineTo(0, this.bottom.y + step * h * 2);
        this.gl.lineTo(-step * w, this.bottom.y + step * h);

        this.gl.moveTo(this.top.x, this.top.y);
        this.gl.lineTo(step * w, this.top.y - step * h);
        this.gl.lineTo(this.left.x + step * w * 2, 0);
        this.gl.lineTo(this.left.x + step * w, step * h);


        this.gl.moveTo(this.right.x, this.right.y);
        this.gl.lineTo(this.right.x - step * w, - step * h);
        this.gl.lineTo(0, this.top.y - 2 * step * h);
        this.gl.lineTo(step * w, this.top.y - step * h);


        this.gl.moveTo(this.bottom.x, this.bottom.y);
        this.gl.lineTo(- step * w, this.bottom.y + step * h);
        this.gl.lineTo(this.right.x - 2 * step * w, 0);
        this.gl.lineTo(this.right.x - step * w, - step * h);

        this.gl.close();
        this.gl.fill();
    }



    // 画区域 
    public drawRect2(step: number, color: Color): void {



        let xOffset = step * GlobalConfig.HRhombusSize.x;
        let yOffset = step * GlobalConfig.HRhombusSize.y;
        let p1 = this.left;

        let p2 = new Vec2(this.left.x + xOffset, this.left.y + yOffset);
        let p3 = new Vec2(this.bottom.x, this.bottom.y + yOffset * 2);
        let p4 = new Vec2(this.right.x - xOffset * 2, this.right.y);
        let p5 = new Vec2(this.top.x, this.top.y - yOffset * 2);
        let p6 = new Vec2(this.left.x + xOffset * 2, this.left.y);
        let p7 = p2;
        let p8 = this.top;
        let p9 = this.right;
        let p10 = this.bottom
        let p11 = this.left;

        this.gl.clear();
        this.gl.lineWidth = 1;
        this.gl.fillColor = this.color;

        this.gl.moveTo(p1.x, p1.y);
        this.gl.lineTo(p2.x, p2.y);
        this.gl.lineTo(p3.x, p3.y);
        this.gl.lineTo(p4.x, p4.y);
        this.gl.lineTo(p5.x, p5.y);
        this.gl.lineTo(p6.x, p6.y);
        this.gl.lineTo(p7.x, p7.y);
        this.gl.lineTo(p8.x, p8.y);
        this.gl.lineTo(p9.x, p9.y);
        this.gl.lineTo(p10.x, p10.y);
        this.gl.lineTo(p11.x, p11.y);

        this.gl.close();
        this.gl.fill();

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
