
import { _decorator, Component, Node, js, Prefab, instantiate } from 'cc';
import { StatisticalData, StatisticalManager } from '../core/StatisticalManager';
import { UIStatisticalItem } from './UIStatisticalItem';
const { ccclass, property } = _decorator;

 // 统计面板 
@ccclass('UIStatisticalData')
export class UIStatisticalData extends Component {
  
    // 1 s 更新
    static readonly UPDATE_DELAY = 1000;// 1000ms 

    @property(Prefab)
    itemPrefab: Prefab

    @property([UIStatisticalItem])
    items: UIStatisticalItem[] = [];



    @property(Node)
    content: Node

    _lastUpdateTime: number = 0;

    start() {
        this.BindStatisticalData();
    }

    public BindStatisticalData(): void {

        let datas = StatisticalManager.instance.GetItems();

        for (let i = 0; i < datas.length; i++) {

            if (i < this.items.length) {
                this.items[i].bindData(datas[i]);
            }
            else {
                let itemNode = instantiate(this.itemPrefab);
                itemNode.getComponent(UIStatisticalItem).bindData(datas[i]);
                this.items.push(itemNode.getComponent(UIStatisticalItem));
                this.content.addChild(itemNode);
            }
        }

        if (datas.length < this.items.length) {
            let removeList = this.items.slice(datas.length, this.items.length - datas.length);
            removeList.forEach(element => {
                element.node.destroy();
            });
        }


    }

    public update(delta: number): void {

        if (this.node.active && Date.now() - this._lastUpdateTime > UIStatisticalData.UPDATE_DELAY) {
            this._lastUpdateTime = Date.now();
            this.BindStatisticalData();
        }
    }

}

