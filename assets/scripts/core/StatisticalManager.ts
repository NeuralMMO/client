import { Entity } from "../entity/Entity";
import { EntityView } from "../entity/EntityView";
import { EntityManager } from "./EntityManager";
import { GlobalConfig } from "./GlobalConfig";


export class StatisticalData {

    static high = 21
    static mid = 10
    static low = 4

    public rank: number;
    public teamName: string;

    public score: number;
    public alive: number;
    public defeat: number;
    public gold: number;
    public dmgtaken: number;
    isEnd: boolean;
    /*
    constructor(team, PlayerDefeats, Equipment, Exploration, Foraging) {
        this.teamName = team;
        this.kills = PlayerDefeats;
        this.equip = Equipment;
        this.explore = Exploration;
        this.forage = Foraging;
        this.score = StatisticalData.calculateScore(PlayerDefeats, Equipment, Exploration, Foraging);
    }
    */

    constructor(teamName: string, alive: number, totalDefeat: number, maxGold: number, maxDmgtaken: number, isEnd: boolean) {
        this.teamName = teamName;
        this.alive = alive;
        this.defeat = totalDefeat;
        this.gold = maxGold;
        this.dmgtaken = maxDmgtaken;

        this.score = this.defeat + this.alive;

        this.isEnd = isEnd;
        // StatisticalData.calculateScore(PlayerDefeats, Equipment, Exploration, Foraging);
    }
    static calculateScore(PlayerDefeats: any, Equipment: any, Exploration: any, Foraging: any): number {
        throw new Error("Method not implemented.");
    }




    /*
    public static calculateScore(PlayerDefeats, Equipment, Exploration, Foraging): number {
        let score = 0

        if (PlayerDefeats >= 6)
            score += StatisticalData.high
        else if (PlayerDefeats >= 3)
            score += StatisticalData.mid
        else if (PlayerDefeats >= 1)
            score += StatisticalData.low

        let equipment = Equipment;
        if (equipment >= 20)
            score += StatisticalData.high
        else if (equipment >= 10)
            score += StatisticalData.mid
        else if (equipment >= 1)
            score += StatisticalData.low

        let exploration = Exploration

        if (exploration >= 127)
            score += StatisticalData.high
        else if (exploration >= 64)
            score += StatisticalData.mid
        else if (exploration >= 32)
            score += StatisticalData.low

        let foraging = Foraging
        if (foraging >= 50)
            score += StatisticalData.high
        else if (foraging >= 35)
            score += StatisticalData.mid
        else if (foraging >= 20)
            score += StatisticalData.low

        return score

    }
    */
}


// 统计工具 
export class StatisticalManager {


    static instance: StatisticalManager;



    //  获得每支队伍的统计数据 
    public GetItems(): StatisticalData[] {


        let items: StatisticalData[] = [];
        let teams = EntityManager.instance.teams;

        for (let i = 0; i < teams.length; i++) {
            let item: StatisticalData = this.calculateStatisticalData(teams[i]);
            items.push(item);
        }

        items.sort((A, B) => { return B.score - A.score });

        for (let i = 0; i < items.length; i++) {
            items[i].rank = i + 1;
        }
        return items;
    }




    // 计算单个队伍的统计数据 
    public calculateStatisticalData(teamName: string): StatisticalData {

        if (GlobalConfig.isEnd) {

            let team = EntityManager.instance.GetTeamByName(teamName);


            let AliveScore = GlobalConfig.replay.metrics.AliveScore[team.population];
            let DamageTaken = GlobalConfig.replay.metrics.DamageTaken[team.population];
            let DefeatScore = GlobalConfig.replay.metrics.DefeatScore[team.population];
            let Gold = GlobalConfig.replay.metrics.Gold[team.population];
            let TimeAlive = GlobalConfig.replay.metrics.TimeAlive[team.population];
            let TotalScore = GlobalConfig.replay.metrics.TotalScore[team.population];

            /*
            let metric: FinalMetricsDB = GlobalConfig.replay.metrics[team.population];
            if(metric == null)
            {
                metric =  GlobalConfig.replay.metrics[teamName];
            }
            if (metric == null) {
                console.error("no metric teamName = ", teamName,team.population);
            }
            else {
                    
                */
            return new StatisticalData(teamName, AliveScore != null ? AliveScore : 0, 
                DefeatScore != null ? DefeatScore : 0, 
                Gold != null ? Gold : 0, DamageTaken!= null ?DamageTaken:0 , true);

        } else {
            let entitys: Entity[] = EntityManager.instance.GetPlayersByTeam(teamName);

            let maxAlive = 0;
            let totalDefeat = 0;
            let maxGold = 0;
            let maxDmgtaken = 0;

            for (let i = 0; i < entitys.length; i++) {
                let metrics = entitys[i].data.metrics;

                maxAlive = Math.max(maxAlive, metrics.TimeAlive);
                totalDefeat += metrics.PlayerDefeats;
                /*
                maxGold = Math.max(maxGold, metrics.Gold);
                maxDmgtaken = Math.max(maxDmgtaken, metrics.DamageTaken);
                */

                maxGold +=  metrics.Gold;
                maxDmgtaken +=   metrics.DamageTaken;

            }
            return new StatisticalData(teamName, maxAlive, totalDefeat / 2, maxGold, maxDmgtaken, false);
        }

    }


}