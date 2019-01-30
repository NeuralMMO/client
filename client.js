import * as engineM from './engine.js';
import * as playerM from './player.js';
import * as terrainM from './terrain.js';
import * as countsM from './counts.js';
import * as valuesM from './values.js';
import * as textsprite from './textsprite.js';

var client, counts, values, stats;


class Client {
   constructor (client_container) {
      this.engine = new engineM.Engine(modes.ADMIN, client_container);
      this.handler = new playerM.PlayerHandler(this.engine);
      this.init = true;
      this.packet = null;
      this.frame = 0;

   }

   // hook up signals
   setupSignals() {
      client_container.innerHTML = ""; // get rid of the text after loading
      client_container.appendChild( this.engine.renderer.domElement );

      var scope = this; // javascript quirk... don't touch this
      function onMouseDown( event ) { scope.onMouseDown( event ); }
      function onWindowResize() { scope.onWindowResize(); }

      client_container.addEventListener( 'click', onMouseDown, false );
      window.addEventListener( 'resize', onWindowResize, false );
   }

   updatePacket() {
      if (inbox.length > 0) {
         this.packet = inbox.pop();
      } else {
         this.packet = null;
      }
   }

   update() {
      var delta = this.engine.clock.getDelta();
      this.updatePacket();

      if (this.packet) {
         // Receive packet, begin translating based on the received position
         var packet = JSON.parse(this.packet);
         this.handler.updateData(packet['ent']);
         this.frame += 1;
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.terrain = new terrainM.Terrain(map, this.engine);

            /*
            var pkt1 = {
               'pos': [18, 11],
               'entID': 394,
               'color':'#ff8000',
               'name': 'Neural_394',
               'food':25, 
               'water':12,
               'health':16,
               'maxFood':32,
               'maxWater':32,
               'maxHealth':32,
               'damage':2,
            }

            var pkt2 = {
               'pos': [19, 13],
               'entID': 383,
               'color':'#8000ff',
               'name': 'Neural_383',
               'food':8, 
               'water':28,
               'health':23,
               'maxFood':32,
               'maxWater':32,
               'maxHealth':32,
               'damage':0,
               'attack':'Range',
               'target':'394',
            }
        
            var player1 = new playerM.Player(this.handler, 0, pkt1)
            this.engine.scene.add(player1);
            player1.updateData(this.engine, pkt1, {})
            this.player1 = player1

            var player2 = new playerM.Player(this.handler, 0, pkt2)
            this.engine.scene.add(player2);
            player2.updateData(this.engine, pkt2, {394: player1})
            this.player2 = player2

            var attkGeom = new THREE.IcosahedronGeometry(10);
            var attkMatl = new THREE.MeshBasicMaterial({color: '#0000ff'});
            var attkMesh = new THREE.Mesh(attkGeom, attkMatl);

            var p2 = player2.obj.position
            var p1 = player1.obj.position
            var moveFrac = 0.75
            var y = 96;

            var x = p1.x + moveFrac * (p2.x - p1.x) + 16;
            var z = p1.z + moveFrac * (p2.z - p1.z) + 16;
            var pos = new THREE.Vector3(x, y, z)
            attkMesh.position.x = x
            attkMesh.position.y = y
            attkMesh.position.z = z
            this.attkMesh = attkMesh
            this.engine.scene.add(attkMesh);

            var dmg = textsprite.makeTextSprite(2, "200", '#ff0000');
            dmg.scale.set( 10, 30, 1 );
            dmg.position.x = p2.x
            dmg.position.y = p2.y + 128
            dmg.position.z = p2.z
            this.dmg = dmg
            this.engine.scene.add(dmg);
            */

         }
 
         this.terrain.update(packet['map']);
      }
      this.terrain.updateFast();
      this.handler.updateFast();
      this.engine.update(delta);
   }

   onWindowResize () {
      this.engine.onWindowResize();
   }

   onMouseDown(event) {
      //player.moveTarg = this.engine.raycast(event.clientX, event.clientY);
      //player.sendMove();

      //var pos = this.engine.raycast(event.clientX, event.clientY);
      //this.engine.controls.target.set(pos[0], pos[1], pos[2]);
   }
}

class Counts {
   constructor (client, viewer_container) {
      this.engine = new engineM.Engine(modes.ADMIN, viewer_container);
      viewer_container.innerHTML = ""; // get rid of the text after loading
      viewer_container.appendChild( this.engine.renderer.domElement );
      this.handler = new playerM.PlayerHandler(this.engine);
      this.client = client;
      this.init = true;
   }

   update() {
      var delta = this.engine.clock.getDelta();
      if (this.client.packet) {
         // Receive packet, begin translating based on the received position
         var packet = this.client.packet;
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.counts = new countsM.Counts(
                  packet['map'], packet['counts'], this.engine);
 
         }
         this.counts.update(packet['map'], packet['counts']);
      }
      this.counts.updateFast();
      this.engine.update(delta);
   }
}

class Values{
   constructor (client, viewer_container) {
      this.engine = new engineM.Engine(modes.ADMIN, viewer_container);
      viewer_container.innerHTML = ""; // get rid of the text after loading
      viewer_container.appendChild( this.engine.renderer.domElement );
      this.handler = new playerM.PlayerHandler(this.engine);
      this.client = client;
      this.init = true;
   }

   update() {
      var delta = this.engine.clock.getDelta();
      if (this.client.packet) {
         // Receive packet, begin translating based on the received position
         var packet = this.client.packet;
         packet = JSON.parse(packet);
         this.handler.updateData(packet['ent']);
         if (this.init) {
            this.init = false;
            var map = packet['map'];
            this.values = new valuesM.Values(
                  packet['map'], packet['values'], this.engine);
         }
         this.values.update(packet['map'], packet['values']);
      }
      this.values.updateFast();
      this.engine.update(delta);
   }
}


function webglError() {
   if ( WEBGL.isWebGLAvailable() === false ) {
      document.body.appendChild( WEBGL.getWebGLErrorMessage() );
   }
}

function init() {
   webglError();
   var client_container = document.getElementById("client_container");
   var viewer_container = document.getElementById("viewer_container");

   client = new Client(client_container);
   //counts = new Counts(client, counts_container);
   //values = new Values(client, values_container);
   stats  = new Stats();
   client.setupSignals();
   client_container.appendChild(stats.dom);

   var blocker = document.getElementById("blocker");
   var instructions = document.getElementById("instructions");
   instructions.addEventListener("click", function() {
	   client.engine.controls.enabled = true;
	   client.engine.controls.update();
	   //counts.engine.controls.enabled = true;
	   //counts.engine.controls.update();
	   //values.engine.controls.enabled = true;
	   //values.engine.controls.update();
	   instructions.style.display = "none";
      blocker.style.display = "none";
   }, false);

   animate();
}

function animate() {
   requestAnimationFrame( animate );
   client.update();
   //counts.update();
   //values.update();
   stats.update();
}


// Main
init();
