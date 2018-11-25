var chimeAudio = undefined;
var cAudio = undefined;
var sfxAudio = undefined;
var currentStatus = undefined;
var inCar = false;

window.addEventListener('message',
    function (e) {
        if (e.data.transactionType === 'isBuckled') {
            if (inCar != e.data.inCar) {
                if (!e.data.inCar) {
                    // Kill all sounds, we got out!
                    clearInterval(chimeAudio);
                    chimeAudio = undefined;
                    if (cAudio !== undefined)
                        cAudio.pause();
                    if (sfxAudio !== undefined)
                        sfxAudio.pause();
                    currentStatus = false;
                    return;
                }
                inCar = e.data.inCar;
            }
            if (currentStatus != e.data.transactionValue) {
                if (e.data.transactionValue === true) {
                    playSound("sounds/buckle.ogg");
                    if (chimeAudio !== undefined) {
                        clearInterval(chimeAudio);
                        chimeAudio = undefined;
                    }
                    /* In case we want this later, hides seatbelt icon.

                    $('#container').stop(false, true);
                    $('#container')
                        .css('display', 'none')
                        .animate({
                            bottom: '-50%',
                            opacity: 0.0
                        }, 700, () => {});
                    */
                } else {
                    // We don't get in and unbuckle.. but the chime should still  happen.
                    if (currentStatus != undefined)
                        playSound("sounds/unbuckle.ogg");

                    /* In case we want this later, flashes seatbelt icon.

                    $('#container').stop(false, true);
                    $('#container')
                        .css('display', 'flex')
                        .animate({
                            bottom: '25%',
                            opacity: 1.0
                        }, 700, () => {});
                    */
                    if (chimeAudio !== undefined) {
                        clearInterval(chimeAudio);
                        chimeAudio = undefined;
                    }
                    chimeAudio = setInterval(function () { playChime(); }, 10000);
                }
                currentStatus = e.data.transactionValue;
            }
        }
    });
function playSound(file) {
    if (sfxAudio != undefined) sfxAudio.pause();

    sfxAudio = new Audio(file);
    sfxAudio.volume = 1.0;
    sfxAudio.play();
}
function playChime() {
    cAudio = new Audio("sounds/chime.ogg");
    cAudio.volume = 1.0;
    cAudio.play();
}