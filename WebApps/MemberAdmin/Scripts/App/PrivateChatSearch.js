var modalInitialized = false;

function startPrivateChat(data, event) {
    var panel = $("#privateChatPopup");
    var userName = $("#userName");
    var content = $("#chatPage");
    userName.val('...');
    if (!modalInitialized) {
        panel.modal();
        panel.on("hidden.bs.modal", function () {
            //content.contents().find("body").trigger('unload');
            content[0].src = 'about:blank';// appRoot + 'PrivateChat/ChatPopup?toId=';
        });
        modalInitialized = true;
    }
    panel.modal('show');
    content[0].src = appRoot + 'PrivateChat/ChatPopup?toId=' + data.data.ID; 
}