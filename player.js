class PlayerHandler {
   /*
    * The PlayerHandler receives packets from the server containing player
    * information (other players' movements, interactions with our player)
    * and disperses the signals appropriately.
    */

   constructor() {
      this.players = {};
   }

   addPlayer(id, params) {
      var player = new Player(id, params);
      this.players[id] = player;
      engine.scene.add(player);
   }

   removePlayer( playerIndex ) {
      engine.scene.remove(this.players[playerIndex])
      delete this.players[playerIndex];
   }

   updateData(ents) {
      for (var id in this.players) {
         if (!(id in ents)) {
            this.removePlayer(id);
         }
      }

      for (var id in ents) {
         if (!(id in this.players)) {
            this.addPlayer(id, ents[id])
         }
         this.players[id].updateData(ents[id]);
      }
   }

   /*
   update( delta ) {
      for (var id in this.players) {
         this.players[id].update(delta);
      }
   }
   */
}

class Player extends THREE.Object3D {
   constructor(id, params)  {
      super()
      this.translateState = false;
      this.translateDir = new THREE.Vector3(0.0, 0.0, 0.0);
      this.moveTarg = [0, 0];
      this.height = sz;    // above grass, below mountains

      this.initObj(params);
      this.initOverhead(params);
   }

   initObj(params) {
      var pos = params['pos'];
      this.obj = loadObj( "resources/nn.obj", "resources/nn.mtl" );
      this.obj.position.y = this.height;
      this.obj.position.copy(this.coords(pos[0], pos[1]));
      this.target = this.obj.position.clone();
      this.add(this.obj)
   }

   initOverhead(params) {
      this.overhead = new Overhead(params);
      this.obj.add(this.overhead)
      this.overhead.position.y = sz;
   }

   coords(x, z) {
      return new THREE.Vector3(x*sz+sz/2, this.height, z*sz+sz/2);
   }

   updateData (packet) {
      var move = packet['pos'];
      console.log("Move: ", move)
      new Move(this, move);
      //this.moveTo(move);
   }

   update(delta) {
      this.translate( delta );
   }

   //Initialize a translation for the player, send current pos to server
   moveTo( pos ) {
      var x = pos[0];
      var z = pos[1];

      this.target = this.coords(x, z);

      // Signal for begin translation
      this.translateState = true;
      this.translateDir = this.target.clone();
      this.translateDir.sub(this.obj.position);

      /*
      if (this.index == 0) {
         this.sendMove();
      }
      */
   }

   sendMove() {
      var packet = JSON.stringify({
         "pos" : this.moveTarg
      });
      ws.send(packet);
   }

   /*
   translate(delta) {
      var target = false;
      if (this.translateState) {
         var movement = this.translateDir.clone();
         movement.multiplyScalar(delta / tick);

         // Move player, then camera
         this.obj.position.add(movement);

         // Turn the target into the new position of the player
         //Translate, but also move the camera at the same time.
         if (target) {
            engine.camera.position.add(movement);
            engine.controls.target.copy(this.obj.position);
         }

         var eps = 0.0000001;
         if (this.obj.position.distanceToSquared(this.target) <= eps) {
            // Finish animating, reset
            this.translateState = false;
            this.obj.position.copy(this.target);
            if (target) {
               engine.controls.target.copy(this.obj.position);
            }
            this.translateDir.set(0.0, 0.0, 0.0);
         }
      }
   }
   */
}

//We dont exactly have animation tracks for this project
class ProceduralAnimation {
   constructor() {
      this.clock = new THREE.Clock()
      this.elapsedTime = 0.0;
      this.delta = 0.0;
      setTimeout(this.update.bind(this), 1000*tick/nAnim);
   }

   update() {
      this.delta = this.clock.getDelta();
      var time = this.elapsedTime + this.delta;
      this.elapsedTime = Math.min(time, tick);
      this.step(this.delta, this.elapsedTime);
      if (this.elapsedTime < tick) {
         setTimeout(this.update.bind(this), 1000*tick/nAnim);
      }
   }

   //Abstract
   step(delta, elapsedTime) {
      throw new Error('Must override abstract step method of ProceduralAnimation');
   }

}

class Overhead extends THREE.Object3D {
   constructor(params) {
      super()
      this.initBars()
      this.initName(params)
   }

   initBars() {
      this.health = this.initSprite(0xff0000, 0);
      this.food = this.initSprite(0xd4af37, 8)
      this.water = this.initSprite(0x0000ff, 16)
   }

   initSprite(hexColor, height) {
      var material = new THREE.SpriteMaterial({color: hexColor});
      var sprite = new THREE.Sprite(material)
      sprite.scale.set( 64, 8, 1 );
      sprite.position.y = height;
      this.add(sprite)
   }

   initName(params) {
      var sprite = makeTextSprite(params['name'], "200");
      sprite.scale.set( 30, 90, 1 );
      //sprite.position.y = -30;
      this.add(sprite);
   }
}
