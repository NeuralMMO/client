var container, stats;
var player, engine, client, mesh;
var firstMesh = true;

class Client {
   constructor () {
      this.handler = new PlayerHandler();
      engine = new Engine();
   }

   update() {
      var delta = engine.clock.getDelta();
      while (inbox.length > 0) {
         // Receive packet, begin translating based on the received position
         var packet = inbox.shift();
         //console.log(packet);
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
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

   /*
   onMouseDown(event) {
      player.translateState = true;
      player.moveTarg = engine.raycast(event.clientX, event.clientY);
      player.sendMove();
   }
   */

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

   function onMouseDown( event ) { }; // client.onMouseDown( event ); }
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
