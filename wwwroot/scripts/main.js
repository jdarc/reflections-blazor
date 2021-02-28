let canvas = null;
let context = null;
let imageData = null;

function arrayBufferToBase64(buffer) {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    for (let i = 0; i < bytes.byteLength; ++i) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

function imageToData(image) {
    let canvas = document.createElement("canvas");
    canvas.width = image.width;
    canvas.height = image.height;
    let context = canvas.getContext("2d");
    context.drawImage(image, 0, 0);
    return arrayBufferToBase64(context.getImageData(0, 0, image.width, image.height).data.buffer);

}

function blit(dataPtr) {
    let data = new Uint32Array(Module.HEAPU8.buffer, dataPtr + 4, Blazor.platform.getArrayLength(dataPtr))
    new Uint32Array(imageData.data.buffer).set(data, 0);
    context.putImageData(imageData, 0, 0);
}


function gameLoop(timeStamp) {
    window.requestAnimationFrame(gameLoop);
    theInstance.invokeMethodAsync('GameLoop', timeStamp);
}

function resizeCanvas() {
    if (canvas != null) {
        const scaleX = (document.body.offsetWidth) / canvas.width;
        const scaleY = (document.body.offsetHeight) / canvas.height;
        const sca = Math.min(scaleX, scaleY);
        canvas.style.transform = `translateY(-50%) translateX(-50%) scale(${sca}, ${sca})`;
    }
}

window.addEventListener("resize", () => resizeCanvas());


window.initCanvas = c => {
    canvas = c;
    context = canvas.getContext("2d");
    imageData = context.createImageData(canvas.width, canvas.height);
    resizeCanvas();
}

window.initGame = (instance) => {
    window.theInstance = instance;
    window.requestAnimationFrame(gameLoop);
};
