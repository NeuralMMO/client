/*
Copyright (c) 2017 Marcel Greter (http://github.com/mgreter)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

var LZMA = LZMA || {};

(function (LZMA) {
  // very simple in memory input stream class
  LZMA.iStream = function (buffer) {
    // create byte array view of buffer
    this.array = new Uint8Array(buffer);
    // convenience status member
    this.size = buffer.byteLength;
    // position pointer
    this.offset = 0;
  };

  // simply return the next byte from memory
  LZMA.iStream.prototype.readByte = function () {
    // advance pointer and return byte
    return this.array[this.offset++];
  };

  // output stream constructor
  LZMA.oStream = function (buffers) {
    // aggregated size
    this.size = 0;
    // initialize empty
    this.buffers = [];
    buffers = buffers || [];
    // make sure size matches data
    for (var i = 0, L = buffers.length; i < L; i++) {
      // unwrap nested output streams
      if (buffers[i] instanceof LZMA.oStream) {
        var oBuffers = buffers[i].buffers;
        for (var n = 0; n < oBuffers.length; n++) {
          this.buffers.push(buffers[i].buffers[n]);
          this.size += buffers[i].buffers[n].length;
        }
      } else {
        // simply append the one buffer
        this.buffers.push(buffers[i]);
        this.size += buffers[i].length;
      }
    }
  };

  // we expect a Uint8Array buffer and the size to read from
  // creates a copy of the buffer as needed so you can re-use it
  // tests with js-lzma have shown that this is at most for 16MB
  LZMA.oStream.prototype.writeBytes = function writeBytes(buffer, size) {
    // can we just take the full buffer?
    // or just some part of the buffer?
    if (size <= buffer.byteLength) {
      // we need to make a copy, as the original
      // buffer will be re-used. No way around!
      this.buffers.push(buffer.slice(0, size));
    }
    // assertion for out of boundary access
    else {
      throw Error("Buffer too small?");
    }
    // increase counter
    this.size += size;
  };

  // return a continous Uint8Array with the full content
  // the typed array is guaranteed to have to correct length
  // also meaning that there is no space remaining to add more
  // you may should expect malloc errors if size gets a few 10MB
  // calling this repeatedly always returns the same array instance
  // NOTE: An alternative approach would be to use a Blob. A Blob
  // can be created out of an array of array chunks (our buffers).
  // Via a FileReader we can then convert it back to a continous
  // Uint8Array. But this would make this method async in nature!
  LZMA.oStream.prototype.toUint8Array = function toUint8Array() {
    // local variable access
    var size = this.size,
      buffers = this.buffers;

    // the simple case with only one buffer
    if (buffers.length == 1) {
      // make a copy if needed!
      return buffers[0];
    }
    // otherwise we need to concat them all now
    try {
      // allocate the continous memory chunk
      var continous = new Uint8Array(size);
      // process each buffer in the output queue
      for (var i = 0, offset = 0; i < buffers.length; i++) {
        continous.set(buffers[i], offset);
        offset += buffers[i].length;
      }
      // release memory chunks
      buffers[0] = continous;
      // only one chunk left
      buffers.length = 1;
      // return typed array
      return continous;
      // Asynchronous alternative:
      // var blob = new Blob(outStream.buffers);
      // var reader = new FileReader();
      // reader.onload = function() { ... };
      // reader.readAsArrayBuffer(blob);
    } catch (err) {
      // probably allocation error
      // this error is somewhat expected so you should take care of it
      console.error("Error allocating Uint8Array of size: ", size);
      console.error("Message given was: ", err.toString());
    }
    // malloc error
    return null;
  };

  // invoke fn on every Uint8Array in the stream
  // using this interface can avoid the need to
  // create a full continous buffer of the result
  LZMA.oStream.prototype.forEach = function forEach(fn) {
    for (var i = 0; i < this.buffers.length; i++) {
      fn.call(this, this.buffers[i]);
    }
  };

  // returns a typed array of codepoints; depending if
  // UTF8 decoder is loaded, we treat the byte sequence
  // either as an UTF8 sequence or fixed one byte encoding
  // the result can then be converted back to a JS string
  LZMA.oStream.prototype.toCodePoints = function toCodePoints() {
    // treat as one byte encoding (i.e. US-ASCII)
    if (!LZMA.UTF8) {
      this.toUint8Array();
    }
    // we could probably make this work with our chunked
    // buffers directly, but unsure how much we could gain
    return LZMA.UTF8.decode(this.toUint8Array());
  };

  // convert the buffer to a javascript string object
  /*
	LZMA.oStream.prototype.toString = function toString()
	{
		var buffers = this.buffers, string = '';
		// optionally get the UTF8 codepoints
		// possibly avoid creating a continous buffer
		if (LZMA.UTF8) buffers = [ this.toCodePoints() ];
		for (var n = 0, nL = buffers.length; n < nL; n++) {
			for (var i = 0, iL = buffers[n].length; i < iL; i++) {
				string += String.fromCharCode(buffers[n][i]);
			}
		}
		return string;
	}*/

  LZMA.oStream.prototype.toString = function toString() {
    var buffers = this.buffers,
      string = "";
    // optionally get the UTF8 codepoints
    // possibly avoid creating a continous buffer
    if (LZMA.UTF8) buffers = [this.toCodePoints()];

    for (var n = 0, nL = buffers.length; n < nL; n++) {
      let str = "";
      for (var i = 0, iL = buffers[n].length; i < iL; i++) {
        str += String.fromCharCode(buffers[n][i]);
        buffers[n][i] = null;
      }
      str.replace(/[\r\n]+/gi, "");
      string += str;
      str = null;
    }
    return string;
  };
})(LZMA);
