var appRoot;
var userId;

var ___dt = new Date();
var utcoff = ___dt.getTimezoneOffset() * 60000;

getDateVal = function (val) {
    if (val == null) {
        return null;
    }
    var tv = val.substr(6);
    tv = tv.substr(0, tv.length - 2);
    var ms = parseInt(tv);
    return new Date(ms);
};

toJsonDate = function (dt) {
    if (dt == null) {
        return null;
    }
    var str = "/Date(";
    str += (dt.getTime() + utcoff).toString();
    var off = utcoff > 0 ? dt.getTimezoneOffset() : -dt.getTimezoneOffset();
    var h = off / 60;
    var hstr = h > 9 ? h.toString() : '0' + h.toString();
    var m = off % 60;
    var mstr = m > 9 ? m.toString() : '0' + m.toString();
    if (utcoff > 0) {
        str += '-' + hstr + mstr;
    } else {
        str += '+' + hstr + mstr;
    }
    return str + ")/";
};

function ChatPeer(data) {
    var self = this;
    self.selected = ko.observable(false);
    self.checked = ko.observable(false);
    self.id = data.id;
    self.name = data.name;
    self.email = data.email;
    self.active = ko.observable(data.active);
    self.lastActive = ko.observable(getDateVal(data.lastActive));
    self.isSelf = ko.computed(function () {
        return self.id == userId;
    });
    self.chatUrl = ko.computed(function () {
        return appRoot + 'PrivateChat/Chat?toId=' + self.id;
    });
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
    self.SendSimpleMessageToAll = function () {
        root.chatHub.server.sendSimpleMessageToAll(root.id, self.id, { title: "", body: self.CurrentMessage() });
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

function ChatMsgLink(id, title) {
    var self = this;
    self.title = title;
    self.link = '#' + id;
}

function ChatRoom(id) {
    var self = this;
    self.chatHub = null;
    self.id = id;
    self.Started = ko.observable(false);
    self.Joined = ko.observable(false);
    self.IsSubscriber = ko.observable(false);
    self.IsNotifying = ko.observable(false);
    self.ConnectedCount = ko.observable(0);
    self.SubscriberCount = ko.observable(0);
    self.Members = ko.observableArray();
    self.Messages = ko.observableArray();
    self.MessageLinks = ko.observableArray();
    self.RoomInfo = ko.observable('');
    self.CurrentMessage = ko.observable('');
    self.IsSendReady = ko.computed(function () {
        return self.CurrentMessage().length > 0;
    });
    self.Subscribe = function () {
        alert('Subscription will be available in next version.')
    }
    self.Unsubscribe = function () {
        alert('Unsubscription will be available in next version.')
    }
    self.ToggleNotifications = function () {

    }
    self.ClearMsgLinks = function () {
        self.MessageLinks.removeAll();
    }
    self.SendSimpleMessageToAll = function () {
        if (!self.Started() || self.chatHub == null) {
            return;
        }
        self.chatHub.server.sendSimpleMessageToAll(self.id, null, { title: "test", body: self.CurrentMessage() });
        self.CurrentMessage('');
    }
}

function registerClientMethods(hub) {

    hub.client.userConnected = tryAddPeer;

    hub.client.userReconnected = tryAddPeer;

    function tryAddPeer(p) {
        try {
            var peer = JSON.parse(p);
            root.Members.remove(function (item) {
                return item.id == peer.id;
            });
            root.Members.push(new ChatPeer(peer));
        }
        catch (ex) {
            alert(e.message);
        }
    }

    hub.client.userDisConnected = function (p) {
        try {
            var peer = JSON.parse(p);
            root.Members.remove(function (item) {
                return item.id == peer.id;
            });
        }
        catch (ex) {
            alert(e.message);
        }
    }

    hub.client.messageReceived = function (user, msg) {
        try {
            var msgObj = JSON.parse(msg);
            msgObj.self = msgObj.fromId == userId;
            if (msgObj.replyToId == null || msgObj.replyToId == '' || !dialogMode) {
                root.Messages.push(new ChatMessage(msgObj));
                var div = $(".message-list")[0];
                div.scrollTop = div.scrollHeight;
            } else {
                appendMessage(root.Messages, msgObj);
                //var div = $("#" + msgObj.replyToId)[0];
                //div.scrollIntoView();
                var str = msgObj.text;
                if (str.length > 15) {
                    str = str.substr(0, 15) + '...';
                }
                root.MessageLinks.push(new ChatMsgLink(msgObj.id, '[' + msgObj.from + ']: ' + str));
            }
            beep(10, 2, function () {

            });
        }
        catch (e) {
            alert(e.message);
        }
    }

    hub.client.onUserVoteMessage = function (msgId, del) {
        updateVote(root.Messages, msgId, del);
    }

    hub.client.sendError = function (error) {
        alert(error);
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

    function updateVote(parents, id, score) {
        for (var i = 0; i < parents().length; i++) {
            if (parents()[i].id == id) {
                parents()[i].score(score);
                return true;
            }
        }
        for (var i = 0; i < parents().length; i++) {
            if (parents()[i].Replies().length > 0) {
                if (updateVote(parents()[i].Replies, id, score)) {
                    return true;
                }
            }
        }
        return false;
    }

    function beep (duration, type, finishedCallback) {

        if (!(window.audioContext || window.webkitAudioContext)) {
            //throw Error("Your browser does not support Audio Context.");
            return;
        }

        duration = +duration;

        // Only 0-4 are valid types.
        type = (type % 5) || 0;

        if (typeof finishedCallback != "function") {
            finishedCallback = function () { };
        }

        var ctx = new (window.audioContext || window.webkitAudioContext);
        var osc = ctx.createOscillator();

        osc.type = type;

        osc.connect(ctx.destination);
        osc.noteOn(0);

        setTimeout(function () {
            osc.noteOff(0);
            finishedCallback();
        }, duration);
    };
}

var root;
var roomId;
var dialogMode;

$(function () {
    root = new ChatRoom(roomId);
    addInitialMembers(root);
    addInitialMessages(root);
    ko.applyBindings(root);
    root.chatHub = $.connection.groupChatHub;
    registerClientMethods(root.chatHub);
    $.connection.hub.start().done(function () {
        root.chatHub.server.userConnected(roomId);
        root.Started(true);
        root.Joined(true);
    });
})