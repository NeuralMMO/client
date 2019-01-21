export {EntityBox}


class EntityBox {

   constructor(color) {
      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color( color );

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth, window.innerHeight );
      this.renderer.shadowMapEnabled = true;
      this.renderer.shadowMap.renderSingleSided = false;
      this.renderer.shadowMap.renderReverseSided = false;

      document.body.appendChild( this.renderer.domElement );
   }
}


