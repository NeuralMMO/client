
export { Component }


class Component {

    protected Unpack(key, val: object): any {
        return val[key]
    }

    protected UnpackList(keys: string[], value): { [key: string]: any } {

        return null;
    }

    public Update(data:any)
    {
        
    }
}