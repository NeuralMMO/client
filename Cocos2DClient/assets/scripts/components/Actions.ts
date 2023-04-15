import { ActionsDB, EquipmentDB, ItemDB } from "../data/Packet";
import { Component } from "./Component";
import { ItemComponent } from "./ItemComponent";

export class UseAction {
	item: ItemComponent;
}

export class BuyAction {
	item: ItemComponent;
}

export class SellAction {
	item: ItemComponent;
	price: string;
	// public bindData(data: { Item: ItemDB, Price: string }) {
	// 	this.item = new ItemComponent();
	// 	this.item.Update(data.Item);
	// 	this.Price = data.Price;
	// }
}

export class Actions extends Component {

	move: any;
	use: UseAction;
	buy: BuyAction;
	sell: SellAction;

	public Update(data: ActionsDB): void {

		if (data.Use != null) {
			this.use = new UseAction();
			this.use.item = new ItemComponent();
			this.use.item.Update(data.Use.Item);
		}
		else {
			this.use = null;
		}


		if (data.Buy != null) {
			this.buy = new BuyAction();
			this.buy.item = new ItemComponent();
			this.buy.item.Update(data.Buy.Item);
		}
		else {
			this.buy = null;
		}

		if (data.Sell != null) {
			this.sell = new SellAction();
			this.sell.item = new ItemComponent();
			this.sell.item.Update(data.Sell.Item);
			this.sell.price = data.Sell.Price;
		} else {
			this.sell = null;
		}
	}
}