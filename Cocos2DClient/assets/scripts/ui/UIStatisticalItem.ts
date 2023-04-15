
import { _decorator, Component, Node, Label } from 'cc';
import { StatisticalData } from '../core/StatisticalManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = UIStatisticalItem
 * DateTime = Sat Apr 23 2022 13:44:25 GMT+0800 (中国标准时间)
 * Author = wuhaishengxxx
 * FileBasename = UIStatisticalItem.ts
 * FileBasenameNoExtension = UIStatisticalItem
 * URL = db://assets/scripts/ui/UIStatisticalItem.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/zh/
 *
 */

@ccclass('UIStatisticalItem')
export class UIStatisticalItem extends Component {



    @property(Label)
    rank: Label;

    @property(Label)
    teamName: Label;

    @property(Label)
    score: Label;

    @property(Label)
    alive: Label;

    @property(Label)
    defeat: Label;

    @property(Label)
    gold: Label;


    @property(Label)
    dmgtaken: Label;

    start() {
    }

    public bindData(data: StatisticalData): void {
        this.rank.string = data.isEnd ? "" + data.rank : "-";
        this.teamName.string = data.teamName;
        this.score.string = data.isEnd ? "" + data.score : "-";
        this.alive.string = data.isEnd ? "" + data.alive:"-";
        this.defeat.string = "" + data.defeat;
        this.gold.string = "" + data.gold;
        this.dmgtaken.string = "" + Math.round(data.dmgtaken);
    }

}


