
import { _decorator, Component, Node, Label, Sprite, UITransform, Toggle, instantiate, EventHandler } from 'cc';
import DropdownItem from './DropdownItem';
import DropdownOptionData from './DropdownOptionData';
const { ccclass, property } = _decorator;


// 下来选中框 
@ccclass('Dropdown')
export class Dropdown extends Component {

    // 默认
    @property(Label)
    label: Label;

    // 模板
    @property(Node)
    template: Node = undefined;

    // 当前展示
    @property(Label)
    labelCaption: Label = undefined;
    // 当前展示的图图标
    @property(Sprite)
    spriteCaption: Sprite = undefined;

    @property(Label)
    labelItem: Label = undefined;

    @property(Sprite)
    spriteItem: Sprite = undefined;

    @property(EventHandler)
    onChanged: EventHandler = undefined;


    @property([DropdownOptionData])
    options: DropdownOptionData[] = [];


    private _dropDown: Node;
    private validTemplate: boolean = false;
    private items: DropdownItem[] = [];
    private isShow: boolean = false;
    private _selectedIndex: number = -1;

    private get selectedIndex(): number {
        return this._selectedIndex;
    }

    private set selectedIndex(value: number) {
        this._selectedIndex = value;
        this.refreshShownValue();
    }


    public Select(label: string): void {
        for(let i = 0 ; i < this.options.length;i++)
        {
           if( this.options[i].label == label)
           {
               this.selectedIndex = i;
           }
        }
    }
    public addOptionDatas(optionDatas: DropdownOptionData[], refresh: boolean = true) {

        optionDatas && optionDatas.forEach(data => {
            this.options.push(data);
        });

        let bottom: DropdownOptionData;

        let index = -1;

        for (let i = 0; i < this.options.length; i++) {

            if (this.options[i].alwaysBottom) {

                bottom = this.options[i];
                index = i;
                break;
            }
        }

        if (bottom != null) {
            this.options.splice(index, 1);
            this.options.push(bottom);
        }
        // if(refresh)
        // {
        //     this.refreshShownValue();
        // }

    }

    public clearOptionDatas() {
        this.options = [];
        this.refreshShownValue();
    }



    public show() {

        if (!this.validTemplate) {

            this.setUpTemplate();
            if (!this.validTemplate) { return; }
        }

        this.isShow = true;

        this._dropDown = this.createDropDownList(this.template);
        this._dropDown.name = "DropDown";
        this._dropDown.active = true;
        this._dropDown.setParent(this.template.parent);

        let itemTemplate = this._dropDown.getComponentInChildren<DropdownItem>(DropdownItem);

        let content: Node = itemTemplate.node.parent;

        itemTemplate.node.active = true;

        this.items = [];

        for (let i = 0, len = this.options.length; i < len; i++) {
            let data = this.options[i];
            let item: DropdownItem = this.addItem(data, i == this.selectedIndex, itemTemplate, this.items);

            if (!item) {
                continue;
            }
            item.toggle.isChecked = i == this.selectedIndex;
            item.toggle.node.on(Toggle.EventType.TOGGLE, this.onSelectedItem, this);
        }
        itemTemplate.node.active = false;

        content.getComponent(UITransform).height = itemTemplate.node.getComponent(UITransform).height * this.options.length;

    }

    private addItem(data: DropdownOptionData, selected: boolean, itemTemplate: DropdownItem, DropdownItems: DropdownItem[]): DropdownItem {
        let item = this.createItem(itemTemplate);
        item.node.setParent(itemTemplate.node.parent);
        item.node.active = true;
        item.node.name = `item_${this.items.length + data.label ? data.label : ""}`;
        if (item.toggle) {
            item.toggle.isChecked = false;
        }
        if (item.label) {
            item.label.string = data.label;
        }
        if (item.sprite) {
            item.sprite.spriteFrame = data.icon;
            item.sprite.enabled = data.icon != undefined;
        }
        this.items.push(item);
        return item;
    }

    public hide() {
        this.isShow = false;
        if (this._dropDown != undefined) {
            this.delayedDestroyDropdownList(0.15);
        }
    }

    private async delayedDestroyDropdownList(delay: number) {
        // await WaitUtil.waitForSeconds(delay);
        // wait delay;
        for (let i = 0, len = this.items.length; i < len; i++) {
            if (this.items[i] != undefined)
                this.destroyItem(this.items[i]);
        }
        this.items = [];
        if (this._dropDown != undefined)
            this.destroyDropDownList(this._dropDown);
        this._dropDown = undefined;
    }

    private destroyItem(item) {

    }

    // 设置模板，方便后面item
    private setUpTemplate() {
        this.validTemplate = false;

        if (!this.template) {
            console.error("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item");
            return;
        }

        this.template.active = true;

        let itemToggle: Toggle = this.template.getComponentInChildren<Toggle>(Toggle);

        this.validTemplate = true;
        // 一些判断
        if (!itemToggle || itemToggle.node == this.template) {

            this.validTemplate = false;

            console.error("The dropdown template is not valid. The template must have a child Node with a Toggle component serving as the item.");

        } else if (this.labelItem != undefined && !this.labelItem.node.isChildOf(itemToggle.node)) {
            this.validTemplate = false;
            console.error("The dropdown template is not valid. The Item Label must be on the item Node or children of it.");

        } else if (this.spriteItem != undefined && !this.spriteItem.node.isChildOf(itemToggle.node)) {
            this.validTemplate = false;
            console.error("The dropdown template is not valid. The Item Sprite must be on the item Node or children of it.");

        }

        if (!this.validTemplate) {
            this.template.active = false;
            return;
        }

        let item = itemToggle.node.addComponent<DropdownItem>(DropdownItem);
        item.label = this.labelItem;
        item.sprite = this.spriteItem;
        item.toggle = itemToggle;
        item.node = itemToggle.node;

        this.template.active = false;
        this.validTemplate = true;
    }

    // 刷新显示的选中信息
    private refreshShownValue() {


        if (this.options.length <= 0) {

            return;
        }

        let data = this.options[this.clamp(this.selectedIndex, 0, this.options.length - 1)];


        if (this.labelCaption) {
            if (data && data.label != ""  &&  data.label != "Clear") {
                this.labelCaption.string = "" + data.label;
            } else {
                this.labelCaption.string = "Team";
            }
        }

        /*
               // 显示当前信息
       
               if (this.spriteCaption) {
       
                   if (data && data.icon) {
       
                       this.spriteCaption.spriteFrame = data.icon;
       
                   } else {
                       this.spriteCaption.spriteFrame = undefined;
                   }
                   this.spriteCaption.enabled = this.spriteCaption.spriteFrame != undefined;
               }
               */

    }

    protected createDropDownList(template: Node): Node {
        return instantiate(template);
    }

    protected destroyDropDownList(dropDownList: Node) {
        dropDownList.destroy();
    }

    protected createItem(itemTemplate: DropdownItem): DropdownItem {
        let newItem = instantiate(itemTemplate.node);
        return newItem.getComponent<DropdownItem>(DropdownItem);
    }

    // 选中
    private onSelectedItem(toggle: Toggle) {

        let parent = toggle.node.parent;

        for (let i = 0; i < parent.children.length; i++) {

            if (parent.children[i] == toggle.node) {
                this.selectedIndex = i - 1;
                break;
            }
        }

        this.onChanged && this.onChanged.emit([this.options[this.selectedIndex]]);
        this.hide();
    }



    private onClick() {
        if (!this.isShow) {
            this.show();
        } else {

            this.hide();
        }
    }

    start() {
        this.template.active = false;
        // this.refreshShownValue();
        this.node.on(Node.EventType.TOUCH_END, this.onClick, this);
    }


    // destroy(): boolean {
    //     this.node.off(Node.EventType.TOUCH_END, this.onClick, this);

    //     return super.destroy();
    // }

    private clamp(value: number, min: number, max: number): number {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }



}

