
import { _decorator, Component, Node, CCInteger, Enum, CCFloat } from 'cc';
const { ccclass, property } = _decorator;


@ccclass('ZIndex')
export class ZIndex extends Component {

    r: number;
    c: number;

    // 同一格子安装优先级排序 大的在上
    @property({ type: CCFloat, displayName: "priority 越大优先级越高" })
    priority: number = 0;

    start() {
        // [3]
    }

    public compare(other: ZIndex): number {
        return this.r - other.r != 0 ? this.r - other.r : this.c - other.c != 0 ? this.c - other.c : this.priority - other.priority;
       
        /*
        if (this.r > other.r) return 1;

        if (this.r < other.r) return -1;

        // if (this.r == other.r && this.c != other.c) return this.c - other.c;

        if (this.c != other.c) return this.c - other.c;

        return this.priority - other.priority;
        */

    }

}


