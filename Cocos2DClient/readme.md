
# 开发环境

[CocosDashboard](https://download.cocos.com/CocosDashboard/v1.1.2/CocosDashboard-v1.1.2-win-042820.exe)

[cocos creator 3.4.2](从CocosDashboard下)



# 重要目录
assets   
  |  
  |-- prefab UI，角色，技能等预设体  
  |  
  |-- resources 所有素材  
  |  
  |-- scene    
       |-- Main.scene 主界面场景，加载Replay  
       |-- Game.scene 游戏场景  
  |  
  |-- scripts 所有代码目录  

build  发布路径  

  

# 配置

** cocos 的打包机制 不单独作为外部json配置  
assets/scripts/GlobalConfig.ts  



# 发布

修改发布template  

## 文件路径  
```
  // CocosDashboard\resources\.editors 可以通过IDE右上角 “编辑器”打开
  CocosDashboard\resources\.editors\Creator\3.4.2\resources\app.asar.unpacked\platforms\internal\web-desktop\static\build-template
```
  
css修改的地方 style.css   
```css
body {
  cursor: default;
  padding: 0;
  border: 0;
  margin: 0;

  text-align: center;
  background-color: rgb(0, 0, 0); /* 修改底图颜色 */ 
  font-family: Helvetica, Verdana, Arial, sans-serif;
}

#Cocos2dGameContainer {
  position: absolute;
  margin: 0;
  overflow: hidden;
  left: 0px;
  top: 0px;
}


#GameDiv {
    margin: 0 auto;
    background: black;
    position:relative;
    border:5px solid black;
    border-radius: 10px;
    box-shadow: 0 5px 50px #333;
    overflow: hidden;
}

#Cocos3dGameContainer, #GameCanvas {
  width: 100%;
  height: 100%;
  overflow: hidden;
}

  
```  

index.ejs  

```html  
<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8" />

    <title><%= projectName %></title>

    <meta name="viewport" content="width=device-width,user-scalable=no,initial-scale=1,minimum-scale=1,maximum-scale=1,minimal-ui=true" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="full-screen" content="yes" />
    <meta name="screen-orientation" content="portrait" />
    <meta name="x5-fullscreen" content="true" />
    <meta name="360-fullscreen" content="true" />

    <meta name="renderer" content="webkit" />
    <meta name="force-rendering" content="webkit" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" />

    <link rel="stylesheet" type="text/css" href="<%= cssUrl %>" />
    <link rel="icon" href="favicon.ico" />
  </head>
  <body>
    <div id="GameDiv" style="width: <%= previewWidth %>px; height: <%= previewHeight %>px">
      <div id="Cocos3dGameContainer">
        <canvas id="GameCanvas" width="<%= previewWidth %>" height="<%= previewHeight %>"></canvas>
      </div>
    </div>
  </body>
</html>

```

替换 favicon.ico   

## 去掉Cocos开屏   

启动屏幕需要修改 build/发布路径/src/setting.json  
如： build/release/src/setting.json  

1. 删除 "base64src": 字段  
2. "displayWatermark":true, 修改为 "displayWatermark":false  
3. "exactFitScreen": false, 改为  "exactFitScreen": true,  

参考   
```json
{
    "debug": false,
    "CocosEngine": "3.4.2",
    "designResolution": {
        "width": 1920,
        "height": 1080,
        "policy": 1
    },
    "platform": "web-desktop",
    "exactFitScreen": true,
    "bundleVers": {},
    "subpackages": [],
    "remoteBundles": [],
    "hasResourcesBundle": true,
    "hasStartSceneBundle": false,
    "launchScene": "db://assets/scene/Main.scene",
    "jsList": [],
    "moduleIds": [],
    "renderPipeline": "",
    "engineModules": [],
    "server": "",
    "customLayers": [
        {
            "name": "WORLD",
            "bit": 14
        },
        {
            "name": "MAP",
            "bit": 15
        }
    ],
    "splashScreen": {
        "displayRatio": 0.4,
        "totalTime": 3000,
        "effect": "",
        "clearColor": {
            "x": 0.88,
            "y": 0.88,
            "z": 0.88,
            "w": 1
        },
        "displayWatermark": false
       
    },
    "customJointTextureLayouts": [],
    "physics": {
        "physicsEngine": "",
        "gravity": {
            "x": 0,
            "y": -10,
            "z": 0
        },
        "allowSleep": true,
        "sleepThreshold": 0.1,
        "autoSimulation": true,
        "fixedTimeStep": 0.0166667,
        "maxSubSteps": 1,
        "defaultMaterial": {
            "friction": 0.5,
            "rollingFriction": 0.1,
            "spinningFriction": 0.1,
            "restitution": 0.1
        }
    },
    "scriptPackages": [
        "./src/chunks/bundle.js"
    ]
}
```

# 第三方插件  
无

# 2022-8 

* 新增 Worker多线程加载方案,避免Loading进度条不动
 1. 修改 Main.ts
 2. 新增 replace\libs\worker.js


* 构建发布流程 
1. 编辑器构建发布
2. 拷贝 NeuralMMO/replace 下文件到  NeuralMMO/build/release 目录下


*  lzma修改 （修改内存问题）
```js
// 只修改 LZMA.oStream.prototype.toString 
LZMA.oStream.prototype.toString = function toString() {
    var buffers = this.buffers,
      string = "";
    // optionally get the UTF8 codepoints
    // possibly avoid creating a continous buffer
    if (LZMA.UTF8) buffers = [this.toCodePoints()];

    for (var n = 0, nL = buffers.length; n < nL; n++) {
      let str = "";
      for (var i = 0, iL = buffers[n].length; i < iL; i++) {
        str += String.fromCharCode(buffers[n][i]);
	    	buffers[n][i] = null;
      }
      str.replace(/[\r\n]+/gi, "");
      string += str;
      str = null;
    }
    return string;
  };
```


