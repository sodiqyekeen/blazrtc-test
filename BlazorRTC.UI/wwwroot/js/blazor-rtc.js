'use strict';

let peerConnection;
let localStream;
let sendChannel;
let peerConnections = {};
window.createPeerOffer = async (caller, clientId) => {
    console.info('creating offer...', clientId);
    const configuration = { 'iceServers': [{ 'urls': 'stun:stun.l.google.com:19302' }] }
    const currentPeerConn = new RTCPeerConnection(configuration);
    peerConnections[clientId] = currentPeerConn;
    console.info('connection created', clientId, currentPeerConn);
    addLocalStream(currentPeerConn);
    currentPeerConn.ontrack = gotRemoteStream;

    currentPeerConn.onicecandidate = e => {
        if (e.candidate == null)
            return
        caller.invokeMethodAsync("addcandidate", e.candidate, clientId);
    }

    currentPeerConn.onconnectionstatechange = e => {
        console.info('Connection state changed', e);
    }
    console.info('creating offer...');
    currentPeerConn.createOffer().then(offer => {
        offerCreated(offer, caller, clientId, currentPeerConn);
    });
};



function offerCreated(offer, dotnetHelper, clientId, peerConn) {
    console.log('offer created ', offer);
    dotnetHelper.invokeMethodAsync("saveoffer", offer, clientId);
    peerConn.setLocalDescription(offer);
}


window.joinCall = async (caller, offer, clientId) => {
    console.info('joinning call...', offer)

    openMediaDevices()
        .then(async stream => {
            console.log('Got MediaStream:', stream);
            localStream = stream;
            document.getElementById("local-video").srcObject = localStream;
            const configuration = { 'iceServers': [{ 'urls': 'stun:stun.l.google.com:19302' }] }
            const currentConnection = new RTCPeerConnection(configuration);
            peerConnections[clientId] = currentConnection;
            addLocalStream(currentConnection);
            currentConnection.ontrack = gotRemoteStream;

            currentConnection.onicecandidate = e => {
                if (e.candidate == null)
                    return
                caller.invokeMethodAsync("sendcandidate", e.candidate, clientId);
            }

            currentConnection.setRemoteDescription(offer);
            console.info('creating answer...')
            currentConnection.createAnswer().then(answer => {
                answerCreated(answer, caller, clientId, currentConnection);
            });
        })
        .catch(error => {
            console.error('Error accessing media devices.', error);
        });
}

function gotLocalStream(stream) {
    console.log('Got local stream', stream);
    document.getElementById("local-video").srcObject = stream;
    localStream = stream;
}

function gotRemoteStream(e) {
    console.log('gotRemoteStream', e.track, e.streams[0]);
    const remoteVideo = document.getElementById("remote-video");
    //if (remoteVideo.srcObject) {
    //    console.info('already had remote stream.');
    //    return;
    //}
    remoteVideo.srcObject = e.streams[0];
}

function answerCreated(answer, dotnetHelper, clientId, peerConn) {
    console.info('answer created ', answer);
    peerConn.setLocalDescription(answer);
    dotnetHelper.invokeMethodAsync("sendanswer", clientId, answer);
}

function openMediaDevices() {
    return navigator.mediaDevices.getUserMedia({ video: { frameRate: 24, width: { min: 480, ideal: 720, max: 1280 }, aspectRatio: 1.33333 }, audio: true });
}

function addLocalStream(peerConn) {
    console.info('adding local stream...');
    localStream.getTracks().forEach(track => {
        peerConn.addTrack(track, localStream);
    });
}

function handleAnser(answer, clientId) {
    console.info('answer received', answer);
    peerConnections[clientId].setRemoteDescription(answer)
}

function handleCandidate(candidate, clientId) {
    console.info('candidate received', candidate);
    peerConnections[clientId].addIceCandidate(candidate)
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

//function handleSuccess(stream) {
//    startButton.disabled = true;
//    //const video = document.querySelector('video');
//    // video.srcObject = stream;

//    // demonstrates how to detect that the user has stopped
//    // sharing the screen via the browser UI.
//    stream.getVideoTracks()[0].addEventListener('ended', () => {
//        errorMsg('The user has ended sharing the screen');
//        startButton.disabled = false;
//    });
//}

//function handleError(error) {
//    console.error(`getDisplayMedia error: ${error.name}`, error);
//}

function startMeeting() {
    navigator.mediaDevices
        .getUserMedia({ video: { frameRate: 24, width: { min: 480, ideal: 720, max: 1280 }, aspectRatio: 1.33333 }, audio: true })
        .then(gotLocalStream)
        .catch(error => {
            console.error('Error accessing media devices.', error);
        });
}