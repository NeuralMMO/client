export {EntityBox}


class EntityBox {

   constructor() {
      // HTML stuff
      this.container = document.createElement("entity_container");
      this.container.style.cssText =
		   'position:fixed;left:0;top:50%;opacity:0.5;z-index:10000';

      this.panels = [];

		var rp = new RenderedPanel();
		//var fp = new FlatPanel();

		this.addPanel(rp);
		//this.addPanel(fp);

      this.dom = this.container;
		document.body.appendChild(this.dom);
   }

   addPanel (panel) {
      this.container.appendChild(panel.dom);
		this.panels.push(panel);
   }

   showPanel( id ) {
		for ( var i = 0; i < this.container.children.length; i ++ ) {
			this.container.children[i].style.display = i === id ? 'block' : 'none';
		}
		mode = id;
	}

   changeColor(color) {
      for (var i = 0; i < this.panels.length; i++) {
         this.panels[i].changeColor(color);
      }
   }

   update(delta) {
      // This will be replaced with something more complicated later
      for (var i = 0; i < this.panels.length; i++) {
         this.panels[i].update(delta);
      }
   }
}

class FlatPanel {
   constructor(text, fgColor, bgColor) {

		var min = Infinity, max = 0, round = Math.round;
		var PR = round( window.devicePixelRatio || 1 );

		var WIDTH = 80 * PR, HEIGHT = 48 * PR,
				TEXT_X = 3 * PR, TEXT_Y = 2 * PR,
				GRAPH_X = 3 * PR, GRAPH_Y = 15 * PR,
				GRAPH_WIDTH = 74 * PR, GRAPH_HEIGHT = 30 * PR;

		var canvas = document.createElement( 'canvas' );
		canvas.width = WIDTH;
		canvas.height = HEIGHT;
		canvas.style.cssText = 'width:80px;height:48px';
		this.canvas = canvas;

		var context = canvas.getContext( '2d' );
		context.font = 'bold ' + ( 9 * PR ) + 'px Helvetica,Arial,sans-serif';
		context.textBaseline = 'top';

		context.fillStyle = bg;
		context.fillRect( 0, 0, WIDTH, HEIGHT );

		context.fillStyle = fg;
		context.fillText( name, TEXT_X, TEXT_Y );
		context.fillRect( GRAPH_X, GRAPH_Y, GRAPH_WIDTH, GRAPH_HEIGHT );

		context.fillStyle = bg;
		context.globalAlpha = 0.9;
		context.fillRect( GRAPH_X, GRAPH_Y, GRAPH_WIDTH, GRAPH_HEIGHT );

		this.dom = canvas;

	}

	changeColor( color ) {
		// currently pass, maybe change FG color?
	}

   update ( delta ) {
		// currently we do nothing
		// this.context.drawImage(
		//		canvas, GRAPH_X + PR, GRAPH_Y, GRAPH_WIDTH - PR, GRAPH_HEIGHT,
		//		GRAPH_X, GRAPH_Y, GRAPH_WIDTH - PR, GRAPH_HEIGHT );
   }
}

class RenderedPanel {

   constructor() {
      // engine stuff in a panel
      this.scene = new THREE.Scene();

      this.camera = new THREE.PerspectiveCamera(
              60, window.innerWidth / window.innerHeight, 1, 20000);

      this.renderer = new THREE.WebGLRenderer( { antialias: true } );
      this.renderer.setPixelRatio( window.devicePixelRatio );
      this.renderer.setSize( window.innerWidth/2, window.innerHeight/2 );
      this.renderer.shadowMap.enabled = true;

      var pointLight = new THREE.PointLight( 0xffffff, 1.5, 0, 2);
      pointLight.position.set(64*40, 1500, 64*40);
      pointLight.caseShadow = true;
      pointLight.shadow.camera.far = 0;
      this.scene.add(pointLight);

		this.dom = this.renderer.domElement;
   }

   changeColor(color) {
      this.scene.background = new THREE.Color( color );
   }

   update(delta) {
      this.renderer.render( this.scene, this.camera );
   }
}
