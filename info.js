class EntityBox {

   constructor(mode) {
      this.mode = mode;
      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color( 0x003333 );

      // initialize map
      // var map = new Terrain( false ); // flat = False
      // this.mesh = map.getMapMesh();
      // this.scene.add( this.mesh );
      this.mesh = null;

      this.camera = new THREE.PerspectiveCamera(
              60, window.innerWidth / window.innerHeight, 1, 20000 );
      this.camera.position.y = 2 * sz;
      this.camera.position.z = 10;

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth, window.innerHeight );
      this.renderer.shadowMapEnabled = true;
      this.renderer.shadowMap.renderSingleSided = false;
      this.renderer.shadowMap.renderReverseSided = false;

      this.initializeControls();

      this.mouse = new THREE.Vector2();
      this.raycaster = new THREE.Raycaster();
      this.clock = new THREE.Clock();

      document.body.appendChild( this.renderer.domElement );
   }
   constructor() {
      this.renderer = new THREE.WebGLRenderer( { antialias: true });
   }

}

function init() {
   var entityBox = document.getElementById( 'entityBox' );
   var box = new EntityBox();

   // hook up signals
   entityBox.innerHTML = "";
   entityBox.appendChild( box.renderer.domElement );

   function onMouseDown( event ) { box.onMouseDown( event ); }

   stats = new Stats();
   entityBox.addEventListener( 'click', onMouseDown, false );
}
