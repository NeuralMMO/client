if ( WEBGL.isWebGLAvailable() === false ) {

   document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   document.getElementById( 'container' ).innerHTML = "";

}

var container, stats;
var player, mesh, engine, handler;
const numPlayers = 10;


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
      controls.enablePan = false;
      controls.minPolarAngle = 0.0001;
      controls.maxPolarAngle = Math.PI / 2.0 - 0.1;
      controls.movementSpeed = 1000;
      controls.lookSpeed = 0.125;
      controls.lookVertical = true;
      this.controls = controls;

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

         player.translateState = true;
         player.moveTarg = [x, z];
         player.sendMove();
      }
   }

   translate(delta) {

      if (player.translateState) {
         var movement = player.translateDir.clone();
         movement.multiplyScalar(delta / tick);

         // Move player, then camera
         player.position.add(movement);
         this.camera.position.add(movement);

         // Turn the target into the new position of the player
         this.controls.target.copy(player.position);

         var eps = 0.0000001;
         if (player.position.distanceToSquared(player.target) <= eps) {
            // Finish animating, reset
            player.translateState = false;
            player.position.copy(player.target);
            this.controls.target.copy(player.position);
            player.translateDir.set(0.0, 0.0, 0.0);
         }
         console.log("translate");
      }
   }

}


class PlayerHandler {
   /*
    * The PlayerHandler receives packets from the server containing player
    * information (other players' movements, interactions with our player)
    * and disperses the signals appropriately.
    */
   constructor() {
      this.players = [];
      this.numPlayers = 0;
   }

   addPlayer( player ) {
      this.players.push(player);
      this.numPlayers += 1;
   }

   removePlayer( playerIndex ) {
      this.players.splice(playerIndex, 1);
      this.numPlayers -= 1;
   }

   receiveMoves( moves ) {
      /*
       * moves is a list of "{i}, [x, y]"
       */
      for (var i = 0; i < moves.length; i++) {
         this.players[i].onReceive(moves[i]);
      }
   }

}


class Player extends THREE.Mesh {

   constructor( geometry, material, index )  {
      super(geometry, material);
      this.translateState = false;
      this.translateDir = new THREE.Vector3(0.0, 0.0, 0.0);
      this.moveTarg = [0, 0];

      this.target = this.position.clone();
      this.index = index;
   }


   onReceive( pos ) {
      /*
       * Initialize a translation for the main player and send current pos to
       * engine
       */
      var x = pos[0];
      var z = pos[1];
      //var x = this.moveTarg[0];
      //var z = this.moveTarg[1];
      this.target = new THREE.Vector3(x*sz, sz+0.1, z*sz);
      this.translateDir = this.target.clone();
      this.translateDir.sub(this.position);

      // Signal for begin translation
      this.sendMove();
   }


   sendMove() {
      var packet = JSON.stringify({
         "pos" : {[this.index] : this.moveTarg}
      });
      ws.send(packet);
   }
}


class TargetPlayer extends Player {}


function init() {

   container = document.getElementById( 'container' );
   engine = new Engine();

   handler = new PlayerHandler();

   // initialize player
   var geometry = new THREE.CubeGeometry(sz, sz, sz);
   var material = new THREE.MeshBasicMaterial( {color: 0xff0000} );
   player = new TargetPlayer(geometry, material, 0);
   engine.scene.add(player);
   handler.addPlayer(player);

   // initialize map
   mesh = getMapMesh();
   engine.scene.add( mesh );

   // initialize lights
   var ambientLight = new THREE.AmbientLight( 0xcccccc );
   engine.scene.add( ambientLight );

   var directionalLight = new THREE.DirectionalLight( 0xffffff, 2 );
   directionalLight.position.set( 1, 1, 0.5 ).normalize();
   engine.scene.add( directionalLight );

   // hook up signals
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


function animatePlayers( packet ) {
   packet = JSON.parse(packet);
   // players is Array of positions
   var players = packet.players;
   for (var i = 0; i < numPLayers; i++) {
      animatePlayer( i, players[i] );
   }
}


function animate() {

   requestAnimationFrame( animate );

   while (inbox.length > 0) {
      // Receive packet, begin translating based on the received position
      var packet = inbox.shift();
      packet = JSON.parse(packet);
      var pos = packet.pos;
      console.log(pos);
      handler.receiveMoves( pos );
   }

   engine.render();
   stats.update();
}

// Main
init();
animate();
