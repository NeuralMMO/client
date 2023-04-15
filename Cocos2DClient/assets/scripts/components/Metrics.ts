import { MetricsDB } from "../data/Packet";
import { Component } from "./Component";

export class Metrics extends Component {

    static high = 21
    static mid = 10
    static low = 4

    public PlayerDefeats: number = 0; //kills
    public Gold: number = 0;
    public TimeAlive: number = 0;
    public DamageTaken: number; // 总计承受伤害

    public bindData(data: MetricsDB): void {
        this.PlayerDefeats = data.PlayerDefeats;
        this.Gold = data.Gold;
        this.TimeAlive = data.TimeAlive;
        this.DamageTaken = data.DamageTaken;
        
    }

    public get total(): number {
        return 0;
    }


    public static calculateScore(PlayerDefeats, Equipment, Exploration, Foraging): number {
        let score = 0

        if (PlayerDefeats >= 6)
            score += Metrics.high
        else if (PlayerDefeats >= 3)
            score += Metrics.mid
        else if (PlayerDefeats >= 1)
            score += Metrics.low

        let equipment = Equipment;
        if (equipment >= 20)
            score += Metrics.high
        else if (equipment >= 10)
            score += Metrics.mid
        else if (equipment >= 1)
            score += Metrics.low

        let exploration = Exploration

        if (exploration >= 127)
            score += Metrics.high
        else if (exploration >= 64)
            score += Metrics.mid
        else if (exploration >= 32)
            score += Metrics.low

        let foraging = Foraging
        if (foraging >= 50)
            score += Metrics.high
        else if (foraging >= 35)
            score += Metrics.mid
        else if (foraging >= 20)
            score += Metrics.low

        return score

    }
}