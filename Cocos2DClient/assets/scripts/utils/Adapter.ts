
import { _decorator, Component, Node, Canvas, find, Size, UITransform, view, Vec2, ResolutionPolicy } from 'cc';
import { GlobalConfig } from '../core/GlobalConfig';
const { ccclass, property } = _decorator;



@ccclass('Adapter')
export class Adapter extends Component {

    start() {
        this.resize();
    }


    public resize() {
        var cvs = find('Canvas').getComponent(Canvas);
        GlobalConfig.CanvasNode = cvs.node;
        //保存原始设计分辨率，供屏幕大小变化时使用
        if (!this.curDR) {
            //获取当前视图的设计分辨率
            this.curDR = view.getDesignResolutionSize();
        }

        var dr = this.curDR;
        this.clientWidth = document.body.clientWidth;
        this.clientHeight = document.body.clientHeight
        let s = new Size(this.clientWidth, this.clientHeight);

        var rw = s.width;
        var rh = s.height;
        var finalW = rw;
        var finalH = rh;

        if ((rw / rh) > (dr.width / dr.height)) {
            //如果更长，则用定高
            finalH = dr.height;
            finalW = finalH * rw / rh;
        }
        else {
            //如果更短，则用定宽
            finalW = dr.width;
            finalH = rh / rw * finalW;
        }
        //将计算出来的分辨率重新设置为设计分辨率
        // view.setDesignResolutionSize(finalW, finalH, 5)
        //   一定要 ResolutionPolicy.EXACT_FIT
        view.setDesignResolutionSize(finalW, finalH, ResolutionPolicy.EXACT_FIT)
        //将 Canvas 的宽高设置为新的设计分辨率
        cvs.node.getComponent(UITransform).setContentSize(finalW, finalH)
        cvs.node.emit('resize');

        GlobalConfig.Viewport.width = finalW + 512;
        GlobalConfig.Viewport.height = finalH + 256;

        GlobalConfig.Viewport.x = -GlobalConfig.Viewport.width / 2;
        GlobalConfig.Viewport.y = -GlobalConfig.Viewport.height / 2;
    }



    clientWidth: number;
    clientHeight: number;
    public curDR: Size = null;

    update() {
        if (document.body.clientWidth != this.clientWidth || this.clientHeight != document.body.clientHeight) {
            this.resize2();
        }
    }

    public resize2() {
        console.log("resize2 ------------- ");
        var cvs = find('Canvas').getComponent(Canvas);
        //保存原始设计分辨率，供屏幕大小变化时使用

        GlobalConfig.CanvasNode = cvs.node;

        if (!this.curDR) {
            //获取当前视图的设计分辨率
            this.curDR = view.getDesignResolutionSize();
        }

        var dr = this.curDR;
        this.clientWidth = document.body.clientWidth;
        this.clientHeight = document.body.clientHeight
        let s = new Size(this.clientWidth, this.clientHeight);

        // console.log("clientWidth ", document.body.clientWidth, "clientHeight ", document.body.clientHeight);

        var rw = s.width;
        var rh = s.height;
        var finalW = rw;
        var finalH = rh;

        if ((rw / rh) > (dr.width / dr.height)) {
            //如果更长，则用定高
            console.log("如果更长，则用定高");
            // finalH = dr.height;
            finalW = finalH * rw / rh;
        }
        else {
            //如果更短，则用定宽
            console.log("如果更短，则用定宽 ");
            // finalW = dr.width;
            finalH = rh / rw * finalW;
        }

        console.log(`resize =>clientSize: ${this.clientWidth},${this.clientHeight}| final : ${finalW},${finalH}`);

        //将计算出来的分辨率重新设置为设计分辨率
        view.setDesignResolutionSize(finalW, finalH, 5)

        //将 Canvas 的宽高设置为新的设计分辨率
        // cvs.node.getComponent(UITransform).setContentSize(finalW, finalH)
        view.setDesignResolutionSize(finalW, finalH, ResolutionPolicy.SHOW_ALL)
        cvs.node.emit('resize');

        // let  gameCanvas:HTMLCanvasElement =  document.getElementById("GameCanvas") as HTMLCanvasElement;
        // if(gameCanvas != null)
        // {
        //     gameCanvas.width = finalW;
        //     gameCanvas.height = finalH;
        // }

        GlobalConfig.Viewport.width = finalW + 512;
        GlobalConfig.Viewport.height = finalH + 256;

        GlobalConfig.Viewport.x = -GlobalConfig.Viewport.width / 2;
        GlobalConfig.Viewport.y = -GlobalConfig.Viewport.height / 2;
    }



}

