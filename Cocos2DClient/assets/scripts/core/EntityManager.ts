import { instantiate, Prefab, Node, SpriteFrame, resources, math } from "cc";
import { PacketDB } from "../data/Packet";
import { Entity, EntityType } from "../entity/Entity";
import { EntityView } from "../entity/EntityView";
import { NpcData } from "../entity/NpcData";
import { PlayerData } from "../entity/PlayerData";
import { TileView } from "../entity/TileView";
import { ZIndex } from "../entity/ZIndex";
import { BattleEvent, EventManager } from "../event/Event";
import { GlobalConfig } from "./GlobalConfig";
import { ResourcesHelper } from "../utils/ResourcesHelper";
import { HeadInfo } from "../ui/HeadInfo";
import { Utils } from "../utils/Utils";
import { World } from "./World";

export class Team {

    teamName: string;
    skins: SpriteFrame[];
    color: string;
    entities: Entity[];
    population: number;

    constructor(name: string, population: number) {
        this.teamName = name;
        this.skins = [];
        this.color = ""
        this.entities = [];
        this.population = population;
    }

    public addEntity(entity: Entity): void {
        this.entities.push(entity);
    }

}

export class EntityManager {


    public static NPC_SKIN_IDS: number[] = [1, 2];

    public static PAWNS_IDS: number[] = [1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];

    static instance: EntityManager;

    public players: { [key: string]: Entity };

    public npcs: { [key: string]: Entity }

    public headInfoLayer: Node;

    public entityLayer: Node;

    public characterPrefab: Prefab;

    public headPrefab: Prefab;

    public playerSkinsTemp: number[];

    public npcSkinsTemp: number[];

    public teamMap: { [teamName: string]: Team };

    public populationTeamMap: { [population: number]: Team };
    public teams: string[];


    public init(world: World, characterPrefab: Prefab, headPrefab: Prefab): void {
        this.players = {};
        this.npcs = {};
        this.teamMap = {};
        this.teams = [];
        this.populationTeamMap = {};

        this.playerSkinsTemp = [...EntityManager.PAWNS_IDS];
        this.npcSkinsTemp = [...EntityManager.NPC_SKIN_IDS];

        this.entityLayer = world.entityLayer;

        this.headInfoLayer = world.headInfoLayer;
        this.characterPrefab = characterPrefab;
        this.headPrefab = headPrefab;

    }

    public executePacket(packet: PacketDB, step: number): void {

        this.UpdatePlayers(packet);
        this.UpdateNpcs(packet);
        this.updateZindex();
    }

    // 层级排序 TODO 优化 只排序周边的
    public updateZindex(): void {
        // console.time("updateZindex");

        // this.entityLayer.children.sort((A: Node, B: Node) => {

        //     let az = A.getComponent(ZIndex);
        //     let bz = B.getComponent(ZIndex);

        //     if (az == null || bz == null) 
        //     {
        //         console.log("az == null || bz == null");
        //         return -1;
        //     }
        //     return az.compare(bz);
        // });

        let children = this.entityLayer.children;
        children.sort((A: Node, B: Node) => {

            let az = A.getComponent(ZIndex);
            let bz = B.getComponent(ZIndex);

            if (az == null || bz == null) {
                console.log("az == null || bz == null");
                return -1;
            }
            return az.compare(bz);
        });

        // for (let i = 0; i < children.length; i++) {
        //     if (children[i].active)
        //         children[i].active = Utils.InViewport(GlobalConfig.Viewport, children[i]);
        // }


    }

    public CreateEntity(key: string, data: any, type: EntityType): Entity {
        let entity: Entity;
        switch (type) {

            case EntityType.None:
                entity = null;

                break;

            case EntityType.NPC:
                {
                    let entityData: NpcData = new NpcData(data);

                    let node = instantiate(this.characterPrefab);
                    node.setParent(this.entityLayer);

                    let view = node.getComponent(EntityView);

                    let headNode = instantiate(this.headPrefab);
                    headNode.setParent(this.headInfoLayer);
                    headNode.getComponent(HeadInfo).entityType = type;

                    entity = new Entity();
                    entity.data = entityData;
                    entity.type = type;
                    entity.view = view;
                    entity.selectable = false;
                    entity.headInfo = headNode.getComponent(HeadInfo);
                    entity.headInfo.setFollow(entity.view);

                    if (this.npcs[key] != null) console.log(key);

                    this.npcs[key] = entity;

                    if (this.npcSkinsTemp.length == 0) this.npcSkinsTemp = [...EntityManager.NPC_SKIN_IDS];

                    let index = math.randomRangeInt(0, this.npcSkinsTemp.length);
                    let id: number = this.npcSkinsTemp.splice(index, 1)[0];

                    entity.setSkins(ResourcesHelper.GetSkins(id, type));
                    let color = ResourcesHelper.GetNPCNameColorByName(entity.data.name[0].toUpperCase())
                    entity.setNameColor(color);

                    entity.setNameOutlineColor(ResourcesHelper.GetNPCNameOutlineColorByName(entity.data.name[0].toUpperCase()));

                    entity.start();
                    break;
                }

            case EntityType.Player:
                {

                    let entityData = new PlayerData(data);
                    let node = instantiate(this.characterPrefab);
                    node.setParent(this.entityLayer);

                    let view = node.getComponent(EntityView);

                    let headNode = instantiate(this.headPrefab);
                    headNode.setParent(this.headInfoLayer);
                    headNode.getComponent(HeadInfo).entityType = type;

                    entity = new Entity();
                    entity.data = entityData;
                    entity.type = type;
                    entity.view = view;
                    entity.headInfo = headNode.getComponent(HeadInfo);
                    entity.selectable = true;
                    entity.headInfo.setFollow(entity.view);

                    this.players[key] = entity;

                    let teamName = entity.teamName;

                    let team = this.teamMap[teamName];

                    if (this.teamMap[teamName] == null) {

                        team = new Team(teamName, entityData.GetPopulation());
                        this.teams.push(teamName);
                        if (this.playerSkinsTemp.length == 0) {
                            this.playerSkinsTemp = [...EntityManager.PAWNS_IDS];
                        }

                        let index = math.randomRangeInt(0, this.playerSkinsTemp.length);
                        let id: number = this.playerSkinsTemp.splice(index, 1)[0];
                        team.color = ResourcesHelper.GetColorBySkinID(id, type);
                        team.skins = ResourcesHelper.GetSkins(id, type);


                        entity.setSkins(team.skins);
                        entity.setNameColor(team.color);

                        team.addEntity(entity);
                        this.teamMap[teamName] = team;
                        this.populationTeamMap[team.population] = team;
                        EventManager.view.emit(BattleEvent.AddTeam, teamName);
                    }
                    else {
                        entity.setSkins(team.skins);
                        entity.setNameColor(team.color);

                        team.addEntity(entity);
                    }

                    entity.start();

                    break;
                }

            default:
                {
                    break;
                }

        }
        return entity;
    }



    private UpdatePlayers(packet: PacketDB): void {

        for (let key in packet.player) {

            let data = packet.player[key];

            if (this.players[key] == null) {

                this.players[key] = this.CreateEntity(key, data, EntityType.Player);
            }
            else {
                this.players[key].update(data);
            }
        }

        // 死亡被移除View
        let removeList = [];

        for (let key in this.players) {
            if (packet.player[key] == null) {
                removeList.push(key);
            }
        }

        for (let i = 0; i < removeList.length; i++) {

            let key = removeList[i];

            if (this.players[key].removed == false) {

                this.players[key].update(Utils.GetPlayerLastData(key));
                this.players[key] && this.players[key].removeView();
            }

        }
    }


    private UpdateNpcs(packet: PacketDB): void {

        for (let key in packet.npc) {

            let data = packet.npc[key];

            if (this.npcs[key] == null) {

                this.npcs[key] = this.CreateEntity(key, data, EntityType.NPC);
            }
            else {
                this.npcs[key].update(data);
            }
        }

        // 死亡被移除View 
        let removeList = [];

        for (let key in this.npcs) {

            if (packet.npc[key] == null) {

                removeList.push(key);
            }
        }

        for (let i = 0; i < removeList.length; i++) {

            let key = removeList[i];

            this.npcs[key] && this.npcs[key].removeView();
        }
    }


    public GetPlayersByTeam(team: string): Entity[] {

        return this.teamMap[team] && this.teamMap[team].entities;
    }


    public GetEntityByKey(key): Entity {
        if (this.players[key]) return this.players[key]
        if (this.npcs[key]) return this.npcs[key]
        return null;
    }

    public GetTeamByPopulation(population: number): Team {
        return this.populationTeamMap[population]
    }

    public GetTeamByName(name: string): Team {
        return this.teamMap[name];
    }


}