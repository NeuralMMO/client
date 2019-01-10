class Move extends ProceduralAnimation {
   constructor(ent, targ) {
      super();
      this.pos  = ent.obj.position.clone();
      this.targ = ent.coords(targ[0], targ[1]);
      this.isTarget = false;
      this.ent = ent;
   }

   step(delta, elapsedTime) {
      var moveFrac = elapsedTime / tick;
      var x = this.pos.x + moveFrac * (this.targ.x - this.pos.x);
      var y = this.pos.y + moveFrac * (this.targ.y - this.pos.y);
      var z = this.pos.z + moveFrac * (this.targ.z - this.pos.z);
      var pos = new THREE.Vector3(x, y, z)
      this.ent.obj.position.copy(pos);
      if (this.isTarget) {
         engine.camera.position.add(movement);
         engine.controls.target.copy(this.ent.obj.position);
      }
   }
}
