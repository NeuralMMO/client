
import { _decorator, Component, Node, Graphics, randomRange, Color, Vec2, Vec3, UITransform, Rect, find, Canvas } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = Demo
 * DateTime = Tue Aug 23 2022 17:14:16 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = Demo.ts
 * FileBasenameNoExtension = Demo
 * URL = db://assets/scripts/Demo.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('Demo')
export class Demo extends Component {
    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;
    color: Color = new Color().fromHEX("#0d430580");
    left: Vec2;
    right: Vec2;
    top: Vec2;
    bottom: Vec2;

    @property(Node)
    testNode: Node;

    @property(Graphics)
    gl: Graphics;

    @property(Canvas)
    cav: Canvas

    start() {


        /*
        this.draw(x1, y2, x2, y2, x3, y3, x4, y4);

        this.draw(x1, y2, x2, y2, x3, y3, x4, y4);

        this.draw(x1, y2, x2, y2, x3, y3, x4, y4);
*/
        this.drawRect();

        let pos = new Vec3(0, 0, 0);

        this.node.getComponent(UITransform)

        let cav = this.cav;
        if (cav != null) {
            
            pos = this.testNode.getComponent(UITransform).convertToWorldSpaceAR(pos)
            // pos =cav.getComponent(UITransform).
            pos = cav.getComponent(UITransform).convertToNodeSpaceAR(pos);

            console.log(pos);
            // let rect = new Rect(-50, -50, 100, 100);
        }
        else {
            console.log("Canvas = null");
        }


    }

    public draw(
        x1: number, y1: number,
        x2: number, y2: number,
        x3: number, y3: number,
        x4: number, y4: number
    ): void {

        this.gl.moveTo(x1, y1);
        this.gl.lineTo(x2, y2);
        this.gl.lineTo(x3, y3);
        this.gl.lineTo(x4, y4);
    }

    public draw4Rect(): void {
        let size = new Vec2(256, 128);
        let count = 6;

        this.left = new Vec2(-size.x * count / 2, 0);
        this.right = new Vec2(size.x * count / 2, 0);
        this.top = new Vec2(0, size.y * count / 2);
        this.bottom = new Vec2(0, -size.y * count / 2);

        let step = 2;


        let w = 128;//size.x / 2;
        let h = 64;//size.y / 2;

        this.gl.lineWidth = 1;
        this.gl.fillColor = this.color;

        let x1: number = 0; let y1: number = 0;
        let x2: number = 0; let y2: number = 0;
        let x3: number = 0; let y3: number = 0;
        let x4: number = 0; let y4: number = 0;

        x1 = this.left.x; y1 = 0;
        x2 = this.left.x + step * w; y2 = step * h;
        x3 = 0; y3 = this.bottom.y + step * h * 2;
        x4 = -step * w; y4 = this.bottom.y + step * h;

        // console.log(x1, y1, " | ", x2, y2, " | ", x3, y3, " | ", x4, y4)
        this.draw(x1, y1, x2, y2, x3, y3, x4, y4);

        x1 = 0; y1 = this.top.y;
        x2 = step * w; y2 = this.top.y - step * h;
        x3 = this.left.x + step * w * 2; y3 = 0;
        x4 = this.left.x + step * w; y4 = step * h;
        this.draw(x1, y1, x2, y2, x3, y3, x4, y4);

        // rigth 
        x1 = this.right.x; y1 = 0;
        x2 = this.right.x - step * w; y2 = - step * h;
        x3 = 0; y3 = this.top.y - 2 * step * h;
        x4 = step * w; y4 = this.top.y - step * h;
        this.draw(x1, y1, x2, y2, x3, y3, x4, y4);

        // bottom 
        x1 = 0; y1 = this.bottom.y;
        x2 = - step * w; y2 = this.bottom.y + step * h;
        x3 = this.right.x - 2 * step * w; y3 = 0;
        x4 = this.right.x - step * w; y4 = - step * h;
        this.draw(x1, y1, x2, y2, x3, y3, x4, y4);
        this.gl.close();
        // this.gl.stroke();
        this.gl.fill();
    }


    // 画区域 
    public drawRect(): void {

        let size = new Vec2(256, 128);
        let count = 4;
        let w = size.x * count;
        let h = size.y * count;
        let tileW = size.x;
        let tileH = size.y;

        this.left = new Vec2(-w / 2 - size.x / 2, 0);
        this.right = new Vec2(w / 2 + size.x / 2, 0);
        this.top = new Vec2(0, h / 2 + size.y / 2);
        this.bottom = new Vec2(0, -h / 2 - size.y / 2);

        let step = 1;

        let xOffset = step * size.x / 2;
        let yOffset = step * size.y / 2;
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

    /*
    delay = 0;
    a: number = 0.5;

    update(deltaTime: number) {

        if (this.delay == 0) {
            this.a += 0.1;
            this.a = this.a > 1 ? 0.5 : this.a;

            this.gl.clear();
            this.gl.lineWidth = 10;
            this.gl.strokeColor = Color.GREEN;
            Color.set(this.gl.strokeColor, this.gl.strokeColor.r, this.gl.strokeColor.g, this.gl.strokeColor.b, this.gl.strokeColor.a * this.a);
            this.gl.moveTo(-160, 0);
            this.gl.lineTo(0, -180);

            this.gl.moveTo(160, 0);
            this.gl.lineTo(0, 180);
            this.gl.stroke();

            this.gl.close();
            this.gl.fill();
        }
        if (this.delay > 5) this.delay = 0;
        else {
            this.delay++;
        }
    }
    */
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
