import { FinalMetricsesDB, PacketDB } from "./Packet";




export interface Replay {

    map: number[][];
    packets: PacketDB[];
    metrics: FinalMetricsesDB;
}
