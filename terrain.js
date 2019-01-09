//var worldWidth = 256, worldDepth = 256,
//worldHalfWidth = worldWidth / 2, worldHalfDepth = worldDepth / 2;
var width  = 80;
var height = 80;
var resolution = 1;

function tile(val) {
   switch (val) {
      case 0:
         return 1;
         break;
      case 1:
         return 0;
         break;
      case 2:
         return 1;
         break;
      case 3:
         return 1;
         break;
      case 4:
         return 1;
         break;
      case 5:
         return 2;
         break;
      case 6:
         return 1;
         break;
   }
}

function generateHeight(map) {
   this.sz = map.length;
   var data = new Uint8Array( resolution*this.sz*this.sz );
   var k = 0;
   var val;
   for ( var r = 0; r <  this.sz; r ++ ) {
      for ( var c = 0; c < this.sz; c ++ ) {
         val = tile(map[r][c]);
         data[k] = val;
         k++; 
      }
   }
   return data
   /*
         var ll = 0.0;
         var tt = 0.0;
         var tl = 0.0;
         var cc = 0.0;

         //r = Math.floor(r / resolution);
         //c = Math.floor(c / resolution);

         cc = tile(map[r][c]);
         if (c != 0) {
            ll = tile(map[r][c-1]);
         }
         if (r != 0) {
            tt = tile(map[r-1][c]);
         }
         if (r != 0 && c != 0) {
            tl = tile(map[r-1][c-1]);
         }
         var val = 0.25*(ll + tt + tl + cc);
         //var val = Math.max(ll, tt, tl, cc);
         data[k] = mag * val;
   */
}

function generateFlat(map) {
   this.sz = map.length;
   var data = new Uint8Array( this.sz*this.sz );
   var k = 0;
   for ( var r = 0; r < this.sz; r ++ ) {
      for ( var c = 0; c < this.sz; c ++ ) {
         data[k] = map[r][c];
         k++;
      }
   }
   return data;
}

function addTerrain(map) {
    /*
     * Adds terrain which shades water, grass, dirt, and mountains
     * based on a heightmap given by the server.
     */
   var tileSz = 64;
   var nTiles = map.length;

   // LIGHT
	//var light = new THREE.PointLight(0xffffff, 1, 10000, 1);
	//light.position.set(0,0,0);
	//light.position.set(640,640,100);
	//engine.scene.add(light);

    var loader = new THREE.TextureLoader();

	// SKYBOX
   /*
	var skyBoxGeometry = new THREE.CubeGeometry( 20000, 20000, 10000 );
	var skyBoxMaterial = new THREE.MeshBasicMaterial( {
        color: 0x9999ff, side: THREE.BackSide } );
	var skyBox = new THREE.Mesh( skyBoxGeometry, skyBoxMaterial );
	engine.scene.add(skyBox);
   */

   var length = 64*nTiles;
   var axisSz = tileSz;
	var xGeometry = new THREE.CubeGeometry( length, 2*axisSz, axisSz);
   var xMaterial = new THREE.MeshBasicMaterial( {
        color: 0xff0000} );
	var xMesh = new THREE.Mesh( xGeometry, xMaterial );
   xMesh.position.x = length / 2;
   xMesh.position.y = tileSz;
   xMesh.position.z = -tileSz/2;
	engine.scene.add(xMesh);

	var zGeometry = new THREE.CubeGeometry( axisSz, 2*axisSz, length);
   var zMaterial = new THREE.MeshBasicMaterial( {
        color: 0x00ff00} );
	var zMesh = new THREE.Mesh( zGeometry, zMaterial );
   zMesh.position.z = length / 2;
   zMesh.position.x = -tileSz/2;
   zMesh.position.y = tileSz;
	engine.scene.add(zMesh);

	var yGeometry = new THREE.CubeGeometry( axisSz, length, axisSz);
   var yMaterial = new THREE.MeshBasicMaterial( {
        color: 0x0000ff } );
	var yMesh = new THREE.Mesh( yGeometry, yMaterial );
	engine.scene.add(yMesh);
   yMesh.position.y = length / 2;
   yMesh.position.x = -tileSz/2;
   yMesh.position.z = -tileSz/2;

	var aGeometry = new THREE.CubeGeometry( 5+2*axisSz, 5+2*axisSz, 5+2*axisSz);
   var aMaterial = new THREE.MeshBasicMaterial( {
        color: 0x000000 } );
	var aMesh = new THREE.Mesh( aGeometry, aMaterial );
	engine.scene.add(aMesh);
   aMesh.position.y = tileSz;


    // texture used to generate "bumpiness"
    // We're going to use a DataTexture instead
    var heights = generateHeight(map);
    var flats = generateFlat(map);
    var bumpMap = new Uint8Array( 3 * heights.length );
    var tileMap = new Uint8Array( 3 * heights.length );

    for (var i = 0; i < heights.length; i++) {
        // only R channel is updated for now as that's what the vertex
        // shader will care about. Change this to RGB later
        bumpMap[i*3]   = heights[i];
        bumpMap[i*3+1] = heights[i];
        bumpMap[i*3+2] = heights[i];

        tileMap[i*3]   = flats[i];
        tileMap[i*3+1] = flats[i];
        tileMap[i*3+2] = flats[i];
    }

	var bumpTexture = new THREE.DataTexture(
            bumpMap, width, height, THREE.RGBFormat);
	bumpTexture.wrapS = bumpTexture.wrapT = THREE.ClampToEdgeWrapping;
   bumpTexture.needsUpdate = true;

	var tileTexture = new THREE.DataTexture(
            tileMap, width, height, THREE.RGBFormat);
	tileTexture.wrapS = tileTexture.wrapT = THREE.ClampToEdgeWrapping;
   tileTexture.needsUpdate = true;


	// magnitude of normal displacement
	var bumpScale   = 64.0;

	var oceanTexture = loader.load('resources/images/dirt-512.jpg' );
	oceanTexture.wrapS = oceanTexture.wrapT = THREE.RepeatWrapping;

	var sandyTexture = loader.load('resources/images/sand-512.jpg' );
	sandyTexture.wrapS = sandyTexture.wrapT = THREE.RepeatWrapping;

	var rockyTexture = loader.load('resources/images/rock-512.jpg' );
	rockyTexture.wrapS = rockyTexture.wrapT = THREE.RepeatWrapping;

	var snowyTexture = loader.load('resources/images/snow-512.jpg' );
	snowyTexture.wrapS = snowyTexture.wrapT = THREE.RepeatWrapping;

	var scrubTexture = loader.load('resources/tiles/scrub.png' );
	scrubTexture.wrapS = scrubTexture.wrapT = THREE.RepeatWrapping;

	var forestTexture = loader.load('resources/tiles/forest.png' );
	forestTexture.wrapS = forestTexture.wrapT = THREE.RepeatWrapping;

	var lavaTexture = loader.load('resources/tiles/lava.png' );
	lavaTexture.wrapS = lavaTexture.wrapT = THREE.RepeatWrapping;

	var stoneTexture = loader.load('resources/tiles/stone.png' );
	stoneTexture.wrapS = stoneTexture.wrapT = THREE.RepeatWrapping;

	var grassTexture = loader.load('resources/tiles/grass.png' );
	grassTexture.wrapS = grassTexture.wrapT = THREE.RepeatWrapping;


	// use "this." to create global object
	this.customUniforms = {
		bumpTexture:	{ type: "t", value: bumpTexture },
		bumpScale:	   { type: "f", value: bumpScale },
		oceanTexture:	{ type: "t", value: oceanTexture },
		sandyTexture:	{ type: "t", value: sandyTexture },
		grassTexture:	{ type: "t", value: grassTexture },
		rockyTexture:	{ type: "t", value: rockyTexture },
		snowyTexture:	{ type: "t", value: snowyTexture },
		forestTexture: { type: "t", value: forestTexture},
		lavaTexture:   { type: "t", value: lavaTexture},
	   tileTexture:	{ type: "t", value: tileTexture },
		scrubTexture:  { type: "t", value: scrubTexture},
		stoneTexture:  { type: "t", value: stoneTexture},
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

   var mapSz = nTiles*tileSz;
   // var planeGeo = new THREE.PlaneGeometry(
   //      mapSz, mapSz, tileSz*resolution, tileSz*resolution);

   //May be off by one, but careful not to break border detection
   //in the shader, as that is far harder to debug
   var planeGeo = new THREE.PlaneGeometry(
         mapSz, mapSz, width*resolution, height*resolution);


   // Only use first left quadrant
   //planeGeo.translate(mapSz/2, mapSz/2, 0);
   //planeGeo.translate(mapSz/2, 0, 0);
   //planeGeo.translate(mapSz/2, 0, 0);

   var waterTiles = nTiles-20
   var waterSz = waterTiles*tileSz; 
	//var waterGeo = new THREE.PlaneGeometry( 1000, 1000, 1, 1 );
   var waterGeo = new THREE.PlaneGeometry(
         waterSz, waterSz, waterTiles*resolution, waterTiles*resolution);
   //waterGeo.translate(mapSz/2, mapSz/2, 0);


	var waterTex = loader.load( 'resources/tiles/water.png' );
	waterTex.wrapS = waterTex.wrapT = THREE.RepeatWrapping;
	waterTex.repeat.set(50,50);
	var waterMat = new THREE.MeshBasicMaterial( {
        map: waterTex, transparent:true, opacity:0.75} );
	var water = new THREE.Mesh(	waterGeo, waterMat );
	water.rotation.x = -Math.PI / 2;
	water.position.y = 3*tileSz/4;
	water.position.x = mapSz / 2;
	water.position.z = mapSz / 2;
	engine.scene.add( water);

	var plane = new THREE.Mesh(	planeGeo, customMaterial );
	plane.rotation.x = -Math.PI / 2;
	plane.rotation.z = -Math.PI / 2;
	//plane.position.x = mapSz / 2;
	//plane.position.z = mapSz;
	engine.scene.add( plane );
   engine.mesh = plane;


}
