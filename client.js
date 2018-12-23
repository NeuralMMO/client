if ( WEBGL.isWebGLAvailable() === false ) {

   document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   document.getElementById( 'container' ).innerHTML = "";

}

var container, stats;
var player, mesh;
var engine;


class Engine {

   constructor() {
      this.camera = new THREE.PerspectiveCamera(
              60, window.innerWidth / window.innerHeight, 1, 20000 );
      this.camera.position.y = getY( worldHalfWidth, worldHalfDepth ) * 100 + 100;
      this.camera.position.z = 5;

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth, window.innerHeight );

      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color( 0x006666 );

      this.mouse = new THREE.Vector2();
      this.raycaster = new THREE.Raycaster();
      this.clock = new THREE.Clock();

      var controls = new THREE.OrbitControls(this.camera, container);
      controls.mouseButtons = {
         LEFT: THREE.MOUSE.MIDDLE, // rotate
         RIGHT: THREE.MOUSE.LEFT // pan
      }
      controls.target.set( 0, 0, 0 );
      //controls.target = player
      controls.enablePan = false;
      controls.minPolarAngle = 0.0001;
      controls.maxPolarAngle = Math.PI / 2.0 - 0.1;
      //controls.target = target
      controls.movementSpeed = 1000;
      controls.lookSpeed = 0.125;
      controls.lookVertical = true;
      this.controls = controls;

      this.translateState = -1;
      this.translateDir = new THREE.Vector3(0.0, 0.0, 0.0);
      this.moveTarg = [0, 0];

      document.body.appendChild( this.renderer.domElement );
   }

   onWindowResize() {

      this.camera.aspect = window.innerWidth / window.innerHeight;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize( window.innerWidth, window.innerHeight );
   }

   render() {
      /*
       * TODO: fix what happens when another square is clicked before the current
       * animation is finished, or when the camera is rotated/zoomed before it's
       * finished
       * TODO: sometimes the camera rotates itself?
       */
      var delta = this.clock.getDelta();
      this.translate(delta)
      this.controls.update( delta );
      this.renderer.render( this.scene, this.camera );
   }

   onMouseDown( event ) {
      /*
       * Sets the translation direction based on the clicked square and toggles
       * state to translate.
       */

      this.mouse.x = ( event.clientX / this.renderer.domElement.clientWidth ) * 2 - 1;
      this.mouse.y = - ( event.clientY / this.renderer.domElement.clientHeight ) * 2 + 1;
      this.raycaster.setFromCamera( this.mouse, this.camera );

      // See if the ray from the camera into the world hits one of our meshes
      var intersects = this.raycaster.intersectObject( mesh );

      // Toggle rotation bool for meshes that we clicked
      if ( intersects.length > 0 ) {
         var x = intersects[ 0 ].point.x;
         var z = intersects[ 0 ].point.z;

         x = Math.floor(x/sz);
         z = Math.floor(z/sz);

         this.setMove(x, z);
      }
   }

   translate(delta) {

      if (this.translateState != -1) {
         var movement = this.translateDir.clone();
         movement.multiplyScalar(delta / tick);

         // Move player, then camera
         player.position.add(movement);
         this.camera.position.add(movement);
         // this *should* be equivalent
         // controls.object.position.add(movement);

         // Turn the target into the new position of the player
         this.controls.target.copy(player.position);

         this.translateState += delta;

         var eps = 0.0000001;
         if (player.position.distanceToSquared( this.controls.target0 ) <= eps
                 || this.translateState >= tick) {
            // Finish animating, reset
            this.translateState = -1;
            player.position.copy(this.controls.target0);
            this.controls.target.copy(player.position);
            this.translateDir.set(0.0, 0.0, 0.0);
         }
      }
   }

   setMove(x, z) {

      this.moveTarg = [x, z];
      var packet = JSON.stringify({'pos': this.moveTarg});
      // console.log("Set Move:", packet);
      ws.send(packet);
   }
}


function init() {
   container = document.getElementById( 'container' );
   engine = new Engine();

   var geometry = new THREE.CubeGeometry(sz, 1, sz);
   var material = new THREE.MeshBasicMaterial( {color: 0xff0000} );
   player = new THREE.Mesh(geometry, material);
   engine.scene.add(player);
   //player.add(camera)

   // sides
   mesh = getSideMesh();
   engine.scene.add( mesh );

   var ambientLight = new THREE.AmbientLight( 0xcccccc );
   engine.scene.add( ambientLight );

   var directionalLight = new THREE.DirectionalLight( 0xffffff, 2 );
   directionalLight.position.set( 1, 1, 0.5 ).normalize();
   engine.scene.add( directionalLight );

   container.innerHTML = "";
   container.appendChild( engine.renderer.domElement );

   function onMouseDown( event ) {
      engine.onMouseDown( event );
   }
   function onWindowResize() {
      engine.onWindowResize()
   }

   stats = new Stats();
   container.appendChild( stats.dom );
   container.addEventListener( 'click', onMouseDown, false );
   window.addEventListener( 'resize', onWindowResize, false );
}


function animate() {

   requestAnimationFrame( animate );

   while (inbox.length > 0) {
      var packet = inbox.shift();
      packet = JSON.parse(packet);
      var pos = packet.pos;
      // console.log('Inbox: ', pos);

      var x = pos[0];
      var z = pos[1];
      var clickedSquare = new THREE.Vector3(x*sz, sz/2+0.1, z*sz);
      engine.translateDir = clickedSquare.clone();
      engine.translateDir.sub(engine.controls.target0);

      engine.translateState = 0.0;
      //engine.controls.saveState();
      engine.controls.target0.copy(clickedSquare);

      var x = engine.moveTarg[0];
      var z = engine.moveTarg[1];
      engine.setMove(x, z);
   }

   engine.render();
   stats.update();
}

// Main
init();
animate();
