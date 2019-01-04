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
      var obj = loadObj( "nn.obj", "nn.mtl" );
      player = new TargetPlayer(obj, 0);
      engine.scene.add(obj)
      this.handler.addPlayer(player)

      const maxPlayers = 10;
      for (var i = 1; i < maxPlayers; i++) {
         var obj = loadObj( "nn.obj", "nn.mtl" );
         var otherPlayer = new Player(obj, i);
         obj.position.y = 100*i; // silly seal
         engine.scene.add(obj);
         this.handler.addPlayer(otherPlayer);
      }
   }
}


function addTerrain(map) {

    // LIGHT
	var light = new THREE.PointLight(0xffffff);
	light.position.set(100,250,100);
	engine.scene.add(light);

    var loader = new THREE.TextureLoader();

	// SKYBOX
	var skyBoxGeometry = new THREE.CubeGeometry( 20000, 20000, 10000 );
	var skyBoxMaterial = new THREE.MeshBasicMaterial( {
        color: 0x9999ff, side: THREE.BackSide } );
	var skyBox = new THREE.Mesh( skyBoxGeometry, skyBoxMaterial );
	engine.scene.add(skyBox);

	////////////
	// CUSTOM //
	////////////

	// texture used to generate "bumpiness"
    // We're going to use a DataTexture instead
    var heights = generateHeight(map);
    var bumpMap = new Uint8Array( 3 * heights.length );

    const heightScale = 45;
    for (var i = 0; i < heights.length; i++) {
        // only R channel is updated for now as that's what the vertex
        // shader will care about. Change this to RGB later
        bumpMap[i*3] = heightScale * heights[i];
    }

	var bumpTexture = new THREE.DataTexture(
            bumpMap, width, height, THREE.RGBFormat);
    console.log(bumpMap, width, height);
	bumpTexture.wrapS = bumpTexture.wrapT = THREE.ClampToEdgeWrapping;
    bumpTexture.needsUpdate = true;

	// magnitude of normal displacement
	var bumpScale   = 200.0;

	var oceanTexture = loader.load('resources/images/dirt-512.jpg' );
	oceanTexture.wrapS = oceanTexture.wrapT = THREE.RepeatWrapping;

	var sandyTexture = loader.load('resources/images/sand-512.jpg' );
	sandyTexture.wrapS = sandyTexture.wrapT = THREE.RepeatWrapping;

	var grassTexture = loader.load('resources/images/grass-512.jpg' );
	grassTexture.wrapS = grassTexture.wrapT = THREE.RepeatWrapping;

	var rockyTexture = loader.load('resources/images/rock-512.jpg' );
	rockyTexture.wrapS = rockyTexture.wrapT = THREE.RepeatWrapping;

	var snowyTexture = loader.load('resources/images/snow-512.jpg' );
	snowyTexture.wrapS = snowyTexture.wrapT = THREE.RepeatWrapping;


	// use "this." to create global object
	this.customUniforms = {
		bumpTexture:	{ type: "t", value: bumpTexture },
		bumpScale:	    { type: "f", value: bumpScale },
		oceanTexture:	{ type: "t", value: oceanTexture },
		sandyTexture:	{ type: "t", value: sandyTexture },
		grassTexture:	{ type: "t", value: grassTexture },
		rockyTexture:	{ type: "t", value: rockyTexture },
		snowyTexture:	{ type: "t", value: snowyTexture },
	};

	// create custom material from the shader code above
	//   that is within specially labelled script tags
	var customMaterial = new THREE.ShaderMaterial(
	{
	    uniforms: customUniforms,
		vertexShader:   document.getElementById('vertexShader').textContent,
		fragmentShader: document.getElementById('fragmentShader').textContent,
		// side: THREE.DoubleSide
	}   );

	var planeGeo = new THREE.PlaneGeometry( 1000, 1000, 100, 100 );
	var plane = new THREE.Mesh(	planeGeo, customMaterial );
	plane.rotation.x = -Math.PI / 2;
	plane.position.y = -100;
	engine.scene.add( plane );

	var waterGeo = new THREE.PlaneGeometry( 1000, 1000, 1, 1 );
	var waterTex = loader.load( 'resources/images/water512.jpg' );
	waterTex.wrapS = waterTex.wrapT = THREE.RepeatWrapping;
	waterTex.repeat.set(5,5);
	var waterMat = new THREE.MeshBasicMaterial( {
        map: waterTex, transparent:true, opacity:0.40} );
	var water = new THREE.Mesh(	planeGeo, waterMat );
	water.rotation.x = -Math.PI / 2;
	water.position.y = -50;
	engine.scene.add( water);
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
