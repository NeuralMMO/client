export {loadObj, loadNN};


function loadObj(objf, mtlf) {
    var container = new THREE.Object3D();
    var obj;

    function onMTLLoad( materials ) {
        materials.preload();

        var objLoader = new THREE.OBJLoader();
        objLoader.setMaterials( materials );
        //objLoader.setPath( path );

        function onOBJLoad(object) {
           obj = object;
           obj.scale.x = 50;
           obj.scale.y = 50;
           obj.scale.z = 50;
           container.add(obj)
        }
        objLoader.load( objf, onOBJLoad);
    }

    var mtlLoader = new THREE.MTLLoader();
    //mtlLoader.setPath( path );
    mtlLoader.load( mtlf, onMTLLoad);
    return container
}

function loadNN(color) {
    var objf = 'resources/nn.obj';
    var mtlf = 'resources/nn.mtl';
    var container = new THREE.Object3D();
    var obj;

    function onMTLLoad( materials ) {
        materials.preload();

        var objLoader = new THREE.OBJLoader();
        objLoader.setMaterials( materials );

        function onOBJLoad(object) {
           obj = object;
           obj.scale.x = 50;
           obj.scale.y = 50;
           obj.scale.z = 50;
           obj.children[0].material.color.setHex(parseInt(color.substr(1), 16));

           container.add(obj)
        }
        objLoader.load( objf, onOBJLoad);
    }

    var mtlLoader = new THREE.MTLLoader();
    mtlLoader.load( mtlf, onMTLLoad);
    return container
}

