//start: wait animation

var r1 = 100;
var r2 = 85;
var r3 = 55;
var slitWidth = 10;
var speed = 0.0004;
var attenuation = 1.2; //1.7;

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
        shade1 = Math.round(Math.pow(1 - (d + p) % 1, attenuation) * 255)
        shade2 = Math.round(Math.pow(1 - (d + p) % 1, attenuation) * 155)
        shade3 = Math.round(Math.pow(1 - (d + p) % 1, attenuation) * 055)
        th = Math.PI * 2 * (p);
        cos = Math.cos(th);
        sin = Math.sin(th);
        waiting.strokeStyle = rgbToHex(shade1, shade2, shade3);
        waiting.beginPath();
        waiting.moveTo(r2 * cos, r2 * sin);
        waiting.lineTo(r3 * cos, r3 * sin);
        waiting.stroke();
        waiting.closePath();
    }
    function frame() {
        waiting.arc(0, 0, r1, 0, Math.PI * 2);
        waiting.fillStyle = 'transparent';
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
    self.fromObj = ko.observable(null);
    self.to = data.to;
    self.toId = data.toId;
    self.replyToId = data.replyToId;
    //self.date = getDateVal(data.date);
    self.jsonDate = data.date;
    self.self = data.fromId == userId ? true : false;
    if (self.self) {
        self.fromObj = ko.observable(userObj);
    } else {
        if (root.status() == "Connected" && root.peerActive()) {
            self.fromObj(peerObj);
        } else {
            self.fromObj(null);
        }
    }
    self.text = unescape(data.text);
    self.score = ko.observable(data.score);
    self.RichEditor = ko.observable(false);
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

    self.ToggleEditor = function () {
        self.RichEditor(!self.RichEditor());
    }

    self.SendSimpleMessage = function () {
        var msg = escape(self.CurrentMessage());
        root.chatHub.server.sendSimpleMessage(root.peerId, self.id, { title: "", body: msg }, root.recordSession());
        self.CurrentMessage('');
        self.checked(false);
    }
}

ChatContext = function (uid, pid) {
    var self = this;
    self.chatHub = null;
    self.status = ko.observable('');
    self.peerState = ko.observable(0);
    self.peerActive = ko.observable(true);
    self.recordSession = ko.observable(true);
    self.userId = uid;
    self.peerId = pid;
    self.isConnectied = ko.observable(false);
    self.respondMsgs = ko.observableArray();
    self.Messages = ko.observableArray();
    self.CurrentMessage = ko.observable('');
    self.RichEditor = ko.observable(false);
    self.IsSendReady = ko.computed(function () {
        return self.CurrentMessage().length > 0;
    })

    self.ToggleEditor = function () {
        self.RichEditor(!self.RichEditor());
    }

    self.SyncRecordState = function () {
        self.recordSession(!self.recordSession());
        root.chatHub.server.syncRecordState(peerId, self.recordSession());
    }

    self.RefreshMessages = function () {
        $.ajax({
            url: appRoot + "PrivateChat/LoadMessages?toId=" + peerId,
            type: "GET",
            success: function (content) {
                self.Messages.removeAll();
                for (var i = 0; i < content.length; i++) {
                    self.Messages.push(new ChatMessage(JSON.parse(content[i])));
                }
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
            }
        });
    }

    self.SendSimpleMessage = function () {
        var msg = escape(self.CurrentMessage());
        self.chatHub.server.sendSimpleMessage(peerId, null, { title: "", body: msg }, self.recordSession());
        self.CurrentMessage('');
    }

}

function appendMessage(parents, msg) {
    for (var i = 0; i < parents().length; i++) {
        if (msg.replyToId == parents()[i].id) {
            parents()[i].Replies.push(new ChatMessage(msg));
            return true;
        }
    }
    for (var i = 0; i < parents().length; i++) {
        if (parents()[i].Replies().length > 0) {
            if (appendMessage(parents()[i].Replies, msg)) {
                return true;
            }
        }
    }
    return false;
}

function updateMsgSender(msgList, peer, join) {
    for (var i = 0; i < msgList().length; i++) {
        if (msgList()[i].fromId == peer.id) {
            if (join) {
                msgList()[i].fromObj(peer);
            } else {
                msgList()[i].fromObj(null);
            }
        }
        if (msgList()[i].Replies().length > 0) {
            updateMsgSender(msgList()[i].Replies, peer, join);
        }
    }
}

function registerClientMethods(hub) {

    hub.client.peerConnectResponse = function (msg) {
        switch (msg.status) {
            case 1:
                return;
            case 2:
                if (msg.msg.length > 0) {
                    root.respondMsgs.push(msg);
                }
                return;
            case 3:
                root.peerState(3);
                if (msg.msg.length > 0) {
                    root.respondMsgs.push(msg);
                }
                break;
            case 4:
                root.peerState(4);
                if (msg.msg.length > 0) {
                    root.respondMsgs.push(msg);
                }
                break;
        }
        root.status("Failed");
    }

    hub.client.onSyncRecordState = function (record) {
        root.recordSession(record);
    }

    hub.client.onConnectAck = function () {
        root.status("Connected");
        if (typeof parent != 'undefined' && parent != null && typeof parent.__open_chat_handlers != 'undefined') {
            parent.__open_chat_handlers[peerId] = openChatHandler;
        } else if (__open_chat_handlers != 'undefined') {
            __open_chat_handlers[peerId] = openChatHandler;
        }
        root.peerActive(true);
        updateMsgSender(root.Messages, peerObj, true);
    }

    hub.client.messageReceived = function (user, msg) {
        try {
            var msgObj = JSON.parse(msg);
            if (msgObj.fromId == userId)
                msgObj.self = true;
            if (msgObj.replyToId == null || msgObj.replyToId == '') {
                root.Messages.push(new ChatMessage(msgObj));
                var div = $("." + (popupMode ? "popup-" : "") + "message-list")[0];
                if (typeof div != 'undefined' && div != null) {
                    div.scrollTop = div.scrollHeight;
                }
            } else {
                appendMessage(root.Messages, msgObj);
                var div = $("#" + msgObj.replyToId)[0];
                if (typeof div != 'undefined' && div != null) {
                    div.scrollIntoView();
                }
            }
        }
        catch (e) {
            alert(e.message);
        }
    }

    hub.client.userDisConnected = function (peer) {
        alert('fired');
    }
}

var openChatHandler = function (peerId) {
    root.peerActive(false);
    updateMsgSender(root.Messages, peerObj, false);
}

var root;
var userId;
var userObj;
var peerId;
var peerObj;
var stopWaiting = false;
var popupMode = true;

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
                } else if (r.status == "Connected") {
                    if (typeof parent != 'undefined' && parent != null && typeof parent.__open_chat_handlers != 'undefined') {
                        parent.__open_chat_handlers[peerId] = openChatHandler;
                    } else if (__open_chat_handlers != 'undefined') {
                        __open_chat_handlers[peerId] = openChatHandler;
                    }
                    root.chatHub.server.connectAck(peerId);
                }
            } else {
                root.status('Failed');
                root.peerState(5);
            }
            if (typeof parent != 'undefined' && parent != null) {
                __systemNotifier = parent.__systemNotifier;
            }
            $(window).unload(function () {
                if (root.status() == 'Notifiable' || root.status() == 'MessageSent') {
                    if (typeof __systemNotifier != 'undefined' && __systemNotifier != null) {
                        __systemNotifier.userCancelInteraction(peerId);
                    }
                } else if (root.status() == "Connected") {
                    if (typeof __systemNotifier != 'undefined' && __systemNotifier != null) {
                        __systemNotifier.userCancelInteraction(peerId);
                    }
                    if (typeof parent != 'undefined' && parent != null && typeof parent.__open_chat_handlers != 'undefined') {
                        parent.__open_chat_handlers[peerId] = null;
                    } else if (__open_chat_handlers != 'undefined') {
                        __open_chat_handlers[peerId] = null;
                    }
                }
            });
        });
    }).fail(function () {
        root.status('Failed');
    });
})