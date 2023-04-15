
import { _decorator, Component, Node, HtmlTextParser, CCString, director, Label, resources, SpriteFrame, assetManager, AssetManager, view, ResolutionPolicy, url, randomRange, math } from 'cc';
import { GlobalConfig } from './core/GlobalConfig';
import { ResourcesHelper } from './utils/ResourcesHelper';

//  import {LZMA} from  "../libs/lzma.js" ;

const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = Main
 * DateTime = Sat Apr 16 2022 11:22:11 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = Main.ts
 * FileBasenameNoExtension = Main
 * URL = db://assets/scripts/Main.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('Main')
export class Main extends Component {

    // [1]
    // dummy = '';

    // [2]
    // @property
    // serializableDummy = 0;

    @property(Label)
    uploadLabel: Label;

    @property(Label)
    loadingLabel: Label;


    @property(Node)
    btnUpload: Node;

    file: HTMLInputElement;

    start() {
        let urlInfo = this.GetQueryString(window.location)
        if (urlInfo == 'LOCAL') {
            this.initLocalUpload();
        }
        else if (urlInfo == 'ERROR') {
            alert('The URL link does not exist. Please check it !!!')
        }
        else {
            this.initWebUpload(urlInfo)
        }
    }


    public initWebUpload(urlInfo: string): void {

        resources.loadDir(ResourcesHelper.character);

        this.loadingLabel.string = "Downloading Replay File . . .";
        this.uploadLabel.string = "0%";

        this.loadingLabel.node.active = true;
        this.uploadLabel.node.active = true;
        this.btnUpload.active = false;

        let totalTimes = 18;
        let times = 1;
        var jsonFile: string;
        var isDecompressed: boolean = false;
        let processBar = function () {
            times++;
            if (times <= totalTimes) {
                this.uploadLabel.string = (100 * times / totalTimes).toFixed(1) + "%";
            }

            if (times == 3) {
                var oReq = new XMLHttpRequest();
                oReq.open("GET", urlInfo, true);
                oReq.responseType = "arraybuffer";
                oReq.send();


                oReq.onload = function (oEvent) {
                    if (window["LZMA"] == undefined) return;

                    var buffer = oReq.response;
                    var input = new window["LZMA"].iStream(buffer);
                    var output = new window["LZMA"].oStream();
                    window["LZMA"].decompressFile(input, output);
                    jsonFile = output.toString();
                    isDecompressed = true;
                };
            }

            if (isDecompressed) {
                GlobalConfig.replay = JSON.parse(jsonFile as string);
                director.loadScene("Game");
            }

            //判断进度条是否加载完全 
            if (times == totalTimes + 1) {
                this.unschedule(processBar);
            }
        }

        this.schedule(processBar, 0.3);

        // let totalTimes = 13;
        // for ( let i=0; i<4; i++ ){
        //     let time = 100000;
        //     while (time>0){
        //         time--;
        //     }
        //     this.uploadLabel.string = (100*i/totalTimes).toFixed(1) + "%";
        // }

    }


    public initLocalUpload(): void {
        resources.loadDir(ResourcesHelper.character);
        resources.loadDir(ResourcesHelper.Map);

        this.loadingLabel.string = "Loading . . .";
        this.uploadLabel.string = "0%";

        let file: HTMLInputElement = document.createElement("input");
        let jsonFile: Boolean = true;
        file.type = "file";
        file.name = "fileToUpload";
        file.accept = ".json, .replay, .lzma, .xz";

        file.style.width = "10px";
        file.style.height = "10px";
        file.style.top = "0";
        file.style.left = "0";
        file.style.right = "0";
        file.style.bottom = "0";
        file.style.margin = "auto";
        file.style.position = "absolute";
        file.style.fontSize = "30px";
        file.style.opacity = "0";


        let fileReader: FileReader = new FileReader();
        const self = this;

        file.onchange = function (e): void {

            if (file.files.length > 0) {

                if (self.loadingLabel) self.loadingLabel.node.active = true;

                if (self.uploadLabel) self.uploadLabel.node.active = true;

                self.btnUpload && (self.btnUpload.active = false);

                var fileName = file["files"][0].name
                var fileType = fileName.split('.').pop().toLowerCase();
                if (fileType == 'json') {
                    fileReader.readAsText(file["files"][0]);
                }
                else {
                    console.log(fileName)
                    jsonFile = false;
                    fileReader.readAsArrayBuffer(file["files"][0]);
                }
            }
            else {
                console.log("files error ");
            }
        };


        fileReader.onload = function (evt): void {

            if (FileReader.DONE == fileReader.readyState) {
                self.onFileLoad(fileReader, jsonFile, file, self);
            }
        };

        fileReader.onprogress = (ev: ProgressEvent<FileReader>) => {
            self.uploadLabel.string = (ev.loaded / ev.total * 100).toFixed(1) + "%";
        };

        document.body.appendChild(file);
        this.file = file;
    }



    private async onFileLoad(fileReader, jsonFile, file, self) {

        
        ResourcesHelper.PrepareLoad();

        try {
            if (jsonFile == true) {
                GlobalConfig.replay = JSON.parse(fileReader.result as string);
                file.remove();
                director.loadScene("Game");
            }
            else {

                if (window["LZMA"] == undefined) return;

                //  不使用worker 多线程
                // this.autoDecode(fileReader, file);

                //-----------  使用多线程 ----------- 
                // 模拟进度 
                self.loadingLabel.string = "Decompressing . . .";
                self.uploadLabel.string = "0%";
                self.loadingLabel.string = "Decompressing . . .";
                let step = 0;
                let process = 0;
                let delay = 0;;
                // 动态变化进度条
                let interval = setInterval(() => {
                    // 固定间隔300展示 ...
                    if (delay > 10) {
                        delay = 0;
                        self.loadingLabel.string = "Decompressing "
                      
                        for (let i = 0; i < step; i++) {
                            self.loadingLabel.string += " .";
                        }
                        step++;
                        step = step > 3 ? 0 : step;
                    }
                    else {
                        delay++;
                    }
                    // 不固定间隔变化进度 ， 10%的概率变化进度条 
                    if (math.random() >= 0.9) {
                        // 随机进度条
                        process += randomRange(0.1, 5);
                        process = Math.min(process, 99.99);
                        self.uploadLabel.string = process.toFixed(2) + "%";
                    }
                }, 30);

                // 采用wotker 
                this.decode(fileReader, file, () => { clearInterval(interval) });
            }

        } catch (error) {
            console.log("error =>", error);
        }
    }

    // worker decode
    public decode(fileReader, file, callback = null): void {
        //worker
        window.Worker != null ? this.workerDecode(fileReader, file, callback) : this.autoDecode(fileReader, file, callback);
    }

    public autoDecode(fileReader, file, callback = null): void {
        var input = new window["LZMA"].iStream(fileReader.result);
        var output = new window["LZMA"].oStream();

        console.time("decompressFile");
        window["LZMA"].decompressFile(input, output);
        console.timeEnd("decompressFile");

        console.time("JSON.parse");
        GlobalConfig.replay = JSON.parse(output.toString() as string);
        console.timeEnd("JSON.parse");

        callback && callback();
        file.remove();
        director.loadScene("Game");
    }

    public workerDecode(fileReader, file, callback = null): void {
        try {
            const self = this;
            const myWorker = new Worker("./libs/worker.js");
            myWorker.onerror = ()=>{
                self.autoDecode(fileReader, file, callback);
            }
            myWorker.postMessage(fileReader.result);

            myWorker.onmessage = function (e) {
                try {
                    GlobalConfig.replay = e.data;
                    file.remove();
                    director.loadScene("Game");
                    callback && callback();
                } catch (e) {
                    self.autoDecode(fileReader, file, callback);
                }

            }
        } catch (e) {
            this.autoDecode(fileReader, file, callback);
        }
    }

    // public decodeWorker(result): any {


    //     if (window.Worker) {
    //         const myWorker = new Worker("worker.js");
    //         myWorker.postMessage(result);
    //         myWorker.onmessage = function (e) {
    //             result.textContent = e.data;
    //         }
    //     }
    //     else {
    //         var input = new window["LZMA"].iStream(result);
    //         var output = new window["LZMA"].oStream();
    //         window["LZMA"].decompressFile(input, output);
    //         let res = JSON.parse(output.toString() as string);
    //         return res;
    //     }
    // }


    public onFileClick(): void {

        this.file && this.file.click();
    }

    public GetQueryString(url) {

        // var url1 = 'https://ijcai2022-viewer.nmmo.org/?file=https://nmmo-1251735782.cos.accelerate.myqcloud.com/replays/submissions/187935/replay-PVE_STAGE3.lzma'
        // var url1 = 'https://ijcai2022-viewer.nmmo.org/?file=https%3A%2F%2Fnmmo-1251735782.cos.accelerate.myqcloud.com%2Freplays%2Fsubmissions%2F187935%2Freplay-PVE_STAGE3.lzma'
        // var url1 = 'https://ijcai2022-viewer.nmmo.org/?file=https://service-e5fgw204-1251735782.hk.apigw.tencentcs.com/replays/submissions/181213/replay-PVE_STAGE2.lzma'
        // var url1 = 'https://ijcai2022-viewer.nmmo.org/?file=https%3A%2F%2Fservice-e5fgw204-1251735782.hk.apigw.tencentcs.com%2Freplays%2Fsubmissions%2F181213%2Freplay-PVE_STAGE2.lzma'

        var r = url.toString().split('?')[1];
        if (r === undefined) {
            return 'LOCAL'
        }
        else {
            r = r.replace(/%2F/g, '/').replace(/%3A/g, ':')
            var parsedUrl = r.split('=')[1].split('/')
            // 分别判定域名和文件格式是否符合规定
            if (parsedUrl[2] == 'service-e5fgw204-1251735782.hk.apigw.tencentcs.com' && parsedUrl.pop().split('.')[1] == 'lzma') {
                return decodeURI(r.split('=')[1])
            }
            else {
                return 'ERROR'
            }

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
