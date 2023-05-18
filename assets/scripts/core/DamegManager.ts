
import { _decorator, Component, Node, Vec3, Label, Prefab, instantiate, Color } from 'cc';
import { EntityView } from '../entity/EntityView';
import { UIDamge } from '../ui/UIDamge';
const { ccclass, property } = _decorator;



@ccclass('DamgeManager')
export class DamgeManager extends Component {


    static ColorDic: { [key: string]: Color } =
        {
            "damge": new Color().fromHEX("#C30303"),
            "gold": new Color().fromHEX("#EAE509"),
            "poultice": new Color().fromHEX("#2B7A0B"),
        };


    @property(Prefab)
    damagePrefab: Prefab;

    public static instance: DamgeManager;

    start() {
        DamgeManager.instance = this;
    }


    public showDamge(value: number, pos: Vec3, follow: EntityView = null): void {
        let node = instantiate(this.damagePrefab);
        node.position = pos;
        let  damge =  node.getComponent(UIDamge);
        damge.setText("HP" + value, DamgeManager.ColorDic["damge"]);
        damge.follow = follow;
        node.setParent(this.node);
    }

    // 获得黄金
    public showGetGold(value: number, pos: Vec3, follow: EntityView = null): void {

        let node = instantiate(this.damagePrefab);
        node.position = pos;
        node.getComponent(UIDamge).setText("Gold+" + value, DamgeManager.ColorDic["gold"]);
        node.getComponent(UIDamge).follow = follow;
        node.setParent(this.node);

    }

    // 使用药物
    public showUsePoultice(msg: string, pos: Vec3, follow: EntityView = null): void {

        let node = instantiate(this.damagePrefab);
        node.position = pos;
        node.getComponent(UIDamge).setText(msg, DamgeManager.ColorDic["poultice"]);
        node.getComponent(UIDamge).follow = follow;
        node.setParent(this.node);
    }


}
