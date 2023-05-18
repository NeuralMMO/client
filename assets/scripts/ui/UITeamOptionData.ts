import DropdownOptionData from "./dropdown/DropdownOptionData";

export class UITeamOptionData extends DropdownOptionData {

    public invalid: boolean = false;

    constructor(data: { label?: string, invalid?: boolean, alwaysBottom?: boolean }) {

        super(data.label, null, data.alwaysBottom);
        if (data.invalid) this.invalid = data.invalid;
    }

}