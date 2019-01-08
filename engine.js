class Engine {

   constructor() {
      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color( 0x006666 );

      // initialize map
      // var map = new Terrain( false ); // flat = False
      // this.mesh = map.getMapMesh();
      // this.scene.add( this.mesh );
      this.mesh = null;

      this.camera = new THREE.PerspectiveCamera(
              60, window.innerWidth / window.innerHeight, 1, 20000 );
      this.camera.position.y = 2 * sz;
      // this.camera.position.y = map.getY(
      //      worldHalfWidth, worldHalfDepth ) * sz + 2 * sz;
      this.camera.position.z = 10;

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth, window.innerHeight );

      this.initializeControls();

      this.mouse = new THREE.Vector2();
      this.raycaster = new THREE.Raycaster();
      this.clock = new THREE.Clock();

      // initialize lights
      //var ambientLight = new THREE.AmbientLight( 0xcccccc );
      //this.scene.add( ambientLight );

      var directionalLight = new THREE.DirectionalLight( 0xffffff, 2 );
      directionalLight.position.set( 1, 1, 0.5 ).normalize();
      this.scene.add( directionalLight );

      document.body.appendChild( this.renderer.domElement );
   }

   initializeControls() {
      var controls = new THREE.OrbitControls(this.camera, container);
      controls.mouseButtons = {
         LEFT: THREE.MOUSE.MIDDLE, // rotate
         RIGHT: THREE.MOUSE.LEFT // pan
      }
      controls.target.set( 0, 0, 0 );
      controls.enablePan = false;
      controls.minPolarAngle = 0.0001;
      controls.maxPolarAngle = Math.PI / 2.0 - 0.1;
      controls.movementSpeed = 1000;
      controls.lookSpeed = 0.125;
      controls.lookVertical = true;
      this.controls = controls;
   }

   onWindowResize() {
      this.camera.aspect = window.innerWidth / window.innerHeight;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize( window.innerWidth, window.innerHeight );
   }

   raycast(clientX, clientY) {
      this.mouse.x = (
            clientX / engine.renderer.domElement.clientWidth ) * 2 - 1;
      this.mouse.y = - (
            clientY / engine.renderer.domElement.clientHeight ) * 2 + 1;
      this.raycaster.setFromCamera( this.mouse, this.camera );

      // See if the ray from the camera into the world hits one of our meshes
      var intersects = this.raycaster.intersectObject( this.mesh );

      // Toggle rotation bool for meshes that we clicked
      if ( intersects.length > 0 ) {
         var x = intersects[ 0 ].point.x;
         var z = intersects[ 0 ].point.z;

         x = Math.min(Math.max(0, Math.floor(x/sz)), worldWidth);
         // new terrain gen uses +x, -z
         z = Math.max(Math.min(0, Math.floor(z/sz)), -worldDepth);
         // z = Math.min(Math.max(0, Math.floor(z/sz)), worldDepth);
      }

      return [x, z]
   }

   //TODO: sometimes the camera rotates itself?
   update(delta) {
      this.controls.update( delta );
      this.renderer.render( this.scene, this.camera );
   }
}

