export {makeTextSprite};


function makeTextSprite(message, fontsize, color) {
    var ctx, texture, sprite, spriteMaterial;
    var canvas = document.createElement('canvas');
    ctx = canvas.getContext('2d');
    ctx.font = fontsize + "px DragonSlapper";

    if (color == 'undefined') {
       color = '#' + (Math.random()*0xFFFFFF<<0).toString(16);
    }

    // setting canvas width/height before ctx draw, else canvas is empty
    canvas.width = ctx.measureText(message).width;
    canvas.height = fontsize * 1.5

    // after setting the canvas width/height we have to re-set font to apply
    // looks like ctx reset
    ctx.font = fontsize + "px DragonSlapper";
    ctx.fillStyle = color;
    ctx.fillText(message, 0, fontsize);

    texture = new THREE.Texture(canvas);
    texture.minFilter = THREE.LinearFilter; // NearestFilter;
    texture.needsUpdate = true;

    spriteMaterial = new THREE.SpriteMaterial({map : texture});
    sprite = new THREE.Sprite(spriteMaterial);
    return sprite;
}
