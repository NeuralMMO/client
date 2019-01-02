var container, stats;
var player, engine, client, mesh;

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
         if (mesh == 0) {
            map = packet['map'];
            mesh = terrain(map);
            engine.scene.add(mesh);
         }
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

class Overhead {
   constructor( pos ) {
      this.position = pos.clone();
      // Health: red
      this.health = this.initSprite(0xff0000, pos.y + 1.5 * sz);
      // Food: gold
      this.food = this.initSprite(0xd4af37, pos.y + 1.75 * sz);
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

function init() {
   if ( WEBGL.isWebGLAvailable() === false ) {
      document.body.appendChild( WEBGL.getWebGLErrorMessage() );
      document.getElementById( 'container' ).innerHTML = "";
   }

   client = new Client();
   container = document.getElementById( 'container' );

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

function animate() {
   requestAnimationFrame( animate );
   client.update();
   stats.update();
}


// Main
init();
animate();
