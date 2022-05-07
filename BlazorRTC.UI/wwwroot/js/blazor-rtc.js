let peerConnection;
let localStream;
let sendChannel;
window.createPeerOffer = async (caller) => {
    const constraints = { video: { frameRate: 24, width: { min: 480, ideal: 720, max: 1280 }, aspectRatio: 1.33333 }, audio: true };

    navigator.mediaDevices.getUserMedia(constraints)
        .then(async stream => {
            console.log('Got MediaStream:', stream);
            localStream = stream;
            document.getElementById("local-video").srcObject = localStream;
            const configuration = { 'iceServers': [{ 'urls': 'stun:stun.l.google.com:19302' }] }
            peerConnection = new RTCPeerConnection(configuration);

            localStream.getTracks().forEach(track => {
                peerConnection.addTrack(track, localStream);
            });

            console.info('peer connection created...');
            const remoteVideo = document.getElementById("remote-video");

            peerConnection.addEventListener('track', async (event) => {
                const [remoteStream] = event.streams;
                remoteVideo.srcObject = remoteStream;
            });

            peerConnection.onicecandidate = e => {
                if (e.candidate == null)
                    return
                caller.invokeMethodAsync("addcandidate", e.candidate);
            }

            const offer = await peerConnection.createOffer();
            console.log('offer created ', offer);
            caller.invokeMethodAsync("saveoffer", offer);
            peerConnection.setLocalDescription(offer);
        })
        .catch(error => {
            console.error('Error accessing media devices.', error);
        });

};

window.joinCall = async (caller, offer, id) => {
    console.info('joinning call...', offer)
    const constraints = { video: { frameRate: 24, width: { min: 480, ideal: 720, max: 1280 }, aspectRatio: 1.33333 }, audio: true };

    navigator.mediaDevices.getUserMedia(constraints)
        .then(async stream => {
            console.log('Got MediaStream:', stream);
            localStream = stream;
            document.getElementById("local-video").srcObject = localStream;
            const configuration = { 'iceServers': [{ 'urls': 'stun:stun.l.google.com:19302' }] }
            peerConnection = new RTCPeerConnection(configuration);

            localStream.getTracks().forEach(track => {
                peerConnection.addTrack(track, localStream);
            });

            console.info('peer connection created...')

            const remoteVideo = document.getElementById("remote-video");

            peerConnection.addEventListener('track', async (event) => {
                const [remoteStream] = event.streams;
                remoteVideo.srcObject = remoteStream;
            });

            peerConnection.onicecandidate = e => {
                if (e.candidate == null)
                    return
                caller.invokeMethodAsync("sendcandidate", e.candidate);
            }

            peerConnection.setRemoteDescription(offer);
            console.info('creating answer...')
            const answer = await peerConnection.createAnswer();
            console.info('answer created ', answer);
            peerConnection.setLocalDescription(answer);
            caller.invokeMethodAsync("sendanswer", id, answer);

        })
        .catch(error => {
            console.error('Error accessing media devices.', error);
        });



}

async function openMediaDevices() {
    return await navigator.mediaDevices.getUserMedia({ video: { frameRate: 24, width: { min: 480, ideal: 720, max: 1280 }, aspectRatio: 1.33333 }, audio: true });
}

function handleAnser(answer) {
    console.info('answer received', answer);
    peerConnection.setRemoteDescription(answer)
}

function handleCandidate(candidate) {
    console.info('candidate received', candidate);
    peerConnection.addIceCandidate(candidate)
}

function sendMessage(message) {
    sendChannel.send(message);
}

function stopCamera(camera) {
    const video = document.getElementById(camera);
    if (video != null && video.srcObject != null) {
        const stream = video.srcObject;
        const tracks = stream.getTracks();

        tracks.forEach(track => track.stop());
        video.srcObject = null;
    }
}

async function hangup() {
    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }
    if (localStream) {
        localStream.getTracks().forEach(track => track.stop());
        localStream = null;
    }
};

function toggleVideo(status) {
    localStream.getVideoTracks()[0].enabled = status
}

function toggleMic(status) {
    localStream.getAudioTracks()[0].enabled = status
}