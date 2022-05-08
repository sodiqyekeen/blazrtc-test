'use strict';

let peerConnection;
let localStream;
let sendChannel;
window.createPeerOffer = async (caller) => {

    openMediaDevices()
        .then(async stream => {
            console.log('Got MediaStream:', stream);
            localStream = stream;
            document.getElementById("local-video").srcObject = localStream;
            const configuration = { 'iceServers': [{ 'urls': 'stun:stun.l.google.com:19302' }] }
            peerConnection = new RTCPeerConnection(configuration);

            addLocalStream();
            peerConnection.ontrack = gotRemoteStream;

            peerConnection.onicecandidate = e => {
                if (e.candidate == null)
                    return
                caller.invokeMethodAsync("addcandidate", e.candidate);
            }

            peerConnection.onconnectionstatechange = e => {
                console.info('Connection state changed', e);
            }

            peerConnection.createOffer().then(offer => {
                offerCreated(offer, caller);
            });
        })
        .catch(error => {
            console.error('Error accessing media devices.', error);
        });
};

function offerCreated(offer, dotnetHelper) {
    console.log('offer created ', offer);
    dotnetHelper.invokeMethodAsync("saveoffer", offer);
    peerConnection.setLocalDescription(offer);
}


window.joinCall = async (caller, offer, id) => {
    console.info('joinning call...', offer)

    openMediaDevices()
        .then(async stream => {
            console.log('Got MediaStream:', stream);
            localStream = stream;
            document.getElementById("local-video").srcObject = localStream;
            const configuration = { 'iceServers': [{ 'urls': 'stun:stun.l.google.com:19302' }] }
            peerConnection = new RTCPeerConnection(configuration);

            addLocalStream();
            peerConnection.ontrack = gotRemoteStream;

            peerConnection.onicecandidate = e => {
                if (e.candidate == null)
                    return
                caller.invokeMethodAsync("sendcandidate", e.candidate);
            }

            peerConnection.setRemoteDescription(offer);
            console.info('creating answer...')
            peerConnection.createAnswer().then(answer => {
                answerCreated(answer, caller, id);
            });
        })
        .catch(error => {
            console.error('Error accessing media devices.', error);
        });
}

function gotRemoteStream(e) {
    console.log('gotRemoteStream', e.track, e.streams[0]);
    const remoteVideo = document.getElementById("remote-video");
    remoteVideo.srcObject = e.streams[0];
}

function answerCreated(answer, dotnetHelper, id) {
    console.info('answer created ', answer);
    peerConnection.setLocalDescription(answer);
    dotnetHelper.invokeMethodAsync("sendanswer", id, answer);
}

function openMediaDevices() {
    return navigator.mediaDevices.getUserMedia({ video: { frameRate: 24, width: { min: 480, ideal: 720, max: 1280 }, aspectRatio: 1.33333 }, audio: true });
}

function addLocalStream() {
    localStream.getTracks().forEach(track => {
        peerConnection.addTrack(track, localStream);
    });
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

function handleSuccess(stream) {
    startButton.disabled = true;
    //const video = document.querySelector('video');
    // video.srcObject = stream;

    // demonstrates how to detect that the user has stopped
    // sharing the screen via the browser UI.
    stream.getVideoTracks()[0].addEventListener('ended', () => {
        errorMsg('The user has ended sharing the screen');
        startButton.disabled = false;
    });
}

function handleError(error) {
    console.error(`getDisplayMedia error: ${error.name}`, error);
}