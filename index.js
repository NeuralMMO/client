


const  worker = new Worker("worker.js");

worker.onmessage = (e)=>{
    console.log("worker.onmessage", );
}
worker.postMessage([0,0,1,2,3,4,5,6]);
