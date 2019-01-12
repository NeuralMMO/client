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
      this.engine = engine;
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
            this.terrain = new terrainM.Terrain(map, engine);
         }
         this.terrain.update(packet['map']);
      }
      engine.update(delta);
   }

   onMouseDown(event) {
      //player.moveTarg = engine.raycast(event.clientX, event.clientY);
      //player.sendMove();

      //var pos = this.engine.raycast(event.clientX, event.clientY);
      //this.engine.controls.target.set(pos[0], pos[1], pos[2]);
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
