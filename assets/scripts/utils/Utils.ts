import { math, Node, Prefab, Rect, SpriteFrame, UITransform, Vec2, Vec3 } from "cc";
import { GlobalConfig } from "../core/GlobalConfig";

export class Utils {



    public static GetPlayerLastData(key: string): any {

        if (GlobalConfig.LastDataMap[key] == null && GlobalConfig.replay != null && GlobalConfig.replay.packets != null) {

            let packets = GlobalConfig.replay.packets;

            for (let i = packets.length - 1; i > 0; i--) {

                if (packets[i].player[key] != null) {

                    GlobalConfig.LastDataMap[key] = packets[i].player[key];

                    break;
                }
            }
        }

        return GlobalConfig.LastDataMap[key];

    }



    public static GetDragPos(target: Vec3): Vec3 {



        let w = GlobalConfig.RealMapPixelSize.x * GlobalConfig.CurScale / 2;
        let h = GlobalConfig.RealMapPixelSize.y * GlobalConfig.CurScale / 2;

        let x = math.clamp(target.x, -w, w);
        let y = math.clamp(target.y, -h, h);

        return new Vec3(x, y);
    }

    //  根据r,c得到像素点 格子中心点的位置
    public static GetTilePos(r: number, c: number): Vec3 {

        if (GlobalConfig.Use25D) {

            //  图片按菱形中心对齐 ,长的一半
            let w = GlobalConfig.HRhombusSize.width;
            // 菱形高度 
            let h: number = GlobalConfig.HRhombusSize.height;

            let x = -w * r + c * w;

            let y = (GlobalConfig.MapSize - r - c) * h - GlobalConfig.TilePixelSize.y / 2;

            return new Vec3(x, y, y);

        }
        else {
            let x = (r - GlobalConfig.MapSize / 2) * GlobalConfig.TilePixelSize.x;
            let y = (c - GlobalConfig.MapSize / 2) * GlobalConfig.TilePixelSize.y;
            return new Vec3(x, y, y);
        }
    }

    // 根据r,c获得 块（2.5D为菱形中心）
    public static RCToTileOrigin(r: number, c: number): Vec3 {

        if (GlobalConfig.Use25D) {

            //  图片按菱形中心对齐 ,长的一半
            let w = GlobalConfig.HRhombusSize.width;
            // 菱形高度的一半 
            let h: number = GlobalConfig.HRhombusSize.height;

            // 菱形中心 x  
            let x = -w * r + c * w;

            //  菱形中心 Y
            let y = (GlobalConfig.MapSize - r - c) * h - h;

            return new Vec3(x, y, y);

        }
        else {  // 2D 直接取中心 

            let x = (r - GlobalConfig.MapSize / 2) * GlobalConfig.TilePixelSize.x;
            let y = (c - GlobalConfig.MapSize / 2) * GlobalConfig.TilePixelSize.y;
            return new Vec3(x, y, y);
        }
    }


    // 像素转换为格子 位置 
    public static PosToTile(pos: Vec3): Vec2 {

        let x = (pos.x / GlobalConfig.TilePixelSize.x / 2) - GlobalConfig.MapSize / 2;
        let y = (pos.y / GlobalConfig.TilePixelSize.y / 2) - GlobalConfig.MapSize / 2;


        return Vec2.ZERO;
    }

    public static PosToTile2D(pos: Vec2): Vec2 {

        pos.x = pos.x + GlobalConfig.TilePixelSize.x / 2;
        pos.y = pos.y + GlobalConfig.TilePixelSize.y / 2;

        let r = Math.floor(pos.x / GlobalConfig.TilePixelSize.x) + GlobalConfig.MapSize / 2;
        let c = Math.floor(pos.y / GlobalConfig.TilePixelSize.y) + GlobalConfig.MapSize / 2;

        r = Math.max(0, Math.min(GlobalConfig.MapSize - 1, r));
        c = Math.max(0, Math.min(GlobalConfig.MapSize - 1, c));

        return new Vec2(r, c);
    }


    public static ConvertToRoman(num) {
        var ans = "";
        var k = Math.floor(num / 1000);
        var h = Math.floor((num % 1000) / 100);
        var t = Math.floor((num % 100) / 10);
        var o = num % 10;
        var one = ['I', 'II', 'III', 'IV', 'V', 'VI', 'VII', 'VIII', 'IX'];
        var ten = ['X', 'XX', 'XXX', 'XL', 'L', 'LX', 'LXX', 'LXXX', 'XC'];
        var hundred = ['C', 'CC', 'CCC', 'CD', 'D', 'DC', 'DCC', 'DCCC', 'CM']
        var thousand = 'M';
        for (var i = 0; i < k; i++) {
            ans += thousand;
        }
        if (h)
            ans += hundred[h - 1];
        if (t)
            ans += ten[t - 1];
        if (o)
            ans += one[o - 1];
        return ans;
    }


    public static InViewport(viewPort: Rect, node: Node): boolean {
        let pos = new Vec3(0, 0, 0);
        pos = node.getComponent(UITransform).convertToWorldSpaceAR(pos);
        pos = GlobalConfig.CanvasNode.getComponent(UITransform).convertToNodeSpaceAR(pos);
        return viewPort.contains(new Vec2(pos.x, pos.y));
    }


}