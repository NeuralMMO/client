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
