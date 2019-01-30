export {Engine};

class Engine {

   constructor(mode, aContainer) {
      this.mode = mode;
      this.container = aContainer;
      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color( 0x003333 );

      this.mesh = null; // we'll initialize from the server packet

      this.camera = new THREE.PerspectiveCamera(
              60, window.innerWidth / window.innerHeight, 1, 20000 );
      this.camera.position.y = 2 * sz;
      this.camera.position.z = 10;

      var width  = window.innerWidth; 
      var height = window.innerHeight;
      var aspect = width/height;
      var near = 0.1;
      var far = 10000;
      var fov = 90;

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      //this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setPixelRatio(2); //Antialias x2
      this.renderer.setSize( window.innerWidth, window.innerHeight );
      this.renderer.shadowMap.enabled = true;
      this.renderer.shadowMap.type = THREE.PCFSoftShadowMap

      this.initializeControls();

      this.mouse = new THREE.Vector2();
      this.raycaster = new THREE.Raycaster();
      this.clock = new THREE.Clock();

      //initialize lights
      var ambientLight = new THREE.AmbientLight( 0xcccccc, 1.0);
      this.scene.add( ambientLight );

      var pointLight = new THREE.PointLight( 0xffffff, 1.5, 0, 2 );
      pointLight.position.set( 64*40, 500, 64*40 );
      //pointLight.position.set( 0, 1500, 0 );
      pointLight.castShadow = true;
      pointLight.shadow.camera.far = 0;
      this.scene.add(pointLight);

      /*
      var light = new THREE.DirectionalLight(0xffffff);
      light.position.set(7000,500,7000);
      light.target.position.set(0,0,0);
      var clip = 1000;
      light.shadow.camera.near = near;       
      light.shadow.camera.far = far;      
      light.shadow.camera.left = -clip;
      light.shadow.camera.bottom = -clip;
      light.shadow.camera.right = clip;
      light.shadow.camera.top = clip;
      light.castShadow = true;
      light.shadow.mapSize.width = 5092;
      light.shadow.mapSize.height = 5092;
      this.scene.add(light)

      var geometry = new THREE.SphereGeometry(100, 32, 32);
      var material = new THREE.MeshPhongMaterial({color: 0x0000ff, side: THREE.DoubleSide});
      var crosscap = new THREE.Mesh(geometry, material);
      crosscap.receiveShadow = true;
      crosscap.castShadow = true;
      crosscap.position.y = 100;
      crosscap.position.x = 4380;
      crosscap.position.z = 4380;
      this.scene.add(crosscap);

      var floor_geometry = new THREE.PlaneGeometry(5000,5000);
      var floor_material = new THREE.MeshPhongMaterial({color: 0xffffff});
      var floor = new THREE.Mesh(floor_geometry,floor_material);
      floor.position.set(0,-2,0);
      floor.position.y = 50;
      floor.rotation.x -= Math.PI/2;
      floor.receiveShadow = true;
      floor.castShadow = false;
      this.scene.add(floor);
      */

      /*
      var directionalLight = new THREE.DirectionalLight( 0xffffff, 2 );
      directionalLight.position.set( 1, 0.5, 0 ).normalize();
      directionalLight.castShadow = true
      this.scene.add( directionalLight );

      var spotLight = new THREE.SpotLight( 0xffffff, 2 );
      spotLight.castShadow = true
      this.scene.add( spotLight);
      */

      document.body.appendChild( this.renderer.domElement );
   }

   initializeControls() {
      var controls = new THREE.OrbitControls(this.camera, this.container);
      controls.mouseButtons = {
         LEFT: THREE.MOUSE.MIDDLE, // rotate
         // RIGHT: THREE.MOUSE.LEFT // pan
      }
      controls.target.set( 40*sz, 0, 40*sz );
      controls.minPolarAngle = 0.0001;
      controls.maxPolarAngle = Math.PI / 2.0 - 0.1;

      controls.movementSpeed = 1000;
      controls.lookSpeed = 0.125;
      controls.lookVertical = true;

      if ( this.mode == modes.ADMIN ) {
         controls.enableKeys = true;
         controls.enablePan = true;
      }

      controls.enabled = false;
      controls.update();
      this.controls = controls;
   }

   onWindowResize() {
      this.camera.aspect = window.innerWidth / window.innerHeight;
      this.camera.updateProjectionMatrix();
      // this.controls.update();
      this.renderer.setSize( window.innerWidth, window.innerHeight );
   }

   raycast(clientX, clientY) {
      this.mouse.x = (
            clientX / this.renderer.domElement.clientWidth ) * 2 - 1;
      this.mouse.y = - (
            clientY / this.renderer.domElement.clientHeight ) * 2 + 1;
      this.raycaster.setFromCamera( this.mouse, this.camera );

      // See if the ray from the camera into the world hits one of our meshes
      var intersects = this.raycaster.intersectObject( this.mesh );

      // Toggle rotation bool for meshes that we clicked
      if ( intersects.length > 0 ) {
         var x = intersects[ 0 ].point.x;
         var y = intersects[ 0 ].point.x;
         var z = intersects[ 0 ].point.z;

         //x = Math.min(Math.max(0, Math.floor(x/sz)), worldWidth);
         // new terrain gen uses +x, -z
         //z = Math.max(Math.min(0, Math.floor(z/sz)), -worldDepth);
         // z = Math.min(Math.max(0, Math.floor(z/sz)), worldDepth);
      }

      return [x, y, z]
   }

   update(delta) {
      this.controls.update( delta );
      this.renderer.render( this.scene, this.camera );
   }

}

