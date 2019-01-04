var container, stats;
var player, engine, client, mesh;
var firstMesh = true;

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
         if (firstMesh) {
            firstMesh = false;
            var map = packet['map'];
            addTerrain(map);
            // mesh = terrain(map);
            // engine.scene.add(mesh);
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
      var obj = loadObj( "resources/nn.obj", "resources/nn.mtl" );
      player = new TargetPlayer(obj, 0);
      engine.scene.add(obj)
      this.handler.addPlayer(player)

      const maxPlayers = 10;
      for (var i = 1; i < maxPlayers; i++) {
         var obj = loadObj( "resources/nn.obj", "resources/nn.mtl" );
         var otherPlayer = new Player(obj, i);
         obj.position.y = 100*i; // silly seal
         engine.scene.add(obj);
         this.handler.addPlayer(otherPlayer);
      }
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
