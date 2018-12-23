var data = generateHeight( worldWidth, worldDepth );


function getSideMesh() {

   var matrix = new THREE.Matrix4();
   var pxGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   pxGeometry.attributes.uv.array[ 1 ] = 0.5;
   pxGeometry.attributes.uv.array[ 3 ] = 0.5;
   pxGeometry.rotateY( Math.PI / 2 );
   pxGeometry.translate( 50, 0, 0 );

   var nxGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   nxGeometry.attributes.uv.array[ 1 ] = 0.5;
   nxGeometry.attributes.uv.array[ 3 ] = 0.5;
   nxGeometry.rotateY( - Math.PI / 2 );
   nxGeometry.translate( - 50, 0, 0 );

   var pyGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   pyGeometry.attributes.uv.array[ 5 ] = 0.5;
   pyGeometry.attributes.uv.array[ 7 ] = 0.5;
   pyGeometry.rotateX( - Math.PI / 2 );
   pyGeometry.translate( 0, 50, 0 );

   var pzGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   pzGeometry.attributes.uv.array[ 1 ] = 0.5;
   pzGeometry.attributes.uv.array[ 3 ] = 0.5;
   pzGeometry.translate( 0, 0, 50 );

   var nzGeometry = new THREE.PlaneBufferGeometry( 100, 100 );
   nzGeometry.attributes.uv.array[ 1 ] = 0.5;
   nzGeometry.attributes.uv.array[ 3 ] = 0.5;
   nzGeometry.rotateY( Math.PI );
   nzGeometry.translate( 0, 0, - 50 );


   var geometries = [];

   for ( var z = 0; z < worldDepth; z ++ ) {
      for ( var x = 0; x < worldWidth; x ++ ) {

         var h = getY( x, z );

         matrix.makeTranslation(
            x * 100,
            h * 100,
            z * 100,
         );

         var px = getY( x + 1, z );
         var nx = getY( x - 1, z );
         var pz = getY( x, z + 1 );
         var nz = getY( x, z - 1 );

         geometries.push( pyGeometry.clone().applyMatrix( matrix ) );

         if ( ( px !== h && px !== h + 1 ) || x === 0 ) {
            geometries.push( pxGeometry.clone().applyMatrix( matrix ) );
         }

         if ( ( nx !== h && nx !== h + 1 ) || x === worldWidth - 1 ) {
            geometries.push( nxGeometry.clone().applyMatrix( matrix ) );
         }

         if ( ( pz !== h && pz !== h + 1 ) || z === worldDepth - 1 ) {
            geometries.push( pzGeometry.clone().applyMatrix( matrix ) );
         }

         if ( ( nz !== h && nz !== h + 1 ) || z === 0 ) {
            geometries.push( nzGeometry.clone().applyMatrix( matrix ) );
         }
      }
   }

   var geometry = THREE.BufferGeometryUtils.mergeBufferGeometries( geometries );
   geometry.computeBoundingSphere();
   geometry.computeFaceNormals()

   var texture = new THREE.TextureLoader().load(
           'three.js/examples/textures/minecraft/atlas.png' );
   texture.magFilter = THREE.NearestFilter;

   return new THREE.Mesh( geometry, new THREE.MeshLambertMaterial(
               { map: texture, side: THREE.DoubleSide } ) );
}


function generateHeight( width, height ) {

   var data = [], perlin = new ImprovedNoise(),
      size = width * height, quality = 2, z = Math.random() * 100;

   for ( var i = 0; i < size; i ++ ) {
      data[i] = 0;
   }

   return data;

}

function getY( x, z ) {

   return ( data[ x + z * worldWidth ] * 0.2 ) | 0;

}
