if ( WEBGL.isWebGLAvailable() === false ) {

   document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   document.getElementById( 'container' ).innerHTML = "";

}

var container, stats;
var player, engine, client;

class Engine {

   constructor() {
      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color( 0x006666 );

      // initialize map
      var map = new Terrain( false ); // flat = False
      this.mesh = map.getMapMesh();
      this.scene.add( this.mesh );

      this.camera = new THREE.PerspectiveCamera(
              60, window.innerWidth / window.innerHeight, 1, 20000 );
      this.camera.position.y = map.getY(
            worldHalfWidth, worldHalfDepth ) * sz + 2 * sz;
      this.camera.position.z = 10;

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth, window.innerHeight );

      this.initializeControls();

      this.mouse = new THREE.Vector2();
      this.raycaster = new THREE.Raycaster();
      this.clock = new THREE.Clock();

      // initialize lights
      var ambientLight = new THREE.AmbientLight( 0xcccccc );
      this.scene.add( ambientLight );

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
         z = Math.min(Math.max(0, Math.floor(z/sz)), worldDepth);
      }

      return [x, z]
  }


   //TODO: sometimes the camera rotates itself?
   update(delta) {
      this.controls.update( delta );
      this.renderer.render( this.scene, this.camera );
   }
}

class Client {
   constructor () {
      this.handler = new PlayerHandler();
      engine = new Engine();
      this.initializePlayers();
   }

   update() {
      var delta = engine.clock.getDelta();
      while (inbox.length > 0) {
         // Receive packet, begin translating based on the received position
         var packet = inbox.shift();
         //console.log(packet);
         packet = JSON.parse(packet);
         this.handler.updateData(packet);
      }
      this.handler.update(delta);
      engine.update(delta);
   }

   onMouseDown(event) {
      player.translateState = true;
      player.moveTarg = engine.raycast(event.clientX, event.clientY);
      player.sendMove();
   }
 
   initializePlayers() {
      var obj = loadObj( "nn.obj", "nn.mtl" );
      player = new TargetPlayer(obj, 0);
      engine.scene.add(obj)
      this.handler.addPlayer(player)

      const maxPlayers = 10;
      for (var i = 1; i < maxPlayers; i++) {
         var obj = loadObj( "nn.obj", "nn.mtl" );
         var otherPlayer = new Player(obj, i);
         obj.position.y = 100*i; // silly seal
         engine.scene.add(obj);
         this.handler.addPlayer(otherPlayer);
      }
   }

   //Sets the translation direction based on the clicked 
   //square and toggles state to translate.
}

class PlayerHandler {
   /*
    * The PlayerHandler receives packets from the server containing player
    * information (other players' movements, interactions with our player)
    * and disperses the signals appropriately.
    */

   constructor() {
      this.players = [];
   }

   addPlayer( player ) {
      this.players.push(player);
   }

   removePlayer( playerIndex ) {
      this.players.splice(playerIndex, 1);
   }

   updateData(packets) {
      for (var id in packets) {
         this.players[id].updateData(packets[id])
      }
   }

   update( delta ) {
      for (var id in this.players) {
         this.players[id].update(delta)
      }
   }
}


class Overhead {
   constructor( pos ) {
      this.position = pos.clone();
      // Health: red
      this.health = this.initSprite(0xff0000, pos.y + 1.5 * sz);
      // Food: green
      this.food = this.initSprite(0x00ff00, pos.y + 1.75 * sz);
      // Water: blue
      this.water = this.initSprite(0x0000ff, pos.y + 2 * sz);

      engine.scene.add(this.health);
      engine.scene.add(this.food);
      engine.scene.add(this.water);
   }

   initSprite( colorRGB, height) {
      var sprite = new THREE.Sprite( new THREE.SpriteMaterial( {
         color: colorRGB
      } ) );
      sprite.scale.set( 128, 16, 1 );
      sprite.position.copy(this.position.clone());
      sprite.position.y = height;
      return sprite;
   }

   move( movement ) {
      this.position.add(movement);
      this.health.position.add(movement);
      this.food.position.add(movement);
      this.water.position.add(movement);
   }
}

class Player {
   constructor( obj, index )  {
      this.translateState = false;
      this.translateDir = new THREE.Vector3(0.0, 0.0, 0.0);
      this.moveTarg = [0, 0];
      this.index = index;

      this.initObj(obj);
      this.overhead = new Overhead( this.obj.position );
      this.initOverhead();
   }

   initObj(obj) {
      this.obj = obj;
      this.target = obj.position.clone();
   }

   initOverhead() {
      /*
      var spriteMap = new THREE.TextureLoader().load( "resources/hpbar.png" );
      var spriteMaterial = new THREE.SpriteMaterial({
         map: spriteMap,
         color: 0xffffff
      } );
      this.overhead = new THREE.Sprite( spriteMaterial );
      this.overhead.scale.set(256, 64, 1);
      this.overhead.position.copy(this.obj.position.clone());
      this.overhead.position.y += 1.5 * sz;
      engine.scene.add( this.overhead );
      */
   }

   setPos(x, y, z) {
      var pos = new THREE.Vector3(x*sz, sz+0.1, z*sz);
      this.obj.position.copy(pos);
   }

   updateData (packet) {
      var move = packet['pos'];
      console.log("Move: ", move)
      this.moveTo(move);
      /*
      for (var i = 1; i < this.numPlayers; i++) {
         this.players[i].moveTo([Math.random() * worldWidth,
               Math.random() * worldDepth]);
      }
      */
   }
 
   update(delta) {
      this.translate( delta );
   }

   //Initialize a translation for the player, send current pos to server
   moveTo( pos ) {
      /*
       */
      var x = pos[0];
      var z = pos[1];

      this.target = new THREE.Vector3(x*sz, sz+0.1, z*sz);

      // Signal for begin translation
      this.translateState = true;
      this.translateDir = this.target.clone();
      this.translateDir.sub(this.obj.position);

      if (this.index == 0) {
         this.sendMove();
      }
   }

   sendMove() {
      var packet = JSON.stringify({
         "pos" : this.moveTarg
      });
      ws.send(packet);
   }

   translate(delta) {
      if (this.translateState) {
         var movement = this.translateDir.clone();
         movement.multiplyScalar(delta / tick);
         this.obj.position.add(movement);
         this.overhead.move(movement);

         var eps = 0.0000001;
         if (this.obj.position.distanceToSquared(this.target) <= eps) {
            // Finish animating, reset
            this.translateState = false;
            this.obj.position.copy(this.target);
            this.translateDir.set(0.0, 0.0, 0.0);
         }
      }
   }
}

class TargetPlayer extends Player {

   //Deprecated
   //Resets the camera on me.
   focus() {
      engine.camera.position.add(this.translateDir);
      engine.controls.target.copy(this.obj.position.clone());
   }

   //Translate, but also move the camera at the same time.
   translate(delta) {
      if (this.translateState) {
         var movement = this.translateDir.clone();
         movement.multiplyScalar(delta / tick);

         // Move player, then camera
         this.obj.position.add(movement);
         this.overhead.move(movement);
         engine.camera.position.add(movement);

         // Turn the target into the new position of the player
         engine.controls.target.copy(this.obj.position);

         var eps = 0.0000001;
         if (this.obj.position.distanceToSquared(this.target) <= eps) {
            // Finish animating, reset
            this.translateState = false;
            this.obj.position.copy(this.target);
            engine.controls.target.copy(this.obj.position);
            this.translateDir.set(0.0, 0.0, 0.0);
         }
      }
   }
}

function init() {
   container = document.getElementById( 'container' );
   client = new Client();

   // hook up signals
   container.innerHTML = "";
   container.appendChild( engine.renderer.domElement );

   function onMouseDown( event ) { client.onMouseDown( event ); }
   function onWindowResize() { engine.onWindowResize(); }

   stats = new Stats();
   container.appendChild( stats.dom );
   container.addEventListener( 'click', onMouseDown, false );
   window.addEventListener( 'resize', onWindowResize, false );
}

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
           obj.scale.x = 100;
           obj.scale.y = 100;
           obj.scale.z = 100;
           container.add(obj)
        }
        objLoader.load( objf, onOBJLoad);
    }

    var mtlLoader = new THREE.MTLLoader();
    //mtlLoader.setPath( path );
    mtlLoader.load( mtlf, onMTLLoad);
    return container
}


function animate() {
   requestAnimationFrame( animate );
   client.update();
   stats.update();
}


// Main
init();
animate();
