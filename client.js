import * as engineM from './engine.js';
import * as playerM from './player.js';
import * as terrainM from './terrain.js';

var container, stats;
var engine, client, mesh;
var firstMesh = true;


class Client {
   constructor () {
      engine = new engineM.Engine();
      this.handler = new playerM.PlayerHandler(engine);
   }

   update() {
      var delta = engine.clock.getDelta();
      while (inbox.length > 0) {
         // Receive packet, begin translating based on the received position
         //var packet = inbox.shift();
         var packet = inbox.pop();
         while(inbox.length > 0) {
            inbox.shift();
         }
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
         if (firstMesh) {
            firstMesh = false;
            var map = packet['map'];
            this.terrainMaterial = terrainM.addTerrain(map, engine);
            // mesh = terrain(map);
            // engine.scene.add(mesh);
         }
         terrainM.updateTerrain(packet['map'], this.terrainMaterial);
      }
      //this.handler.update(delta);
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
