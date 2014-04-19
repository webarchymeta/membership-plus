//start: wait animation

var r1 = 400;
var r2 = 340;
var r3 = 220;
var slitWidth = 40;
var speed = 0.0004;
var attenuation = 1.7;

function rgbToHex(r, g, b) {
    return '#' + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
}

window.nextFrame = (function (callback) {
    return window.requestAnimationFrame ||
    window.webkitRequestAnimationFrame ||
    window.mozRequestAnimationFrame ||
    window.oRequestAnimationFrame ||
    window.msRequestAnimationFrame ||
    function (callback) {
        if (!stopWaiting) {
            window.setTimeout(callback, 1000 / 60);
        }
    };
})();

function startWaitAnimation(waitAreaId, fillColor) {
    var waiting = document.getElementById(waitAreaId).getContext('2d');
    function slit(d, p) {
        shade = Math.round(Math.pow(1 - (d + p) % 1, attenuation) * 255)
        th = Math.PI * 2 * (p);
        cos = Math.cos(th);
        sin = Math.sin(th);
        waiting.strokeStyle = rgbToHex(shade, shade, shade);
        waiting.beginPath();
        waiting.moveTo(r2 * cos, r2 * sin);
        waiting.lineTo(r3 * cos, r3 * sin);
        waiting.stroke();
        waiting.closePath();
    }
    function frame() {
        waiting.arc(0, 0, r1, 0, Math.PI * 2);
        waiting.fill();
        var time = new Date().getTime() * speed;
        for (var p = 1; p > 0; p -= 0.05) { slit(time, p); }
        nextFrame(function () { frame(); });
    }
    waiting.lineCap = 'round';
    waiting.lineWidth = slitWidth;
    waiting.fillStyle = fillColor;
    waiting.translate(r1, r1);
    frame();
}

//end: wait animation

function RespondMsg(data) {
    var self = this;
    self.msg = data;
}

function ChatMessage(data) {
    var self = this;
    self.selected = ko.observable(false);
    self.checked = ko.observable(false);
    self.id = data.id;
    self.from = data.from;
    self.fromId = data.fromId;
    self.to = data.to;
    self.toId = data.toId;
    self.replyToId = data.replyToId;
    self.date = getDateVal(data.date);
    self.jsonDate = data.date;
    self.self = data.self;
    self.text = data.text;
    self.score = ko.observable(data.score);
    self.CurrentMessage = ko.observable('');
    self.IsSendReady = ko.computed(function () {
        return self.CurrentMessage().length > 0;
    });
    self.Replies = ko.observableArray();
    if (typeof data.replies != 'undefined' && data.replies.length > 0) {
        for (var i = 0; i < data.replies.length; i++) {
            self.Replies.push(new ChatMessage(data.replies[i]));
        }
    }
    self.ToggleReplyArea = function () {
        if (self.checked()) {
            self.checked(false);
        } else {
            self.checked(true);
        }
    }
    self.SendSimpleMessage = function () {
        root.chatHub.server.sendSimpleMessage(root.id, self.id, { title: "", body: self.CurrentMessage() });
        self.CurrentMessage('');
        self.checked(false);
    }

    self.VoteUp = function () {
        root.chatHub.server.voteOnMessage(root.id, self.id, userId, 1);
    }

    self.VoteDown = function () {
        root.chatHub.server.voteOnMessage(root.id, self.id, userId, -1);
    }
}

ChatContext = function (uid, pid) {
    var self = this;
    self.chatHub = null;
    self.status = ko.observable('');
    self.peerState = ko.observable('');
    self.userId = uid;
    self.peerId = pid;
    self.isConnectied = ko.observable(false);
    self.respondMsgs = ko.observableArray();
    self.chatMessages = ko.observableArray();
    self.startPrivateChat = function (data, event) {

    }
}

function registerClientMethods(hub) {
    hub.client.peerConnectResponse = function (msg) {
        switch (msg.status) {
            case 'Connecting':
                break;
            case 'Connected':
                break;
            case 'Wait':
                break;
            case 'Busy':
                break;
            case 'Rejected':
                break;
        }
    }

    hub.client.messageReceived = function (user, msg) {
        try {
            var msgObj = JSON.parse(msg);
            //...
        }
        catch (e) {
            alert(e.message);
        }
    }

    hub.client.userDisConnected = function (peer) {
        alert('fired');
    }
}

var root;
var userId;
var peerId;
var stopWaiting = false;

$(function () {
    root = new ChatContext(userId, peerId);
    ko.applyBindings(root);
    root.chatHub = $.connection.privateChatHub;
    registerClientMethods(root.chatHub);
    $.connection.hub.start().done(function () {
        root.chatHub.server.userConnected(peerId).done(function (r) {
            if (r.status != 'DeadEnd') {
                root.status(r.status);
                if (r.status == 'Notifiable' || r.status == 'MessageSent') {
                    startWaitAnimation('WaitArea', '#353535');
                }
            } else {
                root.status('Failed');
            }
        });
    }).fail(function () {
        root.status('Failed');
    });
})