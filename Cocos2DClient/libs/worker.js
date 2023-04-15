importScripts("lzma.js");
importScripts("lzma.shim.js");

onmessage = (e) => {
  try {
    let result = e.data;
    var input = new LZMA.iStream(result);
    var output = new LZMA.oStream();
    LZMA.decompressFile(input, output);
    console.time("parse");
    let res = JSON.parse(output.toString());
    // let res = fastJson.format(str);
    console.timeEnd("parse");
    postMessage(res);
  } catch (error) {
    postMessage("error  " + error);
  }
};

 