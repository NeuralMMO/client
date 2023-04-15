import { math, Size, Vec2, Node, Rect, Canvas } from "cc";
import { Replay } from "../data/Replay";
import { TileType } from "./TileType";

export class GlobalConfig {

    public static readonly MAX_SPEED = 8;
    public static readonly MIN_SPEED = 1;
    // 子弹基础飞行时间 
    public static readonly BASE_FLY_TIME = 0.2;//0.5s
    // 菱形夹角的一半
    public static readonly RADIAN = math.toRadian(30);//math.toRadian(30);//math.toRadian(35);

    // 岩浆宽度 16个格子
    public static readonly BORDER_SIZE = 16;
    public static readonly TILE_SIZE_25D = new Size(256, 285);// new Size(330, 262);// new Size(256, 284);
    public static readonly TILE_SIZE = new Size(128, 128);

    public static MapSize: number = 160;

    // 计算出来的地图像素 
    public static MapPixelSize: Size = new Size(128, 128);
    //  地块像素
    public static TilePixelSize = new Size(128, 128);
    // 真实地图像素  剔除岩浆 
    public static RealMapPixelSize: Size = new Size(128, 128);
    public static Border = 16;
    // 岩浆像素宽
    public static BorderWidth: number = GlobalConfig.TilePixelSize.x * GlobalConfig.BORDER_SIZE;

    public static LastDataMap: { [key: string]: any } = {};
    public static replay: Replay;
    public static step: number = 0;

    public static SpeedRate: number = 1;
    public static SpeedRates: number[] = [0.5, 1, 2, 4, 8];

    public static Scales = [0.04, 0.1, 0.4];
    public static ScaleIndex = 0;

    public static CurScale: number = 0.04;
    public static LastScale: number = 0.04;

    public static MinScale: number = 0.04;
    public static MaxScale: number = 0.4;

    public static ScaleStep: number = 0.1;

    // 600ms 执行一个packet 
    public static StepTick: number = 600;
    // 真实执行间隔 = StepTick / 倍速 毫秒
    public static get TickFrac() { return GlobalConfig.StepTick / GlobalConfig.SpeedRate; };

    public static Use25D: boolean = true;
    //   //  图片按菱形中心对齐 ,长的一半
    //   let w = GlobalConfig.TileSize.x / 2;

    //   // 菱形高度 
    //   let hH: number = w * Math.sin(GlobalConfig.RADIAN);

    // 菱形长，高
    public static RhombusSize: Size = new Size(GlobalConfig.TilePixelSize.x, GlobalConfig.TilePixelSize.x * Math.sin(GlobalConfig.RADIAN));
    // 菱形长，高 的一半 
    public static HRhombusSize: Size = new Size(GlobalConfig.RhombusSize.x / 2, GlobalConfig.RhombusSize.y / 2);

    // public static  sacleStep   GlobalConfig
    public static Pause: boolean = true;
    public static PosZIndexMap: { [key: string]: { type: TileType, value: number } } = {};
    
    public static FlyTime = GlobalConfig.BASE_FLY_TIME  / GlobalConfig.SpeedRate;
    public static isEnd: boolean = false;

    public   static  Viewport:Rect = new Rect();

    public   static  CanvasNode :Node;
}