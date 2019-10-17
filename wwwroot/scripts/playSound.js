function playSound(fileData) {
    audioElem = new Audio();
    audioElem.src = fileData;
    audioElem.play();
}